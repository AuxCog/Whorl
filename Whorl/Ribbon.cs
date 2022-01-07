using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml;
using System.Diagnostics;
using ParserEngine;
using System.Drawing.Drawing2D;

namespace Whorl
{
    public enum RibbonDrawingModes: byte
    {
        Fill,
        WaveFill,
        Line,
        LineFill,
        CopyPattern,
        DirectFill
    }

    //public enum RibbonPathModes: byte
    //{
    //    Path,
    //    Segmented
    //}

    public class Ribbon : Pattern, ICloneable
    {
        [PropertyAction(MinValue = 10)]
        public double RibbonDistance { get; set; } = 200D;
        public Complex PatternZVector { get; set; } = new Complex(30, 30);
        public List<PointF> RibbonPath { get; } = new List<PointF>();
        public float PenWidth { get; set; } = 1F;
        public float TaperPercentage { get; set; } = 0;
        public bool TrackAngle { get; set; } = true;
        public bool CycleColors { get; set; }
        public bool SmoothGradient { get; set; } = true;
        public bool LinearGradientPerSegment { get; set; }
        public RibbonDrawingModes DrawingMode { get; set; } = RibbonDrawingModes.WaveFill;
        //public RibbonPathModes PathMode { get; set; }
        public int MinSegments { get; set; } = 20;
        public bool FitToPathVertices { get; set; }
        public bool ScaleTaper { get; set; }
        public bool DrawReversed { get; set; }
        public double SegmentPadding { get; set; }
        public double VertexPadding { get; set; }
        public FormulaSettings FormulaSettings { get; private set; }
        public bool FormulaEnabled { get; set; } = true;
        public float DirectFillAngleOffset { get; set; }
        public float DirectFillAmpOffset { get; set; }
        public Pattern CopiedPattern { get; set; }

        private Pen RibbonPen { get; set; }
        private bool drawFilled;
        private ColorGradient gradient;
        private float sizeFactor, sizeFactorDecrement, taperFac, fPenWidth, penWidthDecrement;
        private double currRibbonDistance;
        private List<PointF> lineDrawPoints;
        private List<int> lineSegmentStartIndices;
        private Color color1;
        private bool useLinearGradient;

        [PropertyAction]
        public double RotationAngle
        {
            get
            {
                return Tools.RadiansToDegrees(PatternZVector.GetArgument());
            }
            set
            {
                double angle = Tools.DegreesToRadians(value);
                PatternZVector = Complex.CreateFromModulusAndArgument(PatternZVector.GetModulus(), angle);
            }
        }

        [PropertyAction(MinValue = 1)]
        public double PatternSize
        {
            get
            {
                return PatternZVector.GetModulus();
            }
            set
            {
                double factor = value / PatternZVector.GetModulus();
                PatternZVector *= factor;
            }
        }

        private Ribbon(WhorlDesign design): base(design)
        { }

        public Ribbon(WhorlDesign design, XmlNode node): base(design)
        {
            FromXml(node);
        }

        public Ribbon(Pattern pattern, WhorlDesign design = null) : base(design ?? pattern.Design)
        {
            if (pattern.PixelRendering != null && pattern.PixelRendering.Enabled)
            {
                DrawingMode = RibbonDrawingModes.CopyPattern;
            }
            base.CopyProperties(pattern);
            AddToRibbonPath();
        }

        public Ribbon(Ribbon source, bool keepRecursiveParent, WhorlDesign design = null) : this(source, design)
        {
            CopyRibbonPath(source);
            if (source.FormulaSettings != null)
            {
                FormulaSettings = source.FormulaSettings.GetCopy(ConfigureParser, pattern: this);
            }
        }

        public override Pattern GetCopy(bool keepRecursiveParent = false, WhorlDesign design = null)
        {
            return new Ribbon(this, keepRecursiveParent, design);
            //Ribbon copy = new Ribbon();
            //copy.CopyProperties(this, copySharedPatternID: copySharedPatternID);
            //copy.CopyRibbonPath(this);
            //if (FormulaSettings != null)
            //{
            //    copy.InitFormulaSettings();
            //    copy.FormulaSettings.CopyProperties(FormulaSettings);
            //}
            //return copy;
        }

        public override object Clone()
        {
            return GetCopy();
        }
        public override Complex PreviewZFactor
        {
            get { return new Complex(1, 0); }
            set { }
        }

        public bool ShouldAddToPath
        {
            get
            {
                return OutlineZVector.GetModulus() >= RibbonDistance;
            }
        }

        public override Ribbon GetRibbon()
        {
            return this;
        }

        //public double GetRibbonDistance(int pathIndex)
        //{
        //    double ribbonDistance = RibbonDistance;
        //    if (ScaleTaper && TaperPercentage != 0 && RibbonPath.Count > 2)
        //    {
        //        if (TaperPercentage < 0)
        //            pathIndex = RibbonPath.Count - pathIndex - 1;
        //        if (pathIndex >= 2)
        //        {
        //            double taperFac = 0.01 * Math.Abs(TaperPercentage) / (RibbonPath.Count - 1);
        //            ribbonDistance = RibbonDistance * (RibbonPath.Count - pathIndex) * taperFac;
        //        }
        //    }
        //    return ribbonDistance;
        //}

        private enum GlobalVarNames
        {
            //StepRatio,
            //RibbonDistance,
            //TaperFactor,
            //RotationOffset,
            RibbonInfo
        }

        //Variables which are inputs or outputs for formula:
        //private ValidIdentifier stepRatioIdent, ribbonDistanceIdent, taperFactorIdent, rotationOffsetIdent, ribbonInfoIdent;
        private RibbonFormulaInfo ribbonFormulaInfo;

        public void InitFormulaSettings()
        {
            if (FormulaSettings == null)
            {
                FormulaSettings = new FormulaSettings(FormulaTypes.Ribbon, pattern: this);
                ConfigureParser(FormulaSettings.Parser);
                //stepRatioIdent = parser.GetVariableIdentifier(GlobalVarNames.StepRatio.ToString());
                //stepRatioIdent.IsReadOnly = true;
                //ribbonDistanceIdent = parser.GetVariableIdentifier(GlobalVarNames.RibbonDistance.ToString());
                //taperFactorIdent = parser.GetVariableIdentifier(GlobalVarNames.TaperFactor.ToString());
                //rotationOffsetIdent = parser.GetVariableIdentifier(GlobalVarNames.RotationOffset.ToString());
            }
        }

        private void ConfigureParser(ExpressionParser parser)
        {
            ribbonFormulaInfo = new RibbonFormulaInfo();
            foreach (GlobalVarNames varName in Enum.GetValues(typeof(GlobalVarNames)))
            {
                if (varName == GlobalVarNames.RibbonInfo)
                    parser.DeclareVariable(varName.ToString(), ribbonFormulaInfo.GetType(),
                                           initialValue: ribbonFormulaInfo, isGlobal: true, isReadOnly: true);
                else
                    parser.DeclareVariable(varName.ToString(), typeof(double), initialValue: 0D, isGlobal: true);
            }
        }

        public void AddToRibbonPath()
        {
            AddToRibbonPath(new PointF(this.Center.X + (float)ZVector.Re, this.Center.Y + (float)ZVector.Im));
        }

        public PointF AddToRibbonPath(PointF newCenter, bool interpolate = false)
        {
            if (RibbonPath.Count == 0)
                RibbonPath.Add(this.Center);
            else
                this.Center = RibbonPath[RibbonPath.Count - 1];
            if (interpolate && newCenter != Center)
            {
                PointF pDiff = Tools.SubtractPoint(newCenter, Center);
                float distance = (float)Tools.Distance(Center, newCenter);
                float factor = (float)RibbonDistance / distance;
                if (factor != 1F)
                {
                    newCenter = new PointF(Center.X + factor * pDiff.X, Center.Y + factor * pDiff.Y);
                }
            }
            RibbonPath.Add(newCenter);
            return newCenter;
        }

