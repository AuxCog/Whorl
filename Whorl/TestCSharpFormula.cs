using System;
using Whorl;
using ParserEngine;
using System.Windows.Forms;
using System.Linq;
namespace WhorlEvalTest
{
    public class WhorlEvalClass
    {

        public enum CVersions
        {
            Normal,
            Outer,
            Inner
        }
        public enum ColorOffsetModes
        {
            Outer,
            Mid,
            Inner
        }
        public class EvalParameters
        {
            [ParameterLabel(@"Only Fractal")] public bool OnlyFractal { get; set; }
            [ParameterLabel(@"Fractal Strength")] public double zStrength { get; set; }
            [ParameterLabel(@"Fractal Version")] public EnumValuesParameter<CVersions> zVersion { get; } = new EnumValuesParameter<CVersions>(CVersions.Normal);
            [ParameterInfo(MinValue = 2)] public int zPaletteN { get; set; } = 16;
            public Complex zPan { get; set; } = Complex.Zero;
            [ParameterInfo(MinValue = 0)] public double zZoom { get; set; } = 1;
            public double zXStretch { get; set; } = 1;
            public double zCutoff { get; set; } = 0.001;
            [ParameterInfo(MinValue = 1)] public int zMaxSteps { get; set; } = 16;
            public double zFeedback { get; set; }
            public Func2Parameter<double> zOp { get; } = new Func2Parameter<double>("Product", null, null);
            public double zOffset { get; set; }
            public Complex zA { get; set; } = Complex.One;
            public double zC { get; set; } = 1;
            public Complex zCoeff { get; set; } = Complex.One;
            public double zCInner { get; set; } = 1;
            public DerivFuncParameter<Complex> fnZDeriv = new DerivFuncParameter<Complex>("Sin");
            public Func1Parameter<Complex> fnZ { get; } = new Func1Parameter<Complex>("Ident", typeof(ParserEngine.Complex), null);
            public Complex zPower { get; set; } = new Complex(3, 0);

            public bool NormalizeAngle { get; set; } = true;
            public EnumValuesParameter<CVersions> CVersion { get; } = new EnumValuesParameter<CVersions>(CVersions.Normal);
            [ParameterInfo(MinValue = 0, UpdateParametersOnChange = true)] public int CCount { get; set; } = 4;
            [ArrayBaseName("C", 0)] public double[] C { get; private set; }
            [ArrayBaseName("Power", 0)] public double[] Power { get; private set; }
            [ArrayBaseName("fn", 0)] public Func1Parameter<double>[] fnC { get; private set; }
            public double polyWeight { get; set; }
            public double polySlope { get; set; }
            public Func1Parameter<double> fnPolySlope { get; } = new Func1Parameter<double>("Ident", null, null);
            public Func1Parameter<double> fnColor { get; } = new Func1Parameter<double>("Ident", null, null);
            public double colorScale { get; set; } = 1;
            public double colorSlope { get; set; }
            public double colorOffset { get; set; }
            public EnumValuesParameter<ColorOffsetModes> colorMode { get; } = new EnumValuesParameter<ColorOffsetModes>(ColorOffsetModes.Outer);
            public double rotation { get; set; }
            public int sectorPetals { get; set; } = 4;
            public double sectorRotation { get; set; }
            public Func1Parameter<double> fnSector { get; } = new Func1Parameter<double>("Ident", null, null);
            [ParameterLabel(@"Radius %")] public double radiusPct { get; set; } = 50;
            public double sectorPhase { get; set; }
            public double sectorPower { get; set; } = 4;
            public double sectorFac { get; set; } = 1;
            [ParameterLabel(@"Center Weight %")] public double centerWeightPct { get; set; }
            public double centerPower { get; set; } = 1;
            public Func1Parameter<double> fnCenter { get; } = new Func1Parameter<double>("Ident", null, null);
            public bool InvertCenter { get; set; }
            [ParameterLabel(@"Sector Weight %")] public double SectorWeightPct { get; set; } = 10;
            [ParameterLabel(@"Clip Center %")] public double clipCenterPct { get; set; }
            public double clipColor { get; set; }
            public Func1Parameter<double> fnAngle { get; } = new Func1Parameter<double>("Ident", null, null);
            public Func1Parameter<double> fnAngle2 { get; } = new Func1Parameter<double>("Ident", null, null);
            public Func2Parameter<double> fnOutline { get; } = new Func2Parameter<double>("Sine", typeof(ParserEngine.OutlineMethods), null);
            public Func2Parameter<double> fnOutlineOp { get; } = new Func2Parameter<double>("Product", null, null);
            public bool InvertOutline { get; set; }
            public Func1Parameter<double> fnAmplitude { get; } = new Func1Parameter<double>("Ident", null, null);
            public double Pointiness { get; set; } = 4;
            public double scale { get; set; } = 1;
            public double petals { get; set; } = 4;
            public double phase { get; set; }
            public double ampPower { get; set; } = 2;
            public double anglePower { get; set; } = 1;
            public double strength { get; set; } = 5;
            public double distanceScale { get; set; } = 1;

