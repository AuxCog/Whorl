using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public static class EvalMethods
    {
        public const string IdentMethodName = nameof(Ident);

        public static double Sign(double x)
        {
            return Convert.ToDouble(Math.Sign(x));
        }

        public static double Sign2(double x)
        {
            return x >= 0D ? 1D : -1D;
        }

        public static double Ident(double x)
        {
            return x;
        }

        public static double Recip(double x)
        {
            return 1.0 / x;
        }

        public static double Square(double x)
        {
            return x * x;
        }

        public static double Cotan(double x)
        {
            return 1.0 / Math.Tan(x);
        }

        public static double ClippedSine(double x)
        {
            return Math.Max(0.0, Math.Sin(x));
        }

        public static double Sum(double x, double y)
        {
            return x + y;
        }

        public static double Average(double x, double y)
        {
            return 0.5 * (x + y);
        }

        public static double XtoX(double x)
        {
            return Math.Pow(Math.Abs(x), x);
        }

        public static double XtoInvX(double x)
        {
            return Math.Pow(Math.Abs(x), 1.0 / (x + 0.001 * Sign2(x)));
        }

        public static double Difference(double x, double y)
        {
            return x - y;
        }

        public static double Product(double x, double y)
        {
            return x * y;
        }

        public static double Quotient(double x, double y)
        {
            return x / y;
        }

        public static void PolarToRect(double r, double a, out double x, out double y)
        {
            x = r * Math.Cos(a);
            y = r * Math.Sin(a);
        }

        public static void RectToPolar(double x, double y, out double r, out double a)
        {
            r = Math.Sqrt(x * x + y * y);
            a = Math.Atan2(y, x);
        }

        public static void AddVectors(double r1, double a1, double r2, double a2,
                                      out double rOut, out double aOut)
        {
            double x1, y1, x2, y2;
            PolarToRect(r1, a1, out x1, out y1);
            PolarToRect(r2, a2, out x2, out y2);
            RectToPolar(x2 + x1, y2 + y1, out rOut, out aOut);
        }
    }
}
