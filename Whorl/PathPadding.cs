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
        public float Padding { get; set; }
        public bool IsClosedPath { get; set; } = true;
        public double MinAngle { get; set; } = Math.PI / 60.0;  //3 degrees.
        public double Sign { get; private set; }
        private double avgSegLen { get; set; }
        private float avgSegLenSquared { get; set; } 
        public List<AngleInfo> AngleInfos { get; private set; }

        public List<PointF> ComputePath(PointF[] points, double sign = 1)
        {
            Points = points;
            Sign = sign * Math.Sign(Padding);
            if (points.Length < 2 || Padding == 0)
                return new List<PointF>(Points);
            avgSegLen = Tools.SegmentLengths(points).Average();
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
                if (j <= AngleInfos.Count && AngleInfos.Count > 0)
                {
                    AngleInfo angleInfo = AngleInfos[j % AngleInfos.Count];
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
            var pathInfos = AngleInfos.FindAll(o => o.PathIndex >= -1 && o.PathIndex < path.Count - 1);
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
                path = path2;
            }
            return path;
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

        private void ComputeAngleInfos()
        {
            AngleInfos = new List<AngleInfo>();
            Complex zPrev = Complex.Zero;
            double angle = 0;
            int iStart = -1, iEnd = -1;
            int i = 1;
            PointF pPrev = Points[0];
            while (true)
            {
                int ind = i;
                if (i >= Points.Length)
                {
                    if (IsClosedPath)
                    {
                        ind -= Points.Length;
                        if (ind >= 10)
                            iEnd = ind;
                    }
                    else
                        iEnd = Points.Length - 1;
                }
                if (ind < Points.Length)
                {
                    PointF p = Points[ind];
                    PointF vec = Tools.GetVector(pPrev, p);
                    if (vec.X == 0 && vec.Y == 0)
                    {
                        i++;
                        continue;
                    }
                    pPrev = p;
                    Complex zVec = new Complex(vec.X, vec.Y);
                    if (zPrev != Complex.Zero)
                    {
                        Complex zAngle = zVec / zPrev;
                        double angle1 = zAngle.GetArgument();
                        if (Math.Abs(angle1) > MinAngle)
                        {
                            if (iStart == -1)
                                iStart = ind;
                            angle += angle1;
                        }
                        else if (iStart != -1)
                        {
                            iEnd = ind;
                        }
                    }
                    zPrev = zVec;
                }
                if (iStart != -1 && iEnd != -1)
                {
                    if (Math.Abs(angle) > MinAngle)
                    {
                        int i2 = iEnd;
                        if (iEnd < iStart)
                            i2 += Points.Length;
                        int iMid = ((iStart + i2) / 2) % Points.Length;
                        var angleInfo = new AngleInfo((float)angle);
                        if (angle * Sign > 0)
                        {
                            float minLen = Math.Abs(Padding) * (float)Math.Cos(0.5 * angle);
                            minLen *= minLen;
                            angleInfo.StartIndex = FindIndex(iMid, minLen, -1);
                            angleInfo.EndIndex = FindIndex(iMid, minLen, 1);
                        }
                        else
                        {
                            angleInfo.StartIndex = iMid;
                            angleInfo.EndIndex = -1;
                        }
                        AngleInfos.Add(angleInfo);
                    }
                    iStart = iEnd = -1;
                    angle = 0;
                }
                if (IsClosedPath)
                {
                    if (i >= Points.Length + 10)
                        break;
                }
                else if (i >= Points.Length)
                {
                    break;
                }
                i++;
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
