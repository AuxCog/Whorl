using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class InfluenceLinkParentCollection: IXml
    {
        public Dictionary<string, BaseInfluenceLinkParent> InfluenceLinkParentsByParameterName { get; } =
           new Dictionary<string, BaseInfluenceLinkParent>();
        public Pattern ParentPattern { get; }
        public FormulaSettings FormulaSettings { get; }
        //public PatternTransform PatternTransform { get; }
        public bool Enabled { get; private set; } = true;
        public bool IsCSharpFormula { get; private set; }

        //INFL Legacy
        //public List<BaseInfluenceLinkParent> BaseInfluenceLinkParents { get; } = new List<BaseInfluenceLinkParent>();

        public InfluenceLinkParentCollection(Pattern pattern, FormulaSettings formulaSettings)
        {
            if (pattern == null)
                throw new NullReferenceException("pattern cannot be null.");
            if (formulaSettings == null)
                throw new NullReferenceException("formulaSettings cannot be null.");
            ParentPattern = pattern;
            FormulaSettings = formulaSettings;
            IsCSharpFormula = FormulaSettings.IsCSharpFormula;
            //PatternTransform = transform;
        }

        public InfluenceLinkParentCollection GetCopy(FormulaSettings formulaSettings, Pattern pattern)
        {
            var copy = new InfluenceLinkParentCollection(pattern, formulaSettings);
            foreach (var linkParent in InfluenceLinkParentsByParameterName.Values)
            {
                copy.InfluenceLinkParentsByParameterName.Add(linkParent.ParameterName, linkParent.GetCopy(copy));
            }
            copy.ResolveReferences();
            return copy;
        }

        public string ResolveReferences(bool throwException = true)
        {
            //INFL Legacy
            //foreach (TransformInfluenceLinkParent legacyLinkParent in BaseInfluenceLinkParents)
            //{
            //    foreach (InfluenceLink legacyInfluenceLink in legacyLinkParent.InfluenceLinkList)
            //    {
            //        if (!InfluenceLinkParentsByParameterName.TryGetValue(legacyInfluenceLink.ParameterName, out var parent))
            //        {
            //            parent = new TransformInfluenceLinkParent(this, legacyInfluenceLink.ParameterName);
            //            InfluenceLinkParentsByParameterName.Add(legacyInfluenceLink.ParameterName, parent);
            //        }
            //        var pointInfo = ParentPattern.InfluencePointInfoList.InfluencePointInfos.FirstOrDefault(p => p.Id == legacyLinkParent.InfluencePointId);
            //        var newLink = new InfluenceLink(parent, pointInfo);
            //        parent.AddInfluenceLink(newLink);
            //    }
            //}
            IsCSharpFormula = FormulaSettings.IsCSharpFormula;
            var sbErrors = new StringBuilder();
            foreach (var linkParent in InfluenceLinkParentsByParameterName.Values.ToList())
            {
                if (!linkParent.ResolveReferences(sbErrors))
                    InfluenceLinkParentsByParameterName.Remove(linkParent.ParameterName);
            }
            if (sbErrors.Length > 0)
            {
                if (throwException)
                    throw new Exception(sbErrors.ToString());
                else
                    return sbErrors.ToString();
            }
            else
                return null;
        }

        public void Initialize()
        {
            if (Enabled)
            {
                foreach (var linkParent in InfluenceLinkParentsByParameterName.Values)
                {
                    linkParent.Initialize();
                }
            }
        }

        /// <summary>
        /// Reset the C# formula's parameter properties to their original values, after processing has finished.
        /// (Does nothing if formula is not C#).
        /// </summary>
        public void FinalizeSettings()
        {
            if (Enabled)
            {
                foreach (var linkParent in InfluenceLinkParentsByParameterName.Values)
                {
                    linkParent.FinalizeSettings();
                }
            }
        }

        public void SetParameterValues(DoublePoint patternPoint)
        {
            if (!Enabled)
                return;
            foreach (var linkParent in InfluenceLinkParentsByParameterName.Values)
            {
                linkParent.SetParameterValue(patternPoint);
            }
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(InfluenceLinkParentCollection);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (var linkParent in InfluenceLinkParentsByParameterName.Values)
            {
                linkParent.ToXml(xmlNode, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                BaseInfluenceLinkParent linkParent;
                if (childNode.Name == nameof(ParameterInfluenceLinkParent) ||
                    childNode.Name == "TransformInfluenceLinkParent")
                {
                    linkParent = new ParameterInfluenceLinkParent(this, childNode);
                }
                else if (childNode.Name == nameof(PropertyInfluenceLinkParent))
                {
                    linkParent = new PropertyInfluenceLinkParent(this, childNode);
                }
                else
                    throw new Exception("Invalid node name found in XML.");
                InfluenceLinkParentsByParameterName.Add(linkParent.ParameterName, linkParent);
                //INFL Legacy
                //else if (childNode.Name == "InfluenceLinkParent")
                //{
                //    var linkParent = new TransformInfluenceLinkParent(this, childNode, forLegacy: true);
                //    BaseInfluenceLinkParents.Add(linkParent);
                //}
            }
        }
    }
}
