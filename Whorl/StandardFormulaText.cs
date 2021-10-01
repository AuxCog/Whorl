using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class StandardFormulaText
    {
        public string ClipboardText { get; }

        public StandardFormulaText(string text)
        {
            ClipboardText = text;
        }
    }
}