        public bool ScaleRibbonPath()
        {
            bool useFormula = InitForFormula();
            bool fixRibbon = ScaleTaper && RibbonPath.Count > 2;
            if (!fixRibbon)
                return false;
            double taperFac = useFormula ? 0 : 0.01 * TaperPercentage;
            var factors = new double[RibbonPath.Count - 1];
            double currFac = useFormula ? 0 : (taperFac > 0 ? 1 : 1 + taperFac);
            taperFac /= (RibbonPath.Count - 1);
            var newPath = new PointF[RibbonPath.Count];
            newPath[0] = RibbonPath[0];
            double length = Tools.PathLength(RibbonPath);
            for (int i = 0; i < RibbonPath.Count - 1; i++)
            {
                if (useFormula)
                {
                    ribbonFormulaInfo.StepRatio = (double)i / (RibbonPath.Count - 2);
                    if (FormulaSettings.EvalFormula())
                        factors[i] = Math.Abs(ribbonFormulaInfo.TaperFactor); // (double)taperFactorIdent.CurrentValue);
                }
                else
                {
                    factors[i] = currFac;
                    currFac -= taperFac;
                }
            }
            double lengthFactor = length / factors.Sum();
            for (int i = 0; i < RibbonPath.Count - 1; i++)
            {
                int i2 = i + 1;
                var zVec = new Complex(RibbonPath[i2].X - RibbonPath[i].X, RibbonPath[i2].Y - RibbonPath[i].Y);
                zVec *= lengthFactor * factors[i] / zVec.GetModulus();
                var p = new PointF(newPath[i].X + (float)zVec.Re, newPath[i].Y + (float)zVec.Im);
                newPath[i2] = p;
            }
            for (int i = 1; i < RibbonPath.Count; i++)
            {
                RibbonPath[i] = newPath[i];
            }
            return true;
        }

        public void RecomputeRibbonPath()
        {
            var newPath = new PointF[RibbonPath.Count];
            newPath[0] = RibbonPath[0];
            float segmentLength = (float)RibbonDistance;
            for (int i = 0; i < RibbonPath.Count - 1; i++)
            {
                int i2 = i + 1;
                PointF vector = Tools.SubtractPoint(RibbonPath[i2], RibbonPath[i]);
                float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
                float factor = segmentLength / length;
                newPath[i2] = new PointF(newPath[i].X + factor * vector.X, newPath[i].Y + factor * vector.Y);
            }
            for (int i = 1; i < RibbonPath.Count; i++)
            {
                RibbonPath[i] = newPath[i];
            }
        }

        public void FlipPathVertically()
        {
            if (RibbonPath.Count == 0)
                return;
            float offsetX = 2F * RibbonPath.Select(p => p.X).Average();
            for (int i = 0; i < RibbonPath.Count; i++)
            {
                RibbonPath[i] = new PointF(offsetX - RibbonPath[i].X, RibbonPath[i].Y);
            }
        }

        public void FlipPathHorizontally()
        {
            if (RibbonPath.Count == 0)
                return;
            float offsetY = 2F * RibbonPath.Select(p => p.Y).Average();
            for (int i = 0; i < RibbonPath.Count; i++)
            {
                RibbonPath[i] = new PointF(RibbonPath[i].X, offsetY - RibbonPath[i].Y);
            }
        }


        //public bool ScaleRibbonPath()
        //{
        //    bool fixedRibbon = ScaleTaper && TaperPercentage != 0 && RibbonPath.Count > 2;
        //    if (fixedRibbon)
        //    {
        //        double taperFac = 0.01 * Math.Abs(TaperPercentage) / (RibbonPath.Count - 1);
        //        int startI, increment;
        //        if (TaperPercentage > 0)
        //        {
        //            startI = 0;
        //            increment = 1;
        //        }
        //        else
        //        {
        //            startI = RibbonPath.Count - 1;
        //            increment = -1;
        //        }
        //        double length = Tools.PathLength(RibbonPath);
        //        double newLength = 0;
        //        int i = startI;
        //        double currFac = 1;
        //        double segLength = Tools.Distance(RibbonPath[0], RibbonPath[1]);
        //        for (int passes = 0; passes < RibbonPath.Count - 1; passes++)
        //        {
        //            int i2 = i + increment;
        //            newLength += currFac * segLength;
        //            i = i2;
        //            currFac -= taperFac;
        //        }
        //        double lengthFactor = segLength * length / newLength;
        //        i = startI;
        //        currFac = 1;
        //        var newPath = new PointF[RibbonPath.Count];
        //        newPath[startI] = RibbonPath[startI];
        //        for (int passes = 0; passes < RibbonPath.Count - 1; passes++)
        //        {
        //            int i2 = i + increment;
        //            var zVec = new Complex(RibbonPath[i2].X - RibbonPath[i].X, RibbonPath[i2].Y - RibbonPath[i].Y);
        //            zVec *= currFac * lengthFactor / zVec.GetModulus();
        //            var p = new PointF(RibbonPath[i].X + (float)zVec.Re, RibbonPath[i].Y + (float)zVec.Im);
        //            newPath[i2] = p;
        //            i = i2;
        //            currFac -= taperFac;
        //        }
        //        for (i = 0; i < RibbonPath.Count; i++)
        //        {
        //            RibbonPath[i] = newPath[i];
        //        }
        //    }
        //    return fixedRibbon;
        //}

        private ColorBlend colorBlend = null;

        private void InitColorGradient(int segmentsCount)
        {
            var pathFillInfo = FillInfo as PathFillInfo;
            int steps;
            if (SmoothGradient)
                steps = segmentsCount;
            else
            {
                if (segmentsCount <= 1)
                    steps = 1;
                else
                    steps = segmentsCount - 1;
            }
            gradient = new ColorGradient();
            colorBlend = null;
            if (pathFillInfo != null && pathFillInfo.ColorMode == FillInfo.PathColorModes.Radial && 
                pathFillInfo.ColorInfo != null)
            {
                gradient.Initialize(steps, pathFillInfo.ColorInfo.Colors.ToList(), 
                                    pathFillInfo.ColorInfo.Positions.ToList(), CycleColors);
                if (LinearGradientPerSegment)
                {
                    colorBlend = new ColorBlend();
                    colorBlend.Colors = pathFillInfo.ColorInfo.Colors.ToArray();
                    colorBlend.Positions = pathFillInfo.ColorInfo.Positions.ToArray();
                }
            }
            else
            {
                gradient.Initialize(steps, BoundaryColor, CenterColor, CycleColors);
                if (LinearGradientPerSegment)
                {
                    colorBlend = new ColorBlend();
                    colorBlend.Colors = new Color[] { BoundaryColor, CenterColor };
                    colorBlend.Positions = new float[] { 0F, 1F };
                }
            }
        }

        public void InitTaper(int segmentsCount, bool forPoints)
        {
            sizeFactor = 1;
            sizeFactorDecrement = 0;
            penWidthDecrement = 0;
            if (this.TaperPercentage != 0F)
            {
                InitTaper(segmentsCount, 
                          Math.Abs(TaperPercentage / 100F), forPoints);
            }
        }

        private void InitTaper(int segmentsCount, float absTaper, bool forPoints)
        {
            taperFac = absTaper * Math.Sign(TaperPercentage) / segmentsCount;
            if (forPoints)
            {
                int pointsCount = RotationSteps;
                if (ComputeCurvePoints(PatternZVector))
                    pointsCount = CurvePoints.Length;
                //if (PathMode == RibbonPathModes.Path)
                //{
                //  var zVector = new Complex(RibbonDistance, 0);
                //  if (ComputeCurvePoints(PatternZVector))
                //        pointsCount = CurvePoints.Length;
                //}
                sizeFactorDecrement = taperFac / (float)pointsCount;
            }
            penWidthDecrement = fPenWidth * taperFac;
            if (TaperPercentage < 0F)
            {
                sizeFactor = 1F - absTaper;
                fPenWidth *= Math.Abs(sizeFactor);
            }
            else
                sizeFactor = 1F;
        }

