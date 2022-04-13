using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Whorl
{
    public enum PathOutlineTypes
    {
        Cartesian,
        Polar
    }

    public class PathOutline : BasicOutline, ICloneable
    {
        public const double MaxModulus = 800.0;
        public const double PolygonUnitFactor = 1.0 / MaxModulus;
        private class UserVertexInfo
        {
            public int Index { get; }
            public PointF UserVertex { get; }
            public int Steps { get; }
            public PointF UnitVector { get; }

            public UserVertexInfo(int index, PointF userVertex, int steps, PointF unitVector)
            {
                Index = index;
                UserVertex = userVertex;
                Steps = steps;
                UnitVector = unitVector;
            }
        }
        public class PathOutlineVars
        {
            public PathOutline PathOutline { get; }
            [ReadOnly]
            public double AddDenom { get; set; }
            [ReadOnly]
            public double Petals { get; set; }
            [ReadOnly]
            public double AngleOffset { get; set; }

            public PathOutlineVars(PathOutline pathOutline)
            {
                PathOutline = pathOutline;
            }

            public void AddVertex(double x, double y)
            {
                PathOutline.AddVertex(x, y);
            }
        }
        public PathOutlineVars GlobalInfo { get; }
        //Multiple of 2 * pi:
        public double RotationSpan { get; set; } = 1D;

        private List<PointF> pathPoints { get; set; }

        public IEnumerable<PointF> PathPoints => pathPoints;

        public List<PointF> SegmentVertices { get; private set; }
        public List<PointF> LineVertices { get; private set; }
        public PointF SegmentVerticesCenter { get; set; } = PointF.Empty;
        public bool UseVertices { get; set; }
        public bool UserDefinedVertices { get; set; }
        public bool HasClosedPath { get; set; } = true;

        [XmlPreviousProperty("ClosedCurveVertices")]
        public bool HasCurveVertices { get; set; }

        [XmlPreviousProperty("PolygonUserVertices")]
        public bool HasLineVertices { get; set; }

        public override bool UseSingleOutline => HasLineVertices || HasCurveVertices;

        public List<int> CurveCornerIndices { get; } = new List<int>();

        public int[] VertexIndices { get; private set; }

        private PathOutlineTypes _pathOutlineType = PathOutlineTypes.Polar;
        public PathOutlineTypes PathOutlineType
        {
            get { return _pathOutlineType; }
            set
            {
                _pathOutlineType = value;
                VertexIndices = null;
            }
        }


        //Only computed for Cartesian path:
        public double NormalizedPathLength { get; private set; }

        public FormulaSettings VerticesSettings { get; }

        private int pathIndex;

        private enum GlobalVarNames
        {
            addDenom,
            petals,
            angleOffset
        }
        //private ValidIdentifier addDenomIdent, petalsIdent, angleOffsetIdent;

        public PathOutline() : base(BasicOutlineTypes.Path)
        {
            GlobalInfo = new PathOutlineVars(this);
            VerticesSettings = new FormulaSettings(FormulaTypes.PathVertices); // parseOnChanges: true);
            var memberNames = new List<string>(Enum.GetNames(typeof(GlobalVarNames)));
            memberNames.Add(nameof(AddVertex));
            VerticesSettings.TokensTransformInfo = new TokensTransformInfo(nameof(GlobalInfo), memberNames);
            ConfigureParser(VerticesSettings.Parser);
        }

        public PathOutline(PathOutline source) : base(BasicOutlineTypes.Path)
        {
            GlobalInfo = new PathOutlineVars(this);
            VerticesSettings = source.VerticesSettings.GetCopy(ConfigureParser);
            CopyProperties(source, excludedPropertyNames:
                new string[] { nameof(BasicOutlineType), nameof(UnitFactor), nameof(VerticesSettings),
                               nameof(SegmentVertices), nameof(LineVertices) });
            CurveCornerIndices.AddRange(source.CurveCornerIndices);
            if (source.pathPoints != null)
                pathPoints = new List<PointF>(source.pathPoints);
            if (source.SegmentVertices != null)
                SegmentVertices = new List<PointF>(source.SegmentVertices);
            if (source.LineVertices != null)
                LineVertices = new List<PointF>(source.LineVertices);
        }

        static PathOutline()
        {
            ExpressionParser.DeclareExternType(typeof(PathOutlineVars));
        }
        private void ConfigureParser(ExpressionParser parser)
        {
            parser.DeclareVariable(nameof(GlobalInfo), GlobalInfo.GetType(), GlobalInfo,
                                   isGlobal: true, isReadOnly: true);
            //MethodInfo fnMethod = typeof(PathOutline).GetMethod(nameof(AddVertex),
            //                      BindingFlags.NonPublic | BindingFlags.Instance);
            //parser.DeclareInstanceFunction(this, fnMethod);
            //foreach (GlobalVarNames varName in Enum.GetValues(typeof(GlobalVarNames)))
            //{
            //    parser.DeclareVariable(varName.ToString(), typeof(double), 0.0, isGlobal: true);
            //}
            //addDenomIdent = parser.GetVariableIdentifier(GlobalVarNames.addDenom.ToString());
            //petalsIdent = parser.GetVariableIdentifier(GlobalVarNames.petals.ToString());
            //angleOffsetIdent = parser.GetVariableIdentifier(GlobalVarNames.angleOffset.ToString());
        }

        public override object Clone()
        {
            return new PathOutline(this);
            //PathOutline copy = new PathOutline();
            //copy.CopyProperties(this, excludedPropertyNames:
            //    new string[] { nameof(VerticesSettings), nameof(BasicOutlineType),
            //                   nameof(UnitFactor) });
            //copy.VerticesSettings.CopyProperties(this.VerticesSettings);
            ////copy.VerticesSettings.Formula = this.VerticesSettings.Formula;
            ////copy.VerticesSettings.ParseOnChanges = true;
            //if (this.pathVertices != null)
            //    copy.pathVertices = new List<PointF>(this.pathVertices);
            //return copy;
        }

        public override FormulaSettings GetFormulaSettings()
        {
            if (UseVertices)
                return VerticesSettings;
            else
                return base.GetFormulaSettings();
        }

        public void InitUserDefinedVertices(bool forCurveVertices, bool hasClosedPath)
        {
            UserDefinedVertices = UseVertices = true;
            HasClosedPath = hasClosedPath;
            HasCurveVertices = forCurveVertices;
            HasLineVertices = !HasCurveVertices;
            SegmentVertices = new List<PointF>();
            CurveCornerIndices.Clear();
        }

        public Complex UpdateUserDefinedVertices(Complex zVector, bool forCurveVertices, bool hasClosedPath)
        {
            UserDefinedVertices = UseVertices = true;
            bool changed = false, curveChanged = false;
            if (HasClosedPath != hasClosedPath)
            {
                HasClosedPath = hasClosedPath;
                changed = true;
            }
            if (HasCurveVertices != forCurveVertices)
            {
                HasCurveVertices = forCurveVertices;
                changed = curveChanged = true;
            }
            HasLineVertices = !HasCurveVertices;
            if (changed)
            {
                if (HasLineVertices)
                    FinishUserDefinedVertices();
                else
                    SetCurvePathPoints();
                NormalizePathVertices();
                if (curveChanged)
                {
                    double maxModulus = GetPolygonMaxModulus(SegmentVerticesCenter);
                    if (HasLineVertices)
                    {
                        zVector *= maxModulus / MaxPathFactor;
                    }
                    else
                    {
                        zVector *= MaxPathFactor / maxModulus;
                    }
                }
            }
            return zVector;
        }

        /// <summary>
        /// Normalize SegmentVertices.
        /// </summary>
        /// <param name="center"></param>
        public void FinishUserDefinedVertices(PointF center)
        {
            SegmentVerticesCenter = center;
            FinishUserDefinedVertices();
        }

        public void FinishUserDefinedVertices()
        {
            PointF center = SegmentVerticesCenter;
            if ((UserDefinedVertices || HasLineVertices) && !HasCurveVertices)
            {
                float scale = (float)(1.0 / GetPolygonMaxModulus(center));
                var scaledVertices = SegmentVertices.Select(p => new PointF(scale * (p.X - center.X), scale * (p.Y - center.Y)));
                LineVertices = scaledVertices.ToList();
            }
        }

        /// <summary>
        /// Get the un-normalized Segment Vertex for a normalized vertex, p.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public PointF GetOrigSegmentVertex(PointF p)
        {
            float scale = (float)GetPolygonMaxModulus(SegmentVerticesCenter);
            return new PointF(scale * p.X + SegmentVerticesCenter.X, scale * p.Y + SegmentVerticesCenter.Y);
        }

        public bool FormulaIsValid
        {
            get
            {
                if (UserDefinedVertices)
                    return true;
                else
                {
                    var formulaSettings = GetFormulaSettings();
                    return formulaSettings == null || formulaSettings.IsValid;
                }
            }
        }

        //public void AddUserDefinedVertex(PointF vertex)
        //{
        //    pathVertices.Add(vertex);
        //}

        public void SetCurvePathPoints()
        {
            SetCurvePathPoints(SegmentVertices);
        }

        public void SetCurvePathPoints(List<PointF> segmentPoints)
        {
            if (HasLineVertices)
                return;
            bool setCurve;
            if (segmentPoints.Count < 3)
                setCurve = false;
            else
                setCurve = HasCurveVertices;
            if (HasClosedPath)
                Tools.ClosePoints(segmentPoints);
            if (setCurve)
            {
                pathPoints = CubicSpline.FitParametric(segmentPoints, CurveCornerIndices, HasClosedPath);
            }
            else
                pathPoints = new List<PointF>(segmentPoints);
        }

        public float MaxPathFactor { get; private set; }

        private float GetMaxPathFactor()
        {
            float xMax = Math.Max(Math.Abs(pathPoints.Select(p => p.X).Min()),
                      Math.Abs(pathPoints.Select(p => p.X).Max()));
            float yMax = Math.Max(Math.Abs(pathPoints.Select(p => p.Y).Min()),
                                  Math.Abs(pathPoints.Select(p => p.Y).Max()));
            float xyMax = Math.Max(xMax, yMax);
            return xyMax == 0 ? 1F : xyMax;
        }

        /// <summary>
        /// Translate vertices so center is at (0, 0), and scale to have max modulus of 1.
        /// </summary>
        /// <returns></returns>
        public Complex NormalizePathVertices()
        {
            if (HasLineVertices)
            {
                return new Complex(GetPolygonMaxModulus(SegmentVerticesCenter), 0.0);
            }
            PointF center = SegmentVerticesCenter;
            pathPoints = pathPoints.Select(
                p => new PointF(p.X - center.X, p.Y - center.Y)).ToList();
            MaxPathFactor = GetMaxPathFactor();
            float factor = 1F / MaxPathFactor;
            pathPoints = pathPoints.Select(
                p => new PointF(factor * p.X, factor * p.Y)).ToList();
            if (pathPoints.Count >= 2 && !HasCurveVertices)
            {
                Complex zOrig = new Complex(pathPoints[0].X,
                                            pathPoints[0].Y);
                if (zOrig != Complex.Zero)
                {
                    Complex zVec1 = new Complex(pathPoints[1].X - pathPoints[0].X,
                                                pathPoints[1].Y - pathPoints[0].Y);
                    Complex zProjected = zVec1 / zOrig;
                    if (zProjected.Im < 0)
                        pathPoints.Reverse();
                }
            }
            return new Complex(MaxPathFactor, 0.0);
        }

        public override double GetRotationSpan()
        {
            return RotationSpan;
        }

        public void AddVertices()
        {
            if (UserDefinedVertices)
                return;
            if (!UseVertices || !VerticesSettings.IsValid)
                pathPoints = null;
            else
            {
                pathPoints = new List<PointF>();
                GlobalInfo.AddDenom = AddDenom;
                GlobalInfo.Petals = Petals;
                GlobalInfo.AngleOffset = AngleOffset;
                VerticesSettings.SetCSharpInfoInstance(GlobalInfo);
                //addDenomIdent.SetCurrentValue(AddDenom);
                //petalsIdent.SetCurrentValue(Petals);
                //angleOffsetIdent.SetCurrentValue(AngleOffset);
                VerticesSettings.InitializeGlobals();
                if (VerticesSettings.EvalFormula())
                {
                    if (PathOutlineType == PathOutlineTypes.Cartesian)
                    {
                        NormalizeVertices();
                        NormalizedPathLength = Tools.PathLength(pathPoints);
                    }
                    else if (HasLineVertices)
                    {
                        SegmentVertices = new List<PointF>(pathPoints);
                        FinishUserDefinedVertices(PointF.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Called from parsed formula via reflection.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddVertex(double x, double y)
        {
            const int maxVerticesCount = 10000;
            if (pathPoints.Count >= maxVerticesCount)
                throw new Exception($"More than {maxVerticesCount} calls to AddVertex.");
            pathPoints.Add(new PointF((float)x, (float)y));
        }

        private void NormalizeVertices()
        {
            if (pathPoints == null || pathPoints.Count == 0)
                return;
            float xMin = pathPoints.Select(p => p.X).Min();
            float xMax = pathPoints.Select(p => p.X).Max();
            float yMin = pathPoints.Select(p => p.Y).Min();
            float yMax = pathPoints.Select(p => p.Y).Max();
            float absYMax = Math.Max(Math.Abs(yMin), Math.Abs(yMax));
            float xFactor = (xMin == xMax) ? 1 : 1F / (xMax - xMin);
            float yFactor = (absYMax == 0) ? 1 : 1F / absYMax;
            float factor = Math.Min(xFactor, Math.Min(yFactor, 1F));
            for (int i = 0; i < pathPoints.Count; i++)
            {
                PointF p = pathPoints[i];
                pathPoints[i] = new PointF((p.X - xMin) * factor, p.Y * factor);
            }
        }

        private void InitVertexIndices()
        {
            VertexIndices = new int[pathPoints.Count - 1];
            verticesIndex = 0;
            pathIndex = 0;
            prevVerticesIndex = -1;
        }

        public override bool InitComputeAmplitude(int rotationSteps)
        {
            if (PathOutlineType == PathOutlineTypes.Cartesian)
                //PathPattern.ComputeSeedPoints() does nothing in this case.
                throw new Exception("Cannot call InitComputeAmplitude on Cartesian path.");
            bool retVal;
            if (UseSingleOutline)
            {
                if (HasLineVertices)
                {
                    retVal = LineVertices != null && LineVertices.Count >= 3;
                    if (retVal)
                    {
                        InitComputePolygon(rotationSteps);
                    }
                }
                else
                {
                    retVal = pathPoints != null && pathPoints.Count > 1;
                    if (retVal)
                    {
                        InitVertexIndices();
                    }
                }
                if (retVal)
                {
                    computeAmplitude = null;
                }
            }
            else
            {
                retVal = pathPoints != null && pathPoints.Count > 1;
                if (retVal)
                {
                    InitVertexIndices();
                    computeAmplitude = ComputePathAmplitude;
                }
                else
                {
                    base.InitComputeAmplitude(rotationSteps);
                    computeAmplitude = CustomComputeAmplitude;
                    VertexIndices = null;
                }
            }
            return retVal;
        }

        //public List<PointF> GetScaledSegmentVertices()
        //{
        //    double maxModulus = GetPolygonMaxModulus();
        //    float scaleFactor = (float)(MaxModulus / maxModulus);
        //    //Scale vertices:
        //    return SegmentVertices.Select(v => Tools.ScalePoint(v, scaleFactor, SegmentVerticesCenter)).ToList();
        //}

        /// <summary>
        /// Initialize userVertexInfos array.  Already determined SegmentVertices.Count >= 3.
        /// </summary>
        /// <param name="rotationSteps"></param>
        private void InitComputePolygon(int rotationSteps)
        {
            if (!HasLineVertices) return;
            verticesIndex = 0;
            List<PointF> vertices = new List<PointF>(LineVertices);
            //Add first vertex to end of list if not there:
            if (HasClosedPath)
                Tools.ClosePoints(vertices);
            userVertexInfos = new UserVertexInfo[vertices.Count - 1];
            int index = 0;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                PointF vertex = vertices[i];
                PointF nextVertex = vertices[i + 1];
                int steps = Math.Max(1, (int)Math.Ceiling(MaxModulus * Tools.Distance(vertex, nextVertex)));
                PointF unitVector = new PointF((nextVertex.X - vertex.X) / steps,
                                               (nextVertex.Y - vertex.Y) / steps);
                userVertexInfos[i] = new UserVertexInfo(index, vertex, steps, unitVector);
                index += steps;
            }
        }

        //private void InitComputePolygon(int rotationSteps)
        //{
        //    const double newMaxModulus = 800.0;
        //    userVertexInfos = new UserVertexInfo[SegmentVertices.Count];
        //    verticesIndex = 0;
        //    double maxModulus = GetPolygonMaxModulus();
        //    float scaleFactor = (float)(newMaxModulus / maxModulus);
        //    maxModulus = newMaxModulus;
        //    int index = 0;
        //    for (int i = 0; i < SegmentVertices.Count; i++)
        //    {
        //        PointF vertex = Tools.ScalePoint(SegmentVertices[i], scaleFactor, SegmentVerticesCenter);
        //        PointF nextVertex = (i < SegmentVertices.Count - 1) ?
        //                             SegmentVertices[i + 1] : SegmentVertices[0];
        //        nextVertex = Tools.ScalePoint(nextVertex, scaleFactor, SegmentVerticesCenter);
        //        int steps = (int)Math.Ceiling(Tools.Distance(vertex, nextVertex));
        //        PointF unitVector = new PointF((nextVertex.X - vertex.X) / steps, 
        //                                       (nextVertex.Y - vertex.Y) / steps);
        //        userVertexInfos[i] = new UserVertexInfo(index, vertex, steps, unitVector);
        //        index += steps;
        //    }
        //    polygonUnitFactor = 1.0 / maxModulus;
        //}

        public double GetPolygonMaxModulus(PointF center)
        {
            return Math.Sqrt(SegmentVertices.Select(p => Tools.DistanceSquared(p, center)).Max());
        }

        public int GetVertexSteps()
        {
            int steps;
            if (HasLineVertices)
                steps = userVertexInfos.Select(v => v.Steps).Sum();
            else if (pathPoints != null)
                steps = pathPoints.Count;
            else
                throw new Exception("pathVertices is null for curve.");
            return steps;
        }

        public double ComputeVerticesPoint(int ind, out double angle)
        {
            double modulus;
            if (HasCurveVertices)
            {
                PointF p = pathPoints[ind];
                angle = Math.Atan2(p.Y, p.X);
                modulus = Math.Sqrt(p.X * p.X + p.Y * p.Y);
            }
            else
            {
                UserVertexInfo pInfo = userVertexInfos[verticesIndex];
                if (ind == pInfo.Index + pInfo.Steps && verticesIndex < userVertexInfos.Length - 1)
                {
                    pInfo = userVertexInfos[++verticesIndex];
                }
                if (ind == pInfo.Index)
                {
                    currPolygonPoint = pInfo.UserVertex;
                }
                else
                {
                    currPolygonPoint = new PointF(
                        currPolygonPoint.X + pInfo.UnitVector.X,
                        currPolygonPoint.Y + pInfo.UnitVector.Y);
                }
                var pVec = currPolygonPoint;
                modulus = Math.Sqrt(pVec.X * pVec.X + pVec.Y * pVec.Y);
                angle = Math.Atan2(pVec.Y, pVec.X);
            }
            return modulus;
        }

        private PointF currPolygonPoint { get; set; }
        private UserVertexInfo[] userVertexInfos { get; set; }
        private int verticesIndex = 0;
        private int prevVerticesIndex = -1;
        private Complex zP0;
        private Complex zVec;
        private double pModulus;

        private bool IntersectWithSegment(Complex zOrig, ref double modulus)
        {
            double rDiv = zVec.Re * zOrig.Im - zVec.Im * zOrig.Re;
            if (rDiv == 0)
                return false;  //Parallel to segment.
            double modP = (zOrig.Re * zP0.Im - zOrig.Im * zP0.Re) / rDiv;
            if (modP < 0 || modP > pModulus)
                return false;  //Intersection not within segment.
            if (Math.Abs(zOrig.Im) < 0.1)
                modulus = (zP0.Re + zVec.Re * modP) / zOrig.Re;
            else
                modulus = (zP0.Im + zVec.Im * modP) / zOrig.Im;
            //modulus = (zVec.Im * zP0.Re - zVec.Re * zP0.Im) / rDiv;
            return true;
        }

        private double ComputePathAmplitude(double angle)
        {
            int origIndex = verticesIndex;
            int currPathIndex = -1;
            double amplitude = 0;
            do
            {
                if (verticesIndex != prevVerticesIndex)
                {
                    prevVerticesIndex = verticesIndex;
                    zP0 = new Complex(pathPoints[verticesIndex].X,
                                      pathPoints[verticesIndex].Y);
                    zVec = new Complex(pathPoints[verticesIndex + 1].X,
                                       pathPoints[verticesIndex + 1].Y) - zP0;
                    pModulus = zVec.GetModulus();
                    if (pModulus != 0)
                        zVec /= pModulus;  //Normalize zVec
                    currPathIndex = pathIndex;
                }
                if (pModulus != 0)
                {
                    Complex zOrig = Complex.CreateFromModulusAndArgument(1D, angle);
                    if (IntersectWithSegment(zOrig, ref amplitude))
                    {
                        if (amplitude >= 0)
                            break;
                    }
                }
                if (verticesIndex < pathPoints.Count - 2)
                    verticesIndex++;
                else
                    verticesIndex = 0;
            } while (verticesIndex != origIndex);
            if (currPathIndex >= 0)
                VertexIndices[verticesIndex] = currPathIndex;
            pathIndex++;
            return amplitude;
        }

        private void AppendPointsXml(List<PointF> points, XmlNode parentNode, string nodeName, XmlTools xmlTools)
        {
            XmlNode childNode = xmlTools.CreateXmlNode(nodeName);
            foreach (PointF p in points)
            {
                childNode.AppendChild(xmlTools.CreateXmlNode("Vertex", p));
            }
            parentNode.AppendChild(childNode);
        }

        protected override void AppendExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.AppendExtraXml(parentNode, xmlTools);
            xmlTools.AppendAttributeChildNode(parentNode, "PathOutlineType", PathOutlineType);
            VerticesSettings.ToXml(parentNode, xmlTools, nameof(VerticesSettings));
            if (UserDefinedVertices)
            {
                if (SegmentVertices != null)
                    AppendPointsXml(SegmentVertices, parentNode, "SegmentVertices", xmlTools);
                parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(SegmentVerticesCenter), SegmentVerticesCenter));
                if (CurveCornerIndices.Count != 0)
                {
                    string sList = string.Join(",", CurveCornerIndices);
                    xmlTools.AppendAttributeChildNode(parentNode, nameof(CurveCornerIndices), sList);
                }
            }
        }

        public override void FromXml(XmlNode node)
        {
            base.FromXml(node);
            if (node.Attributes[nameof(UseVertices)] == null)
                UseVertices = VerticesSettings.IsValid;
            if (UseVertices)
            {
                if (UserDefinedVertices)
                {
                    FinishUserDefinedVertices();
                    if (!HasLineVertices)
                    {
                        SetCurvePathPoints();
                        NormalizePathVertices();
                    }
                }
                else
                    AddVertices();
            }
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            bool retVal = true;
            switch (node.Name)
            {
                case nameof(PathOutlineType):
                    PathOutlineType = Tools.GetEnumXmlAttr<PathOutlineTypes>(node,
                                      "Value", PathOutlineTypes.Polar);
                    break;
                case nameof(VerticesSettings):
                    VerticesSettings.FromXml(node, this);
                    break;
                case "SegmentVertices":
                case nameof(PathPoints):
                    SegmentVertices = new List<PointF>();
                    foreach (XmlNode vertexNode in node.ChildNodes)
                    {
                        SegmentVertices.Add(Tools.GetPointFFromXml(vertexNode));
                    }
                    break;
                case "SegmentVerticesCenter":
                    SegmentVerticesCenter = Tools.GetPointFFromXml(node);
                    break;
                case "VerticesFormula":
                    VerticesSettings.Parse(Tools.GetXmlNodeValue(node));
                    break;
                case nameof(CurveCornerIndices):
                    string sList = Tools.GetXmlAttribute<string>(node);
                    CurveCornerIndices.AddRange(sList.Split(',').Select(s => int.Parse(s)));
                    break;
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(PathOutline);
            return base.ToXml(parentNode, xmlTools, xmlNodeName);
        }
    }
}
