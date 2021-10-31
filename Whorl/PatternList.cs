using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Drawing;
using ParserEngine;

namespace Whorl
{
    public class PatternList : BaseObject, ICloneable, IXml, IDisposable
    {
        public static string GetPatternListName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = null;
            else
                name = name.Trim();
            return name;
        }

        private string _patternListName;
        public string PatternListName
        {
            get { return _patternListName; }
            set
            {
                string name = GetPatternListName(value);
                if (_patternListName != name)
                {
                    _patternListName = name;
                    IsChanged = true;
                }
            }
        }

        public bool IsChanged { get; set; }
        public bool IsDeleted { get; set; }

        public List<Pattern> PatternsList { get; private set; } =
            new List<Pattern>();

        public Complex[] ZFactors { get; set; }
        public Complex PreviewZFactor { get; set; }

        public IEnumerable<Pattern> Patterns
        {
            get { return PatternsList; }
        }

        public WhorlDesign Design { get; }

        public PatternList(WhorlDesign design)
        {
            if (design == null)
                throw new NullReferenceException("design cannot be null.");
            Design = design;
        }

        public void AddPattern(Pattern pattern)
        {
            if (PatternsList.Exists(ptn => ptn is Ribbon) || 
                PatternsList.Count > 0 && pattern is Ribbon)
                throw new Exception("A ribbon must be the only pattern in a pattern group.");
            PatternsList.Add(pattern);
            IsChanged = true;
        }

        public void AddPatterns(IEnumerable<Pattern> PatternsList)
        {
            foreach (Pattern pattern in PatternsList)
                AddPattern(pattern);
        }

        public Ribbon GetRibbon()
        {
            return PatternsList.FirstOrDefault() as Ribbon;
        }

        public void SetZVectors(Complex zVector)
        {
            if (this.ZFactors == null)
                SetProperties();
            for (int i = 0; i < this.PatternsList.Count; i++)
            {
                this.PatternsList[i].ZVector = zVector * ZFactors[i];
            }
        }

        public void SetOutlineZVectors(Complex zVector)
        {
            if (this.ZFactors == null)
                SetProperties();
            for (int i = 0; i < this.PatternsList.Count; i++)
            {
                this.PatternsList[i].OutlineZVector = zVector * ZFactors[i];
            }
        }

        public void SetCenters(PointF center)
        {
            if (this.PatternsList.Count == 0)
                return;
            Pattern pattern1 = this.PatternsList[0];
            PointF DeltaCenter = new PointF(center.X - pattern1.Center.X, center.Y - pattern1.Center.Y);
            foreach (Pattern pattern in this.PatternsList)
            {
                pattern.Center = new PointF(pattern.Center.X + DeltaCenter.X, pattern.Center.Y + DeltaCenter.Y);
            }
        }

        public void SetProperties(bool normalizeAngles = false)
        {
            if (this.PatternsList.Count == 0)
                return;
            if (this.PatternsList.Count == 1)
            {
                Pattern pattern = PatternsList[0];
                if (pattern.GetSingleBasicOutline(BasicOutlineTypes.Rectangle) != null)
                    PreviewZFactor = Complex.One;
                else
                    PreviewZFactor = pattern.PreviewZFactor;
                this.ZFactors = new Complex[] { Complex.One };
                return;
            }
            Pattern maxPattern = this.Patterns.OrderBy(
                ptn => ptn.ZVector.GetModulusSquared()).Last();
            //this.PreviewFactor = maxPattern.PreviewFactor;
            this.PreviewZFactor = maxPattern.PreviewZFactor;
            ZFactors = new Complex[this.Patterns.Count()];
            //double modulus1 = maxPattern.ZVector.GetModulus();
            //double angle1 = maxPattern.ZVector.GetArgument();
            Complex maxVector = maxPattern.ZVector;
            if (maxVector == Complex.Zero)
                maxVector = new Complex(1, 0);
            for (int i = 0; i < ZFactors.Length; i++)
            {
                if (normalizeAngles)
                    ZFactors[i] = PatternsList[i].ZVector / maxVector;
                else
                    ZFactors[i] = PatternsList[i].ZVector / maxVector.GetModulus();
                //double modulus = this.PatternsList[i].ZVector.GetModulus();
                //double angle = this.PatternsList[i].ZVector.GetArgument();
                //ZFactors[i] = Complex.CreateFromModulusAndArgument(
                //    modulus / modulus1, angle - angle1);
            }
        }

