using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class OutlinePadding
    {
        public static List<PolarCoord> GetPaddedPoints(PolarCoord[] seedPoints, float scale, double maxAngle, 
                                                       out List<PolarCoord[]> seedPointArrays, bool enhance = true)
        {
            List<PointF> points = seedPoints.Select(sp => sp.ToRectangular()).ToList();
            List<PointF> newPoints = GetPaddedPoints(points, scale, maxAngle, out List<int> newPointIndices, enhance);
            var newSeedPoints = newPoints.Select(p => PolarCoord.ToPolar(p)).ToList();
            if (newPointIndices == null || newPointIndices.Count == 0)
                seedPointArrays = null;
            else
            {
                seedPointArrays = new List<PolarCoord[]>();
                int iPrev = 0;
                foreach (int ind in newPointIndices)
                {
                    if (ind > iPrev)
                    {
                        seedPointArrays.Add(newSeedPoints.Skip(iPrev).Take(ind - iPrev).ToArray());
                        iPrev = ind;
                    }
                }
                if (iPrev < seedPoints.Length)
                {
                    seedPointArrays.Add(newSeedPoints.Skip(iPrev).ToArray());
                }
                foreach (int ind in newPointIndices.OrderByDescending(i => i))
                {
                    newSeedPoints.Insert(ind, new PolarCoord(0, 0));
                }
            }
            return newSeedPoints;
        }

        public static List<PointF> GetPaddedPoints(
                      List<PointF> points, float scale, double maxAngle, out List<int> newPointIndices, bool enhance = true)
        {
            newPointIndices = null;
            var newPoints = new List<PointF>();
            scale = -scale;
            //points = points.Select(p => new PointF(p.X - center.X, p.Y - center.Y)).ToList();
            PointF p0 = points[0];
            int maxI = points.Count - 1;
            bool isClosed = p0 == points[maxI];
            if (isClosed)
            {
                maxI--;
                points = points.Take(maxI + 1).ToList();
            }
            if (maxI <= 1)
                return newPoints;
            maxAngle = Math.PI * maxAngle / 180.0;
            double maxSine = Math.Abs(Math.Sin(maxAngle));
            double minCosine = Math.Cos(maxAngle + Math.PI);
            var cornerIndices = new List<int>();
            for (int i = 1; i <= points.Count; i++)
            {
                PointF p = points[i % points.Count];
                PointF vec = Tools.GetVector(points[i - 1], p);
                float vecScale = scale / (float)Tools.VectorLength(vec);
                //Get perpendicular vector:
                vec = new PointF(vecScale * vec.Y, -vecScale * vec.X);
                newPoints.Add(new PointF(p.X + vec.X, p.Y + vec.Y));
                if (maxSine != 0)
                {
                    int ind = i - 1;
                    int prevI = ind - 1;
                    if (prevI < 0)
                        prevI += points.Count;
                    PointF vec1 = Tools.GetVector(points[prevI], points[ind]);
                    PointF vec2 = Tools.GetVector(points[ind], points[(ind + 1) % points.Count]);
                    Complex z1 = new Complex(vec1.X, vec1.Y);
                    Complex z2 = new Complex(vec2.X, vec2.Y);
                    if (z2 != Complex.Zero)
                    {
                        Complex zAngle = z1 / z2;
                        zAngle.Normalize();
                        if (Math.Abs(zAngle.Im) > maxSine || zAngle.Re < minCosine)
                        {
                            if (cornerIndices.Count == 0 || ind > cornerIndices.Last() + 1)
                            {
                                if (zAngle.Im < 0 && zAngle.Re > minCosine)
                                    cornerIndices.Add(ind);
                            }
                        }
                    }
                }
            }
            if (enhance && cornerIndices.Count > 0)
            {
                newPoints = EnhancePadding(points, newPoints, cornerIndices, out newPointIndices);
            }
            if (isClosed)
            {
                if (newPoints.Count > 1 && newPoints.Last() != newPoints.First())
                    newPoints.Add(newPoints.First());
            }
            //newPoints = newPoints.Select(p => new PointF(p.X + center.X, p.Y + center.Y)).ToList();
            return newPoints;
        }

        private static List<PointF> EnhancePadding(List<PointF> points, List<PointF> newPoints, 
                                                   List<int> cornerIndices, out List<int> newPointIndices)
        {
            //cornerIndices.Add(cornerIndices.First());
            newPointIndices = new List<int>();
            var infos = new List<Tuple<int, int, bool>>();
            PointF lastVec = PointF.Empty;
            int lastInd = 0;
            int firstMinInd = -1;
            bool firstPass = true;
            int cornerInd = cornerIndices[0];
            int minInd;
            if (cornerInd > 0)
                minInd = FindMinInd(points, 0, cornerInd);
            else if (cornerIndices.Count > 1)
                minInd = FindMinInd(points, cornerInd, cornerIndices[1]);
            else
                minInd = FindMinInd(points, 0, points.Count);
            if (minInd < 0)
                return newPoints;
            if (minInd == 0)
            {
                float minDist = Tools.VectorLengthSquared(points[minInd]);
                for (int i = points.Count - 1; i > cornerIndices.Last(); i--)
                {
                    float dist = Tools.VectorLengthSquared(points[i]);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minInd = i;
                    }
                }
            }
            if (minInd > 0)
            {
                points = points.Skip(minInd).Concat(points.Take(minInd)).ToList();
                newPoints = newPoints.Skip(minInd).Concat(newPoints.Take(minInd)).ToList();
                for (int i = 0; i < cornerIndices.Count; i++)
                {
                    cornerInd = cornerIndices[i] - minInd;
                    if (cornerInd < 0)
                        cornerInd += points.Count;
                    cornerIndices[i] = cornerInd;
                }
                cornerIndices.Sort();
                minInd = 0;
            }
            for (int i = 0; i < cornerIndices.Count; i++)
            {
                cornerInd = cornerIndices[i];
                if (i > 0)
                    minInd = FindMinInd(points, lastInd, cornerInd);
                if (minInd >= 0)
                {
                    if (firstPass)
                        firstMinInd = minInd;
                    else
                    {
                        AddStartEndTuple(lastInd + 1, minInd, newPoints, lastVec, infos, 1F);
                    }
                    PointF vec = points[cornerInd];
                    float unitFactor = 1F / (float)Tools.VectorLength(vec);
                    vec = new PointF(unitFactor * vec.X, -unitFactor * vec.Y);
                    AddStartEndTuple(minInd, Math.Min(cornerInd, newPoints.Count), newPoints, vec, infos, -1F);
                    lastVec = vec;
                }
                lastInd = cornerInd;
                firstPass = false;
            }
            if (lastInd < newPoints.Count)
            {
                AddStartEndTuple(lastInd + 1, newPoints.Count, newPoints, lastVec, infos, 1F);
            }
            if (infos.Count > 0)
            {
                var points2 = new List<PointF>();
                //PointF newCenter = PointF.Empty;
                var lastInfo = infos.Last();
                int endInd = lastInfo.Item1 + lastInfo.Item2;
                for (int i = 0; i < infos.Count; i++)
                {
                    var info = infos[i];
                    var section = newPoints.Skip(info.Item1).Take(info.Item2);
                    bool addCenter = !info.Item3;
                    if (addCenter)
                    {
                        PointF vec;
                        if (i == 0)
                            vec = Tools.GetVector(newPoints[endInd], section.First());
                        else
                            vec = Tools.GetVector(points2.Last(), section.First());
                        if (Tools.VectorLengthSquared(vec) > 0.0001F)
                        {
                            newPointIndices.Add(points2.Count);
                            //points2.Add(newCenter);
                        }
                    }
                    points2.AddRange(section);
                }
                newPoints = points2;
            }
            return newPoints;
        }

        private static int FindMinInd(List<PointF> points, int startInd, int endInd)
        {
            float minDist = float.MaxValue;
            int minInd = -1;
            for (int j = startInd; j < endInd; j++)
            {
                float dist = Tools.VectorLengthSquared(points[j]);
                if (dist < minDist)
                {
                    minDist = dist;
                    minInd = j;
                }
            }
            return minInd;
        }

        private static void AddStartEndTuple(int minInd, int maxInd, List<PointF> newPoints, PointF vec,
                                      List<Tuple<int, int, bool>> infos, float sign, bool append = true)
        {
            int startInd = -1, endInd = maxInd - 1;
            for (int j = minInd; j < maxInd; j++)
            {
                PointF p1 = Tools.RotatePoint(newPoints[j], vec);
                if (sign * p1.Y >= 0)
                {
                    if (startInd == -1)
                        startInd = j;
                }
                else if (startInd != -1)
                {
                    endInd = j;
                    break;
                }
            }
            if (startInd >= 0 && endInd > startInd)
            {
                var tuple = new Tuple<int, int, bool>(startInd, endInd - startInd, sign > 0);
                if (append)
                    infos.Add(tuple);
                else
                    infos.Insert(0, tuple);
            }

        }
    }
}
