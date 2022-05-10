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
        public struct AngleInfo
        {
            public float Angle { get; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public int PathIndex { get; set; }

            public AngleInfo(float angle)
            {
                Angle = angle;
                StartIndex = -1;
                EndIndex = -1;
                PathIndex = int.MinValue;
            }
        }

        public PointF[] Points { get; private set; }
        public float Padding { get; private set; }
        public bool IsClosedPath { get; private set; }
        public double MinAngle { get; set; } = 0.01 * Math.PI;
        public double Sign { get; private set; }
        private float avgSegLenSquared { get; set; } 
        private List<AngleInfo> angleInfos { get; set; }

        public List<PointF> ComputePath(PointF[] points, float padding, bool isClosed = true, double sign = 0)
        {
            Points = points;
            Padding = padding;
            IsClosedPath = isClosed;
            if (sign == 0)
                sign = 1;
            sign *= Math.Sign(Padding);
            Sign = sign;
            if (points.Length < 2 || padding == 0)
                return new List<PointF>(Points);
            double avgSegLen = Tools.SegmentLengths(points).Average();
            if (avgSegLen == 0)
                return new List<PointF>(Points);
            avgSegLenSquared = (float)(avgSegLen * avgSegLen);
            ComputeAngleInfos();
            return ComputePath();
        }

        private List<PointF> ComputePath()
        {
            var path = new List<PointF>();
            int i = 0, j = 0;
            while (true)
            {
                if (j <= angleInfos.Count)
                {
                    AngleInfo angleInfo = angleInfos[j % angleInfos.Count];
                    if (angleInfo.EndIndex >= 0)
                    {
                        if (i == angleInfo.StartIndex ||
                           (j == 0 && angleInfo.StartIndex > angleInfo.EndIndex && i <= angleInfo.EndIndex))
                        {
                            if (angleInfo.EndIndex < angleInfo.StartIndex)
                                break;
                            i = angleInfo.EndIndex;
                            j++;
                        }
                    }
                    else if (i - 1 == angleInfo.StartIndex)
                    {
                        j++;
                        angleInfo.PathIndex = path.Count - 1;
                    }
                }
                if (i < Points.Length)
                {
                    int i2 = i + 1;
                    if (i2 >= Points.Length)
                    {
                        if (IsClosedPath)
                            i2 = 0;
                        else
                            break;
                    }
                    PointF vec = Tools.GetVector(Points[i], Points[i2]);
                    if (vec.X != 0 || vec.Y != 0)
                    {
                        float scale = Padding / (float)Tools.VectorLength(vec);
                        //Get perpendicular vector of length = Padding:
                        vec = new PointF(scale * vec.Y, -scale * vec.X);
                        path.Add(Tools.AddPoint(Points[i], vec));
                    }
                }
                i++;
                if (i > Points.Length)
                    break;
            }
            var pathInfos = angleInfos.FindAll(o => o.PathIndex >= -1 && o.PathIndex < path.Count - 1);
            if (pathInfos.Count > 0 && path.Count >= 2)
            {
                var path2 = new List<PointF>();
                int startI = 0;
                foreach (var pathInfo in pathInfos)
                {
                    int ind = Tools.GetIndexInRange(pathInfo.PathIndex, path.Count);
                    PointF pStart = path[ind];
                    int ind2 = pathInfo.PathIndex + 1;
                    PointF pEnd = path[ind2];
                    if (Tools.DistanceSquared(pStart, pEnd) >= 4F * avgSegLenSquared)
                    {
                        if (pathInfo.PathIndex > startI)
                        {
                            path2.AddRange(path.Skip(startI).Take(pathInfo.PathIndex - startI - 1));
                            startI = pathInfo.PathIndex + 1;
                        }
                        path2.Add(pStart);
                        PointF midVec = Tools.GetRotationVector(Math.PI + 0.5 * pathInfo.Angle);
                        PointF p = Points[pathInfo.StartIndex];
                        PointF pMid = new PointF(p.X + Padding * midVec.X, p.Y + Padding * midVec.Y);
                        InterpolateTo(path2, pMid);
                        InterpolateTo(path2, pEnd);
                    }
                }
                if (startI < path.Count)
                    path2.AddRange(path.Skip(startI));
            }
            return path;
        }

        private void InterpolateTo(List<PointF> path, PointF pEnd)
        {
            PointF p = path.Last();
            PointF vec = Tools.GetVector(p, pEnd);
            float dist = (float)Tools.VectorLength(vec);
            int n = (int)Math.Ceiling(dist / (float)Math.Sqrt(avgSegLenSquared));
            float scale = 1F / n;
            vec = new PointF(scale * vec.X, scale * vec.Y);
            for (int i = 1; i < n; i++)
            {
                p = Tools.AddPoint(p, vec);
                path.Add(p);
            }
            path.Add(pEnd);
        }

        private void ComputeAngleInfos()
        {
            angleInfos = new List<AngleInfo>();
            Complex zPrev = Complex.Zero;
            for (int i = 1; i <= Points.Length; i++)
            {
                int ind = i;
                if (i == Points.Length)
                {
                    if (IsClosedPath)
                        ind = 0;
                    else
                        break;
                }
                PointF vec = Tools.GetVector(Points[i - 1], Points[ind]);
                if (vec.X == 0 && vec.Y == 0)
                    continue;
                Complex zVec = new Complex(vec.X, vec.Y);
                if (zPrev != Complex.Zero)
                {
                    Complex zAngle = zVec / zPrev;
                    double angle = zAngle.GetArgument();
                    if (Math.Abs(angle) > MinAngle)
                    {
                        var angleInfo = new AngleInfo((float)angle);
                        if (angle * Sign > 0)
                        {
                            float minLen = Math.Abs(Padding) * (float)Math.Cos(0.5 * angle);
                            minLen *= minLen;
                            angleInfo.StartIndex = FindIndex(ind, minLen, -1);
                            angleInfo.EndIndex = FindIndex(ind, minLen, 1);
                        }
                        else
                        {
                            angleInfo.StartIndex = ind;
                            angleInfo.EndIndex = -1;
                        }
                        angleInfos.Add(angleInfo);
                    }
                }
                zPrev = zVec;
            }
        }

        private int FindIndex(int i, float minLenSquared, int increment)
        {
            PointF p = Points[i];
            int j = i + increment;
            while (true)
            {
                if (j >= Points.Length)
                {
                    if (IsClosedPath)
                        j = 0;
                    else
                        break;
                }
                else if (j < 0)
                {
                    if (IsClosedPath)
                        j = Points.Length - 1;
                    else
                        break;
                }
                if (j == i)
                    break;
                if (Tools.DistanceSquared(p, Points[j]) >= minLenSquared)
                    break;
                j += increment;
            }
            return j;
        }
        
    }
}
