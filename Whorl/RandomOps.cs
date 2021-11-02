using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class RandomOps
    {
        public float Weight { get; set; }
        public int XLength { get; set; }
        public int Smoothness { get; set; }
        public bool ClipYValues { get; set; }
        public int? RandomSeed { get; private set; }

        private Random randomGenerator { get; set; }

        public void SetRandomSeed(int seed)
        {
            randomGenerator = new Random(seed);
            RandomSeed = seed;
        }

        public float[] ComputeYValues(out float[] xValues)
        {
            if (randomGenerator == null)
                randomGenerator = new Random();
            if (XLength <= 0)
                throw new Exception("XLength must be positive.");
            if (Smoothness <= 0)
                throw new Exception("Smoothness must be positive.");
            int xCount = XLength / Smoothness;
            float xScale = (float)XLength / xCount;
            float[] xVals = Enumerable.Range(0, xCount).Select(i => xScale * i).ToArray();
            float[] vals = Enumerable.Range(0, xCount).Select(i => (float)randomGenerator.NextDouble() - 0.5F).ToArray();
            float[] xs = Enumerable.Range(0, XLength).Select(i => (float)i).ToArray();
            IEnumerable<float> yVals = CubicSpline.Compute(xVals, vals, xs);
            if (ClipYValues)
            {
                yVals = yVals.Select(y => 0.5F * (float)Math.Tanh(2F * y));
            }
            xValues = xs.ToArray();
            return yVals.Select(y => Weight * y).ToArray();
        }
    }
}
