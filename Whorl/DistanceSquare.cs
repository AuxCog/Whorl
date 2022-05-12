using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class DistanceSquare
    {
        public PointF Center { get; }
        public PointF[] Points { get; }
        public float Distance { get; set; }

        public DistanceSquare(PointF center, PointF[] points)
        {
            Center = center;
            Points = points;
        }

        public static float FindMinDistanceSquared(PointF p, List<DistanceSquare> distanceSquares, out PointF? nearestPoint, int distanceCount = 9)
        {
            float minDist = float.MaxValue;
            nearestPoint = null;
            foreach (DistanceSquare distSquare in distanceSquares)
            {
                distSquare.Distance = Tools.DistanceSquared(p, distSquare.Center);
            }
            foreach (DistanceSquare minSquare in distanceSquares.OrderBy(ds => ds.Distance)
                     .Take(distanceCount))
            {
                int minDistI = -1;
                for (int i = 0; i < minSquare.Points.Length; i++)
                {
                    float dist = Tools.DistanceSquared(minSquare.Points[i], p);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minDistI = i;
                    }
                }
                if (minDistI != -1)
                    nearestPoint = minSquare.Points[minDistI];
            }
            return minDist;
        }

        public static List<DistanceSquare> GetSquares(List<PointF> points, RectangleF boundsRect, int distanceRows,
                                           out SizeF distanceSquareSize, bool offset = false)
        {
            distanceSquareSize = new SizeF((boundsRect.Width + 1) / distanceRows,
                                           (boundsRect.Height + 1) / distanceRows);
            var halfSquareSize = new SizeF(distanceSquareSize.Width / 2F, distanceSquareSize.Height / 2F);
            var distanceSquares = new List<DistanceSquare>();
            PointF pOff = offset ? boundsRect.Location : new PointF(0, 0);
            for (int row = 0; row < distanceRows; row++)
            {
                for (int col = 0; col < distanceRows; col++)
                {
                    var topLeft = new PointF(distanceSquareSize.Width * col + pOff.X, distanceSquareSize.Height * row + pOff.Y);
                    var bottomRight = new PointF(topLeft.X + distanceSquareSize.Width,
                                                 topLeft.Y + distanceSquareSize.Height);
                    var includedPoints = new List<PointF>();
                    for (int i = 0; i < points.Count; i++)
                    {
                        PointF p = points[i];
                        bool xInbounds = p.X >= topLeft.X && p.X < bottomRight.X;
                        bool yInbounds = p.Y >= topLeft.Y && p.Y < bottomRight.Y;
                        bool include = xInbounds && yInbounds;
                        if (row == 0)
                        {
                            if (p.Y < topLeft.Y && xInbounds)
                                include = true;
                        }
                        else if (row == distanceRows - 1)
                        {
                            if (p.Y >= bottomRight.Y && xInbounds)
                                include = true;
                        }
                        if (col == 0)
                        {
                            if (p.X < topLeft.X && yInbounds)
                                include = true;
                        }
                        else if (col == distanceRows - 1)
                        {
                            if (p.X >= bottomRight.X && yInbounds)
                                include = true;
                        }
                        if (include)
                            includedPoints.Add(p);
                    }
                    if (includedPoints.Any())
                    {
                        var center = new PointF(topLeft.X + halfSquareSize.Width,
                                                topLeft.Y + halfSquareSize.Height);
                        distanceSquares.Add(new DistanceSquare(center, includedPoints.ToArray()));
                    }
                }
            }
            return distanceSquares;
        }
    }

}
