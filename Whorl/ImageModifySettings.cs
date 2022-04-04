using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Whorl.ImageModifier;

namespace Whorl
{
    public class ImageModifySettings: IXml
    {
        public string ImageFileName { get; set; }
        public int StepNumber { get; set; } = 1;
        public List<ImageModifyStepSettings> Steps { get; } = new List<ImageModifyStepSettings>();

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(ImageModifySettings);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(ImageFileName), ImageFileName);
            foreach (var step in Steps)
            {
                step.ToXml(xmlNode, xmlTools, "Step");
            }
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            Steps.Clear();
            ImageFileName = Tools.GetXmlAttribute<string>(node, nameof(ImageFileName));
            foreach (XmlNode stepNode in node.ChildNodes)
            {
                if (stepNode.Name == "Step")
                {
                    var step = new ImageModifyStepSettings();
                    step.FromXml(stepNode);
                    Steps.Add(step);
                }
                else
                    throw new Exception($"Invalid XmlNode named {stepNode.Name}.");
            }
        }

        public bool SetOutlinePatterns(WhorlDesign design)
        {
            bool foundAllPatterns = true;
            foreach (var step in Steps)
            {
                if (!step.SetOutlinePatterns(design))
                    foundAllPatterns = false;
            }
            return foundAllPatterns;
        }
    }

    public class ImageModifyStepSettings: IXml
    {
        public Pattern[] OutlinePatterns { get; set; }
        private List<Guid> outlinePatternGuids { get; } = new List<Guid>();
        public BoundModes BoundMode { get; set; }
        public ColorModes ColorMode { get; set; }
        public Color ModifiedColor { get; set; }
        public bool IsCumulative { get; set; }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(ImageModifyStepSettings);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(BoundMode), BoundMode);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(ColorMode), ColorMode);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(ModifiedColor), ModifiedColor.ToArgb());
            xmlTools.AppendXmlAttribute(xmlNode, nameof(IsCumulative), IsCumulative);
            foreach (Pattern pattern in OutlinePatterns)
            {
                XmlNode guidNode = xmlTools.CreateXmlNode("PatternGuid");
                xmlTools.AppendXmlAttribute(guidNode, "Guid", pattern.KeyGuid.ToString());
                xmlNode.AppendChild(guidNode);
            }
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            outlinePatternGuids.Clear();
            BoundMode = Tools.GetEnumXmlAttr(node, nameof(BoundMode), BoundModes.Outside);
            ColorMode = Tools.GetEnumXmlAttr(node, nameof(ColorMode), ColorModes.Set);
            int argb = Tools.GetXmlAttribute<int>(node, nameof(ModifiedColor));
            ModifiedColor = Color.FromArgb(argb);
            IsCumulative = Tools.GetXmlAttribute(node, false, nameof(IsCumulative));
            foreach (XmlNode guidNode in node.ChildNodes)
            {
                if (guidNode.Name == "PatternGuid")
                {
                    string sGuid = Tools.GetXmlAttribute<string>(guidNode, "Guid");
                    outlinePatternGuids.Add(Guid.Parse(sGuid));
                }
                else
                    throw new Exception($"Invalid XmlNode named {guidNode.Name}.");
            }
        }

        public bool SetOutlinePatterns(WhorlDesign design)
        {
            OutlinePatterns = outlinePatternGuids.Select(g => design.AllDesignPatterns.FirstOrDefault(p => p.KeyGuid == g))
                              .Where(p => p != null).ToArray();
            return OutlinePatterns.Length == outlinePatternGuids.Count;
        }
    }
}
