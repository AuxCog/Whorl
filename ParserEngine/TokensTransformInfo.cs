using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class TokensTransformInfo
    {
        public string ParentPropertyName { get; }
        public HashSet<string> TransformedPropertyNames { get; }

        public TokensTransformInfo(string parentPropertyName, IEnumerable<string> transformedPropertyNames)
        {
            ParentPropertyName = parentPropertyName;
            TransformedPropertyNames = new HashSet<string>(
                                       transformedPropertyNames, StringComparer.OrdinalIgnoreCase);
        }
    }
}
