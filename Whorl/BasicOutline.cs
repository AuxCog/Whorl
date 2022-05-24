using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public enum BasicOutlineTypes
    {
        Round,
        Pointed,
        Pointed2,
        Pointed3,
        Pointed4,
        Pointed5,
        Sine,
        Lobed,
        Ellipse,
        Rectangle,
        Custom,
        Path,
        NewEllipse,
        Broad,
        BroadFull
    }

    public class BasicOutline: GuidKey, ICloneable, IXml
    {

        public class CustomOutline: BaseObject, ICloneable
        {
            public class CustomOutlineInfo
            {
                public double Angle { get; set; }
                public double Amplitude { get; set; }
                [ReadOnly]
                public double AddDenom { get; set; }
                public double MaxAmplitude { get; set; }
                [ReadOnly]
                public double RotationSpan { get; set; }
                [ReadOnly]
                public double Petals { get; set; }
            }
            private enum GlobalVarNames
            {
                angle,
                amplitude,
                addDenom,
                maxAmplitude,
                rotationSpan,
                petals
            }
            private CustomOutlineInfo CustomInfo { get; } = new CustomOutlineInfo();
            //private ValidIdentifier angleIdent, amplitudeIdent,
            //    addDenomIdent, maxAmplitudeIdent, rotationSpanIdent, petalsIdent;
            public BasicOutline ParentOutline { get; set; }

            public FormulaSettings AmplitudeSettings { get; private set; }
            public FormulaSettings MaxAmplitudeSettings { get; private set; }
            private ExpressionParser parser { get; }

            private CustomOutline(BasicOutline parent, CustomOutline source)
            {
                parser = FormulaSettings.CreateExpressionParser();
                this.ParentOutline = parent;
                parser.DeclareVariable(nameof(CustomInfo), CustomInfo.GetType(), CustomInfo, 
                                       isGlobal: true, isReadOnly: true);
                //foreach (GlobalVarNames varName in Enum.GetValues(typeof(GlobalVarNames)))
                //{
                //    parser.DeclareVariable(varName.ToString(), typeof(double), initialValue: 0D, isGlobal: true,
                //            isReadOnly: varName == GlobalVarNames.addDenom || varName == GlobalVarNames.rotationSpan
                //                        || varName == GlobalVarNames.petals);
                //}
                //angleIdent = parser.GetVariableIdentifier(GlobalVarNames.angle.ToString());
                //amplitudeIdent = parser.GetVariableIdentifier(GlobalVarNames.amplitude.ToString());
                //addDenomIdent = parser.GetVariableIdentifier(GlobalVarNames.addDenom.ToString());
                //maxAmplitudeIdent = parser.GetVariableIdentifier(GlobalVarNames.maxAmplitude.ToString());
                //maxAmplitudeIdent.SetCurrentValue(1D);
                //rotationSpanIdent = parser.GetVariableIdentifier(GlobalVarNames.rotationSpan.ToString());
                //rotationSpanIdent.SetCurrentValue(1D);
                //petalsIdent = parser.GetVariableIdentifier(GlobalVarNames.petals.ToString());
                if (source == null)
                {
                    AmplitudeSettings = new FormulaSettings(FormulaTypes.Outline, parser); //, parseOnChanges: true);
                    MaxAmplitudeSettings = new FormulaSettings(FormulaTypes.Outline, parser); //, parseOnChanges: true);
                    var transformInfo = new TokensTransformInfo(
                                        nameof(CustomInfo), Enum.GetNames(typeof(GlobalVarNames)));
                    AmplitudeSettings.TokensTransformInfo = transformInfo;
                    MaxAmplitudeSettings.TokensTransformInfo = transformInfo;
                }
                else
                {
                    AmplitudeSettings = source.AmplitudeSettings.GetCopy(null, parser);
                    MaxAmplitudeSettings = source.MaxAmplitudeSettings.GetCopy(null, parser);
                }
            }

            public CustomOutline(BasicOutline parent): this(parent, null)
            {
            }

            public CustomOutline(CustomOutline source): this(source.ParentOutline, source)
            {
            }

            static CustomOutline()
            {
                ExpressionParser.DeclareExternType(typeof(CustomOutlineInfo));
            }

            public void Initialize()
            {
                AmplitudeSettings.SetCSharpInfoInstance(CustomInfo);
            }

            public void CopyFormulaSettings(CustomOutline source)
            {
                AmplitudeSettings = source.AmplitudeSettings.GetCopy(null, parser);
                MaxAmplitudeSettings = source.MaxAmplitudeSettings.GetCopy(null, parser);
            }

            public double ComputeAmplitudeAndAngle(ref double angle)
            {
                double amplitude = ParentOutline.AmplitudeFactor * ParentOutline.UnitFactor 
                                   * ComputeAmplitude(angle);
                angle = CustomInfo.Angle;
                //angle = (double)angleIdent.CurrentValue;
                return amplitude;
            }

            public double ComputeAmplitude(double angle)
            {
                if (!AmplitudeSettings.IsValid)
                    return 0;
                CustomInfo.Angle = angle;
                CustomInfo.AddDenom = ParentOutline.AddDenom;
                CustomInfo.RotationSpan = ParentOutline.GetRotationSpan();
                CustomInfo.Petals = ParentOutline.Petals;
                //angleIdent.SetCurrentValue(angle);
                //addDenomIdent.SetCurrentValue(ParentOutline.AddDenom);
                //rotationSpanIdent.SetCurrentValue(ParentOutline.GetRotationSpan());
                //petalsIdent.SetCurrentValue(ParentOutline.Petals);
                if (AmplitudeSettings.EvalFormula())
                    return CustomInfo.Amplitude;
                //try
                //{
                //    return (double)amplitudeIdent.CurrentValue;
                //}
                //catch (Exception ex)
                //{
                //    throw new Exception($"Amplitude = {amplitudeIdent.CurrentValue}", ex);
                //}
                else
                    return 0D;
            }

            public double ComputeMaxAmplitude()
            {
                if (!MaxAmplitudeSettings.IsValid)
                    return 1;
                CustomInfo.AddDenom = ParentOutline.AddDenom;
                CustomInfo.RotationSpan = ParentOutline.GetRotationSpan();
                //addDenomIdent.SetCurrentValue(ParentOutline.AddDenom);
                //rotationSpanIdent.SetCurrentValue(ParentOutline.GetRotationSpan());
                MaxAmplitudeSettings.SetCSharpInfoInstance(CustomInfo);
                if (MaxAmplitudeSettings.EvalFormula())
                    return CustomInfo.MaxAmplitude;
                //return (double)maxAmplitudeIdent.CurrentValue;
                else
                    return 1;
            }

            public object Clone()
            {
                return new CustomOutline(ParentOutline, this);
            }
        }

        public delegate double ComputeAmplitudeDelegate(double angle);

        protected ComputeAmplitudeDelegate computeAmplitude { get; set; } = null;
        //private ComputeAmplitudeDelegate computeSoundAmplitude = null;
        private BasicOutlineTypes basicOutlineType { get; set; }
        private double frequency { get; set; }
        private double addDenom { get; set; }
        private double angleOffset { get; set; } = 0D;
        public CustomOutline customOutline { get; private set; }
        public virtual bool SupportsInfluencePoints => false;

        /// <summary>
        /// True if only one of this type of outline is supported for a pattern.
        /// </summary>
        public virtual bool UseSingleOutline => false;  //Overriden in PathOutline class.

        public virtual bool NoCustomOutline => false;

        public BasicOutline()
        {
        }

        public BasicOutline(BasicOutlineTypes basicOutlineType)
        {
            this.BasicOutlineType = basicOutlineType;
            this.Frequency = 2D;
            this.AddDenom = 0.5;
        }

        public BasicOutline(BasicOutline source) : base(source)
        {
            BasicOutlineType = source.BasicOutlineType;
            CopyProperties(source, GetExcludedCopyPropertyNames().ToArray());
            if (customOutline != null)
                customOutline.ParentOutline = this;
        }

        protected virtual IEnumerable<string> GetExcludedCopyPropertyNames()
        {
            return new string[] { nameof(BasicOutlineType), nameof(UnitFactor) };
        }


        public virtual FormulaSettings GetFormulaSettings()
        {
            return customOutline?.AmplitudeSettings;
        }

        public bool UsesCustomFormula
        {
            get
            {
                return BasicOutlineType == BasicOutlineTypes.Custom || 
                       BasicOutlineType == BasicOutlineTypes.Path;
            }
        }

        public BasicOutlineTypes BasicOutlineType
        {
            get { return basicOutlineType; }
            set
            {
                basicOutlineType = value;
                switch (basicOutlineType)
                {
                    case BasicOutlineTypes.Round:
                        computeAmplitude = RoundComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Pointed:
                        computeAmplitude = PointedComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Pointed2:
                    case BasicOutlineTypes.Pointed3:
                        computeAmplitude = Pointed2_3ComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Pointed4:
                        computeAmplitude = Pointed4_ComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Pointed5:
                        computeAmplitude = Pointed5_ComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Sine:
                        computeAmplitude = SineComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Lobed:
                        computeAmplitude = LobedComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Ellipse:
                        computeAmplitude = EllipseComputeAmplitude;
                        break;
                    case BasicOutlineTypes.NewEllipse:
                        computeAmplitude = NewEllipseComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Broad:
                        computeAmplitude = BroadComputeAmplitude;
                        break;
                    case BasicOutlineTypes.BroadFull:
                        computeAmplitude = BroadFullComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Rectangle:
                        AngleOffset = 0D;
                        computeAmplitude = RectangleComputeAmplitude;
                        break;
                    case BasicOutlineTypes.Path:
                    case BasicOutlineTypes.Custom:
                        if (customOutline == null && !NoCustomOutline)
                            customOutline = new CustomOutline(this);
                        computeAmplitude = CustomComputeAmplitude;
                        break;
                    default:
                        throw new Exception(
                  $"The basic outline type {basicOutlineType} is not yet implemented.");
                }
            }
        }

        [PropertyAction(Name = "Petals", MinValue = 1)]
        public int IntPetals
        {
            get { return (int)Petals; }
            set
            {
                Frequency = 0.5 * value;
            }
        }

        public double Frequency
        {
            get { return frequency; }
            set
            {
                frequency = value;
                Petals = Math.Round(2 * frequency);
                UsedFrequency = 0.5 * Petals;
            }
        }

        public double Petals { get; private set; }

        public double UsedFrequency { get; private set; }

        [PropertyAction]
        public double Phase
        {
            get { return Tools.RadiansToDegrees(AngleOffset); }
            set
            {
                AngleOffset = Tools.DegreesToRadians(value);
            }
        }

        public double AngleOffset
        {
            get
            {
                return angleOffset;
            }
            set
            {
                angleOffset = BasicOutlineType == BasicOutlineTypes.Rectangle ? 0D : value;
            }
        }

        public bool Enabled { get; set; } = true;

        private double amplitudeFactor = 1D;

        [PropertyAction(Name = "Weight", MinValue = 0)]
        public double AmplitudeFactor
        {
            get { return amplitudeFactor; }
            set
            {
                //if (value < 0D)
                //    throw new Exception("AmplitudeFactor cannot be negative.");
                //else
                //{
                    amplitudeFactor = value;
                    computeAmplitudeFactor = AmplitudeFactor * UnitFactor;
                //}
            }
        }

        public double AddDenom
        {
            get { return addDenom; }
            set
            {
                addDenom = Math.Max(value, 0.0001);
                ComputeUnitFactor();
            }
        }

        public void SetPointiness(double pointiness)
        {
            AddDenom = 1D / Math.Max(0.0001, pointiness);
        }

        public double GetPointiness()
        {
            return 1D / AddDenom;
        }

        [PropertyAction(MinValue = 0.0001)]
        public double Pointiness
        {
            get { return GetPointiness(); }
            set { SetPointiness(value); }
        }

        private double unitFactor = 1;
        public double UnitFactor
        {
            get { return unitFactor; }
            private set
            {
                unitFactor = value;
                computeAmplitudeFactor = AmplitudeFactor * UnitFactor;
            }
        }

        private double computeAmplitudeFactor = 1;

        public double ComputeAmplitude(double angle)
        {
            if (BasicOutlineType != BasicOutlineTypes.Path)
                angle = UsedFrequency * angle + AngleOffset;
            return computeAmplitudeFactor * computeAmplitude.Invoke(angle);
        }

        private double SineComputeAmplitude(double angle)
        {
            return Math.Sin(angle);
        }

        private double SineMaxAmplitude()
        {
            return 1D;
        }

        private double RoundComputeAmplitude(double angle)
        {
            return Math.Abs(Math.Sin(angle));
        }

        private double RoundMaxAmplitude()
        {
            return 1D;
        }

        private double PointedComputeAmplitude(double angle)
        {
            return AddDenom / (AddDenom + Math.Abs(Math.Sin(angle)));
        }

        private double PointedMaxAmplitude()
        {
            return 1D;
        }

        private double Pointed2_3ComputeAmplitude(double angle)
        {
            double sine = Math.Sin(angle);
            double amp = Math.Cos(angle) / (sine >= 0D ? sine + AddDenom : sine - AddDenom);
            if (BasicOutlineType == BasicOutlineTypes.Pointed3)
                amp = Math.Sqrt(Math.Abs(amp));
            return amp;
        }

        private double Pointed2_3MaxAmplitude()
        {
            double maxAmp = 1D / AddDenom;
            if (BasicOutlineType == BasicOutlineTypes.Pointed3)
                maxAmp = Math.Sqrt(maxAmp);
            return maxAmp;
        }

        private double Pointed4_ComputeAmplitude(double angle)
        {
            double amp = AddDenom / (AddDenom + Math.Abs(Math.Tan(angle)));
            return Math.Pow(amp, 0.5 + amp);
            //double sine = Math.Sin(angle);
            //double amp;
            //if (sine < 0 && sine > -addDenom)
            //    amp = Math.Cos(Math.Round(1D / addDenom) * angle) / addDenom;
            //else
            //    amp = Math.Cos(angle) / (sine >= 0D ? sine + AddDenom : sine - AddDenom);
            //return amp;
        }

        private double Pointed4_MaxAmplitude()
        {
            return 1D;
        }

        private double Pointed5_ComputeAmplitude(double angle)
        {
            return AddDenom / (AddDenom + Math.Abs(Math.Tan(angle)));
        }

        private double Pointed5_MaxAmplitude()
        {
            return 1D;
        }

        private double LobedComputeAmplitude(double angle)
        {
            return 1D + AddDenom - Math.Abs(Math.Sin(angle));
        }

        private double LobedMaxAmplitude()
        {
            return 1D + AddDenom;
        }

        private double EllipseComputeAmplitude(double angle)
        {
            return 1D / (1D - Math.Cos(angle) / (1D + AddDenom));
        }

        private double EllipseMaxAmplitude()
        {
            return 1D / (1D - 1 / (1 + AddDenom));
        }

        private double NewEllipseComputeAmplitude(double angle)
        {
            double b = 1D + 1D / AddDenom;
            double sine = Math.Sin(angle);
            double cosine = b * Math.Cos(angle);
            return b / Math.Sqrt(sine * sine + cosine * cosine);
        }

        private double NewEllipseMaxAmplitude()
        {
            double b = 1D + 1D / AddDenom;
            return Math.Max(b, 1D);
        }

        private double BroadComputeAmplitude(double angle)
        {
            return Math.Pow(Math.Max(0D, Math.Sin(angle)), 0.1D / AddDenom);
        }

        private double BroadMaxAmplitude()
        {
            return 1D;
        }

        private double BroadFullComputeAmplitude(double angle)
        {
            return Math.Pow(Math.Abs(Math.Sin(angle)), 0.1D / AddDenom);
        }

        private double RectangleComputeAmplitude(double angle)
        {
            double ratio = Math.Min(1000D, AddDenom);
            const double height = 1;
            double width = 1D / ratio;
            double sine = Math.Abs(Math.Sin(angle));
            double cosine = Math.Abs(Math.Cos(angle));
            double amp;
            if (sine <= ratio * cosine)
                amp = width / cosine;
            else
                amp = height / sine;
            return amp;
        }

        private double RectangleMaxAmplitude()
        {
            return Math.Sqrt(1D + 1D / (AddDenom * AddDenom));
        }

        protected double CustomComputeAmplitude(double angle)
        {
            if (NoCustomOutline)
                return 1;
            return customOutline.ComputeAmplitude(angle);
        }

        private double CustomMaxAmplitude()
        {
            if (NoCustomOutline)
                return 1;
            return customOutline.ComputeMaxAmplitude();
        }

        public virtual bool InitComputeAmplitude(int rotationSteps)
        {
            if (customOutline != null)
                customOutline.Initialize();
            return false;
        }

        private void ComputeUnitFactor()
        {
            double maxAmplitude = 1;
            switch (BasicOutlineType)
            {
                case BasicOutlineTypes.Round:
                    maxAmplitude = RoundMaxAmplitude();
                    break;
                case BasicOutlineTypes.Pointed:
                    maxAmplitude = PointedMaxAmplitude();
                    break;
                case BasicOutlineTypes.Pointed2:
                case BasicOutlineTypes.Pointed3:
                    maxAmplitude = Pointed2_3MaxAmplitude();
                    break;
                case BasicOutlineTypes.Pointed4:
                    maxAmplitude = Pointed4_MaxAmplitude();
                    break;
                case BasicOutlineTypes.Pointed5:
                    maxAmplitude = Pointed5_MaxAmplitude();
                    break;
                case BasicOutlineTypes.Sine:
                    maxAmplitude = SineMaxAmplitude();
                    break;
                case BasicOutlineTypes.Lobed:
                    maxAmplitude = LobedMaxAmplitude();
                    break;
                case BasicOutlineTypes.Ellipse:
                    maxAmplitude = EllipseMaxAmplitude();
                    break;
                case BasicOutlineTypes.NewEllipse:
                    maxAmplitude = NewEllipseMaxAmplitude();
                    break;
                case BasicOutlineTypes.Broad:
                    maxAmplitude = BroadMaxAmplitude();
                    break;
                case BasicOutlineTypes.BroadFull:
                    maxAmplitude = BroadMaxAmplitude();
                    break;
                case BasicOutlineTypes.Rectangle:
                    maxAmplitude = RectangleMaxAmplitude();
                    break;
                case BasicOutlineTypes.Custom:
                    maxAmplitude = CustomMaxAmplitude();
                    break;
            }
            UnitFactor = 1D / Math.Max(0.0001D, maxAmplitude);
        }

        public virtual double GetRotationSpan()
        {
            return 1D;
        }

        public override string ToString()
        {
            return "BasicOutline(" + this.BasicOutlineType.ToString() + ")";
        }

        protected void CopyProperties(BasicOutline sourceOutline,
                                      string[] excludedPropertyNames = null)
        {
            Tools.CopyProperties(this, sourceOutline, excludedPropertyNames);
            if (sourceOutline.customOutline != null)
            {
                this.customOutline.CopyFormulaSettings(sourceOutline.customOutline);
            }
        }

        public BasicOutline GetCopy()
        {
            return new BasicOutline(this);
            //BasicOutline copy = new BasicOutline(BasicOutlineType);
            //copy.CopyProperties(this, excludedPropertyNames:
            //                    new string[] { nameof(BasicOutlineType), nameof(UnitFactor) });
            //if (copy.customOutline != null)
            //    copy.customOutline.ParentOutline = copy;
            //return copy;
        }

        public virtual object Clone()
        {
            return GetCopy();
        }

        protected virtual void AppendExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
        }

        public virtual XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "BasicOutline";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttributesExcept(node, this, "PathOutlineType", nameof(UnitFactor), nameof(Petals));
            if (customOutline != null)
            {
                customOutline.AmplitudeSettings.ToXml(node, xmlTools, "AmplitudeSettings");
                customOutline.MaxAmplitudeSettings.ToXml(node, xmlTools, "MaxAmplitudeSettings");
            }
            AppendExtraXml(node, xmlTools);
            return xmlTools.AppendToParent(parentNode, node);
        }

        protected virtual bool FromExtraXml(XmlNode node)
        {
            return false;
        }

        public virtual void FromXml(XmlNode node)
        {
            string sType = node.Attributes["BasicOutlineType"].Value;
            BasicOutlineTypes outlineType;
            if (sType == null || !Enum.TryParse(sType, out outlineType))
            {
                throw new Exception("XML did not contain valid BasicOutlineType.");
            }
            this.BasicOutlineType = outlineType;
            Tools.GetXmlAttributesExcept(this, node, excludedPropertyNames: 
                new string[] { nameof(BasicOutlineType), nameof(AddDenom),
                               "SegmentVerticesCenter", "SegmentVerticesAreNormalized"  });
            if (outlineType == BasicOutlineTypes.Custom || 
                outlineType == BasicOutlineTypes.Path)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case "AmplitudeSettings":
                            customOutline.AmplitudeSettings.FromXml(childNode, this);
                            break;
                        case "MaxAmplitudeSettings":
                            customOutline.MaxAmplitudeSettings.FromXml(childNode, this);
                            break;
                        case "AmplitudeFormula":
                            customOutline.AmplitudeSettings.Parse(Tools.GetXmlNodeValue(childNode));
                            break;
                        case "MaxAmplitudeFormula":
                            customOutline.MaxAmplitudeSettings.Parse(Tools.GetXmlNodeValue(childNode));
                            break;
                        default:
                            if (!FromExtraXml(childNode))
                                throw new Exception("Invalid XML node found: " + childNode.Name);
                            break;
                    }
                }
            }
            this.AddDenom = (double)Tools.GetXmlAttribute(
                             nameof(AddDenom), typeof(double), node);
        }
    }
}