            public double CalcPolyTerm(int i, double v)
            {
                //@fn1(@C1 * CMath.Pow(v, @Power1)) 
                switch (CVersion.SelectedValue)
                {
                    case CVersions.Normal:
                    default:
                        return fnC[i].Function(C[i] * CMath.Pow(v, Power[i]));
                    case CVersions.Outer:
                        return C[i] * fnC[i].Function(CMath.Pow(v, Power[i]));
                    case CVersions.Inner:
                        return CMath.Pow(fnC[i].Function(C[i] * v), Power[i]);
                }
            }
            public double CalcPoly(double v)
            {
                return Enumerable.Range(0, CCount).Select(i => CalcPolyTerm(i, v)).Sum();
            }

            public EvalParameters()
            {
                C = new double[CCount];
                Power = new double[CCount];
                fnC = new Func1Parameter<double>[CCount];
                for (int i = 0; i < CCount; i++)
                {
                    Power[i] = 1;
                    fnC[i] = new Func1Parameter<double>("Ident", null, null);
                }
            }

            public void Update()
            {
                int previousLength;
                previousLength = C.Length;
                if (CCount != previousLength)
                {
                    C = Tools.RedimArray(C, CCount);
                    Power = Tools.RedimArray(Power, CCount, 1);
                    fnC = Tools.RedimArray(fnC, CCount);
                    for (int i = previousLength; i < CCount; i++)
                    {
                        fnC[i] = new Func1Parameter<double>("Ident", null, null);
                    }
                }
            }
        }
        private double zFeedback, zStrength;
        private Complex zScale;
        [ParmsProperty]
        public EvalParameters Parms { get; } = new EvalParameters();
        public Whorl.PixelRenderInfo Info { get; set; }
        public Boolean IsFirstCall { get; set; }
        public Single Position { get; set; }
        public const Double invPi = 1 / Math.PI;
        //private Double rotation { get; set; }
        private double sectorRotation { get; set; }
        private double sectorPhase { get; set; }
        public Double phase { get; set; }
        public Double strength { get; set; }
        public Double addDenom { get; set; }
        private double sector1Angle, sectorFactor, sectorFac, centerWeight, sectorPower, radius;

        public void Initialize()
        {
            Info.Normalize = true;
            Info.NormalizeAngle = true;
            zFeedback = 0.01 * Parms.zFeedback;
            zStrength = 0.01 * Parms.zStrength;
            double zoomFac = 1 / Math.Max(0.00001, Parms.zZoom);
            double xFac = Math.Sign(Parms.zXStretch) * 1.0 / Math.Max(0.00001, Math.Abs(Parms.zXStretch));
            zScale = new Complex(xFac * zoomFac, zoomFac);

            Info.Normalize = true;
            Info.NormalizeAngle = Parms.NormalizeAngle;
            phase = Math.PI * Parms.phase / 180;
            Info.Rotation = Tools.DegreesToRadians(Parms.rotation) + Info.PatternAngle;
            //rotation = Info.Rotation;
            radius = 0.01 * Parms.radiusPct;
            sectorPhase = Tools.DegreesToRadians(Parms.sectorPhase);
            sectorRotation = Tools.DegreesToRadians(Parms.sectorRotation);
            sector1Angle = (2.0 * Math.PI / Parms.sectorPetals);
            sectorFactor = 1.0 / sector1Angle;
            sectorFac = 0.001 * Parms.sectorFac;
            strength = 0.1 * Parms.strength;
            addDenom = 1 / Parms.Pointiness;
            centerWeight = 0.01 * Parms.centerWeightPct;
            sectorPower = Parms.sectorPower == 0 ? 1 : 1.0 / Parms.sectorPower;
            //MessageBox.Show($"PatternAngle = {Tools.RadiansToDegrees(Info.PatternAngle)}");
        }

        private double ColorPosition(double v, double modulus)
        {
            double pos;
            double scale = Parms.colorScale + Parms.colorSlope * modulus;
            switch (Parms.colorMode.SelectedValue)
            {
                case ColorOffsetModes.Outer:
                default:
                    pos = scale * Parms.fnColor.Function(v) + Parms.colorOffset;
                    break;
                case ColorOffsetModes.Mid:
                    pos = scale * (Parms.fnColor.Function(v) + Parms.colorOffset);
                    break;
                case ColorOffsetModes.Inner:
                    pos = scale * Parms.fnColor.Function(v + Parms.colorOffset);
                    break;
            }
            return pos;
        }

