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
        public List<Pattern> Patterns { get; } = new List<Pattern>();
        private readonly int blackArgb = Color.Black.ToArgb();
        private int[] boundsPixels { get; set; }
        private int pixelCount { get; set; }
        private Rectangle boundingRect { get; set; }
        private PointF deltaPoint { get; set; }

        public MergedPattern(WhorlDesign design): base(design, FillInfo.FillTypes.Path)
        {
        }

        public MergedPattern(WhorlDesign design, XmlNode node): base(design, node)
        {
        }

        public void Initialize()
        {
            if (Patterns.Count == 0)
            {
                throw new Exception("There must be at least 1 merged pattern.");
            }
            Center = new PointF(Patterns.Select(p => p.Center.X).Average(),
                                Patterns.Select(p => p.Center.Y).Average());
            FillInfo = Patterns.First().FillInfo;
        }

        public bool InitCurvePoints(Complex zVector)
        {
            var boundPoints = GetCurvePoints();
            boundsPixels = null;
            if (boundPoints == null)
                CurvePoints = new PointF[0];
            else
                CurvePoints = boundPoints.Select(pt => new PointF(pt.X + deltaPoint.X, pt.Y + deltaPoint.Y)).ToArray();
            return boundPoints != null;
        }

        public override bool ComputeCurvePoints(Complex zVector, bool recomputeInnerSection = true, bool forOutline = false)
        {
            if (CurvePoints == null)
                return InitCurvePoints(zVector);
            else
                return true;
        }

        public override bool ComputeSeedPoints(bool computeRandom = false)
        {
            SeedPoints = new PolarCoord[0];
            return true;
        }

        protected override void CopyProperties(Pattern sourcePattern, bool copyFillInfo = true, bool copySharedPatternID = true, 
                                               bool copySeedPoints = true, bool setRecursiveParent = true, WhorlDesign design = null)
        {
            base.CopyProperties(sourcePattern, copyFillInfo, copySharedPatternID, copySeedPoints, setRecursiveParent, design);
            var source = sourcePattern as MergedPattern;
            if (source != null)
            {
                Patterns.Clear();
                Patterns.AddRange(source.Patterns.Select(p => p.GetCopy()));
                Initialize();
            }
        }

        public List<Point> GetCurvePoints()
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
                    if (!foundPoint)
                    {
                        break;
                        //boundPoints.Add(firstP);
                        //loop = false;
                    }
                    p = pNext;
                }
            } while (loop && boundPoints.Count <= pixelCount);
            return loop ? null : boundPoints;
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
            Initialize();
            InitCurvePoints(ZVector);
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
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            if (node.Name == nameof(Patterns))
            {
                foreach (XmlNode patternNode in node.ChildNodes)
                {
                    Patterns.Add(CreatePatternFromXml(Design, patternNode, throwOnError: true));
                }
                return true;
            }
            else
                return base.FromExtraXml(node);
        }
    }
}
