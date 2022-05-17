using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class TranslateToCSharp: DocumentFormatter
    {
        private const string ParametersClassName = "EvalParameters";
        private const string ParamsObjName = "Parms";
        private const string InfoPropName = "Info";

        private Type infoType { get; set; }
        private bool forPreprocessor { get; set; }
        //private TokensTransformInfo tokensTransformInfo { get; set; }
        private Expression parsedExpression { get; set; }
        private ExpressionParser parser { get; set; }
        
        private Dictionary<string, int> byRefVarNameDict { get; } =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public string TranslateFormula(Expression expression, ExpressionParser parser, Type infoType,
                                       TokensTransformInfo tokensTransformInfo,
                                       bool forPreprocessor = true)
        {
            parsedExpression = expression;
            this.parser = parser;
            //this.tokensTransformInfo = tokensTransformInfo;
            indentLevel = 0;
            this.infoType = infoType;
            this.forPreprocessor = forPreprocessor;
            StringBuilder sbCode = new StringBuilder();
            sbCode.AppendLine(
@"using ParserEngine;
using Whorl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WhorlEval");
            OpenBrace(sbCode);
            AppendLine(sbCode, "public class WhorlEvalClass");
            OpenBrace(sbCode);

            sbCode.AppendLine(GetParametersClass(expression));

            List<string> propertyDefs = new List<string>();
            propertyDefs.Add("[ParmsProperty]");
            propertyDefs.Add($"public {ParametersClassName} {ParamsObjName}" + " { get; }" + 
                             $" = new {ParametersClassName}();");
            AppendLines(sbCode, propertyDefs);

            sbCode.AppendLine(TranslateDeclarations(parser, out List<string> initializations, isGlobal: true));

            //AppendLine(sbCode, "public WhorlEvalClass()");
            //OpenBrace(sbCode);
            //AppendLine(sbCode, $"{ParamsObjName} = new {ParametersClassName}();");
            //CloseBrace(sbCode);

            AppendLine(sbCode, "public void Initialize()");
            OpenBrace(sbCode);
            AppendLines(sbCode, initializations);
            CloseBrace(sbCode);
            sbCode.AppendLine();

            AppendLine(sbCode, "public void Eval()");
            OpenBrace(sbCode);
            sbCode.Append(TranslateDeclarations(parser, out _, isGlobal: false));
            sbCode.Append(TranslateStatements(expression.ExpressionStacks, topLevel: true));
            //AppendLine(sbCode, "return Position;");
            CloseBrace(sbCode);

            CloseBrace(sbCode);
            CloseBrace(sbCode);
            return sbCode.ToString();
        }

        private string GetParametersClass(Expression expression)
        {
            StringBuilder sbClass = new StringBuilder();
            List<string> propertyDefs = new List<string>();
            List<string> constructorStatements = new List<string>();
            foreach (BaseParameter baseParameter in expression.Parameters)
            {
                string propDef;
                if (forPreprocessor)
                    propDef = TranslateParameterForPreprocessor(baseParameter);
                else
                {
                    propDef = TranslateParameter(baseParameter, out string constructorStmt);
                    if (!string.IsNullOrEmpty(constructorStmt))
                        constructorStatements.Add(constructorStmt);
                }
                propertyDefs.Add(propDef);
            }
            AppendLine(sbClass, $"public class {ParametersClassName}");
            OpenBrace(sbClass);
            if (propertyDefs.Any())
            {
                AppendLines(sbClass, propertyDefs);
                if (constructorStatements.Any())
                {
                    sbClass.AppendLine();
                    AppendLine(sbClass, $"public {ParametersClassName}()");
                    OpenBrace(sbClass);
                    AppendLines(sbClass, constructorStatements);
                    CloseBrace(sbClass);
                }
            }
            CloseBrace(sbClass);
            return sbClass.ToString();
        }

        private string NotImplemented(string code)
        {
            return $"//Code not Implemented: {code}";
        }

        //public Func2Parameter<double> fnOutline {get; private set;}
        //fnOutline = new Func2Parameter<double>("PointedRound", typeof(OutlineMethods));
        private string TranslateParameter(BaseParameter baseParameter, out string constructorCode)
        {
            StringBuilder sb = new StringBuilder();
            constructorCode = null;
            //StringBuilder sbConstructor = new StringBuilder();
            bool notImplemented = false;
            string varName = baseParameter.ParameterName;
            VarFunctionParameter varFunctionParameter = baseParameter as VarFunctionParameter;
            string defaultValue = null;
            if (varFunctionParameter != null)
            {
                string fnTypeName;
                if (varFunctionParameter.ParameterCount == 1)
                    fnTypeName = "Func1Parameter";
                else if (varFunctionParameter.ParameterCount == 2)
                    fnTypeName = "Func2Parameter";
                else
                {
                    fnTypeName = null;
                    notImplemented = true;
                }
                if (fnTypeName != null)
                {
                    List<string> constructorParams = new List<string>();
                    sb.Append($"public {fnTypeName}<double> {varName}" + " { get; }");
                    if (!string.IsNullOrEmpty(varFunctionParameter.DefaultValue))
                    {
                        defaultValue = varFunctionParameter.DefaultValue;
                        constructorParams.Add($"\"{defaultValue}\"");
                    }
                    if (varFunctionParameter.CustomMethodTypes != null)
                    {
                        Type domainType = varFunctionParameter.CustomMethodTypes.FirstOrDefault();
                        if (domainType != null)
                        {
                            constructorParams.Add($"typeof({domainType.Name})");
                        }
                    }
                    sb.Append($" = new {fnTypeName}<double>({string.Join(", ", constructorParams)});");
                    //sbConstructor.Append($"{varName} = new {fnTypeName}<double>({string.Join(", ", constructorParams)});");
                }
            }
            else
            {
                bool handled = false;
                Parameter parameter = baseParameter as Parameter;
                if (parameter != null)
                {
                    if (parameter.DefaultValue != null && parameter.DefaultValue != 0)
                        defaultValue = parameter.DefaultValue.ToString();
                }
                else
                {
                    BooleanParameter booleanParameter = baseParameter as BooleanParameter;
                    if (booleanParameter != null)
                    {
                        if (booleanParameter.DefaultValue)
                            defaultValue = "true";
                    }
                    else
                    {
                        var customParameter = baseParameter as CustomParameter;
                        if (customParameter != null && customParameter.CustomType == CustomParameterTypes.RandomRange)
                        {
                            sb.Append($"public RandomParameter {varName}" + " { get; }");
                            sb.Append(" = new RandomParameter();");
                            //sbConstructor.Append($"{varName} = new RandomParameter();");
                            handled = true;
                        }
                        else
                            notImplemented = true;
                    }
                }
                if (!(handled || notImplemented))
                {
                    sb.Append($"public {baseParameter.ValueType.Name} {varName}" + " { get; set; }");
                }
                if (defaultValue != null)
                {
                    sb.Append($" = {defaultValue};");
                    //sbConstructor.Append($"{varName} = {defaultValue};");
                }
            }
            if (notImplemented)
            {
                sb.Append(NotImplemented($"{baseParameter.GetType().Name} {varName};"));
            }
            //constructorCode = sbConstructor.ToString();
            return sb.ToString();
        }

        private string TranslateParameterForPreprocessor(BaseParameter baseParameter)
        {
            var sb = new StringBuilder();
            sb.Append("@# ");
            bool notImplemented = false;
            string varName = baseParameter.ParameterName;
            sb.Append(varName);
            VarFunctionParameter varFunctionParameter = baseParameter as VarFunctionParameter;
            string defaultValue = null;
            Parameter parameter = null;
            if (varFunctionParameter != null)
            {
                List<string> elems = new List<string>();
                elems.Add(" Function");
                if (varFunctionParameter.ParameterCount == 2)
                    elems.Add("Params = 2");
                if (varFunctionParameter.CustomMethodTypes != null)
                {
                    Type domainType = varFunctionParameter.CustomMethodTypes.FirstOrDefault();
                    if (domainType != null)
                    {
                        elems.Add($"From {domainType.Name}");
                    }
                }
                if (!string.IsNullOrEmpty(varFunctionParameter.DefaultValue))
                {
                    defaultValue = varFunctionParameter.DefaultValue;
                }
                sb.Append(string.Join(" ", elems));
            }
            else
            {
                bool handled = false;
                parameter = baseParameter as Parameter;
                if (parameter != null)
                {
                    if (parameter.DefaultValue != null && parameter.DefaultValue != 0)
                        defaultValue = parameter.DefaultValue.ToString();
                }
                else
                {
                    BooleanParameter booleanParameter = baseParameter as BooleanParameter;
                    if (booleanParameter != null)
                    {
                        if (booleanParameter.DefaultValue)
                            defaultValue = "true";
                    }
                    else
                    {
                        var customParameter = baseParameter as CustomParameter;
                        if (customParameter != null && customParameter.CustomType == CustomParameterTypes.RandomRange)
                        {
                            sb.Append(" Random");
                            handled = true;
                        }
                        else
                            notImplemented = true;
                    }
                }
                if (!(handled || notImplemented))
                {
                    if (baseParameter.ValueType != typeof(double))
                        sb.Append($": {baseParameter.ValueType.Name}");
                }
            }
            if (parameter != null)
            {
                if (parameter.MinValue != null)
                    sb.Append($" Min = {parameter.MinValue}");
                if (parameter.MaxValue != null)
                    sb.Append($" Max = {parameter.MaxValue}");
            }
            if (baseParameter.Label != null)
            {
                sb.Append($" Label = {GetCSharpString(baseParameter.Label)}");
            }
            if (defaultValue != null)
            {
                sb.Append($" = {defaultValue}");
            }
            sb.Append(";");
            if (notImplemented)
            {
                sb.Append(NotImplemented($"{baseParameter.GetType().Name} {varName};"));
            }
            //constructorCode = sbConstructor.ToString();
            return sb.ToString();
        }

        private List<string> extraStatements { get; } = new List<string>();

        private string TranslateStatements(List<Expression.ExpressionInfo> expressionStacks, bool topLevel = false)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < expressionStacks.Count; i++)
            {
                Token[] statement = expressionStacks[i].ExpressionStack;
                Token topToken = statement.LastOrDefault();
                var controlToken = topToken as ControlToken;
                if (controlToken != null)
                {
                    sb.Append(TranslateControlBlock(controlToken));
                }
                else
                {
                    if (expressionStacks[i].IsInitialization)
                    {
                        var varToken = statement.FirstOrDefault() as OperandToken;
                        if (varToken?.ReturnType == null)
                            throw new Exception("Expecting variable token in initialization.");
                        AppendLine(sb, $"{varToken.ReturnType.Name} {TranslateExpression(statement)};");
                    }
                    else
                    {
                        AppendLine(sb, TranslateExpression(statement) + ";");
                    }
                    AppendExtraStatements(sb);
                }
            }
            return sb.ToString();
        }

        private string TranslateControlBlock(ControlToken controlToken)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetIndent() + controlToken.ControlName.ToString());
            switch (controlToken.ControlName)
            {
                case ReservedWords.@if:
                case ReservedWords.@while:
                    sb.AppendLine(
                 $" ({TranslateExpression(controlToken.ExpressionStacks[0].ExpressionStack)})");
                    OpenBrace(sb);
                    AppendExtraStatements(sb);
                    sb.Append(TranslateStatements(controlToken.ExpressionStacks.Skip(1).ToList()));
                    CloseBrace(sb);
                    if (controlToken.ControlName == ReservedWords.@if && controlToken.ElseToken != null)
                    {
                        sb.Append(TranslateControlBlock(controlToken.ElseToken));
                    }
                    break;
                case ReservedWords.@else:
                    Token[] firstStatement = 
                        controlToken.ExpressionStacks.Select(es => es.ExpressionStack).FirstOrDefault();
                    ControlToken ctlTok = firstStatement?.LastOrDefault() as ControlToken;
                    if (ctlTok != null && ctlTok.ControlName == ReservedWords.@break)
                        ctlTok = null;
                    if (ctlTok == null)
                    {
                        sb.AppendLine();
                        OpenBrace(sb);
                    }
                    sb.Append(TranslateStatements(controlToken.ExpressionStacks));
                    if (ctlTok == null)
                        CloseBrace(sb);
                    break;
                case ReservedWords.@break:
                    sb.AppendLine(";");
                    break;
                default:
                    throw new Exception($"Invalid ControlName: {controlToken.ControlName}");
            }
            return sb.ToString();
        }

        private string TranslateExpression(Token[] expression)
        {
            extraStatements.Clear();
            if (expression.Any())
            {
                int stackIndex = expression.Length - 1;
                string code = TranslateExpression(expression, ref stackIndex);
                if (stackIndex >= 0)
                    throw new Exception("Not all expression tokens were consumed.");
                return code;
            }
            else
                return string.Empty;
        }

        public static string GetCSharpString(string s)
        {
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private string TranslateExpression(Token[] expression, ref int stackIndex)
        {
            if (stackIndex < 0)
                throw new Exception("Unexpected end of expression stack.");
            string cSharpCode;
            Token topToken = expression[stackIndex];
            string memberName = null;
            var operandToken = topToken as OperandToken;
            if (operandToken != null)
            {
                if (operandToken.OperandClass == IdentifierClasses.Literal)
                {
                    if (operandToken.OperandValue == null)
                        cSharpCode = "null";
                    else if (operandToken.OperandValue is string)
                        cSharpCode = GetCSharpString(operandToken.OperandValue.ToString());
                    else
                        cSharpCode = operandToken.OperandValue.ToString();
                }
                else
                {
                    var valIdent = operandToken.ValidIdentifier;
                    if (operandToken.BaseParameter != null)
                    {
                        string name = operandToken.BaseParameter.ParameterName;
                        cSharpCode = forPreprocessor ? $"@{name}" : $"{ParamsObjName}.{name}";
                        if (!forPreprocessor && operandToken.BaseParameter is CustomParameter)
                            cSharpCode += ".Value";
                    }
                    else if (valIdent != null)
                    {
                        if (valIdent.TokenClass == IdentifierClasses.Variable && valIdent.IsExternal)
                        {
                            if (valIdent.IdentifierType == infoType)
                                cSharpCode = InfoPropName;
                            else
                                cSharpCode = $"{InfoPropName}.{valIdent.Name}";
                        }
                        else if (valIdent.TokenClass == IdentifierClasses.Constant && valIdent.Name.ToLower() == "pi")
                            cSharpCode = "Math.PI";
                        else
                            cSharpCode = valIdent.Name;
                    }
                    else
                        cSharpCode = operandToken.Text;
                }
                --stackIndex;
                return cSharpCode;
            }
            var functionToken = topToken as FunctionToken;
            if (functionToken != null)
            {
                MethodInfo methodInfo = null;
                var fnParam = functionToken.BaseParameter as VarFunctionParameter;
                bool hasParams = true;
                if (fnParam != null)
                    memberName = forPreprocessor ? $"@{fnParam.ParameterName}" : 
                                 $"{ParamsObjName}.{fnParam.ParameterName}.Function";
                else
                {
                    if (functionToken.MemberInfo != null)
                    {
                        memberName = functionToken.MemberInfo.Name;
                        switch (functionToken.MemberInfo.MemberType)
                        {
                            case System.Reflection.MemberTypes.Field:
                            case System.Reflection.MemberTypes.Property:
                                hasParams = false;
                                break;
                            case MemberTypes.Method:
                                methodInfo = (MethodInfo)functionToken.MemberInfo;
                                if (functionToken.FunctionCategory == FunctionCategories.Normal &&
                                    methodInfo.IsStatic)
                                {
                                    memberName = $"{methodInfo.DeclaringType.Name}.{memberName}";
                                }
                                break;
                            case System.Reflection.MemberTypes.Constructor:
                                var cInfo = (ConstructorInfo)functionToken.MemberInfo;
                                memberName = "new " + cInfo.ReflectedType.Name;
                                break;
                        }
                    }
                    else
                        memberName = functionToken.Text;
                    //if (functionToken.FunctionCategory == FunctionCategories.Normal)
                    //{
                        
                    //    if (typeof(CMath).GetMethod(functionName) != null)
                    //        functionName = "CMath." + functionName;
                    //    else if (typeof(EvalMethods).GetMethod(functionName) != null)
                    //        functionName = $"{nameof(EvalMethods)}.{functionName}";
                    //    else
                    //        functionName = "Math." + functionName;
                    //}
                }
                --stackIndex;
                if (hasParams)
                {
                    var paramInfos = methodInfo?.GetParameters();
                    if (paramInfos != null && paramInfos.Length != functionToken.FunctionArgumentCount)
                        paramInfos = null;
                    List<string> paramsCode = new List<string>();
                    for (int i = 0; i < functionToken.FunctionArgumentCount; i++)
                    {
                        int saveStackIndex = stackIndex;
                        string exprCode = TranslateExpression(expression, ref stackIndex);
                        string paramCode = exprCode;
                        if (paramInfos != null)
                        {   
                            ParameterInfo paramInfo = paramInfos[paramInfos.Length - i - 1];
                            if (paramInfo.ParameterType.IsByRef)
                            {
                                //out or ref parameter.
                                if (paramInfo.IsOut && expression[saveStackIndex].Text == ".")
                                {
                                    //out parameter, and need to change property to variable.
                                    var memberTok = expression[saveStackIndex - 1] as FunctionToken;
                                    if (memberTok?.MemberInfo != null)
                                    {
                                        string varName;
                                        if (byRefVarNameDict.TryGetValue(memberTok.MemberInfo.Name,
                                            out int suffix))
                                        {
                                            byRefVarNameDict[memberTok.MemberInfo.Name] = ++suffix;
                                            varName = $"{memberTok.MemberInfo.Name}{suffix}";
                                        }
                                        else
                                        {
                                            varName = memberTok.MemberInfo.Name;
                                            suffix = 0;
                                            while (parser.GetValidIdentifier(varName) != null)
                                            {
                                                suffix++;
                                                varName = $"{memberTok.MemberInfo.Name}{suffix}";
                                            }
                                            byRefVarNameDict.Add(memberTok.MemberInfo.Name, suffix);
                                        }
                                        paramCode = $"var {varName}";  //Will be 'out var varName'
                                        string assignmentStatement = $"{exprCode} = {varName};";
                                        extraStatements.Add(assignmentStatement);
                                    }
                                }
                                string refOrOut = paramInfo.IsOut ? "out" : "ref";
                                paramCode = $"{refOrOut} {paramCode}";
                            }
                        }
                        paramsCode.Add(paramCode);
                    }
                    paramsCode.Reverse();
                    //if (paramInfos != null)
                    //{
                    //    for (int i = 0; i < paramInfos.Length; i++)
                    //    {
                    //        ParameterInfo paramInfo = paramInfos[i];
                    //        if (paramInfo.ParameterType.IsByRef)
                    //        {
                    //            string refOrOut = paramInfo.IsOut ? "out" : "ref";
                    //            paramsCode[i] = $"{refOrOut} {paramsCode[i]}";
                    //        }
                    //    }
                    //}
                    cSharpCode = $"{memberName}({string.Join(", ", paramsCode)})";
                }
                else
                    cSharpCode = memberName;
                return cSharpCode;
            }
            var operatorToken = topToken as OperatorToken;
            if (operatorToken != null)
            {
                --stackIndex;
                string rightExpr;
                rightExpr = TranslateExpression(expression, ref stackIndex);
                if (operatorToken.OperatorInfo.IsUnary)
                {
                    if (operatorToken.OperatorInfo.Definition == OperatorDefinitions.Not)
                        cSharpCode = $"!{rightExpr}";
                    else
                        cSharpCode = $"{operatorToken.Text}{rightExpr}";
                }
                else
                {
                    string leftExpr = TranslateExpression(expression, ref stackIndex);
                    switch (operatorToken.OperatorInfo.Definition)
                    {
                        case OperatorDefinitions.Exponentiation:
                            cSharpCode = $"CMath.Pow({leftExpr}, {rightExpr})";
                            break;
                        case OperatorDefinitions.Dot:
                            cSharpCode = $"{leftExpr}{operatorToken.Text}{rightExpr}";
                            break;
                        case OperatorDefinitions.And:
                            cSharpCode = $"{leftExpr} && {rightExpr}";
                            break;
                        case OperatorDefinitions.Or:
                            cSharpCode = $"{leftExpr} || {rightExpr}";
                            break;
                        default:
                            cSharpCode = $"{leftExpr} {operatorToken.Text} {rightExpr}";
                            break;
                    }
                }
                return cSharpCode;
            }
            if (topToken.TokenType == Token.TokenTypes.LeftParen)
            {
                --stackIndex;
                cSharpCode = "(" + TranslateExpression(expression, ref stackIndex) + ")";
            }
            else
            {
                cSharpCode = topToken.Text;
                --stackIndex;
            }
            return cSharpCode;
        }

        private string TranslateDeclarations(ExpressionParser parser, out List<string> initializations, bool isGlobal)
        {
            var sb = new StringBuilder();
            initializations = new List<string>();
            if (isGlobal)
            {
                string infoTypeText = infoType.FullName.Replace('+', '.');
                AppendLine(sb, $"public {infoTypeText} {InfoPropName} " + "{ get; set; }");
            }
            //bool declaredInfoProperty = false;
            foreach (ValidIdentifier valIdent in parser.GetBaseIdentifiers()
                    .OrderBy(id => id.SourceToken == null ? 0 : id.SourceToken.CharIndex))
            {
                string statement = isGlobal ? "public " : string.Empty;
                switch (valIdent.TokenClass)
                {
                    case IdentifierClasses.Variable:
                        if (valIdent.IsGlobal != isGlobal)
                            continue;
                        if (isGlobal)
                        {
                            if (valIdent.IdentifierType == infoType)
                                continue;  //Already declared above.
                        }
                        else if (valIdent.InitializationExpression != null)
                            continue;      //Appended in TranslateStatements.
                        break;
                    case IdentifierClasses.ParameterVariable:
                        if (!isGlobal)
                            continue;
                        statement = "private ";
                        break;
                    case IdentifierClasses.Constant:
                        statement = $"{statement}const ";
                        break;
                    //case IdentifierClasses.Function:
                    //case IdentifierClasses.Literal:
                    //case IdentifierClasses.Member:
                    //case IdentifierClasses.Parameter:
                    //case IdentifierClasses.ParameterObject:
                    //case IdentifierClasses.ReservedWord:
                    //case IdentifierClasses.Type:
                    default:
                        continue;
                }
                string initialization;
                if (valIdent.InitializationExpression != null)
                    initialization = TranslateExpression(valIdent.InitializationExpression);
                else
                    initialization = null;
                statement += valIdent.IdentifierType.Name + " ";
                if (valIdent.TokenClass == IdentifierClasses.Constant)
                {
                    statement += $"{valIdent.Name} = {initialization};";
                }
                else if (isGlobal)
                {
                    statement += valIdent.Name + " { get; set; }";
                    if (initialization != null)
                        initializations.Add($"{initialization};");
                }
                else
                {
                    statement += valIdent.Name + ";";
                }
                AppendLine(sb, statement);
                AppendExtraStatements(sb);
            }
            return sb.ToString();
        }

        private void AppendExtraStatements(StringBuilder sb)
        {
            if (extraStatements.Any())
            {
                AppendLines(sb, extraStatements);
                extraStatements.Clear();
            }
        }
    }
}