        public void InitDraw()
        {
            drawFilled = DrawingMode == RibbonDrawingModes.Fill ||
                         DrawingMode == RibbonDrawingModes.WaveFill ||
                         DrawingMode == RibbonDrawingModes.CopyPattern ||
                         DrawingMode == RibbonDrawingModes.DirectFill ||
                         this.UsesSection;
            if (FillInfo.FillType != FillInfo.FillTypes.Path && FillInfo.FillBrush == null)
                FillInfo.CreateFillBrush();
            useLinearGradient = FillInfo.FillType == FillInfo.FillTypes.Path && 
                                (UseLinearGradient || (SmoothGradient && !drawFilled));
            if (!drawFilled)
            {
                if (RibbonPen == null)
                {
                    if (FillInfo.FillType == FillInfo.FillTypes.Path)
                        RibbonPen = new Pen(this.BoundaryColor);
                    else
                        RibbonPen = new Pen(FillInfo.FillBrush);
                }
                else if (FillInfo.FillType == FillInfo.FillTypes.Texture)
                    RibbonPen.Brush = FillInfo.FillBrush;
                RibbonPen.Width = this.PenWidth;
                lineDrawPoints = new List<PointF>();
                lineSegmentStartIndices = new List<int>();
            }
            fPenWidth = PenWidth;
            taperFac = 0;
            sizeFactor = 1;
        }

        public bool UsesFormula
        {
            get { return FormulaEnabled && FormulaSettings != null && FormulaSettings.IsValid; }
        }

        private bool InitForFormula()
        {
            bool useFormula = UsesFormula;
            if (useFormula)
            {
                ribbonFormulaInfo.TaperFactor = 1D;
                ribbonFormulaInfo.RotationOffset = 0D;

                FormulaSettings.InitializeGlobals();
            }
            return useFormula;
        }

        public override void DrawFilled(Graphics g, IRenderCaller caller, bool computeRandom = true, 
                                        bool draftMode = false, int recursiveDepth = 0,
                                        float textureScale = 1, Complex? patternZVector = null, bool enableCache = true)
        {
            int segmentsCount = RibbonPath.Count - 1;
            if (segmentsCount <= 0)
                return;
            if (!computeRandom)
                this.RandomGenerator.ResetRandom();
            InitDraw();
            InitColorGradient(segmentsCount);
            if (useLinearGradient)
                color1 = gradient.GetCurrentColor();
            bool forPoints = DrawingMode == RibbonDrawingModes.Line;
            InitTaper(segmentsCount, forPoints);
            bool useFormula = segmentsCount > 1 && InitForFormula();
            int startI, increment, i;
            if (DrawReversed)
            {
                startI = segmentsCount;
                increment = -1;
            }
            else
            {
                startI = 0;
                increment = 1;
            }
            i = startI;
            for (int passes = 0; passes < segmentsCount; passes++)
            {
                this.Center = RibbonPath[i];
                var zVec = new Complex(RibbonPath[i + increment].X - RibbonPath[i].X,
                                       RibbonPath[i + increment].Y - RibbonPath[i].Y);
                if (SegmentPadding > 0)
                {
                    zVec *= 1 - SegmentPadding;
                }
                this.ZVector = zVec;
                if (penWidthDecrement != 0F && RibbonPen != null && fPenWidth >= Pattern.MinPenWidth)
                {
                    //int penWidth = (int)Math.Round(fPenWidth);
                    //if (penWidth <= 0)
                    //    penWidth = 1;
                    RibbonPen.Width = fPenWidth;
                }
                if (useFormula)
                {
                    double nextTaperFactor = 0;
                    if (forPoints)
                    {
                        ribbonFormulaInfo.StepRatio = (double)(passes + 1) / (segmentsCount - 1);
                        if (FormulaSettings.EvalFormula())
                            nextTaperFactor = ribbonFormulaInfo.TaperFactor; // (double)taperFactorIdent.CurrentValue;
                    }
                    ribbonFormulaInfo.StepRatio = (double)passes / (segmentsCount - 1);
                    if (FormulaSettings.EvalFormula())
                    {
                        if (forPoints)
                        {
                            double taperFactor = ribbonFormulaInfo.TaperFactor; // (double)taperFactorIdent.CurrentValue;
                            sizeFactor = (float)taperFactor;
                            taperFac = (float)(nextTaperFactor - taperFactor);
                        }
                    }
                }
                int maxInd = (drawFilled && DrawingMode != RibbonDrawingModes.CopyPattern) ? PatternLayers.PatternLayers.Count : 1;
                bool computeRand = true;
                for (int layerInd = 0; layerInd < maxInd; layerInd++)
                {
                    DrawRibbonSegment(g, caller, useFormula, computeRand, layerInd, enableCache, draftMode);
                    computeRand = false;
                }
                fPenWidth = Math.Max(Pattern.MinPenWidth, fPenWidth - penWidthDecrement);
                i += increment;
            }
            if (!drawFilled)
            {
                DrawLinePointsCurve(g);
                lineDrawPoints = null;
                lineSegmentStartIndices = null;
            }
            this.Center = RibbonPath[0]; //saveCenter;
        }

        private bool UseDistinctPoints
        {
            get { return true; } // return PathMode != RibbonPathModes.Segmented; }
        }

        private void DrawRibbonSegment(Graphics g, IRenderCaller caller, bool useFormula, bool computeRandom, int layerIndex = 0, 
                                       bool enableCache = true, bool draftMode = false)
        {
            double zVectorModulus = ZVector.GetModulus();
            Complex unitZVector = ZVector / zVectorModulus;
            Complex patternZVector = PatternZVector;
            if (layerIndex > 0)
                patternZVector *= PatternLayers.PatternLayers[layerIndex].ModulusRatio;
            if (TrackAngle)
                patternZVector = patternZVector * unitZVector;  //Rotate pattern to track angle of path.
            if (useFormula)
            {
                double rotation = ribbonFormulaInfo.RotationOffset; // (double)rotationOffsetIdent.CurrentValue;
                if (rotation != 0)
                {
                    Complex rotationVec = Complex.CreateFromModulusAndArgument(1D, rotation);
                    patternZVector *= rotationVec;
                }
            }
            bool applyPointsTaper = DrawingMode == RibbonDrawingModes.Line;
            if (!applyPointsTaper && (useFormula || taperFac != 0))
            {
                double factor;
                if (useFormula)
                    factor = ribbonFormulaInfo.TaperFactor; // (double)taperFactorIdent.CurrentValue;
                else
                    factor = sizeFactor;
                patternZVector *= factor;
            }
            if (SeedPoints == null || (computeRandom && HasRandomElements))
                ComputeSeedPoints(computeRandom);
            if (DrawingMode == RibbonDrawingModes.DirectFill)
            {
                DrawDirectFilledSegment(g, patternZVector);
            }
            else if (DrawingMode == RibbonDrawingModes.CopyPattern)
            {
                if (CopiedPattern != null)
                    CopiedPattern.DrawFilled(g, caller, computeRandom: false, draftMode: draftMode,
                                             patternZVector: patternZVector, enableCache: enableCache);
                else
                    base.DrawFilled(g, caller, computeRandom: false, draftMode: draftMode,
                                    patternZVector: patternZVector, enableCache: enableCache);
            }
            else if (ComputeCurvePoints(patternZVector))
            {
                PointF[] distinctPoints = CurvePoints;
                TransformPoints(distinctPoints, zVectorModulus, unitZVector, applyPointsTaper);
                if (drawFilled)
                {
                    DrawFilledSegment(g, distinctPoints, unitZVector, layerIndex);
                }
                else if (useLinearGradient)
                {
                    DrawLinearGradientSegment(g, distinctPoints, unitZVector);
                }
                else
                {
                    AddLineFillPoints(distinctPoints);
                    //DrawCurveWithGradient(g, distinctPoints);
                }
            }
            if (!applyPointsTaper)
            {
                sizeFactor -= taperFac;
            }
        }

