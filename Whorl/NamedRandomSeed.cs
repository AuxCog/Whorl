using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class NamedRandomSeed: BaseObject, IXml
    {
        public string Name { get; set; }
        public int Seed { get; set; }

        public NamedRandomSeed()
        { }

        /// <summary>
        /// Returns copy of source.
        /// </summary>
        /// <param name="source"></param>
        public NamedRandomSeed(NamedRandomSeed source)
        {
            this.Name = source.Name;
            this.Seed = source.Seed;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(NamedRandomSeed);
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttributes(node, this, nameof(Name), nameof(Seed));
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributesExcept(this, node);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
