using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class PathPadding
    {
        //public struct AngleInfo
        //{
        //    public float Angle { get; }
        //    public int StartIndex { get; set; }
        //    public int EndIndex { get; set; }
        //    public int PathIndex { get; set; }

        //    public AngleInfo(float angle)
        //    {
        //        Angle = angle;
        //        StartIndex = -1;
        //        EndIndex = -1;
        //        PathIndex = int.MinValue;
        //    }
        //}

        public bool TransformPath { get; set; } = true;
        public bool InterpolateCorners { get; set; } = true;
        public float Padding { get; set; }
        public bool IsClosedPath { get; set; } = true;
        public bool AllowMultiplePaths { get; set; } = true;
        //public double MinAngle { get; set; } = Math.PI / 60.0;  //3 degrees.
        //public double Sign { get; private set; }
        public Func<int, float> PaddingScaleFunc { get; set; }
        public PointF[] SourcePoints { get; private set; }
        public List<PointF> Corners { get; } = new List<PointF>();
        private double avgSegLen { get; set; }
        private float avgSegLenSquared { get; set; } 
        //public List<AngleInfo> AngleInfos { get; private set; }
        private List<DistanceSquare> distanceSquares { get; set; }

        private float currentPadding { get; set; }
        private float minDeltaDist { get; set; }
        private float paddingSquared { get; set; }

        public List<List<PointF>> ComputePath(PointF[] points)
        {
            if (!Initialize(points))
                return new List<List<PointF>>() { new List<PointF>(points) };
            List<List<PointF>> paths = ComputePathHelper();
            Cleanup();
            return paths;
        }

        public PolarCoord[] ComputePath(PolarCoord[] seedPoints, float padding, out List<PolarCoord[]> seedPointArrays)
        {
            Padding = padding;
            PointF[] points = seedPoints.Select(sp => sp.ToRectangular()).ToArray();
            var paths = ComputePath(points);
            seedPointArrays = paths.Select(ps => ps.Select(p => PolarCoord.ToPolar(p)).ToArray()).ToList();
            var polarPath = new List<PolarCoord>();
            PolarCoord center = new PolarCoord(0, 0);
            for (int i = 0; i < seedPointArrays.Count; i++)
            {
                polarPath.AddRange(seedPointArrays[i]);
                if (i < paths.Count - 1)
                    polarPath.Add(center);
            }
            return polarPath.ToArray();
        }

        public bool Initialize(PointF[] points)
        {
            if (points.Length < 2 || Padding == 0)
                return false;
            avgSegLen = Tools.SegmentLengths(points).Average();
            if (avgSegLen == 0)
                return false;
            avgSegLenSquared = (float)(avgSegLen * avgSegLen);
            var pointsList = InterpolatePoints(points);
            SourcePoints = pointsList.ToArray();
            RectangleF boundingRect = Tools.GetBoundingRectangleF(SourcePoints);
            distanceSquares = DistanceSquare.GetSquares(pointsList, boundingRect, 20, out _, offset: true);
            //AngleInfos = new List<AngleInfo>();
            Corners.Clear();
            return true;
        }

        public void Cleanup()
        {
            distanceSquares = null;
            SourcePoints = null;
        }

        private void SetCurrentPadding(float padding)
        {
            currentPadding = padding;
            paddingSquared = padding * padding;
            minDeltaDist = 0.001F * Math.Max(paddingSquared, 0.1F);
        }

        private List<PointF> InterpolatePoints(PointF[] points)
        {
            var points2 = new List<PointF>();
            points2.Add(points[0]);
            float minLenSq = 4F * avgSegLenSquared;
            for (int i = 1; i < points.Length; i++)
            {
                float distSq = Tools.DistanceSquared(points[i - 1], points[i]);
                if (distSq > 0)
                {
                    if (distSq <= minLenSq)
                        points2.Add(points[i]);
                    else
                        InterpolateTo(points2, points[i]);
                }
            }
            return points2;
        }

        public List<List<PointF>> ComputePathHelper()
        {
            var path = new List<PointF>();
            var breakIndices = new HashSet<int>();
            bool addedPoint = false;
            SetCurrentPadding(Padding);
            for (int i = 0; i < SourcePoints.Length; i++)
            {
                int i2 = i + 1;
                if (i2 == SourcePoints.Length)
                {
                    if (IsClosedPath)
                        i2 = 0;
                    else
                        break;
                }
                if (PaddingScaleFunc != null)
                {
                    SetCurrentPadding(PaddingScaleFunc(i) * Padding);
                }
                PointF vec = Tools.GetVector(SourcePoints[i], SourcePoints[i2]);
                if (vec.X != 0 || vec.Y != 0)
                {
                    float scale = currentPadding / (float)Tools.VectorLength(vec);
                    //Get perpendicular vector of length = Padding:
                    vec = new PointF(scale * vec.Y, -scale * vec.X);
                    float vecLenSq = Tools.VectorLengthSquared(vec);
                    PointF pathP = Tools.AddPoint(SourcePoints[i], vec);
                    float distSq = DistanceSquare.FindMinDistanceSquared(pathP, distanceSquares, out PointF? nearestP);
                    if (Math.Abs(paddingSquared - distSq) < minDeltaDist)
                    {
                        path.Add(pathP);
                        addedPoint = true;
                    }
                    else if (addedPoint)
                    {
                        if (AllowMultiplePaths)
                            breakIndices.Add(path.Count);
                        addedPoint = false;
                    }
                }
            }
            var paths = new List<List<PointF>>();
            if (TransformPath && path.Count > 0)
            {
                List<PointF> curPath = new List<PointF>();
                curPath.Add(path[0]);
                float lenSq = 0;
                float minLenSq = 4F * avgSegLenSquared;
                int iA1 = 0, iA2 = 0;
                for (int i = 1; i < path.Count; i++)
                {
                    int iB1 = -1, iB2 = -1;
                    float segLenSq = Tools.DistanceSquared(path[i - 1], path[i]);
                    if (segLenSq > minLenSq)
                    {
                        if (breakIndices.Contains(i))
                        {
                            if (curPath.Count > 0)
                                paths.Add(curPath);
                            curPath = new List<PointF>();
                            lenSq = 0;
                        }
                        else if (InterpolateCorners)
                        {
                            if (lenSq == 0)
                            {
                                iA1 = Tools.GetIndexInRange(i - 3, path.Count);
                                iA2 = Tools.GetIndexInRange(i - 2, path.Count);
                            }
                            lenSq += segLenSq;
                        }
                    }
                    if (segLenSq <= minLenSq || i == path.Count - 1)
                    {
                        if (lenSq > minLenSq)
                        {
                            iB1 = Tools.GetIndexInRange(i + 2, path.Count);
                            iB2 = Tools.GetIndexInRange(i + 1, path.Count);
                            PointF? intersection = Tools.GetIntersection(path[iA1], path[iA2], path[iB1], path[iB2]);
                            if (intersection.HasValue)
                            {
                                Corners.Add(intersection.Value);
                                InterpolateTo(curPath, intersection.Value);
                                InterpolateTo(curPath, path[i]);
                            }
                            else
                                curPath.Add(path[i]);
                        }
                        lenSq = 0;
                    }
                    if (lenSq == 0 && iB1 == -1)
                        curPath.Add(path[i]);
                }
                if (curPath.Count > 0)
                    paths.Add(curPath);
            }
            else
                paths.Add(path);
            if (IsClosedPath)
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    Tools.ClosePoints(paths[i]);
                }
            }
            return paths;
        }

        private void InterpolateTo(List<PointF> path, PointF pEnd)
        {
            PointF p = path.Last();
            PointF vec = Tools.GetVector(p, pEnd);
            double dist = Tools.VectorLength(vec);
            int n = (int)Math.Ceiling(dist / avgSegLen);
            float scale = 1F / n;
            vec = new PointF(scale * vec.X, scale * vec.Y);
            for (int i = 1; i < n; i++)
            {
                p = Tools.AddPoint(p, vec);
                path.Add(p);
            }
            path.Add(pEnd);
        }

        //private void ComputeAngleInfos()
        //{
        //    AngleInfos = new List<AngleInfo>();
        //    Complex zPrev = Complex.Zero;
        //    double angle = 0;
        //    int iStart = -1, iEnd = -1;
        //    int i = 1;
        //    PointF pPrev = points[0];
        //    while (true)
        //    {
        //        int ind = i;
        //        if (i >= points.Length)
        //        {
        //            if (IsClosedPath)
        //            {
        //                ind -= points.Length;
        //                if (ind >= 10)
        //                    iEnd = ind;
        //            }
        //            else
        //                iEnd = points.Length - 1;
        //        }
        //        if (ind < points.Length)
        //        {
        //            PointF p = points[ind];
        //            PointF vec = Tools.GetVector(pPrev, p);
        //            if (vec.X == 0 && vec.Y == 0)
        //            {
        //                i++;
        //                continue;
        //            }
        //            pPrev = p;
        //            Complex zVec = new Complex(vec.X, vec.Y);
        //            if (zPrev != Complex.Zero)
        //            {
        //                Complex zAngle = zVec / zPrev;
        //                double a = zAngle.GetArgument();
        //                if (Math.Abs(a) > MinAngle)
        //                {
        //                    if (iStart == -1)
        //                        iStart = ind;
        //                    angle += a;
        //                }
        //                else if (iStart != -1)
        //                {
        //                    iEnd = ind;
        //                }
        //            }
        //            zPrev = zVec;
        //        }
        //        if (iStart != -1 && iEnd != -1)
        //        {
        //            if (Math.Abs(angle) > MinAngle)
        //            {
        //                int i2 = iEnd;
        //                if (iEnd < iStart)
        //                    i2 += points.Length;
        //                int iMid = ((iStart + i2) / 2) % points.Length;
        //                int sign = Math.Sign(angle);
        //                double angle1 = Math.Abs(angle);
        //                angle1 = sign * Math.Min(angle1, Math.PI - angle1);
        //                if (Math.Abs(angle1) > MinAngle)
        //                {
        //                    var angleInfo = new AngleInfo((float)angle1);
        //                    if (angle1 * Sign > 0)
        //                    {
        //                        float minLen = Math.Abs(Padding / (float)Math.Tan(0.5 * angle1));
        //                        minLen *= minLen;
        //                        angleInfo.StartIndex = FindIndex(iMid, minLen, -1);
        //                        angleInfo.EndIndex = FindIndex(iMid, minLen, 1);
        //                    }
        //                    else
        //                    {
        //                        angleInfo.StartIndex = iMid;
        //                        angleInfo.EndIndex = -1;
        //                    }
        //                    AngleInfos.Add(angleInfo);
        //                }
        //            }
        //            iStart = iEnd = -1;
        //            angle = 0;
        //        }
        //        if (IsClosedPath)
        //        {
        //            if (i >= points.Length + 10)
        //                break;
        //        }
        //        else if (i >= points.Length)
        //        {
        //            break;
        //        }
        //        i++;
        //    }
        //}

        //private int FindIndex(int i, float minLenSquared, int increment)
        //{
        //    PointF p = points[i];
        //    int j = i + increment;
        //    while (true)
        //    {
        //        if (j >= points.Length)
        //        {
        //            if (IsClosedPath)
        //                j = 0;
        //            else
        //                break;
        //        }
        //        else if (j < 0)
        //        {
        //            if (IsClosedPath)
        //                j = points.Length - 1;
        //            else
        //                break;
        //        }
        //        if (j == i)
        //            break;
        //        if (Tools.DistanceSquared(p, points[j]) >= minLenSquared)
        //            break;
        //        j += increment;
        //    }
        //    return j;
        //}
        
    }
}
