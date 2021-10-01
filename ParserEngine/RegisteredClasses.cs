using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParserEngine.ParserTools;

namespace ParserEngine
{
    public struct DoublePoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public DoublePoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public PolarPoint ToPolar()
        {
            return new PolarPoint(angle: Math.Atan2(Y, X), modulus: Math.Sqrt(X * X + Y * Y));
        }

        public PointF ToPointF()
        {
            return new PointF((float)X, (float)Y);
        }

        public double DistanceSquared(DoublePoint p)
        {
            double dX = X - p.X;
            double dY = Y - p.Y;
            return dX * dX + dY * dY;
        }

        public double Distance(DoublePoint p)
        {
            return Math.Sqrt(DistanceSquared(p));
        }

        public static bool TryParse(string text, out DoublePoint doublePoint)
        {
            text = text.Replace("(", string.Empty).Replace(")", string.Empty);
            string[] parts = text.Split(',');
            double x = 0, y = 0;
            bool isValid = parts.Length == 2 &&
                           double.TryParse(parts[0], out x) && 
                           double.TryParse(parts[1], out y);
            if (!isValid)
                x = y = 0;
            doublePoint = new DoublePoint(x, y);
            return isValid;
        }

        public bool IsEqual(DoublePoint dp, double tolerance = 0.01)
        {
            return NumbersEqual(X, dp.X, tolerance) && NumbersEqual(Y, dp.Y, tolerance);
        }

        public bool IntegerEquals(DoublePoint dp)
        {
            return Math.Round(X) == Math.Round(dp.X) && Math.Round(Y) == Math.Round(dp.Y);
        }

        public override string ToString()
        {
            return $"({X:0.##}, {Y:0.##})";
        }
    }

    public struct PolarPoint
    {
        public double Angle { get; set; }
        public double Modulus { get; set; }

        public PolarPoint(double angle, double modulus)
        {
            Angle = angle;
            Modulus = modulus;
        }

        public DoublePoint ToRectangular()
        {
            return new DoublePoint(Modulus * Math.Cos(Angle), Modulus * Math.Sin(Angle));
        }

        public double Distance(PolarPoint pp)
        {
            return ToRectangular().Distance(pp.ToRectangular());
        }

        public PolarPoint AddVector(PolarPoint v2)
        {
            var p1 = ToRectangular();
            var p2 = v2.ToRectangular();
            return new DoublePoint(p1.X + p2.X, p1.Y + p2.Y).ToPolar();
        }

        public PolarPoint SubtractVector(PolarPoint v2)
        {
            var p1 = ToRectangular();
            var p2 = v2.ToRectangular();
            return new DoublePoint(p1.X - p2.X, p1.Y - p2.Y).ToPolar();
        }

        public bool IsEqual(PolarPoint pp, double tolerance = 0.005)
        {
            double aDiff = Normalize(Math.Abs(Angle - pp.Angle), 2.0 * Math.PI);
            if (aDiff > Math.PI)
                aDiff = 2.0 * Math.PI - aDiff;
            return NumbersEqual(aDiff, 0.0, tolerance) && NumbersEqual(Modulus, pp.Modulus, tolerance);
        }

        public override string ToString()
        {
            return $"({RadiansToDegrees(Angle):0.##}, {Modulus:0.##})";
        }

        public static bool TryParse(string text, out PolarPoint polarPoint)
        {
            text = text.Replace("(", string.Empty).Replace(")", string.Empty);
            string[] parts = text.Split(',');
            double angle = 0, modulus = 0;
            bool isValid = parts.Length == 2 &&
                           double.TryParse(parts[0], out angle) &&
                           double.TryParse(parts[1], out modulus);
            if (isValid)
                angle = DegreesToRadians(angle);
            else
                angle = modulus = 0;
            polarPoint = new PolarPoint(angle, modulus);
            return isValid;
        }
    }
}
