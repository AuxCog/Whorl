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
    public class MergedPattern: Pattern
    {
        public List<Pattern> Patterns { get; private set; } = new List<Pattern>();
        public Complex FirstZVector { get; private set; }
        public PointF OrigCenter { get; private set; }
        private PointF[] seedCurvePoints { get; set; }  //Normalized curve points.
        private Complex zRotation { get; set; }
        private readonly int blackArgb = Color.Black.ToArgb();
        private int[] boundsPixels { get; set; }
        private int pixelCount { get; set; }
        private Rectangle boundingRect { get; set; }
        private PointF deltaPoint { get; set; }

        public MergedPattern(WhorlDesign design): base(design, FillInfo.FillTypes.Path)
        {
            PreviewZFactor = Complex.One;
        }

        public MergedPattern(MergedPattern source, WhorlDesign design = null) : base(design ?? source.Design)
        {
            CopyProperties(source);
            Patterns.AddRange(source.Patterns.Select(p => p.GetCopy()).ToArray());
            FirstZVector = source.FirstZVector;
            zRotation = source.zRotation;
            seedCurvePoints = (PointF[])source.seedCurvePoints?.Clone();
            PreviewZFactor = Complex.One;
        }

        public MergedPattern(WhorlDesign design, XmlNode node): base(design, node)
        {
            PreviewZFactor = Complex.One;
        }

        public bool Initialize(Size picSize)
        {
            if (Patterns.Count == 0)
            {
                throw new Exception("There must be at least 1 merged pattern.");
            }
            Patterns = Patterns.OrderByDescending(p => p.ZVector.GetModulusSquared()).ToList();
            Pattern pattern1 = Patterns.First();
            FirstZVector = pattern1.ZVector;
            SetZRotation();
            OrigCenter = Center = pattern1.Center;
            FillInfo = pattern1.FillInfo.GetCopy(this);
            double factor = 1000.0 / FirstZVector.GetModulus();
            float fFac = (float)factor;
            foreach (Pattern ptn in Patterns)
            {
                ptn.ZVector *= factor;
                PointF dc = new PointF(ptn.Center.X - Center.X, ptn.Center.Y - Center.Y);
                ptn.Center = new PointF(Center.X + fFac * dc.X, Center.Y + fFac * dc.Y);
            }
            bool success = ComputeSeedCurvePoints(out Complex zVector);
            ZVector = zVector;
            return success;
        }

        private void SetZRotation()
        {
            Complex zR = FirstZVector;
            zR.Normalize();
            zR = Complex.One / zR;
            zRotation = zR;
        }

        public override bool ComputeCurvePoints(Complex zVector, bool recomputeInnerSection = true, bool forOutline = false)
        {
            if (seedCurvePoints == null)
            {
                CurvePoints = new PointF[0];
                return false;
            }
            else
            {
                zVector *= zRotation;
                PointF z = new PointF((float)zVector.Re, (float)zVector.Im);
                CurvePoints = new PointF[seedCurvePoints.Length];
                for (int i = 0; i < CurvePoints.Length; i++)
                {
                    PointF p = Tools.RotatePoint(seedCurvePoints[i], z);
                    CurvePoints[i] = new PointF(p.X + Center.X, p.Y + Center.Y);
                }
                return true;
            }
        }

        public override bool ComputeSeedPoints(bool computeRandom = false)
        {
            SeedPoints = new PolarCoord[0];
            PreviewZFactor = Complex.One;
            ClearRenderingCache();
            return true;
        }

        public override Pattern GetCopy(bool keepRecursiveParent = false, WhorlDesign design = null)
        {
            return new MergedPattern(this, design);
        }

        public void DrawBoundary(Graphics g)
        {
            GetBoundsPixels();
            for (int y = 0; y < boundingRect.Height; y++)
            {
                for (int x = 0; x < boundingRect.Width; x++)
                {
                    Point p = new Point(x, y);
                    if (IsBoundaryPoint(p))
                    {
                        g.FillRectangle(Brushes.Red, new Rectangle(p, new Size(1, 1)));
                    }
                }
            }
        }

        public bool ComputeSeedCurvePoints(out Complex zVector)
        {
            var boundPoints = GetBoundPoints();
            boundsPixels = null;
            if (boundPoints == null)
            {
                seedCurvePoints = null;
                zVector = Complex.One;
            }
            else
            {
                PointF center = OrigCenter; //Patterns.First().Center;
                var points2 = boundPoints.Select(pt =>
                    new PointF(pt.X + deltaPoint.X - center.X,
                               pt.Y + deltaPoint.Y - center.Y));
                double maxModulus = Math.Sqrt(points2.Select(p => p.X * p.X + p.Y * p.Y).Max());
                float scale = 1F / (float)maxModulus;
                var pointsList = points2.Select(p => new PointF(scale * p.X, scale * p.Y)).ToList();
                //var interpPoints = new List<PointF>();
                if (pointsList.Any())
                {
                    Tools.ClosePoints(pointsList);
                    //if (pointsList.Count >= RotationSteps)
                    //    interpPoints = pointsList;
                    //else
                    //{
                    //    int steps = (int)Math.Ceiling((double)RotationSteps / pointsList.Count);
                    //    float fac = 1F / steps;
                    //    PointF prevP = pointsList[0];
                    //    for (int i = 1; i < pointsList.Count; i++)
                    //    {
                    //        interpPoints.Add(prevP);
                    //        PointF p = pointsList[i];
                    //        PointF dp = new PointF(p.X - prevP.X, p.Y - prevP.Y);
                    //        for (int s = 1; s < steps; s++)
                    //        {
                    //            float fs = fac * s;
                    //            interpPoints.Add(new PointF(prevP.X + fs * dp.X, prevP.Y + fs * dp.Y));
                    //        }
                    //        prevP = p;
                    //    }
                    //    interpPoints.Add(pointsList.Last());
                    //}
                }
                seedCurvePoints = pointsList.ToArray();
                zVector = FirstZVector; // * maxModulus / FirstZVector.GetModulus();
            }
            return seedCurvePoints != null;
        }

        private List<Point> GetBoundPoints()
        {
            GetBoundsPixels();
            bool foundPoint = false;
            Point p = Point.Empty;
            for (int y = 0; y < boundingRect.Height && !foundPoint; y++)
            {
                for (int x = 0; x < boundingRect.Width; x++)
                {
                    p = new Point(x, y);
                    if (IsBoundaryPoint(p))
                    {
                        foundPoint = true;
                        break;
                    }
                }
            }
            if (!foundPoint)
                return null;
            var boundPoints = new List<Point>();
            Point firstP = p;
            boundPoints.Add(p);
            bool loop = true;
            var prevPointSet = new HashSet<Point>();
            var deltaPs = new Point[] 
            { new Point(-1, -1), new Point(-1, 0), new Point(-1, 1),
              new Point(0, -1), new Point(0, 1), 
              new Point(1, -1), new Point(1, 0), new Point(1, 1) };
            do
            {
                foundPoint = false;
                var nextPoints = new List<Point>();
                foreach (Point deltaP in deltaPs)
                {
                    Point p2 = new Point(p.X + deltaP.X, p.Y + deltaP.Y);
                    if (p2 == firstP)
                    {
                        if (boundPoints.Count > 2)
                        {
                            loop = false;
                            boundPoints.Add(firstP);
                        }
                    }
                    else if (IsBoundaryPoint(p2))
                        nextPoints.Add(p2);
                }
                Point pNext = Point.Empty;
                if (loop)
                {
                    int prevIndex = -1;
                    foreach (Point p2 in nextPoints)
                    {
                        int ind = boundPoints.LastIndexOf(p2);
                        if (ind != -1)
                        {
                            if (Math.Abs(p2.X - firstP.X) <= 1 && Math.Abs(p2.Y - firstP.Y) <= 1)
                            {
                                loop = false;
                                foundPoint = true;
                                boundPoints.Add(firstP);
                                break;
                            }
                            else if (!prevPointSet.Contains(p2))
                            {
                                if (ind > prevIndex)
                                    prevIndex = ind;
                            }
                        }
                        else
                        {
                            pNext = p2;
                            boundPoints.Add(pNext);
                            prevPointSet.Clear();
                            foundPoint = true;
                            break;
                        }
                    }
                    if (!foundPoint && prevIndex != -1)
                    {
                        pNext = boundPoints[prevIndex];
                        prevPointSet.Add(pNext);
                        //boundPoints.Add(pNext);
                        foundPoint = true;
                    }
                    if (foundPoint)
                        p = pNext;
                    else
                    {
                        //break;
                        boundPoints.Add(firstP);
                        loop = false;
                    }
                }
            } while (loop && boundPoints.Count <= pixelCount);
            return loop ? null : boundPoints;
        }

        private bool IsPatternPixel(Point p)
        {
            int pixInd = boundingRect.Size.Width * p.Y + p.X;
            return pixInd >= 0 && pixInd < boundsPixels.Length && boundsPixels[pixInd] == blackArgb;
        }

        private bool IsBoundaryPoint(Point p)
        {
            if (IsPatternPixel(p))
            {
                for (int dY = -1; dY <= 1; dY++)
                {
                    for (int dX = -1; dX <= 1; dX++)
                    {
                        if (dX == 0 & dY == 0)
                            continue;
                        if (!IsPatternPixel(new Point(p.X + dX, p.Y + dY)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void GetBoundsPixels()
        {
            //Complex zVector1 = Patterns.First().ZVector;
            //zVector1.Normalize();
            //zVector1 = Complex.One / zVector1;
            foreach (Pattern ptn in Patterns)
            {
                ptn.ComputeCurvePoints(ptn.ZVector, forOutline: true);
            }
            int xMin = (int)Patterns.SelectMany(p => p.CurvePoints).Select(pt => pt.X).Min() - 2;
            int xMax = (int)Patterns.SelectMany(p => p.CurvePoints).Select(pt => pt.X).Max() + 2;
            int yMin = (int)Patterns.SelectMany(p => p.CurvePoints).Select(pt => pt.Y).Min() - 2;
            int yMax = (int)Patterns.SelectMany(p => p.CurvePoints).Select(pt => pt.Y).Max() + 2;
            boundingRect = new Rectangle(new Point(xMin, yMin), new Size(xMax - xMin, yMax - yMin));
            using (Bitmap bmp = BitmapTools.CreateFormattedBitmap(boundingRect.Size))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    deltaPoint = new PointF(xMin + 1, yMin + 1);
                    foreach (Pattern ptn in Patterns)
                    {
                        var points = ptn.CurvePoints.Select(p => new PointF(p.X - deltaPoint.X, p.Y - deltaPoint.Y));
                        FillCurvePoints(g, points.ToArray(), Brushes.Black, ptn.DrawCurve);
                    }
                }
                boundsPixels = new int[bmp.Width * bmp.Height];
                BitmapTools.CopyBitmapToColorArray(bmp, boundsPixels);
                pixelCount = boundsPixels.Count(pix => pix == blackArgb);
            }
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            return base.ToXml(parentNode, xmlTools, xmlNodeName ?? nameof(MergedPattern));
        }

        public override void FromXml(XmlNode node)
        {
            base.FromXml(node);
            SetZRotation();
            ComputeSeedCurvePoints(out _);
        }

        protected override void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.ExtraXml(parentNode, xmlTools);
            XmlNode childNode = xmlTools.CreateXmlNode(nameof(Patterns));
            foreach (Pattern ptn in Patterns)
            {
                ptn.ToXml(childNode, xmlTools);
            }
            parentNode.AppendChild(childNode);
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(FirstZVector), FirstZVector));
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(OrigCenter), OrigCenter));
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            bool retVal = true;
            if (node.Name == nameof(Patterns))
            {
                foreach (XmlNode patternNode in node.ChildNodes)
                {
                    Patterns.Add(CreatePatternFromXml(Design, patternNode, throwOnError: true));
                }
            }
            else if (node.Name == nameof(FirstZVector))
            {
                FirstZVector = Tools.GetComplexFromXml(node);
            }
            else if (node.Name == nameof(OrigCenter))
            {
                OrigCenter = Tools.GetPointFFromXml(node);
            }
            else
                retVal = base.FromExtraXml(node);
            return retVal;
        }
    }
}