        private void DrawLinePointsCurve(Graphics g)
        {
            int pointsCount = lineDrawPoints.Count;
            if (pointsCount == 0)
                return;
            if (FillInfo.FillType == FillInfo.FillTypes.Texture)
            {
                Tools.DrawCurve(g, RibbonPen, lineDrawPoints.ToArray());
            }
            else if (SmoothGradient)
            {
                //InitColorGradient(pointsCount);
                //for (int i = 1; i < pointsCount; i++)
                //{
                //    RibbonPen.Color = gradient.GetCurrentColor();
                //    g.DrawLine(RibbonPen, lineDrawPoints[i - 1], lineDrawPoints[i]);
                //}
            }
            else
            {
                for (int i = 1; i <= lineSegmentStartIndices.Count; i++)
                {
                    int iStart = lineSegmentStartIndices[i - 1];
                    int iEnd = i < lineSegmentStartIndices.Count ? lineSegmentStartIndices[i] : pointsCount;
                    if (iStart > 0)
                        iStart--;
                    PointF[] segmentPoints = lineDrawPoints.Skip(iStart).Take(iEnd - iStart).ToArray();
                    RibbonPen.Color = gradient.GetCurrentColor();
                    Tools.DrawCurve(g, RibbonPen, segmentPoints);
                }
            }
        }

        private void DrawDirectFilledSegment(Graphics g, Complex patternZVector)
        {
            float minAmp = DirectFillAmpOffset + SeedPoints[MinPointIndex].Modulus;
            float minAngle = SeedPoints.Select(sp => sp.Angle).Min();
            float maxAngle = SeedPoints.Select(sp => sp.Angle).Max();
            float startAngle = SeedPoints[MinPointIndex].Angle - DirectFillAngleOffset;
            while (startAngle < 0)
                startAngle += 2F * (float)Math.PI;
            PointF p = Center;
            List<PointF> points = new List<PointF>();
            points.Add(p);
            var points2 = new List<PointF>();
            points2.Add(p);
            float factor = 1F / (maxAngle - minAngle);
            PointF vecP = new PointF((float)ZVector.Re, (float)ZVector.Im);
            PointF fPatternVec = new PointF((float)patternZVector.Re, (float)patternZVector.Im);
            float prevAngle = 0;
            for (int i = MinPointIndex + 1; i < MinPointIndex + SeedPoints.Length; i++)
            {
                PolarCoord pc = SeedPoints[i % SeedPoints.Length];
                float angle = pc.Angle - startAngle;
                if (angle < 0 && prevAngle > (float)Math.PI)
                {
                    while (angle < 0)
                        angle += 2F * (float)Math.PI;
                }
                float aFac = angle * factor;
                p = new PointF(Center.X + aFac * vecP.X, Center.Y + aFac * vecP.Y);
                float amp = pc.Modulus - minAmp;
                PointF newP = new PointF(p.X + amp * (float)patternZVector.Re, p.Y + amp * (float)patternZVector.Im);
                //if (i == MinPointIndex + SeedPoints.Length - 1)
                //{
                //    Debug.WriteLine($"End: newP = {newP}; amp = {amp}");
                //}
                if (Tools.PointsDiffer(newP, points.Last()))
                {
                    points.Add(newP);
                    PointF newP2 = new PointF(p.X - amp * fPatternVec.X, p.Y - amp * fPatternVec.Y);
                    points2.Add(newP2);
                }
                prevAngle = angle;
            }
            points2.Reverse();
            points.AddRange(points2);
            PointF[] distinctPoints = points.ToArray();
            InitRibbonBrush(distinctPoints);
            FillCurvePoints(g, distinctPoints);
        }

        private void GetTransformPointsInfo(int maxI, double zVectorModulus, Complex unitZVector, out PointF offset, out PointF offsetInc)
        {
            float fac = (float)zVectorModulus / maxI;
            offsetInc = new PointF(fac * (float)unitZVector.Re, fac * (float)unitZVector.Im);
            //if (DrawReversed)
            //{
            //    offset = new PointF(offsetInc.X * maxI, offsetInc.Y * maxI);
            //    offsetInc = new PointF(-offsetInc.X, -offsetInc.Y);
            //}
            //else
            //{
                offset = new PointF(0, 0);
            //}
        }

        private void TransformPoints(PointF[] distinctPoints, double zVectorModulus,
                                     Complex unitZVector, bool applyTaper = true)
        {
            int maxI;
            if (this.UsesSection)
            {
                maxI = this.InnerSectionIndex;
                //applyTaper = false;
            }
            else
                maxI = distinctPoints.Length;
            sizeFactorDecrement = taperFac / maxI;
            PointF offset, offsetInc;
            GetTransformPointsInfo(maxI, zVectorModulus, unitZVector, out offset, out offsetInc);
            if (applyTaper && sizeFactor == 1 && sizeFactorDecrement == 0D)
                applyTaper = false;
            for (int i = 1; i < maxI; i++)
            {
                offset.X += offsetInc.X;
                offset.Y += offsetInc.Y;
                if (applyTaper)
                {
                    distinctPoints[i].X = sizeFactor * (distinctPoints[i].X - Center.X)
                                          + Center.X;
                    distinctPoints[i].Y = sizeFactor * (distinctPoints[i].Y - Center.Y)
                                          + Center.Y;
                    sizeFactor -= sizeFactorDecrement;
                }
                distinctPoints[i].X += offset.X;
                distinctPoints[i].Y += offset.Y;
                //RibbonPen.Color = gradient.GetCurrentColor();
                //g.DrawLine(RibbonPen, curvePoints[i - 1], curvePoints[i]);
            }
            if (this.UsesSection)
            {
                GetTransformPointsInfo(distinctPoints.Length - maxI, zVectorModulus, unitZVector, out offset, out offsetInc);
                //fac = (float)zVectorModulus / (distinctPoints.Length - maxI);
                //offsetInc = new PointF(fac * (float)unitZVector.Re,
                //                       fac * (float)unitZVector.Im);
                //offset = new PointF(0, 0);
                float sizeFactor1 = sizeFactor;
                sizeFactorDecrement = taperFac / (distinctPoints.Length - maxI);
                for (int i = distinctPoints.Length - 2; i >= maxI; i--)
                {
                    offset.X += offsetInc.X;
                    offset.Y += offsetInc.Y;
                    if (applyTaper)
                    {
                        distinctPoints[i].X = sizeFactor1 * (distinctPoints[i].X - Center.X)
                                              + Center.X;
                        distinctPoints[i].Y = sizeFactor1 * (distinctPoints[i].Y - Center.Y)
                                              + Center.Y;
                        sizeFactor1 += sizeFactorDecrement;
                    }
                    distinctPoints[i].X += offset.X;
                    distinctPoints[i].Y += offset.Y;
                }
            }
        }

