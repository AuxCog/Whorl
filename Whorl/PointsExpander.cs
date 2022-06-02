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
        public bool InterpolateCorners { get; set; } = true;
        public bool UseNewVersion { get; set; } = false;

        private bool closeStart { get; set; }
        private PathPadding pathPadding { get; } = new PathPadding();
        private float thickness { get; set; }

        private double tanhSlope { get; set; }
        private bool useScale { get; set; }

        private double GetTanh(double x)
        {
            return 0.5 * (1.0 + Math.Tanh(tanhSlope * (x - EndOffset)));
        }

        public static PointF GetCenter(List<PointF> points, bool average = false)
        {
            if (points.Count == 0)
                return PointF.Empty;
            PointF center;
            if (average)
            {
                center = new PointF(points.Select(p => p.X).Average(),
                                    points.Select(p => p.Y).Average());
            }
            else
            {
                RectangleF boundingRect = Tools.GetBoundingRectangleF(points);
                center = new PointF(boundingRect.X + 0.5F * boundingRect.Width,
                                           boundingRect.Y + 0.5F * boundingRect.Height);
            }
            return center;
            //return points.OrderBy(p => Tools.DistanceSquared(p, center)).First();
        }

        public static List<PointF> SetCenter(List<PointF> points, out PointF center, bool average = false)
        {
            center = GetCenter(points, average);
            PointF c = center;
            return points.Select(p => new PointF(p.X - c.X, p.Y - c.Y)).ToList();
        }

        private float GetPaddingWidthScale(int index)
        {
            return GetWidthScale(pathPadding.SourcePoints.Length, index);
        }

        private float GetWidthScale(int pointsCount, int index)
        {
            double tanh1 = closeStart ? GetTanh(index) : 1.0;
            double tanh2 = GetTanh(pointsCount - index);
            float scale = (float)(tanh1 * tanh2);
            if (useScale)
            {
                scale *= Scales[Tools.GetIndexInRange(index, Scales.Length)];
            }
            return scale;
        }

        private float GetScaledThickness(List<PointF> points, int i)
        {
            return thickness * GetWidthScale(points.Count, i);
        }

        public List<PointF> ClosePoints(List<PointF> points)
        {
            closeStart = true;
            SetThickness(points);
            if (!GetPaths(points, out var path, out var path2))
                return points;
            path2.Reverse();
            path.AddRange(path2);
            return path;
        }

        public List<PointF> RepeatPath(List<PointF> points, int sectors, int repetitions = 0, bool reversePoints = false)
        {
            if (points.Count < MinPointsCount)
                return points;
            if (reversePoints)
                points.Reverse();
            if (sectors <= 1)
                return ClosePoints(points);
            if (repetitions <= 0)
                repetitions = sectors;
            closeStart = false;
            SetThickness(points);
            double angle = 2.0 * Math.PI / sectors;
            if (!GetPaths(points, out var path, out var path2))
                return points;
            path2.Reverse();
            path.AddRange(path2);
            var fullPath = new List<PointF>(path);
            var centerPoints = new List<PointF>();
            centerPoints.Add(path[0]);
            for (int i = 1; i < repetitions; i++)
            {
                PointF rotationVec = Tools.GetRotationVector(i * angle);
                var newPath = path.Select(p => Tools.RotatePoint(p, rotationVec)).ToList();
                PointF p0 = fullPath.Last();
                PointF delta = Tools.SubtractPoint(p0, newPath.First());
                centerPoints.Add(p0);
                fullPath.AddRange(newPath.Select(p => Tools.AddPoint(p, delta)));
            }
            PointF center = new PointF(centerPoints.Select(p => p.X).Average(), 
                                       centerPoints.Select(p => p.Y).Average());
            fullPath = fullPath.Select(p => Tools.SubtractPoint(p, center)).ToList();
            return fullPath;
        }

        private void SetThickness(List<PointF> points)
        {
            useScale = Scales != null && Scales.Length > 0;
            if (points.Count <= 1)
                return;
            RectangleF boundingRect = Tools.GetBoundingRectangleF(points);
            float maxExtent = Math.Max(boundingRect.Width, boundingRect.Height);
            thickness = 0.001F * maxExtent * Thickness;
        }

        private bool GetPaths(List<PointF> points, out List<PointF> path, out List<PointF> path2)
        {
            path = path2 = null;
            MinCornerAngle = Math.Max(1.0, MinCornerAngle);
            if (points.Count < MinPointsCount)
                return false;
            tanhSlope = 0.01 * EndSlope;
            if (UseNewVersion)
            {
                pathPadding.IsClosedPath = false;
                pathPadding.AllowMultiplePaths = false;
                pathPadding.PaddingScaleFunc = GetPaddingWidthScale;
                pathPadding.InterpolateCorners = InterpolateCorners;
                pathPadding.Padding = thickness;
                if (!pathPadding.Initialize(points.ToArray()))
                    return false;
                var paths = pathPadding.ComputePathHelper();
                path = paths.SelectMany(p => p).ToList();
                pathPadding.Padding = -thickness;
                paths = pathPadding.ComputePathHelper();
                path2 = paths.SelectMany(p => p).ToList();
                return true;
            }
            double maxIm = Math.Sin(Tools.DegreesToRadians(MinCornerAngle));
            path = new List<PointF>();
            path2 = new List<PointF>();
            path.Add(points[0]);
            path2.Add(points[0]);
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
                float scale = GetScaledThickness(points, i);
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
            return true;
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
