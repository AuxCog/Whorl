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
        public List<PathPointInfo> PointInfos { get; }
        //public int Row { get; }
        //public int Column { get; }
        public float Distance { get; set; }

        public DistanceSquare(PointF center, List<PathPointInfo> pointInfos)
        {
            Center = center;
            PointInfos = pointInfos;
            //Row = row;
            //Column = column;
        }

        public static float FindMinDistanceSquared(PointF p, List<DistanceSquare> distanceSquares, 
                                                   out PathPointInfo? nearestPointInfo, int distanceCount = 9)
        {
            float minDist = float.MaxValue;
            nearestPointInfo = null;
            foreach (DistanceSquare distSquare in distanceSquares)
            {
                distSquare.Distance = Tools.DistanceSquared(p, distSquare.Center);
            }
            var orderedSquares = distanceSquares.OrderBy(ds => ds.Distance).Take(distanceCount);
            foreach (DistanceSquare square in orderedSquares)
            {
                for (int i = 0; i < square.PointInfos.Count; i++)
                {
                    float dist = Tools.DistanceSquared(square.PointInfos[i].Point, p);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestPointInfo = square.PointInfos[i];
                    }
                }
            }
            return minDist;
        }

        public static List<DistanceSquare> GetSquares(List<PointF> points, RectangleF boundsRect, int distanceRows,
                                           out SizeF distanceSquareSize, bool computePathLength, bool offset = false)
        {
            distanceSquareSize = new SizeF((boundsRect.Width + 1) / distanceRows,
                                           (boundsRect.Height + 1) / distanceRows);
            var halfSquareSize = new SizeF(distanceSquareSize.Width / 2F, distanceSquareSize.Height / 2F);
            PointF pOff = offset ? boundsRect.Location : new PointF(0, 0);
            var dict = new Dictionary<int, Dictionary<int, List<PathPointInfo>>>();
            float maxVal = (float)distanceRows - 0.001F;
            float pathLength = 0;
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
                    colDict = new Dictionary<int, List<PathPointInfo>>();
                    dict.Add(row, colDict);
                }
                if (!colDict.TryGetValue(col, out var pointsList))
                {
                    pointsList = new List<PathPointInfo>();
                    colDict.Add(col, pointsList);
                }
                if (computePathLength && i > 0)
                {
                    pathLength += (float)Tools.Distance(points[i], points[i - 1]);
                }
                pointsList.Add(new PathPointInfo(points[i], pathLength));
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
                    distanceSquares.Add(new DistanceSquare(center, colPair.Value));
                }
            }
            return distanceSquares;
        }

    }

}
