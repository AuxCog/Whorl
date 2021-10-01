using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public enum ImproviseParameterType
    {
        Outline,    //Parameter from formula for a Custom Outline
        Transform   //Parameter from formula for a Transform
    }

    public class ParameterImproviseConfig: BaseObject, IXml
    {
        //private ParserEngine.Parameter parameter;

        public ImproviseParameterType ParameterType { get; private set; }
        public ParserEngine.Parameter Parameter { get; set; } //Parameter object
        //{
        //    get { return parameter; }
        //    set
        //    {
        //        if (value == null)
        //            throw new Exception($"{GetType().Name}.Parameter cannot be null.");
        //        parameter = value;
        //    }
        //}
        public Guid ParameterGuid { get; private set; } //GUID of parameter
        public string ParameterName
        {
            get { return Parameter?.ParameterName; }
        }
        public string FormulaName { get; set; } //name of transform or outline formula
        public bool Enabled { get; set; } = false;
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double ImprovStrength { get; set; } = 0.5D;
        public int DecimalPlaces { get; set; } = 4;

        public ParameterImproviseConfig()
        {
        }

        public ParameterImproviseConfig(ImproviseParameterType paramType, 
                                        ParserEngine.Parameter parameter)
        {
            ParameterType = paramType;
            Parameter = parameter;
            ParameterGuid = parameter.Guid;
            MinValue = parameter.MinValue;
            MaxValue = parameter.MaxValue;
        }

        /// <summary>
        /// Constructor to get copy from source.
        /// </summary>
        /// <param name="source"></param>
        public ParameterImproviseConfig(ParameterImproviseConfig source)
        {
            Tools.CopyProperties(this, source, excludedPropertyNames:
                                 new string[] { nameof(ParameterType), nameof(ParameterGuid) });
            this.ParameterType = source.ParameterType;
            this.ParameterGuid = source.ParameterGuid;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "ParameterImproviseConfig";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttributes(node, this, nameof(ParameterType), nameof(ParameterGuid), nameof(FormulaName),
                                         nameof(MinValue), nameof(MaxValue), nameof(ImprovStrength), nameof(DecimalPlaces));
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            FormulaName = null;
            Tools.GetXmlAttributesExcept(this, node, excludedPropertyNames: 
                        new string[] { nameof(ParameterType), nameof(ParameterGuid),
                        nameof(MinValue), nameof(MaxValue) });
            this.ParameterType = Tools.GetEnumXmlAttr(
                                 node, nameof(ParameterType), ImproviseParameterType.Outline);
            string guid = (string)Tools.GetXmlAttribute(nameof(ParameterGuid), typeof(string), node);
            this.ParameterGuid = Guid.Parse(guid);
            MinValue = (double?)Tools.GetXmlAttribute(nameof(MinValue), typeof(double),
                                                      node, required: false);
            MaxValue = (double?)Tools.GetXmlAttribute(nameof(MaxValue), typeof(double),
                                                      node, required: false);
        }
    }
}
