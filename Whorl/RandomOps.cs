using System;
using System.Collections.Generic;
using System.Drawing;
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
        public bool Closed { get; set; }
        public int? RandomSeed { get; private set; }

        private Random randomGenerator { get; set; }

        public void SetRandomSeed(int seed)
        {
            randomGenerator = new Random(seed);
            RandomSeed = seed;
        }

        public float[] ComputeYValues(out float[] xValues, out PointF[] points)
        {
            if (randomGenerator == null)
                randomGenerator = new Random();
            if (XLength <= 0)
                throw new Exception("XLength must be positive.");
            if (Smoothness <= 0)
                throw new Exception("Smoothness must be positive.");
            int xCount = (int)Math.Ceiling((double)XLength / Smoothness);
            if (Closed)
                xCount++;
            int xLength = (xCount - 1) * Smoothness + 1;
            float xScale = (float)Smoothness;
            var xVals = Enumerable.Range(0, xCount).Select(i => xScale * i).ToList();
            var vals = Enumerable.Range(0, xCount).Select(i => (float)randomGenerator.NextDouble() - 0.5F).ToList();
            var xs = Enumerable.Range(0, xLength).Select(i => (float)i).ToList();
            float startSlope, endSlope;
            startSlope = endSlope = float.NaN;
            if (Closed)
            {
                startSlope = endSlope = 0F;
                vals[vals.Count - 1] = vals.First();
            }
            else
                startSlope = endSlope = float.NaN;
            xValues = xs.ToArray();
            points = Enumerable.Range(0, xVals.Count).Select(i => new PointF(xVals[i], Weight * vals[i])).ToArray();
            IEnumerable<float> yVals = CubicSpline.Compute(xVals.ToArray(), vals.ToArray(), xValues, startSlope, endSlope);
            if (ClipYValues)
            {
                yVals = yVals.Select(y => 0.5F * (float)Math.Tanh(2F * y));
            }
            return yVals.Select(y => Weight * y).ToArray();
        }

    }
}
