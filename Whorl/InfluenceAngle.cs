using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParserEngine;

namespace Whorl
{
    public class InfluenceAngle
    {
        public class AngleInfo
        {
            public double X0 { get; set; }
            public double Y0 { get; set; }
            public double M { get; set; }
        }

        public List<AngleInfo> AngleInfos { get; private set; }
        public double ModuloBase { get; private set; }
        public double Phase { get; set; }

        public void Initialize(double modulo, IEnumerable<PointF> points, string skippedPointIdsCsv = null)
        {
            Initialize(modulo, points.Select(p => new DoublePoint(p.X, p.Y)), skippedPointIdsCsv);
        }

        public void Initialize(double modulo, IEnumerable<DoublePoint> points, string skippedPointIdsCsv = null)
        {
            AngleInfos = new List<AngleInfo>();
            if (!points.Any())
                return;
            var skippedIds = new HashSet<int>();
            if (!string.IsNullOrWhiteSpace(skippedPointIdsCsv))
            {
                try
                {
                    skippedIds.UnionWith(skippedPointIdsCsv.Split(',').Select(s => int.Parse(s)));
                }
                catch { }
            }
            ModuloBase = modulo;
            var polarPoints = new List<PolarPoint>();
            int id = 0;
            foreach (DoublePoint point in points)
            {
                ++id;
                if (skippedIds.Contains(id))
                    continue;
                PolarPoint polarPoint = point.ToPolar();
                polarPoint.Angle = Tools.NormalizeAngle(polarPoint.Angle);
                polarPoints.Add(polarPoint);
            }
            polarPoints = polarPoints.OrderBy(p => p.Angle).ToList();
            double targetY = 0;
            AngleInfo angleInfo = null;
            foreach (PolarPoint polarPoint in polarPoints)
            {
                targetY += ModuloBase;
                if (angleInfo != null && polarPoint.Angle == angleInfo.X0)
                    angleInfo.Y0 = targetY;
                else
                {
                    var nextAngleInfo = new AngleInfo() { X0 = polarPoint.Angle, Y0 = targetY };
                    if (angleInfo != null)
                    {
                        angleInfo.M = (nextAngleInfo.Y0 - angleInfo.Y0) / (nextAngleInfo.X0 - angleInfo.X0);
                    }
                    AngleInfos.Add(nextAngleInfo);
                    angleInfo = nextAngleInfo;
                }
            }
            var firstInfo = AngleInfos[0];
            double xDiff = 2 * Math.PI - angleInfo.X0 + firstInfo.X0;
            targetY += ModuloBase;
            angleInfo.M = (targetY - angleInfo.Y0) / xDiff;
        }

        public double ComputeAngle(double a)
        {
            if (AngleInfos == null)
                throw new NullReferenceException("Initialize was not called.");
            if (AngleInfos.Count == 0)
                return a;
            a = Tools.NormalizeAngle(a);
            AngleInfo angleInfo;
            int index = AngleInfos.FindIndex(ai => a < ai.X0);
            if (index == -1 || index == 0)
                angleInfo = AngleInfos.Last();
            else
                angleInfo = AngleInfos[index - 1];
            double xDiff;
            if (index == 0)
                xDiff = (2 * Math.PI - angleInfo.X0 + a);
            else
                xDiff = a - angleInfo.X0;
            return angleInfo.M * xDiff + angleInfo.Y0 + Phase;
        }

    }
}
