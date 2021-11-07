using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class ParamInfo
    {
        public string ParameterName { get; }
        public int ArrayIndex { get; }
        public string ParameterKey { get; }

        public ParamInfo(string paramName, int index = -1)
        {
            ParameterName = paramName;
            ArrayIndex = index;
            if (ArrayIndex == -1)
                ParameterKey = ParameterName;
            else
                ParameterKey = $"{ParameterName}[{ArrayIndex + 1}]";
        }

        public override string ToString()
        {
            return ParameterKey;
        }
    }
}
