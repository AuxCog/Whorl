using System;
using System.Drawing;

namespace Whorl
{
    public struct PolarCoord
    {
        public float Angle;
        public float Modulus;

        public PolarCoord(float angle, float modulus)
        {
            Angle = angle;
            Modulus = modulus;
        }

        public PointF ToRectangular()
        {
            return new PointF(Modulus * (float)Math.Cos(Angle), Modulus * (float)Math.Sin(Angle));
        }

        public static PolarCoord ToPolar(PointF p)
        {
            return new PolarCoord((float)Math.Atan2(p.Y, p.X), (float)Math.Sqrt(p.X * p.X + p.Y * p.Y));
        }
    }
 
}