        public void DrawFilled(Graphics g, IRenderCaller caller, Size imgSize, bool computeRandom = true, 
                               bool checkTilePattern = true, bool enableCache = true, bool draftMode = false)
        {
            DrawDesign.DrawPatterns(g, caller, this.Patterns, imgSize, computeRandom, checkTilePattern: checkTilePattern,
                                    enableCache: enableCache, draftMode: draftMode);
        }

        public bool HasSurroundColors()
        {
            return this.PatternsList.Exists(ptn => ptn.HasSurroundColors());
        }

        public bool HasPixelRendering()
        {
            return this.PatternsList.Exists(ptn => ptn.PixelRendering != null && ptn.PixelRendering.Enabled);
        }

        public Bitmap ThumbnailImage { get; private set; }

        public void ClearThumbnailImage()
        {
            if (ThumbnailImage != null)
                ThumbnailImage.Dispose();
            ThumbnailImage = null;
        }

        //public void CheckCreateThumbnailImage()
        //{
        //    if (ThumbnailImage == null && (HasSurroundColors() || HasPixelRendering()))
        //    {
        //        int width = SelectPatternForm.ThumbnailImageWidth;
        //        ThumbnailImage = new Bitmap(width, width);
        //        using (Graphics g = Graphics.FromImage(ThumbnailImage))
        //        {
        //            DrawFilled(g, null, ThumbnailImage.Size, computeRandom: false, enableCache: false);
        //        }
        //    }
        //}

        public async Task CheckCreateThumbnailImageAsync()
        {
            if (ThumbnailImage != null)
                return;
            bool createImage = HasSurroundColors() || HasPixelRendering();
            if (createImage)
            {
                int width = SelectPatternForm.ThumbnailImageWidth;
                ThumbnailImage = new Bitmap(width, width);
                using (Graphics g = Graphics.FromImage(ThumbnailImage))
                {
                    await Task.Run(() => DrawForThumbnail(g, ThumbnailImage.Size));
                }
            }
        }

