using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class CustomEvalMethods
    {
        public static void WriteDebug(string format, params object[] vals)
        {
            string message = vals == null ? format : string.Format(format, vals);
            DebugMessages.sbMessages.AppendLine(message);
        }
    }
}
