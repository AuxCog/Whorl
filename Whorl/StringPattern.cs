using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ParserEngine;

namespace Whorl
{
    public class StringPattern: Pattern
    {
        private string _text = string.Empty;
        private Font _font = new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Point);

        public string Text
        {
            get { return _text; }
            set
            {
                string txt = value ?? string.Empty;
                if (_text != txt)
                {
                    _text = txt;
                }
            }
        }

        public Font Font
        {
            get { return _font; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("StringPattern.Font cannot be null.");
                if (_font != value)
                {
                    _font = value;
                }
            }
        }

        public override bool UseLinearGradient
        {
            get { return true; }
            set { }
        }

        public bool KeepRightAngle { get; set; } = true;

        protected StringPattern(WhorlDesign design): base(design)
        { }

        public StringPattern(WhorlDesign design, FillInfo.FillTypes fillType): base(design, fillType)
        { }

        public StringPattern(WhorlDesign design, XmlNode node): base(design)
        {
            FromXml(node);
        }

        public StringPattern(Pattern sourcePattern, bool copySharedPatternID = true) : base(sourcePattern.Design, FillInfo.FillTypes.Path)
        {
            if (sourcePattern != null)
                this.CopyProperties(sourcePattern, copySharedPatternID: copySharedPatternID);
        }

        private double AdjustAngle(double angle)
        {
            const double halfPi = 0.5 * Math.PI;
            const double twoPi = 2.0 * Math.PI;
            if (KeepRightAngle)
            {
                angle = halfPi * Math.Round(angle / halfPi);
            }
            if (angle < 0 || angle > twoPi)
            {
                angle = angle / twoPi;
                angle = twoPi * (angle - Math.Floor(angle));
            }
            return angle;
        }

        public override bool ComputeCurvePoints(Complex zVector, bool recomputeInnerSection = true, bool forOutline = false)
        {
            if (SeedPoints == null)
                ComputeSeedPoints();
            double angle = AdjustAngle(zVector.GetArgument());
            Size size = TextRenderer.MeasureText(Text, Font);
            double factor = size.Width == 0 ? 1 : size.Width;
            CurvePoints = SeedPoints.Select(sp => Pattern.ComputeCurvePoint(angle + sp.Angle, factor * sp.Modulus, Center)).ToArray();
            return true;
        }

        public override bool ComputeSeedPoints(bool computeRandom = false)
        {
            Size size = TextRenderer.MeasureText(Text, Font);
            double unitWidth = 1;
            double unitHeight = (double)size.Height / (size.Width == 0 ? 1 : size.Width);
            double modulus = Math.Sqrt(1D + unitHeight * unitHeight);
            unitWidth /= modulus;
            unitHeight /= modulus;
            DoublePoint[] dblPoints = new DoublePoint[5];
            dblPoints[0] = new DoublePoint(0, 0);
            dblPoints[1] = new DoublePoint(unitWidth, 0);
            dblPoints[2] = new DoublePoint(unitWidth, unitHeight);
            dblPoints[3] = new DoublePoint(0, unitHeight);
            dblPoints[4] = dblPoints[0];

            IEnumerable<PolarPoint> plrPoints = dblPoints.Select(dp => dp.ToPolar());
            SeedPoints = plrPoints.Select(pp => new PolarCoord((float)pp.Angle, (float)pp.Modulus)).ToArray();
            PreviewZFactor = new Complex(1, 0);
            return true;
        }

        public override object Clone()
        {
            return base.Clone();
        }

        public override Pattern GetCopy(bool copySharedPatternID = true, bool keepRecursiveParent = false)
        {
            StringPattern copy = new StringPattern(Design);
            copy.CopyProperties(this, copySharedPatternID: copySharedPatternID);
            return copy;
        }

        private void DrawText(Graphics g, Brush brush, Complex zVector)
        {
            double angle = Tools.RadiansToDegrees(AdjustAngle(zVector.GetArgument()));
            if (angle != 0)
            {
                g.TranslateTransform(Center.X, Center.Y);
                g.RotateTransform((float)angle);
                g.TranslateTransform(-Center.X, -Center.Y);
            }
            try
            {
                //Debug.WriteLine($"Angle = {angle}");
                g.DrawString(Text, Font, brush, Center);
            }
            finally
            {
                g.ResetTransform();
            }
        }

        public override void DrawFilled(Graphics g, IRenderCaller caller, bool computeRandom = false, bool draftMode = false, int recursiveDepth = -1, float textureScale = 1, 
                                        Complex? patternZVector = null, bool enableCache = true)
        {
            Complex zVector = patternZVector ?? DrawnZVector;
            ComputeCurvePoints(zVector);
            double angle = AdjustAngle(zVector.GetArgument());
            LinearGradientBrush linearGradientBrush = InitializeFillInfo(FillInfo, this.CurvePoints, checkLinearGradient: true, patternAngle: angle);
            Brush brush = linearGradientBrush ?? FillInfo.FillBrush;
            DrawText(g, brush, zVector);
        }

        public override void DrawOutline(Graphics g, Color? color = null)
        {
            if (color == null)
                color = Tools.InverseColor(this.BoundaryColor);
            ComputeCurvePoints(OutlineZVector);
            using (Brush brush = new SolidBrush((Color)color))
            {
                DrawText(g, brush, OutlineZVector);
            }
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "StringPattern";
            return base.ToXml(parentNode, xmlTools, xmlNodeName);
        }

        protected override void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.ExtraXml(parentNode, xmlTools);
            xmlTools.AppendAttributeChildNode(parentNode, "Text", Text);
            xmlTools.AppendAttributeChildNode(parentNode, "KeepRightAngle", KeepRightAngle);
            XmlNode fontNode = xmlTools.CreateXmlNode("Font");
            xmlTools.AppendXmlAttribute(fontNode, "FontName", Font.FontFamily.Name);
            xmlTools.AppendXmlAttribute(fontNode, "Size", Font.SizeInPoints);
            xmlTools.AppendXmlAttribute(fontNode, "Style", (int)Font.Style);
            parentNode.AppendChild(fontNode);
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            bool retVal = true;
            switch (node.Name)
            {
                case "Text":
                    Text = Tools.GetXmlAttribute<string>(node);
                    break;
                case "KeepRightAngle":
                    KeepRightAngle = Tools.GetXmlAttribute<bool>(node);
                    break;
                case "Font":
                    string fontName = Tools.GetXmlAttribute<string>(node, "FontName");
                    float size = Tools.GetXmlAttribute<float>(node, "Size");
                    int intStyle = Tools.GetXmlAttribute<int>(node, "Style");
                    Font = new Font(fontName, size, (FontStyle)intStyle, GraphicsUnit.Point);
                    break;
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }
    }
}
