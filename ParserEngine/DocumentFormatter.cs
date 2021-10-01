using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class DocumentFormatter
    {
        protected const int tabSpaces = 4;

        public int indentLevel { get; set; }

        protected string GetIndent()
        {
            return string.Empty.PadRight(indentLevel * tabSpaces);
        }

        protected void AppendLines(StringBuilder sb, IEnumerable<string> lines)
        {
            string indent = GetIndent();
            sb.AppendLine(indent + string.Join(Environment.NewLine + indent, lines));
        }

        protected void AppendLine(StringBuilder sb, string line)
        {
            sb.AppendLine(GetIndent() + line);
        }

        protected void OpenBrace(StringBuilder sb)
        {
            sb.AppendLine(GetIndent() + "{");
            indentLevel++;
        }

        protected void CloseBrace(StringBuilder sb)
        {
            indentLevel--;
            sb.AppendLine(GetIndent() + "}");
        }
    }
}
