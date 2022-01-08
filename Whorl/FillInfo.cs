using ParserEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public enum TextureImageModes
    {
        Stretch,
        Tile,
        StretchTile,
        StretchFit
    }
    public abstract class FillInfo: BaseObject, IXml, IDisposable
    {
        public enum FillTypes
        {
            Path,
            Texture,
            Background
        }

        public enum PathColorModes
        {
            Normal,
            Radial,
            Surround
        }

        public abstract FillTypes FillType { get; }

        public Brush FillBrush { get; set; }

        public Pattern ParentPattern { get; }

        public FillInfo(Pattern parentPattern)
        {
            if (parentPattern == null)
                throw new ArgumentException("parentPattern for new FillInfo is null.",
                                            nameof(parentPattern));
            ParentPattern = parentPattern;
        }

        public abstract void CreateFillBrush();

        public abstract XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null);

        public abstract void FromXml(XmlNode node);

        public void SetFillBrushToNull()
        {
            if (FillBrush != null)
                FillBrush.Dispose();
            FillBrush = null;
        }

        public virtual ColorNodeList GetColorNodes()
        {
            ColorNodeList colorNodeList = new ColorNodeList();
            colorNodeList.AddDefaultNodes();
            return colorNodeList;
        }

        public virtual void ApplyTransforms(float scale = 1F)
        { }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            if (FillBrush != null)
            {
                FillBrush.Dispose();
                FillBrush = null;
            }
            IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }

        public abstract FillInfo GetCopy(Pattern parentPattern);
    }

    public class PathFillInfo: FillInfo, IXml, IColorNodeList
    {
        public class ColorPositions: BaseObject, IXml
        {
            private List<Color> colors { get; } = new List<Color>();
            private List<Color> waveColors { get; set; } 
            private List<float> positions { get; } = new List<float>();
            private List<float> wavePositions { get; set; }
            private bool _translucentWaves;
            private int _translucentCycles = 10;
            private float _translucentStrength = 1;
            private bool _randomWaves = true;

            public IEnumerable<Color> Colors
            {
                get { return colors; }
            }
            public IEnumerable<float> Positions
            {
                get { return positions; }
            }
            public IEnumerable<Color> RadialWaveColors
            {
                get { return waveColors; }
            }
            public IEnumerable<float> RadialWavePositions
            {
                get { return wavePositions; }
            }
            public bool TranslucentWaves
            {
                get { return _translucentWaves; }
                set
                {
                    if (_translucentWaves != value)
                    {
                        _translucentWaves = value;
                        ClearWaveProperties();
                    }
                }
            }
            public int TranslucentCycles
            {
                get { return _translucentCycles; }
                set
                {
                    int val = Math.Max(1, value);
                    if (_translucentCycles != val)
                    {
                        _translucentCycles = val;
                        ClearWaveProperties();
                    }
                }
            }
            public float TranslucentStrength
            {
                get { return _translucentStrength; }
                set
                {
                    float val = Math.Max(0F, Math.Min(1F, (float)Math.Round(value, 3)));
                    if (_translucentStrength != val)
                    {
                        _translucentStrength = val;
                        ClearWaveProperties();
                    }
                }
            }
            public bool RandomWaves
            {
                get { return _randomWaves; }
                set
                {
                    if (_randomWaves != value)
                    {
                        _randomWaves = value;
                        ClearWaveProperties();
                    }
                }
            }

            public Color[] SurroundColors { get; private set; }

            public RandomGenerator WaveRandomGenerator { get; } = new RandomGenerator();

            public IEnumerable<Color> GetColors()
            {
                if (TranslucentWaves)
                {
                    if (waveColors == null)
                        InitRadialWaves();
                    return waveColors;
                }
                else
                    return colors;
            }

            public IEnumerable<float> GetPositions()
            {
                if (TranslucentWaves)
                {
                    if (wavePositions == null)
                        InitRadialWaves();
                    return wavePositions;
                }
                else
                    return positions;
            }

            private void ClearWaveProperties()
            {
                SurroundColors = null;
                wavePositions = null;
                waveColors = null;
            }

            public void InitRadialWaves()
            {
                if (wavePositions != null && waveColors != null)
                    return;
                wavePositions = new List<float>();
                waveColors = new List<Color>();
                if (positions.Count == 0)
                    return;
                WaveRandomGenerator.ResetRandom();
                Random random = WaveRandomGenerator.Random;
                float[] newPositions = new float[TranslucentCycles];
                for (int i = 0; i < newPositions.Length; i++)
                {
                    if (RandomWaves)
                    {
                        newPositions[i] = Math.Max(0.01F, (float)random.NextDouble());
                        if (i > 0)
                            newPositions[i] += newPositions[i - 1];
                    }
                    else
                        newPositions[i] = (float)(i + 1);
                }
                float maxVal = newPositions[newPositions.Length - 1];
                for (int i = 0; i < newPositions.Length; i++)
                    newPositions[i] /= maxVal;
                int posI = 0;
                //int newAlpha = (int)((1F - TranslucentStrength) * 255F);
                bool setTranslucent = true;
                Color prevColor = Color.Empty;
                float prevPos = 0;
                for (int i = 0; i < positions.Count; i++)
                {
                    float pos = positions[i];
                    Color clr = colors[i];
                    int newAlpha = (int)((1F - TranslucentStrength) * clr.A);
                    while (posI < newPositions.Length && newPositions[posI] <= pos)
                    {
                        if (newPositions[posI] < pos)
                        {
                            wavePositions.Add(newPositions[posI]);
                            Color color;
                            if (i == 0 || pos == prevPos)
                                color = prevColor;
                            else
                            {
                                float factor = (newPositions[posI] - prevPos)
                                             / (pos - prevPos);
                                color = Tools.InterpolateColor(prevColor, clr, factor);
                                if (setTranslucent)
                                    color = Color.FromArgb(newAlpha, color);
                            }
                            waveColors.Add(color);
                            setTranslucent = !setTranslucent;
                        }
                        posI++;
                    }
                    wavePositions.Add(pos);
                    waveColors.Add(clr);
                    prevPos = pos;
                    prevColor = clr;
                }
            }

            //Find index of nearest element in Positions to position.
            //Positions is sorted ascending.
            public int FindNearestIndex(ref float position)
            {
                if (positions.Count == 0)
                    return -1;
                float pos = position;
                var sortedPos = Enumerable.Range(0, positions.Count)
                                .Select(i => new Tuple<int, float>(i, positions[i]))
                                .OrderBy(tpl => Math.Abs(pos - tpl.Item2));
                int index = sortedPos.First().Item1;
                position = positions[index];
                return index;
                //int index = positions.FindIndex(x => pos <= x);
                //if (index == -1)
                //    index = positions.Count - 1;
                //else if (index > 0)
                //{
                //    if (positions[index] - position > position - positions[index - 1])
                //        index--;
                //}
                //if (index >= 0)
                //    position = positions[index];
                //return index;
            }

            public Color GetColorAtIndex(int index)
            {
                if (index < 0 && index >= colors.Count)
                    throw new ArgumentException("index out of range for GetColorAtIndex.",
                                                nameof(index));
                return colors[index];
            }

            public void SetColorAtIndex(Color color, int index)
            {
                if (index < 0 && index >= colors.Count)
                    throw new ArgumentException("index out of range for SetColorAtIndex.",
                                                nameof(index));
                colors[index] = color;
                ClearWaveProperties();
            }

            public int AddOrSetColorAtPosition(Color color, float position)
            {
                if (position < 0 || position > 1)
                    throw new Exception(
                        $"position out of range for {nameof(AddOrSetColorAtPosition)}.");
                ClearWaveProperties();
                int addIndex = positions.FindIndex(x => position <= x);
                if (addIndex == -1)
                    addIndex = positions.Count;
                else if (positions[addIndex] == position)
                {
                    colors[addIndex] = color;
                    return addIndex;
                }
                positions.Insert(addIndex, position);
                colors.Insert(addIndex, color);
                return addIndex;
            }

            internal bool DeleteColorAtPosition(float position, float tolerance = 0.05F)
            {
                float indexPos = position;
                int index = FindNearestIndex(ref indexPos);
                if (index <= 0 || index == positions.Count - 1)
                    return false;
                if (Math.Abs(position - indexPos) > tolerance)
                    return false;
                positions.RemoveAt(index);
                colors.RemoveAt(index);
                ClearWaveProperties();
                return true;
            }

            private void SetSurroundColor(Color color, int index)
            {
                if (translucentFactors != null)
                {
                    float factor = translucentFactors[index % translucentFactors.Length];
                    color = Color.FromArgb((int)(factor * (float)color.A), color);
                }
                SurroundColors[index] = color;
            }

            private float[] translucentFactors;

            public Color[] ComputeSurroundColors(PathFillInfo pathFillInfo)
            {
                int pointCount = pathFillInfo.GraphicsPath.PointCount;
                if (SurroundColors != null && SurroundColors.Length == pointCount)
                    return SurroundColors;
                translucentFactors = null;
                bool patternIsSection = pathFillInfo.ParentPattern.UsesSection;
                if (TranslucentWaves)
                {
                    int factorCount = pointCount / TranslucentCycles;
                    if (patternIsSection)
                        factorCount /= 2;
                    if (factorCount > 1)
                    {
                        translucentFactors = new float[factorCount];
                        double angleDelta = 2 * Math.PI / (factorCount - 1);
                        double angle = 0;
                        for (int i = 0; i < translucentFactors.Length; i++)
                        {
                            translucentFactors[i] = (float)
                                (1D - (TranslucentStrength * 0.5 * (1D + Math.Sin(angle))));
                            angle += angleDelta;
                        }
                    }
                }
                List<float> newPositions;
                List<Color> newColors = new List<Color>(colors);
                if (patternIsSection)
                {
                    newPositions = positions.Select(pos => 0.5F * pos).ToList();
                    for (int i = positions.Count - 2; i >= 0; i--)
                    {
                        newPositions.Add(1F - positions[i]);
                        newColors.Add(colors[i]);
                    }
                }
                else
                    newPositions = new List<float>(positions);
                SurroundColors = new Color[pointCount];
                ColorGradient clrGrd = new ColorGradient();
                int lastInd = 0;
                for (int i = 0; i < newPositions.Count; i++)
                {
                    int ind = (int)Math.Round(newPositions[i] * (pointCount - 1));
                    if (ind < 0 || ind >= pointCount)
                        continue;
                    if (i > 0)
                    {
                        if (ind > lastInd + 1)
                        {
                            clrGrd.Initialize(steps: ind - lastInd - 1, color1: newColors[i -1], color2: newColors[i]);
                            while (lastInd < ind)
                                SetSurroundColor(clrGrd.GetCurrentColor(), lastInd++);
                        }
                        else
                        {
                            while (lastInd < ind)
                                SetSurroundColor(newColors[i], lastInd++);
                        }
                    }
                    SetSurroundColor(newColors[i], ind);
                    lastInd = ind;
                }
                if (newColors.Count != 0)
                {
                    Color lastColor = newColors[newColors.Count - 1];
                    while (lastInd < SurroundColors.Length)
                        SetSurroundColor(lastColor, lastInd++);
                }
                return SurroundColors;
            }

            public void ClearLists()
            {
                this.colors.Clear();
                this.positions.Clear();
            }

            public ColorNodeList GetColorNodes()
            {
                var colorNodes = Enumerable.Range(0, colors.Count).Select(i => new ColorNode(colors[i], positions[i]));
                return new ColorNodeList(colorNodes);
            }

            public void SetFromColorNodes(ColorNodeList colorNodes)
            {
                if (colorNodes.Count >= 2)
                {
                    ClearLists();
                    var nodes = colorNodes.GetColorBlendNodes();
                    colors.AddRange(nodes.Select(cn => cn.Color));
                    positions.AddRange(nodes.Select(cn => cn.Position));
                    if (TranslucentWaves)
                    {
                        ClearWaveProperties();
                        InitRadialWaves();
                    }
                }
            }

            public void CopyProperties(ColorPositions source)
            {
                ClearLists();
                this.TranslucentWaves = source.TranslucentWaves;
                this.TranslucentCycles = source.TranslucentCycles;
                this.TranslucentStrength = source.TranslucentStrength;
                this.RandomWaves = source.RandomWaves;
                this.colors.AddRange(source.colors);
                this.positions.AddRange(source.positions);
                this.WaveRandomGenerator.ReseedRandom(source.WaveRandomGenerator.RandomSeed);
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = "ColorPositions";
                XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributes(node, this, nameof(TranslucentWaves), nameof(TranslucentCycles), 
                                             nameof(TranslucentStrength), nameof(RandomWaves));
                xmlTools.AppendXmlAttribute(node, "RandomWavesSeed", WaveRandomGenerator.RandomSeed);
                string sList = String.Join(",", colors.Select(clr => clr.ToArgb()));
                xmlTools.AppendXmlAttribute(node, "Colors", sList);
                sList = String.Join(",", positions);
                xmlTools.AppendXmlAttribute(node, "Positions", sList);
                return xmlTools.AppendToParent(parentNode, node);
            }

            public void FromXml(XmlNode node)
            {
                ClearLists();
                Tools.GetXmlAttributesExcept(this, node, excludedPropertyNames:
                    new string[] { "Colors", "Positions", "RandomWavesSeed" });
                string sList = (string)Tools.GetXmlAttribute("Colors", typeof(string), node);
                colors.AddRange(sList.Split(',').Select(ic => Color.FromArgb(int.Parse(ic))));
                sList = (string)Tools.GetXmlAttribute("Positions", typeof(string), node);
                positions.AddRange(sList.Split(',').Select(x => float.Parse(x)));
                int? randomSeed = (int?)Tools.GetXmlAttribute("RandomWavesSeed", typeof(int), node, required: false);
                if (randomSeed != null)
                    WaveRandomGenerator.ReseedRandom(randomSeed);
            }

        }

        private Color centerColor;
        private Color boundaryColor;

        public override FillTypes FillType
        {
            get { return FillTypes.Path; }
        }

        public GraphicsPath GraphicsPath { get; set; }

        private PathColorModes _colorMode = PathColorModes.Normal;
        public PathColorModes ColorMode
        {
            get { return _colorMode; }
            set
            {
                if (_colorMode == value)
                    return;
                _colorMode = value;
                if (_colorMode == PathColorModes.Normal)
                    ColorInfo = null;
                else
                {
                    if (ColorInfo == null)
                        ColorInfo = new ColorPositions();
                    else
                        ColorInfo.ClearLists();
                    if (_colorMode == PathColorModes.Radial)
                    {
                        ColorInfo.AddOrSetColorAtPosition(CenterColor, 1);
                        ColorInfo.AddOrSetColorAtPosition(BoundaryColor, 0);
                    }
                    else if (_colorMode == PathColorModes.Surround)
                    {
                        ColorInfo.AddOrSetColorAtPosition(BoundaryColor, 0);
                        ColorInfo.AddOrSetColorAtPosition(BoundaryColor, 1);
                    }
                    SetFillBrushToNull();
                }
            }
        }

        public ColorNodeList ColorNodes
        {
            get { return GetColorNodes(); }
            set
            {
                SetFromColorNodes(value);
            }
        }

        public override ColorNodeList GetColorNodes()
        {
            ColorNodeList colorNodeList;
            if (ColorInfo != null)
                colorNodeList = ColorInfo.GetColorNodes();
            else
            {
                colorNodeList = new ColorNodeList();
                colorNodeList.AddNode(new ColorNode(BoundaryColor, 0));
                colorNodeList.AddNode(new ColorNode(CenterColor, 1));
            }
            return colorNodeList;
        }

        public void SetFromColorNodes(ColorNodeList colorNodes)
        {
            if (ColorMode != PathColorModes.Normal && ColorInfo != null)
            {
                ColorInfo.SetFromColorNodes(colorNodes);
            }
            else if (colorNodes.ColorNodes.Any())
            {
                BoundaryColor = colorNodes.ColorNodes.First().Color;
                CenterColor = colorNodes.ColorNodes.Last().Color;
            }
        }

        public PathFillInfo.ColorPositions ColorInfo { get; private set; }

        public Color CenterColor
        {
            get { return centerColor; }
            set
            {
                if (centerColor == value)
                    return;
                centerColor = value;
                if (ColorMode == PathColorModes.Radial)
                {
                    ColorInfo.AddOrSetColorAtPosition(centerColor, 1);
                    SetFillBrushToNull();
                }
                else
                {
                    var pthGrdBr = this.FillBrush as PathGradientBrush;
                    if (pthGrdBr != null)
                        pthGrdBr.CenterColor = centerColor;
                }
            }
        }

        public Color BoundaryColor
        {
            get { return boundaryColor; }
            set
            {
                if (boundaryColor == value)
                    return;
                boundaryColor = value;
                if (ColorMode != PathColorModes.Normal)
                {
                    ColorInfo.AddOrSetColorAtPosition(boundaryColor, 0);
                    if (ColorMode == PathColorModes.Surround)
                        ColorInfo.AddOrSetColorAtPosition(boundaryColor, 1);
                    SetFillBrushToNull();
                }
                else
                {
                    var pthGrdBr = this.FillBrush as PathGradientBrush;
                    if (pthGrdBr != null)
                        pthGrdBr.SurroundColors = new Color[] { boundaryColor };
                }
            }
        }

        public PathFillInfo(Pattern parentPattern): base(parentPattern)
        { }

        public int GetColorCount()
        {
            if (ColorMode == PathColorModes.Normal || ColorInfo == null)
                return 2;  //Boundary and Center color.
            else
                return ColorInfo.Colors.Count();
        }

        public static PathGradientBrush CreatePathGradientBrush(PathFillInfo pathFillInfo)
        {
            if (pathFillInfo.GraphicsPath == null)
                throw new Exception("graphicsPath == null for CreatePathGradientBrush.");
            pathFillInfo.GraphicsPath.Flatten();
            var pthGrBr = new PathGradientBrush(pathFillInfo.GraphicsPath);
            pthGrBr.WrapMode = WrapMode.TileFlipXY;
            pthGrBr.SetSigmaBellShape(1F);
            pthGrBr.CenterColor = pathFillInfo.CenterColor;
            var colorInfo = pathFillInfo.ColorInfo;
            var colorMode = pathFillInfo.ColorMode;
            if (colorInfo == null || colorInfo.Positions.Count() < 2)
                colorMode = PathColorModes.Normal;
            switch (colorMode)
            {
                case PathColorModes.Normal:
                    pthGrBr.SurroundColors = new Color[] { pathFillInfo.BoundaryColor };
                    break;
                case PathColorModes.Radial:
                    ColorBlend colorBlend = new ColorBlend();
                    if (colorInfo.TranslucentWaves)
                    {
                        colorInfo.InitRadialWaves();
                        colorBlend.Colors = colorInfo.RadialWaveColors.ToArray();
                        colorBlend.Positions = colorInfo.RadialWavePositions.ToArray();
                    }
                    else
                    {
                        colorBlend.Colors = colorInfo.Colors.ToArray();
                        colorBlend.Positions = colorInfo.Positions.ToArray();
                    }
                    pthGrBr.InterpolationColors = colorBlend;
                    break;
                case PathColorModes.Surround:
                    colorInfo.ComputeSurroundColors(pathFillInfo);
                    pthGrBr.SurroundColors = colorInfo.SurroundColors;
                    break;
            }
            return pthGrBr;
        }

        public override void CreateFillBrush()
        {
            if (FillBrush != null)
                FillBrush.Dispose();
            FillBrush = CreatePathGradientBrush(this);
        }

        public override FillInfo GetCopy(Pattern parentPattern)
        {
            PathFillInfo copy = new PathFillInfo(parentPattern);
            copy.ColorMode = this.ColorMode;
            copy.BoundaryColor = this.BoundaryColor;
            copy.CenterColor = this.CenterColor;
            if (this.ColorMode != PathColorModes.Normal)
                copy.ColorInfo.CopyProperties(this.ColorInfo);
            return copy;
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(PathFillInfo);
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(node, nameof(ColorMode), ColorMode);
            node.AppendChild(xmlTools.CreateXmlNode(nameof(BoundaryColor), BoundaryColor));
            node.AppendChild(xmlTools.CreateXmlNode(nameof(CenterColor), CenterColor));
            if (ColorInfo != null)
                ColorInfo.ToXml(node, xmlTools, "ColorInfo");
            return xmlTools.AppendToParent(parentNode, node);
        }

        public override void FromXml(XmlNode node)
        {
            ColorMode = (PathColorModes)Tools.GetEnumXmlAttr(node, "ColorMode", 
                                                             PathColorModes.Normal);
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case nameof(BoundaryColor):
                        BoundaryColor = Tools.GetColorFromXml(childNode);
                        break;
                    case nameof(CenterColor):
                        CenterColor = Tools.GetColorFromXml(childNode);
                        break;
                    case "ColorInfo":
                        if (ColorInfo != null)
                            ColorInfo.FromXml(childNode);
                        break;
                }
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            base.Dispose();
            if (this.GraphicsPath != null)
            {
                this.GraphicsPath.Dispose();
                this.GraphicsPath = null;
            }
        }
    }

    public class TextureFillInfo: FillInfo, IXml
    {
        public TextureFillInfo(Pattern parentPattern): base(parentPattern)
        {
        }

        private string textureImageFileName = string.Empty;

        private static Dictionary<string, Image> TextureImagesByFileName =
                   new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public override FillTypes FillType => FillTypes.Texture;

        public string TextureImageFileName
        {
            get { return textureImageFileName; }
            set
            {
                string sValue = value ?? string.Empty;
                if (textureImageFileName != sValue)
                {
                    textureImageFileName = sValue;
                    SetFillBrushToNull();
                    //CreateFillBrush();
                    Image image = GetTextureImage(TextureImageFileName);
                    if (image != null)
                    {
                        SampleColor = ((Bitmap)image).GetPixel(0, 0);
                    }
                }
            }
        }

        public Point TextureOffset { get; set; }

        public Color SampleColor { get; private set; }

        //public bool PreserveTextureSize { get; set; }

        public float TextureScale { get; set; } = 1F;

        public TextureImageModes ImageMode { get; set; } = TextureImageModes.Tile;

        public override void ApplyTransforms(float scaleX = 1F)
        {
            TextureBrush txtrBr = FillBrush as TextureBrush;
            if (txtrBr == null)
                return;
            scaleX *= TextureScale;
            Point offset = TextureOffset;
            float scaleY = scaleX;
            //double rotation = 0;
            if ((ImageMode == TextureImageModes.StretchTile || ImageMode == TextureImageModes.Stretch) && txtrBr.Image != null)
            {
                offset = new Point(0, 0);
                Complex zVector = ParentPattern.ZVector;
                Complex zSize = new Complex();
                BasicOutline rectOutline = ParentPattern.GetSingleBasicOutline(BasicOutlineTypes.Rectangle);
                if (rectOutline != null)
                {
                    double modulusSquared = zVector.GetModulusSquared();
                    double widthSquared = modulusSquared / (rectOutline.AddDenom * rectOutline.AddDenom + 1);
                    zSize.Re = 2D * Math.Sqrt(widthSquared);
                    zSize.Im = 2D * Math.Sqrt(modulusSquared - widthSquared);
                    //rotation = zVector.GetArgument();
                    //rotation += Math.Acos(widthSquared / modulusSquared);
                    //rotation = Tools.RadiansToDegrees(rotation);
                }
                else
                    zSize.Re = zSize.Im = zVector.GetModulus();
                if (zSize.Re != 0 && zSize.Im != 0)
                {
                    if (ImageMode == TextureImageModes.StretchTile && scaleX != 0)
                    {
                        float scaledWidth = scaleX * txtrBr.Image.Width;
                        float scaledHeight = scaleX * txtrBr.Image.Height;
                        float tilesAcross = (float)Math.Ceiling(zSize.Re / scaledWidth);
                        float tilesDown = (float)Math.Ceiling(zSize.Im / scaledHeight);
                        scaledWidth = (float)zSize.Re / tilesAcross;
                        scaleX = scaledWidth / txtrBr.Image.Width;
                        scaledHeight = (float)zSize.Im / tilesDown;
                        scaleY = scaledHeight / txtrBr.Image.Height;
                    }
                    else if (ImageMode == TextureImageModes.Stretch)
                    {
                        scaleX = (float)zSize.Re / txtrBr.Image.Width;
                        scaleY = (float)zSize.Im / txtrBr.Image.Height;
                    }
                }
            }
            txtrBr.ResetTransform();
            //if (rotation != 0)
            //    txtrBr.RotateTransform((float)rotation);
            //Debug.WriteLine($"TranslateTransform: offset = {offset.X},{offset.Y}");
            txtrBr.TranslateTransform(
                ParentPattern.Center.X + scaleX * offset.X,
                ParentPattern.Center.Y + scaleY * offset.Y);
            if ((scaleX != 1F || scaleY != 1F) && scaleX != 0F)
            {  //TextureScale != 1F || (scaleX != 1F && PreserveTextureSize == (scaleX > 1F)))
                txtrBr.ScaleTransform(scaleX, scaleY);
                //Debug.WriteLine($"ScaleTransform({scaleX}, {scaleY})");
            }
        }

        public override void CreateFillBrush()
        {
            if (FillBrush != null)
                FillBrush.Dispose();
            Image image = GetTextureImage(TextureImageFileName);
            if (image != null)
            {
                var textureBrush = new TextureBrush(image);
                textureBrush.WrapMode = WrapMode.Tile; //this.ImageWrapMode;
                //textureBrush.TranslateTransform(TextureOffset.X, TextureOffset.Y);
                FillBrush = textureBrush;
                ApplyTransforms();
            }
            else
                //File not found.
                FillBrush = new SolidBrush(Color.Red);
        }

        public static Image GetTextureImage(string imageFileName)
        {
            Image image;
            if (!TextureImagesByFileName.TryGetValue(imageFileName, out image))
            {
                if (File.Exists(imageFileName))
                {
                    image = Bitmap.FromFile(imageFileName);
                    if (Math.Max(image.Width, image.Height) <= 350)
                        TextureImagesByFileName.Add(imageFileName, image);
                }
                else
                    image = null;
            }
            return image;
        }

        public override FillInfo GetCopy(Pattern parentPattern)
        {
            TextureFillInfo copy = new TextureFillInfo(parentPattern);
            copy.TextureOffset = this.TextureOffset;
            //copy.PreserveTextureSize = this.PreserveTextureSize;
            copy.TextureScale = this.TextureScale;
            copy.ImageMode = this.ImageMode;
            copy.TextureImageFileName = this.TextureImageFileName;
            //copy.ImageWrapMode = this.ImageWrapMode;
            return copy;
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(TextureFillInfo);
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            //if (PreserveTextureSize)
            //    Tools.AppendXmlAttribute(sb, nameof(PreserveTextureSize), PreserveTextureSize);
            xmlTools.AppendXmlAttribute(node, nameof(TextureScale), TextureScale);
            if (ImageMode != TextureImageModes.Tile)
                xmlTools.AppendXmlAttribute(node, nameof(ImageMode), ImageMode);
            xmlTools.AppendChildNode(node, nameof(TextureImageFileName), TextureImageFileName);
            node.AppendChild(xmlTools.CreateXmlNode(nameof(TextureOffset), TextureOffset));
            return xmlTools.AppendToParent(parentNode, node);
        }

        public override void FromXml(XmlNode node)
        {
            bool preserveTextureSize = (bool)Tools.GetXmlAttribute("PreserveTextureSize", typeof(bool), node, defaultValue: false);
            TextureScale = (float)Tools.GetXmlAttribute(nameof(TextureScale), typeof(float), node, defaultValue: 0F);
            ImageMode = Tools.GetEnumXmlAttr(node, nameof(ImageMode), TextureImageModes.Tile);
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case nameof(TextureImageFileName):
                        TextureImageFileName = Tools.GetValidTextureFileName(Tools.GetXmlNodeValue(childNode));
                        break;
                    case nameof(TextureOffset):
                        TextureOffset = Tools.GetPointFromXml(childNode);
                        break;
                }
            }
            if (TextureScale == 0)
            {
                //Legacy code.
                if (preserveTextureSize)
                    TextureScale = 1F;
                else
                {
                    int minDim = Math.Min(WhorlDesign.StaticPictureBoxSize.Width,
                                          WhorlDesign.StaticPictureBoxSize.Height);
                    if (minDim == 0)
                        TextureScale = 1F;
                    else
                        TextureScale = (float)minDim / WhorlSettings.Instance.QualitySize;
                }
            }
            CreateFillBrush();
            //ApplyTransforms();
        }
    }

    public class BackgroundFillInfo : FillInfo
    {
        public override FillTypes FillType => FillTypes.Background;
        //public Image BackgroundSectionImage { get; private set; }

        //private RectangleF bounds { get; set; }
        //private GraphicsPath backgroundGrPath;
        //private PathGradientBrush backgroundPthGrBrush;

        public BackgroundFillInfo(Pattern parent) : base(parent)
        {
        }

        //public void ClearBackgroundImage()
        //{
        //    //if (BackgroundSectionImage != null)
        //    //{
        //    //    BackgroundSectionImage.Dispose();
        //    //    BackgroundSectionImage = null;
        //    //}
        //    //SetFillBrushToNull();
        //}

        //private void SetBackgroundImage()
        //{
        //    //var design = ParentPattern.Design;
        //    //ClearBackgroundImage();
        //    //ParentPattern.ComputeCurvePoints(ParentPattern.ZVector);
        //    //Size size = design.DesignSize;
        //    //if (size.Width == 0 || size.Height == 0)
        //    //    throw new Exception("DesignSize was not set.");
        //    //bounds = Tools.GetBoundingRectangle(ParentPattern.CurvePoints);
        //    //bounds = Tools.RectangleFromVertices(new PointF(Math.Max(0, bounds.Left), Math.Max(0, bounds.Top)),
        //    //                                     new PointF(Math.Min(size.Width, bounds.Right), Math.Min(size.Height, bounds.Bottom)));
        //    //BackgroundSectionImage = design.CreateDesignBitmap(size.Width, size.Height, ref backgroundGrPath, ref backgroundPthGrBrush);
        //    //using (Graphics g = Graphics.FromImage(BackgroundSectionImage))
        //    //{
        //    //    var patterns = design.DesignPatterns.Where(p => p.IsBackgroundPattern && !p.HasPixelRendering);
        //    //    DrawDesign.DrawPatterns(g, null, patterns, size);
        //    //}
        //    //if (bounds.Width < 1 || bounds.Height < 1)
        //    //{
        //    //    BackgroundSectionImage = new Bitmap(1, 1);
        //    //}
        //    //else
        //    //{
        //    //    BackgroundSectionImage = new Bitmap((int)bounds.Width, (int)bounds.Height);
        //    //    Bitmap bitmap = design.CreateDesignBitmap(size.Width, size.Height, ref backgroundGrPath, ref backgroundPthGrBrush);
        //    //    using (Graphics g = Graphics.FromImage(bitmap))
        //    //    {
        //    //        var patterns = design.DesignPatterns.Where(p => p.IsBackgroundPattern && !p.HasPixelRendering);
        //    //        DrawDesign.DrawPatterns(g, null, patterns, bitmap.Size);
        //    //    }
        //    //    using (Graphics g = Graphics.FromImage(BackgroundSectionImage))
        //    //    {
        //    //        g.DrawImage(bitmap, new RectangleF(PointF.Empty, bounds.Size), bounds, GraphicsUnit.Pixel);
        //    //    }
        //    //}
        //}

        public override void CreateFillBrush()
        {
            //if (BackgroundSectionImage == null)
            //    SetBackgroundImage();
            Bitmap bitmap = ParentPattern.Design.BackgroundBitmap;
            SetFillBrushToNull();
            if (bitmap == null)
            {
                FillBrush = new SolidBrush(Color.Black);
            }
            else
            {
                var txtrBrush = new TextureBrush(bitmap, WrapMode.Tile);
                //txtrBrush.TranslateTransform(bounds.Left, bounds.Top);
                FillBrush = txtrBrush;
            }
        }

        public override void FromXml(XmlNode node)
        {
        }

        public override FillInfo GetCopy(Pattern parentPattern)
        {
            return new BackgroundFillInfo(parentPattern)
            {
                //BackgroundSectionImage = this.BackgroundSectionImage
            };
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName ?? nameof(BackgroundFillInfo));
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }
    }

}
