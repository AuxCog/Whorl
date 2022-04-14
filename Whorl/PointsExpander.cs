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

        private double tanhSlope { get; set; }

        private double GetTanh(double x)
        {
            return 0.5 * (1.0 + Math.Tanh(tanhSlope * (x - EndOffset)));
        }

        public static PointF GetCenter(List<PointF> points)
        {
            if (points.Count == 0)
                return PointF.Empty;
            RectangleF boundingRect = Tools.GetBoundingRectangle(points);
            PointF center = new PointF(boundingRect.X + 0.5F * boundingRect.Width,
                                       boundingRect.Y + 0.5F * boundingRect.Height);
            return points.OrderBy(p => Tools.Distance(p, center)).First();
        }

        public static List<PointF> SetCenter(List<PointF> points, out PointF center)
        {
            center = GetCenter(points);
            PointF c = center;
            return points.Select(p => new PointF(p.X - c.X, p.Y - c.Y)).ToList();
        }

        public List<PointF> ClosePoints(List<PointF> points)
        {
            if (points.Count < MinPointsCount)
                return points;
            var path = new List<PointF>();
            var path2 = new List<PointF>();
            path.Add(points[0]);
            path2.Add(points[0]);
            tanhSlope = 0.01 * EndSlope;
            for (int i = 1; i < points.Count; i++)
            {
                PointF prevP = points[i - 1];
                PointF p = points[i];
                if (p == prevP)
                    continue;
                //Get perpendicular vector:
                PointF perp = new PointF(p.Y - prevP.Y, prevP.X - p.X);
                double tanh1 = GetTanh(i);
                double tanh2 = GetTanh(points.Count - i);
                float scale = Math.Max(0.5F, Thickness * (float)(tanh1 * tanh2))
                              / (float)Math.Sqrt(perp.X * perp.X + perp.Y * perp.Y);
                perp = new PointF(scale * perp.X, scale * perp.Y);
                path.Add(new PointF(p.X + perp.X, p.Y + perp.Y));
                path2.Add(new PointF(p.X - perp.X, p.Y - perp.Y));
            }
            path2.Reverse();
            path.AddRange(path2);
            return path;
        }

    }
}
