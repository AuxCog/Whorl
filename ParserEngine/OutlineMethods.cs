using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class OutlineMethods
    {
        public const double HalfPi = 0.5 * Math.PI;

        public static double Sine(double angle, double addDenom)
        {
            return 0.5 * Math.Sin(angle);
        }

        public static double Round(double angle, double addDenom)
        {
            return Math.Abs(Math.Sin(angle));
        }

        public static double Pointed(double angle, double addDenom)
        {
            return addDenom / (addDenom + Math.Abs(Math.Sin(angle)));
        }

        public static double Pointed2(double angle, double addDenom)
        {
            double sine = Math.Sin(angle);
            return 0.5 * Math.Cos(angle) / (sine >= 0D ? sine + addDenom : sine - addDenom);
        }

        public static double Pointed3(double angle, double addDenom)
        {
            double amp = addDenom * Math.Cos(angle) / (addDenom + Math.Abs(Math.Sin(angle)));
            return Math.Sqrt(Math.Abs(amp));
        }

        public static double Pointed4(double angle, double addDenom)
        {
            double amp = addDenom / (addDenom + Math.Abs(Math.Tan(angle)));
            return Math.Pow(amp, 0.5 + amp);
        }

       public static double Pointed5(double angle, double addDenom)
        {
            return addDenom / (addDenom + Math.Abs(Math.Tan(angle)));
        }

        public static double Lobed(double angle, double addDenom)
        {
            double fac = 1D + addDenom;
            return (fac - Math.Abs(Math.Sin(angle))) / fac;
        }

        public static double Ellipse(double angle, double addDenom)
        {
            double b = 1D + 1D / addDenom;
            double sine = Math.Sin(angle);
            double cosine = b * Math.Cos(angle);
            return (b / Math.Sqrt(sine * sine + cosine * cosine)) / Math.Max(b, 1D);
        }

        public static double Broad(double angle, double addDenom)
        {
            return Math.Pow(Math.Max(0D, Math.Sin(2D * angle)), 0.1D / addDenom);
        }

        public static double BroadFull(double angle, double addDenom)
        {
            return Math.Pow(Math.Abs(Math.Sin(angle)), 0.1D / addDenom);
        }

        public static double Rectangle(double angle, double addDenom)
        {
            double ratio = Math.Min(100D, addDenom);
            const double height = 1;
            double width = 1D / ratio;
            double sine = Math.Abs(Math.Sin(angle));
            double cosine = Math.Abs(Math.Cos(angle));
            double amp;
            if (sine <= ratio * cosine)
                amp = width / cosine;
            else
                amp = height / sine;
            return amp / Math.Sqrt(1D + width * width);
        }

        public static double PointedRound(double angle, double addDenom)
        {
            return 0.5 * (Round(angle, addDenom) + Pointed5(angle + HalfPi, addDenom));
        }
    }
}
