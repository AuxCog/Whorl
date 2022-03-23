using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms;
using ParserEngine;
using System.Diagnostics;

namespace Whorl
{
    public delegate void RenderDelegate(int step, bool initial = false);

    public class Pattern : BasePattern, IDisposable, IFillInfo, ICloneable
    {
        public enum MergeOperations
        {
            Sum,
            Max,
            Min
        }
        public enum RenderModes
        {
            Paint,
            Stain
        }

        public const float MinPenWidth = 0.5F;

        /// <summary>
        /// Class for recursive subpatterns.
        /// </summary>
        public class PatternInfo
        {
            public Pattern Pattern { get; set; }
            public virtual Complex ZVector
            {
                get { return Pattern.ZVector; }
            }
            public virtual PointF Center
            {
                get { return Pattern.Center; }
            }
            public virtual int RecursiveLevel
            {   get { return -1; }
            }

            public PatternInfo(Pattern pattern)
            {
                Pattern = pattern;
            }

            public virtual PatternInfo GetCopy()
            {
                return new PatternInfo(this.Pattern);
            }
        }

        public class SubpatternInfo : PatternInfo
        {
            public override Complex ZVector { get; }
            public override PointF Center { get; }
            public override int RecursiveLevel { get; }

            public SubpatternInfo(Pattern pattern, Pattern infoPattern, int recursiveLevel): base(pattern)
            {
                ZVector = infoPattern.ZVector;
                Center = infoPattern.Center;
                RecursiveLevel = recursiveLevel;
            }

            public SubpatternInfo(SubpatternInfo source): base(source.Pattern)
            {
                ZVector = source.ZVector;
                Center = source.Center;
                RecursiveLevel = source.RecursiveLevel;
            }

            public override PatternInfo GetCopy()
            {
                return new SubpatternInfo(this);
            }
        }

        public class PatternSection: BaseObject, IXml
        {
            public bool IsSection { get; set; }
            private float _sectionAmplitudeRatio = 0.8F;
            public float SectionAmplitudeRatio
            {
                get { return _sectionAmplitudeRatio; }
                set
                {
                    if (value <= 0 || value >= 1)
                        throw new Exception("SectionAmplitudeRatio must be > 0 and < 1");
                    _sectionAmplitudeRatio = (float)Math.Round(value, 3);
                }
            }
            public bool RecomputeInnerSection { get; set; }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = "PatternSection";
                XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributesExcept(node, this);  //Append all properties.
                return xmlTools.AppendToParent(parentNode, node);
            }

            public void FromXml(XmlNode node)
            {
                Tools.GetXmlAttributesExcept(this, node);
            }
        }

        public class PatternRecursionInfo : BaseObject, IXml
        {
            public int Id { get; }
            public int IndexOffset { get; set; } = 0;
            private float scaleRatioPercentage = 100F;
            public float ScaleRatioPercentage
            {
                get { return scaleRatioPercentage; }
                set
                {
                    if (scaleRatioPercentage != value)
                    {
                        scaleRatioPercentage = value;
                        ScaleRatio = scaleRatioPercentage / 100F;
                    }
                }
            }
            public float ScaleRatio { get; private set; } = 1F;
            private float rotationOffsetDegrees = 0F;
            public float RotationOffsetDegrees
            {
                get { return rotationOffsetDegrees; }
                set
                {
                    if (rotationOffsetDegrees != value)
                    {
                        rotationOffsetDegrees = value;
                        RotationOffset = (float)Tools.DegreesToRadians(rotationOffsetDegrees);
                    }
                }
            }
            public float RotationOffset { get; private set; } = 0F;

            public PatternRecursionInfo(int id)
            {
                Id = id;
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(PatternRecursionInfo);
                XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributesExcept(node, this, nameof(Id), nameof(ScaleRatio), nameof(RotationOffset) );
                return xmlTools.AppendToParent(parentNode, node);
            }

            public void FromXml(XmlNode node)
            {
                Tools.GetXmlAttributesExcept(this, node);
            }

            public void CopyProperties(PatternRecursionInfo source)
            {
                Tools.CopyProperties(this, source, excludedPropertyNames:
                    new string[] { nameof(Id), nameof(ScaleRatio), nameof(RotationOffset) });
            }
        }

        public class PatternRecursion: BaseObject, IXml
        {
            private int repetitions = 4;
            public bool IsRecursive { get; set; }
            public bool IsRecursivePattern { get; set; }
            public float Scale { get; set; } = 0.5F;
            public int Repetitions
            {
                get { return repetitions; }
                set
                {
                    if (value <= 0)
                        throw new Exception("Recursion Repetitions must be positive.");
                    if (repetitions == value)
                        return;
                    repetitions = value;
                    RotationRatio = 1F / repetitions;
                    if (InfoList.Count < repetitions)
                    {
                        for (int i = InfoList.Count; i < repetitions; i++)
                            InfoList.Add(new PatternRecursionInfo(id: i + 1));
                    }
                    else if (InfoList.Count > repetitions)
                        InfoList.RemoveRange(repetitions, InfoList.Count - repetitions);
                }
            }
            public List<PatternRecursionInfo> InfoList { get; } = new List<PatternRecursionInfo>();
            public double RotationAngle { get; set; } = 0D;
            public float RotationRatio { get; private set; } = 0.25F;
            public float RotationOffsetRatio { get; set; }
            public float AutoSampleFactor { get; set; } = 10F;
            public int Depth { get; set; } = 2;
            public bool DrawAsPatterns { get; set; }
            public bool OffsetPatterns { get; set; }
            public bool UsePerspectiveScale { get; set; }
            public float OffsetAdjustmentFactor { get; set; }
            public bool UnderlayDrawnPatterns { get; set; }
            public bool SkipFirstDrawnPattern { get; set; }
            public List<Pattern> RecursionPatterns { get; } = new List<Pattern>();
            public Pattern ParentPattern { get; private set; }
            public List<List<SubpatternInfo>> RecursionPatternsInfo { get; } = new List<List<SubpatternInfo>>();

            public PatternRecursion(Pattern parentPattern)
            {
                ParentPattern = parentPattern;
            }

            public Pattern AddRecursionPattern(Pattern origPattern)
            {
                Pattern sourcePattern = RecursionPatterns.LastOrDefault();
                if (sourcePattern == null)
                    sourcePattern = origPattern;
                Pattern recursionPattern = new Pattern(sourcePattern, isRecursivePattern: true, recursiveParent: origPattern);
                RecursionPatterns.Add(recursionPattern);
                return recursionPattern;
            }

            public void CopyProperties(PatternRecursion source, Pattern parentPattern)
            {
                repetitions = 0;
                foreach (var sourceInfo in source.InfoList)
                {
                    var info = new PatternRecursionInfo(sourceInfo.Id);
                    info.CopyProperties(sourceInfo);
                    InfoList.Add(info);
                }
                if (!source.IsRecursivePattern)
                {
                    //RecursionPatterns = new List<Pattern>();
                    RecursionPatterns.AddRange(source.RecursionPatterns.Select(
                            ptn => new Pattern(ptn, isRecursivePattern: true, recursiveParent: parentPattern)));
                    //foreach (Pattern rPattern in source.RecursionPatterns)
                    //{
                    //    RecursionPatterns.Add(new Pattern(rPattern, isRecursivePattern: true));
                    //}
                    RecursionPatternsInfo.AddRange(source.RecursionPatternsInfo.Select(
                                lst => new List<SubpatternInfo>(lst.Select(info => new SubpatternInfo(info)))));
                }
                Tools.CopyProperties(this, source, excludedPropertyNames: new string[]
                                     { nameof(IsRecursivePattern), nameof(ParentPattern) });
                if (parentPattern != null)
                    ParentPattern = parentPattern;
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = "PatternRecursion";
                XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributesExcept(node, this, nameof(RotationRatio));
                foreach (PatternRecursionInfo info in InfoList)
                {
                    info.ToXml(node, xmlTools, "PatternRecursionInfo");
                }
                if (RecursionPatterns != null)
                {
                    foreach (Pattern rPattern in RecursionPatterns)
                    {
                        rPattern.ToXml(node, xmlTools, "RecursionPattern");
                    }
                }
                return xmlTools.AppendToParent(parentNode, node);
            }

