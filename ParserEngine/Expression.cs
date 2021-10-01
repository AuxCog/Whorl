using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ParserEngine.ExpressionParser;

namespace ParserEngine
{
    public class Expression: IDisposable
    {
        public class ExpressionInfo
        {
            public Token[] ExpressionStack { get; }
            public bool IsInitialization { get; }

            public ExpressionInfo(Token[] exprStack, bool isInitialization)
            {
                ExpressionStack = exprStack;
                IsInitialization = isInitialization;
            }
        }

        public const double MaxDoubleValue = 100000000.0;
        internal List<ExpressionInfo> ExpressionStacks { get; set; }
        private Token[] ExpressionStack { get; set; }
        public List<string> ErrorMessages { get; } = new List<string>();
        internal List<ExpressionInfo> InitialExpressionStacks { get; } = new List<ExpressionInfo>();
        internal Dictionary<string, BaseParameter> parameterDict { get; } =
             new Dictionary<string, BaseParameter>(StringComparer.OrdinalIgnoreCase);
        internal HashSet<int> InitExpressionIndexes { get; } = new HashSet<int>();
        public ValidIdentifier IsFirstCallVarIdent { get; internal set; }
        public static int MaxLoopCount { get; set; } = 100000;

        public bool HaveExpressionStacks
        {
            get
            {
                return ExpressionStacks != null && ExpressionStacks.Any();
            }
        }

        private bool TokenHasValidIdentifier(Token token, ValidIdentifier validIdentifier)
        {
            var operandTok = token as BaseOperandToken;
            return operandTok?.ValidIdentifier == validIdentifier;
        }

        public bool ReferencesValidIdentifier(ValidIdentifier validIdentifier)
        {
            return ExpressionStacks.SelectMany(es => es.ExpressionStack).Any(
                   tok => TokenHasValidIdentifier(tok, validIdentifier));
        }

        public IEnumerable<BaseParameter> Parameters
        {
            get { return parameterDict.Values.AsEnumerable(); }
        }

        public BaseParameter GetParameter(string parameterName)
        {
            BaseParameter basePrm;
            if (!parameterDict.TryGetValue(parameterName, out basePrm))
                basePrm = null;
            return basePrm;
        }

        public void InitializeGlobals()
        {
            IsFirstCallVarIdent.CurrentValue = true;
            foreach (var expression in InitialExpressionStacks)
            {
                EvalExpression(expression.ExpressionStack);
            }
        }

        public void EvalStatements()
        {
            if (ExpressionStacks == null)
                throw new Exception("Statements have not been parsed.");
            ErrorMessages.Clear();
            bool breakExecution = false;
            EvalStatements(ExpressionStacks, ref breakExecution, startInd: 0);
            IsFirstCallVarIdent.CurrentValue = false;
        }

        //private int stackIndex;

        //private object FixDouble(object oX)
        //{
        //    if (oX is double)
        //    {
        //        var x = (double)oX;
        //        if (double.IsNaN(x))
        //            oX = 0.0;
        //        else if (Math.Abs(x) > MaxDoubleValue)
        //            oX = Math.Sign(x) * MaxDoubleValue;
        //    }
        //    return oX;
        //}

        private int InitEvalExpr(Token[] exprStack)
        {
            //if (exprStack == null)
            //    throw new Exception("An expression has not been parsed.");
            ExpressionStack = exprStack;
            return ExpressionStack.Length - 1;
        }

        private void EvalStatements(List<ExpressionInfo> expressionStacks,
                                    ref bool breakExecution, int startInd)
        {
            for (int ind = startInd; ind < expressionStacks.Count && !breakExecution; ind++)
            {
                int stackIndex = InitEvalExpr(expressionStacks[ind].ExpressionStack);
                var controlToken = ExpressionStack[stackIndex] as ControlToken;
                if (controlToken != null)
                    EvalControlBlock(controlToken, ref breakExecution, ref stackIndex);
                else
                    EvalExpr(ref stackIndex /* forConstant: false */);
            }
        }

