using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class DoubleFuncLibraryExt: DoubleFuncLibrary
    {
        private double pointiness = 1, addDenom = 1;
        public double Pointiness
        {
            get => pointiness;
            protected set
            {
                pointiness = Math.Max(0.0001, value);
                addDenom = 1.0 / pointiness;
            }
        }
        public bool AdjustOutlineX { get; protected set; } = true;
        public bool AdjustOutlineY { get; protected set; } = true;

        public override double AdjustX(double x)
        {
            return AdjustOutlineX ? base.AdjustX(x) : x;
        }

        protected double AdjustY(double y)
        {
            return AdjustOutlineY ? YWeight * y + YOffset : y;
        }

        public double SineOutline(double angle)
        {
            return AdjustY(OutlineMethods.Sine(AdjustX(angle), addDenom));
        }

        public double Round(double angle)
        {
            return AdjustY(OutlineMethods.Round(AdjustX(angle), addDenom));
        }

        public double Pointed(double angle)
        {
            return AdjustY(OutlineMethods.Pointed(AdjustX(angle), addDenom));
        }

        public double Pointed2(double angle)
        {
            return AdjustY(OutlineMethods.Pointed2(AdjustX(angle), addDenom));
        }

        public double Pointed3(double angle)
        {
            return AdjustY(OutlineMethods.Pointed3(AdjustX(angle), addDenom));
        }

        public double Pointed4(double angle)
        {
            return AdjustY(OutlineMethods.Pointed4(AdjustX(angle), addDenom));
        }

        public double Pointed5(double angle)
        {
            return AdjustY(OutlineMethods.Pointed5(AdjustX(angle), addDenom));
        }

        public double PointedRound(double angle)
        {
            return AdjustY(OutlineMethods.PointedRound(AdjustX(angle), addDenom));
        }

        public double Lobed(double angle)
        {
            return AdjustY(OutlineMethods.Lobed(AdjustX(angle), addDenom));
        }

        public double Ellipse(double angle)
        {
            return AdjustY(OutlineMethods.Ellipse(AdjustX(angle), addDenom));
        }

        public double Broad(double angle)
        {
            return AdjustY(OutlineMethods.Broad(AdjustX(angle), addDenom));
        }

        public double BroadFull(double angle)
        {
            return AdjustY(OutlineMethods.BroadFull(AdjustX(angle), addDenom));
        }

        public double Rectangle(double angle)
        {
            return AdjustY(OutlineMethods.Rectangle(AdjustX(angle), addDenom));
        }

    }
}
