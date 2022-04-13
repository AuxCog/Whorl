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
    public class PathPattern: Pattern, ICloneable, IXml
    {
        public const float DefaultPenWidth = 2F;
        public PathPattern(WhorlDesign design, XmlNode node): base(design)
        {
            FromXml(node);
        }

        public PathPattern(Pattern sourcePattern, bool copySharedPatternID = true, WhorlDesign design = null) 
               : base(design ?? sourcePattern.Design, FillInfo.FillTypes.Path)
        {
            this.CopyProperties(sourcePattern, copySharedPatternID: copySharedPatternID);
        }

        public PathPattern(WhorlDesign design): base(design, FillInfo.FillTypes.Path)
        {
        }

        public Ribbon PathRibbon { get; set; }
        public PathOutline CartesianPathOutline { get; set; }
        public int[] CurveVertexIndices { get; private set; }
        public float PenWidth { get; set; } = DefaultPenWidth;
        public bool InterpolatePoints { get; set; }

        public override Ribbon GetRibbon()
        {
            return PathRibbon;
        }

        protected override void _DrawFilled(Graphics g, IRenderCaller caller, bool computeRandom = false,
                                        bool draftMode = false, int recursiveDepth = 0,
                                        float textureScale = 1, Complex? patternZVector = null, bool enableCache = true)
        {
            if (!PatternIsEnabled || RenderMode == RenderModes.Stain)
                return;
            Complex drawnZVector = patternZVector ?? DrawnZVector;
            if (PathRibbon != null)
            {
                if (ComputeCurvePoints(drawnZVector))
                    PathRibbon.DrawForPathPattern(g, caller, this, Center, drawnZVector, computeRandom, enableCache: enableCache,
                                                  draftMode: draftMode);
            }
            else
            {
                DrawPath(g, drawnZVector, computeRandom: true);
            }
        }

        public override bool ComputeSeedPoints(bool computeRandom = false)
        {
            if (CartesianPathOutline != null)
            {
                CartesianPathOutline.AddVertices();
                this.PreviewZFactor = new Complex(1, 0);
                return false;
            }
            else
                return base.ComputeSeedPoints();
        }

        public override bool ComputeCurvePoints(Complex zVector,
                                                bool computeRandom = true, bool forOutline = false)
        {
            if (CartesianPathOutline != null)
                return ComputeCartesianCurvePoints(zVector);
            else
            {
                if (SeedPoints == null)
                    ComputeSeedPoints();
                int[] vertexIndices = GetVertexIndices();
                if (vertexIndices == null)
                    CurveVertexIndices = null;
                else
                    CurveVertexIndices = new int[vertexIndices.Length];
                bool retVal = base.ComputeCurvePoints(zVector, computeRandom, forOutline: true);
                if (InterpolatePoints && retVal)
                {
                    CurvePoints = Tools.InterpolatePoints(CurvePoints).ToArray();
                }
                return retVal;
            }
        }

        public int[] GetVertexIndices()
        {
            int[] vertexIndices = null;
            PathOutline pthOtl = BasicOutlines.Select(otl => otl as PathOutline)
                                 .Where(po => po?.VertexIndices != null)
                                 .FirstOrDefault();
            if (pthOtl != null)
                vertexIndices = pthOtl.VertexIndices;
            else
            {
                BasicOutline rectangleOutline = BasicOutlines.Find(
                        otl => otl.BasicOutlineType == BasicOutlineTypes.Rectangle && otl.Petals == 2);
                if (rectangleOutline != null)
                {
                    vertexIndices = new int[4];
                    double ratio = Math.Min(100D, rectangleOutline.AddDenom);
                    const double height = 1;
                    double width = 1D / ratio;
                    double angle = Math.Atan2(height, width);
                    vertexIndices[0] = (int)(RotationSteps * 0.5 * angle / Math.PI);
                    angle = Math.PI - angle;
                    vertexIndices[1] = (int)(RotationSteps * 0.5 * angle / Math.PI);
                    vertexIndices[2] = vertexIndices[0] + RotationSteps / 2;
                    vertexIndices[3] = vertexIndices[1] + RotationSteps / 2;
                }
            }
            return vertexIndices;
        }

        private bool ComputeCartesianCurvePoints(Complex zVector)
        {
            PathOutline otl = CartesianPathOutline;
            if (otl == null || otl.PathPoints == null)
                return false;
            List<PointF> points = new List<PointF>();
            PointF prevPoint = PointF.Empty;
            foreach (PointF p in otl.PathPoints)
            {
                Complex zP = new Complex(p.X, p.Y);
                zP *= zVector;  //Rotate and scale.
                zP.Re += Center.X;
                zP.Im += Center.Y;
                PointF newP = new PointF((float)zP.Re, (float)zP.Im);
                if (points.Count == 0 || Tools.PointsDiffer(prevPoint, newP))
                {
                    if (points.Count > 0)
                    {
                        PointF diffP = new PointF(newP.X - prevPoint.X, newP.Y - prevPoint.Y);
                        float dist = (float)Math.Sqrt(diffP.X * diffP.X + diffP.Y * diffP.Y);
                        if (dist >= 2)
                        {
                            //Interpolate points:
                            diffP = new PointF(diffP.X / dist, diffP.Y / dist);
                            PointF iP = prevPoint;
                            while (dist >= 2)
                            {
                                iP = new PointF(iP.X + diffP.X, iP.Y + diffP.Y);
                                points.Add(iP);
                                dist--;
                            }
                        }
                    }
                    points.Add(newP);
                    prevPoint = newP;
                }
            }
            if (points.Count <= 1)
                return false;
            CurvePoints = points.ToArray();
            return true;
        }

        public override void DrawOutline(Graphics g, Color? color = null)
        {
            SetOutlinePen(color);
            DrawPath(g, OutlineZVector, computeRandom: false, pen: outlinePen);
        }

        private void DrawPath(Graphics g, Complex zVector, bool computeRandom, Pen pen = null)
        {
            if (ComputeCurvePoints(zVector))
            {
                bool disposePen = pen == null;
                if (pen == null)
                {
                    var linearGradientBrush = InitializeFillInfo(FillInfo, CurvePoints, checkLinearGradient: true);
                    pen = new Pen(linearGradientBrush ?? FillInfo.FillBrush, PenWidth);
                }
                Tools.DrawCurve(g, pen, CurvePoints);
                if (disposePen)
                    pen.Dispose();
            }
        }

        public override Pattern GetCopy(bool keepRecursiveParent = false, WhorlDesign design = null)
        {
            return new PathPattern(this, design: design);
        }

        public override object Clone()
        {
            //var copy = new PathPattern();
            //copy.CopyProperties(this);
            return new PathPattern(this);
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(PathPattern);
            var xmlNode = base.ToXml(parentNode, xmlTools, xmlNodeName);
            if (PenWidth != DefaultPenWidth)
            {
                xmlTools.AppendXmlAttribute(xmlNode, nameof(PenWidth), PenWidth);
            }
            if (InterpolatePoints)
            {
                xmlTools.AppendXmlAttribute(xmlNode, nameof(InterpolatePoints), InterpolatePoints);
            }
            return xmlNode;
        }

        public override void FromXml(XmlNode node)
        {
            base.FromXml(node);
            PenWidth = Tools.GetXmlAttribute<float>(node, defaultValue: DefaultPenWidth, nameof(PenWidth));
            if (node.Attributes[nameof(InterpolatePoints)] != null)
                InterpolatePoints = Tools.GetXmlAttribute<bool>(node, nameof(InterpolatePoints));
        }

        protected override void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            if (PathRibbon != null)
                PathRibbon.ToXml(parentNode, xmlTools, nameof(PathRibbon));
            if (CartesianPathOutline != null)
                CartesianPathOutline.ToXml(parentNode, xmlTools, nameof(CartesianPathOutline));
            //xmlTools.AppendAttributeChildNode(parentNode, nameof(InterpolatePoints), InterpolatePoints);
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            bool retVal = true;
            switch (node.Name)
            {
                case nameof(PathRibbon):
                    PathRibbon = new Ribbon(Design, node);
                    break;
                case nameof(CartesianPathOutline):
                    CartesianPathOutline = new PathOutline();
                    CartesianPathOutline.FromXml(node);
                    break;
                case nameof(InterpolatePoints):  //Legacy code.
                    InterpolatePoints = Tools.GetXmlAttribute<bool>(node);
                    break;
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }

    }
}