        private bool EvalControlBlock(ControlToken controlToken, ref bool breakExecution, ref int stackIndex)
        {
            Token[] conditionExpr;
            bool condition;
            //Execute control block:
            switch (controlToken.ControlName)
            {
                case ReservedWords.@if:
                    conditionExpr = controlToken.ExpressionStacks[0].ExpressionStack;
                    condition = (bool)EvalExpression(conditionExpr);
                    if (condition)
                        EvalStatements(controlToken.ExpressionStacks,
                                        ref breakExecution,
                                        startInd: 1);
                    else if (controlToken.ElseToken != null)
                        EvalStatements(controlToken.ElseToken.ExpressionStacks,
                                        ref breakExecution,
                                        startInd: 0);
                    break;
                case ReservedWords.@while:
                    conditionExpr = controlToken.ExpressionStacks[0].ExpressionStack;
                    bool breakExecution1 = false;
                    int count = 0;
                    while (!breakExecution1)
                    {
                        condition = (bool)EvalExpression(conditionExpr);
                        if (!condition)
                            break;
                        EvalStatements(controlToken.ExpressionStacks,
                                        ref breakExecution1,
                                        startInd: 1);
                        if (++count > MaxLoopCount)
                            break;
                    }
                    break;
                case ReservedWords.@break:
                    breakExecution = true;
                    break;
                default:
                    throw new Exception(
                        $"Invalid control statement: {controlToken.ControlName}");
            }
            return true;
        }

        public object EvalExpression(Token[] exprStack)
        {
            int stackIndex = InitEvalExpr(exprStack);
            return EvalExpr(ref stackIndex);
        }

        internal object EvalExpression(Token[] exprStack, int topStackIndex) /* , bool forConstant = false */
        {
            ExpressionStack = exprStack;
            int stackIndex = topStackIndex;
            return EvalExpr(ref stackIndex);
        }

        private object EvalExpr(ref int stackIndex)
        {
            object retVal;
            Token topToken = ExpressionStack[stackIndex--];
            OperatorToken opToken = topToken as OperatorToken;
            if (opToken != null)
            {
                if (opToken.OperatorInfo.Definition == OperatorDefinitions.Dot)
                {
                    var memberToken = ExpressionStack[stackIndex--] as FunctionToken;
                    if (memberToken != null)
                    {
                        retVal = EvalMember(ref stackIndex, memberToken);
                    }
                    else
                        throw new Exception("Method token not found for . operator.");
                }
                else
                {
                    object operand2 = EvalExpr(ref stackIndex /* forConstant */);
                    if (opToken.OperatorInfo.Definition == OperatorDefinitions.Assignment)
                    {
                        EvalAssignment(operand2, ref stackIndex);
                        retVal = operand2;
                    }
                    else
                    {
                        object[] arguments;
                        if (opToken.OperatorInfo.IsUnary)
                        {
                            if (opToken.OperatorInfo.Definition == OperatorDefinitions.Cast)
                            {
                                arguments = new object[] { operand2, opToken.ReturnType };
                            }
                            else
                            {
                                arguments = new object[] { operand2 };
                            }
                        }
                        else
                        {
                            object operand1 = EvalExpr(ref stackIndex /* forConstant */);
                            arguments = new object[] { operand1, operand2 };
                        }
                        retVal = GetMethod.EvalMethod(null, opToken.OperatorMethod, arguments);
                        //if (opToken.OperatorMethod.ReturnType == typeof(double))
                        //    retVal = GetMethod.EvalMethod(null, opToken.OperatorMethod, arguments);
                        //else
                        //    retVal = opToken.OperatorMethod.Invoke(null, arguments);
                    }
                }
            }
            else 
            {
                BaseOperandToken operandTok = topToken as BaseOperandToken;
                if (operandTok != null)
                {
                    retVal = EvalOperand(ref stackIndex, operandTok /*, forConstant */);
                }
                else if (topToken.IsLeftParen)
                {
                    retVal = EvalExpr(ref stackIndex /* forConstant */);
                }
                else if (topToken is TypeToken)
                    retVal = null;
                else
                    throw new Exception("Invalid token for evaluating expression.");
            }
            return retVal;
            //return FixDouble(retVal);
        }

