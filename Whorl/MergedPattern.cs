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
        public enum MergeModes
        {
            Boundary,
            Max,
            Sum
        }

        private const double factorCoeff = 1000.0;
        public MergeModes MergeMode { get; private set; } = MergeModes.Boundary;
        public bool IsMerged { get; private set; }
        public override bool DrawFilledIsEnabled => IsMerged;
        private List<Pattern> mergedPatterns { get; set; } = new List<Pattern>();
        private List<Pattern> unmergedPatterns { get; set; } = new List<Pattern>();
        public Complex FirstZVector { get; private set; }
        public PointF OrigCenter { get; private set; }
        private PointF[] seedCurvePoints { get; set; }  //Normalized curve points.
        private Complex zRotation { get; set; }

        public const byte OutsidePix = 1;
        public const byte InsidePix = 2;
        public const byte BoundaryPix = 3;

        //public static readonly int OutsideArgb = Color.White.ToArgb();
        public static readonly Color InsideColor = Color.Blue;
        public static readonly int InsideArgb = InsideColor.ToArgb();
        //public static readonly int BoundaryArgb = Color.Green.ToArgb();
        public byte[] BoundsBytes { get; private set; }
        //public int[] BoundsPixels { get; private set; }
        public int PixelCount { get; private set; }
        public bool FoundFirstPoint { get; private set; }
        private PointF? firstMergePoint { get; set; } = null;
        public Rectangle BoundingRect { get; private set; }
        public PointF DeltaPoint { get; private set; }
        public Point FirstPoint { get; private set; }
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

        public MergedPattern(WhorlDesign design, MergeModes mergeMode = MergeModes.Boundary): base(design, FillInfo.FillTypes.Path)
        {
            MergeMode = mergeMode;
            DoInits();
        }

        public MergedPattern(MergedPattern source, WhorlDesign design = null) : base(design ?? source.Design)
        {
            MergeMode = source.MergeMode;
            CopyProperties(source);
            mergedPatterns.AddRange(source.mergedPatterns.Select(p => p.GetCopy()).ToArray());
            FirstZVector = source.FirstZVector;
            OrigCenter = source.OrigCenter;
            firstMergePoint = source.firstMergePoint;
            zRotation = source.zRotation;
            seedCurvePoints = (PointF[])source.seedCurvePoints?.Clone();
            DoInits();
        }

        public MergedPattern(WhorlDesign design, XmlNode node): base(design, node)
        {
        }

        public static bool IsValidMergePattern(Pattern pattern)
        {
            return pattern.GetType() == typeof(Pattern) || pattern.GetType() == typeof(PathPattern);
        }

        public void SetPatterns(IEnumerable<Pattern> patterns)
        {
            Tools.DisposeList(unmergedPatterns);
            if (patterns.Any(p => !IsValidMergePattern(p)))
                throw new CustomException("Merge patterns must be of type Pattern or Path.");
            if (patterns.Count() < 2)
                throw new CustomException("There must be at least 2 merge patterns.");
            patterns = patterns.Select(p => p.GetCopy(design: Design));
            unmergedPatterns = patterns.ToList();
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

        private bool DoPatternMerge(PointF firstPoint)
        {
            Pattern pattern1 = mergedPatterns.OrderByDescending(p => p.ZVector.GetModulusSquared()).First();
            FirstZVector = pattern1.ZVector;
            SetZRotation();
            OrigCenter = Center = pattern1.Center;
            FillInfo = pattern1.FillInfo.GetCopy(this);
            if (MergeMode == MergeModes.Boundary)
            {
                double factor = factorCoeff / FirstZVector.GetModulus();
                float fFac = (float)factor;
                Complex zFactor;
                if (isInitialized && ZVector != FirstZVector)
                {
                    zFactor = FirstZVector / ZVector;
                    fFac *= (float)zFactor.GetModulus();
                }
                else
                    zFactor = Complex.One;
                foreach (Pattern ptn in mergedPatterns)
                {
                    ptn.ZVector *= factor;
                    if (isInitialized)
                        ptn.ZVector *= zFactor;
                    else
                    {
                        double mod0 = 3.0 / ptn.ZVector.GetModulus();
                        if (ptn.SeedPoints.Any(c => c.Modulus < mod0))
                        {
                            var otl = new BasicOutline(BasicOutlineTypes.NewEllipse);
                            otl.AddDenom = 1000.0;
                            otl.AmplitudeFactor = mod0;
                            otl.IntPetals = 2;
                            ptn.BasicOutlines.Add(otl);
                            var designPtn = GetDesignPattern(ptn);
                            if (designPtn != null)
                            {
                                designPtn.BasicOutlines.Add(otl.GetCopy());
                            }
                        }
                    }
                    ptn.ComputeSeedPoints();
                    PointF dc = new PointF(ptn.Center.X - Center.X, ptn.Center.Y - Center.Y);
                    ptn.Center = new PointF(Center.X + fFac * dc.X, Center.Y + fFac * dc.Y);
                }
                PointF delta = new PointF(firstPoint.X - Center.X, firstPoint.Y - Center.Y);
                firstMergePoint = new PointF(Center.X + fFac * delta.X, Center.Y + fFac * delta.Y);
            }
            IsMerged = ComputeSeedCurvePoints(out Complex zVector);
            if (!isInitialized)
                ZVector = zVector;
            if (MergeMode != MergeModes.Boundary)
            {
                SeedPoints = null;
            }
            isInitialized = true;
            return IsMerged;
        }

        public Pattern GetDesignPattern(Pattern pattern)
        {
            return Design.AllDesignPatterns.FirstOrDefault(ptn => ptn.KeyGuid == pattern.KeyGuid);
        }

        public Pattern[] GetDesignPatterns()
        {
            Pattern[] patterns = unmergedPatterns.Select(p => GetDesignPattern(p))
                                 .Where(p => p != null).ToArray();
            if (patterns.Length != unmergedPatterns.Count)
            {
                throw new Exception("Didn't find all merged patterns in design.");
            }
            return patterns;
        }

        public bool SetMerged(bool isMerged, PointF firstPoint)
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
                success = DoPatternMerge(firstPoint);
                if (success)
                    ClearRenderingCache();
                else
                    isMerged = false;
            }
            else
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
            if (MergeMode != MergeModes.Boundary)
            {
                Tools.DisposeList(unmergedPatterns);
                unmergedPatterns = new List<Pattern>(mergedPatterns);
            }
            else
            {
                Pattern[] patterns = GetDesignPatterns();
                Tools.DisposeList(unmergedPatterns);
                unmergedPatterns = new List<Pattern>();
                //double factor = FirstZVector.GetModulus() / factorCoeff;
                Complex zFactor = FirstZVector.GetModulus() / factorCoeff * ZVector / FirstZVector;
                float fFac = (float)zFactor.GetModulus(); //factor;
                foreach (Pattern ptn in mergedPatterns)
                {
                    Pattern ptnCopy = ptn.GetCopy(design: Design);
                    ptnCopy.ZVector *= zFactor; //factor;
                    PointF dc = new PointF(ptn.Center.X - OrigCenter.X, ptn.Center.Y - OrigCenter.Y);
                    ptnCopy.Center = new PointF(Center.X + fFac * dc.X, Center.Y + fFac * dc.Y);
                    unmergedPatterns.Add(ptnCopy);
                    Pattern designPattern = patterns.FirstOrDefault(p => p.KeyGuid == ptn.KeyGuid);
                    if (designPattern != null)
                    {
                        designPattern.ZVector = ptnCopy.ZVector;
                        designPattern.Center = ptnCopy.Center;
                    }
                }
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
            if (MergeMode != MergeModes.Boundary)
            {
                return base.ComputeCurvePoints(zVector, recomputeInnerSection, forOutline);
            }
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
            if (MergeMode != MergeModes.Boundary)
            {
                return ComputeModeSeedPoints();
            }
            SeedPoints = new PolarCoord[0];
            PreviewZFactor = Complex.One;
            ClearRenderingCache();
            return true;
        }

        private PolarCoord GetPolarCoord(IEnumerable<PolarCoord> coords)
        {
            float modulus;
            switch (MergeMode)
            {
                case MergeModes.Sum:
                default:
                    modulus = coords.Select(c => c.Modulus).Sum();
                    break;
                case MergeModes.Max:
                    modulus = coords.Select(c => c.Modulus).Max();
                    break;
            }
            return new PolarCoord(coords.Select(c => c.Angle).Average(), modulus);
        }

        private struct IndexedPolarCoord
        {
            public int Index { get; set; }
            public PolarCoord PolarCoord { get; set; }
            public float Angle => PolarCoord.Angle;
            public float Modulus => PolarCoord.Modulus;

            public IndexedPolarCoord(int index, PolarCoord polarCoord)
            {
                Index = index;
                PolarCoord = polarCoord;
            }
        }

        private bool ComputeModeSeedPoints()
        {
            var patterns = mergedPatterns; //.Where(p => !p.BasicOutlines.Any(o => o is PathOutline));
            if (!patterns.Any())
                return false;
            //int maxSteps = patterns.Select(p => p.RotationSteps).Max();
            //var coordsList = new List<List<PolarCoord>>();
            float maxModulus = (float)patterns.Select(p => p.ZVector.GetModulus()).Max();
            var seedPoints = new List<IndexedPolarCoord>();
            float twoPi = (float)(2.0 * Math.PI);
            float deltaAngle = twoPi / DefaultRotationSteps;
            int seedPointsCount = int.MaxValue;
            bool hasAngleTransforms = false;
            for (int ptnI = 0; ptnI < patterns.Count; ptnI++)
            {
                Pattern pattern = patterns[ptnI];
                if (!pattern.ComputeSeedPoints())
                    return false;
                float angleOff = (float)pattern.ZVector.GetArgument();
                float modScale = (float)pattern.ZVector.GetModulus() / maxModulus;
                var sortedCoords = Enumerable.Range(0, pattern.SeedPoints.Length)
                                   .Select(i => new IndexedPolarCoord(i, pattern.SeedPoints[i])).ToArray();
                for (int i = 0; i < sortedCoords.Length; i++)
                {
                    IndexedPolarCoord crd = sortedCoords[i];
                    sortedCoords[i].PolarCoord = new PolarCoord((float)Tools.NormalizeAngle(crd.Angle + angleOff), modScale * crd.Modulus);
                }
                sortedCoords = sortedCoords.OrderBy(c => c.Angle).ToArray();
                if (!hasAngleTransforms)
                {
                    if (Enumerable.Range(1, sortedCoords.Length).Any(i => sortedCoords[i - 1].Index > sortedCoords[i].Index))
                    {
                        hasAngleTransforms = true;
                    }
                }
                int ic = 0;
                float angle = 0, lastAngle = 0, lastModulus = sortedCoords.First().Modulus;
                int ind = 0, lastInd = 0;
                var coords = new List<IndexedPolarCoord>();
                while (angle <= twoPi)
                {
                    while (ic < sortedCoords.Length)
                    {
                        float diffAngle = sortedCoords[ic].Angle - angle;
                        if (diffAngle >= 0 || lastInd == 0)
                        {
                            if (diffAngle >= deltaAngle)
                                break;
                            float newModulus = sortedCoords[ic].Modulus;
                            int index = sortedCoords[ic].Index;
                            if (ind == 0)
                                coords.Add(new IndexedPolarCoord(index, new PolarCoord(lastAngle, lastModulus)));
                            else
                            {
                                int steps = ind - lastInd;
                                if (steps > 1)
                                {
                                    float modInc = (newModulus - lastModulus) / steps;
                                    for (int j = 0; j < steps; j++)
                                    {
                                        lastAngle += deltaAngle;
                                        lastModulus += modInc;
                                        coords.Add(new IndexedPolarCoord(index, new PolarCoord(lastAngle, lastModulus)));
                                    }
                                }
                                coords.Add(new IndexedPolarCoord(index, new PolarCoord(angle, newModulus)));
                            }
                            int startJ = ind == 0 ? 0 : lastInd + 1;
                            for (int j = startJ; j <= ind; j++)
                            {
                                IndexedPolarCoord coord = coords[j - startJ];
                                IndexedPolarCoord seedPoint;
                                if (j < seedPoints.Count)
                                {
                                    seedPoint = seedPoints[j];
                                    if (hasAngleTransforms)
                                        seedPoint.Index = coord.Index;
                                    if (MergeMode == MergeModes.Sum)
                                    {
                                        seedPoint.PolarCoord = new PolarCoord(seedPoint.PolarCoord.Angle,
                                                               seedPoint.PolarCoord.Modulus + coord.Modulus);
                                    }
                                    else if (MergeMode == MergeModes.Max)
                                    {
                                        seedPoint.PolarCoord = new PolarCoord(seedPoint.PolarCoord.Angle,
                                                               Math.Max(seedPoint.PolarCoord.Modulus, coord.Modulus));
                                    }
                                    seedPoints[j] = seedPoint;
                                }
                                else
                                {
                                    seedPoint = new IndexedPolarCoord(0, new PolarCoord(angle, coord.Modulus));
                                    seedPoints.Add(seedPoint);
                                }
                            }
                            lastAngle = angle;
                            lastModulus = newModulus;
                            lastInd = ind;
                            coords.Clear();
                            break;
                        }
                        ic++;
                    }
                    if (ic == sortedCoords.Length)
                        break;
                    angle += deltaAngle;
                    ind++;
                }
                seedPointsCount = Math.Min(seedPointsCount, lastInd + 1);
            }
            if (seedPointsCount < seedPoints.Count)
            {
                seedPoints.RemoveRange(seedPointsCount, seedPoints.Count - seedPointsCount);
            }
            IndexedPolarCoord lastCoord = seedPoints.Last();
            if (lastCoord.Angle < twoPi)
                seedPoints.Add(new IndexedPolarCoord(lastCoord.Index + 1, new PolarCoord(twoPi, lastCoord.Modulus)));
            if (MergeMode == MergeModes.Sum)
            {
                float scale = 1F / seedPoints.Select(c => c.Modulus).Max();
                for (int i = 0; i < seedPoints.Count; i++)
                {
                    IndexedPolarCoord crd = seedPoints[i];
                    seedPoints[i] = new IndexedPolarCoord(crd.Index, new PolarCoord(crd.Angle, scale * crd.Modulus));
                }
            }
            if (hasAngleTransforms)
                seedPoints = seedPoints.OrderBy(c => c.Index).ToList();
            SeedPoints = seedPoints.Select(c => c.PolarCoord).ToArray();
            return true;
        }

        public override Pattern GetCopy(bool keepRecursiveParent = false, WhorlDesign design = null)
        {
            return new MergedPattern(this, design);
        }

        public void DrawBoundary(Graphics g)
        {
            const float margin = 10F;
            GetBoundsPixels();
            float scale = (float)(FirstZVector.GetModulus() / factorCoeff);
            for (int y = 0; y < BoundingRect.Height; y++)
            {
                for (int x = 0; x < BoundingRect.Width; x++)
                {
                    Point p = new Point(x, y);
                    if (IsBoundaryPoint(p))
                    {
                        Point p2 = Tools.RoundPointF(new PointF(scale * p.X + margin, scale * p.Y + margin));
                        g.FillRectangle(Brushes.Red, new Rectangle(p2, new Size(1, 1)));
                    }
                }
            }
            if (FirstPoint != PointF.Empty)
            {
                Tools.DrawSquare(g, Color.Yellow, new PointF(scale * FirstPoint.X + margin, scale * FirstPoint.Y + margin), size: 1);
            }
        }

        public bool ComputeSeedCurvePoints(out Complex zVector, bool setFirstPoint = true)
        {
            if (MergeMode != MergeModes.Boundary)
            {
                zVector = FirstZVector;
                return true;
            }
            var boundPoints = GetBoundaryPoints(setFirstPoint);
            //var boundPoints = GetBoundPoints();
            BoundsBytes = null;
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
                float scale;
                if (firstMergePoint == null)
                {
                    //Legacy code:
                    double maxModulus = Math.Sqrt(points2.Select(p => p.X * p.X + p.Y * p.Y).Max());
                    scale = 1F / (float)maxModulus;
                }
                else
                    scale = 1F / (float)factorCoeff; //maxModulus;
                var pointsList = points2.Select(p => new PointF(scale * p.X, scale * p.Y)).ToList();
                if (pointsList.Any())
                {
                    Tools.ClosePoints(pointsList);
                }
                seedCurvePoints = pointsList.ToArray();
                zVector = FirstZVector;
            }
            return seedCurvePoints != null;
        }

        //private List<Point> GetDistinctPoints(Pattern pattern)
        //{
        //    var distinctPoints = Tools.DistinctPoints(pattern.CurvePoints)
        //                         .Select(pt => new PointF(pt.X - DeltaPoint.X, pt.Y - DeltaPoint.Y));
        //    var points = distinctPoints.Select(p => Tools.RoundPointF(p)).ToList();
        //    if (points.Any() && points.Last() != points.First())
        //        points.Add(points.First());
        //    return points;
        //}

        //private struct PointInfo
        //{
        //    public int PatternIndex { get; }
        //    public int PointIndex { get; }
        //    public double Distance { get; }

        //    public PointInfo(int patternInd, int pointInd, double distance)
        //    {
        //        PatternIndex = patternInd;
        //        PointIndex = pointInd;
        //        Distance = distance;
        //    }
        //}

        private HashSet<Point> prevPoints { get; } = new HashSet<Point>();

        private void SetFirstPoint()
        {
            bool foundPoint = false;
            Point p = Point.Empty;
            if (firstMergePoint != null)
            {
                var pFirst = (PointF)firstMergePoint;
                Point p1 = Tools.RoundPointF(new PointF(pFirst.X - DeltaPoint.X, pFirst.Y - DeltaPoint.Y));
                if (IsBoundaryPoint(p1))
                {
                    foundPoint = true;
                    p = p1;
                }
                else
                {
                    double scale = factorCoeff / FirstZVector.GetModulus();
                    int maxFac = (int)(5.0 * scale);
                    for (int factor = 1; factor <= maxFac && !foundPoint; factor++)
                    {
                        for (int i = 0; i < deltaPoints.Length; i++)
                        {
                            Point pDel = deltaPoints[i];
                            Point p2 = new Point(p1.X + factor * pDel.X, p1.Y + factor * pDel.Y);
                            if (IsBoundaryPoint(p2))
                            {
                                p = p2;
                                foundPoint = true;
                                break;
                            }
                        }
                    }
                }
                if (!foundPoint)
                    FirstPoint = p1;
            }
            else
            {
                for (int y = 0; y < BoundingRect.Height && !foundPoint; y++)
                {
                    for (int x = 0; x < BoundingRect.Width; x++)
                    {
                        p = new Point(x, y);
                        if (IsBoundaryPoint(p))
                        {
                            int pixInd = GetPixelIndex(p);
                            BoundsBytes[pixInd] = BoundaryPix;
                            float adjInside = CountAdjacent(p, 2, pt => IsPatternPixel(pt));
                            if (adjInside >= 0.4F && adjInside <= 0.6F)
                            {
                                foundPoint = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (foundPoint)
                FirstPoint = p;
            FoundFirstPoint = foundPoint;
        }

        private List<Point> GetBoundaryPoints(bool setFirstPoint = true)
        {
            GetBoundsPixels();
            if (setFirstPoint)
                SetFirstPoint();
            if (!FoundFirstPoint)
                return null;
            prevPoints.Clear();
            Point p = FirstPoint;
            var boundPoints = new List<Point>();
            boundPoints.Add(p);
            prevPoints.Add(p);
            int deltaInd = 4, prevInd = -1;
            bool finished = false;
            while (true)
            {
                if (boundPoints.Count > 100)
                {
                    if (Tools.DistanceSquared(p, FirstPoint) <= 2.0)
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
                        {
                            prevInd = boundPoints.Count;
                            deltaInd = (deltaInd + 2) % 8;
                        }
                        while (--prevInd >= 0)
                        {
                            p = boundPoints[prevInd];
                            nextP = GetNextBoundaryPoint(p, ref deltaInd, out foundType);
                            if (foundType == FoundTypes.New)
                            {
                                boundPoints.RemoveRange(prevInd + 1, boundPoints.Count - prevInd - 1);
                                boundPoints.Add(nextP);
                                //prevInd = -1;
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
                isInPattern = BoundsBytes[pixInd] != OutsidePix;
            }
            return isInPattern;
        }

        public bool IsBoundaryPoint(Point p)
        {
            bool isBoundary = false;
            if (IsPatternPixel(p))
            {
                int pixInd = GetPixelIndex(p);
                if (BoundsBytes[pixInd] == BoundaryPix)
                    isBoundary = true;
                else
                    isBoundary = IsBoundaryPointHelper(p, pixInd);
            }
            return isBoundary;
        }

        private bool IsBoundaryPointHelper(Point p, int pixInd)
        {
            bool isBoundary = false;
            for (int i = 0; i < deltaPoints.Length; i++)
            {
                Point delta = deltaPoints[i];
                if (!IsPatternPixel(new Point(p.X + delta.X, p.Y + delta.Y)))
                {
                    isBoundary = true;
                    break;
                }
            }
            return isBoundary;
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
                    if (BoundsBytes[pixInd] != BoundaryPix)
                    {
                        if (IsBoundaryPointHelper(nextP, pixInd))
                        {
                            foundType = FoundTypes.New;
                            BoundsBytes[pixInd] = BoundaryPix;
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
                var pixelArray = new int[bmp.Width * bmp.Height];
                BitmapTools.CopyBitmapToColorArray(bmp, pixelArray);
                int insideCount = 0;
                BoundsBytes = new byte[pixelArray.Length];
                for (int i = 0; i < pixelArray.Length; i++)
                {
                    if (pixelArray[i] == InsideArgb)
                    {
                        BoundsBytes[i] = InsidePix;
                        insideCount++;
                    }
                    else
                        BoundsBytes[i] = OutsidePix;
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
            firstMergePoint = null;
            FoundFirstPoint = false;
            MergeMode = Tools.GetEnumXmlAttr(node, nameof(MergeMode), MergeModes.Boundary);
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
                IsMerged = ComputeSeedCurvePoints(out _, setFirstPoint: !FoundFirstPoint);
            }
            isInitialized = true;
        }

        protected override void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.ExtraXml(parentNode, xmlTools);
            xmlTools.AppendXmlAttribute(parentNode, nameof(MergeMode), MergeMode);
            xmlTools.AppendXmlAttribute(parentNode, nameof(IsMerged), IsMerged);
            xmlTools.AppendXmlAttribute(parentNode, nameof(FoundFirstPoint), FoundFirstPoint);
            XmlNode childNode = xmlTools.CreateXmlNode("Patterns");
            foreach (Pattern ptn in mergedPatterns)
            {
                ptn.ToXml(childNode, xmlTools);
            }
            parentNode.AppendChild(childNode);
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(FirstZVector), FirstZVector));
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(OrigCenter), OrigCenter));
            if (firstMergePoint != null)
                parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(firstMergePoint), (PointF)firstMergePoint));
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(FirstPoint), FirstPoint));
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
            else if (node.Name == nameof(firstMergePoint))
            {
                firstMergePoint = Tools.GetPointFFromXml(node);
            }
            else if (node.Name == nameof(FirstPoint))
            {
                FirstPoint = Tools.GetPointFromXml(node);
            }
            else
                retVal = base.FromExtraXml(node);
            return retVal;
        }

        public override void Dispose()
        {
            base.Dispose();
            BoundsBytes = null;
            seedCurvePoints = null;
            Tools.DisposeList(unmergedPatterns);
            Tools.DisposeList(mergedPatterns);
        }
    }
}
