using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class ErrorInfo
    {
        public Token Token { get; }
        public string Message { get; }

        public ErrorInfo(Token token, string message)
        {
            Token = token;
            Message = message;
        }
    }
}
