using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml;

namespace Whorl
{
    public class GradientColors: BaseObject, ICloneable, IXml
    {
        public Color BoundaryColor { get; set; }
        public Color CenterColor { get; set; }

        public object Clone()
        {
            GradientColors copy = new GradientColors();
            copy.BoundaryColor = this.BoundaryColor;
            copy.CenterColor = this.CenterColor;
            return copy;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "GradientColors";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            node.AppendChild(xmlTools.CreateXmlNode(nameof(BoundaryColor), BoundaryColor));
            node.AppendChild(xmlTools.CreateXmlNode(nameof(CenterColor), CenterColor));
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "BoundaryColor":
                        BoundaryColor = Tools.GetColorFromXml(childNode);
                        break;
                    case "CenterColor":
                        CenterColor = Tools.GetColorFromXml(childNode);
                        break;
                    default:
                        throw new Exception("Invalid XML found for a GradientColors object.");
                }
            }
        }
    }
}
