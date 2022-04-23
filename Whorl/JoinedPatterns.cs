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
    public class JoinedPatterns: Pattern
    {
        public class PointInfo
        {
            public List<IntersectionInfo> IntersectionInfos { get; set; }
            /// <summary>
            /// Index  of Point in first pattern.
            /// </summary>
            public int Index1 { get; }

            public PointInfo(int index1)
            {
                Index1 = index1;
            }

            public PointInfo(PointInfo source)
            {
                Index1 = source.Index1;
                if (source.IntersectionInfos != null)
                    IntersectionInfos = source.IntersectionInfos.Select(ii => new IntersectionInfo(ii)).ToList();
            }

            public IntersectionInfo AddIntersection(int joinInfoIndex, Point point, int index2)
            {
                if (IntersectionInfos == null)
                    IntersectionInfos = new List<IntersectionInfo>();
                var intersectionInfo = new IntersectionInfo(joinInfoIndex, point, this, index2);
                IntersectionInfos.Add(intersectionInfo);
                return intersectionInfo;
            }
        }

        public class IntersectionInfo
        {
            public int JoinInfoIndex { get; }
            public Point Point { get; }
            public PointInfo PointInfo { get; }
            /// <summary>
            /// Index of Point in JoinInfo's pattern.
            /// </summary>
            public int Index2 { get; }

            public IntersectionInfo(int joinInfoIndex, Point point, PointInfo pointInfo, int index2)
            {
                if (pointInfo == null)
                    throw new ArgumentNullException("pointInfo cannot be null.");
                JoinInfoIndex = joinInfoIndex;
                Point = point;
                PointInfo = pointInfo;
                Index2 = index2;
            }

            public IntersectionInfo(IntersectionInfo source) : 
                                    this(source.JoinInfoIndex, source.Point, new PointInfo(source.PointInfo), source.Index2)
            {
            }
        }
        public class JoinInfo
        {
            public int Index { get; private set; }
            public Pattern Pattern { get; private set; }
            public List<Point> CurvePoints { get; private set; }
            public bool IsValid { get; private set; }

            public JoinInfo(Pattern pattern, int index)
            {
                Initialize(pattern, index);
            }

            public JoinInfo(XmlNode node, WhorlDesign design)
            {
                FromXml(node, design);
            }

            private void Initialize(Pattern pattern, int index)
            {
                if (pattern == null)
                    throw new ArgumentNullException("Pattern cannot be null.");
                Pattern = pattern;
                Index = index;
                if (pattern.CurvePoints == null)
                    IsValid = pattern.ComputeCurvePoints(pattern.ZVector);
                else
                    IsValid = true;
                if (IsValid)
                {
                    var points = Tools.InterpolatePoints(Pattern.CurvePoints, minDistanceSquared: 1F);
                    CurvePoints = points.Select(p => Tools.RoundPointF(p)).ToList();
                }
            }

            public JoinInfo(JoinInfo source)
            {
                Pattern = source.Pattern;
                Index = source.Index;
                IsValid = source.IsValid;
                if (source.CurvePoints != null)
                    CurvePoints = new List<Point>(source.CurvePoints);
            }

            public void AddIntersectionsToArray(PointInfo[,] array, Rectangle extents, 
                                                List<IntersectionInfo> intersectionInfos)
            {
                HashSet<Point> traversed = new HashSet<Point>();
                for (int i = 0; i < CurvePoints.Count; i++)
                {
                    Point p = CurvePoints[i];
                    if (i > 0 && p == CurvePoints[i - 1])
                        continue;
                    Point p2 = new Point(p.X - extents.X + 1, p.Y - extents.Y + 1);
                    if (!traversed.Add(p2))
                        continue;
                    if (p2.X >= 1 && p2.X < extents.Width - 1 && p2.Y >= 1 && p2.Y < extents.Height - 1)
                    {
                        PointInfo pointInfo = array[p2.X, p2.Y];
                        if (pointInfo == null)
                        {
                            for (int dx = -1; dx <= 1 && pointInfo == null; dx++)
                            {
                                for (int dy = -1; dy <= 1; dy++)
                                {
                                    Point p3 = new Point(p2.X + dx, p2.Y + dy);
                                    if (!traversed.Add(p3))
                                        continue;
                                    pointInfo = array[p3.X, p3.Y];
                                    if (pointInfo != null)
                                    {
                                        p2 = p3;
                                        break;
                                    }
                                }
                            }
                        }
                        if (pointInfo != null)
                        {
                            if (!intersectionInfos.Exists(ii => Tools.ManhattanDistance(p2, ii.Point) <= 4))
                            {
                                var intersectionInfo = pointInfo.AddIntersection(Index, p2, i);
                                intersectionInfos.Add(intersectionInfo);
                            }
                        }
                    }
                }
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(JoinInfo);
                XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttribute(xmlNode, nameof(Index), Index);
                Pattern.ToXml(xmlNode, xmlTools);
                return xmlTools.AppendToParent(parentNode, xmlNode);
            }

            public void FromXml(XmlNode node, WhorlDesign design)
            {
                int index = Tools.GetXmlAttribute<int>(node, nameof(Index));
                Pattern pattern = Pattern.CreatePatternFromXml(design, node.FirstChild, throwOnError: true);
                Initialize(pattern, index);
            }
        }
        public List<JoinInfo> JoinInfos { get; } = new List<JoinInfo>();
        public List<PointF> JoinPoints { get; } = new List<PointF>();
        private Complex origZVector { get; set; }

        private List<PointF> seedCurvePoints { get; set; }

        public JoinedPatterns(WhorlDesign design): base(design, FillInfo.FillTypes.Path)
        {
            DoInits();
        }

        public JoinedPatterns(WhorlDesign design, XmlNode xmlNode) : base(design)
        {
            FromXml(xmlNode);
        }

        public JoinedPatterns(JoinedPatterns source, WhorlDesign design = null): base(design ?? source.Design)
        {
            CopyProperties(source);
            origZVector = source.origZVector;
            JoinInfos.AddRange(source.JoinInfos.Select(ji => new JoinInfo(ji)));
            if (source.seedCurvePoints != null)
                seedCurvePoints = new List<PointF>(source.seedCurvePoints);
            DoInits();
        }

        public void AddPatterns(IEnumerable<Pattern> patterns)
        {
            if (!patterns.Any())
                return;
            Pattern pattern1 = patterns.First();
            Center = pattern1.Center;
            origZVector = ZVector = pattern1.ZVector;
            FillInfo = pattern1.FillInfo;
            double maxModulus = patterns.Select(p => p.ZVector.GetModulus()).Max();
            double scale = maxModulus == 0 ? 1.0 : 1000.0 / maxModulus;
            float fScale = (float)scale;
            foreach (Pattern pattern in patterns)
            {
                int index = JoinInfos.Count;
                Pattern copy = pattern.GetCopy();
                copy.ZVector *= scale;
                PointF dc = new PointF(copy.Center.X - Center.X, copy.Center.Y - Center.Y);
                copy.Center = new PointF(Center.X + fScale * dc.X, Center.Y + fScale * dc.Y);
                JoinInfos.Add(new JoinInfo(copy, index));
            }
        }

        private void DoInits()
        {
            if (PreviewZFactor == Complex.Zero)
                PreviewZFactor = Complex.One;
        }

        public override Pattern GetCopy(bool keepRecursiveParent = false, WhorlDesign design = null)
        {
            return new JoinedPatterns(this, design);
        }

        //protected override IEnumerable<string> GetExtraExcludedCopyProperties()
        //{
        //    return base.GetExtraExcludedCopyProperties().Concat(
        //           new string[] { nameof(Pattern1), nameof(Pattern2) });
        //}

        public bool IsValid()
        {
            return JoinInfos.Count >= 2 && !JoinInfos.Exists(ji => !ji.IsValid);
            //bool isValid = Pattern1 != null && Pattern2 != null;
            //if (isValid)
            //{
            //    if (Pattern1.CurvePoints == null)
            //        isValid = Pattern1.ComputeCurvePoints(Pattern1.ZVector);
            //    if (isValid)
            //    {
            //        if (InsertIndex < 0 || InsertIndex >= Pattern1.CurvePoints.Length)
            //            isValid = false;
            //        else if (EndInsertIndex >= 0 && EndInsertIndex >= Pattern1.CurvePoints.Length)
            //            isValid = false;
            //    }
            //}
            //return isValid;
        }

        public bool FinishJoin()
        {
            JoinPoints.Clear();
            if (!IsValid())
                return false;
            return ComputeSeedCurvePoints(validate: false);
        }

        //private Dictionary<int, Dictionary<int, PointInfo>> CreateDictionary(List<Point> curvePoints)
        //{
        //    var dict = new Dictionary<int, Dictionary<int, PointInfo>>();
        //    for (int i = 0; i < curvePoints.Count; i++)
        //    {
        //        Point p = curvePoints[i];
        //        if (!dict.TryGetValue(p.Y, out Dictionary<int, PointInfo> xDict))
        //        {
        //            xDict = new Dictionary<int, PointInfo>();
        //            dict.Add(p.Y, xDict);
        //        }
        //        if (!xDict.ContainsKey(p.X))
        //        {
        //            xDict.Add(p.X, new PointInfo(i));
        //        }
        //    }
        //    return dict;
        //}

        private PointInfo[,] CreatePointArray(List<Point> curvePoints, out Rectangle extents)
        {
            extents = Tools.GetBoundingRectangle(curvePoints, padding: 1);
            var points = new PointInfo[extents.Width, extents.Height];
            for (int i = 0; i < curvePoints.Count; i++)
            {
                Point p = curvePoints[i];
                Point p2 = new Point(p.X - extents.X + 1, p.Y - extents.Y + 1);
                if (points[p2.X, p2.Y] == null)
                {
                    points[p2.X, p2.Y] = new PointInfo(i);
                }
            }
            return points;
        }

        private bool ComputeSeedCurvePoints(bool validate = true)
        {
            if (validate && !IsValid())
                return false;
            var curvePoints1 = JoinInfos[0].CurvePoints;
            if (curvePoints1.Count == 0)
                return false;
            var pointsArray = CreatePointArray(curvePoints1, out Rectangle extents);
            var intersections = new List<IntersectionInfo>();
            foreach (JoinInfo joinInfo in JoinInfos.Skip(1))
            {
                joinInfo.AddIntersectionsToArray(pointsArray, extents, intersections);
            }
            JoinPoints.AddRange(intersections.Select(ii => (PointF)ii.Point));
            if (intersections.Count < 2)
                return false;
            intersections = intersections.OrderBy(ii => ii.PointInfo.Index1).ToList();
            var points = new List<Point>();
            int i = 0;
            int prevIndex = 0;
            while (i < intersections.Count)
            {
                IntersectionInfo intersection = intersections[i++];
                int joinIndex = intersection.JoinInfoIndex;
                while (i < intersections.Count && intersections[i].JoinInfoIndex != joinIndex)
                {
                    i++;
                }
                if (i < intersections.Count)
                {
                    IntersectionInfo intersection2 = intersections[i++];
                    points.AddRange(curvePoints1.Skip(prevIndex).Take(intersection.PointInfo.Index1 - prevIndex));
                    prevIndex = intersection2.PointInfo.Index1;
                    int startI = intersection.Index2;
                    int endI = intersection2.Index2;
                    var curvePoints2 = JoinInfos[joinIndex].CurvePoints;
                    if (startI <= endI)
                        points.AddRange(curvePoints2.Skip(startI).Take(endI - startI));
                    else
                    {
                        points.AddRange(curvePoints2.Skip(startI));
                        points.AddRange(curvePoints2.Take(endI));
                    }
                }
            }
            if (prevIndex < curvePoints1.Count)
                points.AddRange(curvePoints1.Skip(prevIndex));
            PointF center = JoinInfos[0].Pattern.Center;
            var points2 = points.Select(p => new PointF(p.X - center.X, p.Y - center.Y));
            var lenInfos = points2.Select(p => new Tuple<PointF, double>(p, Tools.VectorLengthSquared(p))).OrderByDescending(tpl => tpl.Item2);
            float maxLen = (float)Math.Sqrt(lenInfos.First().Item2);
            float fac = maxLen == 0 ? 1F : 1F / maxLen;
            seedCurvePoints = points2.Select(p => new PointF(fac * p.X, fac * p.Y)).ToList();
            PointF maxPoint = lenInfos.First().Item1;
            PreviewZFactor = Complex.CreateFromModulusAndArgument(1D, (2D * Math.PI) - Math.Atan2(maxPoint.Y, maxPoint.X));
            return true;
        }

        public override bool ComputeCurvePoints(Complex zVector, bool recomputeInnerSection = true, bool forOutline = false, 
                                                int seedPointsIndex = -1)
        {
            bool success = seedCurvePoints != null && JoinInfos.Count > 0;
            if (success)
            {
                Complex zVec1 = origZVector;
                zVec1.Normalize();
                Complex zTransform = zVector / zVec1;
                var pTransform = new PointF((float)zTransform.Re, (float)zTransform.Im);
                CurvePoints = seedCurvePoints.Select(p => Tools.RotatePoint(p, pTransform))
                                             .Select(p => new PointF(p.X + Center.X, p.Y + Center.Y))
                                             .ToArray();
                TransformCurvePoints(CurvePoints);
            }
            else
                CurvePoints = new PointF[0];
            return success;
        }

        public override bool ComputeSeedPoints(bool computeRandom = false)
        {
            SeedPoints = new PolarCoord[1];
            DoInits();
            ClearRenderingCache();
            return true;
        }

        public static int FindClosestIndex(PointF p, PointF[] points, double bufferSize = 30.0)
        {
            var infos = Enumerable.Range(0, points.Length)
                        .Select(i => new Tuple<int, double>(i, Tools.DistanceSquared(p, points[i])))
                        .Where(tpl => tpl.Item2 <= bufferSize)
                        .OrderBy(tpl => tpl.Item2).ThenBy(tpl => tpl.Item1);
            if (infos.Any())
                return infos.First().Item1;
            else
                return -1;
        }

        public static int FindInsertIndex(Pattern pattern, PointF joinPoint, float bufferSize = 30F)
        {
            if (pattern.CurvePoints == null)
            {
                if (!pattern.ComputeCurvePoints(pattern.ZVector))
                    return -1;
            }
            return Tools.FindClosestIndex(joinPoint, pattern.CurvePoints, bufferSize);
        }

        //private void AppendPatternXml(XmlNode parentNode, Pattern pattern, string nodeName, XmlTools xmlTools)
        //{
        //    if (pattern == null)
        //        return;
        //    XmlNode xmlNode = xmlTools.CreateXmlNode(nodeName);
        //    pattern.ToXml(xmlNode, xmlTools);
        //    parentNode.AppendChild(xmlNode);
        //}

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(JoinedPatterns);
            XmlNode xmlNode = base.ToXml(parentNode, xmlTools, xmlNodeName);
            return xmlNode;
        }

        protected override void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.ExtraXml(parentNode, xmlTools);
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(origZVector), origZVector));
            foreach (JoinInfo info in JoinInfos)
            {
                info.ToXml(parentNode, xmlTools);
            }
        }

        public override void FromXml(XmlNode node)
        {
            base.FromXml(node);
            if (origZVector == Complex.Zero)
                origZVector = ZVector; //Legacy code.
            ComputeSeedCurvePoints();
            DoInits();
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            bool retVal = true;
            switch (node.Name)
            {
                case nameof(JoinInfo):
                    JoinInfo joinInfo = new JoinInfo(node, Design);
                    JoinInfos.Add(joinInfo);
                    break;
                case nameof(origZVector):
                    origZVector = Tools.GetComplexFromXml(node);
                    break;
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }
    }
}
