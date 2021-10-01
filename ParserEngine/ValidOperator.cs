using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class ValidOperator
    {
        public MethodInfo OperatorMethod { get; private set; }

        public OperatorDefinitions Definition { get; private set; }

        public string TokenText { get; private set; }

        public int Priority { get; private set; }

        public bool IsUnary { get; private set; }

        public Type ReturnType { get; private set; }
        
        public Type Operand1Type { get; private set; }

        public Type Operand2Type { get; private set; }

        public MethodInfo GetCustomMethodInfo(IEnumerable<Type> customTypes, TypeTokenInfo[] operandTypes)
        {
            MethodInfo methodInfo = null;
            OperatorMethodInfo operatorMethodInfo = OperatorMethodInfo.FindInfo(TokenText, IsUnary);
            if (operatorMethodInfo != null)
            {
                foreach (Type type in customTypes)
                {
                    methodInfo = GetMethod.GetMethodInfo(type, operatorMethodInfo.MethodName, operandTypes);
                    if (methodInfo != null)
                        break;
                }
            }
            return methodInfo;
        }

        public ValidOperator(OperatorDefinitions definition, string text, int priority,
                             bool isUnary, Type returnType, Type operand1Type, bool setMethodInfo = true, Type operand2Type = null)
        {
            this.Definition = definition;
            this.TokenText = text;
            this.Priority = priority;
            this.IsUnary = isUnary;
            this.ReturnType = returnType ?? typeof(double);
            this.Operand1Type = operand1Type ?? typeof(double);
            Operand2Type = operand2Type ?? Operand1Type;
            if (setMethodInfo)
            {
                List<TypeTokenInfo> operandTypes = new List<TypeTokenInfo>();
                operandTypes.Add(new TypeTokenInfo() { Type = Operand1Type });
                if (!IsUnary || Definition == OperatorDefinitions.Cast)
                    operandTypes.Add(new TypeTokenInfo() { Type = Operand2Type });
                MethodInfo methodInfo = GetMethod.GetMethodInfo(typeof(OperatorMethods), 
                                        Definition.ToString(), operandTypes.ToArray());
                if (methodInfo == null || methodInfo.ReturnType != ReturnType)
                    throw new Exception($"Operator method for {definition} not found.");
                OperatorMethod = methodInfo;
            }
        }
    }

}
