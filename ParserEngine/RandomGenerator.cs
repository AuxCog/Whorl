using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class RandomGenerator
    {
        private static int lastSeed;

        public int RandomSeed { get; private set; }
        public Random Random { get; private set; }

        public RandomGenerator(int? seed = null)
        {
            ReseedRandom(seed);
        }

        public static int GetNewSeed()
        {
            int seed = Environment.TickCount;
            if (seed == lastSeed)
                seed++;
            lastSeed = seed;
            return seed;
        }

        public void ReseedRandom(int? seed = null)
        {
            if (seed == null)
            {
                seed = GetNewSeed();
            }
            RandomSeed = (int)seed;
            Random = new Random(RandomSeed);
        }

        public void ResetRandom()
        {
            Random = new Random(RandomSeed);
        }
    }
}
