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
            for (int i = 1; i < points.Count; i++)
            {
                PointF prevP = points[i - 1];
                PointF p = points[i];
                if (p == prevP)
                    continue;
                PointF vector = new PointF(p.X - prevP.X, p.Y - prevP.Y);
                float unitScale = 1F / (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
                //Make vector a unit vector:
                vector = new PointF(unitScale * vector.X, unitScale * vector.Y);
                Complex z = new Complex(vector.X, vector.Y);
                if (i > 1)
                {
                    Complex zAngle = z / zPrev;
                    if (Math.Abs(zAngle.Im) > maxIm)
                    {
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
            path2.Reverse();
            path.AddRange(path2);
            return path;
        }

    }
}