        private void DrawFilledSegment(Graphics g, PointF[] distinctPoints, 
                                       Complex unitZVector, int layerIndex)
        {
            if (DrawingMode == RibbonDrawingModes.WaveFill)
            {
                List<PointF> pointsList = distinctPoints.ToList();
                Complex pCenter = new Complex(Center.X, Center.Y);
                var zRotate = new Complex(unitZVector.Re, -unitZVector.Im);
                for (int i = pointsList.Count - 1; i >= 0; i--)
                {
                    var p = new Complex(pointsList[i].X, pointsList[i].Y) - pCenter;
                    //p /= unitZVector;
                    p *= zRotate;
                    double a = (double)i * Math.PI / (double)pointsList.Count;
                    double waveFac = Math.Sin(a);
                    p.Im *= waveFac;
                    Complex pNew = new Complex(p);
                    pNew.Im = -p.Im;
                    p *= unitZVector;
                    pNew *= unitZVector;
                    pointsList[i] = new PointF((float)p.Re + Center.X,
                                                 (float)p.Im + Center.Y);
                    pointsList.Add(new PointF((float)pNew.Re + Center.X,
                                                (float)pNew.Im + Center.Y));
                }
                distinctPoints = Tools.DistinctPoints(pointsList.ToArray()).ToArray();
            }
            else if (!UseDistinctPoints)
                distinctPoints = Tools.DistinctPoints(distinctPoints).ToArray();
            if (distinctPoints.Length <= 3)
                return;
            distinctPoints[distinctPoints.Length - 1] = distinctPoints[0];
            if (useLinearGradient)
            {
                LinearGradientBrush linearGradientBrush = CreateLinearGradientBrush(distinctPoints, unitZVector);
                if (linearGradientBrush != null)
                {
                    using (linearGradientBrush)
                    {
                        FillCurvePoints(g, distinctPoints, linearGradientBrush);
                    }
                }
            }
            else
            {
                var fillInfo = PatternLayers.PatternLayers[layerIndex].FillInfo;
                InitRibbonBrush(distinctPoints, fillInfo);
                FillCurvePoints(g, distinctPoints, fillInfo: fillInfo);
            }
        }


        private List<int> GetSegmentStartIndices(PointF[] points, 
                                                 double ribbonDistance,
                                                 out double segmentsCount,
                                                 out double scale,
                                                 double dSizeFactor = 1D,
                                                 double dTaperFac = 0D)
        {
            const double tolerance = 0.999;
            List<int> segmentStartIndices;
            scale = 1;
            if (points.Length <= 1)
            {
                segmentsCount = 0;
                return new List<int>();
            }
            if (dTaperFac == 0D)
            {
                double length = Tools.PathLength(points);
                segmentsCount = Math.Round(length / (dSizeFactor * ribbonDistance));
                if (segmentsCount < (double)MinSegments)
                {
                    scale = segmentsCount / (double)MinSegments;
                    segmentsCount = (double)MinSegments;
                }
                double segLength = length / segmentsCount;
                segmentStartIndices = new List<int>();
                double curSegLength = 0;
                int startInd = 0;
                for (int ind = 1; ind < points.Length; ind++)
                {
                    curSegLength += Tools.Distance(points[ind - 1], points[ind]);
                    if (curSegLength >= segLength)
                    {
                        length -= curSegLength - segLength;
                        segLength = length / segmentsCount;
                        segmentStartIndices.Add(startInd);
                        startInd = ind;
                        curSegLength = 0;
                    }
                }
                if (curSegLength >= 0.5 * segLength)
                    segmentStartIndices.Add(startInd);
                //double segLen = (double)points.Length / segmentsCount;
                //segmentStartIndices = Enumerable.Range(0, (int)segmentsCount).Select(i => (int)(segLen * i)).ToList();
                return segmentStartIndices;
            }
            segmentStartIndices = new List<int>();
            int startIndex = 0;
            PointF startPoint = points[0];
            double curDistanceSquared = 0;
            double curRibbonDist = dSizeFactor * ribbonDistance;
            double ribbonDistInc = -dTaperFac * ribbonDistance;
            double maxDistanceSquared = tolerance * curRibbonDist * curRibbonDist;
            for (int i = 1; i < points.Length; i++)
            {
                curDistanceSquared = Tools.DistanceSquared(startPoint, points[i]);
                if (curDistanceSquared >= maxDistanceSquared)
                {
                    segmentStartIndices.Add(startIndex);
                    startIndex = i;
                    startPoint = points[startIndex];
                    curDistanceSquared = 0;
                    curRibbonDist += ribbonDistInc;
                    maxDistanceSquared = tolerance * curRibbonDist * curRibbonDist;
                }
            }
            segmentsCount = (double)segmentStartIndices.Count +
                            Math.Sqrt(curDistanceSquared) / ribbonDistance;
            return segmentStartIndices;
        }

        public float ComputeTaper(float segmentsCount, out int nSeg)
        {
            float absTaper = Math.Abs(TaperPercentage) / 100F;
            float n = segmentsCount / (1F - 0.5F * absTaper) - 1F;
            nSeg = (int)Math.Ceiling(n);
            absTaper = 2F - 2F * segmentsCount / ((float)nSeg + 1F);
            return absTaper;
        }

        private PathPattern pathPattern;

