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
    public class ColorNodeList: BaseObject, IXml
    {
        public const int MinCount = 2;

        private List<ColorNode> colorNodes { get; set; }

        public ColorNodeList()
        {
            colorNodes = new List<ColorNode>();
        }

        public ColorNodeList(IEnumerable<ColorNode> colorNodes)
        {
            this.colorNodes = colorNodes.ToList();
        }

        public IEnumerable<ColorNode> ColorNodes
        {
            get { return colorNodes; }
        }

        public int Count
        {
            get { return colorNodes.Count; }
        }

        public ColorNode GetColorNode(int index)
        {
            return colorNodes[index];
        }

        public void AddNode(ColorNode node)
        {
            colorNodes.Add(node);
        }

        public void AddDefaultNodes()
        {
            AddNode(new ColorNode(Color.Black, 0F));
            AddNode(new ColorNode(Color.White, 1F));
        }

        public void InsertNode(int index, ColorNode node)
        {
            colorNodes.Insert(index, node);
        }

        public bool RemoveNode(ColorNode node)
        {
            if (Count <= MinCount)
                return false;
            else
                return colorNodes.Remove(node);
        }

        public int IndexOfNode(ColorNode node)
        {
            return colorNodes.IndexOf(node);
        }

        public bool NodeIsBoundaryNode(ColorNode node)
        {
            int index = colorNodes.IndexOf(node);
            return index == 0 || index == Count - 1;
        }

        public ColorNode FindNearestNode(float position)
        {
            return ColorNode.FindNearest(colorNodes, position);
        }

        public void SortNodes()
        {
            colorNodes = colorNodes.OrderBy(cn => cn.Position).ToList();
        }

        public static float NormalizePosition(float position)
        {
            if (position > 1 || position < 0)
                position = position - (float)Math.Floor(position);
            return position;
        }

        public Color GetColorAtPosition(float position)
        {
            position = NormalizePosition(position);
            int index2 = colorNodes.FindIndex(cn => cn.Position >= position);
            int index1;
            if (index2 <= 0)
            {
                if (Count == 0)
                    return Color.White;
                else
                {
                    index1 = Count - 1;
                    if (index2 == 0)
                        position += 1;
                    else
                        index2 = 0;
                }
            }
            else
            {
                index1 = index2 - 1;
            }
            ColorNode node2 = colorNodes[index2];
            if (index2 == index1 || node2.Position == position)
            {
                return node2.Color;
            }
            else
            {
                ColorNode node1 = colorNodes[index1];
                float diff;
                if (index1 > index2)
                    diff = 1 - node1.Position + node2.Position;
                else
                    diff = node2.Position - node1.Position;
                float factor = Math.Abs((position - node1.Position) / diff);
                return ColorGradient.FloatColor.InterpolateColor(node1.FloatColor, node2.FloatColor, factor);
            }
        }

        public void CopyProperties(ColorNodeList source)
        {
            colorNodes = source.ColorNodes.Select(cn => cn.GetCopy()).ToList();
        }

        public ColorNodeList GetCopy()
        {
            var copy = new ColorNodeList();
            copy.CopyProperties(this);
            return copy;
        }

        /// <summary>
        /// Return color nodes for wrapping color gradient around from last node to first.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColorNode> GetColorBlendNodes()
        {
            var firstNode = ColorNodes.FirstOrDefault();
            var lastNode = ColorNodes.LastOrDefault();
            if (firstNode == null)
                return ColorNodes;
            Color boundColor;
            if (firstNode.Position < 0)
                firstNode.Position = 0;
            if (lastNode.Position > 1)
                lastNode.Position = 1;
            if ((firstNode.Position > 0 || lastNode.Position < 1) && firstNode.Position < lastNode.Position)
            {
                float factor = (1F - lastNode.Position) / (1F - lastNode.Position + firstNode.Position);
                boundColor = ColorGradient.FloatColor.InterpolateColor(lastNode.FloatColor, firstNode.FloatColor, factor);
                var colorNodes = new List<ColorNode>(ColorNodes);
                if (firstNode.Position > 0)
                {
                    colorNodes.Insert(0, new ColorNode(boundColor, 0));
                }
                if (lastNode.Position < 1)
                {
                    colorNodes.Add(new ColorNode(boundColor, 1));
                }
                return colorNodes;
            }
            else
                return ColorNodes;
        }

        public bool IsEqual(ColorNodeList cnl)
        {
            bool isEqual = cnl.Count == Count;
            if (isEqual)
            {
                isEqual = !Enumerable.Range(0, Count).Any(i => !cnl.colorNodes[i].IsEqual(colorNodes[i]));
            }
            return isEqual;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "ColorNodes";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (ColorNode colorNode in colorNodes)
            {
                colorNode.ToXml(node, xmlTools, "ColorNode");
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode xmlNode)
        {
            colorNodes = new List<ColorNode>();
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                var colorNode = new ColorNode();
                colorNode.FromXml(childNode);
                colorNodes.Add(colorNode);
            }
        }
    }
}