        private object EvalOperand(ref int stackIndex, BaseOperandToken operandTok /* , bool forConstant */)
        {
            object retVal = null;
            switch (operandTok.OperandClass)
            {
                case IdentifierClasses.Literal:
                    retVal = ((OperandToken)operandTok).OperandValue;
                    break;
                case IdentifierClasses.Variable:
                case IdentifierClasses.ParameterVariable:
                    //if (forConstant)
                    //    ErrorMessages.Add(
                    //        "A constant's value expression cannot contain variables.");
                    //else
                    //{
                        var validIdent = operandTok.ValidIdentifier;
                        if (validIdent.IsArray)
                        {
                            var arrayVar = (ArrayVariable)operandTok.ValidIdentifier;
                            retVal = GetArrayValue(ref stackIndex, (OperandToken)operandTok, arrayVar.ArrayValue);
                        }
                        else
                        {
                            retVal = validIdent.CurrentValue;
                        }
                    //}
                    break;
                case IdentifierClasses.Function:
                case IdentifierClasses.Member:
                    retVal = EvalMember(ref stackIndex, (FunctionToken)operandTok /*, forConstant */);
                    break;
                case IdentifierClasses.Parameter:
                    //if (forConstant)
                    //    ErrorMessages.Add(
                    //        "A constant's value expression cannot contain parameters.");
                    /*else*/ if (operandTok.BaseParameter.IsArray)
                    {
                        ArrayParameter arrayParam = (ArrayParameter)operandTok.BaseParameter;
                        retVal = GetArrayValue(ref stackIndex, (OperandToken)operandTok, arrayParam.ArrayValue);
                    }
                    else
                    {
                        retVal = operandTok.BaseParameter.UsedValue;
                        if (retVal == null)
                        {
                            ErrorMessages.Add(
                                $"The parameter {operandTok.Text} has no value.");
                            retVal = 0D;
                        }
                    }
                    break;
                default:
                    retVal = ((OperandToken)operandTok).OperandValue;
                    break;
            }
            return retVal;
        }

        private int GetArrayIndex(object value)
        {
            if (!(value is double))
                throw new Exception("Array index was not numeric.");
            return (int)Math.Round((double)value);
        }

        private double GetArrayValue(ref int stackIndex, OperandToken operandTok, ArrayDict arrayDict)
        {
            if (operandTok.OperandModifier == OperandModifiers.ArrayLength)
                return (double)arrayDict.Count;
            else
            {
                //Get array index:
                int index = GetArrayIndex(EvalExpr(ref stackIndex /* forConstant: false */));
                return arrayDict.GetValue(index);
            }
        }

        private void EvalAssignment(object value, ref int stackIndex)
        {
            Token token = ExpressionStack[stackIndex];
            var variableToken = token as OperandToken;
            bool success = true;
            if (variableToken != null)
            {
                stackIndex--;
                var arrayVar = variableToken.ValidIdentifier as ArrayVariable;
                if (arrayVar != null)
                {
                    //Get array index:
                    int index = GetArrayIndex(EvalExpr(ref stackIndex /* forConstant: false */));
                    arrayVar.ArrayValue.SetValue(index, (double)value);
                }
                else
                    variableToken.ValidIdentifier.CurrentValue = value;
            }
            else
            {
                var operatorToken = token as OperatorToken;
                if (operatorToken != null && operatorToken.OperatorInfo.Definition == OperatorDefinitions.Dot)
                {
                    stackIndex--;
                    var memberToken = (FunctionToken)ExpressionStack[stackIndex--];
                    if (memberToken.EvalMethodObject)
                        memberToken.FunctionMethodObject = EvalExpr(ref stackIndex);
                    switch (memberToken.MemberInfo.MemberType)
                    {
                        case MemberTypes.Property:
                            var propInfo = (PropertyInfo)memberToken.MemberInfo;
                            propInfo.SetValue(memberToken.FunctionMethodObject, value);
                            break;
                        case MemberTypes.Field:
                            var fieldInfo = (FieldInfo)memberToken.MemberInfo;
                            fieldInfo.SetValue(memberToken.FunctionMethodObject, value);
                            break;
                        default:
                            success = false;
                            break;
                    }
                }
                else
                    success = false;
            }
            if (!success)
                throw new Exception("Unable to perform assignment.");
        }

        //private void SetOperandValue(OperandToken operandTok, object value)
        //{
        //    //if (operandTok.OperandClass != IdentifierClasses.Variable && 
        //    //    operandTok.OperandClass != IdentifierClasses.ParameterVariable)
        //    //    throw new Exception("Variable expected for assignment.");
        //    var arrayVar = operandTok.ValidIdentifier as ArrayVariable;
        //    if (arrayVar != null)
        //    {
        //        //Get array index:
        //        int index = GetArrayIndex(EvalExpr(/* forConstant: false */));
        //        arrayVar.ArrayValue.SetValue(index, (double)value);
        //    }
        //    else
        //        operandTok.ValidIdentifier.CurrentValue = value;
        //}

        private struct ByRefToken
        {
            public int StackIndex { get; }
            public int Index { get; }
            public OperandToken OperandToken { get; }

            public ByRefToken(int stackIndex, int index, OperandToken operandToken)
            {
                StackIndex = stackIndex;
                Index = index;
                OperandToken = operandToken;
            }
        }