        public void DrawForPathPattern(Graphics g, IRenderCaller caller, PathPattern pathPattern, 
                                       PointF center, Complex zVector, bool computeRandom, 
                                       bool enableCache = true, bool draftMode = false)
        {
            this.pathPattern = pathPattern;
            PointF[] points = pathPattern.CurvePoints;
            if (points == null || points.Length == 0)
                return;
            if (DrawReversed)
            {
                points = points.Reverse().ToArray();
            }
            bool useFormula = InitForFormula();
            List<int> vertexIndices = null;
            bool fitToPathVertices = this.FitToPathVertices
                                  && pathPattern.CurveVertexIndices != null
                                  && pathPattern.CurveVertexIndices.Length > 0
                                  && TaperPercentage == 0F;
            if (fitToPathVertices)
            {
                int firstI = pathPattern.CurveVertexIndices[0];
                if (firstI > 0)
                {
                    //Shift the array of points:
                    var firstPoints = points.Take(firstI);
                    points = points.Skip(firstI).Concat(firstPoints).ToArray();
                    //Make vertexIndices start at 0:
                    vertexIndices = pathPattern.CurveVertexIndices.Select(i => i - firstI).ToList();
                }
                else
                {
                    vertexIndices = pathPattern.CurveVertexIndices.ToList();
                }
            }
            InitDraw();
            double drawnRibbonDistance = RibbonDistance;
            double dSegmentsCount;
            Complex patternZVector = this.PatternZVector;
            List<int> segmentStartIndices = null;
            List<double> vertexRibbonDistances = null;
            if (fitToPathVertices)
            {
                fitToPathVertices = FitSegments(points, vertexIndices, drawnRibbonDistance,
                                                out vertexRibbonDistances, out segmentStartIndices);
                if (segmentStartIndices.Count - 1 < MinSegments)
                {
                    double scale = Math.Max((double)(segmentStartIndices.Count - 1), 1D) / MinSegments;
                    patternZVector *= scale;
                    drawnRibbonDistance *= scale;
                    fitToPathVertices = FitSegments(points, vertexIndices, drawnRibbonDistance,
                                                    out vertexRibbonDistances, out segmentStartIndices);
                }
            }
            if (!fitToPathVertices)
            {
                double scale;
                segmentStartIndices = GetSegmentStartIndices(points, drawnRibbonDistance, 
                                                             out dSegmentsCount, out scale);
                if (scale != 1D)
                {
                    patternZVector *= scale;
                    drawnRibbonDistance *= scale;
                    if (dSegmentsCount < (double)MinSegments)
                    {
                        segmentStartIndices =
                            GetSegmentStartIndices(points, drawnRibbonDistance, out dSegmentsCount, out scale);
                    }
                }
                if (TaperPercentage != 0F)
                {
                    drawnRibbonDistance *= dSegmentsCount / segmentStartIndices.Count;
                    int segmentCount;
                    float absTaper = ComputeTaper((float)dSegmentsCount, out segmentCount);
                    InitTaper(segmentCount, absTaper, forPoints: !drawFilled);
                    segmentStartIndices =
                        GetSegmentStartIndices(points, drawnRibbonDistance,
                                               out dSegmentsCount, out scale, sizeFactor, taperFac);
                    for (int passes = 1;
                            passes <= 10 &&
                            dSegmentsCount > (double)MinSegments - 0.5 &&
                            Math.Abs(dSegmentsCount - (double)segmentStartIndices.Count) > 0.001;
                            passes++)
                    {
                        //Debug.WriteLine($"passes = {passes}; dSegmentsCount = {dSegmentsCount}; segment count = {segmentStartIndices.Count}");
                        drawnRibbonDistance *= dSegmentsCount / segmentStartIndices.Count;
                        segmentStartIndices =
                            GetSegmentStartIndices(points, drawnRibbonDistance, out dSegmentsCount, out scale,
                                                    sizeFactor, taperFac);
                    }
                }
                else
                {
                    sizeFactor = 1F;
                    taperFac = 0F;
                }
                if (segmentStartIndices.Count == 0)
                    segmentStartIndices.Add(0);
                segmentStartIndices.Add(points.Length - 1);
            }
            if (!computeRandom)
                this.RandomGenerator.ResetRandom();
            currRibbonDistance = drawnRibbonDistance;
            if (drawFilled)
                currRibbonDistance *= sizeFactor;
            if (VertexPadding > 0 && vertexIndices != null)
            {
                double padLength = VertexPadding * RibbonDistance;
                PointF[] newPoints = (PointF[])points.Clone();
                for (int i = 0; i < vertexIndices.Count; i++)
                {
                    int startInd = vertexIndices[i];
                    int endInd = i + 1 < vertexIndices.Count ? vertexIndices[i + 1] : points.Length - 1;
                    double sectionLength = Tools.PathLength(points, startInd, endInd - startInd);
                    float scale = (float)(sectionLength / (padLength + sectionLength));
                    for (int j = endInd - 1; j > startInd; j--)
                    {
                        PointF pDiff = Tools.SubtractPoint(points[j], points[j + 1]);
                        PointF pNew = newPoints[j + 1];
                        newPoints[j] = new PointF(pNew.X + scale * pDiff.X, pNew.Y + scale * pDiff.Y);
                    }
                }
                points = newPoints;
                newPoints = null;
            }
            int vertInd = -1;
            //if (!fitToPathVertices)
            //    useFormula = false;
            int edgeLength = 0;
            //if (fitToPathVertices)
            //{
            //    Debug.WriteLine("ZVector=" + ZVector.ToString());
            //    Debug.WriteLine("vertexRibbonDistances=" + string.Join(",", vertexRibbonDistances.Select(d => d.ToString("0.00000"))));
            //}
            if (useLinearGradient || !drawFilled)
            {
                InitColorGradient(segmentStartIndices.Count);
                color1 = gradient.GetCurrentColor();
            }
            if (DrawingMode == RibbonDrawingModes.CopyPattern)
                ComputeSeedPoints();
            for (int i = 1; i < segmentStartIndices.Count; i++)
            {
                int startI = segmentStartIndices[i - 1];
                int nextI = segmentStartIndices[i];
                if (fitToPathVertices)
                {
                    currRibbonDistance = (1.0 - SegmentPadding) * vertexRibbonDistances[i - 1];
                    if (useFormula || VertexPadding > 0)
                    {
                        int prevVertInd = vertInd;
                        while (vertInd < vertexIndices.Count - 1 && startI >= vertexIndices[vertInd + 1])
                        {
                            vertInd++;
                        }
                        if (vertInd != prevVertInd)
                        {
                            if (VertexPadding > 0)
                                startI++;
                            if (useFormula)
                            {
                                edgeLength = (vertInd < vertexIndices.Count - 1 ? vertexIndices[vertInd + 1] : points.Length)
                                           - vertexIndices[vertInd];
                            }
                        }
                    }
                }
                if (useFormula)
                {
                    if (edgeLength > 0)
                        ribbonFormulaInfo.StepRatio = (double)(startI - vertexIndices[vertInd]) / edgeLength;
                    else
                        ribbonFormulaInfo.StepRatio = (double)startI / points.Length;
                    FormulaSettings.EvalFormula();
                }
                if (SegmentPadding > 0)
                {
                    startI = startI + (int)(SegmentPadding * (nextI - startI));
                }
                int maxInd = (drawFilled && DrawingMode != RibbonDrawingModes.CopyPattern) ? 
                             PatternLayers.PatternLayers.Count : 1;
                bool computeRand = true;
                for (int layerInd = 0; layerInd < maxInd; layerInd++)
                {
                    DrawPathPatternSegment(g, caller, points, startI, nextI,
                                           patternZVector, useFormula, computeRand, layerInd, enableCache, draftMode);
                    computeRand = false;
                }
                if (drawFilled && taperFac != 0)
                {
                    sizeFactor -= taperFac;
                    currRibbonDistance = drawnRibbonDistance * sizeFactor;
                }
            }
            if (lineDrawPoints != null && lineDrawPoints.Count > 0)
            {
                DrawLinePathSegments(g, points, pathPattern, center, zVector);
                lineDrawPoints = null;
                lineSegmentStartIndices = null;
            }
        }

        private bool FitSegments(PointF[] points, List<int> vertexIndices, double drawnRibbonDistance,
                                 out List<double> vertexRibbonDistances,
                                 out List<int> segmentStartIndices)
        {
            bool success = true;
            vertexRibbonDistances = new List<double>();
            segmentStartIndices = new List<int>();
            if (points.Length == 0)
                return false;
            //double totalDistance = 0;
            //for (int i = 0; i < vertexIndices.Count; i++)
            //{
            //    int ind1 = vertexIndices[i];
            //    PointF p1 = points[ind1];
            //    int ind2 = (i + 1 == vertexIndices.Count) ? points.Length - 1 : vertexIndices[i + 1];
            //    PointF p2 = points[ind2];
            //    totalDistance += Tools.Distance(p1, p2);
            //}
            //double  dSegmentsCount = totalDistance / drawnRibbonDistance;
            //if (dSegmentsCount < MinSegments)
            //{
            //    double scale = Math.Max(dSegmentsCount, 1D) / MinSegments;
            //    patternZVector *= scale;
            //    drawnRibbonDistance *= scale;
            //}
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                int ind1 = vertexIndices[i];
                PointF p1 = points[ind1];
                int ind2;
                if (i == vertexIndices.Count - 1)
                    ind2 = points.Length - 1;
                else
                    ind2 = Math.Max(0, Math.Min(points.Length - 1, vertexIndices[i + 1]));
                PointF p2 = points[ind2];
                double distance = Tools.Distance(p1, p2);
                int numSegs = (int)Math.Round(distance / drawnRibbonDistance);
                if (numSegs == 0)
                {
                    success = false;
                    numSegs = 1;
                }
                double fitDistance = distance / numSegs;
                segmentStartIndices.Add(ind1);
                vertexRibbonDistances.Add(fitDistance);
                int count = 0;
                for (int j = ind1 + 1; j < ind2 && count < numSegs - 1; j++)
                {
                    p2 = points[j];
                    if (fitDistance <= Tools.Distance(p1, p2) + 0.01)
                    {
                        p1 = p2;
                        segmentStartIndices.Add(j);
                        vertexRibbonDistances.Add(fitDistance);
                        count++;
                    }
                }
                //int indDiff = (ind2 - ind1) / numSegs;
                //for (int j = 0; j < numSegs; j++)
                //{
                //    vertexRibbonDistances.Add(fitDistance);
                //    segmentStartIndices.Add(ind1 + j * indDiff);
                //}
            }
            segmentStartIndices.Add(points.Length - 1);
            return success;
        }

