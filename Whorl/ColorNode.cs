using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Whorl;

namespace Whorl
{
    public class ColorNode: BaseObject, IXml
    {
        private Color _color;

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                FloatColor.SetFromColor(value);
            }
        }
        public float Position { get; set; }
        public ColorGradient.FloatColor FloatColor { get; } = new ColorGradient.FloatColor();
        public bool Selected { get; set; }

        public ColorNode()
        { }

        public ColorNode(Color color, float position)
        {
            Color = color;
            Position = position;
        }

        public ColorNode GetCopy()
        {
            return new ColorNode(Color, Position) { Selected = this.Selected };
        }

        public bool IsEqual(ColorNode cn)
        {
            return cn.Position == Position && cn.Color == Color;
        }

        public static ColorNode FindNearest(List<ColorNode> colorNodes, float position)
        {
            return colorNodes.OrderBy(cn => Math.Abs(cn.Position - position)).FirstOrDefault();
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "ColorNode";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(node, "Color", Color.ToArgb());
            xmlTools.AppendXmlAttribute(node, "Position", Position);
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode xmlNode)
        {
            Color = Color.FromArgb(int.Parse(XmlTools.GetXmlAttribute(xmlNode, "Color")));
            Position = float.Parse(XmlTools.GetXmlAttribute(xmlNode, "Position"));
        }

        /// <summary>
        /// Preserve opacity (Alpha) of origColor.
        /// </summary>
        /// <param name="origColor"></param>
        /// <param name="newColor"></param>
        /// <returns></returns>
        public static Color GetColor(Color origColor, Color newColor)
        {
            return Color.FromArgb(origColor.A, newColor);
        }
    }
}
