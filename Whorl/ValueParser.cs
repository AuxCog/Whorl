using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class ValueParser
    {
        public static bool TryParseDouble(string s, out double val, Func<double, bool> validatePredicate, 
                                          string errorMessage, StringBuilder sbErrors)
        {
            bool isValid = double.TryParse(s ?? string.Empty, out val);
            if (isValid)
                isValid = validatePredicate(val);
            if (!isValid)
                sbErrors.AppendLine(errorMessage);
            return isValid;
        }
    }
}