        private void DrawLinePathSegments(Graphics g, PointF[] points, PathPattern pathPattern,
                                          PointF center, Complex zVector)
        {
            if (DrawingMode == RibbonDrawingModes.LineFill)
            {
                PointF[] fillPoints = lineDrawPoints.ToArray();
                if (fillPoints.Length > 2)
                {
                    fillPoints[fillPoints.Length - 1] = fillPoints[0];
                    InitRibbonBrush(fillPoints);
                    FillCurvePoints(g, fillPoints); //, initBrush: true);
                }
            }
            else if (DrawingMode == RibbonDrawingModes.Line)
            {
                DrawLinePointsCurve(g);
            }
        }

        private void DrawPathPatternSegment(Graphics g, IRenderCaller caller, PointF[] points, 
                int startPathInd, int endPathInd, /* double pathLength, */
                    Complex patternZVector, bool useFormula, bool computeRandom, int layerIndex, bool enableCache = true,
                    bool draftMode = false)
        {
            if (layerIndex > 0)
                patternZVector *= PatternLayers.PatternLayers[layerIndex].ModulusRatio;
            Center = points[startPathInd];
            ZVector = new Complex(points[endPathInd].X - points[startPathInd].X,
                                  points[endPathInd].Y - points[startPathInd].Y);
            if (drawFilled && (useFormula || sizeFactor != 1F))
            {
                double factor;
                if (useFormula)
                    factor = ribbonFormulaInfo.TaperFactor; // (double)taperFactorIdent.CurrentValue;
                else
                    factor = sizeFactor;
                patternZVector *= factor;
            }
            double zVectorModulus = ZVector.GetModulus();
            if (zVectorModulus == 0)
            {
                ZVector = new Complex(1, 0);
                zVectorModulus = 1;
            }
            Complex unitZVector = ZVector / zVectorModulus;
            if (TrackAngle)
            {
                //if (DrawingMode == RibbonDrawingModes.CopyPattern)
                //{
                //    Complex unitMaxVector = Complex.CreateFromModulusAndArgument(
                //                            1D, MaxPoint.Angle);
                //    unitMaxVector.Im = -unitMaxVector.Im;
                //    Complex unitZVec = unitZVector;
                //    double patternMod = patternZVector.GetModulus();
                //    double dSqr0 = patternMod * patternMod;
                //    int i = startPathInd + 1;
                //    float dX, dY;
                //    while (i < points.Length)
                //    {
                //        dX = points[i].X - Center.X;
                //        dY = points[i].Y - Center.Y;
                //        if (dX * dX + dY * dY >= dSqr0)
                //        {
                //            unitZVec = new Complex(dX, dY);
                //            unitZVec.Normalize();
                //            break;
                //        }
                //        i++;
                //    }
                //    patternZVector = patternZVector.GetModulus() *
                //                     (unitZVec * unitMaxVector);
                //}
                //else
                    patternZVector = patternZVector * unitZVector;  //Rotate pattern to track angle of path.
            }
            if (useFormula)
            {
                double rotation = ribbonFormulaInfo.RotationOffset; // (double)rotationOffsetIdent.CurrentValue;
                if (rotation != 0)
                {
                    Complex rotationVec = Complex.CreateFromModulusAndArgument(1D, rotation);
                    patternZVector *= rotationVec;
                }
            }
            if (computeRandom && HasRandomElements)
                ComputeSeedPoints(computeRandom);
            if (DrawingMode == RibbonDrawingModes.CopyPattern)
            {
                if (CopiedPattern != null)
                    CopiedPattern.DrawFilled(g, caller, computeRandom: false, draftMode: draftMode,
                                             patternZVector: patternZVector, enableCache: enableCache);
                else
                    base.DrawFilled(g, caller, computeRandom: false, draftMode: draftMode, 
                                patternZVector: patternZVector, enableCache: enableCache);
            }
            else if (ComputeCurvePoints(patternZVector))
            {
                //List<PointF> distinctList = GetDistinctPoints();
                //if (distinctList.Count == 0)
                //    return;
                PointF[] distinctPoints = CurvePoints;  //distinctList.ToArray();
                //distinctList = null;
                if (drawFilled)
                {
                    if (DrawingMode != RibbonDrawingModes.CopyPattern)
                        TransformPoints(distinctPoints, zVectorModulus, unitZVector,
                                        applyTaper: false);
                }
                else
                {
                    float fac = (float)(endPathInd - startPathInd) / (float)distinctPoints.Length;
                    PointF offset = new PointF(0, 0);
                    for (int i = 1; i < distinctPoints.Length; i++)
                    {
                        float facInd = fac * i;
                        float addFac = facInd - (float)Math.Floor(facInd);
                        int pathInd = Math.Min(points.Length - 1, startPathInd + 1 + (int)facInd);
                        PointF pDiff = new PointF(points[pathInd].X - points[pathInd - 1].X,
                                                  points[pathInd].Y - points[pathInd - 1].Y);
                        distinctPoints[i].X += addFac * pDiff.X + points[pathInd - 1].X - Center.X;
                        distinctPoints[i].Y += addFac * pDiff.Y + points[pathInd - 1].Y - Center.Y;
                        if (sizeFactorDecrement != 0D)
                        {
                            distinctPoints[i].X = sizeFactor * (distinctPoints[i].X - Center.X)
                                                  + Center.X;
                            distinctPoints[i].Y = sizeFactor * (distinctPoints[i].Y - Center.Y)
                                                  + Center.Y;
                            sizeFactor -= sizeFactorDecrement;
                        }
                    }
                    //if (pathLength < currRibbonDistance)
                    //{
                    //    int pointsCount = (int)(pathLength / currRibbonDistance 
                    //                            * distinctPoints.Length);
                    //    if (pointsCount < distinctPoints.Length)
                    //        distinctPoints = distinctPoints.Take(pointsCount).ToArray();
                    //}
                }
                if (drawFilled)
                {
                    DrawFilledSegment(g, distinctPoints, unitZVector, layerIndex);
                }
                else if (useLinearGradient)
                {
                    DrawLinearGradientSegment(g, distinctPoints, unitZVector);
                }
                else
                {
                    AddLineFillPoints(distinctPoints);
                }
            }
        }

        private LinearGradientBrush CreateLinearGradientBrush(PointF[] distinctPoints, Complex unitZVector)
        {
            LinearGradientBrush linearGradientBrush;
            if (distinctPoints.Length >= 2)
            {
                var zRotation = new Complex(1, 0) / unitZVector;
                var pRotation = new PointF((float)zRotation.Re, (float)zRotation.Im);
                PointF pDelta = distinctPoints[0];
                var points1 = Tools.TranslatePoints(distinctPoints, new PointF(-pDelta.X, -pDelta.Y));
                var rotatedPoints = Tools.RotatePoints(points1, pRotation).OrderBy(p => p.X);
                PointF[] bounds = { rotatedPoints.First(), rotatedPoints.Last() };
                bounds[0].X -= GradientPadding;
                bounds[1].X += GradientPadding;
                pRotation = new PointF((float)unitZVector.Re, (float)unitZVector.Im);
                var bounds2 = Tools.TranslatePoints(Tools.RotatePoints(bounds, pRotation), pDelta);
                PointF pMin = bounds2.First();
                PointF pMax = bounds2.Last();
                Color color2 = colorBlend == null ? gradient.GetCurrentColor() : color1;
                linearGradientBrush = new LinearGradientBrush(pMin, pMax, color1, color2);
                if (colorBlend != null)
                    linearGradientBrush.InterpolationColors = colorBlend;
                color1 = color2;
            }
            else
            {
                linearGradientBrush = null;
            }
            return linearGradientBrush;
        }

