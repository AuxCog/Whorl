﻿using ParserEngine;
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
    public class InfluencePointInfo : IXml
    {
        public Pattern ParentPattern { get; private set; }
        public int Id { get; private set; }

        public DoublePoint TransformedPoint { get; set; }

        private DoublePoint _influencePoint;
        public DoublePoint InfluencePoint
        {
            get => _influencePoint;
            set
            {
                _influencePoint = AdjustInfluencePoint(value);
            }
        }

        public double InfluenceFactor { get; set; }
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
        public Dictionary<string, KeyEnumInfo> FilterKeysDict { get; private set; } = new Dictionary<string, KeyEnumInfo>();

        public static IEnumerable<string> TransformFunctionNames => staticMethodsDict.Keys.OrderBy(s => s);

        private static Dictionary<string, MethodInfo> staticMethodsDict { get; } = new Dictionary<string, MethodInfo>();

        public bool Enabled { get; set; } = true;

        public bool Selected { get; set; }

        public EventHandler RemovedFromList;

        private XmlNode filterKeysDictXmlNode { get; set; }

        static InfluencePointInfo()
        {
            VarFunctionParameter.PopulateMethodsDict(staticMethodsDict, parameterCount: 1, ExpressionParser.StaticMethodsTypes);
        }

        public InfluencePointInfo()
        {
            TransformFunctionName = EvalMethods.IdentMethodName;
        }

        public InfluencePointInfo(InfluencePointInfo source, Pattern pattern)
        {
            CopyProperties(source);
            ParentPattern = pattern;
        }

        ///// <summary>
        ///// Set InfluencePoint without setting OrigInfluencePoint.
        ///// </summary>
        ///// <param name="doublePoint"></param>
        //public void SetInfluencePoint(DoublePoint doublePoint, bool setOrigPoint)
        //{
        //    if (setOrigPoint)
        //        TransformedInfluencePoint = doublePoint;
        //    else
        //        _influencePoint = doublePoint;
        //}

        public void CopyProperties(InfluencePointInfo source)
        {
            Tools.CopyProperties(this, source, excludedPropertyNames: new string[] { nameof(ParentPattern), nameof(Id), nameof(FilterKeysDict) });
            Id = source.Id;
            FilterKeys.Clear();
            FilterKeys.UnionWith(source.FilterKeys);
            FilterKeysDict = new Dictionary<string, KeyEnumInfo>(source.FilterKeysDict);
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
                influenceLinks.AddRange(parentCollection.InfluenceLinkParentsByParameterName.Values
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
            return factor * TransformFunc(1.0 / divisor + FunctionOffset);
        }

        //public static DoublePoint GetDoublePoint(PolarPoint polarPoint, PointF center)
        //{
        //    DoublePoint doublePoint = polarPoint.ToRectangular();
        //    doublePoint.X += center.X;
        //    doublePoint.Y += center.Y;
        //    return doublePoint;
        //}

        public void Draw(Graphics g, Bitmap designBitmap, Font font)
        {
            const float crossWidth = 1F;
            const float rectWidth = 2F;
            if (ParentPattern == null)
                throw new NullReferenceException("ParentPattern cannot be null.");
            PointF p = new PointF((float)InfluencePoint.X + ParentPattern.Center.X, (float)InfluencePoint.Y + ParentPattern.Center.Y);
            Color penColor = Color.Black;
            if (designBitmap != null)
            {
                int pX = (int)p.X, 
                    pY = (int)p.Y;
                if (pX >= 0 && pY >= 0 && pX < designBitmap.Width && pY < designBitmap.Height)
                {
                    bool isLight = Tools.ColorIsLight(designBitmap.GetPixel(pX, pY));
                    penColor = isLight ? Color.Black : Color.White; //Tools.InverseColor(designBitmap.GetPixel(pX, pY));
                }
            }
            using (var pen = new Pen(penColor))
            {
                string idText = Id.ToString();
                SizeF textSize = g.MeasureString(idText, font);
                var rectF = new RectangleF(new PointF(p.X - crossWidth, p.Y - crossWidth),
                                           new SizeF(rectWidth, rectWidth));
                //Draw Id:
                using (var brush = new SolidBrush(penColor))
                {
                    g.DrawString(idText, font, brush, new PointF(rectF.Left - textSize.Width, rectF.Top));
                }
                //Draw a cross at point's location:
                g.DrawLine(pen,
                           new PointF(rectF.Left, p.Y),
                           new PointF(rectF.Right, p.Y));
                g.DrawLine(pen,
                           new PointF(p.X, rectF.Top),
                           new PointF(p.X, rectF.Bottom));
                if (Selected)
                {
                    //Draw a circle around the cross:
                    g.DrawEllipse(pen, rectF);
                }
            }
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
            if (FilterKeysDict.Any())
            {
                XmlNode keysDictNode = xmlTools.CreateXmlNode(nameof(FilterKeysDict));
                foreach (var keyVal in FilterKeysDict)
                {
                    var keyValNode = xmlTools.CreateXmlNode("Key");
                    xmlTools.AppendXmlAttribute(keyValNode, "Value", keyVal.Key);
                    FormulaSettings.AppendCSharpParametersToXml(keyValNode, xmlTools, keyVal.Value.ParametersObject);
                    //xmlTools.AppendXmlAttribute(keyValNode, "Value", keyVal.Value);
                    keysDictNode.AppendChild(keyValNode);
                }
                xmlNode.AppendChild(keysDictNode);
            }
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributesExcept(this, node, new string[] { "InfluencePointX", "InfluencePointY" });
            double x = Tools.GetXmlAttribute<double>(node, "InfluencePointX");
            double y = Tools.GetXmlAttribute<double>(node, "InfluencePointY");
            InfluencePoint = new DoublePoint(x, y);
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
                else if (childNode.Name == nameof(FilterKeysDict))
                {
                    filterKeysDictXmlNode = childNode;
                    //foreach (XmlNode keyValNode in childNode.ChildNodes)
                    //{
                    //    string key = Tools.GetXmlAttribute<string>(keyValNode, "Key");
                    //    double factor = Tools.GetXmlAttribute<double>(keyValNode, "Value");
                    //    FactorsByKey.Add(key, factor);
                    //}
                }
            }
        }

        public void FinishFromXml()
        {
            if (filterKeysDictXmlNode == null)
                return;

        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }

    public class InfluencePointInfoList : IXml
    {
        public Pattern ParentPattern { get; }
        private List<InfluencePointInfo> influencePointInfoList { get; } =
            new List<InfluencePointInfo>();
        public IEnumerable<InfluencePointInfo> InfluencePointInfos => influencePointInfoList;
        public int Count => influencePointInfoList.Count;

        public InfluencePointInfoList(Pattern pattern)
        {
            if (pattern == null)
                throw new NullReferenceException("pattern cannot be null.");
            ParentPattern = pattern;
        }

        /// <summary>
        /// Create copy of source.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        public InfluencePointInfoList(InfluencePointInfoList source, Pattern pattern) : this(pattern)
        {
            influencePointInfoList.AddRange(source.InfluencePointInfos.Select(ip => new InfluencePointInfo(ip, pattern)));
        }

        public void AddInfluencePointInfo(InfluencePointInfo influencePointInfo)
        {
            influencePointInfoList.Add(influencePointInfo);
        }

        public bool RemoveInfluencePointInfo(InfluencePointInfo influencePointInfo)
        {
            bool removed = influencePointInfoList.Remove(influencePointInfo);
            if (removed)
            {
                influencePointInfo.OnRemoved();  //Raises event.
            }
            return removed;
        }

        public IEnumerable<InfluencePointInfo> GetFilteredInfluencePointInfos(string key)
        {
            return InfluencePointInfos.Where(ip => key != null && ip.FilterKeysDict.ContainsKey(key));
        }

        public IEnumerable<InfluencePointInfo> GetFilteredInfluencePointInfos(IEnumerable<string> keys, bool useOr = false, bool include = true)
        {
            return InfluencePointInfos.Where(ip => ip.EvalBool(keys, 
                                            (_, a) => a != null && ip.FilterKeysDict.ContainsKey(a), useOr)
                                            == include);
        }

        public double ComputeAverage(DoublePoint patternPoint, bool forRendering)
        {
            if (influencePointInfoList.Any())
                return influencePointInfoList.Select(ip => ip.ComputeValue(patternPoint, forRendering, forAverage: true)).Average();
            else
                return 0;
        }

        public void Clear()
        {
            influencePointInfoList.Clear();
        }

        public void TransformInfluencePoints(Complex zFactor)
        {
            foreach (InfluencePointInfo pointInfo in InfluencePointInfos)
            {
                Complex zP = zFactor * new Complex(pointInfo.InfluencePoint.X, pointInfo.InfluencePoint.Y);
                pointInfo.InfluencePoint = new DoublePoint(zP.Re, zP.Im);
            }
        }

        //public void TransformInfluencePoints(double scale, double rotation, bool setOrigPoints = false)
        //{
        //    var zFactor = Complex.CreateFromModulusAndArgument(scale, rotation);
        //    TransformInfluencePoints(zFactor, setOrigPoints);
        //}

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(InfluencePointInfoList);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (var info in influencePointInfoList)
            {
                info.ToXml(xmlNode, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name != nameof(InfluencePointInfo))
                    throw new Exception("Invalid XML.");
                var influencePointInfo = new InfluencePointInfo();
                influencePointInfo.FromXml(childNode);
                influencePointInfo.AddToPattern(ParentPattern, setId: false);
            }
        }
    }
}