        private void DrawForThumbnail(Graphics g, Size imgSize)
        {
            try
            {
                DrawFilled(g, null, imgSize, computeRandom: false, enableCache: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void DrawOutlines(Graphics g, Color color)
        {
            foreach (Pattern pattern in this.PatternsList)
            {
                pattern.DrawOutline(g, color);
            }
        }

        public static void SetRibbonForPreview(Ribbon ribbon, Size picSize)
        {
            if (ribbon.RibbonDistance <= 0)
                ribbon.RibbonDistance = 1;
            else if (ribbon.RibbonDistance > picSize.Width)
            {
                //Scale ribbon to fit within picture box:
                ribbon.PatternZVector *= (double)picSize.Width / ribbon.RibbonDistance;
                ribbon.RibbonDistance = picSize.Width;
            }
            ribbon.Center = new PointF(0, picSize.Height / 2);
            ribbon.ZVector = new Complex(ribbon.RibbonDistance, 0);
            ribbon.RibbonPath.Clear();
            PointF center = ribbon.Center;
            while (true)
            {
                //Draw as many repitions of ribbon as will fit in picture box:
                center.X += (float)ribbon.RibbonDistance;
                if (center.X > picSize.Width)
                    break;
                ribbon.AddToRibbonPath(center);
            }
        }

        public static double GetPreviewFactor(Pattern pattern)
        {
            double factor = 1;
            if (pattern.Recursion.IsRecursive && pattern.Recursion.DrawAsPatterns)
            {
                for (int i = 1; i <= pattern.Recursion.Depth; i++)
                {
                    factor += Math.Pow(pattern.Recursion.Scale, i);
                }
            }
            return factor;
        }

        public void SetForPreview(Size picSize)
        {
            if (PatternsList.Count == 0)
                return;
            if (this.ZFactors == null)
                SetProperties();
            Ribbon ribbon = GetRibbon();
            if (ribbon != null)
            {
                SetRibbonForPreview(ribbon,  picSize);
            }
            else
            {
                this.SetCenters(new PointF(picSize.Width / 2, picSize.Height / 2));
                double radius = 0.45 * (double)picSize.Width;
                if (PatternsList.Count == 1)
                {
                    Pattern ptn = PatternsList[0];
                    radius /= GetPreviewFactor(ptn);
                }
                this.SetZVectors(this.PreviewZFactor * radius);
                foreach (Pattern pattern in this.PatternsList)
                {
                    pattern.SetForPreview(radius);
                }
            }
            foreach (Pattern pattern in this.PatternsList)
            {
                pattern.ZoomFactor = 1D;
            }
        }

        public Color GetPreviewBackgroundColor()
        {
            Pattern firstPattern = this.Patterns.FirstOrDefault();
            return firstPattern == null ? Color.White : firstPattern.GetPreviewBackColor();
        }

        public PatternList GetCopy(bool copyKeyGuid = true, bool keepRecursiveParents = false)
        {
            PatternList copy = new PatternList(Design);
            copy.IsChanged = this.IsChanged;
            copy.PatternListName = this.PatternListName;
            copy.AddPatterns(this.PatternsList.Select(
                             ptn => ptn.GetCopy(keepRecursiveParents)));
            copy.PreviewZFactor = this.PreviewZFactor;
            copy.ZFactors = this.ZFactors;
            if (!copyKeyGuid)
            {
                foreach (Pattern pattern in PatternsList)
                {
                    pattern.SetKeyGuid(Guid.NewGuid());
                }
            }
            return copy;
        }

        public object Clone()
        {
            return GetCopy();
        }

        public void ClearSelected()
        {
            foreach (Pattern pattern in this.PatternsList)
            {
                pattern.Selected = false;
            }
        }

        //public static string PatternListToXml(List<Pattern> patterns)
        //{
        //    //Pattern.SetPatternIndices(patterns);
        //    return string.Join(Environment.NewLine, patterns.Select(ptn => ptn.ToXml()));
        //}

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "PatternList";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            if (!string.IsNullOrWhiteSpace(PatternListName))
            {
                xmlTools.AppendXmlAttribute(node, nameof(PatternListName), PatternListName);
            }
            foreach (Pattern ptn in PatternsList)
            {
                ptn.ToXml(node, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributesExcept(this, node);
            foreach (XmlNode childNode in node.ChildNodes)
            {
                Pattern pattern = Pattern.CreatePatternFromXml(Design, childNode);
                if (pattern == null)
                    throw new Exception("Invalid XML found for pattern list.");
                AddPattern(pattern);
                this.SetProperties();
            }
            IsChanged = false;
        }

        public bool IsEquivalent(PatternList ptnList)
        {
            if (this.PatternsList.Count != ptnList.PatternsList.Count)
                return false;
            for (int i = 0; i < this.PatternsList.Count; i++)
            {
                Pattern ptn1 = this.PatternsList[i];
                Pattern ptn2 = ptnList.PatternsList[i];
                if (!ptn1.IsEquivalent(ptn2))
                    return false;
            }
            return true;
        }

        public bool Disposed { get; set; }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            foreach (Pattern pattern in this.PatternsList)
                pattern.Dispose();
        }
    }
}
