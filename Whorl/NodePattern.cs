using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class NodePattern : BasePattern
    {
        public int NodeIndex { get; }
        public int NodeId
        {
            get { return this.NodeIndex + 1; }
        }
        public IntColor ColorValue { get; set; }
        public Dictionary<int, float> WeightsDict { get; } =
           new Dictionary<int, float>();

        public NodePattern(int nodeId)
        {
            this.NodeIndex = nodeId;
        }

        public override void Dispose()
        {
        }

        public override void FromXml(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            throw new NotImplementedException();
        }

        public override void DrawOutline(Graphics g, Color? color = null)
        {
            if (color == null)
                color = Tools.InverseColor(ColorValue.GetColor());
            Tools.DrawSquare(g, (Color)color, this.Center);
        }

        public void DrawNodeId(Graphics g, Font font)
        {
            using (Brush brush = new SolidBrush(Tools.InverseColor(ColorValue.GetColor())))
            {
                g.DrawString(this.NodeId.ToString(), font, brush,
                             new PointF(Center.X + 5, Center.Y));
            }
        }
    }
}
