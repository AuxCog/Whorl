using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class DoubleFuncLibrary
    {
        public double XWeight { get; set; } = 1.0;
        public double XOffset { get; set; }
        public double YWeight { get; set; } = 1.0;

        private Func<double, double> baseFunction { get; set; }

        public double DefaultFunction(double x)
        {
            return YWeight * baseFunction(XWeight * x + XOffset);
        }

        public void SetBaseFunction(Func<double, double> func)
        {
            baseFunction = func;
        }
    }
}