        public double GetPosition(PolarPoint ppOff, double modulus = 0.0)
        {
            ppOff.Modulus *= 0.01 * Parms.SectorWeightPct;
            Double angle = Parms.petals * ppOff.Angle + phase;
            var pp = new PolarPoint(angle, ppOff.Modulus);
            double factor = Parms.fnOutline.Function(angle, addDenom);
            if (Parms.InvertOutline)
                factor = 1.0 - factor;
            double amplitude = Parms.fnOutlineOp.Function(Parms.fnAmplitude.Function(CMath.Pow(pp.Modulus, Parms.ampPower)), factor);
            double dist = Math.Sqrt(Info.X * Info.X + Info.Y * Info.Y);
            if (Parms.polyWeight != 0)
                amplitude += (Parms.polyWeight + Parms.fnPolySlope.Function(Parms.polySlope * modulus)) * Parms.CalcPoly(dist);
            PolarPoint pp2 = new PolarPoint(angle, amplitude);
            return Parms.scale * amplitude + strength * Parms.fnAngle.Function(Parms.distanceScale *
                                    CMath.Pow(pp.Distance(pp2), Parms.anglePower));
        }

        public int EvalFractalSteps(double position)
        {
            Complex z = new Complex(zScale.Re * Info.X, zScale.Im * Info.Y) + Parms.zPan;
            int i = 0;
            double fac = 1.0 / (zFeedback + position);
            while (i < Parms.zMaxSteps)
            {
                Complex zF = Parms.fnZ.Function(Parms.zCInner * z);
                Complex zNext = Parms.zA * (Complex.Pow(zF, Parms.zPower) - Parms.zC) / (Parms.zPower * Complex.Pow(zF, Parms.zPower - 1));
                double modSq = (zNext - z).GetModulusSquared();
                if (zFeedback != 0)
                    modSq *= fac / i;
                if (modSq <= Parms.zCutoff)
                    break;
                z = zNext;
                ++i;
            }
            return Parms.zMaxSteps - i;
        }

        public double GetFractalValue(int pos)
        {
            return (double)(pos % Parms.zPaletteN) / Parms.zPaletteN - Parms.zOffset;
        }

        public void Eval()
        {
            if (Parms.OnlyFractal)
            {
                Info.Position = (float)(zStrength * GetFractalValue(EvalFractalSteps(0)));
                return;
            }
            PolarPoint pp0 = Info.GetPolar();
            pp0.Angle += sectorPhase;
            double sectorInd = Math.Floor(sectorFactor * pp0.Angle);
            double sectorAngle = (sectorInd + 0.5) * sector1Angle;
            if (sectorFac != 0 && Parms.sectorPower != 0)
            {
                double delta = sectorFac * (pp0.Angle - sectorAngle);
                sectorAngle += Math.Sign(delta) * Math.Pow(Parms.fnSector.Function(Math.Abs(delta)), sectorPower);
            }
            PolarPoint ppSector = new PolarPoint(sectorAngle, radius);
            DoublePoint realCenter = ppSector.ToRectangular();
            PolarPoint ppOff = Info.GetPolar(realCenter.ToPointF());
            if (ppOff.Modulus <= 0.01 * Parms.clipCenterPct)
                Info.Position = (float)Parms.clipColor;
            else
            {
                double position = GetPosition(ppOff, pp0.Modulus);
                if (centerWeight != 0)
                {
                    double cMod = pp0.Modulus;
                    if (Parms.InvertCenter)
                        cMod = (Info.MaxModulus - cMod) / Info.MaxModulus;
                    position += Math.Pow(Parms.fnCenter.Function(cMod), Parms.centerPower) * centerWeight * GetPosition(pp0);
                }
                if (zStrength != 0)
                {
                    int zPos = EvalFractalSteps(position);
                    position = Parms.zOp.Function(position, zStrength * GetFractalValue(zPos));
                }
                Info.Position = (float)(ColorPosition(position, pp0.Modulus));
            }
        }
    }

    public class TransformEvalClass
    {
        public class EvalParameters
        {
            public Func2Parameter<double> fnOutline { get; } = 
                   new Func2Parameter<double>(nameof(OutlineMethods.PointedRound), methodType: typeof(OutlineMethods), addDefaultMethodTypes: false);
            public double Pointiness { get; set; } = 3;
            [ParameterLabel(@"Weight %")]  public double WeightPct { get; set; }
            public double Phase { get; set; }
        }

        [ParmsProperty]
        public EvalParameters Parms { get; } = new EvalParameters();
        public PatternTransform.FormulaInfo Info { get; set; }

        private InfluenceAngle influenceAngle { get; } = new InfluenceAngle();

        public void Initialize()
        {
            influenceAngle.Phase = Tools.DegreesToRadians(Parms.Phase);
            influenceAngle.Initialize(Math.PI, Info.GetVertices(allowCurve: true));
        }

        public void Eval()
        {
            double angle = influenceAngle.ComputeAngle(Info.Angle);
            double outlineAdd = Parms.fnOutline.Function(angle, 1.0 / Parms.Pointiness);
            Info.Amplitude += outlineAdd;
        }
    }
}
