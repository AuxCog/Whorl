using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class PathOutlineTransform: GuidKey, IXml
    {
        public PathOutline PathOutline { get; }
        public FormulaSettings VerticesSettings { get; }
        public PathOutline.PathOutlineVars GlobalInfo { get; }
        public int SequenceNumber { get; set; }
        public bool Enabled { get; set; }
        public string FormulaName => VerticesSettings.FormulaName;

        public PathOutlineTransform(PathOutline pathOutline)
        {
            PathOutline = pathOutline;
            GlobalInfo = new PathOutline.PathOutlineVars(PathOutline);
            VerticesSettings = new FormulaSettings(pathOutline.VerticesSettings);
        }

        public PathOutlineTransform(PathOutlineTransform source, PathOutline pathOutline): base(source)
        {
            PathOutline = pathOutline;
            GlobalInfo = new PathOutline.PathOutlineVars(PathOutline);
            VerticesSettings = new FormulaSettings(source.VerticesSettings);
            SequenceNumber = source.SequenceNumber;
            Enabled = source.Enabled;
        }

        public void TransformPathPoints()
        {
            PathOutline.InitializeFormula(GlobalInfo, VerticesSettings);
            VerticesSettings.EvalFormula();
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(PathOutlineTransform);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            AddKeyGuidXmlAttribute(xmlNode, xmlTools);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(SequenceNumber), SequenceNumber);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(Enabled), Enabled);
            VerticesSettings.ToXml(xmlNode, xmlTools, nameof(VerticesSettings));
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            if (node.FirstChild == null || node.FirstChild.Name != nameof(VerticesSettings))
                throw new ArgumentException("Invalid XML found.");
            ReadKeyGuidXmlAttribute(node);
            SequenceNumber = Tools.GetXmlAttribute<int>(node, nameof(SequenceNumber));
            Enabled = Tools.GetXmlAttribute<bool>(node, nameof(Enabled));
            VerticesSettings.FromXml(node.FirstChild);
        }
    }
}