            public void FromXml(XmlNode node)
            {
                repetitions = 0;
                //RecursionPatterns = null;
                InfoList.Clear();
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case "RecursionPattern":
                            //if (RecursionPatterns == null)
                            //    RecursionPatterns = new List<Pattern>();
                            var rPattern = new Pattern(ParentPattern.Design, childNode);
                            rPattern.Recursion.IsRecursivePattern = true;
                            rPattern.Recursion.ParentPattern = ParentPattern;
                            RecursionPatterns.Add(rPattern);
                            break;
                        case "PatternRecursionInfo":
                            var info = new PatternRecursionInfo(id: InfoList.Count + 1);
                            info.FromXml(childNode);
                            InfoList.Add(info);
                            break;
                    }
                }
                Tools.GetXmlAttributesExcept(this, node, new string[] { "ParentPatternIsRecursive" });
                //Legacy code:
                var xmlAttr = node.Attributes["ParentPatternIsRecursive"];
                if (xmlAttr != null)
                    IsRecursivePattern = bool.Parse(xmlAttr.Value);
            }
        }

        public class TileInfo: BaseObject, IXml
        {
            public bool TilePattern { get; set; }
            public int PatternsPerRow { get; set; } = 10;
            //public double PatternSizeRatio { get; set; } = 1;
            public int BorderWidth { get; set; }
            public float TopMargin { get; set; }
            public float LeftMargin { get; set; }
            public float BottomMargin { get; set; }
            public float RightMargin { get; set; }
            //public PointF GridAdjustment { get; set; } = new PointF(0, 0);

            private static readonly string[] excludedProperties = { "PatternSizeRatio" };

            public void CopyProperties(TileInfo source)
            {
                Tools.CopyProperties(this, source);
            }

            public Rectangle GetInnerRectangle(Size imgSize)
            {
                Size innerSize = new Size((int)((1F - LeftMargin - RightMargin) * imgSize.Width),
                                          (int)((1F - TopMargin - BottomMargin) * imgSize.Height));
                Point topLeft = new Point((int)(LeftMargin * imgSize.Width), (int)(TopMargin * imgSize.Height));
                return new Rectangle(topLeft, innerSize);
            }

            public SizeF GetSizeInfo(Rectangle innerRect, out int patternsPerCol)
            {
                float rectWidth = (float)innerRect.Width / PatternsPerRow;
                patternsPerCol = (int)(innerRect.Height / rectWidth);
                float rectHeight = (float)innerRect.Height / patternsPerCol;
                return new SizeF(rectWidth, rectHeight);
            }

            public SizeF GetGridSize(Rectangle innerRect)
            {
                float baseSize = 25;
                SizeF rectSize = GetSizeInfo(innerRect, out int patternsPerCol);
                int gridsAcross = 2 * ((int)(rectSize.Width / baseSize) / 2);
                int gridsDown = 2 * ((int)(rectSize.Height / baseSize) / 2);
                if (gridsAcross == 0 || gridsDown == 0)
                    return rectSize;
                float gridWidth = rectSize.Width / gridsAcross;
                float gridHeight = rectSize.Height / gridsDown;
                return new SizeF(gridWidth, gridHeight);
            }

            public void FromXml(XmlNode node)
            {
                Tools.GetXmlAttributesExcept(this, node, excludedProperties);
                //foreach (XmlNode childNode in node.ChildNodes)
                //{
                //    if (childNode.Name == nameof(GridAdjustment))
                //    {
                //        GridAdjustment = Tools.GetPointFFromXml(childNode);
                //    }
                //}
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(TileInfo);
                XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributesExcept(node, this);
                //node.AppendChild(xmlTools.CreateXmlNode(nameof(GridAdjustment), GridAdjustment));
                return xmlTools.AppendToParent(parentNode, node);
            }
        }

        public class RenderingInfo : BaseObject, IXml, IColorNodeList
        {
            //private const int distanceSquareRows = 10;
            private class InfoExt : PixelRenderInfo
            {
                public InfoExt(Pattern.RenderingInfo parent, 
                               List<DistancePatternInfo> distancePatternInfos, 
                               Func<PointF, PointF> transformPoint) : 
                    base(parent, distancePatternInfos, transformPoint)
                {
                }

                public void SetXY(PointF xy)
                {
                    X = xy.X;
                    Y = xy.Y;
                }
                public void SetDraftSize(int size)
                {
                    DraftSize = size;
                }
                public void SetPassType(PassTypes passType)
                {
                    PassType = passType;
                }
                public void SetInfo(SizeF boundsSize, PointF center, double maxPosition,
                                    double scaleFactor, int maxModulusStep)
                {
                    BoundsSize = boundsSize;
                    Center = center;
                    MaxPosition = maxPosition;
                    ScaleFactor = scaleFactor;
                    MaxModulusStep = maxModulusStep;
                }

                public void SetMaxModulus(double val)
                {
                    MaxModulus = val;
                }

                public void SetModulusStep(int modulusStep)
                {
                    ModulusStep = modulusStep;
                }

                public void SetAngleStep(int angleStep)
                {
                    AngleStep = angleStep;
                }

                public void SetMaxAngleStep(int maxAngleStep)
                {
                    MaxAngleStep = maxAngleStep;
                }

                public void SetIntXY(Point intXY)
                {
                    IntXY = intXY;
                }

                public void SetPatternAngle(double angle)
                {
                    PatternAngle = angle;
                }

                public void SetPointOffset(DoublePoint dp)
                {
                    PointOffset = dp;
                }

                public void SetCenter(PointF center)
                {
                    Center = center;
                }

                public void SetScaleFactor(double factor)
                {
                    ScaleFactor = factor;
                }

                public void SetDistanceToPath(double distance)
                {
                    DistanceToPath = distance;
                }

                //public void SetDistanceToPath(int i, double distance, PointF nearestPoint)
                //{
                //    DistancesToPaths[i] = distance;
                //    NearestPoints[i] = nearestPoint;
                //}

                public void AllocateDistancesToPaths(int length)
                {
                    DistancesToPaths = new double[length];
                    DistancePatternCenters = new PointF[length];
                    NearestPoints = new PointF[length];
                }
            }

            //private struct DistancePointInfo
            //{
            //    public PointF Point { get; }
            //    public PointF RotationVector { get; }

            //    public DistancePointInfo(PointF point, PointF rotationVector)
            //    {
            //        Point = point;
            //        RotationVector = rotationVector;
            //    }
            //}

            private class DistanceSquare
            {
                public PointF Center { get; }
                public PointF[] Points { get; }
                public double Distance { get; set; }

                public DistanceSquare(PointF topLeft, PointF[] points)
                {
                    Center = topLeft;
                    Points = points;
                }
            }

            public class DistancePatternSettings : IXml
            {
                public DistancePatternInfo Parent { get; }
                public bool UseFadeOut { get; set; }
                public double FadeStartRatio { get; set; } = 1.1;
                public double FadeEndRatio { get; set; } = 1.25;
                public double EndDistanceValue { get; set; } = 10000.0;
                public bool AutoEndValue { get; set; } = true;
                public double CenterSlope { get; set; }
                public int? InfluencePointId { get; set; }
                public bool Enabled { get; set; } = true;


                public DistancePatternSettings(DistancePatternInfo parent)
                {
                    Parent = parent;
                }

                public DistancePatternSettings(DistancePatternSettings source, DistancePatternInfo parent)
                {
                    Parent = parent;
                    Tools.CopyProperties(this, source);
                }

                public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
                {
                    XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName ?? nameof(DistancePatternSettings));
                    xmlTools.AppendXmlAttributesExcept(xmlNode, this);
                    return xmlTools.AppendToParent(parentNode, xmlNode);
                }

                public void FromXml(XmlNode node)
                {
                    Tools.GetXmlAttributesExcept(this, node);
                }
            }

            public class DistancePatternInfo : GuidKey
            {
                private const float NewTransformXmlVersion = 1.1F;
                public Pattern DistancePattern { get; private set; }
                public DistancePatternSettings DistancePatternSettings { get; }
                public double MaxModulus { get; set; }
                public PointF DistancePatternCenter { get; set; }
                public Complex OrigZVector { get; private set; } = Complex.Zero;
                //public PointF OrigCenter { get; private set; }
                //public Guid Guid { get; }
                public float XmlVersion { get; private set; }
                public WhorlDesign Design { get; }

                public DistancePatternInfo(Pattern parent, Pattern distancePattern) //, Complex prevZVector)
                {
                    DistancePatternSettings = new DistancePatternSettings(this);
                    XmlVersion = WhorlDesign.CurrentXmlVersion;
                    //Guid = Guid.NewGuid();
                    Design = parent.Design;
                    SetDistancePattern(parent, distancePattern, transform: true);
                }

                //For CreateFromXml.
                private DistancePatternInfo(WhorlDesign design)
                {
                    DistancePatternSettings = new DistancePatternSettings(this);
                    //Guid = Guid.NewGuid();
                    if (design == null)
                        throw new NullReferenceException("design cannot be null.");
                    Design = design;
                }

                public DistancePatternInfo(DistancePatternInfo source) : base(source)
                {
                    //Guid = source.Guid;
                    DistancePatternSettings = new DistancePatternSettings(source.DistancePatternSettings, this);

                    DistancePattern = source.DistancePattern.GetCopy();

                    Design = source.Design;
                    OrigZVector = source.OrigZVector;
                    //OrigCenter = source.OrigCenter;
                    XmlVersion = source.XmlVersion;
                }

                public void SetInfluencePointCenter(InfluencePointInfo influencePoint)
                {
                    if (influencePoint != null)
                        influencePoint.SetDistancePatternCenter(this);
                    DistancePatternSettings.InfluencePointId = influencePoint?.Id;
                }

                public void SetDistancePattern(Pattern parent, Pattern distancePattern, bool transform = false)
                {
                    if (distancePattern == null)
                        throw new NullReferenceException("distancePattern == null");
                    OrigZVector = parent.ZVector;
                    distancePattern = distancePattern.GetCopy();
                    distancePattern.ClearPixelRendering();
                    if (transform)
                    {
                        TransformDistancePattern(parent, distancePattern, distancePattern.Center);
                    }
                    if (DistancePattern != null)
                        DistancePattern.Dispose();
                    DistancePattern = distancePattern;
                    parent.ClearRenderingCache();
                }

                //private void TransformDistancePattern(Pattern parent, Pattern distancePattern,
                //                                      PointF center, bool scaleZVector = true)
                //{
                //    //Complex zScale = new Complex(1.0, 0.0) / (parent.PixelRendering.ZoomFactor * parent.ZVector);
                //    //if (scaleZVector)
                //    //{
                //    //    distancePattern.SetZVector(zScale * distancePattern.ZVector, scaleInfluencePoints: false);
                //    //    distancePattern.ScaleInfluencePoints(prevZVector);
                //    //}
                //    PointF pCenter = new PointF(center.X - parent.Center.X, 
                //                                center.Y - parent.Center.Y);
                //    //PointF pScale = new PointF((float)zScale.Re, (float)zScale.Im);
                //    //pCenter = Tools.RotatePoint(pCenter, pScale);  //Also scales pCenter.
                //    distancePattern.Center = pCenter;
                //}

                public void TransformDistancePattern(Pattern parent, Pattern distancePattern, PointF center)
                {
                    PointF pCenter = new PointF(center.X - parent.Center.X,
                                                center.Y - parent.Center.Y);
                    if (parent.ZVector != OrigZVector)
                    {
                        Complex zScale = OrigZVector / parent.ZVector;
                        PointF pScale = new PointF((float)zScale.Re, (float)zScale.Im);
                        pCenter = Tools.RotatePoint(pCenter, pScale);  //Also scales pCenter.
                    }
                    //PointF pScale = new PointF((float)zScale.Re, (float)zScale.Im);
                    //pCenter = Tools.RotatePoint(pCenter, pScale);  //Also scales pCenter.
                    distancePattern.Center = pCenter;
                    //TransformDistancePattern(parent, distancePattern, center, Complex.Zero, scaleZVector: false);
                }

                private PointF GetInfo(Pattern parent, out Complex zScale)
                {
                    zScale = parent.PixelRendering.ZoomFactor * (parent.ZVector / OrigZVector);
                    PointF vec = DistancePattern.Center;
                    PointF pScale = new PointF((float)zScale.Re, (float)zScale.Im);
                    vec = Tools.RotatePoint(vec, pScale);  //Also scales vec.
                    return new PointF(parent.Center.X + vec.X,
                                      parent.Center.Y + vec.Y);
                }

                public PointF GetDistancePatternCenter(Pattern parent)
                {
                    return GetInfo(parent, out _);
                }

                public Pattern GetDistancePattern(Pattern parent)
                {
                    Pattern distCopy = DistancePattern.GetCopy();
                    PointF center = GetInfo(parent, out Complex zScale);
                    distCopy.ZVector = zScale * distCopy.ZVector;
                    distCopy.Center = center;
                    return distCopy;
                }

                public DistancePatternInfo GetCopy()
                {
                    return new DistancePatternInfo(this);
                }

                public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
                {
                    XmlNode node = xmlTools.CreateXmlNode(xmlNodeName ?? "DistancePatternInfo");
                    Tools.SetXmlVersion(node, xmlTools);
                    DistancePatternSettings.ToXml(node, xmlTools);
                    node.AppendChild(xmlTools.CreateXmlNode(nameof(OrigZVector), OrigZVector));
                    DistancePattern.ToXml(node, xmlTools);
                    return xmlTools.AppendToParent(parentNode, node);
                }

                public void FromXml(XmlNode node, Pattern parent)
                {
                    XmlVersion = Tools.GetXmlVersion(node);
                    bool readOrigZVector = false;
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case nameof(DistancePatternSettings):
                                DistancePatternSettings.FromXml(childNode);
                                break;
                            case nameof(OrigZVector):
                                OrigZVector = XmlTools.GetComplexFromXml(childNode);
                                readOrigZVector = true;
                                break;
                            case "OrigCenter":  //Legacy code.
                                //origCenter = XmlTools.GetPointFFromXml(childNode);
                                break;
                            default:
                                DistancePattern = CreatePatternFromXml(Design, childNode);
                                if (DistancePattern == null)
                                    throw new Exception($"Invalid DistancePattern XML node {childNode.Name}");
                                break;
                        }
                    }
                    if (!readOrigZVector)
                    {   //Legacy code.
                        OrigZVector = parent.ZVector;
                        Complex zScale = parent.PixelRendering.ZoomFactor * parent.ZVector;
                        PointF vec = DistancePattern.Center;
                        PointF pScale = new PointF((float)zScale.Re, (float)zScale.Im);
                        vec = Tools.RotatePoint(vec, pScale);  //Also scales vec.
                        DistancePattern.ZVector *= zScale;
                        DistancePattern.Center = vec;
                    }
                }

                private void DoLegacyTransform(Complex origZVector, PointF origCenter) //, RenderingInfo parent)
                {
                    if (XmlVersion < NewTransformXmlVersion && DistancePattern != null)
                    {
                        //Legacy code:
                        Complex zScale = Complex.One / origZVector;
                        DistancePattern.ZVector *= zScale;
                        PointF pScale = new PointF((float)zScale.Re, (float)zScale.Im);
                        PointF vec = new PointF(
                            DistancePattern.Center.X - origCenter.X,
                            DistancePattern.Center.Y - origCenter.Y);
                        vec = Tools.RotatePoint(vec, pScale);  //Also scales vec.
                        DistancePattern.Center = vec;
                    }
                }

                public static DistancePatternInfo CreateFromXml(WhorlDesign design, Pattern parent, XmlNode node) //, RenderingInfo parent)
                {
                    var distancePatternInfo = new DistancePatternInfo(design);
                    distancePatternInfo.FromXml(node, parent);
                    if (distancePatternInfo.XmlVersion < NewTransformXmlVersion)
                    {
                        Complex origZVector = Complex.One;
                        PointF origCenter = PointF.Empty;
                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            switch (childNode.Name)
                            {
                                case "OrigZVector":
                                    origZVector = XmlTools.GetComplexFromXml(childNode);
                                    break;
                                case "OrigCenter":
                                    origCenter = XmlTools.GetPointFFromXml(childNode);
                                    break;
                            }
                        }
                        distancePatternInfo.DoLegacyTransform(origZVector, origCenter); //, parent);
                    }
                    return distancePatternInfo;
                }

                //Needed for legacy code.
                public static DistancePatternInfo CreateFromXml(WhorlDesign design, Pattern distancePattern,
                                                  Complex origZVector, PointF origCenter)
                {
                    if (distancePattern == null)
                        throw new NullReferenceException("distancePattern == null");
                    var distancePatternInfo = new DistancePatternInfo(design);
                    distancePatternInfo.DistancePattern = distancePattern;
                    distancePatternInfo.DoLegacyTransform(origZVector, origCenter); //, parent);
                    //distancePatternInfo.OrigZVector = origZVector;
                    //distancePatternInfo.OrigCenter = origCenter;
                    return distancePatternInfo;
                }
            }
            private enum ParserVarNames
            {
                Info,
                Position
            }
            private enum BooleanOutputParamNames
            {
                ProvideFeedback
            }
            public const int RandomRangeCount = 1000;
            public Pattern ParentPattern { get; }
            private Pattern parentPatternCopy { get; set; }
            public int DraftSize { get; private set; } = 3;
            public bool SmoothedDraft { get; set; } = true;

            public bool Enabled { get; set; }
            private bool _formulaEnabled = true;
            public bool FormulaEnabled
            {
                get { return _formulaEnabled; }
                set
                {
                    if (_formulaEnabled != value)
                    {
                        _formulaEnabled = value;
                        ClearCache();
                    }
                }
            }
            public bool UseDistanceOutline { get; set; }
            private List<DistancePatternInfo> distancePatternInfos { get; } =
               new List<DistancePatternInfo>();
            public IEnumerable<DistancePatternInfo> GetDistancePatternInfos()
            {
                return distancePatternInfos;
            }
            private RaggedArrayRow<double[]>[] allDistanceInfo { get; set; }

            public int PatternLayerIndex { get; set; }
            public FormulaSettings FormulaSettings { get; private set; }
            public ColorNodeList ColorNodes { get; set; }
            public event EventHandler DistancePatternsCountChanged;
            private ValidIdentifier positionIdent { get; set; }
            private ushort[] cachedPositions { get; set; }
            public RectangleF BoundsRect { get; private set; }
            public delegate float GetPositionDelegate();
            public GetPositionDelegate GetPosition { get; set; }
            private InfoExt Info { get; }
            public PointF PanXY { get; set; }
            public float ZoomFactor { get; set; } = 1F;
            public PointsRandomOps PointsRandomOps { get; set; }
            private PointF transformedPanXY { get; set; }
            private PatternBoundsInfo patternBoundsInfo { get; set; }
            private PatternBoundsInfo[] distPatternsBoundsInfos { get; set; }
            private PointF patternCenter { get; set; }
            //private bool polarTraversal;
            private Complex drawnZVector { get; set; }
            private bool draftMode { get; set; }
            private float[] rowPositions { get; set; }
            private double positionAverage { get; set; } = 0;
            private bool isCSharpFormula { get; set; }
            private CSharpCompiledInfo.EvalInstance evalInstance { get; set; }
            private PointF origCenter { get; set; }
            private float floatScaleFactor { get; set; }
            private List<DistanceSquare>[] distanceSquaresArray { get; set; }
            private SizeF distanceSquareSize { get; set; }
            //private PointF[] distPathPoints { get; set; }
            private double distanceFactor { get; set; }
            private InfluenceLinkParentCollection influenceParentCollection { get; set; }
            public Pattern SeedPattern { get; private set; }
            //public WhorlDesign Design { get; }

            //private InstanceInfo[] parallelInfo { get; } = new InstanceInfo[Environment.ProcessorCount];

            public MainForm MainForm
            {
                get { return MainForm.DefaultMainForm; }
            }

            static RenderingInfo()
            {
                ExpressionParser.DeclareExternType(typeof(PixelRenderInfo));
            }

            public RenderingInfo(Pattern pattern, bool createColorNodes = true)
            {
                if (pattern == null)
                    throw new NullReferenceException("pattern cannot be null.");
                //if (design == null)
                //    throw new NullReferenceException("design cannot be null.");
                ParentPattern = pattern;
                //Design = pattern.Design;
                Info = new InfoExt(this, distancePatternInfos, p => TransformPoint(p.X, p.Y));
                if (createColorNodes)
                {
                    ColorNodes = new ColorNodeList();
                    ColorNodes.AddDefaultNodes();
                }
            }

            public Pattern GetParentPatternCopy()
            {
                if (parentPatternCopy == null)
                    parentPatternCopy = ParentPattern.GetCopy();
                return parentPatternCopy;
            }

            public RenderingInfo(RenderingInfo source, Pattern pattern) : this(pattern, createColorNodes: false)
            {
                ColorNodes = source.ColorNodes?.GetCopy();
                Enabled = source.Enabled;
                PatternLayerIndex = source.PatternLayerIndex;
                FormulaEnabled = source.FormulaEnabled;
                PanXY = source.PanXY;
                transformedPanXY = source.GetTransformedPanXY();
                ZoomFactor = source.ZoomFactor;
                SmoothedDraft = source.SmoothedDraft;
                UseDistanceOutline = source.UseDistanceOutline;
                distancePatternInfos.AddRange(source.distancePatternInfos.Select(dp => dp.GetCopy()));
                if (source.SeedPattern != null)
                    SetSeedPattern(source.SeedPattern);
                if (source.FormulaSettings != null)
                {
                    FormulaSettings = source.FormulaSettings.GetCopy(ConfigureParser, pattern: ParentPattern);
                    if (source.FormulaSettings.InfluenceLinkParentCollection != null)
                    {
                        FormulaSettings.InfluenceLinkParentCollection =
                            source.FormulaSettings.InfluenceLinkParentCollection.GetCopy(FormulaSettings, pattern);
                    }
                }
                if (source.PointsRandomOps != null)
                {
                    PointsRandomOps = new PointsRandomOps(source.PointsRandomOps);
                }
            }

            //public RenderingInfo GetCopy()
            //{
            //    return new RenderingInfo(this);
            //}

            public PointF GetTransformedPanXY()
            {
                return transformedPanXY;
            }

            public DistancePatternInfo AddDistancePattern(Pattern parent, Pattern distancePattern) //, Complex prevZVector)
            {
                var distancePatternInfo = new DistancePatternInfo(parent, distancePattern);
                distancePatternInfos.Add(distancePatternInfo);
                ClearCache();
                DistancePatternsCountChanged?.Invoke(this, EventArgs.Empty);
                return distancePatternInfo;
            }

            public void DeleteDistancePattern(DistancePatternInfo distancePatternInfo)
            {
                if (distancePatternInfos.Remove(distancePatternInfo))
                {
                    distancePatternInfo.DistancePattern.Dispose();
                    ClearCache();
                    DistancePatternsCountChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                    throw new Exception("distancePatternInfo not in list.");
            }

            public void ZoomDistancePatterns(double factor, bool zoomCenters = true)
            {
                float fFac = (float)factor;
                foreach (var info in distancePatternInfos)
                {
                    if (zoomCenters)
                    {
                        PointF center = info.DistancePattern.Center;
                        center.X *= fFac;
                        center.Y *= fFac;
                        info.DistancePattern.Center = center;
                    }
                    info.DistancePattern.ZVector *= factor;
                }
            }

            public void SetSeedPattern(Pattern pattern)
            {
                var seedPattern = pattern.GetCopy();
                if (seedPattern.ComputeSeedPoints())
                    SeedPattern = seedPattern;
            }

            private void ConfigureParser(ExpressionParser parser)
            {
                parser.RequireGetRandomValue = true;
                parser.DeclareInstanceFunction(this, this.GetType().GetMethod(nameof(GetPositionAverage)));
                parser.DeclareInstanceFunction(this, this.GetType().GetMethod(nameof(GetPreviousPosition)));
                parser.DeclareVariable(ParserVarNames.Info.ToString(),
                                typeof(PixelRenderInfo), Info, isGlobal: true, isReadOnly: true, isExternal: false);
                positionIdent = parser.DeclareVariable(ParserVarNames.Position.ToString(),
                                typeof(float), 0F, isGlobal: true);
                foreach (string prmName in Enum.GetNames(typeof(BooleanOutputParamNames)))
                {
                    parser.RegisterOutputParameter(prmName, OutputParameterTypes.boolean);
                }
            }

            private bool? GetBooleanOutputParamValue(BooleanOutputParamNames name)
            {
                bool? val = null;
                if (FormulaSettings != null)
                {
                    BaseParameter prm = FormulaSettings.GetParameter(name.ToString(), isOutputParameter: true);
                    if (prm != null && prm.Value is bool)
                        val = (bool)prm.Value;
                }
                return val;
            }

            public void CheckCreateFormulaSettings()
            {
                if (FormulaSettings == null)
                {
                    FormulaSettings = new FormulaSettings(FormulaTypes.PixelRender, pattern: ParentPattern);
                    ConfigureParser(FormulaSettings.Parser);
                }
            }

            public double GetPreviousPosition(int xOffset)
            {
                double position;
                if (rowPositions == null)
                    position = 0;
                else
                {
                    int rowI = (int)Info.X - xOffset;
                    if (rowI < 0)
                        position = 0;
                    else
                        position = rowPositions[rowI];
                }
                return position;
            }

            public double GetPositionAverage(int count)
            {
                if (rowPositions == null)
                    return 0;
                int rowI = (int)Info.X - 1;
                if (rowI < 0)
                    return 0;
                int startI = rowI - count;
                float startPos = startI < 0 ? 0 : rowPositions[startI];
                positionAverage += (double)(rowPositions[rowI] - startPos) / (double)count;
                return positionAverage;
            }

            public void Render(Graphics g, PointF[] points, Pattern pattern, Complex zVector, IRenderCaller caller,
                               bool enableCache = true, bool draftMode = false, bool computeRandom = false)
            {
                int newDraftSize = WhorlSettings.Instance.DraftSize;
                if (this.draftMode != draftMode || DraftSize != newDraftSize)
                {
                    ClearCache();
                    this.draftMode = draftMode;
                    DraftSize = newDraftSize;
                }
                if (FormulaSettings != null && FormulaSettings.IsCSharpFormula != isCSharpFormula)
                {
                    ClearCache();
                    isCSharpFormula = FormulaSettings.IsCSharpFormula;
                }
                influenceParentCollection = FormulaSettings?.InfluenceLinkParentCollection;
                Info.SetDraftSize(draftMode ? DraftSize : 1);
                if (cachedPositions != null && zVector != drawnZVector)
                    ClearCache();
                if (cachedPositions == null)
                {
                    CheckCreateFormulaSettings();
                    if (FormulaEnabled && FormulaSettings.HaveParsedFormula)
                    {
                        if (FormulaSettings.IsCSharpFormula)
                        {
                            evalInstance = FormulaSettings.EvalInstance;
                            if (evalInstance == null)
                                throw new Exception("EvalInstance is null.");
                            evalInstance.SetInfoObject(Info);
                            GetPosition = GetCSharpColorPosition;
                        }
                        else
                            GetPosition = GetColorPosition;
                        //FormulaSettings.InitializeGlobals();
                        pattern.InitRandomParameters(FormulaSettings.RandomParameters.ToList(), RandomRangeCount, computeRandom);
                    }
                    else
                    {
                        GetPosition = Info.DefaultGetPosition;
                    }
                }
                if (influenceParentCollection != null)
                {
                    influenceParentCollection.Initialize();
                }
                try
                {
                    Bitmap bitmap = RenderToBitmap(pattern, points, zVector, caller, enableCache);
                    if (bitmap != null)
                    {
                        using (bitmap)
                        {
                            Render(g, bitmap, pattern);
                        }
                    }
                }
                finally
                {
                    if (influenceParentCollection != null)
                    {
                        influenceParentCollection.FinalizeSettings();
                    }
                }
            }

            public void ClearCache()
            {
                cachedPositions = null;
                patternBoundsInfo = null;
            }

            private float GetCSharpColorPosition()
            {
                evalInstance.EvalFormula();
                return Info.Position;
            }

            private float GetColorPosition()
            {
                float position;
                if (FormulaSettings.EvalFormula())
                    position = (float)positionIdent.CurrentValue;
                else
                    position = 0;
                return position;
            }

            private uint[] GetBoundsBitmap(PointF[] points, Size size, bool drawCurve, out int pixelCount)
            {
                using (Bitmap bmp = BitmapTools.CreateFormattedBitmap(size))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        Pattern.FillCurvePoints(g, points, Brushes.Black, drawCurve);
                    }
                    int[] boundsPixels = new int[bmp.Width * bmp.Height];
                    BitmapTools.CopyBitmapToColorArray(bmp, boundsPixels);
                    int blackArgb = Color.Black.ToArgb();
                    return Tools.GetBitmap(boundsPixels, pix => pix == blackArgb, out pixelCount);
                }
            }

            private int InitRender(ref PointF[] points, Pattern pattern)
            {
                //distancePoints = null;
                BoundsRect = Tools.GetBoundingRectangle(points);
                //Info.BoundsSize = BoundsRect.Size;
                PointF center = pattern.Center;
                double maxPosition = ZoomFactor * Tools.Distance(center, BoundsRect.Location);
                //Info.ScaleFactor = 1.0 / Info.MaxPosition;
                points = points.Select(p => new PointF(p.X - BoundsRect.Left, p.Y - BoundsRect.Top)).ToArray();
                center = new PointF(center.X - BoundsRect.Left, center.Y - BoundsRect.Top);
                //Info.Center = new PointF(center.X - BoundsRect.Left, center.Y - BoundsRect.Top);
                maxModulus = Math.Sqrt(center.X * center.X + center.Y * center.Y);
                //Info.MaxModulusStep = (int)Math.Ceiling(maxModulus / modulusFactor);
                Info.SetInfo(
                    boundsSize: BoundsRect.Size,
                    center: center,
                    maxPosition: maxPosition,
                    scaleFactor: 1.0 / maxPosition,
                    maxModulusStep: (int)Math.Ceiling(maxModulus / modulusFactor));
                Size size = Info.GetXYBounds();
                uint[] pixelBitmap = GetBoundsBitmap(points, size, pattern.DrawCurve, out int pixelCount);
                patternBoundsInfo = new PatternBoundsInfo(size, pixelCount, pixelBitmap);
                return pixelCount;
            }

            public int GetDistancePathsCount()
            {
                int count;
                if (distancePatternInfos.Count == 0)
                    count = 1;
                else
                    count = distancePatternInfos.Count;
                return count;
            }

            private void AddPatternBoundsInfo(Pattern distPtn, int index)
            {
                var boundsRect = Tools.GetBoundingRectangle(distPtn.CurvePoints);
                var origRect = boundsRect;
                distPtn.Center = new PointF(distPtn.Center.X - boundsRect.Left, distPtn.Center.Y - boundsRect.Top);
                distPtn.ComputeCurvePoints(distPtn.ZVector, forOutline: true);
                boundsRect = Tools.GetBoundingRectangle(distPtn.CurvePoints);
                var rect = new Rectangle((int)Math.Ceiling(boundsRect.Left), (int)Math.Ceiling(boundsRect.Top),
                                         (int)Math.Ceiling(boundsRect.Width), (int)Math.Ceiling(boundsRect.Height));
                uint[] pixelBitmap = GetBoundsBitmap(distPtn.CurvePoints, rect.Size,
                                                     distPtn.DrawCurve, out int pixelCount);
                var boundsInfo = new PatternBoundsInfo(rect.Size, pixelCount, pixelBitmap);
                boundsInfo.BoundingRectangle = new Rectangle(
                    rect.Left + (int)Math.Ceiling(origRect.Left),
                    rect.Top + (int)Math.Ceiling(origRect.Top),
                    rect.Width, rect.Height);
                distPatternsBoundsInfos[index] = boundsInfo;
            }

            private void InitDistancePatternSquares(Pattern parentPattern)
            {
                bool multiple = !Info.SingleDistance;
                var pointsList = new List<PointF[]>();
                for (int i = 0; i < distancePatternInfos.Count; i++)
                {
                    var distanceInfo = distancePatternInfos[i];
                    if (!distanceInfo.DistancePatternSettings.Enabled)
                        continue;
                    using (Pattern distPtn = distanceInfo.GetDistancePattern(parentPattern))
                    {
                        if (distanceInfo.DistancePatternSettings.AutoEndValue)
                        {
                            distanceInfo.DistancePatternSettings.EndDistanceValue = 0;
                        }
                        PointF distCenter = distanceInfo.DistancePatternCenter;
                        distPtn.Center = new PointF(distCenter.X - BoundsRect.Left, distCenter.Y - BoundsRect.Top);
                        distPtn.ComputeCurvePoints(distPtn.ZVector, forOutline: true);
                        distanceInfo.MaxModulus = distPtn.ZVector.GetModulus() * distPtn.MaxPoint.Modulus;
                        if (multiple)
                            pointsList.Clear();
                        pointsList.Add(distPtn.CurvePoints);
                        if (multiple)
                            InitDistanceSquares(i, pointsList);
                        if (Info.ComputeExternal)
                        {
                            AddPatternBoundsInfo(distPtn, i);
                        }
                    }
                }
                if (!multiple)
                {
                    InitDistanceSquares(0, pointsList);
                }
            }

            private void InitDistanceSquares(Pattern parentPattern, PointF[] patternPoints)
            {
                int count = GetDistancePathsCount();
                if (Info.ComputeExternal)
                    distPatternsBoundsInfos = new PatternBoundsInfo[count];
                distanceSquaresArray = new List<DistanceSquare>[Info.SingleDistance ? 1 : count];
                if (distancePatternInfos.Count == 0)
                {
                    var pointsList = new List<PointF[]>() { patternPoints };
                    InitDistanceSquares(0, pointsList);
                    if (Info.ComputeExternal)
                        distPatternsBoundsInfos[0] = patternBoundsInfo;
                }
                else
                {
                    InitDistancePatternSquares(parentPattern);
                }
            }

            private bool IncludeSegmentPoint(PointF p, PointF topLeft, PointF bottomRight, int row, int col)
            {
                bool include = (p.X >= topLeft.X && p.Y >= topLeft.Y && p.X < bottomRight.X && p.Y < bottomRight.Y);
                if (row == 0)
                {
                    if (p.Y < topLeft.Y && p.X >= topLeft.X && p.X < bottomRight.X)
                        include = true;
                }
                else if (row == Info.DistanceRows - 1)
                {
                    if (p.Y >= bottomRight.Y && p.X >= topLeft.X && p.X < bottomRight.X)
                        include = true;
                }
                if (col == 0)
                {
                    if (p.X < topLeft.X && p.Y >= topLeft.Y && p.Y < bottomRight.Y)
                        include = true;
                }
                else if (col == Info.DistanceRows - 1)
                {
                    if (p.Y >= bottomRight.Y && p.X >= topLeft.X && p.X < bottomRight.X)
                        include = true;
                }
                return include;
            }

            private double GetSegmentLengthSquared()
            {
                double segLen;
                if (Info.SegmentLength == 0)
                    segLen = 1.0;
                else
                {
                    double diagLen = Tools.Distance(BoundsRect.Location,
                                     new PointF(BoundsRect.Right, BoundsRect.Bottom));
                    //segLen = Math.Max(1.0, Info.SegmentLength * diagLen / 1000.0);
                    segLen = 0.001 * Info.SegmentLength * diagLen;
                }
                return segLen * segLen;
            }

            private void AddSegmentPoints(List<PointF> segmentPoints, PointF[] points, double lenSq)
            {
                if (points.Length == 0)
                    return;
                PointF pLast = points[0];
                segmentPoints.Add(pLast);
                for (int i = 1; i < points.Length; i++)
                {
                    if (Tools.DistanceSquared(pLast, points[i]) >= lenSq)
                    {
                        pLast = points[i];
                        segmentPoints.Add(pLast);
                    }
                }
            }

            private void InitDistanceSquares(int index, List<PointF[]> pointsList)
            {
                List<PointF> segmentPoints = new List<PointF>();
                double lenSq = GetSegmentLengthSquared();
                foreach (PointF[] points in pointsList)
                {
                    AddSegmentPoints(segmentPoints, points, lenSq);
                }
                distanceSquareSize = new SizeF((BoundsRect.Width + 1) / Info.DistanceRows,
                                               (BoundsRect.Height + 1) / Info.DistanceRows);
                var halfSquareSize = new SizeF(distanceSquareSize.Width / 2F, distanceSquareSize.Height / 2F);
                var distanceSquares = new List<DistanceSquare>();
                for (int row = 0; row < Info.DistanceRows; row++)
                {
                    for (int col = 0; col < Info.DistanceRows; col++)
                    {
                        var topLeft = new PointF(distanceSquareSize.Width * col, distanceSquareSize.Height * row);
                        var bottomRight = new PointF(topLeft.X + distanceSquareSize.Width,
                                                     topLeft.Y + distanceSquareSize.Height);
                        var includedPoints = new List<PointF>();
                        for (int i = 0; i < segmentPoints.Count; i++)
                        {
                            PointF p = segmentPoints[i];
                            bool xInbounds = p.X >= topLeft.X && p.X < bottomRight.X;
                            bool yInbounds = p.Y >= topLeft.Y && p.Y < bottomRight.Y;
                            bool include = xInbounds && yInbounds;
                            if (row == 0)
                            {
                                if (p.Y < topLeft.Y && xInbounds)
                                    include = true;
                            }
                            else if (row == Info.DistanceRows - 1)
                            {
                                if (p.Y >= bottomRight.Y && xInbounds)
                                    include = true;
                            }
                            if (col == 0)
                            {
                                if (p.X < topLeft.X && yInbounds)
                                    include = true;
                            }
                            else if (col == Info.DistanceRows - 1)
                            {
                                if (p.X >= bottomRight.X && yInbounds)
                                    include = true;
                            }
                            if (include)
                                includedPoints.Add(p);
                        }
                        if (includedPoints.Any())
                        {
                            var center = new PointF(topLeft.X + halfSquareSize.Width,
                                                    topLeft.Y + halfSquareSize.Height);
                            distanceSquares.Add(new DistanceSquare(center, includedPoints.ToArray()));
                        }
                    }
                }
                distanceSquaresArray[index] = distanceSquares;
            }

            //private void InitDistanceSquares(int index, PointF[] points)
            //{
            //    if (points.Length == 0)
            //        return;
            //    List<PointF> segmentPoints = new List<PointF>();
            //    PointF pLast = points[0];
            //    segmentPoints.Add(pLast);
            //    double segLen;
            //    if (Info.SegmentLength == 0)
            //        segLen = 1.0;
            //    else
            //    {
            //        double diagLen = Tools.Distance(BoundsRect.Location,
            //                         new PointF(BoundsRect.Right, BoundsRect.Bottom));
            //        //segLen = Math.Max(1.0, Info.SegmentLength * diagLen / 1000.0);
            //        segLen = Info.SegmentLength * diagLen / 1000.0;
            //    }
            //    double lenSq = segLen * segLen;
            //    for (int i = 1; i < points.Length; i++)
            //    {
            //        if (Tools.DistanceSquared(pLast, points[i]) >= lenSq)
            //        {
            //            pLast = points[i];
            //            segmentPoints.Add(pLast);
            //        }
            //    }
            //    distanceSquareSize = new SizeF((BoundsRect.Width + 1) / Info.DistanceRows,
            //                                   (BoundsRect.Height + 1) / Info.DistanceRows);
            //    var halfSquareSize = new SizeF(distanceSquareSize.Width / 2F, distanceSquareSize.Height / 2F);
            //    var distanceSquares = new List<DistanceSquare>();
            //    int ind = 0;
            //    for (int row = 0; row < Info.DistanceRows; row++)
            //    {
            //        for (int col = 0; col < Info.DistanceRows; col++)
            //        {
            //            var topLeft = new PointF(distanceSquareSize.Width * col, distanceSquareSize.Height * row);
            //            var bottomRight = new PointF(topLeft.X + distanceSquareSize.Width,
            //                                         topLeft.Y + distanceSquareSize.Height);
            //            var includedPoints = segmentPoints.Where(p => IncludeSegmentPoint(p, topLeft, bottomRight, row, col));
            //            if (includedPoints.Any())
            //            {
            //                var center = new PointF(topLeft.X + halfSquareSize.Width,
            //                                        topLeft.Y + halfSquareSize.Height);
            //                distanceSquares.Add(new DistanceSquare(center, includedPoints.ToArray()));
            //            }
            //            ind++;
            //        }
            //    }
            //    distanceSquaresArray[index] = distanceSquares;
            //}

            private void SetDistancesToPaths(int x, int y)
            {
                for (int index = 0; index < distanceSquaresArray.Length; index++)
                {
                    if (distanceSquaresArray[index] != null)
                    {
                        double distance = GetDistanceToPath(index, x, y, out PointF nearestPoint);
                        Info.DistancesToPaths[index] = distance;
                        Info.NearestPoints[index] = nearestPoint;
                    }
                }
                if (Info.DistancesToPaths.Length != 0)  //Set scalar property as well as array.
                    Info.SetDistanceToPath(Info.DistancesToPaths.Average());
            }

            private double GetDistanceToPath(int index, int x, int y, out PointF nearestPoint)
            {
                var p = new PointF(x, y);
                double minDist = double.MaxValue;
                DistancePatternInfo distInfo = null;
                DistancePatternSettings settings = null;
                bool useFadeOut = false;
                bool computeDist = true;
                double modulus = 0;
                double endFade = 0;
                bool useCenterSlope = false;
                double distanceScale = 1.0;
                PointF center = PointF.Empty;

                nearestPoint = PointF.Empty;
                if (Info.ComputeExternal)
                {
                    PatternBoundsInfo ptnInfo = distPatternsBoundsInfos[index];
                    bool isOutside = true;
                    var rect = ptnInfo.BoundingRectangle;
                    if (x >= rect.Left && x < rect.Right && y >= rect.Top && y < rect.Bottom)
                    {
                        int xi = x - rect.Left;
                        int yi = y - rect.Top;
                        int pixInd = yi * ptnInfo.BoundsSize.Width + xi;
                        if (Tools.BitIsSet(ptnInfo.BoundsBitmap, pixInd))
                            isOutside = false;  //Point is inside distance pattern.
                    }
                    if (isOutside)
                        distanceScale = -1.0;
                }
                if (index < distancePatternInfos.Count)
                {
                    distInfo = distancePatternInfos[index];
                    settings = distInfo.DistancePatternSettings;
                    useFadeOut = settings.UseFadeOut;
                    useCenterSlope = settings.CenterSlope > 0;
                    center = distInfo.DistancePatternCenter;
                }
                if (useFadeOut || useCenterSlope)
                {
                    modulus = Tools.Distance(p, center);
                    if (useCenterSlope)
                    {
                        distanceScale *= Math.Tanh(settings.CenterSlope * modulus / distInfo.MaxModulus);
                    }
                    if (useFadeOut)
                    {
                        endFade = settings.FadeEndRatio * distInfo.MaxModulus;
                        if (modulus >= endFade && settings.EndDistanceValue != 0)
                        {
                            minDist = settings.EndDistanceValue;
                            computeDist = false;
                        }
                    }
                }
                if (computeDist)
                {
                    var distanceSquares = distanceSquaresArray[index];
                    foreach (DistanceSquare distSquare in distanceSquares)
                    {
                        distSquare.Distance = Tools.DistanceSquared(p, distSquare.Center);
                    }
                    int minDistI = -1;
                    foreach (DistanceSquare minSquare in distanceSquares.OrderBy(ds => ds.Distance)
                             .Take(Info.DistanceCount))
                    {
                        minDistI = -1;
                        for (int i = 0; i < minSquare.Points.Length; i++)
                        {
                            double dist = Tools.DistanceSquared(minSquare.Points[i], p);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                minDistI = i;
                            }
                        }
                        if (minDistI != -1)
                            nearestPoint = minSquare.Points[minDistI];
                    }
                    if (useFadeOut)
                    {
                        double startFade = settings.FadeStartRatio * distInfo.MaxModulus;
                        if (modulus > startFade)
                        {
                            if (settings.AutoEndValue)
                            {
                                if (minDist > settings.EndDistanceValue)
                                    settings.EndDistanceValue = minDist;
                            }
                            double factor = (modulus - startFade) / (endFade - startFade);
                            minDist += factor * (settings.EndDistanceValue - minDist);
                        }
                    }
                }
                //For testing center:
                //if (useFadeOut && modulus < 50)
                //    minDist *= 10;
                return distanceFactor * distanceScale * Math.Sqrt(minDist);
            }

            private bool ComputeAllDistances(IRenderCaller caller)
            {
                allDistanceInfo = new RaggedArrayRow<double[]>[patternBoundsInfo.BoundsSize.Height];
                int pixInd = 0;
                uint[] boundsBitmap = patternBoundsInfo.BoundsBitmap;
                for (int y = 0; y < patternBoundsInfo.BoundsSize.Height; y++)
                {
                    int minX = -1;
                    var distancesRow = new List<double[]>();
                    for (int x = 0; x < patternBoundsInfo.BoundsSize.Width; x++)
                    {
                        if (caller != null && caller.CancelRender)
                        {
                            return false;
                        }
                        if (Tools.BitIsSet(boundsBitmap, pixInd++))  //Point (X, Y) is within pattern's bounds.
                        {
                            if (minX == -1)
                                minX = x;
                            var distances = new double[distanceSquaresArray.Length];
                            for (int index = 0; index < distanceSquaresArray.Length; index++)
                            {
                                if (distanceSquaresArray[index] != null)
                                    distances[index] = GetDistanceToPath(index, x, y, out _);
                                else
                                    distances[index] = double.MaxValue;
                            }
                            distancesRow.Add(distances);
                        }
                    }
                    allDistanceInfo[y] = minX == -1 ? null : new RaggedArrayRow<double[]>(minX, distancesRow.ToArray());
                }
                return true;
            }

            private Color GetGradientColor(float position)
            {
                return ColorNodes.GetColorAtPosition(position);
            }

            private int cachedIndex;
            private int rowI;
            private bool provideFeedback, setCachedPositions, getCachedPositions;
            private double pointRotation { get; set; }
            private PointF rotationVector { get; set; }

            private void InitInfo()
            {
                Size boundsSize = patternBoundsInfo.BoundsSize;
                if (PointsRandomOps != null)
                {
                    PointsRandomOps.RandomFunction = Info.RandomFunction;
                    PointsRandomOps.UnitScalePoint = new PointF(1F / boundsSize.Width, 1F / boundsSize.Height);
                    PointsRandomOps.PanPoint = new PointF(0, 0);
                    if (PointsRandomOps.RandomPoints == null)
                        PointsRandomOps.ComputePoints();
                }
                if (Info.Normalize)
                {
                    pointRotation = Info.Rotation;
                    rotationVector = Tools.GetRotationVector(pointRotation);
                }
                else
                    pointRotation = 0;
                Point[] corners =
                {
                    new Point(0, 0),
                    new Point(boundsSize.Width, 0),
                    new Point(boundsSize.Width, boundsSize.Height),
                    new Point(0, boundsSize.Height)
                };
                var transformedCorners = corners.Select(p => TransformPoint(p.X, p.Y))
                                                .Select(pf => new PointF(pf.X - Info.Center.X, pf.Y - Info.Center.Y));
                double maxModulus = transformedCorners.Select(pf => Tools.GetModulus(pf)).Max();
                Info.SetMaxModulus(maxModulus);
                foreach (var influencePointInfo in ParentPattern.InfluencePointInfoList.InfluencePointInfos)
                {
                    PointF pt = TransformPoint((float)influencePointInfo.InfluencePoint.X, 
                                               (float)influencePointInfo.InfluencePoint.Y,
                                               subtractCenter: false);
                    influencePointInfo.TransformedPoint = new DoublePoint(pt.X, pt.Y);
                }
                if (Info.DistancePatternCenters != null)
                {
                    for (int i = 0; i < Info.DistancePatternCenters.Length; i++)
                    {
                        if (i == 0 && distancePatternInfos.Count == 0)
                        {
                            Info.DistancePatternCenters[i] = Info.Center;
                        }
                        else
                        {
                            PointF distCenter = distancePatternInfos[i].DistancePatternCenter;
                            if (Info.Normalize)
                            {
                                distCenter.X -= ParentPattern.Center.X;
                                distCenter.Y -= ParentPattern.Center.Y;
                            }
                            Info.DistancePatternCenters[i] = TransformPoint(distCenter.X, distCenter.Y, subtractCenter: false);
                        }
                    }
                }
                Info.InfluenceValue = 0;
            }

            private PointF TransformPoint(float x, float y, bool subtractCenter = true, bool pan = true)
            {
                float fX = x;
                float fY = y;
                if (pan)
                {
                    fX += transformedPanXY.X;
                    fY += transformedPanXY.Y;
                }
                PointF p;
                if (Info.Normalize)
                {
                    if (subtractCenter)
                    {
                        fX -= origCenter.X;
                        fY -= origCenter.Y;
                    }
                    p = new PointF(floatScaleFactor * fX, floatScaleFactor * fY);
                    if (pointRotation != 0)
                        p = Tools.RotatePoint(p, rotationVector);
                }
                else
                    p = new PointF(fX, fY);
                return p;
            }

            private int[] GetPatternPixels(bool setCachedPositions, bool getCachedPositions, IRenderCaller caller)
            {
                Size boundsSize = patternBoundsInfo.BoundsSize;
                this.setCachedPositions = setCachedPositions;
                this.getCachedPositions = getCachedPositions;
                int[] patternPixels = new int[boundsSize.Width * boundsSize.Height];
                if (getCachedPositions)
                    caller = null;
                if (caller != null)
                    caller.RenderCallback(patternPixels.Length, initial: true);
                cachedIndex = 0;
                InitInfo();
                if (Info.ComputeAllDistances)
                {
                    if (!ComputeAllDistances(caller))
                        return null;  //User cancelled.
                }
                if (FormulaSettings != null)
                {
                    if (!FormulaSettings.Initialize2ForEval())
                        return null;  //Exception thrown.
                }
                provideFeedback = !getCachedPositions && !Info.PolarTraversal &&
                                  GetBooleanOutputParamValue(BooleanOutputParamNames.ProvideFeedback) == true;
                if (provideFeedback)
                    rowPositions = new float[boundsSize.Width];
                else
                    rowPositions = null;
                bool success;
                Info.SetPassType(PixelRenderInfo.PassTypes.FirstPass);
                if (Info.PolarTraversal)
                    success = TraversePolar(caller, patternPixels);
                else
                    success = TraverseRectangular(caller, patternPixels);
                if (success)
                {
                    if (SmoothedDraft)
                        SmoothDraftPixels(patternPixels);
                    if (caller != null)
                        caller.RenderCallback(patternPixels.Length);
                    return patternPixels;
                }
                else
                    return null;  //User cancelled.
            }

            private void SmoothDraftPixels(int[] patternPixels)
            {
                if (!draftMode)
                    return;
                var floatColors = new ColorGradient.FloatColor[4];
                for (int i = 0; i < floatColors.Length; i++)
                {
                    floatColors[i] = new ColorGradient.FloatColor();
                }
                ColorGradient.FloatColor floatColor1, floatColor2;
                for (int y = 0; y < patternBoundsInfo.BoundsSize.Height; y += DraftSize)
                {
                    for (int x = 0; x < patternBoundsInfo.BoundsSize.Width; x += DraftSize)
                    {
                        int pixInd = y * patternBoundsInfo.BoundsSize.Width + x;
                        if (Tools.BitIsSet(patternBoundsInfo.BoundsBitmap, pixInd))  //Point (X, Y) is within pattern's bounds.
                        {
                            bool setColors = true;
                            int ind = 0;
                            for (int yi = 0; yi <= DraftSize && setColors; yi += DraftSize)
                            {
                                for (int xi = 0; xi <= DraftSize && setColors; xi += DraftSize)
                                {
                                    int pixInd1 = (y + yi) * patternBoundsInfo.BoundsSize.Width + x + xi;
                                    if (pixInd1 < patternPixels.Length &&
                                        Tools.BitIsSet(patternBoundsInfo.BoundsBitmap, pixInd1))
                                    {
                                        Color color = Color.FromArgb(patternPixels[pixInd1]);
                                        floatColors[ind++].SetFromColor(color);
                                    }
                                    else
                                        setColors = false;
                                }
                            }
                            if (setColors)
                            {
                                for (int yi = 0; yi < DraftSize; yi++)
                                {
                                    float yFac = (float)yi / DraftSize;
                                    floatColor1 = ColorGradient.FloatColor.InterpolateFloatColor(
                                        floatColors[0], floatColors[2], yFac);
                                    floatColor2 = ColorGradient.FloatColor.InterpolateFloatColor(
                                        floatColors[1], floatColors[3], yFac);
                                    for (int xi = 0; xi < DraftSize; xi++)
                                    {
                                        if (yi == 0 && xi == 0)
                                            continue;
                                        float xFac = (float)xi / DraftSize;
                                        Color color = ColorGradient.FloatColor.InterpolateColor(
                                            floatColor1, floatColor2, xFac);
                                        int pixInd1 = (y + yi) * patternBoundsInfo.BoundsSize.Width + x + xi;
                                        patternPixels[pixInd1] = color.ToArgb();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //private bool TraverseRectangularParallel(IRenderCaller caller, int[] patternPixels)
            //{
            //    int processorCount = Environment.ProcessorCount;
            //    int ySpan = (int)Math.Ceiling((double)boundsSize.Height / processorCount);
            //    bool success = true;
            //    Parallel.For(0, processorCount, (i, state) =>
            //    {
            //        int ind = i;
            //        int yStart = ind * ySpan;
            //        int yMax = Math.Min(yStart + ySpan, boundsSize.Height);
            //        int pixInd = yStart * boundsSize.Width;
            //        if (ind == 2)
            //        {
            //            if (!TraverseRectangular(null, patternPixels, yStart, yMax, pixInd, ind))
            //            {
            //                success = false;
            //                state.Break();
            //            }
            //            //state.Break();
            //        }
            //    });
            //    return success;
            //}

            private bool TraverseRectangular(IRenderCaller caller, int[] patternPixels)
            {
                Size boundsSize = patternBoundsInfo.BoundsSize;
                int pixInd = 0;
                for (int y = 0; y < boundsSize.Height; y++)
                {
                    if (caller != null && caller.CancelRender)
                    {
                        return false;
                    }
                    rowI = 0;
                    positionAverage = 0D;
                    for (int x = 0; x < boundsSize.Width; x++)
                    {
                        if (y == boundsSize.Height - 1 && x == boundsSize.Width - 1)
                        {
                            Info.SetPassType(PixelRenderInfo.PassTypes.LastPass);
                        }
                        if (RenderPoint(x, y, pixInd++, patternPixels))
                            cachedIndex++;
                    }
                    if (y % 5 == 0)
                    {
                        if (caller != null)
                            caller.RenderCallback(pixInd);
                    }
                }
                return true;
            }

            private double maxModulus;
            private const double modulusFactor = 0.5, angleFactor = 0.5;

            private bool TraversePolar(IRenderCaller caller, int[] patternPixels)
            {
                Size boundsSize = patternBoundsInfo.BoundsSize;
                Point center = new Point((int)Info.Center.X, (int)Info.Center.Y); // new Point(boundsSize.Width / 2, boundsSize.Height / 2);
                uint[] travBitmap = (uint[])patternBoundsInfo.BoundsBitmap.Clone();
                float step = 0F;
                float stepInc = (float)patternPixels.Length / (float)(maxModulus / modulusFactor);
                int modulusStep = 0;
                for (double modulus = 0.0; modulus <= maxModulus; modulus += modulusFactor)
                {
                    if (caller != null)
                    {
                        if (caller.CancelRender)
                            return false;
                        int iStep = (int)step;
                        if (iStep % 100 == 0)
                            caller.RenderCallback(iStep);
                    }
                    Info.SetModulusStep(modulusStep++);
                    Complex zVec = new Complex(modulus, 0.0);
                    double angleInc = modulus == 0 ? 0 : angleFactor * Math.Atan2(1.0, modulus);
                    Complex zRotation = Complex.CreateFromModulusAndArgument(1.0, angleInc);
                    int steps = modulus == 0.0 ? 1 : (int)Math.Ceiling(2.0 * Math.PI / angleInc);
                    List<Point> points = new List<Point>();
                    for (int i = 0; i < steps; i++)
                    {
                        int x = (int)Math.Round(zVec.Re) + center.X;
                        int y = (int)Math.Round(zVec.Im) + center.Y;
                        int pixInd = y * boundsSize.Width + x;
                        if (pixInd >= 0 && pixInd < patternPixels.Length && Tools.BitIsSet(travBitmap, pixInd))
                        {
                            points.Add(new Point(x, y));
                            Tools.ClearBit(travBitmap, pixInd);
                        }
                        zVec *= zRotation;
                    }
                    Info.SetMaxAngleStep(points.Count);
                    for (int i = 0; i < points.Count; i++)
                    {
                        Point p = points[i];
                        int pixInd = p.Y * boundsSize.Width + p.X;
                        Info.SetAngleStep(i);
                        if (i == points.Count - 1 && modulus + modulusFactor > maxModulus)
                        {
                            Info.SetPassType(PixelRenderInfo.PassTypes.LastPass);
                        }
                        if (RenderPoint(p.X, p.Y, pixInd, patternPixels))
                            cachedIndex++;
                    }
                    step += stepInc;
                }
                return true;
            }

            private bool RenderPoint(int x, int y, int pixInd, int[] patternPixels)
            {
                bool inBounds = Tools.BitIsSet(patternBoundsInfo.BoundsBitmap, pixInd);  //Point (X, Y) is within pattern's bounds.
                if (inBounds)
                {
                    float position;
                    if (draftMode)
                    {
                        int draftY = y;
                        int draftX = x;
                        int remY = draftY % DraftSize;
                        int remX = draftX % DraftSize;
                        if (remY != 0 || remX != 0)
                        {
                            //draftY -= remY;
                            //draftX -= remX;
                            int pixVal = patternPixels[(draftY - remY) * patternBoundsInfo.BoundsSize.Width + draftX - remX];
                            patternPixels[pixInd] = pixVal;
                            return inBounds;
                        }
                    }
                    if (getCachedPositions)
                    {
                        position = (float)cachedPositions[cachedIndex] / ushort.MaxValue;
                    }
                    else
                    {
                        Info.SetIntXY(new Point(x, y));
                        Info.SetXY(TransformPoint(x, y));
                        var patternPoint = new DoublePoint(Info.X, Info.Y);
                        if (Info.ComputeInfluence)
                        {
                            Info.InfluenceValue = ParentPattern.InfluencePointInfoList.ComputeAverage(patternPoint, forRendering: true);
                        }
                        if (Info.ComputeDistance)
                        {
                            SetDistancesToPaths(x, y);
                        }
                        if (influenceParentCollection != null)
                        {
                            if (influenceParentCollection.HasPixelRandom)
                            {
                                influenceParentCollection.SetPointForRandom(new PointF(x, y));
                            }
                            influenceParentCollection.SetParameterValues(patternPoint, forRendering: true);
                        }
                        position = ColorNodeList.NormalizePosition(GetPosition.Invoke());
                        if (setCachedPositions)
                        {
                            cachedPositions[cachedIndex] = (ushort)(ushort.MaxValue * position);
                        }
                        if (Info.FirstPass)
                        {
                            Info.SetPassType(PixelRenderInfo.PassTypes.Normal);
                        }
                    }
                    patternPixels[pixInd] = GetGradientColor(position).ToArgb();
                    if (provideFeedback)
                    {
                        if (draftMode)
                        {
                            int maxN = Math.Min(DraftSize, rowPositions.Length - rowI);
                            for (int n = 0; n < maxN; n++)
                            {
                                rowPositions[rowI++] = position;
                            }
                        }
                        else
                        {
                            rowPositions[rowI++] = position;
                        }
                    }
                }
                else
                {
                    if (provideFeedback && rowI < rowPositions.Length)
                    {
                        rowPositions[rowI++] = 0;
                    }
                }
                return inBounds;
            }

            private void Render(Graphics g, Bitmap bitmap, Pattern pattern)
            {
                PointF centerDiff = Tools.SubtractPoint(pattern.Center, patternCenter);
                g.DrawImage(bitmap, new PointF(BoundsRect.Location.X + centerDiff.X, BoundsRect.Location.Y + centerDiff.Y));
            }

            private Bitmap RenderToBitmap(Pattern pattern, PointF[] points, Complex zVector, IRenderCaller caller, 
                                          bool enableCache)
            {
                bool setCachedPositions = cachedPositions == null;
                bool getCachedPositions = !setCachedPositions;
                distanceSquaresArray = null;
                allDistanceInfo = null;
                //distPathPoints = null;
                //bool setProgressBar = setCachedPositions && this.MainForm != null;
                if (setCachedPositions)
                {
                    patternCenter = pattern.Center;
                    drawnZVector = zVector;
                    int cacheLength = InitRender(ref points, pattern);
                    origCenter = Info.Center;
                    Info.SetPatternAngle(zVector.GetArgument());
                    transformedPanXY = Tools.RotatePoint(PanXY, Info.PatternAngle);
                    float scaleFactor = (float)Info.MaxPosition / 1000F;
                    transformedPanXY = new PointF(transformedPanXY.X * scaleFactor, transformedPanXY.Y * scaleFactor);
                    Info.SetPointOffset(new DoublePoint(transformedPanXY.X, transformedPanXY.Y));
                    Info.PolarTraversal = Info.Normalize = Info.NormalizeAngle = false;
                    Info.Rotation = 0;
                    Info.DistanceCount = 5;
                    Info.DistanceRows = 10;
                    Info.SegmentLength = 1.0;
                    Info.SetDistanceToPath(0D);
                    Info.RandomFunction = null;
                    Info.ComputeDistance = Info.ComputeAllDistances = false;
                    floatScaleFactor = 1F;
                    if (FormulaSettings != null && FormulaEnabled && FormulaSettings.HaveParsedFormula)
                    {
                        if (!FormulaSettings.InitializeGlobals()) //Calls C# formula's Initialize() method, which can set properties of Info object.
                            return null; //Exception thrown.
                        float maxSize = Math.Max(Info.BoundsSize.Width, Info.BoundsSize.Height);
                        if (Info.Normalize)
                        {
                            Info.SetCenter(new PointF(0, 0));
                            floatScaleFactor = 1F / (ZoomFactor * maxSize);
                            Info.SetScaleFactor(1.0);
                        }
                        Info.AllocateDistancesToPaths(GetDistancePathsCount());
                        for (int i = 0; i < distancePatternInfos.Count; i++)
                        {
                            var distanceInfo = distancePatternInfos[i];
                            PointF distCenter = distanceInfo.GetDistancePatternCenter(pattern);
                            //distCenter = new PointF(distCenter.X - BoundsRect.Left, distCenter.Y - BoundsRect.Top);
                            distanceInfo.DistancePatternCenter = distCenter;
                        }
                        if (Info.ComputeDistance || Info.ComputeAllDistances)
                        {
                            InitDistanceSquares(pattern, points);
                            distanceFactor = 1D / maxSize;
                        }
                    }
                    ParentPattern.RenderingScaleFactor = 1.0 / floatScaleFactor;
                    if (enableCache)
                        cachedPositions = new ushort[cacheLength];
                    else
                        setCachedPositions = false;
                }
                int[] patternPixels = GetPatternPixels(setCachedPositions, getCachedPositions, caller);
                distanceSquaresArray = null;
                if (patternPixels == null)
                    return null;  //User canceled.
                Bitmap bmp = BitmapTools.CreateFormattedBitmap(patternBoundsInfo.BoundsSize);
                BitmapTools.CopyColorArrayToBitmap(bmp, patternPixels);
                return bmp;
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = "PixelRendering";
                XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributes(node, this, nameof(Enabled), nameof(PatternLayerIndex), 
                         nameof(FormulaEnabled), nameof(ZoomFactor),
                         nameof(UseDistanceOutline), nameof(SmoothedDraft));
                XmlNode panNode = xmlTools.CreateXmlNode("PanXY", PanXY);
                node.AppendChild(panNode);
                if (ColorNodes != null)
                    ColorNodes.ToXml(node, xmlTools, nameof(ColorNodes));
                if (SeedPattern != null)
                {
                    XmlNode seedNode = xmlTools.CreateXmlNode("SeedPattern");
                    SeedPattern.ToXml(seedNode, xmlTools);
                    node.AppendChild(seedNode);
                }
                foreach (var distancePatternInfo in distancePatternInfos)
                {
                    distancePatternInfo.ToXml(node, xmlTools);
                }
                //if (DistanceOutlinePattern != null)
                //{
                //    XmlNode childNode = xmlTools.CreateXmlNode(nameof(DistanceOutlinePattern));
                //    DistanceOutlinePattern.ToXml(childNode, xmlTools);
                //    node.AppendChild(childNode);
                //}
                //node.AppendChild(xmlTools.CreateXmlNode(nameof(DistanceOrigZVector), DistanceOrigZVector));
                //node.AppendChild(xmlTools.CreateXmlNode(nameof(DistanceOrigCenter), DistanceOrigCenter));
                if (FormulaSettings != null)
                {
                    FormulaSettings.ToXml(node, xmlTools, nameof(FormulaSettings));
                    if (FormulaSettings.InfluenceLinkParentCollection != null)
                    {
                        FormulaSettings.InfluenceLinkParentCollection.ToXml(node, xmlTools);
                    }
                }
                if (PointsRandomOps != null)
                {
                    PointsRandomOps.ToXml(node, xmlTools);
                }
                return xmlTools.AppendToParent(parentNode, node);
            }

            public void FromXml(XmlNode node)
            {
                Tools.GetXmlAttributesExcept(this, node);
                Pattern distancePattern = null;
                Complex origZVector = Complex.One;
                PointF origCenter = PointF.Empty;
                XmlNode influenceParentNode = null;
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case nameof(ColorNodes):
                            ColorNodes = new ColorNodeList();
                            ColorNodes.FromXml(childNode);
                            break;
                        case nameof(FormulaSettings):
                            FormulaSettings = new FormulaSettings(FormulaTypes.PixelRender, pattern: ParentPattern);
                            ConfigureParser(FormulaSettings.Parser);
                            FormulaSettings.FromXml(childNode);
                            isCSharpFormula = FormulaSettings.IsCSharpFormula;
                            break;
                        case "PanXY":
                            PanXY = Tools.GetPointFFromXml(childNode);
                            break;
                        case "DistancePatternInfo":
                            distancePatternInfos.Add(DistancePatternInfo.CreateFromXml(ParentPattern.Design, ParentPattern, childNode));
                            break;
                        case nameof(PointsRandomOps):
                            PointsRandomOps = new PointsRandomOps(childNode);
                            break;
                        case "SeedPattern":
                            var seedPattern = Pattern.CreatePatternFromXml(ParentPattern.Design, childNode.FirstChild,
                                              throwOnError: true);
                            if (seedPattern.ComputeSeedPoints())
                                SeedPattern = seedPattern;
                            break;
                        //Legacy code:
                        case "DistanceOutlinePattern":
                            distancePattern = Pattern.CreatePatternFromXml(ParentPattern.Design, childNode.FirstChild);
                            break;
                        case "DistanceOrigZVector":
                            origZVector = XmlTools.GetComplexFromXml(childNode);
                            break;
                        case "DistanceOrigCenter":
                            origCenter = XmlTools.GetPointFFromXml(childNode);
                            break;
                        case nameof(InfluenceLinkParentCollection):
                            influenceParentNode = childNode;
                            break;
                    }
                }
                if (distancePattern != null)
                {
                    //Legacy code:
                    distancePatternInfos.Add(DistancePatternInfo.CreateFromXml(ParentPattern.Design,
                                             distancePattern, origZVector, origCenter));
                }
                if (ColorNodes == null)
                {
                    ColorNodes = new ColorNodeList();
                    ColorNodes.AddDefaultNodes();
                }
                if (influenceParentNode != null)
                {
                    if (FormulaSettings == null)
                        throw new NullReferenceException("FormulaSettings cannot be null.");
                    FormulaSettings.InfluenceLinkParentCollection = new InfluenceLinkParentCollection(ParentPattern, FormulaSettings);
                    FormulaSettings.InfluenceLinkParentCollection.FromXml(influenceParentNode);
                }
            }

        }

        /*** Pattern class's properties: ***/

        public const int DefaultRotationSteps = 5000;
        public const int MinRotationSteps = 4;

        private int _rotationSteps = DefaultRotationSteps;
        //private static Complex initialPrevZVector = 
        //    new Complex(double.MinValue, double.MinValue);
        //private Complex prevZVector = initialPrevZVector;
        //private static readonly PointF initialPrevCenter = 
        //    new PointF(float.MinValue, float.MinValue);
        //private PointF prevCenter = initialPrevCenter;
        protected Pen outlinePen = null;

        [PropertyAction(Name = "Section")]
        public PatternSection PatternSectionInfo { get; } = new PatternSection();
        public TileInfo PatternTileInfo { get; } = new Pattern.TileInfo();
        public bool UsesSection
        {
            get { return PatternSectionInfo.IsSection; } 
        }

        public int InnerSectionIndex { get; private set; }

        public PointF[] CurvePoints { get; protected set; }
        private PointF[] LayerCurvePoints { get; set; }
        public MergeOperations MergeOperation { get; set; }
             = MergeOperations.Sum;
        [PropertyAction]
        public List<PatternTransform> Transforms { get; private set; } = new List<PatternTransform>();
        public int MaxPointIndex { get; private set; }
        public int MinPointIndex { get; private set; }
        public PolarCoord MaxPoint
        {
            get { return SeedPoints == null ? new PolarCoord(0, 0) 
                                            : SeedPoints[MaxPointIndex]; }
        }
        public bool DrawCurve { get; set; } = false;
        public RenderModes RenderMode { get; set; } = RenderModes.Paint;
        public int StainWidth { get; set; } = 10;
        public ColorBlendTypes StainBlendType { get; set; } = ColorBlendTypes.Add;

        public bool ShrinkPattern { get; set; }
        public bool ShrinkPatternLayers { get; set; }
        public float ShrinkClipCenterFactor { get; set; } = 0F;
        public float ShrinkPadding { get; set; } = 4F;
        public float ShrinkClipFactor { get; set; } = 4F;

        public float LoopFactor { get; set; } = 0F;

        public bool IsBackgroundPattern { get; set; }

        public bool AllowRandom { get; set; } = true;

        public virtual bool UseLinearGradient { get; set; }
        public float GradientPadding { get; set; } = 15F;
        public float GradientRotation { get; set; }

        //private bool seedPointsChanged;
        private DesignLayer _designLayer;

        [PropertyAction]
        public DesignLayer DesignLayer
        {
            get { return _designLayer; }
            set
            {
                if (DesignLayer == value)
                    return;
                if (DesignLayer != null)
                    DesignLayer.RemovePatternID(this.PatternID);
                _designLayer = value;
                if (DesignLayer != null)
                    DesignLayer.AddPatternID(this.PatternID);
            }
        }

        //public bool IsBackgroundPattern { get; set; }
        public float XmlVersion { get; private set; }

        public long XmlPatternID { get; private set; }
        //public int ZOrder { get; set; } = -1;

        public float SeedPointsNormalizationFactor { get; private set; }

        private FillInfo fillInfo;

        /// <summary>
        /// Object with brushes for filling pattern, and associated information.
        /// </summary>
        [PropertyAction]
        public FillInfo FillInfo
        {
            get { return fillInfo; }
            set
            {
                if (value == null)
                    throw new Exception("Cannot set FillInfo to null.");
                if (fillInfo != null)
                    fillInfo.Dispose();
                fillInfo = value;
                if (this.PatternLayers != null)
                {
                    PatternLayer firstLayer = this.PatternLayers.PatternLayers.FirstOrDefault();
                    if (firstLayer == null)
                    {
                        firstLayer = new PatternLayer(this.PatternLayers);
                        this.PatternLayers.PatternLayers.Add(firstLayer);
                    }
                    firstLayer.FillInfo = fillInfo;
                    firstLayer.SetModulusRatio(1F);
                }
            }
        }

        [PropertyAction]
        public List<BasicOutline> BasicOutlines { get; } =
           new List<BasicOutline>();

        public PolarCoord[] SeedPoints { get; protected set; }

        /// <summary>
        /// Property for Precision setting of a pattern.
        /// </summary>
        [PropertyAction(Name = "Precision", MinValue = MinRotationSteps)]
        public int RotationSteps
        {
            get { return _rotationSteps; }
            set
            {
                if (value < MinRotationSteps)
                    throw new Exception($"RotationSteps must be >= {MinRotationSteps}.");
                _rotationSteps = value;
            }
        }

        public Color CenterColor
        {
            get
            {
                PathFillInfo pathFillInfo = FillInfo as PathFillInfo;
                if (pathFillInfo != null)
                    return pathFillInfo.CenterColor;
                else
                    return Color.Empty;
            }
            set
            {
                PathFillInfo pathFillInfo = FillInfo as PathFillInfo;
                if (pathFillInfo != null)
                    pathFillInfo.CenterColor = value;
            }
        }

        public Color BoundaryColor
        {
            get
            {
                PathFillInfo pathFillInfo = FillInfo as PathFillInfo;
                if (pathFillInfo != null)
                    return pathFillInfo.BoundaryColor;
                else
                    return Color.Empty;
            }
            set
            {
                PathFillInfo pathFillInfo = FillInfo as PathFillInfo;
                if (pathFillInfo != null)
                    pathFillInfo.BoundaryColor = value;
            }
        }

        /// <summary>
        /// Offset of Center for PathGradientBrush center.
        /// </summary>
        public Complex CenterOffsetVector { get; private set; } = Complex.Zero;

        //private PointF CenterOffset { get; set; } = PointF.Empty;

        private Complex _zVector = Complex.Zero;

        public Complex ZVector
        {
            get { return _zVector; }
            set
            {
                SetZVector(value, scaleInfluencePoints: true);
            }
        }

        private Complex? origZVector { get; set; } = null;

        public void SetZVector(Complex zVector, bool scaleInfluencePoints)
        {
            Complex prevZVector = _zVector;
            _zVector = zVector;
            if (origZVector == null)
                origZVector = zVector;
            if (scaleInfluencePoints)
                ScaleInfluencePoints(prevZVector);
        }

        public Complex? GetOrigZVector()
        {
            return origZVector;
        }

        /// <summary>
        /// ZVector scaled by ZoomFactor.
        /// </summary>
        public Complex DrawnZVector
        {
            get { return ZoomFactor * ZVector; }
        }

        public double ZoomFactor { get; set; } = 1D;

        public bool FlipX { get; set; }

        public Complex OutlineZVector { get; set; }

        public bool Selected { get; set; }

        public double GetUnitFactor(List<BasicOutline> enabledOutlines)
        {
            double factor;

            if (enabledOutlines.Count == 0)
                factor = 1D;
            else
            {
                double denom = enabledOutlines.Select(otl => Math.Abs(otl.AmplitudeFactor)).Sum();
                factor = denom == 0 ? 1D : 1D / denom;
            }
            return factor;
        }

        private bool _hasRandomElements;

        public bool HasRandomElements
        {
            get { return _hasRandomElements && AllowRandom; }
            private set { _hasRandomElements = value; }
        }

        public virtual Complex PreviewZFactor { get; set; }

        public PatternLayerList PatternLayers { get; private set; }

        public RandomGenerator RandomGenerator { get; private set; }

        public int? OrigRandomSeed { get; set; } = null;

        public PatternRecursion Recursion { get; private set; }

        //public PathPattern OrigCenterPathPattern { get; set; }
        //public PathPattern CenterPathPattern { get; set; }
        //private PathPattern transformedCenterPathPattern { get; set; }
        //public Complex CenterPathZFactor { get; set; } = new Complex(1, 0);
        //public bool PreserveCenterPath { get; set; } = false;

        public PatternImproviseConfig PatternImproviseConfig { get; set; }

        public RenderingInfo PixelRendering { get; private set; }

        public bool HasPixelRendering
        {
            get { return PixelRendering != null && PixelRendering.Enabled; }
        }

        public bool UsesDistancePattern
        {
            get { return HasPixelRendering && PixelRendering.UseDistanceOutline; }
        }

        public InfluencePointInfoList InfluencePointInfoList { get; private set; }

        public double InfluenceScaleFactor { get; private set; } = 1.0;
        public double RenderingScaleFactor { get; set; } = 1.0;

        //public InfluenceLink LastEditedInfluenceLink { get; set; }
        //public InfluenceLink CopiedLastEditedInfluenceLink { get; set; }

        public WhorlDesign Design { get; private set; }

        private void Initialize(WhorlDesign design, Pattern recursiveParent)
        {
            if (design == null)
                throw new NullReferenceException("design cannot be null.");
            Design = design;
            Recursion = new PatternRecursion(recursiveParent ?? this);
            InfluencePointInfoList = new InfluencePointInfoList(this);
        }

        protected Pattern(WhorlDesign design, Pattern recursiveParent = null)
        {
            Initialize(design, recursiveParent);
        }

        public Pattern(WhorlDesign design, XmlNode node, Pattern recursiveParent = null)
        {
            Initialize(design, recursiveParent);
            FromXml(node);
        }

        public Pattern(Pattern sourcePattern, bool isRecursivePattern, Pattern recursiveParent = null,
                       bool copySharedPatternID = true, WhorlDesign design = null): base(sourcePattern)
        {
            Initialize(design ?? sourcePattern.Design, recursiveParent);
            this.CopyProperties(sourcePattern, 
                                copySharedPatternID: copySharedPatternID, 
                                setRecursiveParent: recursiveParent == null,
                                design: design);
            Recursion.IsRecursivePattern = isRecursivePattern;
        }

        public Pattern(WhorlDesign design, FillInfo.FillTypes fillType) : this(design)
        {
            InitializePattern(fillType);
        }

        protected void InitializePattern(FillInfo.FillTypes fillType)
        {
            PatternLayers = new PatternLayerList(this);
            if (fillType == FillInfo.FillTypes.Path)
                FillInfo = new PathFillInfo(this);
            else if (fillType == FillInfo.FillTypes.Texture)
                FillInfo = new TextureFillInfo(this);
            else
                FillInfo = new BackgroundFillInfo(this);
            RandomGenerator = new RandomGenerator();
        }

        public virtual Pattern GetCopy(bool keepRecursiveParent = false, WhorlDesign design = null)
        {
            return new Pattern(this, Recursion.IsRecursivePattern, 
                               keepRecursiveParent ? Recursion.ParentPattern : null, 
                               copySharedPatternID: true, design: design);
        }

        public virtual object Clone()
        {
            return GetCopy();
        }

        public bool PixelRenderingAllowed
        {
            get
            {
                if (this.GetType() != typeof(Pattern))
                {
                    var ribbon = this as Ribbon;
                    if (ribbon == null || ribbon.DrawingMode != RibbonDrawingModes.CopyPattern)
                    {
                        if (this is StringPattern)
                            return true;
                        else
                            return false;
                    }
                }
                return true;
            }
        }

        public string CheckCreatePixelRendering()
        {
            if (!PixelRenderingAllowed)
            {
                PixelRendering = null;
                return $"Pixel rendering is not supported for a {this.GetType().Name}.";
            }
            if (PixelRendering == null)
                PixelRendering = new RenderingInfo(this);
            return null;
        }

        public void ClearPixelRendering()
        {
            PixelRendering = null;
        }

        public void CopyPixelRendering(RenderingInfo source)
        {
            ClearRenderingCache();
            if (source == null)
            {
                PixelRendering = null;
            }
            else
            {
                PixelRendering = new RenderingInfo(source, this);
            }
        }

        public void ClearRenderingCache()
        {
            if (PixelRendering != null)
                PixelRendering.ClearCache();
        }

        public void SetSeedPointsChanged()
        {
            //seedPointsChanged = true;
            if (FillInfo.FillType == FillInfo.FillTypes.Path)
                FillInfo.SetFillBrushToNull();
        }

        public void SetNewRandomSeed()
        {
            if (HasRandomElements)
            {
                if (OrigRandomSeed == null)
                    OrigRandomSeed = RandomGenerator.RandomSeed;
                RandomGenerator.ReseedRandom();
            }
        }

        public void SetAllFillBrushesToNull()
        {
            foreach (PatternLayer ptnLayer in PatternLayers.PatternLayers)
            {
                ptnLayer.FillInfo.SetFillBrushToNull();
            }
        }

        public void ComputeSeedAndCurvePoints()
        {
            ComputeSeedPoints();
            ComputeCurvePoints(ZVector);
        }

        public void InitRandomParameters(IEnumerable<CustomParameter> randomRangeParams, int count, bool compute)
        {
            foreach (CustomParameter randomRangeParam in randomRangeParams)
            {
                if (randomRangeParam.CustomType != CustomParameterTypes.RandomRange)
                {
                    throw new Exception("Non random parameter passed to InitRandomParameters.");
                }
                RandomRange randomRange = (RandomRange)randomRangeParam.Context;
                //if (randomRange == null)
                //{
                //    randomRange = new RandomRange();
                //    randomRangeParam.Context = randomRange;
                //}
                if (randomRange.Count != count)
                {
                    randomRange.Allocate(count);
                    compute = true;
                }
                randomRange.RandomGenerator = this.RandomGenerator;
                if (compute)
                    randomRange.Compute();
            }
        }

        public double ComputeModulus(double angle, bool normalize = true)
        {
            if (seedOutlines == null)
                ComputeSeedPoints();
            double modulus = ComputeModulusHelper(angle);
            if (normalize)
                modulus = Math.Abs(seedUnitFactor * modulus);
            return modulus;
        }

        private double ComputeModulusHelper(double angle)
        {
            double modulus;
            switch (MergeOperation)
            {
                case MergeOperations.Sum:
                default:
                    modulus = seedOutlines.Select(otl => otl.ComputeAmplitude(angle)).Sum();
                    break;
                case MergeOperations.Max:
                    modulus = seedOutlines.Select(otl => otl.ComputeAmplitude(angle)).Max();
                    break;
                case MergeOperations.Min:
                    modulus = seedOutlines.Select(otl => otl.ComputeAmplitude(angle)).Min();
                    break;
            }
            return modulus;
        }

        public PolarPoint ComputeSeedPoint(double angle)
        {
            double angle1 = ParserTools.Normalize(angle, 2.0 * Math.PI);
            int i = Math.Max(0, Math.Min((int)(angle1 / seedAngleDelta), SeedPoints.Length - 1));
            double modulus = ComputeSeedPoint(i, ref angle1);
            return new PolarPoint(angle1, modulus);
        }

        private double ComputeSeedPoint(int i, ref double angle)
        {
            double modulus;

            foreach (CustomParameter randomRangeParam in randomRangeParams)
            {
                randomRangeParam.Value = ((RandomRange)randomRangeParam.Context).GetValue(i == -1 ? 0 : i);
            }
            if (seedCustomOutline != null)
            {
                modulus = seedCustomOutline.ComputeAmplitudeAndAngle(ref angle);
            }
            else
            {
                if (seedPolygonOutline == null)
                    modulus = 0;
                else
                {
                    modulus = seedPolygonOutline.ComputePolygonPoint(i, out angle);
                }
                modulus += ComputeModulusHelper(angle);
            }
            modulus *= seedUnitFactor;
            if (!seedPatternIsPath)
                modulus = Math.Abs(modulus);
            var polarPoint = new PolarPoint(angle + patternRotation, patternModulus * modulus);
            var doublePoint = polarPoint.ToRectangular();
            foreach (PatternTransform transform in seedTransforms)
            {
                var influenceParent = transform.TransformSettings.InfluenceLinkParentCollection;
                if (influenceParent != null)
                {
                    float randomX;
                    if (influenceParent.GetRandomValues().Any(r => r.Settings.DomainType == RandomValues.RandomDomainTypes.X))
                    {
                        var pc = new PolarCoord((float)angle, (float)modulus);
                        PointF rp = pc.ToRectangular();
                        randomX = 0.5F * (1F + rp.X) * RandomValues.RandomSettings.DefaultXLength;
                    }
                    else
                        randomX = 0;
                    foreach (RandomValues randomValues in influenceParent.GetRandomValues())
                    {
                        if (randomValues.Settings.DomainType == RandomValues.RandomDomainTypes.Angle)
                            randomValues.CurrentXValue = i;
                        else
                            randomValues.CurrentXValue = randomX;
                    }
                    influenceParent.SetParameterValues(doublePoint, forRendering: false);
                }
                if (transform.TransformSettings.InfluenceValueParameter != null)
                {
                    transform.TransformSettings.InfluenceValueParameter.SetInfluenceValue(
                              InfluencePointInfoList.ComputeAverage(doublePoint, forRendering: false));
                }
                transform.TransformPoint(ref modulus, ref angle);
            }
            return modulus;
        }

        private List<BasicOutline> seedOutlines;
        private List<PatternTransform> seedTransforms;
        private BasicOutline.CustomOutline seedCustomOutline;
        private PathOutline seedPolygonOutline;
        private List<CustomParameter> randomRangeParams;
        private bool seedPatternIsPath;
        private double seedUnitFactor;
        private double seedAngleDelta;
        private double patternModulus;
        private double patternRotation;

        public virtual bool ComputeSeedPoints(bool computeRandom = false)
        {
            const double modulusLimit = 0.9F * float.MaxValue;
            bool formulasAreValid = true;
            int pointCount = RotationSteps + 1;
            //Get list of enabled outlines:
            seedOutlines = BasicOutlines.FindAll(otl => otl.Enabled);
            patternModulus = ZVector.GetModulus();
            patternRotation = ZVector.GetArgument();
            bool retVal = seedOutlines.Count != 0;
            double maxModulus = double.MinValue;
            double minModulus = double.MaxValue;
            MaxPointIndex = MinPointIndex = 0;
            HasRandomElements = false;
            if (retVal)
            {
                //Get list of enabled transforms:
                seedTransforms = Transforms.FindAll(tr => tr.Enabled);
                seedCustomOutline = null;
                seedPolygonOutline = null;
                if (seedOutlines.Count == 1)
                {
                    BasicOutline outline1 = seedOutlines[0];
                    if (outline1.BasicOutlineType == BasicOutlineTypes.Path)
                    {
                        seedCustomOutline = outline1.customOutline;
                    }
                }
                //IEnumerable<BasicOutline.CustomOutline> customOutlines = 
                //    outlines.Where(otl => otl.customOutline != null).Select(otl => otl.customOutline);
                double rotationSpan = seedOutlines.Select(o => o.GetRotationSpan()).Max();
                seedUnitFactor = this.GetUnitFactor(seedOutlines);
                seedPatternIsPath = this is PathPattern;
                randomRangeParams = new List<CustomParameter>();
                int minIndex = -1;
                foreach (var outline in seedOutlines)
                {
                    var pathOutline = outline as PathOutline;
                    if (pathOutline != null)
                    {
                        pathOutline.AddVertices();
                    }
                    if (outline.InitComputeAmplitude(RotationSteps))
                    {
                        seedCustomOutline = null;  //SeedPoints computed from path vertices.
                        if (pathOutline != null)
                        {
                            if (pathOutline.PolygonUserVertices && seedPolygonOutline == null)
                            {
                                seedPolygonOutline = pathOutline;
                                pointCount = seedPolygonOutline.GetPolygonSteps();
                                minIndex = 0;
                            }
                            if (!pathOutline.FormulaIsValid)
                            {
                                formulasAreValid = false;
                                break;
                            }
                        }
                    }
                    else if (outline.customOutline != null)
                    {
                        if (!(outline.customOutline.AmplitudeSettings.IsValid &&
                              outline.customOutline.MaxAmplitudeSettings.IsValid))
                        {
                            formulasAreValid = false;
                            break;
                        }
                        outline.customOutline.AmplitudeSettings.InitializeGlobals();
                        outline.customOutline.MaxAmplitudeSettings.InitializeGlobals();
                        randomRangeParams.AddRange(outline.customOutline.AmplitudeSettings.RandomParameters);
                    }
                }
                if (seedPolygonOutline != null)
                    seedOutlines.Remove(seedPolygonOutline);  //Don't try to compute normally.
                //Initialize transforms:
                foreach (var transform in seedTransforms)
                {
                    if (!transform.TransformSettings.IsValid)
                    {
                        formulasAreValid = false;
                        break;
                    }
                    transform.Initialize();
                    transform.TransformSettings.InitializeGlobals();
                    randomRangeParams.AddRange(transform.TransformSettings.RandomParameters);
                }
                if (SeedPoints == null || SeedPoints.Length != pointCount)
                {
                    SeedPoints = new PolarCoord[pointCount];
                }
                if (!formulasAreValid)
                {
                    MessageBox.Show("The patterns' formulas are not all valid.");
                    retVal = false;
                }
                seedAngleDelta = (rotationSpan * 2D * Math.PI) / pointCount;
                double origAngle = -seedAngleDelta;
                InitRandomParameters(randomRangeParams, count: SeedPoints.Length, compute: computeRandom);
                HasRandomElements = randomRangeParams.Count > 0;
                int iMax = SeedPoints.Length;
                if (!seedPatternIsPath)
                    iMax--;
                for (int i = minIndex; i < iMax; i++)
                {
                    double angle = origAngle;
                    double modulus = ComputeSeedPoint(i, ref angle);
                    if (i >= 0)
                    {
                        double absModulus = Math.Abs(modulus);
                        if (absModulus > modulusLimit)
                        {
                            modulus = Math.Sign(modulus) * modulusLimit;
                            absModulus = modulusLimit;
                        }
                        if (absModulus > maxModulus)
                        {
                            MaxPointIndex = i;
                            maxModulus = absModulus;
                        }
                        if (absModulus < minModulus)
                        {
                            MinPointIndex = i;
                            minModulus = absModulus;
                        }
                        SeedPoints[i].Modulus = (float)modulus;
                        SeedPoints[i].Angle = (float)angle;
                    }
                    origAngle += seedAngleDelta;
                }
                foreach (var transform in seedTransforms)
                {
                    transform.FinalizeSettings();
                }
                if (!seedPatternIsPath)
                    SeedPoints[SeedPoints.Length - 1] = SeedPoints[0];  //Close the curve.
                SeedPointsNormalizationFactor =
                    1F / (float)(maxModulus == 0 ? 1D : Math.Abs(maxModulus));
                for (int i = 0; i < SeedPoints.Length; i++)
                    SeedPoints[i].Modulus *= SeedPointsNormalizationFactor;
                //seedPointsChanged = true;
                foreach (FormulaSettings formulaSettings in GetFormulaSettings())
                {
                    if (formulaSettings.InfluenceLinkParentCollection != null)
                    {
                        formulaSettings.InfluenceLinkParentCollection.CleanUp();
                    }
                }
                ClearRenderingCache();
            }
            double previewModulus = 1D; // / (MaxPoint.Modulus == 0 ? 1D : MaxPoint.Modulus);
            PreviewZFactor = Complex.CreateFromModulusAndArgument(
                             previewModulus, (2D * Math.PI) - MaxPoint.Angle);
            if (FillInfo.FillType == FillInfo.FillTypes.Path)
                FillInfo.SetFillBrushToNull();
            //if (ShrinkPattern)
                ApplyShrink(ShrinkPattern);
            //if (ShrinkPatternLayers)
            //{
                foreach (PatternLayer ptnLayer in PatternLayers.PatternLayers)
                {
                    ptnLayer.ApplyShrink(ShrinkPatternLayers);
                }
            //}
            return retVal;
        }

        public PathOutline GetPolygonOutline(bool allowCurve = false)
        {
            return BasicOutlines.Find(otl => Tools.IsPolygonOutline(otl, allowCurve)) as PathOutline;
        }

        public IEnumerable<PointF> GetPolygonVertices(bool allowCurve = false)
        {
            PathOutline outline = BasicOutlines.Select(otl => otl as PathOutline).FirstOrDefault(po => po != null && po.PolygonVertices != null);
            if (outline == null)
                return new PointF[] { };
            else
                return outline.PolygonVertices;
        }

        public IEnumerable<FillInfo> GetFillInfos()
        {
            if (PatternLayers != null)
            {
                return PatternLayers.PatternLayers.Select(l => l.FillInfo);
            }
            else
            {
                return new FillInfo[] { this.FillInfo };
            }
        }

        private void ApplyShrink(bool shrink = true)
        {
            ApplyPatternShrink(SeedPoints, shrink ? 0.005F * ShrinkPadding : 0F, 
                               ShrinkClipFactor, ShrinkClipCenterFactor, LoopFactor);
        }

        public static void ApplyPatternShrink(PolarCoord[] seedPoints, float padding, float clipFactor, float clipShrinkCenterFactor,
                                              float loopFactor)
        {
            if (clipFactor < 0F)
                return;
            if (padding == 0 && loopFactor == 0)
                return;
            float clipPadding = clipFactor == 0 ? float.MaxValue : 1F - clipFactor * Math.Abs(padding);
            float clipCenterModulus = clipShrinkCenterFactor * padding;
            int loopCount = (int)Math.Round(0.001F * loopFactor * seedPoints.Length);
            bool replacingFirst = false;
            var loopIndices = new List<int>();
            int replacementCount = 0;
            for (int i = 1; i <= seedPoints.Length; i++)
            {
                PolarCoord pcNew;
                PolarCoord pc = seedPoints[i - 1];
                PolarCoord pc2 = seedPoints[i % seedPoints.Length];
                if (padding == 0F)
                {
                    pcNew = pc;
                }
                else if (pc.Modulus <= clipCenterModulus)
                {
                    pcNew = new PolarCoord(pc.Angle, modulus: 0F);
                }
                else
                {
                    PointF p1 = pc.ToRectangular();
                    PointF p2 = pc2.ToRectangular();
                    float dist = (float)Tools.Distance(p1, p2);
                    PointF unitVec = new PointF((p2.X - p1.X) / dist, (p2.Y - p1.Y) / dist);
                    PointF perpVec = new PointF(padding * unitVec.Y, -padding * unitVec.X);
                    PointF pNew = new PointF(p1.X - perpVec.X, p1.Y - perpVec.Y);
                    pcNew = PolarCoord.ToPolar(pNew);
                }
                if (padding == 0F || pcNew.Modulus <= clipPadding)
                {
                    if (loopCount > 1 && i > 1)
                    {
                        PolarCoord pcPrev = seedPoints[i - 2];
                        if (pcNew.Angle < pcPrev.Angle && pc2.Angle > pc.Angle || pcNew.Angle > pcPrev.Angle && pc2.Angle < pc.Angle)
                        {
                            loopIndices.Add(i);
                        }
                    }
                    seedPoints[i - 1] = pcNew;
                    replacingFirst = false;
                }
                else if (i > 1)
                {
                    if (replacingFirst)
                        replacementCount++;
                    else
                        seedPoints[i - 1] = seedPoints[i - 2];
                }
                else
                {
                    replacingFirst = true;
                    replacementCount++;
                }
            }
            if (replacementCount > 0)
            {
                PolarCoord pc = seedPoints[seedPoints.Length - 1];
                for (int i = 0; i < replacementCount; i++)
                {
                    seedPoints[i] = pc;
                }
            }
            if (loopCount > 1)
            {
                var indCoords = new IndexedPolarCoord[seedPoints.Length];
                for (int i = 0; i < seedPoints.Length; i++)
                {
                    indCoords[i].Index = i;
                    indCoords[i].Coord = seedPoints[i];
                }
                indCoords = indCoords.OrderBy(ic => ic.Coord.Angle).ToArray();
                float angleTolerance = 10F * (float)Math.PI / seedPoints.Length;
                for (int i = 1; i < indCoords.Length; i++)
                {
                    IndexedPolarCoord ic1 = indCoords[i - 1];
                    IndexedPolarCoord ic2 = indCoords[i];
                    int indDiff = Math.Abs(ic2.Index - ic1.Index);
                    if (indDiff > 1 && indDiff <= loopCount &&
                        Tools.NumbersEqual(ic1.Coord.Angle, ic2.Coord.Angle, angleTolerance) &&
                        Tools.NumbersEqual(ic1.Coord.Modulus, ic2.Coord.Modulus, 0.01F))
                    {
                        int minInd = Math.Min(ic1.Index, ic2.Index);
                        int maxInd = Math.Max(ic1.Index, ic2.Index);
                        //Debug.Print($"minInd = {minInd}, maxInd = {maxInd}");
                        if (loopIndices.Exists(li => li >= minInd && li <= maxInd))
                        {
                            for (int j = minInd; j <= maxInd; j++)
                                seedPoints[j % seedPoints.Length] = ic1.Coord;
                        }
                    }
                }
            }
            //for (int i = 0; i < seedPoints.Length; i++)
            //{
            //    if (seedPoints[i].Modulus > clipPadding)
            //    {
            //        Debug.Print($"Modulus[{i}] greater than clipPadding: {seedPoints[i].Modulus}");
            //    }
            //}
        }

        public void ScaleInfluencePoints(Complex prevZVector)
        {
            if (prevZVector == Complex.Zero || prevZVector == ZVector || ZVector == Complex.Zero)
                return;
            InfluenceScaleFactor *= prevZVector.GetModulus() / ZVector.GetModulus();
            if (InfluencePointInfoList.Count != 0)
            {
                Complex zFactor = ZVector / prevZVector;
                InfluencePointInfoList.TransformInfluencePoints(zFactor);
            }
        }

        private struct IndexedPolarCoord
        {
            public int Index { get; set; }
            public PolarCoord Coord { get; set; }
        }

        //public static void RemoveLoops(PolarCoord[] seedPoints, float loopFactor)
        //{
        //    Debug.Print("Entering RemoveLoops()");
        //    int loopCount = (int)Math.Round(0.001F * loopFactor * seedPoints.Length);
        //    if (loopCount <= 1 || loopCount >= seedPoints.Length)
        //        return;
        //    float angleTolerance = 10F * (float)Math.PI / seedPoints.Length;
        //    var indCoords = new IndexedPolarCoord[seedPoints.Length + loopCount];
        //    int negDirCount = 0, posDirCount = 0;
        //    for (int i = 0; i < seedPoints.Length + loopCount; i++)
        //    {
        //        indCoords[i].Index = i;
        //        indCoords[i].Coord = seedPoints[i % seedPoints.Length];
        //        if (i > 0)
        //        {
        //            if (indCoords[i].Coord.Angle > indCoords[i - 1].Coord.Angle)
        //                posDirCount++;
        //            else if (indCoords[i].Coord.Angle < indCoords[i - 1].Coord.Angle)
        //                negDirCount++;
        //        }
        //    }
        //    indCoords = indCoords.OrderBy(ic => ic.Coord.Angle).ToArray();
        //    for (int i = 1; i < indCoords.Length; i++)
        //    {
        //        IndexedPolarCoord ic1 = indCoords[i - 1];
        //        IndexedPolarCoord ic2 = indCoords[i];
        //        int indDiff = Math.Abs(ic2.Index - ic1.Index);
        //        if (indDiff > 1 && indDiff <= loopCount &&
        //            Tools.NumbersEqual(ic1.Coord.Angle, ic2.Coord.Angle, angleTolerance) &&
        //            Tools.NumbersEqual(ic1.Coord.Modulus, ic2.Coord.Modulus, 0.01F))
        //        {
        //            int minInd = Math.Min(ic1.Index, ic2.Index);
        //            int maxInd = Math.Max(ic1.Index, ic2.Index);
        //            for (int j = minInd; j <= maxInd; j++)
        //                seedPoints[j % seedPoints.Length] = ic1.Coord;
        //            //Debug.Print($"minInd = {minInd}, maxInd = {maxInd}");
        //        }
        //    }
        //}

        protected void InitRibbonBrush(PointF[] distinctPoints, FillInfo fillInfo = null)
        {
            if (distinctPoints.Length <= 2)
                return;
            InitializeFillInfo(fillInfo ?? this.FillInfo, distinctPoints, checkLinearGradient: false);
        }

        public PointF GetPathGradientCenter()
        {
            Complex offsetVec = DrawnZVector * CenterOffsetVector;
            return new PointF(Center.X + (float)offsetVec.Re,
                              Center.Y + (float)offsetVec.Im);
            //return new PointF(Center.X + size * CenterOffset.X,
            //                  Center.Y + size * CenterOffset.Y);
        }

        public void SetCenterOffset(PointF pathGradientCenter)
        {
            Complex zVector = this.DrawnZVector;
            if (zVector == Complex.Zero)
                return;
            CenterOffsetVector = new Complex(pathGradientCenter.X - Center.X, pathGradientCenter.Y - Center.Y) / zVector;
            //float size = (float)this.DrawnZVector.GetModulus();
            //if (size == 0)
            //    return;
            //CenterOffset = new PointF((pathGradientCenter.X - Center.X) / size,
            //                          (pathGradientCenter.Y - Center.Y) / size);
        }

        private bool ShouldUseLinearGradient(FillInfo fillInfo)
        {
            return UseLinearGradient && fillInfo.FillType == FillInfo.FillTypes.Path;
        }

        protected LinearGradientBrush InitializeFillInfo(FillInfo fillInfo, PointF[] points, bool checkLinearGradient = true, double? patternAngle = null)
        {
            PathFillInfo pathFillInfo = fillInfo as PathFillInfo;
            if (checkLinearGradient && UseLinearGradient && pathFillInfo != null)
            {
                RectangleF bounds = Tools.GetBoundingRectangle(points);
                if (patternAngle == null)
                    patternAngle = ZVector.GetArgument();
                float angle = (float)(Tools.RadiansToDegrees((double)patternAngle + GradientRotation));
                while (angle < 0)
                    angle += 360;
                while (angle > 360)
                    angle -= 360;
                bounds.X -= GradientPadding;
                bounds.Y -= GradientPadding;
                bounds.Width += 2 * GradientPadding;
                bounds.Height += 2 * GradientPadding;
                LinearGradientBrush linearGradientBrush = 
                    new LinearGradientBrush(bounds, pathFillInfo.BoundaryColor, pathFillInfo.CenterColor, angle);
                if (pathFillInfo.ColorMode == FillInfo.PathColorModes.Radial)
                {
                    ColorBlend colorBlend = new ColorBlend();
                    colorBlend.Colors = pathFillInfo.ColorInfo.GetColors().ToArray();
                    colorBlend.Positions = pathFillInfo.ColorInfo.GetPositions().ToArray();
                    linearGradientBrush.InterpolationColors = colorBlend;
                }
                return linearGradientBrush;
            }
            if (pathFillInfo != null)
            {
                if (pathFillInfo.GraphicsPath == null)
                    pathFillInfo.GraphicsPath = new GraphicsPath();
                else
                    pathFillInfo.GraphicsPath.Reset();
                pathFillInfo.GraphicsPath.AddClosedCurve(points);
                pathFillInfo.CreateFillBrush();
                if (CenterOffsetVector != Complex.Zero)
                {
                    var pathGradBrush = pathFillInfo.FillBrush as PathGradientBrush;
                    pathGradBrush.CenterPoint = GetPathGradientCenter();
                }
            }
            else if (fillInfo.FillBrush == null)
                fillInfo.CreateFillBrush();
            return null;
        }


        protected PointF[] ComputeSectionCurvePoints(Complex zVector, bool recomputeInnerSection, PolarCoord[] seedPoints = null, bool forOutline = false)
        {
            if (seedPoints == null)
                seedPoints = SeedPoints;
            this.forOutline = forOutline;
            curvePointsList = new List<PointF>();
            AppendCurvePoints(seedPoints, this.Center, zVector, 0);
            List<PointF> pointsList = new List<PointF>(curvePointsList);
            //AppendCurvePoints(pointsList, this.Center, zVector, 0);
            InnerSectionIndex = pointsList.Count;
            if (recomputeInnerSection && PatternSectionInfo.RecomputeInnerSection 
                && this.HasRandomElements)
                ComputeSeedPoints();
            curvePointsList = new List<PointF>();
            AppendCurvePoints(seedPoints, this.Center, 
                              (double)this.PatternSectionInfo.SectionAmplitudeRatio * zVector, 
                              0);
            //List<PointF> innerPoints = new List<PointF>(curvePointsList);
            curvePointsList.Reverse();
            pointsList.AddRange(curvePointsList);
            curvePointsList = null;
            pointsList.Add(pointsList[0]);
            return pointsList.ToArray();
        }

        private List<PointF> curvePointsList;
        private PointF lastCurvePoint;
        private double maxCurveAmplitude;
        private bool forOutline;

        private PointF[] ComputeCurvePoints(PolarCoord[] seedPoints, Complex zVector, bool recomputeInnerSection = true, bool forOutline = false)
        {
            PointF[] curvePoints;
            maxCurveAmplitude = 0;
            if (zVector == Complex.Zero)
            {
                return null;
            }
            //if (CenterPathPattern != null)
            //{
            //    transformedCenterPathPattern = new PathPattern(CenterPathPattern);
            //    transformedCenterPathPattern.Center = this.Center;
            //    if (!PreserveCenterPath && origZVector != null && origZVector != Complex.Zero)
            //    {
            //        transformedCenterPathPattern.ZVector *=
            //            zVector * CenterPathZFactor / (Complex)origZVector;
            //    }
            //    transformedCenterPathPattern.ComputeCurvePoints(transformedCenterPathPattern.ZVector);
            //}
            if (this.UsesSection && this.PatternSectionInfo.SectionAmplitudeRatio > 0)
            {
                curvePoints = ComputeSectionCurvePoints(zVector, recomputeInnerSection, seedPoints);
            }
            else
            {
                curvePointsList = new List<PointF>();
                this.forOutline = forOutline;
                AppendCurvePoints(seedPoints, this.Center, zVector, 0);
                if (!(this is PathPattern))
                    curvePointsList[curvePointsList.Count - 1] = curvePointsList[0]; //Close curve.
                curvePoints = curvePointsList.ToArray();
            }
            if (FlipX && curvePoints != null && curvePoints.Length != 0)
            {
                float minX = curvePoints.Select(p => p.X).Min();
                float maxX = curvePoints.Select(p => p.X).Max();
                float xC2 = minX + maxX;  //2 * middle X
                for (int i = 0; i < curvePoints.Length; i++)
                {
                    curvePoints[i].X = xC2 - curvePoints[i].X;
                }
            }
            curvePointsList = null;
            return curvePoints;
        }

        public virtual bool ComputeCurvePoints(Complex zVector, bool recomputeInnerSection = true, bool forOutline = false)
        {
            if (SeedPoints == null)
                ComputeSeedPoints();
            if (Recursion.IsRecursive && Recursion.RecursionPatterns != null)
            {
                foreach (Pattern rPattern in Recursion.RecursionPatterns)
                {
                    if (rPattern.SeedPoints == null)
                        rPattern.ComputeSeedPoints();
                }
            }
            PointF[] curvePoints = ComputeCurvePoints(SeedPoints, zVector, recomputeInnerSection, forOutline);
            if (curvePoints != null)
                CurvePoints = curvePoints;
            return curvePoints != null;
        }

        public IEnumerable<Parameter> EnumerateParameters(ImproviseParameterType? paramType = null)
        {
            if (paramType == null || paramType == ImproviseParameterType.Outline)
            {
                foreach (var outline in BasicOutlines)
                {
                    PathOutline pathOutline = outline as PathOutline;
                    if (pathOutline == null || !pathOutline.UseVertices)
                    {
                        if (outline.customOutline != null)
                        {
                            foreach (var param in outline.customOutline.AmplitudeSettings.Parameters)
                                yield return param;
                        }
                    }
                    else
                    {
                        foreach (var param in pathOutline.VerticesSettings.Parameters)
                            yield return param;
                    }
                }
            }
            if (paramType == null || paramType == ImproviseParameterType.Transform)
            {
                foreach (var transform in Transforms)
                {
                    foreach (var param in transform.TransformSettings.Parameters)
                        yield return param;
                }
            }
        }

        private void AppendCurvePoints(PolarCoord[] seedPoints, PointF center, Complex zVector, int level)
        {
            PathPattern pathPattern = null;
            int[] vertexIndices = null;
            int vertexIndex = 0;
            int currVertexIndex = -1;
            if (level == 0)
            {
                lastCurvePoint = new PointF(float.MinValue, float.MinValue);
                pathPattern = this as PathPattern;
                if (pathPattern != null && pathPattern.CurveVertexIndices != null)
                {
                    vertexIndices = pathPattern.GetVertexIndices();
                    if (vertexIndices != null)
                    {
                        if (vertexIndices.Length == 0)
                            vertexIndices = null;
                        else
                        {
                            int maxInd = seedPoints.Length - 1;
                            vertexIndices = vertexIndices.Select(i => Math.Min(i, maxInd))
                                                         .OrderBy(i => i).ToArray();
                            currVertexIndex = vertexIndices[0];
                        }
                    }
                }
            }
            float radius = (float)zVector.GetModulus();
            int recursionIndex = -1;
            int recursionIndexIncrement = 0;
            double recursionRotation = 0;
            double recursionRotationInc = 0;
            Pattern sourcePattern = this;
            var infoList = Recursion.InfoList;
            bool isRecursive = Recursion.IsRecursive && !Recursion.DrawAsPatterns;
            if (isRecursive)
            {
                if (level > 0 && Recursion.RecursionPatterns.Count > 0)
                {
                    if (level - 1 < Recursion.RecursionPatterns.Count)
                        sourcePattern = Recursion.RecursionPatterns[level - 1];
                    else
                        sourcePattern = Recursion.RecursionPatterns.Last();
                    seedPoints = sourcePattern.SeedPoints;
                    infoList = sourcePattern.Recursion.InfoList;
                }
                if (level < Recursion.Depth)
                {
                    recursionIndexIncrement =
                        Math.Max(5, (int)Math.Ceiling(
                           (double)seedPoints.Length / sourcePattern.Recursion.Repetitions));
                    recursionIndex =
                        (int)Math.Ceiling(sourcePattern.Recursion.RotationOffsetRatio * seedPoints.Length)
                        % recursionIndexIncrement;
                    recursionRotationInc = 2D * Math.PI / sourcePattern.Recursion.Repetitions;
                }
                recursionRotation = sourcePattern.Recursion.RotationAngle;
            }
            double rotationAngle = zVector.GetArgument();
            int indexOffset = 0;
            PatternRecursionInfo recursionInfo = null;
            int infoListIndex = 0;
            int firstRecursionIndex = -1;
            if (infoListIndex < infoList.Count)
            {
                recursionInfo = infoList[infoListIndex++];
                if (recursionIndex >= 0)
                {
                    if (recursionIndex + recursionInfo.IndexOffset < 0 && infoListIndex < infoList.Count)
                    {
                        firstRecursionIndex = seedPoints.Length + recursionIndex 
                                              + recursionInfo.IndexOffset;
                        recursionInfo = infoList[infoListIndex++];
                        recursionIndex += recursionIndexIncrement;
                    }
                    indexOffset = recursionInfo.IndexOffset;
                }
            }
            for (int i = 0; i < seedPoints.Length; i++)
            {
                PolarCoord seedPoint = seedPoints[i];
                float modulus = radius * seedPoint.Modulus;
                double angle = rotationAngle + seedPoint.Angle;
                //PointF currCenter = center;
                //if (transformedCenterPathPattern == null 
                //    || transformedCenterPathPattern.CurvePoints == null)
                //    currCenter = center;
                //else
                //{
                //    int centerIndex = i * transformedCenterPathPattern.CurvePoints.Length 
                //                    / seedPoints.Length;
                //    currCenter = transformedCenterPathPattern.CurvePoints[centerIndex];
                //    if (level > 0)
                //        currCenter = new PointF(currCenter.X + center.X - Center.X,
                //                                currCenter.Y + center.Y - Center.Y);
                //}
                PointF point = ComputeCurvePoint(angle, modulus, center);
                if (i == recursionIndex + indexOffset || i == firstRecursionIndex)
                {
                    recursionIndex += recursionIndexIncrement;
                    float scale = sourcePattern.Recursion.Scale;
                    double rotation = recursionRotation;
                    if (recursionInfo != null)
                    {
                        scale *= recursionInfo.ScaleRatio;
                        rotation += recursionInfo.RotationOffset;
                    }
                    if (infoListIndex < infoList.Count)
                    {
                        recursionInfo = infoList[infoListIndex++];
                        indexOffset = recursionInfo.IndexOffset;
                    }
                    Complex rotationVec = 
                        Complex.CreateFromModulusAndArgument(1D, rotation);
                    recursionRotation += recursionRotationInc;
                    AppendCurvePoints(seedPoints, point, 
                                      (double)scale * rotationVec * zVector, 
                                      level + 1);
                }
                if (i == currVertexIndex)
                {
                    pathPattern.CurveVertexIndices[vertexIndex] = curvePointsList.Count;
                    if (vertexIndex < vertexIndices.Length - 1)
                    {
                        currVertexIndex = vertexIndices[++vertexIndex];
                    }
                }
                if (curvePointsList.Count == 0 || Tools.PointsDiffer(lastCurvePoint, point))
                {
                    curvePointsList.Add(point);
                    lastCurvePoint = point;
                    double amp = Tools.Distance(point, this.Center);
                    if (amp > maxCurveAmplitude)
                        maxCurveAmplitude = amp;
                }
            }
            if (level == 0 && vertexIndices != null)
            {
                int maxInd = curvePointsList.Count - 1;
                for (int i = 0; i < pathPattern.CurveVertexIndices.Length; i++)
                {
                    if (pathPattern.CurveVertexIndices[i] > maxInd)
                        pathPattern.CurveVertexIndices[i] = maxInd;
                }
            }
        }

        public static PointF ComputeCurvePoint(double angle, double modulus, PointF center)
        {
            return new PointF((float)(modulus * Math.Cos(angle)) + center.X,
                              (float)(modulus * Math.Sin(angle)) + center.Y);
        }

        public static void FillCurvePoints(Graphics g, PointF[] points, Brush fillBrush, bool drawCurve)
        {
            try
            {
                if (points.Length <= 3)
                    return;
                if (drawCurve)
                    g.FillClosedCurve(fillBrush, points, FillMode.Winding);
                else
                    g.FillPolygon(fillBrush, points, FillMode.Winding);
            }
            catch { /*throw;*/ }
        }

        protected void FillCurvePoints(Graphics g, PointF[] points, Brush fillBrush, 
                                     GraphicsPath graphicsPath = null)
        {
            FillCurvePoints(g, points, fillBrush, this.DrawCurve);
        }

        private void FillCurvePoints(Graphics g, PointF[] points, FillInfo fillInfo, bool checkLinearGradient)
        {
            LinearGradientBrush linearGradientBrush = InitializeFillInfo(fillInfo, this.CurvePoints, checkLinearGradient);
            if (linearGradientBrush != null)
            {
                FillCurvePoints(g, points, linearGradientBrush);
            }
            else
            {
                if (fillInfo.FillBrush == null)
                    fillInfo.CreateFillBrush();
                PathFillInfo pathFillInfo = FillInfo as PathFillInfo;
                FillCurvePoints(g, points, fillInfo.FillBrush, pathFillInfo?.GraphicsPath);
            }
        }

        protected void FillCurvePoints(Graphics g, PointF[] points, bool initBrush = false, FillInfo fillInfo = null)
        {
            if (fillInfo == null)
                fillInfo = FillInfo;
            if (fillInfo.FillBrush == null)
                fillInfo.CreateFillBrush();
            Brush fillBrush = fillInfo.FillBrush;
            GraphicsPath graphicsPath = null;
            PathFillInfo pathFillInfo = fillInfo as PathFillInfo;
            if (pathFillInfo != null)
            {
                if (initBrush)
                {
                    graphicsPath = new GraphicsPath();
                    graphicsPath.AddClosedCurve(points);
                    fillBrush = PathFillInfo.CreatePathGradientBrush(pathFillInfo);
                }
                else
                    graphicsPath = pathFillInfo.GraphicsPath;
            }
            FillCurvePoints(g, points, fillBrush, graphicsPath);
            //if (this.DrawCurve)
            //    g.FillClosedCurve(fillBrush, points);
            //else
            //    g.FillPolygon(fillBrush, points);
        }

        protected void SetOutlinePen(Color? color = null)
        {
            if (color == null)
                color = Tools.InverseColor(this.BoundaryColor);
            if (outlinePen == null)
                outlinePen = new Pen((Color)color);
            else
                outlinePen.Color = (Color)color;
        }

        public override void DrawOutline(Graphics g, Color? color = null)
        {
            if (color == null)
                color = Tools.InverseColor(this.BoundaryColor);
            if (Recursion.IsRecursive && Recursion.DrawAsPatterns)
                DrawOutlineRecursive(g, color);
            else
                DrawOutlineHelper(g, color);
        }

        private void DrawOutlineHelper(Graphics g, Color? color)
        {
            if (ComputeCurvePoints(this.OutlineZVector, recomputeInnerSection: false, forOutline: true))
            {
                if (CurvePoints.Length < 4)
                    return;
                SetOutlinePen(color);
                g.DrawClosedCurve(outlinePen, CurvePoints);
            }
        }

        public void DrawOutlineRecursive(Graphics g, Color? color, int recursiveDepth = -1)
        {
            DrawOutlineHelper(g, color);
            if (Recursion.IsRecursive && Recursion.DrawAsPatterns)
            {
                DrawRecursive(g, null, recursiveDepth, this.OutlineZVector, penColor : color);
            }
        }

        public virtual void DrawSelectionOutline(Graphics g, PointF? center = null)
        {
            if (this is PathPattern || WhorlSettings.Instance.ExactOutline)
                this.OutlineZVector = this.ZVector;
            else
                this.OutlineZVector = 0.95 * this.ZVector;
            PointF prevCenter = Center;
            try
            {
                if (center != null)
                    Center = (PointF)center;
                DrawOutline(g, Tools.InverseColor(this.BoundaryColor));
                Tools.DrawSquare(g, Tools.InverseColor(this.CenterColor), this.Center);
            }
            finally
            {
                if (center != null)
                    Center = prevCenter;
            }
        }

        public const int NonRecursiveDepth = -2;

        public virtual void DrawFilled(Graphics g,
                                       IRenderCaller caller,
                                       bool computeRandom = false,
                                       bool draftMode = false,
                                       int recursiveDepth = -1,
                                       float textureScale = 1,
                                       Complex? patternZVector = null, 
                                       bool enableCache = true)
        {
            if (RenderMode == RenderModes.Stain)
                return;
            bool draw = true;
            Complex drawnZVector = patternZVector ?? this.DrawnZVector;
            if (computeRandom && this.HasRandomElements)
                ComputeSeedPoints(computeRandom);
            if (!ComputeCurvePoints(drawnZVector))
                return;
            bool drawRecursive = Recursion.IsRecursive && Recursion.DrawAsPatterns && recursiveDepth != NonRecursiveDepth;
            if (drawRecursive && Recursion.UnderlayDrawnPatterns)
            {
                DrawRecursive(g, caller, recursiveDepth, drawnZVector, computeRandom, draftMode, 
                              textureScale: textureScale, enableCache: enableCache);
            }
            if (!draftMode)
            {
                PatternLayer minSurroundLayer = this.PatternLayers.PatternLayers.FindAll(
                    pl => pl.FillInfo is PathFillInfo && 
                        ((PathFillInfo)pl.FillInfo).ColorMode == 
                        FillInfo.PathColorModes.Surround).LastOrDefault();
                if (minSurroundLayer != null)
                {
                    DrawSurroundPattern(g, caller, drawnZVector, minSurroundLayer.ModulusRatio, enableCache, draftMode, computeRandom);
                    draw = false;
                }
            }
            if (draw)
            {
                DrawFilledWithPatternLayers(g, drawnZVector, true, caller, enableCache, draftMode, computeRandom);
            }
            if (drawRecursive && !Recursion.UnderlayDrawnPatterns)
            {
                DrawRecursive(g, caller, recursiveDepth, drawnZVector, computeRandom, draftMode,
                              textureScale: textureScale, enableCache: enableCache);
            }
        }

        private void DrawFilledWithPatternLayers(Graphics g, Complex patternZVector, bool checkLinearGradient,
                                                 IRenderCaller caller, bool enableCache, bool draftMode, bool computeRandom)
        {
            int renderIndex;
            if (PixelRendering != null && PixelRendering.Enabled)
                renderIndex = PixelRendering.PatternLayerIndex;
            else
                renderIndex = -1;
            DrawPatternLayer(g, patternZVector, FillInfo, checkLinearGradient, caller, enableCache, draftMode, computeRandom, CurvePoints, renderIndex == 0);
            //if (renderIndex == 0)
            //{
            //    PixelRendering.Render(g, CurvePoints, this, patternZVector, caller, enableCache, draftMode, computeRandom);
            //}
            //else
            //{
            //    FillCurvePoints(g, CurvePoints, this.FillInfo, checkLinearGradient);
            //}
            for (int i = 1; i < this.PatternLayers.PatternLayers.Count; i++)
            {
                PatternLayer layer = this.PatternLayers.PatternLayers[i];
                ComputeLayerCurvePoints(layer, patternZVector);
                DrawPatternLayer(g, layer.ModulusRatio * patternZVector, layer.FillInfo, checkLinearGradient, caller, enableCache, draftMode, computeRandom, 
                                 LayerCurvePoints, i == renderIndex);
                //if (i == renderIndex)
                //{
                //    PixelRendering.Render(g, LayerCurvePoints, this, layer.ModulusRatio * patternZVector, 
                //                          caller, enableCache, draftMode, computeRandom);
                //}
                //else
                //{
                //    FillCurvePoints(g, LayerCurvePoints, layer.FillInfo, checkLinearGradient);
                //}
            }
        }

        private void DrawPatternLayer(Graphics g, Complex patternZVector, FillInfo fillInfo, bool checkLinearGradient,
                                      IRenderCaller caller, bool enableCache, bool draftMode, bool computeRandom,
                                      PointF[] points, bool renderPixel)
        {
            if (renderPixel)
            {
                PixelRendering.Render(g, points, this, patternZVector, caller, enableCache, draftMode, computeRandom);
            }
            else
            {
                if (!DrawStretchFit(g, patternZVector, fillInfo, points))
                    FillCurvePoints(g, points, fillInfo, checkLinearGradient);
            }
        }

        private bool DrawStretchFit(Graphics g, Complex patternZVector, FillInfo fillInfo, PointF[] points)
        {
            var textureFillInfo = fillInfo as TextureFillInfo;
            if (textureFillInfo != null && textureFillInfo.ImageMode == TextureImageModes.StretchFit)
            {
                var rectOutline = GetSingleBasicOutline(BasicOutlineTypes.Rectangle);
                if (rectOutline != null)
                {
                    double re = Math.Abs(patternZVector.Re);
                    double im = Math.Abs(patternZVector.Im);
                    if (Math.Min(re, im) / Math.Max(re, im) < 0.01)  //Rectangle is nearly at right angles.
                    {
                        Image img = TextureFillInfo.GetTextureImage(textureFillInfo.TextureImageFileName);
                        if (img == null)
                            return false;
                        RectangleF boundingRect = Tools.GetBoundingRectangle(points);
                        PointF[] rectPoints = new PointF[] 
                        {
                            boundingRect.Location,
                            new PointF(boundingRect.Right, boundingRect.Top),
                            new PointF(boundingRect.Left, boundingRect.Bottom)
                        };
                        //Size rectSize = new Size((int)boundingRect.Size.Width, (int)boundingRect.Size.Height);
                        //img = BitmapTools.ScaleImage(img, rectSize);
                        g.DrawImage(img, rectPoints);
                        return true;
                    }
                }
            }
            return false;
        }

        public void DrawTiledPatterns(Graphics g, IRenderCaller caller, Size imgSize, bool computeRandom, 
                                      bool draftMode, float textureScale, bool enableCache = true)
        {
            if (!PatternTileInfo.TilePattern || 
                 PatternTileInfo.PatternsPerRow <= 0)
                return;
            if (SeedPoints == null)
                ComputeSeedPoints();
            Rectangle innerRect = PatternTileInfo.GetInnerRectangle(imgSize);
            SizeF rectSize = PatternTileInfo.GetSizeInfo(innerRect, out int patternsPerCol);
            Size innerSize = innerRect.Size;
            Point topLeft = innerRect.Location;
            //double patternSize = PatternTileInfo.PatternSizeRatio * Math.Min(rectWidth, rectHeight);
            //double modulus = ZVector.GetModulus();
            //ZVector = new Complex(patternSize * ZVector.Re / modulus, patternSize * ZVector.Im / modulus);
            PointF topLeftCenter = new PointF(topLeft.X + rectSize.Width / 2, topLeft.Y + rectSize.Height / 2);
            PointF center = topLeftCenter;
            for (int colInd = 0; colInd < patternsPerCol; colInd++)
            {
                for (int rowInd = 0; rowInd < PatternTileInfo.PatternsPerRow; rowInd++)
                {
                    bool drawPattern = true;
                    if (PatternTileInfo.BorderWidth > 0)
                    {
                        if (Math.Min(rowInd, PatternTileInfo.PatternsPerRow - rowInd - 1) >= PatternTileInfo.BorderWidth &&
                            Math.Min(colInd, patternsPerCol - colInd - 1) >= PatternTileInfo.BorderWidth)
                            drawPattern = false;
                    }
                    if (drawPattern)
                    {
                        this.Center = center;
                        DrawFilled(g, caller, computeRandom, draftMode, textureScale: textureScale, enableCache: enableCache);
                    }
                    center.X += rectSize.Width;
                }
                center.X = topLeftCenter.X;
                center.Y += rectSize.Height;
            }
            this.Center = topLeftCenter;
        }

        private void DrawRecursive(Graphics g, IRenderCaller caller, int recursiveDepth, Complex patternZVector, 
                                   bool computeRandom = false, bool draftMode = false, Color? penColor = null, 
                                   float textureScale = 1, bool enableCache = true)
        {
            bool drawOutline = penColor != null;
            if (recursiveDepth == -1)
            {
                recursiveDepth = Recursion.Depth;
                //this.Recursion.ParentPattern = this;
                if (!drawOutline)
                {
                    Recursion.RecursionPatternsInfo.Clear();
                    for (int i = 0; i < Recursion.Depth; i++)
                        Recursion.RecursionPatternsInfo.Add(new List<SubpatternInfo>());
                }
            }
            if (recursiveDepth <= 0)
                return;
            Pattern toplevelPattern = Recursion.ParentPattern;
            Pattern sourcePattern;
            int levelIndex = toplevelPattern.Recursion.Depth - recursiveDepth;
            List<SubpatternInfo> subpatternsInfo = null;
            if (toplevelPattern.Recursion.RecursionPatterns.Count == 0)
            {
                sourcePattern = this;
            }
            else
            {
                if (levelIndex < toplevelPattern.Recursion.RecursionPatterns.Count)
                {
                    sourcePattern = toplevelPattern.Recursion.RecursionPatterns[levelIndex];
                    subpatternsInfo = toplevelPattern.Recursion.RecursionPatternsInfo[levelIndex];
                }
                else
                {
                    sourcePattern = toplevelPattern.Recursion.RecursionPatterns.Last();
                }
            }
            bool hasRandom = sourcePattern.HasRandomElements;
            Pattern copiedPattern = new Pattern(sourcePattern, sourcePattern.Recursion.IsRecursivePattern, toplevelPattern);
            //copiedPattern.Recursion.ParentPattern = toplevelPattern;
            bool hasTextureFill;
            if (drawOutline)
                hasTextureFill = false;
            else
                hasTextureFill = copiedPattern.FillInfo is TextureFillInfo;
            if (copiedPattern.SeedPoints == null || hasRandom)
                copiedPattern.ComputeSeedPoints();
            int recursionIndexIncrement =
                Math.Max(5, (int)Math.Round((double)SeedPoints.Length / Recursion.Repetitions));
            int startRecursionIndex = MaxPointIndex % recursionIndexIncrement;
            double incRotation = 2.0 * Math.PI / Recursion.Repetitions;
            float zAngle = (float)patternZVector.GetArgument();
            float zModulus = (float)patternZVector.GetModulus();
            double scale;
            if (Recursion.UsePerspectiveScale)
            {
                int depth = 2 + Recursion.ParentPattern.Recursion.Depth - recursiveDepth;
                scale = toplevelPattern.ZVector.GetModulus() / zModulus;
                scale *= Math.Atan2(1D, (double)depth / Recursion.Scale) / (0.25 * Math.PI);
            }
            else
            {
                scale = Recursion.Scale;
            }
            recursiveDepth--;
            double rotation = Recursion.RotationAngle;
            Complex rotationVec = Complex.CreateFromModulusAndArgument(1D, rotation);
            Complex incRotationVec = Complex.CreateFromModulusAndArgument(1D, incRotation);
            Complex zVector = patternZVector * scale * rotationVec;
            List<PointF> recursionCenters = new List<PointF>();
            int recursionIndex = startRecursionIndex;
            //Compute centers for recursion patterns:
            for (int i = 0; i < Recursion.Repetitions; i++)
            {
                int ind = recursionIndex + Recursion.InfoList[i].IndexOffset;
                if (ind < 0)
                    ind += SeedPoints.Length;
                else if (ind >= SeedPoints.Length)
                    ind -= SeedPoints.Length;
                PolarCoord pc = SeedPoints[ind];
                pc.Angle += zAngle;
                pc.Modulus *= zModulus;
                if (Recursion.OffsetPatterns)
                {
                    pc.Modulus += 0.98F * (float)scale * Recursion.InfoList[i].ScaleRatio * zModulus;
                }
                pc.Modulus *= (1F + Recursion.OffsetAdjustmentFactor);
                PointF p = pc.ToRectangular();
                p = new PointF(p.X + Center.X, p.Y + Center.Y);
                recursionCenters.Add(p);
                recursionIndex += recursionIndexIncrement;
            }
            for (int i = 0; i < recursionCenters.Count; i++)
            {
                zVector *= incRotationVec;
                if (Recursion.SkipFirstDrawnPattern && i == 0)
                    continue;
                Complex zVec2 = (double)Recursion.InfoList[i].ScaleRatio * zVector;
                double rotationOffset = Recursion.InfoList[i].RotationOffset;
                if (rotationOffset != 0)
                    zVec2 *= Complex.CreateFromModulusAndArgument(1D, rotationOffset);
                copiedPattern.Center = recursionCenters[i];
                if (drawOutline)
                    copiedPattern.OutlineZVector = zVec2;
                else
                    copiedPattern.ZVector = zVec2;
                copiedPattern.SetAllFillBrushesToNull();
                if (hasTextureFill)
                {
                    DrawDesign.ApplyTextureScale(copiedPattern, textureScale);
                }
                if (drawOutline)
                    copiedPattern.DrawOutlineRecursive(g, penColor, recursiveDepth);
                else
                {
                    copiedPattern.DrawFilled(g, caller, computeRandom, draftMode, recursiveDepth,
                                             textureScale: textureScale, enableCache: enableCache);
                    if (subpatternsInfo != null)
                    {
                        var subpatternInfo = new SubpatternInfo(sourcePattern, copiedPattern, levelIndex + 1);
                        subpatternsInfo.Add(subpatternInfo);
                    }
                }
            }
            copiedPattern.Dispose();
        }

        public void RecursionAutoSample()
        {
            if (SeedPoints == null)
                ComputeSeedPoints();
            int recursionIndexIncrement =
                Math.Max(5, (int)Math.Round((double)SeedPoints.Length / Recursion.Repetitions));
            int startRecursionIndex = MaxPointIndex % recursionIndexIncrement;
            int indexSpan = Math.Min(SeedPoints.Length, (int)(0.001F * Recursion.AutoSampleFactor * SeedPoints.Length));
            int recursionIndex = startRecursionIndex;
            for (int i = 0; i < Recursion.Repetitions; i++)
            {
                int ind = recursionIndex + Recursion.InfoList[i].IndexOffset;
                if (ind < 0)
                    ind += SeedPoints.Length;
                else if (ind >= SeedPoints.Length)
                    ind -= SeedPoints.Length;
                PolarCoord pc = SeedPoints[ind];
                int maxOffset = 0;
                for (int offset = -indexSpan; offset <= indexSpan; offset++)
                {
                    int i2 = ind + offset;
                    if (i2 < 0)
                        i2 += SeedPoints.Length;
                    else if (i2 >= SeedPoints.Length)
                        i2 -= SeedPoints.Length;
                    if (SeedPoints[i2].Modulus > pc.Modulus)
                    {
                        maxOffset = offset;
                        pc = SeedPoints[i2];
                    }
                }
                Recursion.InfoList[i].IndexOffset += maxOffset;
                recursionIndex += recursionIndexIncrement;
            }
        }


        private void DrawSurroundPattern(Graphics g, IRenderCaller caller, Complex drawnZVector, double sizeRatio, 
                                         bool enableCache, bool draftMode, bool computeRandom)
        {
            double patternSize = drawnZVector.GetModulus();
            if (Recursion.IsRecursive && !Recursion.DrawAsPatterns)
            {
                patternSize *= Math.Pow(1D + this.Recursion.Scale, Recursion.Depth);
            }
            double layerSize = sizeRatio * patternSize;
            double minSize = WhorlSettings.Instance.MinPatternSize;
            if (layerSize < minSize)
            {
                double ratio = minSize / layerSize;
                int drawnSize = (int)(2D * patternSize);
                int bitmapSize = (int)(1.01 * ratio * drawnSize);
                Bitmap bitmap = new Bitmap(bitmapSize, bitmapSize);
                PointF saveCenter = Center;
                Center = new PointF(bitmapSize / 2, bitmapSize / 2);
                ComputeCurvePoints(drawnZVector * ratio, recomputeInnerSection: false);
                using (Graphics gb = Graphics.FromImage(bitmap))
                {
                    DrawFilledWithPatternLayers(gb, drawnZVector, false, caller, enableCache, draftMode, computeRandom);
                }
                Center = saveCenter;
                RectangleF rect =
                    new RectangleF(Center.X - drawnSize / 2,
                                   Center.Y - drawnSize / 2,
                                   drawnSize, drawnSize);
                g.DrawImage(bitmap, rect);
            }
        }

        private void ComputeLayerCurvePoints(PatternLayer layer, Complex patternZVector)
        {
            if (ShrinkPatternLayers)
            {
                LayerCurvePoints = ComputeCurvePoints(layer.SeedPoints ?? SeedPoints, patternZVector);
            }
            else
            {
                if (LayerCurvePoints == null || LayerCurvePoints.Length != CurvePoints.Length)
                    LayerCurvePoints = new PointF[CurvePoints.Length];
                float ratio = layer.ModulusRatio;
                for (int i = 0; i < CurvePoints.Length; i++)
                {
                    LayerCurvePoints[i].X = Center.X + ratio * (CurvePoints[i].X - Center.X);
                    LayerCurvePoints[i].Y = Center.Y + ratio * (CurvePoints[i].Y - Center.Y);
                }
            }
        }

        public static bool RenderStained(List<Pattern> drawnPatterns, Bitmap bitmap,
                                         int squareSize = 1)
        {
            IEnumerable<Pattern> stainPatterns = drawnPatterns.Where(
                ptn => ptn.RenderMode == RenderModes.Stain);
            if (stainPatterns.Count() == 0)
                return false;
            foreach (Pattern pattern in stainPatterns)
            {
                if (pattern.SeedPoints == null)
                    pattern.ComputeSeedPoints();
            }
            var brush = new SolidBrush(Color.White);
            //progressBar.Maximum = stainPatterns.Count() * bitmap.Height / squareSize;
            //progressBar.Value = 0;

            IntColor[] pixels = null;
            int[] colorArray = null;
            int pixInd = 0;
            if (squareSize == 1)
            {
                pixels = new IntColor[bitmap.Width * bitmap.Height];
                colorArray = new int[pixels.Length];
                BitmapTools.CopyBitmapToColorArray(bitmap, colorArray);
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new IntColor(Color.FromArgb(colorArray[i]));
            }
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                foreach (Pattern pattern in stainPatterns)
                {
                    List<PolarCoord> sortedCoords =
                        pattern.SeedPoints.OrderBy(p => p.Angle).ToList();
                    double maxMod = pattern.SeedPoints.Select(p => p.Modulus).Max();
                    if (maxMod == 0)
                        continue;
                    double amplitude = pattern.ZVector.GetModulus();
                    double patternAngle = pattern.ZVector.GetArgument();
                    double factor = 0.02 * amplitude * pattern.StainWidth / maxMod;
                    Color stainColor = pattern.BoundaryColor;
                    ColorBlendTypes blendType = pattern.StainBlendType;
                    pixInd = 0;
                    for (int y = 0; y < bitmap.Height; y += squareSize)
                    {
                        int coordInd = -1;
                        PolarCoord currCoord = new PolarCoord(0, 0);
                        //progressBar.Increment(1);
                        for (int x = 0; x < bitmap.Width; x += squareSize)
                        {
                            Complex zVec = new Complex(
                                (double)x - pattern.Center.X, (double)y - pattern.Center.Y);
                            double distance = zVec.GetModulus();
                            double angle = 0;
                            if (distance > 0)
                                angle = Tools.NormalizeAngle(zVec.GetArgument() - patternAngle);
                            if (coordInd == -1)
                            {
                                coordInd = sortedCoords.FindIndex(p => p.Angle >= angle);
                                if (coordInd < 0)
                                    coordInd = sortedCoords.Count - 1;
                            }
                            else
                            {
                                if (angle < currCoord.Angle)
                                {
                                    while (coordInd > 0 &&
                                           angle <= sortedCoords[coordInd - 1].Angle)
                                        coordInd--;
                                }
                                else if (angle > currCoord.Angle)
                                {
                                    while (coordInd < sortedCoords.Count - 1 &&
                                           angle > sortedCoords[coordInd].Angle)
                                        coordInd++;
                                }
                            }
                            currCoord = sortedCoords[coordInd];
                            float strength =
                                (float)Math.Tanh(factor * currCoord.Modulus /
                                                 Math.Max(distance, 1D));
                            if (strength >= 1F / 255F)
                            {
                                IntColor pixColor;
                                if (squareSize == 1)
                                    pixColor = pixels[pixInd];
                                else
                                    pixColor = new IntColor(bitmap.GetPixel(x, y));
                                //Color pixColor = bitmap.GetPixel(x, y);
                                IntColor newColor = new IntColor(pixColor.A,
                        GetStainColorValue(pixColor.R, stainColor.R, strength, blendType),
                        GetStainColorValue(pixColor.G, stainColor.G, strength, blendType),
                        GetStainColorValue(pixColor.B, stainColor.B, strength, blendType));
                                if (squareSize == 1)
                                {
                                    pixels[pixInd] = newColor;
                                    //bitmap.SetPixel(x, y, newColor);
                                }
                                else
                                {
                                    brush.Color = newColor.GetColor();
                                    g.FillRectangle(brush, x, y, squareSize, squareSize);
                                }
                            }
                            pixInd++;
                        }
                    }
                }
            }
            brush.Dispose();
            if (squareSize == 1)
            {
                for (int i = 0; i < pixels.Length; i++)
                    colorArray[i] = pixels[i].GetColor().ToArgb();
                BitmapTools.CopyColorArrayToBitmap(bitmap, colorArray);
                //pixInd = 0;
                //for (int x = 0; x < bitmap.Width; x++)
                //{
                //    for (int y = 0; y < bitmap.Height; y++)
                //        bitmap.SetPixel(x, y, pixels[pixInd++].GetColor());
                //}
            }
            return true;
        }

        private static int GetStainColorValue(int pixelColorValue, int stainColorValue, 
                                              float strength, ColorBlendTypes blendType)
        {
            float colorVal;
            switch (blendType)
            {
                case ColorBlendTypes.None:
                    colorVal = pixelColorValue;
                    break;
                case ColorBlendTypes.Add:
                case ColorBlendTypes.Average:
                    colorVal = strength * stainColorValue + pixelColorValue;
                    if (blendType == ColorBlendTypes.Average)
                        colorVal /= 1F + strength;
                    break;
                case ColorBlendTypes.Subtract:
                //case ColorBlendTypes.Difference:
                default:
                    colorVal = pixelColorValue - strength * stainColorValue;
                    //if (blendType == ColorBlendTypes.Difference)
                    //    colorVal = -colorVal;
                    break;
            }
            return (int)Math.Round(colorVal);
        }

        public void CopySeedPoints(Pattern sourcePattern)
        {
            if (sourcePattern.SeedPoints != null)
                this.SeedPoints = (PolarCoord[])sourcePattern.SeedPoints.Clone();
            for (int i = 1; i < Math.Min(PatternLayers.PatternLayers.Count, sourcePattern.PatternLayers.PatternLayers.Count); i++)
            {
                PatternLayer ptnLayer = sourcePattern.PatternLayers.PatternLayers[i];
                if (ptnLayer.SeedPoints != null)
                    PatternLayers.PatternLayers[i].SeedPoints = (PolarCoord[])ptnLayer.SeedPoints.Clone();
            }
        }

        public Color GetPreviewBackColor()
        {
            Pattern targetPattern;
            if (this is PathPattern)
                targetPattern = ((PathPattern)this).PathRibbon;
            else
                targetPattern = this;
            Color color;
            var textureFillInfo = targetPattern?.FillInfo as TextureFillInfo;
            if (textureFillInfo != null)
                color = textureFillInfo.SampleColor;
            else
                color = this.BoundaryColor;
            return Tools.ColorIsLight(color) ? Color.Black : Color.White;
        }

        public void NormalizeAmplitudeFactors()
        {
            if (BasicOutlines.Count == 0)
                return;
            double maxAmp = (from otl in this.BasicOutlines select otl.AmplitudeFactor).Max();
            if (maxAmp != 0)
            {
                double fac = 1D / maxAmp;
                foreach (BasicOutline otl in this.BasicOutlines)
                {
                    otl.AmplitudeFactor = Math.Round(fac * otl.AmplitudeFactor, 5);
                }
            }
        }

        public virtual void SetForPreview(Size picSize)
        {
            this.Center = new PointF(picSize.Width / 2, picSize.Height / 2);
            double radius = 0.45 * (double)picSize.Width;
            this.ZVector = (radius / PatternList.GetPreviewFactor(this)) * this.PreviewZFactor;
            this.ZoomFactor = 1D;
        }

        public void SetForPreview(double radius)
        {
            if (Recursion.IsRecursive) // || CenterPathPattern != null)
            {
                ComputeCurvePoints(ZVector);
                if (maxCurveAmplitude != 0)
                    this.ZVector *= (radius / maxCurveAmplitude);
                //this.ZVector /= (maxCurveAmplitude / radius * SeedPointsNormalizationFactor);
            }
        }

        public List<double> GetVertexAngles()
        {
            PathOutline userVerticesOutline =
                BasicOutlines.Find(otl => otl is PathOutline
                                   && ((PathOutline)otl).UserDefinedVertices) as PathOutline;
            if (userVerticesOutline == null || userVerticesOutline.PathVertices == null)
                return null;
            List<double> vertexAngles = new List<double>();
            foreach (PointF vertex in userVerticesOutline.PathVertices)
            {
                double angle = Math.Atan2(vertex.Y, vertex.X);
                if (angle < 0)
                    angle += 2 * Math.PI;
                vertexAngles.Add(angle);
            }
            vertexAngles.Sort();
            return vertexAngles;
        }

        public void SetVertexAnglesParameters()
        {
            if (Transforms.Count == 0)
                return;
            List<double> vertexAngles = null;
            foreach (PatternTransform transform in Transforms)
            {
                ArrayParameter arrayParam =
                    transform.TransformSettings.GetParameter(PatternTransform.VertexAnglesParameterName) as ArrayParameter;
                if (arrayParam != null)
                {
                    if (vertexAngles == null)
                    {
                        vertexAngles = GetVertexAngles();
                        if (vertexAngles == null)
                            return;
                    }
                    transform.SetVertexAnglesParameter(arrayParam, vertexAngles);
                }
            }
        }

        public virtual Ribbon GetRibbon()
        {
            return null;
        }

        public BasicOutline GetSingleBasicOutline(BasicOutlineTypes basicOutlineType)
        {
            BasicOutline basicOutline;
            if (BasicOutlines.Count == 1 && BasicOutlines[0].BasicOutlineType == basicOutlineType)
                basicOutline = BasicOutlines[0];
            else
                basicOutline = null;
            return basicOutline;
        }

        //protected override void OnCenterChanged()
        //{
        //    base.OnCenterChanged();
        //    if (CenterPathPattern != null)
        //        CenterPathPattern.Center = this.Center;
        //}

        private static readonly HashSet<string> excludedCopyProperties = new HashSet<string>()
        { 
            nameof(FillInfo), nameof(CurvePoints), nameof(SeedPoints),
            nameof(Transforms), nameof(Recursion), nameof(MaxPoint), nameof(Design),
            nameof(BasicOutlines), nameof(DesignLayer), //nameof(PrevCenter),
            nameof(RandomGenerator), nameof(PatternLayers), nameof(KeyGuid),
            nameof(CenterOffsetVector), // nameof(CenterPathPattern),
            nameof(PatternImproviseConfig), "FormulaSettings", nameof(InfluencePointInfoList)
        };

        protected virtual void CopyProperties(Pattern sourcePattern, 
                                      bool copyFillInfo = true,
                                      bool copySharedPatternID = true,
                                      bool copySeedPoints = true,
                                      bool setRecursiveParent = true,
                                      WhorlDesign design = null)
        {
            Design = design ?? sourcePattern.Design;
            Tools.CopyProperties(this, sourcePattern, excludedPropertyNames: excludedCopyProperties);
            InfluencePointInfoList = new InfluencePointInfoList(sourcePattern.InfluencePointInfoList, this);
            if (copySharedPatternID)
                SetKeyGuid(sourcePattern);
            //this.PrevCenter = sourcePattern.PrevCenter;
            if (copyFillInfo)
                this.FillInfo = sourcePattern.FillInfo.GetCopy(this);
            Tools.CopyProperties(this.PatternSectionInfo, sourcePattern.PatternSectionInfo);
            PatternTileInfo.CopyProperties(sourcePattern.PatternTileInfo);
            if (sourcePattern.PixelRendering != null)
            {
                PixelRendering = new RenderingInfo(sourcePattern.PixelRendering, this);
            }
            this.Recursion.CopyProperties(sourcePattern.Recursion, parentPattern: (setRecursiveParent ? this : null));
            this.CenterOffsetVector = sourcePattern.CenterOffsetVector;
            //this.CenterPathPattern = sourcePattern.CenterPathPattern;
            if (sourcePattern.PatternImproviseConfig != null)
            {
                this.PatternImproviseConfig =
                    new PatternImproviseConfig(sourcePattern.PatternImproviseConfig);
            }
            //if (sourcePattern.CenterPathPattern != null)
            //{
            //    this.CenterPathPattern = new PathPattern(sourcePattern.CenterPathPattern);
            //    this.CenterPathPattern.ComputeCurvePoints(CenterPathPattern.ZVector);
            //}
            //else
            //    this.CenterPathPattern = null;
            //this.OrigCenterPathPattern = sourcePattern.OrigCenterPathPattern;
            this.origZVector = sourcePattern.GetOrigZVector();
            this.PatternLayers = sourcePattern.PatternLayers.GetCopy(this);
            this.RandomGenerator = new RandomGenerator(
                    sourcePattern.RandomGenerator.RandomSeed);
            this.MaxPointIndex = sourcePattern.MaxPointIndex;
            this.BasicOutlines.Clear();
            BasicOutlines.AddRange(sourcePattern.BasicOutlines.Select(otl => (BasicOutline)otl.Clone()));
            this.Transforms = new List<PatternTransform>(sourcePattern.Transforms.Select(t => new PatternTransform(t, this)));
            var sourceParams = sourcePattern.EnumerateParameters().ToList();
            var copiedParams = this.EnumerateParameters().ToList();
            int maxI = Math.Min(sourceParams.Count, copiedParams.Count);
            for (int i = 0; i < maxI; i++)
            {
                copiedParams[i].Guid = copySharedPatternID ? sourceParams[i].Guid : Guid.NewGuid();
            }
            if (copySeedPoints)
            {
                this.CopySeedPoints(sourcePattern);
                this.HasRandomElements = sourcePattern.HasRandomElements;
            }
            InfluencePointInfoList.CopyKeyParams(sourcePattern.InfluencePointInfoList, this);
        }

        public void SortTransforms()
        {
            Transforms = Transforms.OrderBy(t => t.SequenceNumber).ToList();
        }

        public void CopyFillInfo(Pattern sourcePattern)
        {
            PathPattern pathPattern = this as PathPattern;
            Pattern targetPattern = this;
            if (pathPattern != null && pathPattern.PathRibbon != null)
            {
                targetPattern = pathPattern.PathRibbon;
            }
            targetPattern.ShrinkPatternLayers = sourcePattern.ShrinkPatternLayers;
            targetPattern.PatternLayers.PatternLayers.Clear();
            foreach (var ptnLayer in sourcePattern.PatternLayers.PatternLayers)
            {
                targetPattern.PatternLayers.PatternLayers.Add(ptnLayer.GetCopy(targetPattern.PatternLayers, targetPattern));
            }
            //targetPattern.PatternLayers.PatternLayers.AddRange(sourcePattern.PatternLayers.PatternLayers.Select(
            //                                                   pl => pl.GetCopy(targetPattern.PatternLayers, targetPattern)));
            PatternLayer firstLayer = targetPattern.PatternLayers.PatternLayers.FirstOrDefault();
            if (firstLayer != null)
                targetPattern.FillInfo = firstLayer.FillInfo;
            //targetPattern.FillInfo = sourcePattern.FillInfo.GetCopy(targetPattern);
        }

        protected virtual void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "Pattern";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            Tools.SetXmlVersion(node, xmlTools);
            xmlTools.AppendXmlAttributes(node, this, nameof(RotationSteps), nameof(MergeOperation), nameof(DrawCurve),
                                         nameof(RenderMode), nameof(StainBlendType), nameof(StainWidth), 
                                         nameof(ZoomFactor), nameof(LoopFactor), nameof(KeyGuid), nameof(FlipX),
                                         nameof(ShrinkPattern), nameof(ShrinkPatternLayers), nameof(ShrinkClipCenterFactor),
                                         nameof(ShrinkPadding), nameof(ShrinkClipFactor),
                                         nameof(IsBackgroundPattern), nameof(AllowRandom), nameof(InfluenceScaleFactor),
                                         nameof(UseLinearGradient), nameof(GradientPadding), nameof(GradientRotation));
            xmlTools.AppendXmlAttribute(node, "RandomSeed", OrigRandomSeed ?? RandomGenerator.RandomSeed);
            xmlTools.AppendXmlAttribute(node, "XmlPatternID", PatternID);
            FillInfo.ToXml(node, xmlTools);
            if (PatternLayers != null && PatternLayers.PatternLayers.Count > 0)
                PatternLayers.ToXml(node, xmlTools);
            //sb.AppendLine(Tools.GetXml(CenterColor, "CenterColor"));
            //sb.AppendLine(Tools.GetXml(BoundaryColor, "BoundaryColor"));
            node.AppendChild(xmlTools.CreateXmlNode(nameof(Center), Center));
            if (CenterOffsetVector != Complex.Zero)
                node.AppendChild(xmlTools.CreateXmlNode(nameof(CenterOffsetVector), CenterOffsetVector));
            node.AppendChild(xmlTools.CreateXmlNode(nameof(ZVector), ZVector));
            node.AppendChild(xmlTools.CreateXmlNode(nameof(PreviewZFactor), PreviewZFactor));
            ExtraXml(node, xmlTools);
            foreach (var otl in BasicOutlines)
            {
                otl.ToXml(node, xmlTools);
            }
            foreach (var trn in Transforms)
            {
                trn.ToXml(node, xmlTools);
            }
            if (PatternSectionInfo.IsSection)
            {
                PatternSectionInfo.ToXml(node, xmlTools);
            }
            if (PatternTileInfo.TilePattern)
            {
                PatternTileInfo.ToXml(node, xmlTools);
            }
            if (Recursion.IsRecursive)
            {
                Recursion.ToXml(node, xmlTools);
            }
            if (PixelRendering != null)
            {
                PixelRendering.ToXml(node, xmlTools, "PixelRendering");
            }
            //if (CenterPathPattern != null)
            //{
            //    CenterPathPattern.Center = this.Center;
            //    CenterPathPattern.ToXml(node, xmlTools, nameof(CenterPathPattern));
            //    if (origZVector != null)
            //        node.AppendChild(xmlTools.CreateXmlNode(nameof(origZVector), (Complex)origZVector));
            //    node.AppendChild(xmlTools.CreateXmlNode(nameof(CenterPathZFactor), CenterPathZFactor));
            //}
            if (this.PatternImproviseConfig != null)
            {
                PatternImproviseConfig.ToXml(node, xmlTools);
            }
            if (InfluencePointInfoList.Count != 0)
            {
                InfluencePointInfoList.ToXml(node, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public static Pattern CreatePatternFromXml(WhorlDesign design, XmlNode node, bool throwOnError = false)
        {
            switch (node.Name)
            {
                case nameof(Ribbon):
                    return new Ribbon(design, node);
                case nameof(Pattern):
                    return new Pattern(design, node);
                case nameof(PathPattern):
                    return new PathPattern(design, node);
                case nameof(StringPattern):
                    return new StringPattern(design, node);
                default:
                    if (throwOnError)
                    {
                        throw new Exception($"Invalid pattern class in XML: {node.Name}");
                    }
                    return null;
            }
        }

        //public static Pattern CreateFromXml(XmlNode node)
        //{
        //    Pattern pattern = new Pattern();
        //    pattern.FromXml(node);
        //    return pattern;
        //}

        protected virtual bool FromExtraXml(XmlNode node)
        {
            return false;
        }

        public override void FromXml(XmlNode node)
        {
            float defaultVersion = Design.XmlVersion == 0F ? 1.1F : Design.XmlVersion;
            XmlVersion = Tools.GetXmlVersion(node, defaultVersion);

            Tools.GetXmlAttributesExcept(this, node, excludedPropertyNames: new string[]
                                   { "MergeOperation", "RenderMode", "Selected",
                                     "DrawingMode", "PathMode", "StainBlendType", "SharedPatternID",
                                     "RandomSeed", "TransformCenterPath", "ClipShrinkCenter",
                                      nameof(XmlPatternID), nameof(KeyGuid) });
            this.MergeOperation = Tools.GetEnumXmlAttr(
                                    node, nameof(MergeOperation), MergeOperations.Sum);
            this.RenderMode = Tools.GetEnumXmlAttr(
                                    node, nameof(RenderMode), RenderModes.Paint);
            this.StainBlendType = Tools.GetEnumXmlAttr(
                                    node, nameof(StainBlendType), ColorBlendTypes.Add);
            int? randomSeed = (int?)Tools.GetXmlAttribute("RandomSeed", typeof(int),
                                                          node, required: false);
            long? patternId = (long?)Tools.GetXmlAttribute(nameof(XmlPatternID), typeof(long),
                                                           node, required: false);
            if (patternId != null)
                this.XmlPatternID = (long)patternId;
            //patternId = (long?)Tools.GetXmlAttribute("SharedPatternID", typeof(long),
            //                                         node, required: false);
            //if (patternId != null)
            //    this.SharedPatternID = (long)patternId;
            XmlAttribute guidAttr = node.Attributes[nameof(KeyGuid)];
            if (guidAttr != null)
                SetKeyGuid(Guid.Parse(guidAttr.Value));
            this.RandomGenerator = new RandomGenerator(randomSeed);
            BasicOutlines.Clear();
            bool haveUserVertices = false;
            FillInfo fillInfo = null;
            PointF? centerOffset = null;
            XmlNode textureFillNode = null;
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case nameof(BasicOutline):
                        BasicOutline outline = new BasicOutline();
                        outline.FromXml(childNode);
                        this.BasicOutlines.Add(outline);
                        break;
                    case nameof(PatternTransform):
                        PatternTransform transform = new PatternTransform(this);
                        transform.FromXml(childNode);
                        this.Transforms.Add(transform);
                        break;
                    case nameof(PathOutline):
                        PathOutline pathOutline = new PathOutline();
                        pathOutline.FromXml(childNode);
                        this.BasicOutlines.Add(pathOutline);
                        if (pathOutline.UserDefinedVertices)
                            haveUserVertices = true;
                        break;
                    case "Center":
                        this.Center = Tools.GetPointFFromXml(childNode);
                        break;
                    case "CenterOffset":
                        centerOffset = Tools.GetPointFFromXml(childNode);
                        break;
                    case "CenterOffsetVector":
                        this.CenterOffsetVector = Tools.GetComplexFromXml(childNode);
                        break;
                    case "ZVector":
                        this.ZVector = Tools.GetComplexFromXml(childNode);
                        break;
                    case "PreviewZFactor":
                        this.PreviewZFactor = Tools.GetComplexFromXml(childNode);
                        break;
                    case nameof(PathFillInfo):
                        fillInfo = new PathFillInfo(this);
                        fillInfo.FromXml(childNode);
                        break;
                    case nameof(TextureFillInfo):
                        textureFillNode = childNode;
                        break;
                    case nameof(BackgroundFillInfo):
                        fillInfo = new BackgroundFillInfo(this);
                        fillInfo.FromXml(childNode);
                        break;
                    case nameof(PatternLayerList):
                        this.PatternLayers = new PatternLayerList(this);
                        this.PatternLayers.FromXml(childNode);
                        break;
                    case "CenterColor":
                        if (fillInfo == null)
                            fillInfo = new PathFillInfo(this);
                        this.CenterColor = Tools.GetColorFromXml(childNode);
                        break;
                    case "BoundaryColor":
                        if (fillInfo == null)
                            fillInfo = new PathFillInfo(this);
                        this.BoundaryColor = Tools.GetColorFromXml(childNode);
                        break;
                    case "PatternSection":
                        this.PatternSectionInfo.FromXml(childNode);
                        break;
                    case nameof(TileInfo):
                        PatternTileInfo.FromXml(childNode);
                        break;
                    case "PixelRendering":
                        PixelRendering = new RenderingInfo(this, createColorNodes: false);
                        PixelRendering.FromXml(childNode);
                        break;
                    case "PatternRecursion":
                        this.Recursion.FromXml(childNode);
                        break;
                    case "CenterPathPattern":
                        //this.CenterPathPattern = new PathPattern(childNode);
                        //this.CenterPathPattern.ComputeCurvePoints(CenterPathPattern.ZVector);
                        break;
                    case "origZVector":
                        this.origZVector = Tools.GetComplexFromXml(childNode);
                        break;
                    case "CenterPathZFactor":
                        //this.CenterPathZFactor = Tools.GetComplexFromXml(childNode);
                        break;
                    case "PatternImproviseConfig":
                        this.PatternImproviseConfig = new PatternImproviseConfig();
                        this.PatternImproviseConfig.FromXml(childNode);
                        PatternImproviseConfig.PopulateColorIndicesFromPattern(this, checkCounts: true);
                        break;
                    case nameof(InfluencePointInfoList):
                        InfluencePointInfoList.FromXml(childNode);
                        break;
                    default:
                        if (!FromExtraXml(childNode))
                            throw new Exception($"Invalid XML node name {childNode.Name} found for a pattern.");
                        break;
                }
            }
            foreach (var formulaSettings in GetFormulaSettings())
            {
                formulaSettings.ConfigureAllInfluenceParameters();
                if (formulaSettings.InfluenceLinkParentCollection != null)
                {
                    formulaSettings.InfluenceLinkParentCollection.ResolveReferences();
                }
            }
            InfluencePointInfoList.FinishFromXml();
            foreach (var influencePoint in InfluencePointInfoList.InfluencePointInfos)
            {
                influencePoint.OnLocationChanged();
            }
            if (textureFillNode != null)
            {
                fillInfo = new TextureFillInfo(this);
                fillInfo.FromXml(textureFillNode);
            }
            if (this.PatternLayers == null)
                this.PatternLayers = new PatternLayerList(this);
            if (fillInfo == null)
                fillInfo = new PathFillInfo(this);
            this.FillInfo = fillInfo;
            if (centerOffset != null)
            {   //Legacy code:
                PointF cOff = (PointF)centerOffset;
                float size = (float)ZVector.GetModulus();
                PointF gradientCenter = new PointF(Center.X + size * cOff.X,
                                                   Center.Y + size * cOff.Y);
                SetCenterOffset(gradientCenter);
            }
            if (haveUserVertices && Transforms.Count > 0)
                SetVertexAnglesParameters();
            HasRandomElements = GetHasRandom();
            SetImprovParameters();
        }

        public IEnumerable<FormulaSettings> GetFormulaSettings()
        {
            foreach (var transform in Transforms)
            {
                yield return transform.TransformSettings;
            }
            if (PixelRendering?.FormulaSettings != null)
            {
                yield return PixelRendering.FormulaSettings;
            }
        }

        public IEnumerable<KeyEnumParameters> GetKeyEnumParameters()
        {
            return InfluencePointInfoList.KeyEnumParamsDict.Values
                   .Concat(InfluencePointInfoList.InfluencePointInfos.SelectMany(ip => ip.KeyEnumParamsDict.Values));
        }

        private void SetImprovParameters()
        {
            if (this.PatternImproviseConfig == null)
                return;
            var parametersByGuidDict = new Dictionary<string, Parameter>(StringComparer.OrdinalIgnoreCase);
            foreach (var parameter in EnumerateParameters())
            {
                parametersByGuidDict.Add(parameter.Guid.ToString(), parameter);
            }
            foreach (var paramImprovConfig in this.PatternImproviseConfig.ParameterConfigs)
            {
                ParserEngine.Parameter parameter;
                if (!parametersByGuidDict.TryGetValue(
                               paramImprovConfig.ParameterGuid.ToString(), out parameter))
                {
                    parameter = null;
                }
                paramImprovConfig.Parameter = parameter;
            }
            this.PatternImproviseConfig.ParameterConfigs.RemoveAll(p => p.Parameter == null);
        }

        private bool GetHasRandom()
        {
            foreach (var outline in this.BasicOutlines)
            {
                if (outline.customOutline != null)
                {
                    if (outline.customOutline.AmplitudeSettings.CustomParameters.Where(
                                prm => prm.CustomType == CustomParameterTypes.RandomRange).Any())
                    {
                        HasRandomElements = true;
                        return true;
                    }
                }
            }
            foreach (var transform in this.Transforms)
            {
                if (transform.TransformSettings.CustomParameters.Where(
                            prm => prm.CustomType == CustomParameterTypes.RandomRange).Any())
                {
                    HasRandomElements = true;
                    return true;
                }
            }
            return false;
        }

        public bool HasSurroundColors()
        {
            return PatternLayers.PatternLayers.Exists(pl =>
                pl.FillInfo is PathFillInfo && 
                ((PathFillInfo)pl.FillInfo).ColorMode == FillInfo.PathColorModes.Surround);
        }

        public bool IsEquivalent(Pattern ptn2)
        {
            if (this.GetType() != ptn2.GetType())
                return false;
            if (this.BasicOutlines.Count != ptn2.BasicOutlines.Count)
                return false;
            if (this.SeedPoints == null || ptn2.SeedPoints == null)
                return this.SeedPoints == null && ptn2.SeedPoints == null;
            if (this.SeedPoints.Length != ptn2.SeedPoints.Length)
                return false;
            for (int j = 0; j < this.SeedPoints.Length; j++)
            {
                if (!Tools.NumbersEqual(this.SeedPoints[j].Angle, ptn2.SeedPoints[j].Angle) ||
                    !Tools.NumbersEqual(this.SeedPoints[j].Modulus, ptn2.SeedPoints[j].Modulus))
                    return false;
            }
            return true;
        }

        public void SetLargePropertiesToNull()
        {
            ClearRenderingCache();
            CurvePoints = null;
            SeedPoints = null;
        }

        public override void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            if (outlinePen != null)
            {
                outlinePen.Dispose();
                outlinePen = null;
            }
            if (FillInfo != null)
            {
                FillInfo.Dispose();
            }
            SetLargePropertiesToNull();
        }
    }
}

