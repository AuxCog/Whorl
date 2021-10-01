using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class ControlToken: Token
    {
        public ReservedWords ControlName { get; private set; }
        //public int EndStatementIndex { get; set; }
        public ControlToken ElseToken { get; set; }
        public List<Expression.ExpressionInfo> ExpressionStacks { get; } =
           new List<Expression.ExpressionInfo>();

        public ControlToken(Token token, ReservedWords controlName)
            : base(token, token.TokenType)
        {
            this.ControlName = controlName;
        }
    }
}
