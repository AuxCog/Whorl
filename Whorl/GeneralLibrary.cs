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
        public double RecipCoeff { get; protected set; } = 0.1;
        public bool SCurveIsMax { get; protected set; } = true;


        [ParameterInfo(IsParameter = false)]
        public double SinePhaseRadians { get; private set; }

        private double _sinePhase;
        public double SinePhase
        {
            get => _sinePhase;
            protected set
            {
                _sinePhase = value;
                SinePhaseRadians = Math.PI * _sinePhase / 180.0;
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
            return YWeight * Math.Sin(AdjustX(x, SinePhaseRadians)) + YOffset;
        }

        public double Cosine(double x)
        {
            return YWeight * Math.Cos(AdjustX(x, SinePhaseRadians)) + YOffset;
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

        public double RecipFunc(double x)
        {
            x = Math.Abs(XWeight * x - XOffset);
            x = RecipCoeff * ((x + 1.0) / (x + RecipCoeff) - 1.0);
            return YWeight * Math.Pow(x, Power) + YOffset;
        }

        public double SCurve(double x)
        {
            x *= XWeight;
            return YWeight * x * Tools.SCurveFactor(x, XOffset, SmoothSlope, SCurveIsMax) + YOffset;
        }
    }
}
