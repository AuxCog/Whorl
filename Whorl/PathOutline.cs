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

        public enum DrawTypes
        {
            Normal,
            Lines,
            Curve,
            Custom
        }

        //private class UserVertexInfo
        //{
        //    public int Index { get; }
        //    public PointF UserVertex { get; }
        //    public int Steps { get; }
        //    public PointF UnitVector { get; }

        //    public UserVertexInfo(int index, PointF userVertex, int steps, PointF unitVector)
        //    {
        //        Index = index;
        //        UserVertex = userVertex;
        //        Steps = steps;
        //        UnitVector = unitVector;
        //    }
        //}
        public class PathOutlineVars
        {
            private PathOutline pathOutline { get; }

            [ReadOnly]
            public double AddDenom { get; set; }
            [ReadOnly]
            public double Petals { get; set; }
            [ReadOnly]
            public double AngleOffset { get; set; }
            public IEnumerable<PointF> PathPoints => pathOutline.PathPoints;
            public bool HasClosedPath
            {
                get => pathOutline.HasClosedPath;
                set => pathOutline.HasClosedPath = value;
            }
            public PathOutlineVars(PathOutline pathOutline)
            {
                this.pathOutline = pathOutline;
            }

            public void AddVertex(double x, double y)
            {
                pathOutline.AddVertex(x, y);
            }

            public void AddVertex(PointF vertex)
            {
                pathOutline.AddVertex(vertex);
            }

            public void SetPathPoints(IEnumerable<PointF> points)
            {
                pathOutline.SetPathPoints(points);
            }

            public void ComputeInfluence(double x, double y)
            {
                pathOutline.ComputeInfluence(x, y);
            }

            public List<PointF> GetPathPointsList()
            {
                return pathOutline.GetPathPointsList();
            }
        }
        public PathOutlineVars GlobalInfo { get; }
        //Multiple of 2 * pi:
        public double RotationSpan { get; set; } = 1D;

        public int MaxPathPoints { get; set; } = 10000;

        protected List<PointF> pathPoints { get; set; }

        public IEnumerable<PointF> PathPoints => pathPoints;

        public List<PointF> SegmentVertices { get; private set; }
        public List<PointF> LineVertices { get; private set; }
        public PointF SegmentVerticesCenter { get; set; } = PointF.Empty;
        public PointF? PathCenter { get; set; }
        public PointF? NormalizedPathCenter { get; private set; }
        public bool UseVertices { get; set; }
        public bool UserDefinedVertices { get; set; }
        public bool VerticesAreForCurve { get; set; }
        public bool HasClosedPath { get; set; } = true;
        public override bool SupportsInfluencePoints => VerticesSettings != null;
        private bool computingInfluenceScale { get; set; }

        private DrawTypes _drawType = DrawTypes.Normal;
        public DrawTypes DrawType 
        {
            get => _drawType;
            set
            {
                _drawType = value;
                HasCurveVertices = _drawType == DrawTypes.Curve;
                HasLineVertices = _drawType == DrawTypes.Lines;
            }
        }

        [XmlPreviousProperty("ClosedCurveVertices")]
        public bool HasCurveVertices { get; private set; }

        [XmlPreviousProperty("PolygonUserVertices")]
        public bool HasLineVertices { get; private set; }

        public override bool UseSingleOutline => DrawType != DrawTypes.Normal;

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

        public bool UsesInfluencePoints { get; set; }

        public double InfluencePointsScale { get; protected set; } = 1.0;

        public FormulaSettings VerticesSettings { get; }

        public List<PathOutlineTransform> PathOutlineTransforms { get; } = new List<PathOutlineTransform>();

        private int pathIndex { get; set; }

        private int prevTransformsCount { get; set; }

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
            VerticesSettings = GetVerticesSettings(FormulaTypes.PathVertices);
        }

        public PathOutline(PathOutline source) : base(source)
        {
            GlobalInfo = new PathOutlineVars(this);
            DrawType = source.DrawType;
            VerticesSettings = source.VerticesSettings.GetCopy(ConfigureParser);
            CurveCornerIndices.AddRange(source.CurveCornerIndices);
            PathOutlineTransforms.AddRange(source.PathOutlineTransforms
                                 .Select(ot => new PathOutlineTransform(ot, this)));
            if (source.pathPoints != null)
                pathPoints = new List<PointF>(source.pathPoints);
            if (source.SegmentVertices != null)
                SegmentVertices = new List<PointF>(source.SegmentVertices);
            if (source.LineVertices != null)
                LineVertices = new List<PointF>(source.LineVertices);
        }

        public FormulaSettings GetVerticesSettings(FormulaTypes formulaType)
        {
            var verticesSettings = new FormulaSettings(formulaType);
            var memberNames = new List<string>(Enum.GetNames(typeof(GlobalVarNames)));
            memberNames.Add(nameof(AddVertex));
            verticesSettings.TokensTransformInfo = new TokensTransformInfo(nameof(GlobalInfo), memberNames);
            ConfigureParser(verticesSettings.Parser);
            return verticesSettings;
        }

        protected override IEnumerable<string> GetExcludedCopyPropertyNames()
        {
            return base.GetExcludedCopyPropertyNames().Concat(
                    new string[] { nameof(VerticesSettings),
                                   nameof(SegmentVertices), nameof(LineVertices), nameof(DrawType),
                                   nameof(HasCurveVertices), nameof(HasLineVertices) });
        }

        static PathOutline()
        {
            ExpressionParser.DeclareExternType(typeof(PathOutlineVars));
        }

        public void ConfigureParser(ExpressionParser parser)
        {
            parser.DeclareVariable(nameof(GlobalInfo), GlobalInfo.GetType(), GlobalInfo,
                                   isGlobal: true, isReadOnly: true);
        }

        public override object Clone()
        {
            return new PathOutline(this);
        }

        public override FormulaSettings GetFormulaSettings()
        {
            if (UseVertices)
                return VerticesSettings;
            else
                return base.GetFormulaSettings();
        }

        public void InitUserDefinedVertices(DrawTypes drawType, bool hasClosedPath)
        {
            UserDefinedVertices = UseVertices = true;
            DrawType = drawType;
            HasClosedPath = hasClosedPath;
            SegmentVertices = new List<PointF>();
            CurveCornerIndices.Clear();
        }

        public void ChangeToUserDefinedVertices(Pattern pattern, DrawTypes drawType)
        {
            if (UserDefinedVertices)
                return;
            if (SegmentVertices == null)
                throw new NullReferenceException("SegmentVertices cannot be null.");
            UserDefinedVertices = UseVertices = true;
            DrawType = drawType;
            PointF center = pattern.Center;
            SegmentVerticesCenter = center;
            var unitVertices = GetNormalizedVertices(center: PointF.Empty).ToArray();
            float scale = (float)pattern.ZVector.GetModulus();
            SetSegmentVertices(unitVertices.Select(v => 
                               new PointF(scale * v.X + center.X, scale * v.Y + center.Y)));
            ComputePathPoints();
        }

        public Complex UpdateUserDefinedVertices(Complex zVector, DrawTypes drawType, bool hasClosedPath)
        {
            UserDefinedVertices = UseVertices = true;
            bool changed = false, curveChanged = false;
            if (DrawType != drawType)
            {
                DrawType = drawType;
                changed = curveChanged = true;
            }
            if (HasClosedPath != hasClosedPath)
            {
                HasClosedPath = hasClosedPath;
                changed = true;
            }
            if (changed)
            {
                if (DrawType == DrawTypes.Lines)
                    FinishUserDefinedVertices();
                //else
                //    SetCurvePathPoints();
                ComputePathPoints();
                NormalizePathVertices();
                if (curveChanged)
                {
                    double maxModulus = GetPolygonMaxModulus(SegmentVerticesCenter);
                    if (DrawType == DrawTypes.Lines)
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

        public List<PointF> GetPathPointsList()
        {
            return pathPoints;
        }

        /// <summary>
        /// Normalize SegmentVertices.
        /// </summary>
        /// <param name="center"></param>
        public Complex FinishUserDefinedVertices(PointF center)
        {
            SegmentVerticesCenter = center;
            return FinishUserDefinedVertices();
        }

        public Complex FinishUserDefinedVertices()
        {
            Complex zVector = Complex.DefaultZVector;
            if (UserDefinedVertices || HasLineVertices || VerticesAreForCurve)
            {
                if (VerticesAreForCurve && SegmentVertices == null)
                    _AddVertices(callFinish: false);
                else
                {
                    if (!HasCurveVertices && !VerticesAreForCurve && SegmentVertices != null)
                    {
                        LineVertices = GetNormalizedVertices().ToList();
                    }
                }
                zVector = ComputePathPoints();
            }
            return zVector;
        }

        public IEnumerable<PointF> GetNormalizedVertices()
        {
            return GetNormalizedVertices(SegmentVerticesCenter);
        }

        private IEnumerable<PointF> GetNormalizedVertices(PointF center)
        {
            float scale = (float)(1.0 / GetPolygonMaxModulus(center));
            return SegmentVertices.Select(p => new PointF(scale * (p.X - center.X), scale * (p.Y - center.Y)));
        }

        /// <summary>
        /// Get the un-normalized Segment Vertex for a normalized vertex, p.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public PointF GetUnscaledSegmentVertex(PointF p)
        {
            float scale = (float)GetPolygonMaxModulus(SegmentVerticesCenter);
            return new PointF(scale * p.X + SegmentVerticesCenter.X, scale * p.Y + SegmentVerticesCenter.Y);
        }

        public bool FormulaIsValid
        {
            get
            {
                if (UserDefinedVertices || DrawType == DrawTypes.Custom)
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

        public virtual Complex ComputePathPoints()
        {
            Complex zVector = Complex.DefaultZVector;
            if (!UseVertices)
                return zVector;
            if (HasLineVertices && !VerticesAreForCurve)
                ComputeLinePathPoints();
            else if (UserDefinedVertices || (VerticesAreForCurve && SegmentVertices != null))
                SetCurvePathPoints(SegmentVertices);
            if (HasLineVertices || UserDefinedVertices || VerticesAreForCurve)
                zVector = NormalizePathVertices();
            else
                AddVertices();
            prevTransformsCount = PathOutlineTransforms.Count;
            return zVector;
        }

        public void ComputeInfluence(double x, double y)
        {
            if (computingInfluenceScale || VerticesSettings == null)
                return;
            if (VerticesSettings.InfluenceLinkParentCollection == null)
                return;
            double scale = InfluencePointsScale * 
                           VerticesSettings.InfluenceLinkParentCollection.ParentPattern.ZVector.GetModulus();
            VerticesSettings.InfluenceLinkParentCollection.SetParameterValues(
                             new DoublePoint(scale * x, scale * y), forRendering: false);
        }


        //public void SetCurvePathPoints()
        //{
        //    SetCurvePathPoints(SegmentVertices);
        //}

        public void SetCurvePathPoints(List<PointF> segmentPoints)
        {
            if (HasLineVertices && !VerticesAreForCurve)
                return;
            bool fitCurve;
            if (segmentPoints.Count < 3)
                fitCurve = false;
            else
                fitCurve = (HasCurveVertices && UserDefinedVertices) || VerticesAreForCurve;
            List<PointF> segPoints = segmentPoints;
            if (HasClosedPath)
            {
                segPoints = new List<PointF>(segmentPoints);
                Tools.ClosePoints(segPoints);
            }
            if (fitCurve)
            {
                pathPoints = CubicSpline.FitParametric(segPoints, CurveCornerIndices, HasClosedPath);
                pathPoints = pathPoints.FindAll(p => !(float.IsNaN(p.X) || float.IsNaN(p.Y)));
                if (pathPoints.Count < 3)
                    pathPoints = new List<PointF>(segPoints);
                foreach (PathOutlineTransform pathOutlineTransform in PathOutlineTransforms)
                {
                    pathOutlineTransform.TransformPathPoints();
                }
            }
            else
                pathPoints = new List<PointF>(segPoints);
        }

        public float MaxPathFactor { get; private set; }

        private float GetMaxPathFactor()
        {
            if (pathPoints == null || pathPoints.Count == 0)
                return 1F;
            float xMax = Math.Max(Math.Abs(pathPoints.Select(p => p.X).Min()),
                      Math.Abs(pathPoints.Select(p => p.X).Max()));
            float yMax = Math.Max(Math.Abs(pathPoints.Select(p => p.Y).Min()),
                                  Math.Abs(pathPoints.Select(p => p.Y).Max()));
            float xyMax = Math.Max(xMax, yMax);
            return xyMax == 0 ? 1F : xyMax;
        }

        private void SetNormalizedPathCenter(PointF center, float scale)
        {
            if (PathCenter == null) return;
            PointF p = new PointF(PathCenter.Value.X - center.X, PathCenter.Value.Y - center.Y);
            NormalizedPathCenter = new PointF(scale * p.X + center.X, scale * p.Y + center.Y);
        }

        /// <summary>
        /// Translate vertices so center is at (0, 0), and scale to have max modulus of 1.
        /// </summary>
        /// <returns></returns>
        protected Complex NormalizePathVertices()
        {
            try
            {
                if (computingInfluenceScale)
                    InfluencePointsScale = 1.0;
                NormalizedPathCenter = null;
                if (HasLineVertices && !VerticesAreForCurve)
                {
                    if (SegmentVertices != null)
                    {
                        double modulus = GetPolygonMaxModulus(SegmentVerticesCenter);
                        if (computingInfluenceScale)
                            InfluencePointsScale = 1.0 / modulus;
                        SetNormalizedPathCenter(SegmentVerticesCenter, 1F / (float)modulus);
                        return new Complex(modulus, 0.0);
                    }
                    else
                        return Complex.DefaultZVector;
                }
                PointF center = SegmentVerticesCenter;
                pathPoints = pathPoints.Select(
                    p => new PointF(p.X - center.X, p.Y - center.Y)).ToList();
                MaxPathFactor = GetMaxPathFactor();
                float factor = 1F / MaxPathFactor;
                SetNormalizedPathCenter(SegmentVerticesCenter, factor);
                if (computingInfluenceScale)
                    InfluencePointsScale = factor;
                pathPoints = pathPoints.Select(
                    p => new PointF(factor * p.X, factor * p.Y)).ToList();
                if (pathPoints.Count >= 2 && !HasCurveVertices && DrawType != DrawTypes.Custom)
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
            catch
            {
                if (computingInfluenceScale)
                    InfluencePointsScale = 1.0;
                throw;
            }
        }

        public override double GetRotationSpan()
        {
            return RotationSpan;
        }

        private bool refreshPathPoints { get; set; } = true;

        public void RefreshVertices()
        {
            refreshPathPoints = true;
            if (UserDefinedVertices || DrawType == DrawTypes.Custom)
            {
                ComputePathPoints();
            }
            else
            {
                AddVertices();
            }
        }

        public void AddVertices()
        {
            if (UserDefinedVertices || DrawType == DrawTypes.Custom)
                return;
            if (!UseVertices || !VerticesSettings.IsValid)
            {
                pathPoints = null;
                return;
            }
            if (!refreshPathPoints && pathPoints != null)
                return;
            refreshPathPoints = false;
            bool useInfluence = UsesInfluencePoints && VerticesSettings.InfluenceLinkParentCollection != null;
            if (useInfluence)
            {
                try
                {
                    computingInfluenceScale = true;
                    //Since computingInfluenceScale is true, ComputeInfluence() does nothing.
                    _AddVertices();
                }
                finally
                {
                    computingInfluenceScale = false;
                }
            }
            if (useInfluence)
                VerticesSettings.InfluenceLinkParentCollection.Initialize();
            try
            {
                _AddVertices();
            }
            finally
            {
                if (useInfluence)
                    VerticesSettings.InfluenceLinkParentCollection.FinalizeSettings();
            }
            foreach (PathOutlineTransform pathOutlineTransform in PathOutlineTransforms)
            {
                pathOutlineTransform.TransformPathPoints();
            }
        }

        /// <summary>
        /// Also called from PathOutlineTransform class.
        /// </summary>
        /// <param name="globalInfo"></param>
        /// <param name="verticesSettings"></param>
        public void InitializeFormula(PathOutlineVars globalInfo, FormulaSettings verticesSettings)
        {
            globalInfo.AddDenom = AddDenom;
            globalInfo.Petals = Petals;
            globalInfo.AngleOffset = AngleOffset;
            verticesSettings.SetCSharpInfoInstance(globalInfo);
            verticesSettings.InitializeGlobals();
        }

        private void _AddVertices(bool callFinish = true)
        {
            pathPoints = new List<PointF>();
            InitializeFormula(GlobalInfo, VerticesSettings);
            if (VerticesSettings.EvalFormula())
            {
                if (PathOutlineType == PathOutlineTypes.Cartesian)
                {
                    NormalizePathPoints();
                    NormalizedPathLength = Tools.PathLength(pathPoints);
                }
                else if (HasLineVertices || VerticesAreForCurve)
                {
                    SetSegmentVertices(pathPoints);
                    if (callFinish)
                        FinishUserDefinedVertices(PointF.Empty);
                }
                if (HasCurveVertices || HasLineVertices)
                {
                    NormalizePathVertices();
                }
            }
        }

        private void SetSegmentVertices(IEnumerable<PointF> points)
        {
            SegmentVertices = new List<PointF>();
            foreach (PointF point in points)
            {
                if (SegmentVertices.Count == 0 || SegmentVertices.Last() != point)
                {
                    SegmentVertices.Add(point);
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
            AddVertex(new PointF((float)x, (float)y));
        }

        public void AddVertex(PointF vertex)
        {
            if (pathPoints.Count >= MaxPathPoints)
                throw new CustomException($"There were more than {MaxPathPoints} calls to AddVertex.");
            pathPoints.Add(vertex);
        }

        public void SetPathPoints(IEnumerable<PointF> points)
        {
            pathPoints = points.ToList();
        }

        private void NormalizePathPoints()
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
            VertexIndices = new int[Math.Max(0, pathPoints.Count - 1)];
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
                if (HasLineVertices && !VerticesAreForCurve)
                {
                    retVal = LineVertices != null && LineVertices.Count >= 3;
                    if (retVal)
                    {
                        InitComputePolygon();
                    }
                }
                else
                {
                    bool recompute = PathOutlineTransforms.Count > 0 || prevTransformsCount > 0;
                    if (DrawType == DrawTypes.Custom || recompute)
                    {
                        if (pathPoints == null || recompute)
                            ComputePathPoints();
                    }
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
        private void InitComputePolygon()
        {
            if (HasLineVertices)
                verticesIndex = 0;
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
            if (SegmentVertices == null || SegmentVertices.Count == 0)
                return 0;
            return Math.Sqrt(SegmentVertices.Select(p => Tools.DistanceSquared(p, center)).Max());
        }

        public int GetVertexSteps()
        {
            //int steps;
            //if (HasLineVertices)
            //    steps = userVertexInfos.Select(v => v.Steps).Sum();
            //else 
            if (pathPoints != null)
                return pathPoints.Count;
            else
                throw new Exception("pathPoints is null for PathOutline.");
            //return steps;
        }

        private bool ComputeLinePathPoints()
        {
            if (!(UseSingleOutline && HasLineVertices))
                return false;
            pathPoints = new List<PointF>();
            if (LineVertices == null || LineVertices.Count < 2)
                return false;
            InitComputePolygon();
            List<PointF> vertices = new List<PointF>(LineVertices);
            if (HasClosedPath)
                //Add first vertex to end of list if not there:
                Tools.ClosePoints(vertices);
            //var userVertexInfos = new UserVertexInfo[vertices.Count - 1];
            //int index = 0;
            int i = 0;
            while (true)
            {
                PointF vertex = vertices[i++];
                PointF p = vertex;
                pathPoints.Add(p);
                if (i == vertices.Count)
                    break;
                PointF nextVertex = vertices[i];
                int steps = Math.Max(1, (int)Math.Ceiling(MaxModulus * Tools.Distance(vertex, nextVertex)));
                if (steps > 1)
                {
                    PointF unitVector = new PointF((nextVertex.X - vertex.X) / steps,
                                                   (nextVertex.Y - vertex.Y) / steps);
                    for (int step = 1; step < steps; step++)
                    {
                        p.X += unitVector.X;
                        p.Y += unitVector.Y;
                        pathPoints.Add(p);
                    }
                }
                //userVertexInfos[i] = new UserVertexInfo(index, vertex, steps, unitVector);
                //index += steps;
            }
            //PointF p = Point.Empty;
            //int maxInd = userVertexInfos.Select(v => v.Steps).Sum();
            //for (int ind = 0; ind < maxInd; ind++)
            //{
            //    UserVertexInfo uvInfo = userVertexInfos[verticesIndex];
            //    if (ind == uvInfo.Index + uvInfo.Steps && verticesIndex < userVertexInfos.Length - 1)
            //    {
            //        uvInfo = userVertexInfos[++verticesIndex];
            //    }
            //    if (ind == uvInfo.Index)
            //    {
            //        p = uvInfo.UserVertex;
            //    }
            //    else
            //    {
            //        p = new PointF(p.X + uvInfo.UnitVector.X, p.Y + uvInfo.UnitVector.Y);
            //    }
            //    pathPoints.Add(p);
            //}
            return true;
        }

        public double ComputeVerticesPoint(int ind, out double angle)
        {
            PointF p;
            try
            {
                p = pathPoints[ind];
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException($"pathPoints index out of range: {ind}.");
            }
            angle = Math.Atan2(p.Y, p.X);
            double modulus = Math.Sqrt(p.X * p.X + p.Y * p.Y);
            return modulus;
        }

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
            if (PathCenter != null)
                parentNode.AppendChild(xmlTools.CreateXmlNode("PathCenter", PathCenter.Value));
            foreach (PathOutlineTransform pathOutlineTransform in PathOutlineTransforms)
            {
                pathOutlineTransform.ToXml(parentNode, xmlTools);
            }
        }

        public override void FromXml(XmlNode node)
        {
            base.FromXml(node);
            if (node.Attributes[nameof(DrawType)] == null)
            {
                if (HasLineVertices)
                    DrawType = DrawTypes.Lines;
                else if (HasCurveVertices)
                    DrawType = DrawTypes.Curve;
            }
            if (UserDefinedVertices && DrawType == DrawTypes.Normal)
            {
                if (HasCurveVertices)
                    DrawType = DrawTypes.Curve;
                else
                    DrawType = DrawTypes.Lines;
            }
            if (node.Attributes[nameof(UseVertices)] == null)
                UseVertices = VerticesSettings.IsValid;
            if (UserDefinedVertices || HasLineVertices || VerticesAreForCurve)
            {
                FinishUserDefinedVertices();
            }
            //else
            //    ComputePathPoints();
            //if (UseVertices)
            //{
            //    if (UserDefinedVertices)
            //    {
            //        FinishUserDefinedVertices();
            //        if (!HasLineVertices)
            //        {
            //            SetCurvePathPoints();
            //            NormalizePathVertices();
            //        }
            //    }
            //    else
            //        AddVertices();
            //}
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
                    var points = new List<PointF>();
                    foreach (XmlNode vertexNode in node.ChildNodes)
                    {
                        points.Add(Tools.GetPointFFromXml(vertexNode));
                    }
                    SetSegmentVertices(points);
                    break;
                case "SegmentVerticesCenter":
                    SegmentVerticesCenter = Tools.GetPointFFromXml(node);
                    break;
                case "PathCenter":
                    PathCenter = Tools.GetPointFFromXml(node);
                    break;
                case "VerticesFormula":
                    VerticesSettings.Parse(Tools.GetXmlNodeValue(node));
                    break;
                case nameof(CurveCornerIndices):
                    string sList = Tools.GetXmlAttribute<string>(node);
                    CurveCornerIndices.AddRange(sList.Split(',').Select(s => int.Parse(s)));
                    break;
                case nameof(PathOutlineTransform):
                    PathOutlineTransform pathOutlineTransform = new PathOutlineTransform(this);
                    pathOutlineTransform.FromXml(node);
                    PathOutlineTransforms.Add(pathOutlineTransform);
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
