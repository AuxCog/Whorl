using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class TypeTokenInfo
    {
        public Type Type { get; set; }
        public Token Token { get; set; }
        public TypeTokenInfo[] ArgTypes { get; set; }
        public bool IsLValue { get; set; }
        public Type ConversionType { get; set; }
    }
}
