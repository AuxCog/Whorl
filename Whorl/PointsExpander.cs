using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class PointsExpander
    {
        public const int MinPointsCount = 3;
        public double EndSlope { get; set; } = 5.0;
        public double EndOffset { get; set; } = 20.0;
        public float Thickness { get; set; } = 10F;
        public double MinCornerAngle { get; set; } = 5.0;
        public float[] Scales { get; set; }
        public bool InterpolateCorners { get; set; } = false;

        private double tanhSlope { get; set; }

        private double GetTanh(double x)
        {
            return 0.5 * (1.0 + Math.Tanh(tanhSlope * (x - EndOffset)));
        }

        public static PointF GetCenter(List<PointF> points)
        {
            if (points.Count == 0)
                return PointF.Empty;
            RectangleF boundingRect = Tools.GetBoundingRectangleF(points);
            PointF center = new PointF(boundingRect.X + 0.5F * boundingRect.Width,
                                       boundingRect.Y + 0.5F * boundingRect.Height);
            return points.OrderBy(p => Tools.DistanceSquared(p, center)).First();
        }

        public static List<PointF> SetCenter(List<PointF> points, out PointF center)
        {
            center = GetCenter(points);
            PointF c = center;
            return points.Select(p => new PointF(p.X - c.X, p.Y - c.Y)).ToList();
        }

        public List<PointF> ClosePoints(List<PointF> points)
        {
            MinCornerAngle = Math.Max(1.0, MinCornerAngle);
            double maxIm = Math.Sin(Tools.DegreesToRadians(MinCornerAngle));
            if (points.Count < MinPointsCount)
                return points;
            RectangleF boundingRect = Tools.GetBoundingRectangleF(points);
            float maxExtent = Math.Max(boundingRect.Width, boundingRect.Height);
            float thickness = 0.001F * maxExtent * Thickness;
            var path = new List<PointF>();
            var path2 = new List<PointF>();
            path.Add(points[0]);
            path2.Add(points[0]);
            tanhSlope = 0.01 * EndSlope;
            bool useScale = Scales != null && Scales.Length > 0;
            Complex zPrev = Complex.Zero;
            float lenSum = 0;
            var cornerIndices = new List<int>();
            for (int i = 1; i < points.Count; i++)
            {
                PointF p = points[i];
                PointF vector = GetVector(points, i);
                float vecLen = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
                if (vecLen == 0)
                    continue;
                lenSum += vecLen;
                float unitScale = 1F / vecLen;
                //Make vector a unit vector:
                vector = new PointF(unitScale * vector.X, unitScale * vector.Y);
                Complex z = new Complex(vector.X, vector.Y);
                if (i > 1)
                {
                    Complex zAngle = z / zPrev;
                    if (Math.Abs(zAngle.Im) > maxIm)
                    {
                        cornerIndices.Add(path.Count);
                        zPrev = z;
                        continue;  //Reached a corner.
                    }
                }
                double tanh1 = GetTanh(i);
                double tanh2 = GetTanh(points.Count - i);
                float scale = thickness * (float)(tanh1 * tanh2);
                if (useScale)
                {
                    scale *= Scales[Tools.GetIndexInRange(i, Scales.Length)];
                }
                //Get perpendicular vector:
                PointF perp = new PointF(scale * vector.Y, -scale * vector.X);
                path.Add(new PointF(p.X + perp.X, p.Y + perp.Y));
                path2.Add(new PointF(p.X - perp.X, p.Y - perp.Y));
                zPrev = z;
            }
            if (InterpolateCorners)
            {
                float avgLen = lenSum / points.Count;
                path = HandleCorners(path, cornerIndices, avgLen);
                path2 = HandleCorners(path2, cornerIndices, avgLen);
            }
            path2.Reverse();
            path.AddRange(path2);
            return path;
        }

        private PointF GetVector(List<PointF> points, int i)
        {
            PointF prevP = points[i - 1];
            PointF p = points[i];
            PointF vector = new PointF(p.X - prevP.X, p.Y - prevP.Y);
            return vector;
        }

        private List<PointF> HandleCorners(List<PointF> points, List<int> cornerIndices, float avgLen)
        {
            List<PointF> result = new List<PointF>();
            float maxLen = 3F * avgLen;
            double maxIm = Math.Sin(Tools.DegreesToRadians(MinCornerAngle));
            int prevInd = 0;
            foreach (int ind in cornerIndices)
            {
                if (ind < 2)
                    continue;
                result.AddRange(points.Skip(prevInd).Take(ind - prevInd));
                prevInd = ind;
                int endInd = -1;
                int maxI = Math.Min(points.Count, ind + 10);
                bool foundMax = false;
                for (int i = ind; i < maxI; i++)
                {
                    PointF vector = GetVector(points, i);
                    float vecLen = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
                    if (vecLen > maxLen)
                        foundMax = true;
                    else if (foundMax)
                    {
                        endInd = i;
                        break;
                    }
                }
                if (endInd >= ind + 1)
                {
                    PointF pA1 = points[ind - 2];
                    PointF pA2 = points[ind - 1];
                    PointF pB1 = points[endInd];
                    PointF pB2 = points[endInd - 1];
                    PointF? intersection = Tools.GetIntersection(pA1, pA2, pB1, pB2);
                    if (intersection != null)
                    {
                        InterpolatePoints(result, intersection.Value, avgLen);
                        InterpolatePoints(result, pB2, avgLen);
                        //result.Add(intersection.Value);
                        prevInd =  endInd;
                    }
                }
            }
            if (prevInd < points.Count)
                result.AddRange(points.Skip(prevInd));
            return result;
        }

        private void InterpolatePoints(List<PointF> points, PointF endPoint, float vectorLen)
        {
            if (points.Count == 0)
                return;
            PointF p = points.Last();
            PointF vector = new PointF(endPoint.X - p.X, endPoint.Y - p.Y);
            float vecLen = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (vecLen == 0)
                return;
            int steps = (int)Math.Ceiling((double)vecLen / vectorLen);
            float scale = 1F / steps;
            vector = new PointF(scale * vector.X, scale * vector.Y);
            for (int i = 0; i < steps; i++)
            {
                p = new PointF(p.X + vector.X, p.Y + vector.Y);
                points.Add(p);
            }
        }
    }
}
