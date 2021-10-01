using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    internal class OperatorMethodInfo
    {
        public string OperatorText { get; }
        public string MethodName { get; }
        public bool IsUnary { get; }

        public OperatorMethodInfo(string opText, string methodName, bool isUnary = false)
        {
            OperatorText = opText;
            MethodName = methodName;
            IsUnary = isUnary;
        }

        public static OperatorMethodInfo FindInfo(string opText, bool isUnary)
        {
            return InfoArray.Where(op => op.OperatorText == opText && op.IsUnary == isUnary).FirstOrDefault();
        }

        public static readonly OperatorMethodInfo[] InfoArray = {
            new OperatorMethodInfo("+", "op_UnaryPlus", isUnary: true),
            new OperatorMethodInfo("-", "op_UnaryNegation", isUnary: true),
            new OperatorMethodInfo("+", "op_Addition"),
            new OperatorMethodInfo("-", "op_Subtraction"),
            new OperatorMethodInfo("*", "op_Multiply"),
            new OperatorMethodInfo("/", "op_Division"),
            new OperatorMethodInfo("^", "Pow"),
            new OperatorMethodInfo("==", "op_Equality"),
            new OperatorMethodInfo("!=", "op_Inequality"),
            new OperatorMethodInfo("<", "op_LessThan"),
            new OperatorMethodInfo(">", "op_GreaterThan"),
            new OperatorMethodInfo("<=", "op_LessThanOrEqual"),
            new OperatorMethodInfo(">=", "op_GreaterThanOrEqual"),
            new OperatorMethodInfo("%", "op_Modulus")
        };
    }
}
