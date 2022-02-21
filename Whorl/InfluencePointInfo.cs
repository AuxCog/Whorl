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
    /// <summary>
    /// Information for a point that influences parameters, based on its distance from a point for a pattern.
    /// </summary>
    public class InfluencePointInfo : GuidKey, IXml
    {
        public Pattern ParentPattern { get; private set; }
        public int Id { get; private set; }

        private DoublePoint _influencePoint;
        public DoublePoint InfluencePoint
        {
            get => _influencePoint;
            set
            {
                DoublePoint newPoint = AdjustInfluencePoint(value);
                if (_influencePoint.X != newPoint.X || _influencePoint.Y != newPoint.Y)
                {
                    _influencePoint = newPoint;
                    OnLocationChanged();
                }
            }
        }

        public DoublePoint TransformedPoint { get; set; }

        public double InfluenceFactor { get; set; }

        private double ellipseAngle { get; set; }
        private double ellipseRadians { get; set; }

        public double EllipseAngle 
        {
            get => ellipseAngle;
            set
            {
                ellipseAngle = value;
                ellipseRadians = Tools.NormalizeAngle(Tools.DegreesToRadians(ellipseAngle));
                //ellipseUnitZVector = Complex.CreateFromModulusAndArgument(1.0, radians);
            }
        }
        public double EllipseStretch { get; set; }
        public double AverageWeight { get; set; }

        private double _divFactor = 0.01;
        public double DivFactor
        {
            get => _divFactor;
            set => _divFactor = Math.Max(0.0, value);
        }

        private double _offset = 1.0;
        public double Offset
        {
            get => _offset;
            set => _offset = Math.Max(0.0001, value);
        }

        public double FunctionOffset { get; set; }

        public double Power { get; set; } = 1.0;

        private Func<double, double> TransformFunc { get; set; }

        private string _transformFunctionName;
        public string TransformFunctionName
        {
            get => _transformFunctionName;
            set
            {
                if (_transformFunctionName == value)
                    return;
                if (value == null || !staticMethodsDict.TryGetValue(value, out MethodInfo transformFn))
                {
                    throw new Exception($"Invalid TransformFunctionName: {value}");
                }
                _transformFunctionName = value;
                try
                {
                    TransformFunc = (Func<double, double>)Delegate.CreateDelegate(typeof(Func<double, double>), transformFn);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Invalid type of transform function: {value}.", ex);
                }
            }
        }

        public HashSet<string> FilterKeys { get; } = new HashSet<string>();

        public Dictionary<string, KeyEnumParameters> KeyEnumParamsDict { get; } = new Dictionary<string, KeyEnumParameters>();

        public static IEnumerable<string> TransformFunctionNames => staticMethodsDict.Keys.OrderBy(s => s);

        private static Dictionary<string, MethodInfo> staticMethodsDict { get; } = new Dictionary<string, MethodInfo>();

        public bool Enabled { get; set; } = true;

        public bool Selected { get; set; }

        public EventHandler RemovedFromList;

        private XmlNode keyEnumParamsDictXmlNode { get; set; }

        static InfluencePointInfo()
        {
            VarFunctionParameter.PopulateMethodsDict(staticMethodsDict, parameterCount: 1, ExpressionParser.StaticMethodsTypes);
        }

        public InfluencePointInfo()
        {
            TransformFunctionName = EvalMethods.IdentMethodName;
        }

        public InfluencePointInfo(InfluencePointInfo source, Pattern pattern, bool copyKeyParams = true): base(source)
        {
            CopyProperties(source, pattern, copyKeyParams);
            ParentPattern = pattern;
        }

        public static void CopyKeyParamsDict(Dictionary<string, KeyEnumParameters> copy, Dictionary<string, KeyEnumParameters> source,
                                             Pattern pattern = null)
        {
            copy.Clear();
            foreach (var kvp in source)
            {
                FormulaSettings formulaSettings = kvp.Value.Parent.FormulaSettings;
                if (pattern != null)
                {
                    formulaSettings = formulaSettings.FindByKeyGuid(pattern.GetFormulaSettings(), throwException: true);
                }
                copy.Add(kvp.Key, new KeyEnumParameters(kvp.Value, formulaSettings));
            }
        }

        public void CopyKeyParams(InfluencePointInfo source, Pattern pattern)
        {
            CopyKeyParamsDict(KeyEnumParamsDict, source.KeyEnumParamsDict, pattern);
        }

        public void CopyProperties(InfluencePointInfo source, Pattern pattern = null, bool copyKeyParams = true)
        {
            Tools.CopyProperties(this, source, excludedPropertyNames: new string[] { nameof(ParentPattern), nameof(FilterKeys), nameof(KeyEnumParamsDict) });
            FilterKeys.Clear();
            FilterKeys.UnionWith(source.FilterKeys);
            if (copyKeyParams)
                CopyKeyParamsDict(KeyEnumParamsDict, source.KeyEnumParamsDict, pattern);
        }

        public void OnLocationChanged()
        {
            if (ParentPattern == null || !ParentPattern.HasPixelRendering)
                return;
            var distInfos = ParentPattern.PixelRendering.GetDistancePatternInfos().Where(
                            i => i.DistancePatternSettings.InfluencePointId == Id);
            PointF center = GetOrigLocation();
            foreach (var distInfo in distInfos)
            {
                SetDistancePatternCenter(distInfo, center);
            }
        }

        private void SetDistancePatternCenter(Pattern.RenderingInfo.DistancePatternInfo distInfo, PointF center)
        {
            distInfo.TransformDistancePattern(ParentPattern, distInfo.DistancePattern, center);
        }

        public void SetDistancePatternCenter(Pattern.RenderingInfo.DistancePatternInfo distInfo)
        {
            SetDistancePatternCenter(distInfo, GetOrigLocation());
        }

        public void SetKeyInfosEnabled()
        {
            foreach (var keyInfo in KeyEnumParamsDict.Values)
            {
                keyInfo.IsEnabled = FilterKeys.Contains(keyInfo.Parent.EnumKey);
            }
        }

        public void CopyInfluenceLinks(InfluencePointInfo source)
        {
            if (ParentPattern == null)
                throw new NullReferenceException("ParentPattern cannot be null.");
            var influenceLinks = new List<InfluenceLink>();
            foreach (var transform in ParentPattern.Transforms.Where(t => t.Enabled))
            {
                AddInfluenceLinks(influenceLinks, transform.TransformSettings, source);
            }
            if (ParentPattern.HasPixelRendering)
            {
                AddInfluenceLinks(influenceLinks, ParentPattern.PixelRendering.FormulaSettings, source);
            }
            foreach (InfluenceLink influenceLink in influenceLinks)
            {
                var newLink = influenceLink.GetCopy(influenceLink.Parent, setInfluencePoint: false);
                newLink.InfluencePointInfo = this;
                influenceLink.Parent.AddInfluenceLink(newLink);
            }
        }

        private void AddInfluenceLinks(List<InfluenceLink> influenceLinks, FormulaSettings formulaSettings, InfluencePointInfo source)
        {
            var parentCollection = formulaSettings?.InfluenceLinkParentCollection;
            if (parentCollection != null)
            {
                influenceLinks.AddRange(parentCollection.GetLinkParents()
                                        .SelectMany(lp => lp.InfluenceLinks)
                                        .Where(il => il.InfluencePointInfo == source));
            }
        }

        //public double GetKeyFactor(object key)
        //{
        //    double factor;
        //    if (key == null || !FactorsByKey.TryGetValue(key.ToString(), out factor))
        //        factor = 0.0;
        //    return factor;
        //}

        public bool CopyRadially()
        {
            var frm = new GetValueForm("Number of New Copies:", ValidateCopies);
            if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return false;  //User cancelled.
            int copies = int.Parse(frm.ValueText);
            CopyRadially(copies);
            return true;
        }

        private string ValidateCopies(string valText)
        {
            valText = valText ?? string.Empty;
            if (int.TryParse(valText, out int copies))
            {
                if (copies > 0)
                    return null;
            }
            return "Please enter a positive integer for Number of New Copies.";
        }

        public void CopyRadially(int copies)
        {
            if (copies < 1)
                throw new Exception("copies must be a positive integer.");
            double rotation = 2.0 * Math.PI / (1.0 + copies);
            Complex zRotation = Complex.CreateFromModulusAndArgument(1.0, rotation);
            Complex zVector = new Complex(InfluencePoint.X, InfluencePoint.Y);
            for (int i = 0; i < copies; i++)
            {
                zVector *= zRotation;
                var copy = new InfluencePointInfo(this, ParentPattern);
                if (EllipseStretch != 0)
                {
                    copy.EllipseAngle = EllipseAngle + (i + 1) * 360.0 / (1.0 + copies);
                }
                copy.InfluencePoint = new DoublePoint(zVector.Re, zVector.Im);
                copy.AddToPattern(ParentPattern);
                copy.CopyInfluenceLinks(this);
            }
        }

        public void AddToPattern(Pattern pattern, bool setId = true)
        {
            if (pattern == null)
                throw new NullReferenceException("pattern cannot be null.");
            ParentPattern = pattern;
            ParentPattern.InfluencePointInfoList.AddInfluencePointInfo(this);
            if (setId)
            {
                Id = ParentPattern.InfluencePointInfoList.Count;
                while (ParentPattern.InfluencePointInfoList.InfluencePointInfos.Any(p => p != this && p.Id == Id))
                {
                    Id++;
                }
                InfluencePoint = AdjustInfluencePoint(InfluencePoint);
            }
        }

        public void OnRemoved()
        {
            RemovedFromList?.Invoke(this, EventArgs.Empty);
        }

        //public double ComputeValue(PolarPoint polarPoint) //, PointF center)
        //{
        //    return ComputeValue(GetDoublePoint(polarPoint));
        //}

        public double ComputeValue(DoublePoint patternPoint, bool forRendering, bool forAverage = false)
        {
            double factor;
            if (forAverage)
                factor = AverageWeight;
            else
                factor = Enabled ? InfluenceFactor : 0.0;
            if (factor == 0.0)
                return 0.0;
            DoublePoint point = forRendering ? TransformedPoint : InfluencePoint;
            double xDiff = patternPoint.X - point.X;
            double yDiff = patternPoint.Y - point.Y;
            if (ParentPattern != null)
            {
                double scale = ParentPattern.InfluenceScaleFactor;
                if (forRendering)
                    scale *= ParentPattern.RenderingScaleFactor;
                xDiff *= scale;
                yDiff *= scale;
            }
            double divisor = Offset + DivFactor * (xDiff * xDiff + yDiff * yDiff);
            if (Power != 1.0)
                divisor = Math.Pow(divisor, Power);
            double v = TransformFunc(1.0 / divisor + FunctionOffset);
            if (EllipseStretch != 0)
            {
                double deltaAngle = Tools.NormalizeAngle(Math.Abs(Math.Atan2(yDiff, xDiff) - ellipseRadians));
                if (deltaAngle > Math.PI)
                    deltaAngle = 2 * Math.PI - deltaAngle;
                double weight = Math.Pow(1.0 - deltaAngle / Math.PI, EllipseStretch);
                v *= weight;
            }
            return factor * v;
        }

        private PointF GetOrigLocation()
        {
            if (ParentPattern == null)
                throw new NullReferenceException("ParentPattern cannot be null.");
            return new PointF((float)InfluencePoint.X + ParentPattern.Center.X, (float)InfluencePoint.Y + ParentPattern.Center.Y);
        }

        public void Draw(Graphics g, Bitmap designBitmap, Font font)
        {
            DrawnPoint.Draw(g, designBitmap, font, GetOrigLocation(), Id.ToString(), Selected);
        }

        /// <summary>
        /// Ensure location does not overlap another influence point.
        /// </summary>
        /// <param name="influencePoint"></param>
        /// <returns></returns>
        private DoublePoint AdjustInfluencePoint(DoublePoint influencePoint)
        {
            if (ParentPattern != null)
            {
                while (ParentPattern.InfluencePointInfoList.InfluencePointInfos.Any(ip => ip != this && ip.InfluencePoint.IntegerEquals(influencePoint)))
                {
                    influencePoint.X += 1;
                }
            }
            return influencePoint;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(InfluencePointInfo);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttribute(xmlNode, "InfluencePointX", InfluencePoint.X);
            xmlTools.AppendXmlAttribute(xmlNode, "InfluencePointY", InfluencePoint.Y);
            xmlTools.AppendXmlAttributesExcept(xmlNode, this, 
                     nameof(ParentPattern), nameof(TransformedPoint), nameof(InfluencePoint), 
                     nameof(TransformFunc), nameof(Selected), nameof(FilterKeys));
            if (FilterKeys.Any())
            {
                XmlNode keysNode = xmlTools.CreateXmlNode(nameof(FilterKeys));
                foreach (string key in FilterKeys)
                {
                    var keyNode = xmlTools.CreateXmlNode("Key");
                    xmlTools.AppendXmlAttribute(keyNode, "Value", key);
                    keysNode.AppendChild(keyNode);
                }
                xmlNode.AppendChild(keysNode);
            }
            AppendKeyParamsXml(xmlNode, KeyEnumParamsDict, xmlTools);
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public static void AppendKeyParamsXml(XmlNode xmlNode, Dictionary<string, KeyEnumParameters> dict, XmlTools xmlTools)
        {
            if (dict.Any())
            {
                XmlNode keysDictNode = xmlTools.CreateXmlNode(nameof(KeyEnumParamsDict));
                foreach (var keyVal in dict)
                {
                    var keyValNode = xmlTools.CreateXmlNode("Key");
                    xmlTools.AppendXmlAttribute(keyValNode, "Value", keyVal.Key);
                    if (keyVal.Value.ParametersObject != null)
                        FormulaSettings.AppendCSharpParametersToXml(keyValNode, xmlTools, keyVal.Value.ParametersObject);
                    keysDictNode.AppendChild(keyValNode);
                }
                xmlNode.AppendChild(keysDictNode);
            }
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributesExcept(this, node, new string[] { "InfluencePointX", "InfluencePointY" });
            double x = Tools.GetXmlAttribute<double>(node, "InfluencePointX");
            double y = Tools.GetXmlAttribute<double>(node, "InfluencePointY");
            _influencePoint = new DoublePoint(x, y);
            if (node.Attributes[nameof(AverageWeight)] == null)
            {
                AverageWeight = InfluenceFactor;  //Legacy code.
            }
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == nameof(FilterKeys))
                {
                    foreach (XmlNode keyNode in childNode.ChildNodes)
                    {
                        FilterKeys.Add(Tools.GetXmlAttribute<string>(keyNode, "Value"));
                    }
                }
                else if (childNode.Name == nameof(KeyEnumParamsDict))
                {
                    keyEnumParamsDictXmlNode = childNode;
                }
            }
        }

        public void FinishFromXml()
        {
            InfluencePointInfoList.FinishFromXml(keyEnumParamsDictXmlNode, KeyEnumParamsDict);
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }

}
