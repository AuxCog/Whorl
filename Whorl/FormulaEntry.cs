using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public enum FormulaTypes
    {
        Unknown,
        Outline,
        Transform,
        PathVertices,
        Ribbon,
        PixelRender
    }
    public enum FormulaUsages
    {
        Normal,
        Module,
        Include
    }
    /// <summary>
    /// Class for an entry in list of formula choices, which is saved to XML.
    /// </summary>
    public class FormulaEntry : ChangeTracker, IXml, ICloneable
    {
        private FormulaTypes formulaType = FormulaTypes.Unknown;
        private string formulaName;
        private string formula;
        private string maxAmplitudeFormula;
        private bool isCSharp;
        private bool isSystem;
        private FormulaUsages formulaUsage = FormulaUsages.Normal;
        //private bool isModule;
        private bool initialized;

        public FormulaEntry(FormulaTypes formulaType)
        {
            FormulaType = formulaType;
        }

        public FormulaTypes FormulaType
        {
            get { return formulaType; }
            set
            {
                SetProperty(ref formulaType, value);
            }
        }

        public string FormulaName
        {
            get { return formulaName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception("FormulaName cannot be blank.");
                if (IsSystem && !string.IsNullOrWhiteSpace(formulaName))
                    return;  //Cannot rename system formula.
                if (formulaName != value)
                {
                    if (initialized && MainForm.FormulaEntryList.HandleRename)
                        MainForm.FormulaEntryList.RenameFormula(this, value, throwException: true);
                    else
                        SetProperty(ref formulaName, value);
                }
            }
        }

        public string Formula
        {
            get { return formula; }
            set
            {
                SetProperty(ref formula, value);
            }
        }

        /// <summary>
        /// Only used for Outline formulas.
        /// </summary>
        public string MaxAmplitudeFormula
        {
            get { return maxAmplitudeFormula; }
            set
            {
                SetProperty(ref maxAmplitudeFormula, value);
            }
        }

        public bool IsCSharp
        {
            get { return isCSharp; }
            set
            {
                SetProperty(ref isCSharp, value);
            }
        }

        public bool IsSystem
        {
            get { return isSystem; }
            set { SetProperty(ref isSystem, value); }
        }

        public FormulaUsages FormulaUsage
        {
            get => formulaUsage;
            set => SetProperty(ref formulaUsage, value);
        }

        //public bool IsModule
        //{
        //    get { return isModule; }
        //    set
        //    {
        //        SetProperty(ref isModule, value);
        //    }
        //}

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (IsCSharp)
                sb.Append("C#:");
            sb.Append($"{FormulaType}:");
            if (IsSystem)
                sb.Append("*");
            sb.Append(FormulaName);
            return sb.ToString();
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
               xmlNodeName = "FormulaEntry";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(node, "FormulaType", FormulaType);
            xmlTools.AppendXmlAttribute(node, "FormulaName", FormulaName);
            xmlTools.AppendXmlAttribute(node, "IsCSharp", IsCSharp);
            xmlTools.AppendXmlAttribute(node, nameof(FormulaUsage), FormulaUsage);
            xmlTools.AppendXmlAttribute(node, nameof(IsSystem), IsSystem);
            //xmlTools.AppendXmlAttribute(node, "IsModule", IsModule);
            xmlTools.AppendChildNode(node, "Formula", Formula);
            if (FormulaType == FormulaTypes.Outline)
                xmlTools.AppendChildNode(node, "MaxAmplitudeFormula", MaxAmplitudeFormula);
            //AppendExtraXml(node, xmlTools);
            return xmlTools.AppendToParent(parentNode, node);
        }

        //protected virtual void AppendExtraXml(XmlNode parentNode, XmlTools xmlTools)
        //{
        //}

        public void FromXml(XmlNode node)
        {
            initialized = false;
            if (FormulaType == FormulaTypes.Unknown)  //Always true except for Legacy XML.
                FormulaType = Tools.GetEnumXmlAttr(node, "FormulaType", FormulaTypes.Unknown);
            FormulaName = (string)Tools.GetXmlAttribute("FormulaName", typeof(string), node);
            IsCSharp = Tools.GetXmlAttribute<bool>(node, false, "IsCSharp");
            IsSystem = Tools.GetXmlAttribute<bool>(node, false, nameof(IsSystem));
            FormulaUsage = FormulaUsages.Normal;
            if (node.Attributes["IsModule"] != null)
            {   //Legacy code:
                bool isModule = Tools.GetXmlAttribute<bool>(node, false, "IsModule");
                if (isModule)
                    FormulaUsage = FormulaUsages.Module;
            }
            else
                FormulaUsage = Tools.GetEnumXmlAttr(node, nameof(FormulaUsage), FormulaUsages.Normal);
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Formula":
                        Formula = Tools.GetXmlNodeValue(childNode);
                        break;
                    case "MaxAmplitudeFormula":
                        MaxAmplitudeFormula = Tools.GetXmlNodeValue(childNode);
                        break;
                    default:
                        throw new Exception("Invalid XML node: " + childNode.Name);
                }
                //if (childNode.Name == "Formula")
                //    Formula = Tools.GetXmlNodeValue(childNode);
                //else if (!FromExtraXml(childNode))
                //    throw new Exception("Invalid XML node: " + childNode.Name);
            }
            if (FormulaType == FormulaTypes.Unknown)  //Legacy XML
            {
                if (!string.IsNullOrEmpty(MaxAmplitudeFormula))
                    FormulaType = FormulaTypes.Outline;
            }
            initialized = true;
        }

        public void AfterReadOrSave()
        {
            IsChanged = false;
        }

        protected virtual bool FromExtraXml(XmlNode node)
        {
            return false;
        }

        public object Clone()
        {
            var copy = new FormulaEntry(this.FormulaType);
            Tools.CopyProperties(copy, this, excludedPropertyNames: new string[] { nameof(FormulaType) });
            return copy;
        }
    }

    //public class FormulaEntry: FormulaEntry
    //{
    //    public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
    //    {
    //        if (xmlNodeName == null)
    //            xmlNodeName = "FormulaEntry";
    //        return base.ToXml(parentNode, xmlTools, xmlNodeName);
    //    }

    //    public override object Clone()
    //    {
    //        FormulaEntry copy = new FormulaEntry();
    //        Tools.CopyProperties(copy, this);
    //        return copy;
    //    }
    //}

    //public class OutlineFormulaEntry: FormulaEntry
    //{
    //    private string maxAmplitudeFormula;

    //    public string MaxAmplitudeFormula
    //    {
    //        get { return maxAmplitudeFormula; }
    //        set
    //        {
    //            SetProperty(ref maxAmplitudeFormula, value);
    //        }
    //    }

    //    public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
    //    {
    //        if (xmlNodeName == null)
    //            xmlNodeName = "OutlineFormulaEntry";
    //        return base.ToXml(parentNode, xmlTools, xmlNodeName);
    //    }

    //    protected override void AppendExtraXml(XmlNode parentNode, XmlTools xmlTools)
    //    {
    //        xmlTools.AppendChildNode(parentNode, "MaxAmplitudeFormula", maxAmplitudeFormula);
    //    }

    //    protected override bool FromExtraXml(XmlNode node)
    //    {
    //        bool retVal = node.Name == "MaxAmplitudeFormula";
    //        if (retVal)
    //            MaxAmplitudeFormula = Tools.GetXmlNodeValue(node);
    //        return retVal;
    //    }

    //    public override object Clone()
    //    {
    //        OutlineFormulaEntry copy = new OutlineFormulaEntry();
    //        Tools.CopyProperties(copy, this);
    //        return copy;
    //    }
    //}
}
