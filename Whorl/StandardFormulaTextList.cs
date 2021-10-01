using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class StandardFormulaTextList: IXml
    {
        public List<StandardFormulaText> StandardFormulaTexts { get; } =
           new List<StandardFormulaText>();
        public bool StandardTextsChanged { get; set; }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(StandardFormulaTextList);
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (var stdText in StandardFormulaTexts)
            {
                xmlTools.AppendChildNode(node, nameof(StandardFormulaText), stdText.ClipboardText);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            StandardFormulaTexts.Clear();
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == nameof(StandardFormulaText))
                {
                    var stdText = new StandardFormulaText(Tools.GetXmlNodeValue(childNode));
                    StandardFormulaTexts.Add(stdText);
                }
            }
        }
    }
}
