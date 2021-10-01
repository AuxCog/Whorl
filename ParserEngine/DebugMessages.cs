using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class DebugMessages
    {
        internal static StringBuilder sbMessages = new StringBuilder();

        public static void ClearMessages()
        {
            sbMessages.Clear();
        }

        public static string GetMessages()
        {
            return sbMessages.ToString();
        }

    }
}
