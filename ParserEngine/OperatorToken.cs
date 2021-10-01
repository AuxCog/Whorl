using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class OperatorToken : TypedToken
    {
        public ValidOperator _operatorInfo;
        public ValidOperator OperatorInfo
        {
            get { return _operatorInfo; }
            internal set
            {
                if (_operatorInfo != value)
                {
                    _operatorInfo = value;
                    if (_operatorInfo.OperatorMethod != null)
                    {
                        OperatorMethod = _operatorInfo.OperatorMethod;
                        ReturnType = OperatorMethod.ReturnType;
                    }
                }
            }
        }
        public MethodInfo OperatorMethod { get; internal set; }

        public OperatorToken(Token token, ValidOperator validOperator)
            : base(token, TokenTypes.Operator, validOperator.ReturnType)
        {
            this.OperatorInfo = validOperator;
        }
    }
}
