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

        private DoublePoint _influencePoint;
        public DoublePoint InfluencePoint
        {
            get => _influencePoint;
            set => _influencePoint = AdjustInfluencePoint(value);
        }

        public double InfluenceFactor { get; set; }

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

        public static IEnumerable<string> TransformFunctionNames => staticMethodsDict.Keys.OrderBy(s => s);

        private static Dictionary<string, MethodInfo> staticMethodsDict { get; } = new Dictionary<string, MethodInfo>();

        public bool Enabled { get; set; } = true;

        public bool Selected { get; set; }

        public EventHandler RemovedFromList;

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

        public void CopyProperties(InfluencePointInfo source)
        {
            Tools.CopyProperties(this, source, excludedPropertyNames: new string[] { nameof(ParentPattern), nameof(Id) });
            Id = source.Id;
        }

        public void AddToPattern(Pattern pattern)
        {
            if (pattern == null)
                throw new NullReferenceException("pattern cannot be null.");
            ParentPattern = pattern;
            ParentPattern.InfluencePointInfoList.AddInfluencePointInfo(this);
            Id = ParentPattern.InfluencePointInfoList.Count;
            while (ParentPattern.InfluencePointInfoList.InfluencePointInfos.Any(p => p != this && p.Id == Id))
            {
                Id++;
            }
            InfluencePoint = AdjustInfluencePoint(InfluencePoint);
        }

        public void OnRemoved()
        {
            RemovedFromList?.Invoke(this, EventArgs.Empty);
        }

        //public double ComputeValue(PolarPoint polarPoint) //, PointF center)
        //{
        //    return ComputeValue(GetDoublePoint(polarPoint));
        //}

        public double ComputeValue(DoublePoint patternPoint)
        {
            if (!Enabled)
                return 0.0;
            double xDiff = patternPoint.X - InfluencePoint.X;
            double yDiff = patternPoint.Y - InfluencePoint.Y;
            if (ParentPattern != null && ParentPattern.InfluenceScaleFactor != 1.0)
            {
                xDiff *= ParentPattern.InfluenceScaleFactor;
                yDiff *= ParentPattern.InfluenceScaleFactor;
            }
            double divisor = Offset + DivFactor * (xDiff * xDiff + yDiff * yDiff);
            if (Power != 1.0)
                divisor = Math.Pow(divisor, Power);
            return InfluenceFactor * TransformFunc(1.0 / divisor);
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
                     nameof(ParentPattern), nameof(InfluencePoint), nameof(TransformFunc), nameof(Selected));
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributesExcept(this, node, new string[] { "InfluencePointX", "InfluencePointY" });
            double x = Tools.GetXmlAttribute<double>(node, "InfluencePointX");
            double y = Tools.GetXmlAttribute<double>(node, "InfluencePointY");
            _influencePoint = new DoublePoint(x, y);
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }

    public class InfluencePointInfoList: IXml
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
        public InfluencePointInfoList(InfluencePointInfoList source, Pattern pattern): this(pattern)
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

        public void Clear()
        {
            influencePointInfoList.Clear();
        }

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
                influencePointInfo.AddToPattern(ParentPattern);
            }
        }
    }
}