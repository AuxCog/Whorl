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
            //string message = vals == null ? format : string.Format(format, vals);
            DebugMessages.sbMessages.AppendLine(string.Format(format, vals));
        }

        public static void WriteProperties(object obj, string header = null)
        {
            if (header != null)
                DebugMessages.sbMessages.AppendLine(header);
            if (obj == null)
            {
                DebugMessages.sbMessages.AppendLine("null");
                return;
            }
            foreach (var prp in obj.GetType().GetProperties())
            {
                object val = prp.GetValue(obj);
                DebugMessages.sbMessages.AppendLine($"{prp.Name} = {val}");
            }
        }
        public static bool IsNull(object val)
        {
            return val == null;
        }
    }
}
