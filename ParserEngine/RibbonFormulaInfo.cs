using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class RibbonFormulaInfo
    {
        [ReadOnly]
        public double StepRatio { get; set; }
        public double RibbonDistance { get; set; }
        public double TaperFactor { get; set; }
        public double RotationOffset { get; set; }
    }
}
