using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class ParameterChoice
    {
        public object Value { get; internal set; }
        public string Text { get; internal set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
