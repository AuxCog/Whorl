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

    public class PathOutline: BasicOutline, ICloneable
    {
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

        private List<PointF> pathVertices { get; set; }

        public IEnumerable<PointF> PathVertices
        {
            get { return pathVertices; }
        }

        public List<PointF> SegmentVertices { get; private set; }
        public PointF SegmentVerticesCenter { get; set; }
        public bool UseVertices { get; set; }
        public bool UserDefinedVertices { get; set; }
        public bool ClosedCurveVertices { get; set; }
        public bool PolygonUserVertices { get; set; }

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

        public PathOutline(): base(BasicOutlineTypes.Path)
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
                               nameof(SegmentVertices) });
            if (source.pathVertices != null)
                pathVertices = new List<PointF>(source.pathVertices);
            if (source.SegmentVertices != null)
                SegmentVertices = new List<PointF>(source.SegmentVertices);
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

        public void InitUserDefinedVertices(bool forClosedCurve)
        {
            //if (pathVertices == null)
            //    pathVertices = new List<PointF>();
            //else
            //    pathVertices.Clear();
            this.UserDefinedVertices = this.UseVertices = true;
            this.ClosedCurveVertices = forClosedCurve;
            SegmentVertices = new List<PointF>();
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

        public void SetClosedVertices(List<PointF> segmentPoints, bool setCurve)
        {
            if (PolygonUserVertices)
                return;
            if (segmentPoints.Count < 3)
                setCurve = false;
            Tools.ClosePoints(segmentPoints);
            if (setCurve)
            {
                double pathLength = Tools.PathLength(segmentPoints.ToArray());
                if (pathLength < 3D)
                    setCurve = false;
                else
                {
                    pathVertices = CubicSpline.FitParametricClosed(segmentPoints, (int)pathLength).ToList();
                }
            }
            if (!setCurve)
                pathVertices = new List<PointF>(segmentPoints);
        }

        /// <summary>
        /// Translate vertices so center is at (0, 0), and scale to have max dimension of 1.
        /// </summary>
        /// <returns></returns>
        public Complex NormalizePathVertices()
        {
            //Tools.ClosePoints(NormalizedSegmentVertices);
            //PointF centroid = Tools.Centroid(NormalizedSegmentVertices);
            //center = centroid;
            if (PolygonUserVertices)
            {
                return new Complex(GetPolygonMaxModulus(), 0.0);
            }
            PointF center = SegmentVerticesCenter;
            pathVertices = pathVertices.Select(
                p => new PointF(p.X - center.X, p.Y - center.Y)).ToList();
            float xMax = Math.Max(Math.Abs(pathVertices.Select(p => p.X).Min()),
                                  Math.Abs(pathVertices.Select(p => p.X).Max()));
            float yMax = Math.Max(Math.Abs(pathVertices.Select(p => p.Y).Min()),
                                  Math.Abs(pathVertices.Select(p => p.Y).Max()));
            float xyMax = Math.Max(xMax, yMax);
            float maxFactor = xyMax == 0 ? 1 : xyMax;
            float factor = 1 / maxFactor;
            pathVertices = pathVertices.Select(
                p => new PointF(factor * p.X, factor * p.Y)).ToList();
            if (pathVertices.Count >= 2)
            {
                Complex zOrig = new Complex(pathVertices[0].X, 
                                            pathVertices[0].Y);
                Complex zVec1 = new Complex(pathVertices[1].X - 
                                            pathVertices[0].X,
                                            pathVertices[1].Y - 
                                            pathVertices[0].Y);
                if (zOrig != Complex.Zero)
                {
                    Complex zProjected = zVec1 / zOrig;
                    if (zProjected.Im < 0)
                        pathVertices.Reverse();
                }
            }
            return new Complex(maxFactor, 0);
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
                pathVertices = null;
            else
            {
                pathVertices = new List<PointF>();
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
                        NormalizedPathLength = Tools.PathLength(pathVertices);
                    }
                    else if (PolygonUserVertices)
                    {
                        SegmentVertices = new List<PointF>(pathVertices);
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
            if (pathVertices.Count >= maxVerticesCount)
                throw new Exception($"More than {maxVerticesCount} calls to AddVertex.");
            pathVertices.Add(new PointF((float)x, (float)y));
        }

        private void NormalizeVertices()
        {
            if (pathVertices == null || pathVertices.Count == 0)
                return;
            float xMin = pathVertices.Select(p => p.X).Min();
            float xMax = pathVertices.Select(p => p.X).Max();
            float yMin = pathVertices.Select(p => p.Y).Min();
            float yMax = pathVertices.Select(p => p.Y).Max();
            float absYMax = Math.Max(Math.Abs(yMin), Math.Abs(yMax));
            float xFactor = (xMin == xMax) ? 1 : 1F / (xMax - xMin);
            float yFactor = (absYMax == 0) ? 1 : 1F / absYMax;
            float factor = Math.Min(xFactor, Math.Min(yFactor, 1F));
            for (int i = 0; i < pathVertices.Count; i++)
            {
                PointF p = pathVertices[i];
                pathVertices[i] = new PointF((p.X - xMin) * factor, p.Y * factor);
            }
        }

        public override bool InitComputeAmplitude(int rotationSteps)
        {
            if (PathOutlineType == PathOutlineTypes.Cartesian)
                //PathPattern.ComputeSeedPoints() does nothing in this case.
                throw new Exception("Cannot call InitComputeAmplitude on Cartesian path.");
            bool retVal;
            if (PolygonUserVertices)
            {
                retVal = SegmentVertices != null && SegmentVertices.Count >= 3;
                if (retVal)
                {
                    InitComputePolygon(rotationSteps);
                    computeAmplitude = null;
                }
            }
            else
            {
                retVal = pathVertices != null && pathVertices.Count > 1;
                if (retVal)
                {
                    VertexIndices = new int[pathVertices.Count - 1];
                    verticesIndex = 0;
                    pathIndex = 0;
                    prevVerticesIndex = -1;
                    computeAmplitude = ComputePathAmplitude;
                }
                else
                {
                    computeAmplitude = CustomComputeAmplitude;
                    VertexIndices = null;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Initialize userVertexInfos array.  Already determined SegmentVertices.Count >= 3.
        /// </summary>
        /// <param name="rotationSteps"></param>
        private void InitComputePolygon(int rotationSteps)
        {
            const double newMaxModulus = 800.0;
            userVertexInfos = new UserVertexInfo[SegmentVertices.Count];
            verticesIndex = 0;
            double maxModulus = GetPolygonMaxModulus();
            float scaleFactor = (float)(newMaxModulus / maxModulus);
            maxModulus = newMaxModulus;
            //Scale vertices:
            List<PointF> vertices = SegmentVertices.Select(v => Tools.ScalePoint(v, scaleFactor, SegmentVerticesCenter)).ToList();
            //Add first vertex to end of list:
            vertices.Add(vertices[0]);
            int index = 0;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                PointF vertex = vertices[i];
                PointF nextVertex = vertices[i + 1];
                int steps = Math.Max(1, (int)Math.Ceiling(Tools.Distance(vertex, nextVertex)));
                PointF unitVector = new PointF((nextVertex.X - vertex.X) / steps,
                                               (nextVertex.Y - vertex.Y) / steps);
                userVertexInfos[i] = new UserVertexInfo(index, vertex, steps, unitVector);
                index += steps;
            }
            polygonUnitFactor = 1.0 / maxModulus;
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

        private double GetPolygonMaxModulus()
        {
            return SegmentVertices.Select(p => Tools.Distance(p, SegmentVerticesCenter)).Max();
        }

        public int GetPolygonSteps()
        {
            return userVertexInfos.Select(v => v.Steps).Sum();
        }

        public double ComputePolygonPoint(int ind, out double angle)
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
            var pVec = new PointF(currPolygonPoint.X - SegmentVerticesCenter.X,
                                  currPolygonPoint.Y - SegmentVerticesCenter.Y);
            double modulus = polygonUnitFactor * Math.Sqrt(pVec.X * pVec.X + pVec.Y * pVec.Y);
            angle = modulus == 0 ? 0.0 : Math.Atan2(pVec.Y, pVec.X);
            return modulus;
        }

        private PointF currPolygonPoint { get; set; }
        private double polygonUnitFactor { get; set; }
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
                    zP0 = new Complex(pathVertices[verticesIndex].X,
                                      pathVertices[verticesIndex].Y);
                    zVec = new Complex(pathVertices[verticesIndex + 1].X,
                                       pathVertices[verticesIndex + 1].Y) - zP0;
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
                if (verticesIndex < pathVertices.Count - 2)
                    verticesIndex++;
                else
                    verticesIndex = 0;
            } while (verticesIndex != origIndex);
            if (currPathIndex >= 0)
                VertexIndices[verticesIndex] = currPathIndex;
            pathIndex++;
            return amplitude;
        }

        protected override void AppendExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.AppendExtraXml(parentNode, xmlTools);
            xmlTools.AppendAttributeChildNode(parentNode, "PathOutlineType", PathOutlineType);
            VerticesSettings.ToXml(parentNode, xmlTools, nameof(VerticesSettings));
            if (UserDefinedVertices && SegmentVertices != null)
            {
                XmlNode childNode = xmlTools.CreateXmlNode("SegmentVertices");
                foreach (PointF p in SegmentVertices)
                {
                    childNode.AppendChild(xmlTools.CreateXmlNode("Vertex", p));
                }
                parentNode.AppendChild(childNode);
                parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(SegmentVerticesCenter), SegmentVerticesCenter));
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
                    SetClosedVertices(SegmentVertices, ClosedCurveVertices);
                    NormalizePathVertices();
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
                case nameof(PathVertices):
                case "SegmentVertices":
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
