using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class ImproviseConfig: BaseObject, IXml
    {
        public bool Enabled { get; set; } = true;
        public bool ImproviseOnAllPatterns { get; set; }
        public bool DrawDesignLayers { get; set; }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "ImproviseConfig";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttributes(node, this, nameof(Enabled), nameof(ImproviseOnAllPatterns), nameof(DrawDesignLayers));
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributesExcept(this, node);
        }

    }
}
