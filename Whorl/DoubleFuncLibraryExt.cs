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

        private double AdjustX1(double x)
        {
            return AdjustOutlineX ? AdjustX(x) : x;
        }

        private double AdjustY1(double y)
        {
            return AdjustOutlineY ? AdjustY(y) : y;
        }

        public double SineOutline(double angle)
        {
            return AdjustY1(OutlineMethods.Sine(AdjustX1(angle), addDenom));
        }

        public double Round(double angle)
        {
            return AdjustY1(OutlineMethods.Round(AdjustX1(angle), addDenom));
        }

        public double Pointed(double angle)
        {
            return AdjustY1(OutlineMethods.Pointed(AdjustX1(angle), addDenom));
        }

        public double Pointed2(double angle)
        {
            return AdjustY1(OutlineMethods.Pointed2(AdjustX1(angle), addDenom));
        }

        public double Pointed3(double angle)
        {
            return AdjustY1(OutlineMethods.Pointed3(AdjustX1(angle), addDenom));
        }

        public double Pointed4(double angle)
        {
            return AdjustY1(OutlineMethods.Pointed4(AdjustX1(angle), addDenom));
        }

        public double Pointed5(double angle)
        {
            return AdjustY1(OutlineMethods.Pointed5(AdjustX1(angle), addDenom));
        }

        public double PointedRound(double angle)
        {
            return AdjustY1(OutlineMethods.PointedRound(AdjustX1(angle), addDenom));
        }

        public double Lobed(double angle)
        {
            return AdjustY1(OutlineMethods.Lobed(AdjustX1(angle), addDenom));
        }

        public double Ellipse(double angle)
        {
            return AdjustY1(OutlineMethods.Ellipse(AdjustX1(angle), addDenom));
        }

        public double Broad(double angle)
        {
            return AdjustY1(OutlineMethods.Broad(AdjustX1(angle), addDenom));
        }

        public double BroadFull(double angle)
        {
            return AdjustY1(OutlineMethods.BroadFull(AdjustX1(angle), addDenom));
        }

        public double Rectangle(double angle)
        {
            return AdjustY1(OutlineMethods.Rectangle(AdjustX1(angle), addDenom));
        }

    }
}
