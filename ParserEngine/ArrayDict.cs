using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class ArrayDict
    {
        public string VariableName { get; }

        private Dictionary<int, double> valuesDict =
            new Dictionary<int, double>();

        public int Count
        {
            get { return valuesDict.Count; }
        }

        public ArrayDict(string variableName)
        {
            VariableName = variableName;
        }

        public void SetValue(int index, double value)
        {
            valuesDict[index] = value;
        }

        public double GetValue(int index)
        {
            double val;
            if (!valuesDict.TryGetValue(index, out val))
                throw new Exception($"Array {VariableName} has no value for index {index}.");
            return val;
        }

        public void Clear()
        {
            valuesDict.Clear();
        }
    }
}