        private object EvalMember(ref int stackIndex, FunctionToken functionToken)
        {
            MethodInfo methodInfo;
            ConstructorInfo constructorInfo;
            switch (functionToken.MemberInfo.MemberType)
            {
                case MemberTypes.Method:
                    methodInfo = (MethodInfo)functionToken.MemberInfo;
                    constructorInfo = null;
                    break;
                case MemberTypes.Field:
                    if (functionToken.EvalMethodObject)
                        functionToken.FunctionMethodObject = EvalExpr(ref stackIndex);
                    FieldInfo fieldInfo = (FieldInfo)functionToken.MemberInfo;
                    return fieldInfo.GetValue(functionToken.FunctionMethodObject);
                case MemberTypes.Property:
                    if (functionToken.EvalMethodObject)
                        functionToken.FunctionMethodObject = EvalExpr(ref stackIndex);
                    PropertyInfo propertyInfo = (PropertyInfo)functionToken.MemberInfo;
                    return propertyInfo.GetValue(functionToken.FunctionMethodObject);
                case MemberTypes.Constructor:
                    constructorInfo = (ConstructorInfo)functionToken.MemberInfo;
                    methodInfo = null;
                    break;
                default:
                    throw new Exception($"MemberType {functionToken.MemberInfo.MemberType} is not implemented.");
            }
            List<object> parameters = new List<object>();
            var byRefParams = new List<ByRefToken>();
            for (int i = 0; i < functionToken.FunctionArgumentCount; i++)
            {
                int saveStackIndex = stackIndex;
                //OperandToken operandTok = ExpressionStack[stackIndex] as OperandToken;
                object paramValue;
                //if (operandTok != null)
                //{
                //    stackIndex--;
                //    //if (operandTok.OperandModifier == OperandModifiers.ParameterObject)
                //    //{
                //    //    paramValue = operandTok.BaseParameter;
                //    //}
                //    //else
                //    //{
                //        paramValue = EvalOperand(operandTok /*, forConstant */);
                //        if (operandTok.OperandModifier == OperandModifiers.RefParameter)
                //        {
                //            byRefParams.Add(new ByRefToken(saveStackIndex, i, operandTok));
                //        }
                //    //}
                //}
                //else
                    paramValue = EvalExpr(ref stackIndex /* forConstant */);
                parameters.Add(paramValue);
                var typedToken = ExpressionStack[saveStackIndex] as TypedToken;
                if (typedToken != null && typedToken.OperandModifier == OperandModifiers.RefParameter)
                {
                    byRefParams.Add(new ByRefToken(saveStackIndex, i, typedToken as OperandToken));
                }
                //if (stackIndex >= 0 && ExpressionStack[stackIndex].TokenType == Token.TokenTypes.Comma)
                //    stackIndex--;
                //else if (i < functionToken.FunctionArgumentCount - 1)
                //{
                //    throw new Exception("Expecting comma separating function arguments.");
                //}
            }
            parameters.Reverse();
            if (functionToken.EvalMethodObject)
                functionToken.FunctionMethodObject = EvalExpr(ref stackIndex);
            else if (functionToken.FunctionCategory == FunctionCategories.Custom && 
                     functionToken.RequiredFunctionParameterCount < 0)
            {
                int minParamCount = -functionToken.RequiredFunctionParameterCount - 1;
                object[] extraArgs = parameters.Skip(minParamCount).ToArray();
                parameters = parameters.Take(minParamCount).ToList();
                parameters.Add(extraArgs);
            }
            object[] paramArray = parameters.ToArray();
            object retVal;
            if (methodInfo != null)
            {
                retVal = GetMethod.EvalMethod(functionToken.FunctionMethodObject, methodInfo, paramArray);
                //if (methodInfo.ReturnType == typeof(double))
                //    retVal = GetMethod.EvalMethod(functionToken.FunctionMethodObject, methodInfo, paramArray);
                //else
                //    retVal = methodInfo.Invoke(functionToken.FunctionMethodObject, paramArray);
            }
            else
            {
                retVal = constructorInfo.Invoke(paramArray);
                //if (retVal is double)
                //    retVal = EvalMethods.FixDouble((double)retVal);
            }
            if (byRefParams.Count != 0)
            {
                for (int i = 0; i < byRefParams.Count; i++)
                {
                    ByRefToken byRefToken = byRefParams[i];
                    object paramVal = paramArray[paramArray.Length - byRefToken.Index - 1];
                    if (byRefToken.OperandToken == null)
                    {
                        int stackInd = byRefToken.StackIndex;
                        EvalAssignment(paramVal, ref stackInd);
                    }
                    else
                    {
                        byRefToken.OperandToken.ValidIdentifier.CurrentValue = paramVal;
                    }
                }
            }
            return retVal;
            //return FixDouble(retVal);
        }

