using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class GeneralLibrary : DoubleFuncLibraryExt
    {
        public double SmoothSlope { get; protected set; } = 7.0;
        public double AtanY { get; protected set; } = 1.0;
        public double Power { get; protected set; } = 1.0;

        private double sinePhaseRadians { get; set; }
        private double _sinePhase;
        public double SinePhase
        {
            get => _sinePhase;
            protected set
            {
                _sinePhase = value;
                sinePhaseRadians = Math.PI * _sinePhase / 180.0;
            }
        }

        public double SmoothRound(double x)
        {
            x = XWeight * (x + XOffset);
            if (TakeAbsX)
                x = Math.Abs(x);
            return YWeight * CMath.SmoothRound(x, SmoothSlope) + YOffset;
        }

        public double Sine(double x)
        {
            return YWeight * Math.Sin(AdjustX(x, sinePhaseRadians)) + YOffset;
        }

        public double Cosine(double x)
        {
            return YWeight * Math.Cos(AdjustX(x, sinePhaseRadians)) + YOffset;
        }

        public double Pow(double x)
        {
            return YWeight * CMath.Pow(AdjustX(x), Power) + YOffset;
        }

        public double XMax(double x)
        {
            x = XWeight * x;
            if (TakeAbsX)
                x = Math.Abs(x);
            return YWeight * Math.Max(x, XOffset) + YOffset;
        }

        public double XMin(double x)
        {
            x = XWeight * x;
            if (TakeAbsX)
                x = Math.Abs(x);
            return YWeight * Math.Min(x, XOffset) + YOffset;
        }

        public double AddMax(double x)
        {
            x = XWeight * x;
            if (TakeAbsX)
                x = Math.Abs(x);
            if (x >= XOffset)
                x += YOffset;
            return YWeight * x;
        }

        public double Atan2(double x)
        {
            x = XWeight * (x + XOffset);
            if (TakeAbsX)
                x = Math.Abs(x);
            return YWeight * Math.Atan2(x, AtanY) + YOffset;
        }
    }
}
