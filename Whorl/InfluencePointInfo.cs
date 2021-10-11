using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    /// <summary>
    /// Information for a point that influences parameters, based on its distance from a point for a pattern.
    /// </summary>
    public class InfluencePointInfo : IXml
    {
        public Pattern ParentPattern { get; private set; }
        public int Id { get; private set; }

        private DoublePoint _influencePoint;
        public DoublePoint InfluencePoint
        {
            get => _influencePoint;
            set
            {
                _influencePoint = AdjustInfluencePoint(value);
                OrigInfluencePoint = _influencePoint;
            }
        }

        public DoublePoint OrigInfluencePoint { get; private set; }

        public double InfluenceFactor { get; set; }

        private double _divFactor = 0.01;
        public double DivFactor
        {
            get => _divFactor;
            set => _divFactor = Math.Max(0.0, value);
        }

        private double _offset = 1.0;
        public double Offset
        {
            get => _offset;
            set => _offset = Math.Max(0.0001, value);
        }

        public double FunctionOffset { get; set; }

        public double Power { get; set; } = 1.0;

        private Func<double, double> TransformFunc { get; set; }

        private string _transformFunctionName;
        public string TransformFunctionName
        {
            get => _transformFunctionName;
            set
            {
                if (_transformFunctionName == value)
                    return;
                if (value == null || !staticMethodsDict.TryGetValue(value, out MethodInfo transformFn))
                {
                    throw new Exception($"Invalid TransformFunctionName: {value}");
                }
                _transformFunctionName = value;
                try
                {
                    TransformFunc = (Func<double, double>)Delegate.CreateDelegate(typeof(Func<double, double>), transformFn);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Invalid type of transform function: {value}.", ex);
                }
            }
        }

        public static IEnumerable<string> TransformFunctionNames => staticMethodsDict.Keys.OrderBy(s => s);

        private static Dictionary<string, MethodInfo> staticMethodsDict { get; } = new Dictionary<string, MethodInfo>();

        public bool Enabled { get; set; } = true;

        public bool Selected { get; set; }

        public EventHandler RemovedFromList;

        static InfluencePointInfo()
        {
            VarFunctionParameter.PopulateMethodsDict(staticMethodsDict, parameterCount: 1, ExpressionParser.StaticMethodsTypes);
        }

        public InfluencePointInfo()
        {
            TransformFunctionName = EvalMethods.IdentMethodName;
        }

        public InfluencePointInfo(InfluencePointInfo source, Pattern pattern)
        {
            CopyProperties(source);
            ParentPattern = pattern;
        }

        /// <summary>
        /// Set InfluencePoint without setting OrigInfluencePoint.
        /// </summary>
        /// <param name="doublePoint"></param>
        public void SetInfluencePoint(DoublePoint doublePoint, bool setOrigPoint)
        {
            if (setOrigPoint)
                InfluencePoint = doublePoint;
            else
                _influencePoint = doublePoint;
        }

        public void CopyProperties(InfluencePointInfo source)
        {
            Tools.CopyProperties(this, source, excludedPropertyNames: new string[] { nameof(ParentPattern), nameof(Id) });
            Id = source.Id;
        }

        public void CopyInfluenceLinks(InfluencePointInfo source)
        {
            if (ParentPattern == null)
                throw new NullReferenceException("ParentPattern cannot be null.");
            var influenceLinks = new List<InfluenceLink>();
            foreach (var transform in ParentPattern.Transforms.Where(t => t.Enabled))
            {
                AddInfluenceLinks(influenceLinks, transform.TransformSettings, source);
            }
            if (ParentPattern.HasPixelRendering)
            {
                AddInfluenceLinks(influenceLinks, ParentPattern.PixelRendering.FormulaSettings, source);
            }
            foreach (InfluenceLink influenceLink in influenceLinks)
            {
                var newLink = influenceLink.GetCopy(influenceLink.Parent);
                newLink.SetInfluencePointInfo(this);
                influenceLink.Parent.AddInfluenceLink(newLink);
            }
        }

        private void AddInfluenceLinks(List<InfluenceLink> influenceLinks, FormulaSettings formulaSettings, InfluencePointInfo source)
        {
            var parentCollection = formulaSettings?.InfluenceLinkParentCollection;
            if (parentCollection != null)
            {
                influenceLinks.AddRange(parentCollection.InfluenceLinkParentsByParameterName.Values
                                        .SelectMany(lp => lp.InfluenceLinks)
                                        .Where(il => il.InfluencePointInfo == source));
            }
        }

        public bool CopyRadially()
        {
            var frm = new GetValueForm("Number of New Copies:", ValidateCopies);
            if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return false;  //User cancelled.
            int copies = int.Parse(frm.ValueText);
            CopyRadially(copies);
            return true;
        }

        private string ValidateCopies(string valText)
        {
            valText = valText ?? string.Empty;
            if (int.TryParse(valText, out int copies))
            {
                if (copies > 0)
                    return null;
            }
            return "Please enter a positive integer for Number of New Copies.";
        }

        public void CopyRadially(int copies)
        {
            if (copies < 1)
                throw new Exception("copies must be a positive integer.");
            double rotation = 2.0 * Math.PI / (1.0 + copies);
            Complex zRotation = Complex.CreateFromModulusAndArgument(1.0, rotation);
            Complex zVector = new Complex(OrigInfluencePoint.X, OrigInfluencePoint.Y);
            for (int i = 0; i < copies; i++)
            {
                zVector *= zRotation;
                var copy = new InfluencePointInfo(this, ParentPattern);
                copy.InfluencePoint = new DoublePoint(zVector.Re, zVector.Im);
                copy.AddToPattern(ParentPattern);
                copy.CopyInfluenceLinks(this);
            }
        }

        public void AddToPattern(Pattern pattern)
        {
            if (pattern == null)
                throw new NullReferenceException("pattern cannot be null.");
            ParentPattern = pattern;
            ParentPattern.InfluencePointInfoList.AddInfluencePointInfo(this);
            Id = ParentPattern.InfluencePointInfoList.Count;
            while (ParentPattern.InfluencePointInfoList.InfluencePointInfos.Any(p => p != this && p.Id == Id))
            {
                Id++;
            }
            InfluencePoint = AdjustInfluencePoint(InfluencePoint);
        }

        public void OnRemoved()
        {
            RemovedFromList?.Invoke(this, EventArgs.Empty);
        }

        //public double ComputeValue(PolarPoint polarPoint) //, PointF center)
        //{
        //    return ComputeValue(GetDoublePoint(polarPoint));
        //}

        public double ComputeValue(DoublePoint patternPoint, bool forRendering)
        {
            if (!Enabled)
                return 0.0;
            DoublePoint point = forRendering ? InfluencePoint : OrigInfluencePoint;
            double xDiff = patternPoint.X - point.X;
            double yDiff = patternPoint.Y - point.Y;
            if (ParentPattern != null)
            {
                double scale = ParentPattern.InfluenceScaleFactor;
                if (forRendering)
                    scale *= ParentPattern.RenderingScaleFactor;
                xDiff *= scale;
                yDiff *= scale;
            }
            double divisor = Offset + DivFactor * (xDiff * xDiff + yDiff * yDiff);
            if (Power != 1.0)
                divisor = Math.Pow(divisor, Power);
            return InfluenceFactor * TransformFunc(1.0 / divisor + FunctionOffset);
        }

        //public static DoublePoint GetDoublePoint(PolarPoint polarPoint, PointF center)
        //{
        //    DoublePoint doublePoint = polarPoint.ToRectangular();
        //    doublePoint.X += center.X;
        //    doublePoint.Y += center.Y;
        //    return doublePoint;
        //}

        public void Draw(Graphics g, Bitmap designBitmap, Font font)
        {
            const float crossWidth = 1F;
            const float rectWidth = 2F;
            if (ParentPattern == null)
                throw new NullReferenceException("ParentPattern cannot be null.");
            PointF p = new PointF((float)OrigInfluencePoint.X + ParentPattern.Center.X, (float)OrigInfluencePoint.Y + ParentPattern.Center.Y);
            Color penColor = Color.Black;
            if (designBitmap != null)
            {
                int pX = (int)p.X, 
                    pY = (int)p.Y;
                if (pX >= 0 && pY >= 0 && pX < designBitmap.Width && pY < designBitmap.Height)
                {
                    bool isLight = Tools.ColorIsLight(designBitmap.GetPixel(pX, pY));
                    penColor = isLight ? Color.Black : Color.White; //Tools.InverseColor(designBitmap.GetPixel(pX, pY));
                }
            }
            using (var pen = new Pen(penColor))
            {
                string idText = Id.ToString();
                SizeF textSize = g.MeasureString(idText, font);
                var rectF = new RectangleF(new PointF(p.X - crossWidth, p.Y - crossWidth),
                                           new SizeF(rectWidth, rectWidth));
                //Draw Id:
                using (var brush = new SolidBrush(penColor))
                {
                    g.DrawString(idText, font, brush, new PointF(rectF.Left - textSize.Width, rectF.Top));
                }
                //Draw a cross at point's location:
                g.DrawLine(pen,
                           new PointF(rectF.Left, p.Y),
                           new PointF(rectF.Right, p.Y));
                g.DrawLine(pen,
                           new PointF(p.X, rectF.Top),
                           new PointF(p.X, rectF.Bottom));
                if (Selected)
                {
                    //Draw a circle around the cross:
                    g.DrawEllipse(pen, rectF);
                }
            }
        }

        /// <summary>
        /// Ensure location does not overlap another influence point.
        /// </summary>
        /// <param name="influencePoint"></param>
        /// <returns></returns>
        private DoublePoint AdjustInfluencePoint(DoublePoint influencePoint)
        {
            if (ParentPattern != null)
            {
                while (ParentPattern.InfluencePointInfoList.InfluencePointInfos.Any(ip => ip != this && ip.OrigInfluencePoint.IntegerEquals(influencePoint)))
                {
                    influencePoint.X += 1;
                }
            }
            return influencePoint;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(InfluencePointInfo);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(xmlNode, "InfluencePointX", OrigInfluencePoint.X);
            xmlTools.AppendXmlAttribute(xmlNode, "InfluencePointY", OrigInfluencePoint.Y);
            xmlTools.AppendXmlAttributesExcept(xmlNode, this, 
                     nameof(ParentPattern), nameof(InfluencePoint), nameof(OrigInfluencePoint), nameof(TransformFunc), nameof(Selected));
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributesExcept(this, node, new string[] { "InfluencePointX", "InfluencePointY" });
            double x = Tools.GetXmlAttribute<double>(node, "InfluencePointX");
            double y = Tools.GetXmlAttribute<double>(node, "InfluencePointY");
            OrigInfluencePoint = _influencePoint = new DoublePoint(x, y);
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }

    public class InfluencePointInfoList : IXml
    {
        public Pattern ParentPattern { get; }
        private List<InfluencePointInfo> influencePointInfoList { get; } =
            new List<InfluencePointInfo>();
        public IEnumerable<InfluencePointInfo> InfluencePointInfos => influencePointInfoList;
        public int Count => influencePointInfoList.Count;

        public InfluencePointInfoList(Pattern pattern)
        {
            if (pattern == null)
                throw new NullReferenceException("pattern cannot be null.");
            ParentPattern = pattern;
        }

        /// <summary>
        /// Create copy of source.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        public InfluencePointInfoList(InfluencePointInfoList source, Pattern pattern): this(pattern)
        {
            influencePointInfoList.AddRange(source.InfluencePointInfos.Select(ip => new InfluencePointInfo(ip, pattern)));
        }

        public void AddInfluencePointInfo(InfluencePointInfo influencePointInfo)
        {
            influencePointInfoList.Add(influencePointInfo);
        }

        public bool RemoveInfluencePointInfo(InfluencePointInfo influencePointInfo)
        {
            bool removed = influencePointInfoList.Remove(influencePointInfo);
            if (removed)
            {
                influencePointInfo.OnRemoved();  //Raises event.
            }
            return removed;
        }

        public void Clear()
        {
            influencePointInfoList.Clear();
        }

        public void TransformInfluencePoints(Complex zFactor)
        {
            foreach (InfluencePointInfo pointInfo in InfluencePointInfos)
            {
                Complex zP = zFactor * new Complex(pointInfo.OrigInfluencePoint.X, pointInfo.OrigInfluencePoint.Y);
                pointInfo.InfluencePoint = new DoublePoint(zP.Re, zP.Im);
            }
        }

        //public void TransformInfluencePoints(double scale, double rotation, bool setOrigPoints = false)
        //{
        //    var zFactor = Complex.CreateFromModulusAndArgument(scale, rotation);
        //    TransformInfluencePoints(zFactor, setOrigPoints);
        //}

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(InfluencePointInfoList);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (var info in influencePointInfoList)
            {
                info.ToXml(xmlNode, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name != nameof(InfluencePointInfo))
                    throw new Exception("Invalid XML.");
                var influencePointInfo = new InfluencePointInfo();
                influencePointInfo.FromXml(childNode);
                influencePointInfo.AddToPattern(ParentPattern);
            }
        }
    }
}
