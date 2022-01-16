using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class RaggedArrayRow<TValue>
    {
        public int MinIndex { get; }
        public TValue[] Values { get; }

        public RaggedArrayRow(int minIndex, TValue[] values)
        {
            MinIndex = minIndex;
            Values = values;
        }
    }
}
