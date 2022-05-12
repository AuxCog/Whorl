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
        public int Row { get; }
        public int Column { get; }
        public float Distance { get; set; }

        public DistanceSquare(PointF center, PointF[] points, int row, int column)
        {
            Center = center;
            Points = points;
            Row = row;
            Column = column;
        }

        public bool IsAdjacent(DistanceSquare square, int count = 1)
        {
            return Math.Abs(square.Row - Row) <= count && Math.Abs(square.Column - Column) <= count;
        }

        public static float FindMinDistanceSquared(PointF p, List<DistanceSquare> distanceSquares, 
                                                   out PointF? nearestPoint, int distanceCount = 9)
        {
            float minDist = float.MaxValue;
            nearestPoint = null;
            foreach (DistanceSquare distSquare in distanceSquares)
            {
                distSquare.Distance = Tools.DistanceSquared(p, distSquare.Center);
            }
            var orderedSquares = distanceSquares.OrderBy(ds => ds.Distance).Take(distanceCount);
            if (orderedSquares.Any())
            {
                var square1 = orderedSquares.First();
                foreach (DistanceSquare square in orderedSquares)
                {
                    if (square == square1 || square.IsAdjacent(square1, count: 2))
                    {
                        for (int i = 0; i < square.Points.Length; i++)
                        {
                            float dist = Tools.DistanceSquared(square.Points[i], p);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                nearestPoint = square.Points[i];
                            }
                        }
                    }
                }
            }
            //foreach (DistanceSquare minSquare in distanceSquares.OrderBy(ds => ds.Distance)
            //         .Take(distanceCount))
            //{
            //    int minDistI = -1;
            //    for (int i = 0; i < minSquare.Points.Length; i++)
            //    {
            //        float dist = Tools.DistanceSquared(minSquare.Points[i], p);
            //        if (dist < minDist)
            //        {
            //            minDist = dist;
            //            minDistI = i;
            //        }
            //    }
            //    if (minDistI != -1)
            //        nearestPoint = minSquare.Points[minDistI];
            //}
            return minDist;
        }

        public static List<DistanceSquare> GetSquares(List<PointF> points, RectangleF boundsRect, int distanceRows,
                                           out SizeF distanceSquareSize, bool offset = false)
        {
            distanceSquareSize = new SizeF((boundsRect.Width + 1) / distanceRows,
                                           (boundsRect.Height + 1) / distanceRows);
            var halfSquareSize = new SizeF(distanceSquareSize.Width / 2F, distanceSquareSize.Height / 2F);
            PointF pOff = offset ? boundsRect.Location : new PointF(0, 0);
            var dict = new Dictionary<int, Dictionary<int, List<PointF>>>();
            float maxVal = (float)distanceRows - 0.001F;
            for (int i = 0; i < points.Count; i++)
            {
                PointF p = points[i];
                if (offset)
                    p = new PointF(p.X - pOff.X, p.Y - pOff.Y);
                float fCol = Math.Min(Math.Max(p.X / distanceSquareSize.Width, 0), maxVal);
                float fRow = Math.Min(Math.Max(p.Y / distanceSquareSize.Height, 0), maxVal);
                int col = (int)fCol;
                int row = (int)fRow;
                if (!dict.TryGetValue(row, out var colDict))
                {
                    colDict = new Dictionary<int, List<PointF>>();
                    dict.Add(row, colDict);
                }
                if (!colDict.TryGetValue(col, out var pointsList))
                {
                    pointsList = new List<PointF>();
                    colDict.Add(col, pointsList);
                }
                pointsList.Add(points[i]);
            }
            var distanceSquares = new List<DistanceSquare>();
            foreach (var rowPair in dict)
            {
                int row = rowPair.Key;
                foreach (var colPair in rowPair.Value)
                {
                    int col = colPair.Key;
                    PointF center = new PointF(col * distanceSquareSize.Width + pOff.X + halfSquareSize.Width,
                                               row * distanceSquareSize.Height + pOff.Y + halfSquareSize.Height);
                    distanceSquares.Add(new DistanceSquare(center, colPair.Value.ToArray(), row, col));
                }
            }
            return distanceSquares;
        }

        //public static List<DistanceSquare> GetSquares(List<PointF> points, RectangleF boundsRect, int distanceRows,
        //                                   out SizeF distanceSquareSize, bool offset = false)
        //{
        //    distanceSquareSize = new SizeF((boundsRect.Width + 1) / distanceRows,
        //                                   (boundsRect.Height + 1) / distanceRows);
        //    var halfSquareSize = new SizeF(distanceSquareSize.Width / 2F, distanceSquareSize.Height / 2F);
        //    var distanceSquares = new List<DistanceSquare>();
        //    PointF pOff = offset ? boundsRect.Location : new PointF(0, 0);
        //    for (int row = 0; row < distanceRows; row++)
        //    {
        //        for (int col = 0; col < distanceRows; col++)
        //        {
        //            var topLeft = new PointF(distanceSquareSize.Width * col + pOff.X, distanceSquareSize.Height * row + pOff.Y);
        //            var bottomRight = new PointF(topLeft.X + distanceSquareSize.Width,
        //                                         topLeft.Y + distanceSquareSize.Height);
        //            var includedPoints = new List<PointF>();
        //            for (int i = 0; i < points.Count; i++)
        //            {
        //                PointF p = points[i];
        //                bool xInbounds = p.X >= topLeft.X && p.X < bottomRight.X;
        //                bool yInbounds = p.Y >= topLeft.Y && p.Y < bottomRight.Y;
        //                bool include = xInbounds && yInbounds;
        //                if (row == 0)
        //                {
        //                    if (p.Y < topLeft.Y && xInbounds)
        //                        include = true;
        //                }
        //                else if (row == distanceRows - 1)
        //                {
        //                    if (p.Y >= bottomRight.Y && xInbounds)
        //                        include = true;
        //                }
        //                if (col == 0)
        //                {
        //                    if (p.X < topLeft.X && yInbounds)
        //                        include = true;
        //                }
        //                else if (col == distanceRows - 1)
        //                {
        //                    if (p.X >= bottomRight.X && yInbounds)
        //                        include = true;
        //                }
        //                if (include)
        //                    includedPoints.Add(p);
        //            }
        //            if (includedPoints.Any())
        //            {
        //                var center = new PointF(topLeft.X + halfSquareSize.Width,
        //                                        topLeft.Y + halfSquareSize.Height);
        //                distanceSquares.Add(new DistanceSquare(center, includedPoints.ToArray(), row, col));
        //            }
        //        }
        //    }
        //    return distanceSquares;
        //}
    }

}