        //private object EvalFunction(FunctionToken functionToken /* , bool forConstant */)
        //{
        //    //ValidIdentifier validIdent = functionToken.ValidIdentifier;
        //    MethodInfo fnMethod = functionToken.MemberInfo as MethodInfo;
        //    if (fnMethod == null)
        //    {
        //        throw new Exception($"No method for function {functionToken.Text}.");
        //    }
        //    //if (functionToken.BaseParameter != null)
        //    //{
        //    //    var fnParm = functionToken.BaseParameter as VarFunctionParameter;
        //    //}
        //    List<object> parameters = new List<object>();
        //    var byRefParams = new List<ByRefToken>();
        //    for (int i = 0; i < functionToken.FunctionArgumentCount; i++)
        //    {
        //        OperandToken operandTok = ExpressionStack[stackIndex] as OperandToken;
        //        object paramValue;
        //        if (operandTok != null)
        //        {
        //            stackIndex--;
        //            if (operandTok.OperandModifier == OperandModifiers.ParameterObject)
        //            {
        //                paramValue = operandTok.BaseParameter;
        //            }
        //            else
        //            {
        //                paramValue = EvalOperand(operandTok /*, forConstant */);
        //                if (operandTok.OperandModifier == OperandModifiers.RefParameter)
        //                {
        //                    byRefParams.Add(new ByRefToken(i, operandTok));
        //                }
        //            }
        //        }
        //        else
        //            paramValue = EvalExpr(/* forConstant */);
        //        parameters.Add(paramValue);
        //        Token topToken = stackIndex >= 0 ? ExpressionStack[stackIndex] : null;
        //        if (topToken != null && topToken.IsComma)
        //            stackIndex--;
        //        //else if (i < functionToken.FunctionArgumentCount - 1)
        //        //{
        //        //    throw new Exception("Expecting comma separating function arguments.");
        //        //}
        //    }
        //    parameters.Reverse();
        //    if (functionToken.FunctionCategory == FunctionCategories.Method)
        //    {
        //        if (!fnMethod.IsStatic)
        //            functionToken.FunctionMethodObject = EvalExpr();
        //    }
        //    else if (functionToken.FunctionCategory == FunctionCategories.Custom && functionToken.RequiredFunctionParameterCount < 0)
        //    {
        //        int minParamCount = -functionToken.RequiredFunctionParameterCount - 1;
        //        object[] extraArgs = parameters.Skip(minParamCount).ToArray();
        //        parameters = parameters.Take(minParamCount).ToList();
        //        parameters.Add(extraArgs);
        //    }
        //    object[] paramArray = parameters.ToArray();
        //    object retVal = fnMethod.Invoke(functionToken.FunctionMethodObject, paramArray);
        //    if (byRefParams.Count != 0)
        //    {
        //        for (int i = 0; i < byRefParams.Count; i++)
        //        {
        //            ByRefToken byRefToken = byRefParams[i];
        //            SetOperandValue(byRefToken.OperandToken, paramArray[paramArray.Length - byRefToken.Index - 1]);
        //        }
        //    }
        //    return FixDouble(retVal);
        //}

        public bool AssignsToVariable(ValidIdentifier ident)
        {
            if (ExpressionStacks == null)
                return false;
            foreach (Token[] exprStack in ExpressionStacks.Select(es => es.ExpressionStack))
            {
                if (exprStack.Length >= 3)
                {
                    var varToken = exprStack[0] as OperandToken;
                    if (varToken != null && varToken.ValidIdentifier == ident)
                    {
                        var opToken = exprStack[exprStack.Length - 1] as OperatorToken;
                        if (opToken != null && opToken.OperatorInfo.Definition == OperatorDefinitions.Assignment)
                            return true;
                    }
                }
            }
            return false;
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed || ExpressionStacks == null)
                return;
            Disposed = true;
            foreach (Token[] exprStack in ExpressionStacks.Select(es => es.ExpressionStack))
            {
                foreach (Token token in exprStack)
                    token.Dispose();
            }
            foreach (BaseParameter baseParameter in Parameters)
            {
                baseParameter.Dispose();
            }
        }
    }
}
