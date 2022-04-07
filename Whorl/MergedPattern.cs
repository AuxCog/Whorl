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
        private const double factorCoeff = 1000.0;
        public bool IsMerged { get; private set; }
        public override bool DrawFilledIsEnabled => IsMerged;
        private List<Pattern> mergedPatterns { get; set; } = new List<Pattern>();
        private List<Pattern> unmergedPatterns { get; set; } = new List<Pattern>();
        public Complex FirstZVector { get; private set; }
        public PointF OrigCenter { get; private set; }
        private PointF[] seedCurvePoints { get; set; }  //Normalized curve points.
        private Complex zRotation { get; set; }
        public static readonly int OutsideArgb = Color.White.ToArgb();
        public static readonly Color InsideColor = Color.Blue;
        public static readonly int InsideArgb = InsideColor.ToArgb();
        public static readonly int BoundaryArgb = Color.Green.ToArgb();
        public int[] BoundsPixels { get; private set; }
        public int PixelCount { get; private set; }
        public Rectangle BoundingRect { get; private set; }
        public PointF DeltaPoint { get; private set; }
        private bool isInitialized { get; set; }

        //Deltas ordered clockwise:
        private static readonly Point[] deltaPoints =
        {
            new Point(0, 1),  new Point(1, 1),   new Point(1, 0),  new Point(1, -1), 
            new Point(0, -1), new Point(-1, -1), new Point(-1, 0), new Point(-1, 1)
        };

        private void DoInits()
        {
            PreviewZFactor = Complex.One;
        }

        public MergedPattern(WhorlDesign design): base(design, FillInfo.FillTypes.Path)
        {
            DoInits();
        }

        public MergedPattern(MergedPattern source, WhorlDesign design = null) : base(design ?? source.Design)
        {
            CopyProperties(source);
            mergedPatterns.AddRange(source.mergedPatterns.Select(p => p.GetCopy()).ToArray());
            FirstZVector = source.FirstZVector;
            zRotation = source.zRotation;
            seedCurvePoints = (PointF[])source.seedCurvePoints?.Clone();
            DoInits();
        }

        public MergedPattern(WhorlDesign design, XmlNode node): base(design, node)
        {
        }

        public void SetPatterns(IEnumerable<Pattern> patterns)
        {
            Tools.DisposeList(unmergedPatterns);
            unmergedPatterns = patterns.Select(p => p.GetCopy(design: Design)).ToList();
        }

        public void SetRawPatterns(IEnumerable<Pattern> patterns)
        {
            Tools.DisposeList(mergedPatterns);
            mergedPatterns = patterns.Select(p => p.GetCopy(design: Design))
                             .OrderByDescending(p => p.ZVector.GetModulusSquared()).ToList();
        }

        public void ScaleRawPatterns(float scale)
        {
            foreach (Pattern pattern in mergedPatterns)
            {
                pattern.ZVector *= scale;
                pattern.Center = new PointF(scale * pattern.Center.X, scale * pattern.Center.Y);
            }
        }

        public IEnumerable<Pattern> GetUnmergedPatterns()
        {
            return unmergedPatterns;
        }

        private bool DoPatternMerge()
        {
            Pattern pattern1 = mergedPatterns.OrderByDescending(p => p.ZVector.GetModulusSquared()).First();
            FirstZVector = pattern1.ZVector;
            SetZRotation();
            OrigCenter = Center = pattern1.Center;
            FillInfo = pattern1.FillInfo.GetCopy(this);
            double factor = factorCoeff / FirstZVector.GetModulus();
            float fFac = (float)factor;
            foreach (Pattern ptn in mergedPatterns)
            {
                ptn.ZVector *= factor;
                PointF dc = new PointF(ptn.Center.X - Center.X, ptn.Center.Y - Center.Y);
                ptn.Center = new PointF(Center.X + fFac * dc.X, Center.Y + fFac * dc.Y);
            }
            IsMerged = ComputeSeedCurvePoints(out Complex zVector);
            if (!isInitialized)
                ZVector = zVector;
            isInitialized = true;
            return IsMerged;
        }

        private Pattern[] GetDesignPatterns()
        {
            Pattern[] patterns = unmergedPatterns.Select(
                   p => Design.AllDesignPatterns.FirstOrDefault(ptn => ptn.KeyGuid == p.KeyGuid))
                   .Where(p => p != null).ToArray();
            if (patterns.Length != unmergedPatterns.Count)
            {
                throw new Exception("Didn't find all merged patterns in design.");
            }
            return patterns;
        }

        public bool SetMerged(bool isMerged)
        {
            bool success = true;
            if (IsMerged == isMerged)
                return true;
            Pattern[] patterns = null;
            if (isMerged)
            {
                patterns = GetDesignPatterns();
                if (patterns.Length == 0)
                {
                    throw new Exception("There must be at least 1 merged pattern.");
                }
                Tools.DisposeList(mergedPatterns);
                mergedPatterns = patterns.Select(p => p.GetCopy(design: Design)).ToList();
                success = DoPatternMerge();
                if (success)
                    ClearRenderingCache();
                else
                    isMerged = false;
            }
            if (!isMerged)
            {
                SetUnmergedPatterns();
                patterns = GetDesignPatterns();
            }
            foreach (Pattern pattern in patterns)
            {
                pattern.PatternIsEnabled = !isMerged;
            }
            IsMerged = isMerged;
            return success;
        }

        private void SetUnmergedPatterns()
        {
            Tools.DisposeList(unmergedPatterns);
            unmergedPatterns = new List<Pattern>();
            double factor = FirstZVector.GetModulus() / factorCoeff;
            float fFac = (float)factor;
            foreach (Pattern ptn in mergedPatterns)
            {
                Pattern ptnCopy = ptn.GetCopy(design: Design);
                ptnCopy.ZVector *= factor;
                PointF dc = new PointF(ptn.Center.X - OrigCenter.X, ptn.Center.Y - OrigCenter.Y);
                ptnCopy.Center = new PointF(Center.X + fFac * dc.X, Center.Y + fFac * dc.Y);
                unmergedPatterns.Add(ptnCopy);
            }
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
            for (int y = 0; y < BoundingRect.Height; y++)
            {
                for (int x = 0; x < BoundingRect.Width; x++)
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
            var boundPoints = GetBoundaryPoints();
            //var boundPoints = GetBoundPoints();
            BoundsPixels = null;
            if (boundPoints == null)
            {
                seedCurvePoints = null;
                zVector = Complex.One;
            }
            else
            {
                PointF center = OrigCenter;
                var points2 = boundPoints.Select(pt =>
                    new PointF(pt.X + DeltaPoint.X - center.X,
                               pt.Y + DeltaPoint.Y - center.Y));
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

        private List<Point> GetDistinctPoints(Pattern pattern)
        {
            //Complex zVector = pattern.ZVector;
            //double m = zVector.GetModulus();
            //zVector *= (m - 1.0) / m;
            //pattern.ComputeCurvePoints(zVector, forOutline: true);
            var distinctPoints = Tools.DistinctPoints(pattern.CurvePoints)
                                 .Select(pt => new PointF(pt.X - DeltaPoint.X, pt.Y - DeltaPoint.Y));
            var points = distinctPoints.Select(p => Tools.RoundPointF(p)).ToList();
            if (points.Any() && points.Last() != points.First())
                points.Add(points.First());
            return points;
        }

        private struct PointInfo
        {
            public int PatternIndex { get; }
            public int PointIndex { get; }
            public double Distance { get; }

            public PointInfo(int patternInd, int pointInd, double distance)
            {
                PatternIndex = patternInd;
                PointIndex = pointInd;
                Distance = distance;
            }
        }

        private HashSet<Point> prevPoints { get; } = new HashSet<Point>();

        private List<Point> GetBoundaryPoints()
        {
            GetBoundsPixels();
            prevPoints.Clear();
            bool foundPoint = false;
            Point p = Point.Empty;
            for (int y = 0; y < BoundingRect.Height && !foundPoint; y++)
            {
                for (int x = 0; x < BoundingRect.Width; x++)
                {
                    p = new Point(x, y);
                    if (IsBoundaryPoint(p))
                    {
                        int pixInd = GetPixelIndex(p);
                        BoundsPixels[pixInd] = BoundaryArgb;
                        float adjInside = CountAdjacent(p, 2, pt => IsPatternPixel(pt));
                        if (adjInside >= 0.3F && adjInside <= 0.7F)
                        {
                            foundPoint = true;
                            break;
                        }
                    }
                }
            }
            if (!foundPoint)
                return null;
            var boundPoints = new List<Point>();
            Point firstP = p;
            boundPoints.Add(p);
            prevPoints.Add(p);
            int deltaInd = 4, prevInd = -1;
            bool finished = false;
            while (true)
            {
                if (boundPoints.Count > 100)
                {
                    if (Tools.DistanceSquared(p, firstP) <= 2.0)
                    {
                        finished = true;
                        break;
                    }
                }
                Point nextP = GetNextBoundaryPoint(p, ref deltaInd, out FoundTypes foundType);
                if (foundType == FoundTypes.New)
                {
                    boundPoints.Add(nextP);
                    if (prevInd != -1 && boundPoints.Count > prevInd + 10)
                        prevInd = -1;
                    //prevPoints.Clear();
                }
                else 
                {
                    if (foundType == FoundTypes.None)
                        break;
                    else if (foundType == FoundTypes.Previous)
                    {
                        if (prevInd == -1)
                            prevInd = boundPoints.Count;
                        while (--prevInd >= 0)
                        {
                            p = boundPoints[prevInd];
                            nextP = GetNextBoundaryPoint(p, ref deltaInd, out foundType);
                            if (foundType == FoundTypes.New)
                            {
                                boundPoints.RemoveRange(prevInd + 1, boundPoints.Count - prevInd - 1);
                                boundPoints.Add(nextP);
                                break;
                            }
                        }
                        if (prevInd < 0)
                            break;
                    }
                }
                prevPoints.Add(nextP);
                p = nextP;
            }
            if (finished && boundPoints.Any())
            {
                if (boundPoints.Last() != boundPoints.First())
                    boundPoints.Add(boundPoints.First());
            }
            return finished ? boundPoints : null;
        }

        private float CountAdjacent(Point p, int buffer, Func<Point, bool> predicate)
        {
            int count = 0, totCount = 0;
            for (int dY = -buffer; dY <= buffer; dY++)
            {
                for (int dX = -buffer; dX <= buffer; dX++)
                {
                    if (dY != 0 || dX != 0)
                    {
                        totCount++;
                        if (predicate(new Point(p.X + dX, p.Y + dY)))
                            count++;
                    }
                }
            }
            return  (float)count / totCount;
        }

        //private List<Point> GetBoundPoints()
        //{
        //    GetBoundsPixels();
        //    bool foundPoint = false;
        //    Point p = Point.Empty;
        //    for (int y = 0; y < BoundingRect.Height && !foundPoint; y++)
        //    {
        //        for (int x = 0; x < BoundingRect.Width; x++)
        //        {
        //            p = new Point(x, y);
        //            if (IsBoundaryPoint(p))
        //            {
        //                foundPoint = true;
        //                break;
        //            }
        //        }
        //    }
        //    if (!foundPoint)
        //        return null;
        //    var boundPoints = new List<Point>();
        //    Point firstP = p;
        //    boundPoints.Add(p);
        //    bool loop = true;
        //    var prevPointSet = new HashSet<Point>();
        //    var deltaPs = new Point[] 
        //    { new Point(-1, -1), new Point(-1, 0), new Point(-1, 1),
        //      new Point(0, -1), new Point(0, 1), 
        //      new Point(1, -1), new Point(1, 0), new Point(1, 1) };
        //    do
        //    {
        //        foundPoint = false;
        //        var nextPoints = new List<Point>();
        //        foreach (Point deltaP in deltaPs)
        //        {
        //            Point p2 = new Point(p.X + deltaP.X, p.Y + deltaP.Y);
        //            if (p2 == firstP)
        //            {
        //                if (boundPoints.Count > 2)
        //                {
        //                    loop = false;
        //                    boundPoints.Add(firstP);
        //                }
        //            }
        //            else if (IsBoundaryPoint(p2))
        //                nextPoints.Add(p2);
        //        }
        //        Point pNext = Point.Empty;
        //        if (loop)
        //        {
        //            int prevIndex = -1;
        //            foreach (Point p2 in nextPoints)
        //            {
        //                int ind = boundPoints.LastIndexOf(p2);
        //                if (ind != -1)
        //                {
        //                    if (Math.Abs(p2.X - firstP.X) <= 1 && Math.Abs(p2.Y - firstP.Y) <= 1)
        //                    {
        //                        loop = false;
        //                        foundPoint = true;
        //                        boundPoints.Add(firstP);
        //                        break;
        //                    }
        //                    else if (!prevPointSet.Contains(p2))
        //                    {
        //                        if (ind > prevIndex)
        //                            prevIndex = ind;
        //                    }
        //                }
        //                else
        //                {
        //                    pNext = p2;
        //                    boundPoints.Add(pNext);
        //                    prevPointSet.Clear();
        //                    foundPoint = true;
        //                    break;
        //                }
        //            }
        //            if (!foundPoint && prevIndex != -1)
        //            {
        //                pNext = boundPoints[prevIndex];
        //                prevPointSet.Add(pNext);
        //                //boundPoints.Add(pNext);
        //                foundPoint = true;
        //            }
        //            if (foundPoint)
        //                p = pNext;
        //            else
        //            {
        //                //break;
        //                boundPoints.Add(firstP);
        //                loop = false;
        //            }
        //        }
        //    } while (loop && boundPoints.Count <= PixelCount);
        //    return loop ? null : boundPoints;
        //}

        public int GetPixelIndex(Point p)
        {
            return BoundingRect.Size.Width * p.Y + p.X;
        }

        public bool IsPatternPixel(Point p)
        {
            bool isInPattern;
            if (p.X < 0 || p.X >= BoundingRect.Size.Width ||
                p.Y < 0 || p.Y >= BoundingRect.Size.Height)
            {
                isInPattern = false;  //Out of bounds.
            }
            else
            {
                int pixInd = GetPixelIndex(p);
                isInPattern = BoundsPixels[pixInd] != OutsideArgb;
            }
            return isInPattern;
        }

        public bool IsBoundaryPoint(Point p)
        {
            if (IsPatternPixel(p))
            {
                int pixInd = GetPixelIndex(p);
                if (BoundsPixels[pixInd] == BoundaryArgb)
                    return true;
                else
                    return IsBoundaryPointHelper(p, pixInd);
            }
            return false;
        }

        private bool IsBoundaryPointHelper(Point p, int pixInd)
        {
            for (int i = 0; i < deltaPoints.Length; i++)
            {
                Point delta = deltaPoints[i];
                if (!IsPatternPixel(new Point(p.X + delta.X, p.Y + delta.Y)))
                {
                    return true;
                }
            }
            return false;
        }

        private enum FoundTypes
        {
            None,
            New,
            Previous
        }

        private static readonly int[] indexOffs = { 0, 1, 7, 2, 6, 3, 5, 4 };

        private Point GetNextBoundaryPoint(Point p, ref int deltaInd, out FoundTypes foundType)
        {
            foundType = FoundTypes.None;
            int startInd = deltaInd;
            int prevInd = -1;
            Point prevP = Point.Empty;
            foreach (int indOff in indexOffs)
            {
                deltaInd = (startInd + indOff) % 8;
                Point delta = deltaPoints[deltaInd];
                var nextP = new Point(p.X + delta.X, p.Y + delta.Y);
                if (IsPatternPixel(nextP))
                {
                    int pixInd = GetPixelIndex(nextP);
                    if (BoundsPixels[pixInd] != BoundaryArgb)
                    {
                        if (IsBoundaryPointHelper(nextP, pixInd))
                        {
                            foundType = FoundTypes.New;
                            BoundsPixels[pixInd] = BoundaryArgb;
                            return nextP;
                        }
                    }
                    else if (prevInd == -1 && prevPoints.Contains(nextP))
                    {
                        prevP = nextP;
                        prevInd = deltaInd;
                    }
                }
            }
            if (prevInd != -1)
            {
                deltaInd = prevInd;
                foundType = FoundTypes.Previous;
                return prevP;
            }
            return Point.Empty;
        }

        public void GetBoundsPixels()
        {
            foreach (Pattern ptn in mergedPatterns)
            {
                ptn.ComputeCurvePoints(ptn.ZVector, forOutline: true);
            }
            PointF[] allPoints = mergedPatterns.SelectMany(p => p.CurvePoints).ToArray();
            int xMin = (int)allPoints.Select(pt => pt.X).Min() - 2;
            int xMax = (int)allPoints.Select(pt => pt.X).Max() + 2;
            int yMin = (int)allPoints.Select(pt => pt.Y).Min() - 2;
            int yMax = (int)allPoints.Select(pt => pt.Y).Max() + 2;
            allPoints = null;
            BoundingRect = new Rectangle(new Point(xMin, yMin), new Size(xMax - xMin, yMax - yMin));
            DeltaPoint = new PointF(xMin + 1, yMin + 1);
            using (Bitmap bmp = BitmapTools.CreateFormattedBitmap(BoundingRect.Size))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (var brush = new SolidBrush(InsideColor))
                    {
                        foreach (Pattern ptn in mergedPatterns)
                        {
                            var points = ptn.CurvePoints.Select(p => new PointF(p.X - DeltaPoint.X, p.Y - DeltaPoint.Y));
                            FillCurvePoints(g, points.ToArray(), brush, ptn.DrawCurve);
                        }
                    }
                }
                BoundsPixels = new int[bmp.Width * bmp.Height];
                BitmapTools.CopyBitmapToColorArray(bmp, BoundsPixels);
                int insideCount = 0;
                for (int i = 0; i < BoundsPixels.Length; i++)
                {
                    if (BoundsPixels[i] == InsideArgb)
                        insideCount++;
                    else
                        BoundsPixels[i] = OutsideArgb;
                }
                PixelCount = insideCount;
            }
        }

        public void AddMergedPatternsToDesign()
        {
            if (ContainerIsDesign)
            {
                Design.AddPatterns(unmergedPatterns.Where(p =>
                                   !Design.AllDesignPatterns.Any(ptn => ptn.KeyGuid == p.KeyGuid)));
            }
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (IsMerged)
                return base.ToXml(parentNode, xmlTools, xmlNodeName ?? nameof(MergedPattern));
            else
                return null;
        }

        public override void FromXml(XmlNode node)
        {
            base.FromXml(node);
            if (node.Attributes[nameof(IsMerged)] == null)
                IsMerged = true; //Legacy code.
            DoInits();
            SetZRotation();
            SetUnmergedPatterns();
            foreach (Pattern pattern in unmergedPatterns)
            {
                pattern.PatternIsEnabled = !IsMerged;
            }
            if (IsMerged)
            {
                IsMerged = ComputeSeedCurvePoints(out _);
            }
            isInitialized = true;
        }

        protected override void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.ExtraXml(parentNode, xmlTools);
            xmlTools.AppendXmlAttribute(parentNode, nameof(IsMerged), IsMerged);
            XmlNode childNode = xmlTools.CreateXmlNode("Patterns");
            foreach (Pattern ptn in mergedPatterns)
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
            if (node.Name == "Patterns")
            {
                foreach (XmlNode patternNode in node.ChildNodes)
                {
                    mergedPatterns.Add(CreatePatternFromXml(Design, patternNode, throwOnError: true));
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

        public override void Dispose()
        {
            base.Dispose();
            BoundsPixels = null;
            seedCurvePoints = null;
            Tools.DisposeList(unmergedPatterns);
            Tools.DisposeList(mergedPatterns);
        }
    }
}
