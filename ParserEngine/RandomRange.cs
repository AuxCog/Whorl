using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class RandomRange
    {
        public static PropertyInfo[] ParameterProperties { get; }

        private static PropertyInfo GetPropertyInfo(string propName)
        {
            return typeof(RandomRange).GetProperty(propName);
        }

        static RandomRange()
        {
            ParameterProperties = new PropertyInfo[]
            {
                GetPropertyInfo(nameof(AveragingCount)),
                GetPropertyInfo(nameof(KeepCount))
            };
        }

        public int Count { get; private set; }
        public double Maximum { get; }

        //private double _randomStrength = 1;
        //public double RandomStrength
        //{
        //    get { return _randomStrength; }
        //    set
        //    {
        //        if (value > 0D)
        //            _randomStrength = value;
        //    }
        //}

        private int _averagingCount = 5;
        public int AveragingCount
        {
            get { return _averagingCount; }
            set
            {
                if (value > 0)
                    _averagingCount = value;
            }
        }

        private int _keepCount = 10;
        public int KeepCount
        {
            get { return _keepCount; }
            set
            {
                if (value > 0)
                    _keepCount = value;
            }
        }
        public RandomGenerator RandomGenerator { get; set; }

        private double[] values { get; set; }

        private double GetRandomValue()
        {
            return Math.Max(0.0001, RandomGenerator.Random.NextDouble());
        }

        public RandomRange(double maximum = 2 * Math.PI)
        {
            Maximum = maximum;
        }

        public double GetValue(int index)
        {
            return values[index];
        }

        public void Allocate(int count)
        {
            Count = Math.Max(count, 1);
            values = new double[Count];
        }

        public void Compute()
        {
            if (Count <= 1)
                return;
            if (RandomGenerator == null)
                RandomGenerator = new RandomGenerator();
            double curVal = 0;
            var increments = new double[AveragingCount];
            double incrementSum = 0;
            for (int i = 0; i < AveragingCount; i++)
            {
                increments[i] = GetRandomValue();
                incrementSum += increments[i];
            }
            int incI = 0;
            double curInc = 0;
            for (int i = 0; i < Count; i++)
            {
                values[i] = curVal;
                if (i % KeepCount == 0)
                {
                    curInc = incrementSum / AveragingCount;
                    incrementSum -= increments[incI];
                    increments[incI] = GetRandomValue();
                    incrementSum += increments[incI];
                    incI = (incI + 1) % AveragingCount;
                }
                curVal += curInc;
            }
            double scale = Maximum / values[Count - 1];
            for (int i = 0; i < Count; i++)
            {
                values[i] *= scale;
            }
        }
    }
}