        private void DrawLinearGradientSegment(Graphics g, PointF[] distinctPoints, Complex unitZVector)
        {
            LinearGradientBrush linearGradientBrush = CreateLinearGradientBrush(distinctPoints, unitZVector);
            if (linearGradientBrush != null)
            {
                using (linearGradientBrush)
                {
                    using (var pen = new Pen(linearGradientBrush, PenWidth))
                    {
                        g.DrawLines(pen, distinctPoints);
                    }
                }
            }
        }

        private void AddLineFillPoints(PointF[] distinctPoints)
        {
            lineSegmentStartIndices.Add(lineDrawPoints.Count);
            lineDrawPoints.AddRange(distinctPoints);
        }

        public override void DrawOutline(Graphics g, Color? color = null)
        {
            PointF p2 = new PointF(Center.X + (float)OutlineZVector.Re, 
                                   Center.Y + (float)OutlineZVector.Im);
            SetOutlinePen(color);
            g.DrawLine(outlinePen, Center, p2);
        }

        public override void DrawSelectionOutline(Graphics g, PointF? center = null)
        {
            if (this.RibbonPath.Count < 2)
                return;
            Color color = Tools.InverseColor(this.BoundaryColor);
            SetOutlinePen(color);
            Tools.DrawCurve(g, outlinePen, this.RibbonPath.ToArray());
            Tools.DrawSquare(g, color, this.Center);
        }

        public void CopyRibbonPath(Ribbon sourceRibbon)
        {
            this.RibbonPath.Clear();
            this.RibbonPath.AddRange(sourceRibbon.RibbonPath);
        }

        public void MoveRibbonTo(PointF newCenter)
        {
            PointF delta = new PointF(newCenter.X - Center.X, newCenter.Y - Center.Y);
            Center = newCenter;
            TranslateRibbonPath(delta);
        }

        public void TranslateRibbonPath(PointF delta)
        {
            for (int i = 0; i < RibbonPath.Count; i++)
            {
                RibbonPath[i] = new PointF(RibbonPath[i].X + delta.X,
                                           RibbonPath[i].Y + delta.Y);
            }
        }


        protected override void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            xmlTools.AppendAttributeChildNode(parentNode, nameof(PenWidth), PenWidth);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(RibbonDistance), RibbonDistance);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(VertexPadding), VertexPadding);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(SegmentPadding), SegmentPadding);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(TaperPercentage), TaperPercentage);
            //xmlTools.AppendAttributeChildNode(parentNode, nameof(UseLinearGradient), UseLinearGradient);
            //xmlTools.AppendAttributeChildNode(parentNode, nameof(GradientPadding), GradientPadding);
            if (DirectFillAngleOffset != 0)
                xmlTools.AppendAttributeChildNode(parentNode, nameof(DirectFillAngleOffset), DirectFillAngleOffset);
            if (DirectFillAmpOffset != 0)
                xmlTools.AppendAttributeChildNode(parentNode, nameof(DirectFillAmpOffset), DirectFillAmpOffset);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(ScaleTaper), ScaleTaper);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(DrawReversed), DrawReversed);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(MinSegments), MinSegments);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(TrackAngle), TrackAngle);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(CycleColors), CycleColors);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(SmoothGradient), SmoothGradient);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(LinearGradientPerSegment), LinearGradientPerSegment);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(FitToPathVertices), FitToPathVertices);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(DrawingMode), DrawingMode);
            xmlTools.AppendAttributeChildNode(parentNode, nameof(FormulaEnabled), FormulaEnabled);
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(PatternZVector), PatternZVector));
            if (CopiedPattern != null)
                CopiedPattern.ToXml(parentNode, xmlTools, nameof(CopiedPattern));
            if (FormulaSettings != null)
                FormulaSettings.ToXml(parentNode, xmlTools, nameof(FormulaSettings));
            XmlNode childNode = xmlTools.CreateXmlNode(nameof(RibbonPath));
            foreach (PointF p in RibbonPath)
            {
                childNode.AppendChild(xmlTools.CreateXmlNode("Point", p));
            }
            parentNode.AppendChild(childNode);
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "Ribbon";
            return base.ToXml(parentNode, xmlTools, xmlNodeName);
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            bool retVal = true;

            switch (node.Name)
            {
                case "PenWidth":
                    this.PenWidth = Tools.GetXmlAttribute<float>(node);
                    break;
                case "RibbonDistance":
                    this.RibbonDistance = Tools.GetXmlAttribute<double>(node);
                    break;
                case "VertexPadding":
                    this.VertexPadding = Tools.GetXmlAttribute<double>(node);
                    break;
                case "SegmentPadding":
                    this.SegmentPadding = Tools.GetXmlAttribute<double>(node);
                    break;
                case "TaperPercentage":
                    this.TaperPercentage = Tools.GetXmlAttribute<float>(node);
                    break;
                case nameof(UseLinearGradient):
                    this.UseLinearGradient = Tools.GetXmlAttribute<bool>(node);
                    break;
                case nameof(SmoothGradient):
                    SmoothGradient = Tools.GetXmlAttribute<bool>(node);
                    break;
                case nameof(LinearGradientPerSegment):
                    LinearGradientPerSegment = Tools.GetXmlAttribute<bool>(node);
                    break;
                case nameof(GradientPadding):
                    this.GradientPadding = Tools.GetXmlAttribute<float>(node);
                    break;
                case "DirectFillAngleOffset":
                    this.DirectFillAngleOffset= Tools.GetXmlAttribute<float>(node);
                    break;
                case "DirectFillAmpOffset":
                    this.DirectFillAmpOffset = Tools.GetXmlAttribute<float>(node);
                    break;
                case "MinSegments":
                    this.MinSegments = Tools.GetXmlAttribute<int>(node);
                    break;
                case "TrackAngle":
                    this.TrackAngle = Tools.GetXmlAttribute<bool>(node);
                    break;
                case "CycleColors":
                    this.CycleColors = Tools.GetXmlAttribute<bool>(node);
                    break;
                case "FitToPathVertices":
                    this.FitToPathVertices = Tools.GetXmlAttribute<bool>(node);
                    break;
                case "ScaleTaper":
                    this.ScaleTaper = Tools.GetXmlAttribute<bool>(node);
                    break;
                case "DrawReversed":
                    this.DrawReversed = Tools.GetXmlAttribute<bool>(node);
                    break;
                case "PatternZVector":
                    this.PatternZVector = Tools.GetComplexFromXml(node);
                    break;
                case "DrawingMode":
                    this.DrawingMode = Tools.GetEnumXmlAttr(node,
                                       "Value", RibbonDrawingModes.Line);
                    break;
                case "PathMode":
                    //this.PathMode = Tools.GetEnumXmlAttr(node,
                    //                "Value", RibbonPathModes.Segmented);
                    break;
                case "FormulaEnabled":
                    this.FormulaEnabled = (bool)Tools.GetXmlAttribute("Value", typeof(bool), node,
                                                defaultValue: false);
                    break;
                case "FormulaSettings":
                    InitFormulaSettings();
                    FormulaSettings.FromXml(node, this);
                    break;
                case "RibbonPath":
                    foreach (XmlNode pointNode in node.ChildNodes)
                    {
                        RibbonPath.Add(Tools.GetPointFFromXml(pointNode));
                    }
                    break;
                case "CopiedPattern":
                    CopiedPattern = CreatePatternFromXml(Design, node);
                    break;
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }

        public override void FromXml(XmlNode node)
        {
            this.DrawingMode = RibbonDrawingModes.Line;
            //this.PathMode = RibbonPathModes.Segmented;
            DirectFillAngleOffset = 0;
            base.FromXml(node);
            if (this.RibbonPath.Count == 0)
                AddToRibbonPath();
        }

        public override void SetForPreview(Size picSize)
        {
            PatternList.SetRibbonForPreview(this, picSize);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (this.RibbonPen != null)
            {
                this.RibbonPen.Dispose();
                this.RibbonPen = null;
            }
        }
    }
}
