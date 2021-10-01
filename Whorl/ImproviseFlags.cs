using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class ImproviseFlags: BaseObject, IXml
    {
        //Null-able Boolean properties from Settings Form (use default if null):
        //These are for improvising on pattern properties.
        public bool? ImproviseOnOutlineType { get; set; }
        public bool? ImproviseColors { get; set; }
        public bool? ImproviseShapes { get; set; }
        public bool? ImprovisePetals { get; set; }
        public bool? ImproviseParameters { get; set; }

        //Flags used when improvising, set to Properties setting if base property is null:
        public bool UsedImproviseOnOutlineType { get; private set; }
        public bool UsedImproviseColors { get; private set; }
        public bool UsedImproviseShapes { get; private set; }
        public bool UsedImprovisePetals { get; private set; }
        public bool UsedImproviseParameters { get; private set; }

        public ImproviseFlags()
        {
            SetUsedFlags();
        }

        /// <summary>
        /// Set each Used flag to Properties setting if base property is null, else to base property.
        /// </summary>
        public void SetUsedFlags()
        {
            UsedImproviseOnOutlineType = ImproviseOnOutlineType ?? 
                WhorlSettings.Instance.ImproviseOnOutlineType;
            UsedImproviseColors = ImproviseColors ?? 
                WhorlSettings.Instance.ImproviseColors;
            UsedImproviseShapes = ImproviseShapes ?? 
                WhorlSettings.Instance.ImproviseShapes;
            UsedImprovisePetals = ImprovisePetals ?? 
                WhorlSettings.Instance.ImprovisePetals;
            UsedImproviseParameters = ImproviseParameters ?? 
                WhorlSettings.Instance.ImproviseParameters;
        }

        public ImproviseFlags GetCopy()
        {
            var copy = new ImproviseFlags();
            foreach (PropertyInfo prp in typeof(ImproviseFlags).GetProperties())
            {
                if (prp.CanRead && prp.CanWrite && prp.Name.StartsWith("Improvise"))
                    prp.SetValue(copy, prp.GetValue(this));
            }
            copy.SetUsedFlags();
            return copy;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "ImproviseFlags";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttributes(node, this, nameof(ImproviseOnOutlineType), nameof(ImproviseColors), nameof(ImproviseShapes),
                                         nameof(ImprovisePetals), nameof(ImproviseParameters));
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            ImproviseOnOutlineType = (bool?)Tools.GetXmlAttribute("ImproviseOnOutlineType", 
                                            typeof(bool), node, required: false);
            ImproviseColors = (bool?)Tools.GetXmlAttribute("ImproviseColors",
                                            typeof(bool), node, required: false);
            ImproviseShapes = (bool?)Tools.GetXmlAttribute("ImproviseShapes",
                                            typeof(bool), node, required: false);
            ImprovisePetals = (bool?)Tools.GetXmlAttribute("ImprovisePetals",
                                            typeof(bool), node, required: false);
            ImproviseParameters = (bool?)Tools.GetXmlAttribute("ImproviseParameters",
                                            typeof(bool), node, required: false);
            SetUsedFlags();
        }
    }
}
