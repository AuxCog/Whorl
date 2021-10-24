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
        public InfluenceLinkParentCollection ParentCollection { get; }

        public string ParameterName { get; protected set; }

        public virtual string ParameterKey => ParameterName;

        //INFL Legacy
        //public int InfluencePointId { get; private set; }

        private Dictionary<int, InfluenceLink> influenceLinksById { get; } =
            new Dictionary<int, InfluenceLink>();

        public IEnumerable<InfluenceLink> InfluenceLinks => influenceLinksById.Values;

        //INFL Legacy
        //public List<InfluenceLink> InfluenceLinkList { get; } = new List<InfluenceLink>();

        public bool HaveReferences { get; protected set; }

        //public string TransformName { get; }
        public object PropertiesObject { get; set; }
        public double OrigValue { get; protected set; }

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
            return RemoveInfluenceLink(influenceLink.InfluencePointId);
        }

        protected abstract BaseInfluenceLinkParent _GetCopy(InfluenceLinkParentCollection parentCollection);

        public BaseInfluenceLinkParent GetCopy(InfluenceLinkParentCollection parentCollection)
        {
            var copy = _GetCopy(parentCollection);
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
                double value = InfluenceLinks.Select(l => l.GetParameterValue(patternPoint, forRendering)).Sum();
                _SetParameterValue(OrigValue + value);
            }
        }

        protected abstract void _SetParameterValue(double influenceValue);
        public abstract void Initialize();

        public virtual void FinalizeSettings()
        {
        }

        protected abstract string SetTargetParameter(string parameterName, bool throwException = true);

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
            AddXml(xmlNode, xmlTools);
            foreach (var link in InfluenceLinks)
            {
                link.ToXml(xmlNode, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode xmlNode) //, bool forLegacy)
        {
            //if (forLegacy)
            //    InfluencePointId = Tools.GetXmlAttribute<int>(xmlNode, nameof(InfluencePointId));
            //else
            ParameterName = Tools.GetXmlAttribute<string>(xmlNode, nameof(ParameterName));
            FromAddedXml(xmlNode);
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                //INFL Legacy
                if (childNode.Name != nameof(InfluenceLink)) // && childNode.Name != "TransformInfluenceLink")
                    throw new Exception("Invalid node name found in XML.");
                InfluenceLink influenceLink = new InfluenceLink(this, childNode); //, forLegacy);
                //if (forLegacy)
                //    InfluenceLinkList.Add(influenceLink);
                //else
                AddInfluenceLink(influenceLink);
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

        protected override BaseInfluenceLinkParent _GetCopy(InfluenceLinkParentCollection parentCollection)
        {
            return new ParameterInfluenceLinkParent(parentCollection, ParameterName);
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
        //public PropertyInfo TargetPropertyInfo { get; private set; }
        //public ParameterInfoAttribute ParameterInfoAttribute { get; private set; }
        //private object paramsObject { get; set; }
        public override string ParameterKey => InfluenceParameter == null ? base.ParameterKey : InfluenceParameter.ToString();

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

        protected override BaseInfluenceLinkParent _GetCopy(InfluenceLinkParentCollection parentCollection)
        {
            return new PropertyInfluenceLinkParent(parentCollection, ParameterName);
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

    //public class PropertyInfluenceLinkParent : BaseInfluenceLinkParent
    //{
    //    public PropertyInfo TargetPropertyInfo { get; private set; }
    //    public ParameterInfoAttribute ParameterInfoAttribute { get; private set; }
    //    private object paramsObject { get; set; }

    //    public PropertyInfluenceLinkParent(InfluenceLinkParentCollection parentCollection, string parameterName)
    //           : base(parentCollection, parameterName)
    //    {
    //        if (!parentCollection.FormulaSettings.IsCSharpFormula)
    //            throw new Exception("Formula must be in C#.");
    //    }

    //    public PropertyInfluenceLinkParent(InfluenceLinkParentCollection parentCollection, XmlNode node) //, bool forLegacy = false)
    //           : base(parentCollection)
    //    {
    //        FromXml(node); //, forLegacy);
    //    }

    //    public override void Initialize()
    //    {
    //        if (HaveReferences)
    //        {
    //            var evalInstance = ParentCollection.FormulaSettings.EvalInstance;
    //            paramsObject = evalInstance?.ParamsObj;
    //            if (paramsObject == null)
    //                HaveReferences = false;
    //            else
    //            {
    //                OrigValue = (double)TargetPropertyInfo.GetValue(paramsObject);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Reset the C# formula's parameter property to its original value, after processing has finished.
    //    /// </summary>
    //    public override void FinalizeSettings()
    //    {
    //        base.FinalizeSettings();
    //        if (HaveReferences)
    //        {
    //            TargetPropertyInfo.SetValue(paramsObject, OrigValue);
    //        }
    //    }

    //    protected override BaseInfluenceLinkParent _GetCopy(InfluenceLinkParentCollection parentCollection)
    //    {
    //        return new PropertyInfluenceLinkParent(parentCollection, ParameterName);
    //    }

    //    protected override string SetTargetParameter(string parameterName, bool throwException = true)
    //    {
    //        string errMessage = null;
    //        TargetPropertyInfo = null;
    //        var evalInstance = ParentCollection.FormulaSettings.EvalInstance;
    //        if (evalInstance == null)
    //            errMessage = $"C# formula named {ParentCollection.FormulaSettings.FormulaName} is invalid.";
    //        else
    //        {
    //            var paramsObject = evalInstance.ParamsObj;
    //            if (paramsObject == null)
    //                errMessage = "Formula has no parameters.";
    //            else
    //            {
    //                PropertyInfo prpInfo = paramsObject.GetType().GetProperty(ParameterName, BindingFlags.Public | BindingFlags.Instance);
    //                if (prpInfo == null)
    //                {
    //                    errMessage = $"Didn't find parameter property named {ParameterName}.";
    //                }
    //                else if (prpInfo.PropertyType != typeof(double))
    //                {
    //                    errMessage = $"Parameter property named {ParameterName} is not of type double.";
    //                }
    //                else
    //                {
    //                    TargetPropertyInfo = prpInfo;
    //                    ParameterInfoAttribute = TargetPropertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
    //                }
    //            }
    //        }
    //        HaveReferences = TargetPropertyInfo != null;
    //        if (throwException && errMessage != null)
    //            throw new Exception(errMessage);
    //        return errMessage;
    //    }

    //    protected override void _SetParameterValue(double value)
    //    {
    //        if (ParameterInfoAttribute != null)
    //        {
    //            if (value < ParameterInfoAttribute.MinValue)
    //                value = ParameterInfoAttribute.MinValue;
    //            else if (value > ParameterInfoAttribute.MaxValue)
    //                value = ParameterInfoAttribute.MaxValue;
    //        }
    //        TargetPropertyInfo.SetValue(paramsObject, value);
    //    }
    //}

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

        private InfluencePointInfo _influencePointInfo;
        public InfluencePointInfo InfluencePointInfo
        {
            get => _influencePointInfo;
            set
            {
                if (_influencePointInfo == value)
                    return;
                bool addToParent = _influencePointInfo != null;
                if (addToParent)
                {
                    Parent.RemoveInfluenceLink(this);
                }
                _influencePointInfo = value;
                if (_influencePointInfo != null)
                {
                    InfluencePointId = _influencePointInfo.Id;
                    _influencePointInfo.RemovedFromList += InfluencePointInfo_RemovedFromList;
                    if (addToParent)
                    {
                        Parent.AddInfluenceLink(this);
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
                foreach (var parent in Parent.ParentCollection.InfluenceLinkParentsByParameterName.Values.ToList())
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
            //if (Parent.ParentCollection.ParentPattern.LastEditedInfluenceLink == this)
            //{
            //    Parent.ParentCollection.ParentPattern.CopiedLastEditedInfluenceLink = copy;
            //}
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

        public double GetParameterValue(DoublePoint patternPoint, bool forRendering)
        {
            if (InfluencePointInfo == null)
                return 0.0;
            double influenceValue = InfluencePointInfo.ComputeValue(patternPoint, forRendering);
            double value = LinkFactor * influenceValue;
            if (Multiply)
                value *= Parent.OrigValue;
            return value;
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
            //FromAddedXml(xmlNode);
        }
    }

    //public class TransformInfluenceLink : InfluenceLink
    //{
    //    public TransformInfluenceLink(BaseInfluenceLinkParent parent, XmlNode xmlNode) : base(parent)
    //    {
    //        FromXml(xmlNode);
    //    }

    //    public TransformInfluenceLink(BaseInfluenceLinkParent parent, InfluencePointInfo influencePointInfo) : base(parent, influencePointInfo)
    //    {
    //    }

    //    protected override InfluenceLink _GetCopy(BaseInfluenceLinkParent parent)
    //    {
    //        return new TransformInfluenceLink(parent, GetInfluencePointInfo(parent.ParentCollection));
    //    }

    //    public override string ResolveReferences(bool throwException = true)
    //    {
    //        string errMessage = SetTargetParameter(ParameterName, throwException);
    //        return errMessage;
    //    }

    //    public override void Initialize()
    //    {
    //        if (TargetParameter != null)
    //            origValue = (double)(TargetParameter.Value ?? 0.0);
    //    }

    //    public override void SetParameterValue(double influenceValue)
    //    {
    //        if (TargetParameter != null)
    //        {
    //            TargetParameter.SetUsedValue(GetParameterValue(influenceValue));
    //        }
    //    }

    //}

    //public class PropertyInfluenceLink : InfluenceLink
    //{

    //    public PropertyInfluenceLink(BaseInfluenceLinkParent parent, XmlNode xmlNode): base(parent)
    //    {
    //        FromXml(xmlNode);
    //    }

    //    public PropertyInfluenceLink(BaseInfluenceLinkParent parent, string parameterName, string basePropertyName)
    //                                : base(parent)
    //    {
    //        if (parameterName == null)
    //            throw new NullReferenceException("parameterName cannot be null.");
    //        if (basePropertyName == null)
    //            throw new NullReferenceException("basePropertyName cannot be null.");
    //        ParameterName = parameterName;
    //        BasePropertyName = basePropertyName;
    //    }

    //    protected override InfluenceLink _GetCopy(BaseInfluenceLinkParent parent)
    //    {
    //        return new PropertyInfluenceLink(parent, ParameterName, BasePropertyName);
    //    }

    //    public override string ResolveReferences(bool throwException = true)
    //    {
    //        string errMessage = base.ResolveReferences(throwException);
    //        if (errMessage != null)
    //            return errMessage;
    //        HaveReferences = Parent.PropertiesObject != null;
    //        if (HaveReferences)
    //        {
    //            Type type = Parent.PropertiesObject.GetType();
    //            ParameterPropertyInfo = type.GetProperty(ParameterName);
    //            if (ParameterPropertyInfo != null && ParameterPropertyInfo.PropertyType != typeof(double))
    //                ParameterPropertyInfo = null;
    //            BasePropertyInfo = type.GetProperty(BasePropertyName);
    //            if (BasePropertyInfo != null && BasePropertyInfo.PropertyType != typeof(double))
    //                BasePropertyInfo = null;
    //            ParameterInfoAttribute = ParameterPropertyInfo?.GetCustomAttribute<ParameterInfoAttribute>();
    //            if (ParameterPropertyInfo == null)
    //                errMessage = $"Target Property {ParameterName} was not found.";
    //            if (BasePropertyInfo == null)
    //            {
    //                if (errMessage == null)
    //                    errMessage = string.Empty;
    //                else
    //                    errMessage += Environment.NewLine;
    //                errMessage += $"Base Property {BasePropertyName} was not found.";
    //            }
    //            HaveReferences = errMessage == null;
    //        }
    //        if (throwException && errMessage != null)
    //            throw new Exception(errMessage);
    //        return errMessage;
    //    }

    //    public override void Initialize()
    //    {
    //        if (!HaveReferences)
    //            return;
    //        origValue = (double)BasePropertyInfo.GetValue(Parent.PropertiesObject);
    //    }

    //    public override void SetParameterValue(double influenceValue)
    //    {
    //        if (!HaveReferences)
    //            return;
    //        double newValue = GetParameterValue(influenceValue);
    //        if (ParameterInfoAttribute != null)
    //        {
    //            if (newValue < ParameterInfoAttribute.MinValue)
    //                newValue = ParameterInfoAttribute.MinValue;
    //            else if (newValue > ParameterInfoAttribute.MaxValue)
    //                newValue = ParameterInfoAttribute.MaxValue;
    //        }
    //        ParameterPropertyInfo.SetValue(Parent.PropertiesObject, newValue);
    //    }


    //    protected override void AddXml(XmlNode xmlNode, XmlTools xmlTools)
    //    {
    //        xmlTools.AppendXmlAttribute(xmlNode, nameof(BasePropertyName), BasePropertyName);
    //    }

    //    protected override void FromAddedXml(XmlNode xmlNode)
    //    {
    //        BasePropertyName = Tools.GetXmlAttribute<string>(xmlNode, nameof(BasePropertyName));
    //    }
    //}
}
