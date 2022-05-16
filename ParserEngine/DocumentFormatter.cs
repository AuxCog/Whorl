using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class DocumentFormatter
    {
        public const int tabSpaces = 4;

        public int indentLevel { get; set; }

        public string GetIndent()
        {
            return string.Empty.PadRight(indentLevel * tabSpaces);
        }

        public void AppendLines(StringBuilder sb, IEnumerable<string> lines)
        {
            string indent = GetIndent();
            sb.AppendLine(indent + string.Join(Environment.NewLine + indent, lines));
        }

        public void AppendLine(StringBuilder sb, string line)
        {
            sb.AppendLine(GetIndent() + line);
        }

        public void OpenBrace(StringBuilder sb)
        {
            sb.AppendLine(GetIndent() + "{");
            indentLevel++;
        }

        public void CloseBrace(StringBuilder sb)
        {
            indentLevel--;
            sb.AppendLine(GetIndent() + "}");
        }
    }
}
