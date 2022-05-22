using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class DoubleFuncLibrary: IRenderingValues
    {
        public double XWeight { get; protected set; } = 1.0;
        public double XOffset { get; protected set; }
        public double YOffset { get; protected set; }
        public double YWeight { get; protected set; } = 1.0;
        public double XtoInvOff { get; protected set; } = 0.001;
        public bool TakeAbsX { get; protected set; }
        
        [ParameterInfo(IsParameter = false)]
        public RenderingValues RenderingValues { get; set; }

        private Func<double, double> baseFunction { get; set; }

        protected double AdjustX(double x)
        {
            return AdjustX(x, XOffset);
        }

        protected double AdjustY(double y)
        {
            return YWeight * y + YOffset;
        }

        protected double AdjustX(double x, double xOff)
        {
            x = XWeight * x + xOff;
            if (TakeAbsX)
                x = Math.Abs(x);
            return x;
        }

        [ParserEngine.ExcludeMethod]
        public double DefaultFunction(double x)
        {
            return baseFunction == null ? 0 : AdjustY(baseFunction(AdjustX(x)));
        }

        public void SetBaseFunction(Func<double, double> func)
        {
            baseFunction = func;
        }

        public double XtoX(double x)
        {
            return AdjustY(ParserEngine.EvalMethods.XtoX(AdjustX(x)));
        }

        public double XtoInvX(double x)
        {
            x = AdjustX(x);
            return AdjustY(Math.Pow(Math.Abs(x), 1.0 / (x + XtoInvOff * ParserEngine.EvalMethods.Sign2(x))));
        }

    }
}
