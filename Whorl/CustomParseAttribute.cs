using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class ParseInfoAttribute: Attribute
    {
        public int CharIndex { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
