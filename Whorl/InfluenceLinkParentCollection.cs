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
        private Dictionary<string, BaseInfluenceLinkParent> influenceLinkParentsByParameterKey { get; } =
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

        public BaseInfluenceLinkParent GetLinkParent(string parameterKey)
        {
            if (!influenceLinkParentsByParameterKey.TryGetValue(parameterKey, out var linkParent))
                linkParent = null;
            return linkParent;
        }

        public void AddLinkParent(BaseInfluenceLinkParent linkParent)
        {
            influenceLinkParentsByParameterKey.Add(linkParent.ParameterKey, linkParent);
        }

        public void SetLinkParent(BaseInfluenceLinkParent linkParent)
        {
            influenceLinkParentsByParameterKey[linkParent.ParameterKey] = linkParent;
        }

        public void RemoveLinkParent(BaseInfluenceLinkParent linkParent)
        {
            influenceLinkParentsByParameterKey.Remove(linkParent.ParameterKey);
        }

        public IEnumerable<BaseInfluenceLinkParent> GetLinkParents()
        {
            return influenceLinkParentsByParameterKey.Values;
        }

        public void ClearLinkParents()
        {
            influenceLinkParentsByParameterKey.Clear();
        }

        public InfluenceLinkParentCollection GetCopy(FormulaSettings formulaSettings, Pattern pattern)
        {
            var copy = new InfluenceLinkParentCollection(pattern, formulaSettings);
            foreach (var linkParent in GetLinkParents())
            {
                copy.AddLinkParent(linkParent.GetCopy(copy));
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
            foreach (var linkParent in GetLinkParents().ToList())
            {
                if (!linkParent.ResolveReferences(sbErrors))
                    RemoveLinkParent(linkParent);
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
                foreach (var linkParent in GetLinkParents())
                {
                    linkParent.Initialize();
                }
            }
        }

        public void CleanUp()
        {
            if (Enabled)
            {
                foreach (RandomValues randomValues in GetRandomValues())
                {
                    randomValues.ClearValues();
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
                foreach (var linkParent in GetLinkParents())
                {
                    linkParent.FinalizeSettings();
                }
            }
        }

        public void SetParameterValues(DoublePoint patternPoint, bool forRendering)
        {
            if (!Enabled)
                return;
            foreach (var linkParent in GetLinkParents())
            {
                linkParent.SetParameterValue(patternPoint, forRendering);
            }
        }

        public IEnumerable<RandomValues> GetRandomValues()
        {
            return GetLinkParents().Select(p => p.RandomValues).Where(r => r != null);
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(InfluenceLinkParentCollection);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (var linkParent in GetLinkParents())
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
                AddLinkParent(linkParent);
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
