using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public abstract class BaseInfluenceLinkParent
    {
        public enum RandomModes
        {
            None,
            Transform,
            PixelRendering
        }
        public InfluenceLinkParentCollection ParentCollection { get; }

        public string ParameterName { get; protected set; }

        public virtual string ParameterKey => ParameterName;

        //INFL Legacy
        //public int InfluencePointId { get; private set; }

        private Dictionary<int, InfluenceLink> influenceLinksById { get; } =
            new Dictionary<int, InfluenceLink>();

        public IEnumerable<InfluenceLink> InfluenceLinks => influenceLinksById.Values;

        /// <summary>
        /// Weight applied to random value, if present.
        /// </summary>
        public double RandomWeight { get; set; } = 1.0;

        //INFL Legacy
        //public List<InfluenceLink> InfluenceLinkList { get; } = new List<InfluenceLink>();

        public bool HaveReferences { get; protected set; }

        //public string TransformName { get; }
        public object PropertiesObject { get; set; }
        public double OrigValue { get; protected set; }

        public RandomValues RandomValues { get; set; }

        public RandomModes RandomMode { get; private set; }

        protected BaseInfluenceLinkParent(InfluenceLinkParentCollection parentCollection)
        {
            if (parentCollection == null)
                throw new NullReferenceException("parentCollection cannot be null.");
            ParentCollection = parentCollection;
        }

        public BaseInfluenceLinkParent(InfluenceLinkParentCollection parentCollection, string parameterName)
               : this(parentCollection)
        {
            if (parameterName == null)
                throw new NullReferenceException("parameterName cannot be null.");
            ParameterName = parameterName;
        }

        public InfluenceLink GetInfluencePointInfluenceLink(int pointId)
        {
            if (!influenceLinksById.TryGetValue(pointId, out var influenceLink))
            {
                influenceLink = null;
            }
            return influenceLink;
        }

        public void AddInfluenceLink(InfluenceLink influenceLink)
        {
            if (influenceLink.InfluencePointId == 0)
                throw new Exception("influenceLink.InfluencePointId cannot be 0.");
            if (influenceLink.Parent != this)
            {
                if (influenceLink.Parent != null)
                    throw new Exception("influenceLink must be removed before being added.");
                influenceLink.Parent = this;
            }
            try
            {
                //if (GetInfluencePointInfluenceLink(influenceLink.InfluencePointId) != influenceLink)  //Not already added.
                influenceLinksById.Add(influenceLink.InfluencePointId, influenceLink);
            }
            catch (Exception ex)
            {
                throw new Exception($"Duplicate InfluencePointId: {influenceLink.InfluencePointId}.", ex);
            }
        }

        public bool RemoveInfluenceLink(int pointId)
        {
            if (pointId == 0)
                throw new Exception("pointId cannot be 0.");
            bool removed = influenceLinksById.TryGetValue(pointId, out var influenceLink);
            if (removed)
            {
                if (influenceLink.InfluencePointInfo != null)
                    influenceLink.InfluencePointInfo.RemovedFromList -= influenceLink.InfluencePointInfo_RemovedFromList;
                influenceLinksById.Remove(pointId);
                influenceLink.Parent = null;
            }
            return removed;
        }

        public bool RemoveInfluenceLink(InfluenceLink influenceLink)
        {
            if (influenceLink.InfluencePointId != 0)
                return RemoveInfluenceLink(influenceLink.InfluencePointId);
            else
                return false;
        }

        protected abstract BaseInfluenceLinkParent _GetCopy(InfluenceLinkParentCollection parentCollection, string parameterName = null);

        public BaseInfluenceLinkParent GetCopy(InfluenceLinkParentCollection parentCollection, string parameterName = null)
        {
            var copy = _GetCopy(parentCollection, parameterName);
            copy.RandomWeight = RandomWeight;
            if (RandomValues != null)
                copy.RandomValues = new RandomValues(RandomValues);
            foreach (var influenceLink in InfluenceLinks)
            {
                var linkCopy = influenceLink.GetCopy(copy);
                if (linkCopy.InfluencePointInfo != null)
                    copy.AddInfluenceLink(linkCopy);
            }
            return copy;
        }

        public bool ResolveReferences(StringBuilder sbErrors)
        {
            string errMessage = SetTargetParameter(ParameterName, throwException: false);
            bool retVal = errMessage == null;
            if (errMessage != null)
            {
                sbErrors.AppendLine(errMessage);
            }
            foreach (var link in InfluenceLinks.ToList())
            {
                errMessage = link.ResolveReferences();
                if (errMessage != null)
                {
                    sbErrors.AppendLine(errMessage);
                    RemoveInfluenceLink(link);
                }
            }
            return retVal;
        }

        public void SetParameterValue(DoublePoint patternPoint, bool forRendering)
        {
            if (HaveReferences)
            {
                double randomVal;
                if (RandomMode == RandomModes.Transform)
                {
                    randomVal = RandomValues.GetYValue();
                }
                else if (RandomMode == RandomModes.PixelRendering)
                {
                    randomVal = ParentCollection.PixelRenderingRandomValue;
                }
                else
                    randomVal = 0.0;
                double value = RandomWeight * randomVal;
                foreach (InfluenceLink influenceLink in InfluenceLinks)
                {
                    double influenceVal = influenceLink.GetInfluenceValue(patternPoint, forRendering);
                    double paramVal = influenceLink.GetParameterValue(influenceVal);
                    value += paramVal;
                    if (randomVal != 0.0 && influenceLink.RandomWeight != 0.0)
                    {
                        double randomAdd = influenceLink.RandomWeight * randomVal;
                        if (influenceLink.ScaleRandomWeightByLinkFactor)
                            randomAdd *= paramVal;
                        else
                            randomAdd *= influenceVal;
                        value += randomAdd;
                    }
                }
                _SetParameterValue(OrigValue + value);
            }
        }

        protected abstract void _SetParameterValue(double influenceValue);

        public virtual void Initialize()
        {
            RandomMode = RandomModes.None;
            if (RandomWeight != 0.0 || InfluenceLinks.Any(l => l.RandomWeight != 0.0))
            {
                if (ParentCollection.FormulaSettings.FormulaType == FormulaTypes.Transform)
                {
                    if (RandomValues != null)
                    {
                        RandomMode = RandomModes.Transform;
                        if (RandomValues.Settings.DomainType == RandomValues.RandomDomainTypes.Angle)
                        {
                            RandomValues.Settings.XLength = ParentCollection.ParentPattern.RotationSteps;
                        }
                        RandomValues.ResetSeed();
                        RandomValues.ComputeRandomValues();
                    }
                }
                else
                {
                    if (ParentCollection.PointsRandomOps != null)
                        RandomMode = RandomModes.PixelRendering;
                }
            }
        }

        public virtual void FinalizeSettings()
        {
        }

        protected abstract string SetTargetParameter(string parameterKey, bool throwException = true);

        //public void Initialize()
        //{
        //    if (!HaveReferences)
        //        return;
        //    foreach (var link in InfluenceLinks)
        //    {
        //        link.Initialize();
        //    }
        //}

        protected virtual void AddXml(XmlNode xmlNode, XmlTools xmlTools)
        {
        }

        protected virtual void FromAddedXml(XmlNode xmlNode)
        {
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (ParameterName == null)
                throw new NullReferenceException("ParameterName cannot be null.");
            if (xmlNodeName == null)
                xmlNodeName = GetType().Name;
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(ParameterName), ParameterName);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(RandomWeight), RandomWeight);
            AddXml(xmlNode, xmlTools);
            foreach (var link in InfluenceLinks)
            {
                link.ToXml(xmlNode, xmlTools);
            }
            if (RandomValues != null)
            {
                RandomValues.ToXml(xmlNode, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode xmlNode) //, bool forLegacy)
        {
            //if (forLegacy)
            //    InfluencePointId = Tools.GetXmlAttribute<int>(xmlNode, nameof(InfluencePointId));
            //else
            ParameterName = Tools.GetXmlAttribute<string>(xmlNode, nameof(ParameterName));
            RandomWeight = Tools.GetXmlAttribute(xmlNode, defaultValue: 1.0, nameof(RandomWeight));
            FromAddedXml(xmlNode);
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                if (childNode.Name == nameof(InfluenceLink))
                {
                    InfluenceLink influenceLink = new InfluenceLink(this, childNode); //, forLegacy);
                    AddInfluenceLink(influenceLink);
                }
                else if (childNode.Name == nameof(RandomValues))
                {
                    RandomValues = new RandomValues(childNode);
                }
                else
                {
                    throw new Exception($"Invalid XmlNode named {childNode.Name}.");
                }
            }
        }
    }

    public class ParameterInfluenceLinkParent: BaseInfluenceLinkParent
    {
        public Parameter TargetParameter { get; private set; }

        public ParameterInfluenceLinkParent(InfluenceLinkParentCollection parentCollection, string parameterName)
               : base(parentCollection, parameterName)
        {
            if (parentCollection.FormulaSettings.IsCSharpFormula)
                throw new Exception("Formula must not be in C#.");
            SetTargetParameter(parameterName, throwException: true);
        }

        public ParameterInfluenceLinkParent(InfluenceLinkParentCollection parentCollection, XmlNode node) //, bool forLegacy = false)
               : base(parentCollection)
        {
            FromXml(node); //, forLegacy);
        }

        public override void Initialize()
        {
            base.Initialize();
            if (TargetParameter != null)
                OrigValue = (double)(TargetParameter.Value ?? 0.0);
        }

        protected override void _SetParameterValue(double value)
        {
            TargetParameter.SetUsedValue(value);
        }

        protected override string SetTargetParameter(string parameterName, bool throwException = true)
        {
            string errMessage = null;
            Parameter parameter = null;
            parameter = ParentCollection.FormulaSettings.Parameters.FirstOrDefault(p => p.ParameterName == parameterName);
            if (parameter == null)
                errMessage = $"Parameter named {parameterName} was not found.";
            if (parameter != null)
            {
                ParameterName = parameterName;
                TargetParameter = parameter;
            }
            HaveReferences = TargetParameter != null;
            if (throwException && errMessage != null)
                throw new Exception(errMessage);
            return errMessage;
        }

        protected override BaseInfluenceLinkParent _GetCopy(InfluenceLinkParentCollection parentCollection, string parameterKey = null)
        {
            return new ParameterInfluenceLinkParent(parentCollection, parameterKey ?? ParameterKey);
        }
    }

    public class PropertyInfluenceLinkParent : BaseInfluenceLinkParent
    {
        private int _arrayIndex;
        public int ArrayIndex 
        {
            get => _arrayIndex;
            private set
            {
                if (value < -1)
                {
                    throw new Exception("ArrayIndex cannot be less than -1.");
                }
                _arrayIndex = value;
            }
        }
        public BaseInfluenceParameter InfluenceParameter { get; private set; }
        public override string ParameterKey => ArrayIndex == -1 ? ParameterName : $"{ParameterName}[{ArrayIndex + 1}]";

        public PropertyInfluenceLinkParent(InfluenceLinkParentCollection parentCollection, string parameterName, int arrayIndex = -1)
               : base(parentCollection, parameterName)
        {
            if (!parentCollection.FormulaSettings.IsCSharpFormula)
                throw new Exception("Formula must be in C#.");
            ArrayIndex = arrayIndex;
        }

        public PropertyInfluenceLinkParent(InfluenceLinkParentCollection parentCollection, XmlNode node) //, bool forLegacy = false)
               : base(parentCollection)
        {
            FromXml(node); //, forLegacy);
        }

        public override void Initialize()
        {
            base.Initialize();
            if (HaveReferences)
            {
                var evalInstance = ParentCollection.FormulaSettings.EvalInstance;
                object paramsObject = evalInstance?.ParamsObj;
                InfluenceParameter.Initialize(paramsObject);
                HaveReferences = InfluenceParameter.HaveReferences;
                if (HaveReferences)
                    OrigValue = InfluenceParameter.ParameterValue;
            }
        }

        /// <summary>
        /// Reset the C# formula's parameter property to its original value, after processing has finished.
        /// </summary>
        public override void FinalizeSettings()
        {
            base.FinalizeSettings();
            if (HaveReferences)
            {
                InfluenceParameter.ParameterValue = OrigValue;
            }
        }

        protected override BaseInfluenceLinkParent _GetCopy(InfluenceLinkParentCollection parentCollection, string parameterName = null)
        {
            return new PropertyInfluenceLinkParent(parentCollection, parameterName ?? ParameterName, ArrayIndex);
        }

        protected override string SetTargetParameter(string parameterName, bool throwException = true)
        {
            string errMessage = null;
            var evalInstance = ParentCollection.FormulaSettings.EvalInstance;
            if (evalInstance == null)
                errMessage = $"C# formula named {ParentCollection.FormulaSettings.FormulaName} is invalid.";
            else
            {
                var paramsObject = evalInstance.ParamsObj;
                if (paramsObject == null)
                    errMessage = "Formula has no parameters.";
                else
                {
                    if (ArrayIndex == -1)
                    {   //Parameter is not an array element.
                        var paramInfo = InfluenceParameter as InfluenceParameter;
                        if (paramInfo == null)
                        {
                            paramInfo = new InfluenceParameter();
                            InfluenceParameter = paramInfo;
                        }
                    }
                    else
                    {   //Parameter is an array element.
                        var arrayParamInfo = InfluenceParameter as InfluenceArrayParameter;
                        if (arrayParamInfo == null)
                        {
                            arrayParamInfo = new InfluenceArrayParameter();
                            InfluenceParameter = arrayParamInfo;
                        }
                        arrayParamInfo.Index = ArrayIndex;
                    }
                    InfluenceParameter.PropertyName = parameterName;
                    errMessage = InfluenceParameter.Initialize(paramsObject);
                }
            }
            if (errMessage != null)
                InfluenceParameter = null;
            HaveReferences = InfluenceParameter != null && InfluenceParameter.HaveReferences;
            if (throwException && errMessage != null)
                throw new Exception(errMessage);
            return errMessage;
        }

        protected override void _SetParameterValue(double value)
        {
            InfluenceParameter.ParameterValue = value;
        }

        protected override void AddXml(XmlNode xmlNode, XmlTools xmlTools)
        {
            base.AddXml(xmlNode, xmlTools);
            if (ArrayIndex != -1)
            {
                xmlTools.AppendXmlAttribute(xmlNode, nameof(ArrayIndex), ArrayIndex);
            }
        }

        protected override void FromAddedXml(XmlNode xmlNode)
        {
            base.FromAddedXml(xmlNode);
            ArrayIndex = Tools.GetXmlAttribute<int>(xmlNode, defaultValue: -1, nameof(ArrayIndex));
        }
    }

    public class InfluenceLink
    {
        private BaseInfluenceLinkParent _parent;
        public BaseInfluenceLinkParent Parent 
        {
            get => _parent;
            set
            {
                if (_parent != null && value != null)
                    throw new Exception("Invalid Parent value.");
                _parent = value;
            }
        }

        public double LinkFactor { get; set; } = 1.0;

        public bool Multiply { get; set; } = true;

        /// <summary>
        /// Weight applied to random value, if present.
        /// </summary>
        public double RandomWeight { get; set; } = 0.0;

        public bool ScaleRandomWeightByLinkFactor { get; set; } = true;

        private InfluencePointInfo _influencePointInfo;
        public InfluencePointInfo InfluencePointInfo
        {
            get => _influencePointInfo;
            set
            {
                if (_influencePointInfo == value)
                    return;
                BaseInfluenceLinkParent parent = Parent;  //RemoveInfluenceLink sets Parent to null.
                bool addToParent = _influencePointInfo != null && parent != null;
                if (addToParent)
                {
                    parent.RemoveInfluenceLink(this);
                }
                _influencePointInfo = value;
                if (_influencePointInfo != null)
                {
                    InfluencePointId = _influencePointInfo.Id;
                    _influencePointInfo.RemovedFromList += InfluencePointInfo_RemovedFromList;
                    if (addToParent)
                    {
                        parent.AddInfluenceLink(this);
                    }
                }
            }
        }
        public int InfluencePointId { get; private set; }
        //INFL Legacy
        //public string ParameterName { get; private set; }

        private InfluenceLink(BaseInfluenceLinkParent parent)
        {
            if (parent == null)
                throw new NullReferenceException("parent cannot be null.");
            Parent = parent;
        }

        public InfluenceLink(BaseInfluenceLinkParent parent, InfluencePointInfo influencePointInfo): this(parent)
        {
            if (influencePointInfo == null)
                throw new NullReferenceException("influencePointInfo cannot be null.");
            InfluencePointInfo = influencePointInfo;
        }

        public InfluenceLink(BaseInfluenceLinkParent parent, XmlNode xmlNode) : this(parent) //, bool forLegacy = false) : this(parent)
        {
            FromXml(xmlNode); //, forLegacy);
        }

        public void InfluencePointInfo_RemovedFromList(object sender, EventArgs e)
        {
            try
            {
                if (Parent == null) return;
                foreach (var parent in Parent.ParentCollection.GetLinkParents().ToList())
                {
                    parent.RemoveInfluenceLink(InfluencePointInfo.Id);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public InfluenceLink GetCopy(BaseInfluenceLinkParent parent, bool setInfluencePoint = true)
        {
            InfluenceLink copy = new InfluenceLink(parent);
            if (setInfluencePoint)
                copy.InfluencePointInfo = GetInfluencePointInfo(parent.ParentCollection);
            copy.LinkFactor = LinkFactor;
            copy.Multiply = Multiply;
            copy.RandomWeight = RandomWeight;
            copy.ScaleRandomWeightByLinkFactor = ScaleRandomWeightByLinkFactor;
            return copy;
        }

        /// <summary>
        /// Set point without modifying parent dictionary.
        /// </summary>
        /// <param name="influencePointInfo"></param>
        public void SetInfluencePointInfo(InfluencePointInfo influencePointInfo)
        {
            if (_influencePointInfo == influencePointInfo)
                return;
            if (_influencePointInfo != null)
            {
                _influencePointInfo.RemovedFromList -= InfluencePointInfo_RemovedFromList;
            }
            _influencePointInfo = influencePointInfo;
            if (_influencePointInfo != null)
            {
                InfluencePointId = _influencePointInfo.Id;
                _influencePointInfo.RemovedFromList += InfluencePointInfo_RemovedFromList;
            }
        }

        public string ResolveReferences()
        {
            if (InfluencePointInfo == null || InfluencePointInfo.Id != InfluencePointId)
            {
                SetInfluencePointInfo();
                if (InfluencePointInfo == null)
                    return $"Couldn't find influence point #{InfluencePointId}.";
            }
            return null;
        }


        public double GetParameterValue(double influenceValue)
        {
            influenceValue *= LinkFactor;
            if (Multiply)
                influenceValue *= Parent.OrigValue;
            return influenceValue;
        }

        public double GetInfluenceValue(DoublePoint patternPoint, bool forRendering)
        {
            return InfluencePointInfo == null ? 0.0 : InfluencePointInfo.ComputeValue(patternPoint, forRendering);
        }

        //protected abstract InfluenceLink _GetCopy(BaseInfluenceLinkParent parent);

        public InfluencePointInfo GetInfluencePointInfo(InfluenceLinkParentCollection parentCollection)
        {
            return parentCollection.ParentPattern.InfluencePointInfoList.InfluencePointInfos.FirstOrDefault(p => p.Id == InfluencePointId);
        }

        private void SetInfluencePointInfo()
        {
            InfluencePointInfo = GetInfluencePointInfo(Parent.ParentCollection);
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (InfluencePointInfo == null)
                return null;
            if (xmlNodeName == null)
                xmlNodeName = nameof(InfluenceLink);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(InfluencePointId), InfluencePointId);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(LinkFactor), LinkFactor);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(Multiply), Multiply);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(RandomWeight), RandomWeight);
            if (!ScaleRandomWeightByLinkFactor)
                xmlTools.AppendXmlAttribute(xmlNode, nameof(ScaleRandomWeightByLinkFactor), ScaleRandomWeightByLinkFactor);
            //AddXml(xmlNode, xmlTools);  //Derived classes can add XML.
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode xmlNode) //, bool forLegacy)
        {
            //if (forLegacy)
            //    ParameterName = Tools.GetXmlAttribute<string>(xmlNode, nameof(ParameterName));
            //else
            InfluencePointId = Tools.GetXmlAttribute<int>(xmlNode, nameof(InfluencePointId));
            LinkFactor = Tools.GetXmlAttribute<double>(xmlNode, nameof(LinkFactor));
            Multiply = Tools.GetXmlAttribute<bool>(xmlNode, true, nameof(Multiply));
            RandomWeight = Tools.GetXmlAttribute<double>(xmlNode, defaultValue: 0.0, nameof(RandomWeight));
            ScaleRandomWeightByLinkFactor = Tools.GetXmlAttribute(xmlNode, defaultValue: true, nameof(ScaleRandomWeightByLinkFactor));
            //FromAddedXml(xmlNode);
        }
    }

}
