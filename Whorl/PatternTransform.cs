using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class PatternTransform: GuidKey, IXml
    {
        public class FormulaInfo
        {
            public PatternTransform Parent { get; }
            public double Angle { get; set; }
            public double Amplitude { get; set; }
            public PolarPoint PreviousPoint { get; set; }

            public FormulaInfo(PatternTransform parentTransform)
            {
                Parent = parentTransform;
            }

            public IEnumerable<PointF> GetVertices(bool allowCurve = false)
            {
                return Parent.ParentPattern.GetPolygonVertices(allowCurve);
            }
        }
        private enum GlobalVarNames
        {
            angle,
            amplitude,
            PreviousPoint,
            Info
        }
        //public Pattern Pattern { get; }
        public FormulaSettings TransformSettings { get; }
        private int sequenceNumber;
        public string TransformName { get; set; }
        public int SequenceNumber
        {
            get { return sequenceNumber; }
            set
            {
                if (sequenceNumber != value)
                {
                    sequenceNumber = value;
                    SequenceNumberChanged = true;
                }
            }
        }
        public bool Enabled { get; set; } = true;
        //public bool IsBaseTransform { get; set; }
        //public bool FormulaChanged { get; set; }
        public bool SequenceNumberChanged { get; set; }
        //public WhorlDesign Design { get; }
        public Pattern ParentPattern { get; }
        //public Expression TransformExpression { get; private set; }
        //private ExpressionParser parser { get; set; }
        //private ValidIdentifier angleIdent;
        //private ValidIdentifier amplitudeIdent;
        //private ValidIdentifier previousPointIdent;
        private FormulaInfo Info { get; }

        static PatternTransform()
        {
            ExpressionParser.DeclareExternType(typeof(FormulaInfo));
        }

        public PatternTransform(Pattern parentPattern)
        {
            if (parentPattern == null)
                throw new NullReferenceException("parentPattern cannot be null.");
            ParentPattern = parentPattern;
            Info = new FormulaInfo(this);
            TransformSettings = new FormulaSettings(FormulaTypes.Transform, pattern: ParentPattern);
            TransformSettings.TokensTransformInfo = new TokensTransformInfo(
                              nameof(Info), Enum.GetNames(typeof(GlobalVarNames)));
            ConfigureParser(TransformSettings.Parser);
        }

        public PatternTransform(PatternTransform source, Pattern parentPattern): base(source)
        {
            if (parentPattern == null)
                throw new NullReferenceException("parentPattern cannot be null.");
            ParentPattern = parentPattern;
            Info = new FormulaInfo(this);
            TransformName = source.TransformName;
            SequenceNumber = source.SequenceNumber;
            Enabled = source.Enabled;
            TransformSettings = source.TransformSettings.GetCopy(ConfigureParser, pattern: ParentPattern);
            if (source.TransformSettings.InfluenceLinkParentCollection != null)
            {
                TransformSettings.InfluenceLinkParentCollection =
                    source.TransformSettings.InfluenceLinkParentCollection.GetCopy(TransformSettings, parentPattern);
            }
        }

        private void ConfigureParser(ExpressionParser parser)
        {
            parser.DeclareVariable(nameof(Info), Info.GetType(), Info, isGlobal: true, isReadOnly: true);
            //foreach (GlobalVarNames varName in Enum.GetValues(typeof(GlobalVarNames)))
            //{
            //    if (varName == GlobalVarNames.PreviousPoint)
            //        parser.DeclareVariable(varName.ToString(), typeof(PolarPoint), new PolarPoint(0, 0), isGlobal: true);
            //    else if (varName == GlobalVarNames.Info)
            //        parser.DeclareVariable(varName.ToString(), typeof(FormulaInfo), Info, isGlobal: true, isExternal: false);
            //    else
            //        parser.DeclareVariable(varName.ToString(), typeof(double), 0, isGlobal: true);
            //}
            //angleIdent = parser.GetVariableIdentifier(GlobalVarNames.angle.ToString());
            //amplitudeIdent = parser.GetVariableIdentifier(GlobalVarNames.amplitude.ToString());
            //previousPointIdent = parser.GetVariableIdentifier(GlobalVarNames.PreviousPoint.ToString());
        }

        //public object Clone()
        //{
        //    return new PatternTransform(this);
        //}

        public void Initialize()
        {
            previousPoint = new PolarPoint(0, 0);
            TransformSettings.SetCSharpInfoInstance(Info);
            if (TransformSettings.InfluenceLinkParentCollection != null)
            {
                TransformSettings.InfluenceLinkParentCollection.Initialize();
            }
        }

        public void FinalizeSettings()
        {
            if (TransformSettings.InfluenceLinkParentCollection != null)
            {
                TransformSettings.InfluenceLinkParentCollection.FinalizeSettings();
            }
        }

        private PolarPoint previousPoint;

        public void TransformPoint(ref double amplitude, ref double angle)
        {
            if (!TransformSettings.IsValid)
                return;
            Info.Amplitude = amplitude;
            Info.Angle = angle;
            Info.PreviousPoint = previousPoint;
            previousPoint = new PolarPoint(angle, amplitude);
            if (TransformSettings.EvalFormula())
            {
                amplitude = Info.Amplitude;
                angle = Info.Angle;
            }
        }

        public const string VertexAnglesParameterName = "VertexAngles";

        public void SetVertexAnglesParameter(ArrayParameter arrayParam, List<double> vertexAngles)
        {
            if (arrayParam == null)
                return;
            ArrayDict arrayDict = arrayParam.ArrayValue;
            arrayDict.Clear();
            for (int i = 0; i < vertexAngles.Count; i++)
                arrayDict.SetValue(i, vertexAngles[i]);
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "PatternTransform";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttributes(node, this, nameof(TransformName), nameof(SequenceNumber), 
                                         nameof(Enabled));
            TransformSettings.ToXml(node, xmlTools, nameof(TransformSettings));
            if (TransformSettings.InfluenceLinkParentCollection != null)
            {
                TransformSettings.InfluenceLinkParentCollection.ToXml(node, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributes(this, node, nameof(TransformName), nameof(SequenceNumber),
                                   nameof(Enabled));
            string formula = null;
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == nameof(TransformSettings))
                {
                    TransformSettings.FromXml(childNode, this);
                }
                else if (childNode.Name == nameof(InfluenceLinkParentCollection))
                {
                    TransformSettings.InfluenceLinkParentCollection = new InfluenceLinkParentCollection(ParentPattern, TransformSettings);
                    TransformSettings.InfluenceLinkParentCollection.FromXml(childNode);
                }
                else if (childNode.Name == "Formula")  //Legacy code
                {
                    formula = Tools.GetXmlNodeValue(childNode);
                }
            }
            if (formula != null && !TransformSettings.IsValid)
            {   //Legacy code
                bool? isValid = (bool?)Tools.GetXmlAttribute("IsValid",
                                       typeof(bool), node, required: false);
                if (isValid != null && (bool)isValid)
                {
                    TransformSettings.Parse(formula);
                }
            }
        }
    }
}
