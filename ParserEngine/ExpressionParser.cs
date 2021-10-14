using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ParserEngine
{
    public enum IdentifierClasses
    {
        Literal,
        Variable,
        ParameterVariable,
        Parameter,
        ParameterObject,
        Function,
        Member,
        Constant,
        ReservedWord,
        Type
    }

    public enum ReservedWords
    {
        @const,
        @var,
        //boolean,
        //complex,
        @ref,
        @if,
        @else,
        @while,
        @break,
        parameters,
        parameter,
        length,
        array,
        paramvar,
        global,
        @new
    }

    public enum OutputParameterTypes
    {
        @double,
        boolean
    }

    public class ExpressionParser
    {
        public const char CommentChar = '#';

        /// <summary>
        /// These types are used by default for finding static methods, if a type is not specified.
        /// </summary>
        public static readonly Type[] StaticMethodsTypes = { typeof(EvalMethods), typeof(Math) };
        public bool OptimizeExpressions { get; set; }
        public bool RequireGetRandomValue { get; set; }
        private List<Token> exprStackList;
        private Token[] expressionStack;
        internal static Trie TokenTrie { get; } = new Trie();
        internal static Dictionary<string, List<ValidOperator>> validOperatorDict { get; }
        private static Dictionary<string, ValidIdentifier> staticIdentifierDict { get; }
        private Dictionary<string, ValidIdentifier> baseIdentifierDict =
            new Dictionary<string, ValidIdentifier>(StringComparer.OrdinalIgnoreCase);
        private static Tokenizer tokenizer;
        /// <summary>
        /// Types available for custom operators.
        /// </summary>
        public static readonly Type[] CustomTypes =
        {
            typeof(Complex)
        };

        public struct RegType
        {
            public Type Type { get; }
            public string Name { get; }
            public bool AllowedInDeclarations { get; }

            public RegType(Type type, string name = null, bool allowedInDeclarations = true)
            {
                Type = type;
                Name = name;
                AllowedInDeclarations = allowedInDeclarations;
            }

            public override string ToString()
            {
                return Name ?? (Type == null ? string.Empty : Type.Name);
            }
        }

        private static List<RegType> RegisteredTypes { get; } = new List<RegType>()
        {
            new RegType(typeof(bool)),
            new RegType(typeof(Complex)),
            new RegType(typeof(double)),
            new RegType(typeof(string)),
            new RegType(typeof(int), name: "int"),
            new RegType(typeof(float), name: "float"),
            new RegType(typeof(DateTime)),
            new RegType(typeof(System.Drawing.Point)),
            new RegType(typeof(DoublePoint), name: "RealPoint"),
            new RegType(typeof(PolarPoint)),
            new RegType(typeof(RibbonFormulaInfo)),
            new RegType(typeof(Math)),
            new RegType(typeof(OutlineMethods), allowedInDeclarations: false)
        };

        public static IEnumerable<RegType> GetRegisteredTypes(bool returnAll = false)
        {
            return RegisteredTypes.Where(rt => rt.AllowedInDeclarations || returnAll);
        }

        private Dictionary<string, BaseParameter> parameterDict
        {
            get { return Expression.parameterDict; }
        }

        private int tokenIndex;

        public Expression Expression { get; private set; }

        public ValidIdentifier IsFirstCallVarIdent { get; }

        private List<Token> tokens { get; set; }

        public static bool IgnoreCase { get; set; } = true;

        private List<ErrorInfo> ErrorInfoList { get; set; }

        public IEnumerable<ErrorInfo> GetErrorInfoList()
        {
            if (ErrorInfoList == null)
                return new ErrorInfo[] { };
            else
                return ErrorInfoList;
        }

        public IEnumerable<string> GetErrorMessages()
        {
            return GetErrorInfoList().Select(ei => ei.Message);
        }

        public void AddErrorMessage(string message, Token errorToken = null)
        {
            if (errorToken == null)
            {
                errorToken = GetToken(tokenIndex);
                if (errorToken == null)
                {
                    errorToken = tokens.LastOrDefault();
                    if (errorToken == null)
                        errorToken = new Token(string.Empty, Token.TokenTypes.Identifier, 0);
                }
            }
            ErrorInfoList.Add(new ErrorInfo(errorToken, message));
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static ExpressionParser()
        {
            try
            {
                validOperatorDict = new Dictionary<string, List<ValidOperator>>(StringComparer.OrdinalIgnoreCase);
                staticIdentifierDict = new Dictionary<string, ValidIdentifier>(StringComparer.OrdinalIgnoreCase);
                StandardInitialize();
                PopulateTokenTrie();
                PopulateValidParamAttrDict();
                tokenizer = new Tokenizer(TokenTrie, forCSharp: false);
                tokenizer.AlphaOperatorsHashSet.UnionWith(validOperatorDict.Keys.Where(k => char.IsLetter(k[0])));
                //tokenizer.Initialize(TokenTrie);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex);
            }
        }

        public ExpressionParser()
        {
            IsFirstCallVarIdent = DeclareVariable("IsFirstCall", typeof(bool), false, isGlobal: true);
        }

        private static void PopulateTokenTrie()
        {
            Token[] punctTokens = new Token[] {
                    //new Token("(", Token.TokenTypes.LeftParen, 0),
                    new Token(")", Token.TokenTypes.RightParen, 0),
                    new Token(",", Token.TokenTypes.Comma, 0),
                    new Token(";", Token.TokenTypes.Semicolon, 0),
                    new Token("{", Token.TokenTypes.LeftBrace, 0),
                    new Token("}", Token.TokenTypes.RightBrace, 0),
                    new Token("[", Token.TokenTypes.LeftBracket, 0),
                    new Token("]", Token.TokenTypes.RightBracket, 0)
                    };
            foreach (string opToken in validOperatorDict.Keys)
            {
                if (!char.IsLetter(opToken[0]))
                    TokenTrie.AddToken(opToken, Token.TokenTypes.Operator);
            }
            foreach (Token tok in punctTokens)
            {
                TokenTrie.AddToken(tok.Text, tok.TokenType);
            }
        }

        public static IEnumerable<string> GetOperators()
        {
            return validOperatorDict.Keys.OrderBy(s => s);
        }

        private static string GetIdentifierText(ValidIdentifier ident)
        {
            string text = ident.Name;
            if (ident.TokenClass == IdentifierClasses.Function)
            {
                var valIdent = ident as ValidIdentifier;
                if (valIdent != null)
                {
                    text += "(" + String.Concat(Enumerable.Repeat(", ", Math.Max(0, valIdent.ParameterCount - 1))) + ")";
                }
            }
            return text;
        }

        private static IEnumerable<string> GetDictionaryTokens(Dictionary<string, ValidIdentifier> dict)
        {
            return dict.Values.OrderBy(id => id.TokenClass).ThenBy(id => id.Name).Select(id => GetIdentifierText(id));
        }

        public static IEnumerable<string> GetStaticTokens()
        {
            return GetDictionaryTokens(staticIdentifierDict);
        }

        public IEnumerable<string> GetIdentifierTokens()
        {
            return GetDictionaryTokens(baseIdentifierDict);
        }

        public IEnumerable<ValidIdentifier> GetBaseIdentifiers()
        {
            return baseIdentifierDict.Values;
        }

        private static void StandardInitialize()
        {
            AddValidOperator(OperatorDefinitions.Addition, "+", 10, forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.Addition, "+", 10, returnType: typeof(string), operandsType: typeof(string));
            AddValidOperator(OperatorDefinitions.Subtraction, "-", 10, forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.Multiplication, "*", 20, forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.Division, "/", 20, forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.Modulo, "%", 20, forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.Exponentiation, "^", 30, forNumericTypes: false);
            AddValidOperator(OperatorDefinitions.UnaryMinus, "-", 40, isUnary: true, forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.Not, "not", 40, isUnary: true,
                             returnType: typeof(bool), operandsType: typeof(bool));
            AddValidOperator(OperatorDefinitions.IsEqual, "==", 8,
                             returnType: typeof(bool), operandsType: typeof(object));
            AddValidOperator(OperatorDefinitions.NotEqual, "!=", 8,
                             returnType: typeof(bool), operandsType: typeof(object));
            AddValidOperator(OperatorDefinitions.LT, "<", 8,
                             returnType: typeof(bool), forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.LTE, "<=", 8,
                             returnType: typeof(bool), forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.GT, ">", 8,
                             returnType: typeof(bool), forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.GTE, ">=", 8,
                             returnType: typeof(bool), forNumericTypes: true);
            AddValidOperator(OperatorDefinitions.And, "and", 6,
                             returnType: typeof(bool), operandsType: typeof(bool));
            AddValidOperator(OperatorDefinitions.Or, "or", 4,
                             returnType: typeof(bool), operandsType: typeof(bool));
            AddValidOperator(OperatorDefinitions.Assignment, "=", 2,
                             returnType: typeof(object), operandsType: typeof(object), setMethod: false);
            AddValidOperator(OperatorDefinitions.Dot, ".", 50, setMethod: false);
            AddValidOperator(OperatorDefinitions.Cast, "(", 49, isUnary: true, returnType: typeof(object),
                             operandsType: typeof(object), operand2Type: typeof(Type));
            foreach (ReservedWords wd in Enum.GetValues(typeof(ReservedWords)))
            {
                DeclareReservedWord(wd);
            }
            foreach (RegType regType in RegisteredTypes)
            {
                DeclareType(regType.Type, regType.Name, regType.AllowedInDeclarations);
            }
            DeclareConstant(staticIdentifierDict, "pi", typeof(double), Math.PI);
            //DeclareConstant(staticIdentifierDict, "e", typeof(double), Math.E);
            DeclareConstant(staticIdentifierDict, "false", typeof(bool), false);
            DeclareConstant(staticIdentifierDict, "true", typeof(bool), true);
            //foreach (MethodInfo fnMethod in typeof(Math).GetMethods(
            //            BindingFlags.Static | BindingFlags.Public).OrderBy(mi => mi.GetParameters().Length))
            //{
            //    bool isValid = fnMethod.ReturnType == typeof(double);
            //    if (isValid)
            //    {
            //        foreach (var prmInfo in fnMethod.GetParameters())
            //        {
            //            if (prmInfo.ParameterType != typeof(double))
            //                isValid = false;
            //        }
            //    }
            //    if (!isValid)
            //        continue;
            //    if (!staticIdentifierDict.ContainsKey(fnMethod.Name))
            //        DeclareFunction(fnMethod.Name, fnMethod.ReturnType, fnMethod);
            //}
            //foreach (MethodInfo fnMeth in typeof(EvalMethods).GetMethods(
            //                            BindingFlags.Public | BindingFlags.Static))
            //{
            //    DeclareFunction(fnMeth.Name, fnMeth.ReturnType, fnMeth);
            //}
            foreach (MethodInfo customMeth in typeof(CustomEvalMethods).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                DeclareFunction(customMeth.Name, customMeth.ReturnType,
                                customMeth, functionCategory: FunctionCategories.Custom);
            }
        }

        private static readonly Type[] numericTypes =
        {
            typeof(double),
            typeof(float),
            typeof(int)
        };

        private static void AddValidOperator(OperatorDefinitions definition, string text,
                                            int priority, bool isUnary = false,
                                            Type returnType = null, Type operandsType = null,
                                            bool setMethod = true, Type operand2Type = null,
                                            bool forNumericTypes = false)
        {
            //if (IgnoreCase)
            //    text = text.ToLower();
            List<ValidOperator> validOperatorList;
            if (!validOperatorDict.TryGetValue(text, out validOperatorList))
            {
                validOperatorList = new List<ValidOperator>();
                validOperatorDict.Add(text, validOperatorList);
            }
            if (forNumericTypes)
            {
                foreach (Type numericType in numericTypes)
                {
                    var validOperator = new ValidOperator(definition, text, priority, isUnary, returnType ?? numericType, 
                                                          numericType, setMethod, numericType);
                    validOperatorList.Add(validOperator);
                }
            }
            else
            {
                var validOperator = new ValidOperator(definition, text, priority, isUnary, returnType, 
                                                      operandsType, setMethod, operand2Type);
                validOperatorList.Add(validOperator);
            }
        }

        private static void DeclareReservedWord(ReservedWords reservedWord)
        {
            Type type = null;
            //switch (reservedWord)
            //{
            //    case ReservedWords.boolean:
            //        type = typeof(bool);
            //        break;
            //    case ReservedWords.complex:
            //        type = typeof(Complex);
            //        break;
            //    default:
            //        type = null;
            //        break;
            //}
            DeclareIdentifier(staticIdentifierDict, reservedWord.ToString(), type, IdentifierClasses.ReservedWord);
        }

        private static void DeclareType(Type type, string name = null, bool allowedInDeclarations = true)
        {
            DeclareIdentifier(staticIdentifierDict, name ?? type.Name, type, IdentifierClasses.Type,
                              typeIsAllowedInDeclarations: allowedInDeclarations);
        }

        public static bool DeclareExternType(Type type, string name = null)
        {
            if (RegisteredTypes.Exists(rt => rt.Type == type))
                return false;
            RegisteredTypes.Add(new RegType(type, name));
            DeclareType(type, name);
            return true;
        }

        public static ValidIdentifier DeclareConstant(Dictionary<string, ValidIdentifier> idDict,
                    string name, Type valueType, object value, bool isGlobal = false)
        {
            return DeclareIdentifier(idDict, name, valueType, IdentifierClasses.Constant,
                              initialValue: value, isGlobal: isGlobal);
        }

        private static int GetParameterCount(MethodInfo methodInfo)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            ParameterInfo paramsParameter = GetMethod.GetParamsParameter(parameters);
            int parameterCount;
            if (paramsParameter == null)
                parameterCount = parameters.Length;
            else
                parameterCount = -parameters.Length;
            return parameterCount;
        }

        public static void DeclareFunction(string name, Type returnType, MethodInfo methodInfo, object methodObject = null, 
                                           FunctionCategories functionCategory = FunctionCategories.Normal)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo", "methodInfo cannot be null.");
            DeclareIdentifier(staticIdentifierDict, name, returnType,
                IdentifierClasses.Function, GetParameterCount(methodInfo), null, methodObject, methodInfo, 
                functionCategory: functionCategory);
        }

        public void DeclareInstanceFunction(object methodsObject, MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo", "methodInfo cannot be null.");
            DeclareIdentifier(baseIdentifierDict, methodInfo.Name, methodInfo.ReturnType,
                IdentifierClasses.Function, GetParameterCount(methodInfo),
                null, methodsObject, methodInfo, isGlobal: true);
        }

        public void UpdateInstanceFunction(string functionName, object methodsObject)
        {
            ValidIdentifier fnId;
            //if (IgnoreCase)
            //    functionName = functionName.ToLower();
            if (!baseIdentifierDict.TryGetValue(functionName, out fnId))
                fnId = null;
            ValidIdentifier fnValId = fnId as ValidIdentifier;
            if (fnValId == null || fnValId.TokenClass != IdentifierClasses.Function)
                throw new Exception($"Function {functionName} is not declared.");
            fnValId.FunctionMethodObject = methodsObject;
        }

        public void RegisterOutputParameter(string name, OutputParameterTypes type)
        {
            if (ValidOutputParamsDict.ContainsKey(name))
                throw new Exception("Duplicate name registered for output parameter.");
            ParameterAttributeNames paramClass;
            switch (type)
            {
                case OutputParameterTypes.boolean:
                    paramClass = ParameterAttributeNames.boolean;
                    break;
                case OutputParameterTypes.@double:
                default:
                    paramClass = ParameterAttributeNames.@double;
                    break;
            }
            ValidOutputParamsDict.Add(name, paramClass);
        }

        public ValidIdentifier DeclareVariable(string name, Type valueType, object initialValue = null,
                                    bool isGlobal = false, ReservedWords? modifierWord = null,
                                    IdentifierClasses identifierClass = IdentifierClasses.Variable,
                                    bool? removeOnParse = null, bool isReadOnly = false, bool isExternal = true)
        {
            ValidIdentifier ident = DeclareIdentifier(baseIdentifierDict, name, valueType, identifierClass,
                                  initialValue: initialValue, isGlobal: isGlobal,
                                  isArray: modifierWord == ReservedWords.array,
                                  removeOnParse: removeOnParse, isExternal: isExternal);
            ident.IsReadOnly = isReadOnly;
            return ident;
        }

        //public void DeclareParameter(string name, double? defaultValue, 
        //                             double? minValue, double? maxValue)
        //{
        //    var prm = new Parameter(name);
        //    prm.DefaultValue = defaultValue;
        //    prm.Value = defaultValue;
        //    prm.MinValue = minValue;
        //    prm.MaxValue = maxValue;
        //    parameterDict.Add(prm.ParameterName, prm);
        //}

        private static ValidIdentifier DeclareIdentifier(Dictionary<string, ValidIdentifier> idDict,
                string name, Type identifierType, IdentifierClasses identifierClass,
                int parameterCount = 0, object initialValue = null, object methodsObject = null,
                MethodInfo fnMethod = null, bool isGlobal = false, bool isArray = false,
                FunctionCategories functionCategory = FunctionCategories.Normal, bool? removeOnParse = null,
                bool typeIsAllowedInDeclarations = true, bool isExternal = false)
        {
            //if (IgnoreCase && identiferClass != IdentifierClasses.Parameter)
            //    name = name.ToLower();
            if (idDict.ContainsKey(name))
                throw new Exception("The identifier " + name + " is already declared.");
            if (identifierType == null && identifierClass != IdentifierClasses.ReservedWord)
            {
                throw new Exception($"DeclareIdentifier was passed null for identifierType for {identifierClass} {name}.");
            }
            if (identifierType != null && identifierType != typeof(void) &&
                !RegisteredTypes.Where(rt => rt.Type == identifierType).Any())
            {
                throw new Exception($"The identifier {name} has an unregistered type: {identifierType.FullName}.");
            }
            if (identifierClass == IdentifierClasses.Variable && !isArray)
            {
                if (initialValue == null)
                {
                    if (identifierType == typeof(double))
                        initialValue = 0D;
                    else if (identifierType == typeof(bool))
                        initialValue = false;
                }
                else if (!initialValue.GetType().IsOrIsSubclassOf(identifierType))
                    initialValue = Convert.ChangeType(initialValue, identifierType);
            }
            ValidIdentifier ident;
            if (isArray)
            {
                if (identifierClass != IdentifierClasses.Variable)
                    throw new Exception("Array declaration not for a variable.");
                if (identifierType != typeof(double))
                    throw new Exception("Array declaration must be for type double.");
                ident = new ArrayVariable(name, isGlobal);
            }
            else
            {
                ident = new ValidIdentifier(name, identifierType, identifierClass, isGlobal,
                                            parameterCount, initialValue, methodsObject, fnMethod,
                                            functionCategory: functionCategory, removeOnParse: removeOnParse,
                                            typeIsAllowedInDeclarations: typeIsAllowedInDeclarations, isExternal: isExternal);
            }
            idDict.Add(name, ident);
            return ident;
        }

        public IEnumerable<ValidIdentifier> GetVariables()
        {
            return baseIdentifierDict.Values.Where(ident => ident.TokenClass == IdentifierClasses.Variable);
        }

        int prevErrorMessageCount = 0;

        public bool Success
        {
            get { return ErrorInfoList.Count == 0; }
        }

        private bool CurrentParseSuccess
        {
            get { return ErrorInfoList.Count == prevErrorMessageCount; }
        }

        private Expression.ExpressionInfo GetExpressionInfo(Token[] exprStack, bool isInitialization = false)
        {
            return new Expression.ExpressionInfo(exprStack, isInitialization);
        }

        private bool InitializeParse(string expression)
        {
            ErrorInfoList = new List<ErrorInfo>();
            tokens = tokenizer.TokenizeExpression(expression);
            tokenIndex = 0;
            List<Token> errTokens = tokens.FindAll(x => x.TokenType == Token.TokenTypes.Invalid);
            if (errTokens.Count > 0)
            {
                AddErrorMessage("Invalid Tokens found: " + string.Join(",", errTokens));
                return false;
            }
            return true;
        }

        private void TransformTokens(TokensTransformInfo tokensTransformInfo)
        {
            List<int> transformIndexes = new List<int>();
            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                if (token.TokenType == Token.TokenTypes.Identifier &&
                    tokensTransformInfo.TransformedPropertyNames.Contains(token.Text))
                {
                    if (i > 0)
                    {
                        Token prevToken = tokens[i - 1];
                        if (prevToken.Text == "@" || prevToken.Text == ".")
                            continue;
                    }
                    transformIndexes.Add(i);
                }
            }
            transformIndexes.Reverse();
            foreach (int index in transformIndexes)
            {
                int charIndex = tokens[index].CharIndex;
                var token = new Token(".", Token.TokenTypes.Operator, charIndex);
                tokens.Insert(index, token);
                token = new Token(tokensTransformInfo.ParentPropertyName,
                                  Token.TokenTypes.Identifier, charIndex);
                tokens.Insert(index, token);
            }
        }

        public bool ParseStatements(string statements, TokensTransformInfo tokensTransformInfo = null)
        {
            if (!InitializeParse(statements))
                return false;
            if (tokensTransformInfo != null)
                TransformTokens(tokensTransformInfo);
            this.Expression = new Expression();
            this.Expression.ExpressionStacks = new List<Expression.ExpressionInfo>();
            this.Expression.IsFirstCallVarIdent = IsFirstCallVarIdent;
            List<string> varNamesToRemove = baseIdentifierDict.Where(kv => kv.Value.RemoveOnParse).Select(kv => kv.Key).ToList();
            foreach (string varName in varNamesToRemove)
            {
                baseIdentifierDict.Remove(varName);
            }
            parameterDict.Clear();
            ParseStatementBlock(Expression.ExpressionStacks, topLevel: true);
            exprStackList = null;
            expressionStack = null;
            tokens = null;
            parseExpressionStacks = null;
            if (Success)
            {
                ResetVariables();
                foreach (BaseParameter baseParameter in Expression.Parameters)
                {
                    baseParameter.EvaluateDependentExpressions();
                }
            }
            return Success;
        }

        private bool ParseStatementBlock(List<Expression.ExpressionInfo> exprStacks, bool topLevel = false)
        {
            Token startToken = GetToken(tokenIndex);
            bool openBrace = startToken != null &&
                             startToken.TokenType == Token.TokenTypes.LeftBrace;
            if (openBrace)
                tokenIndex++;
            while (tokenIndex < tokens.Count)
            {
                if (openBrace)
                {
                    Token closeToken = GetToken(tokenIndex);
                    if (closeToken != null &&
                        closeToken.TokenType == Token.TokenTypes.RightBrace)
                    {
                        tokenIndex++;
                        break;
                    }
                }
                ControlToken controlToken1 = ParseControlBlock();
                if (controlToken1 != null)
                {
                    exprStacks.Add(GetExpressionInfo(new Token[] { controlToken1 }));
                }
                else
                {
                    ParseStatement(exprStacks, topLevel);
                }
                if (!Success)
                    break;
                if (!openBrace && !topLevel)
                    break;
            }
            return Success;
        }

        private bool GetSemicolonToken()
        {
            Token endToken = GetToken(tokenIndex);
            bool retVal = endToken != null && endToken.TokenType == Token.TokenTypes.Semicolon;
            if (retVal)
                tokenIndex++;
            else
                AddErrorMessage("Expecting a semicolon.", endToken);
            return retVal;
        }

        private bool ParseControlExpression(ControlToken controlToken)
        {
            Token token = GetToken(tokenIndex);
            bool haveLParen = (token != null && token.IsLeftParen);
            if (haveLParen)
                tokenIndex++;
            else
                AddErrorMessage($"{controlToken.Text} must be followed by (", token);
            int saveTokenIndex = tokenIndex;
            if (ParseExpr(topLevel: true) != typeof(bool))
                AddErrorMessage($"{controlToken.Text} statement must use a boolean expression.", GetToken(saveTokenIndex));
            Token endToken = GetToken(tokenIndex);
            if (haveLParen)
            {
                if (endToken != null || endToken.TokenType == Token.TokenTypes.RightParen)
                    tokenIndex++;
                else
                    AddErrorMessage($"Missing ) for {controlToken.Text} statement.", endToken);
            }
            if (Success)
            {
                //exprStackList.Add(controlToken);
                expressionStack = exprStackList.ToArray();
                controlToken.ExpressionStacks.Add(GetExpressionInfo(expressionStack));
            }
            return Success;
        }

        private ControlToken ParseControlBlock()
        {
            ControlToken controlToken = ParseControlBlock(ifControlToken: null);
            if (controlToken != null && controlToken.ControlName == ReservedWords.@if)
                //Parse Else block if present:
                ParseControlBlock(ifControlToken: controlToken);
            return controlToken;
        }

        private ControlToken ParseControlBlock(ControlToken ifControlToken)
        {
            ControlToken controlToken = null;
            Token token = GetToken(tokenIndex);
            if (token == null)
                return null;
            ReservedWords? controlName = GetReservedWord(token);
            if (controlName != ReservedWords.@if && controlName != ReservedWords.@else &&
                controlName != ReservedWords.@while && controlName != ReservedWords.@break)
                return null;
            if (ifControlToken != null)
            {
                if (controlName != ReservedWords.@else)
                    return null;
            }
            else if (controlName == ReservedWords.@else)
                AddErrorMessage("Else must follow an If statement.", token);
            tokenIndex++;
            controlToken = new ControlToken(token, (ReservedWords)controlName);
            if (controlName == ReservedWords.@if || controlName == ReservedWords.@while)
            {
                if (!ParseControlExpression(controlToken))
                    return null;
            }
            else if (controlName == ReservedWords.@else && ifControlToken != null)
                ifControlToken.ElseToken = controlToken;
            if (controlName == ReservedWords.@break)
                GetSemicolonToken();
            else
                ParseStatementBlock(controlToken.ExpressionStacks);
            return controlToken;
        }

        //public bool ParseExpression(string expression)
        //{
        //    try
        //    {
        //        if (!InitializeParse(expression))
        //            return false;
        //        ParseExpr(topLevel: true, allowDeclaration: true);
        //        if (Success && tokenIndex < tokens.Count)
        //        {
        //            var extraTokens = tokens.GetRange(tokenIndex, tokens.Count - tokenIndex);
        //            AddErrorMessage("Extra Tokens found after expression: "
        //                + string.Join(" ", extraTokens.Select(tok => tok.Text)));
        //        }
        //        if (Success)
        //        {
        //            expressionStack = exprStackList.ToArray();
        //            this.ResetVariables();
        //        }
        //        return Success;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public void ResetVariables()
        {
            foreach (ValidIdentifier ident in baseIdentifierDict.Values)
            {
                if (ident.TokenClass == IdentifierClasses.Variable && !ident.IsGlobal)
                {
                    ArrayVariable arrayVar = ident as ArrayVariable;
                    if (arrayVar != null)
                        arrayVar.ArrayValue.Clear();
                    else
                        ident.CurrentValue = ident.InitialValue;
                }
            }
        }

        public ValidIdentifier GetVariableIdentifier(string name)
        {
            ValidIdentifier valIdent;
            if (!baseIdentifierDict.TryGetValue(name, out valIdent))
                valIdent = null;
            if (valIdent == null || valIdent.TokenClass != IdentifierClasses.Variable)
                throw new Exception($"Scalar variable {name} is not declared.");
            return valIdent;
        }

        public ValidIdentifier GetValidIdentifier(string name)
        {
            ValidIdentifier valIdent;
            if (!baseIdentifierDict.TryGetValue(name, out valIdent))
            {
                if (!staticIdentifierDict.TryGetValue(name, out valIdent))
                    valIdent = null;
            }
            return valIdent;
        }

        public void SetVariable(string name, object value)
        {
            ValidIdentifier varIdent = GetVariableIdentifier(name) as ValidIdentifier;
            if (varIdent != null)
            {
                if (value == null || value.GetType() == varIdent.IdentifierType)
                    varIdent.CurrentValue = value;
            }
        }

        public double? GetVariable(string name)
        {
            ValidIdentifier varIdent = GetVariableIdentifier(name) as ValidIdentifier;
            return (double?)varIdent.CurrentValue;
        }

        public string ExpressionStackListing(Token[] expressionStack)
        {
            return string.Join(Environment.NewLine, (IEnumerable<Token>)expressionStack);
        }

        private Token PeekToken()
        {
            return exprStackList.LastOrDefault();
        }

        //private Token PopToken(string tokenText)
        //{
        //    Token token = PopToken();
        //    if (token.Text != tokenText)
        //    {
        //        throw new Exception("Expecting '" + tokenText + "'.");
        //    }
        //    return token;
        //}

        private Token PopToken(ref int stackIndex)
        {
            if (stackIndex < 0)
                throw new Exception("Expression stack is empty.");
            else
                return expressionStack[stackIndex--];
        }

        private ReservedWords? GetReservedWord(Token token)
        {
            ReservedWords? retVal = null;
            if (token.TokenType == Token.TokenTypes.Identifier)
            {
                ReservedWords reservedWord;
                if (Enum.TryParse(token.Text.ToLower(), out reservedWord))
                    retVal = reservedWord;
            }
            return retVal;
        }
        
        //private Type GetReservedWordType(ReservedWords reservedWord)
        //{
        //    if (staticIdentifierDict.TryGetValue(reservedWord.ToString(), out ValidIdentifier validIdentifier))
        //        return validIdentifier.IdentifierType;
        //    else
        //        return null;
        //}

        private List<Expression.ExpressionInfo> parseExpressionStacks;

        private void ParseStatement(List<Expression.ExpressionInfo> expressionStacks, bool topLevelBlock)
        {
            if (expressionStacks == null)
                throw new Exception("ParseStatement passed null stack.");
            parseExpressionStacks = expressionStacks;
            Token token = GetToken(tokenIndex);
            if (token != null && token.Text.ToLower() == ReservedWords.parameters.ToString())
            {
                tokenIndex++;
                ParseParameters();
                if (!topLevelBlock)
                    AddErrorMessage("Parameter declarations must be in outermost block of formula.", token);
            }
            else
            {
                ParseExpr(topLevel: true, allowDeclaration: true);
                GetSemicolonToken();
            }
            parseExpressionStacks = null;
        }

        private object ParseAndEvalConstantExpression(Type requiredType)
        {
            return ParseAndEvalConstantExpression(requiredType, out Token[] expr);
        }

        private object ParseAndEvalConstantExpression(Type requiredType, out Token[] expression)
        {
            object val = null;
            var saveExprStackList = exprStackList;
            int saveTokenIndex = tokenIndex;
            Type exprType = ParseExpr(topLevel: true, addToExprStacks: false, forConstant: true);
            expression = null;
            if (Success)
            {
                if (exprType == requiredType)
                {
                    expression = exprStackList.ToArray();
                    val = Expression.EvalExpression(expression /*, forConstant: true */);
                    if (Expression.ErrorMessages.Count > 0)
                    {
                        Token errorToken = GetToken(saveTokenIndex);
                        foreach (string message in Expression.ErrorMessages)
                        {
                            AddErrorMessage(message, errorToken);
                        }
                        val = null;
                    }
                }
                else
                {
                    AddErrorMessage($"Expecting expression of type {requiredType.Name}.", GetToken(saveTokenIndex));
                }
            }
            exprStackList = saveExprStackList;
            return val;
        }

        private Type GetTypeFromName(string typeName, out bool matchesForDeclaration, bool forDeclaration = true)
        {
            Type type = null;
            matchesForDeclaration = true;
            if (staticIdentifierDict.TryGetValue(typeName, out ValidIdentifier validIdent))
            {
                if (validIdent.TokenClass == IdentifierClasses.Type)
                {
                    if (validIdent.TypeIsAllowedInDeclarations == forDeclaration)
                    {
                        type = validIdent.IdentifierType;
                    }
                    else
                    {
                        matchesForDeclaration = false;
                    }
                }
            }
            return type;
        }

        private TypeToken GetTypeToken(Token token, out bool matchesForDeclaration, bool forDeclaration = true)
        {
            TypeToken typeToken = null;
            matchesForDeclaration = true;
            if (token != null)
            {
                Type type = GetTypeFromName(token.Text, out matchesForDeclaration, forDeclaration);
                if (type != null)
                {
                    typeToken = new TypeToken(token, type);
                }
                else if (!matchesForDeclaration)
                {
                    AddErrorMessage($"The type {token.Text} is not allowed here.", token);
                    matchesForDeclaration = false;
                }
            }
            return typeToken;
        }

        private bool TryParseDeclaration(Token token)
        {
            bool isValid = false;
            if (!staticIdentifierDict.TryGetValue(token.Text, out ValidIdentifier validIdent))
            {
                return false;
            }
            Token identToken = token;
            Type dataType = null;
            ReservedWords? modifierWord = null;
            ReservedWords? reservedWord;
            if (validIdent.TokenClass == IdentifierClasses.Type)
            {
                if (validIdent.TypeIsAllowedInDeclarations)
                    dataType = validIdent.IdentifierType;
                else
                    return false;
            }
            else if (validIdent.TokenClass == IdentifierClasses.ReservedWord)
            {
                modifierWord = GetReservedWord(token);
            }
            int saveTokenIndex = tokenIndex;
            //Type modType = modifierWord == null ? null : GetReservedWordType((ReservedWords)modifierWord);
            if (dataType != null || modifierWord == ReservedWords.array)
            {
                token = GetToken(++tokenIndex);
                reservedWord = GetReservedWord(token);
                if (modifierWord == ReservedWords.array &&
                    reservedWord == ReservedWords.@const)
                {
                    AddErrorMessage("A constant cannot be an array.", token);
                }
            }
            else
            {
                reservedWord = modifierWord;
                modifierWord = null;
            }
            if (Success)
            {
                if (reservedWord == ReservedWords.@const ||
                    reservedWord == ReservedWords.var ||
                    reservedWord == ReservedWords.global ||
                    reservedWord == ReservedWords.paramvar)
                {
                    tokenIndex++;
                    ParseDeclarations((ReservedWords)reservedWord, modifierWord, dataType);
                    isValid = true;
                }
                else if (modifierWord != null)
                    AddErrorMessage($"Expecting declaration following {identToken.Text}.", identToken);
            }
            if (!isValid)
                tokenIndex = saveTokenIndex;
            return isValid;
        }

        private Type ParseExpr(bool topLevel = false, bool addToExprStacks = true,
                               bool allowDeclaration = false, bool allowParamVarAssignment = false,
                               bool forConstant = false, bool forSubexpression = false,
                               bool isInitialization = false)
        {
            Type expressionType = null;
            if (topLevel)
                exprStackList = new List<Token>();
            prevErrorMessageCount = ErrorInfoList.Count;
            Token token = GetToken(tokenIndex, required: true);
            if (token == null)
                return null;
            else if (allowDeclaration && token.TokenType == Token.TokenTypes.Identifier
                     && exprStackList.Count == 0 && parseExpressionStacks != null)
            {
                if (TryParseDeclaration(token))
                    return null;
                else if (!Success)
                    return null;
            }
            OperatorToken operatorToken = null;
            while (tokenIndex < tokens.Count)
            {
                int prevStackIndex = exprStackList.Count - 1;
                ParseOperand(forConstant, operatorToken);
                if (!Success)
                    break;
                if (operatorToken != null)
                {
                    if (operatorToken.OperatorInfo.Definition == OperatorDefinitions.Assignment)
                    {
                        //Must assign to a variable:
                        var prevOperand = exprStackList[prevStackIndex] as OperandToken;
                        if (prevOperand != null &&
                            (prevOperand.OperandClass == IdentifierClasses.Variable ||
                             prevOperand.OperandClass == IdentifierClasses.ParameterVariable) &&
                             prevOperand.OperandModifier != OperandModifiers.ArrayLength)
                        {
                            if ((prevOperand.ValidIdentifier != null && prevOperand.ValidIdentifier.IsReadOnly) ||
                                (prevOperand.OperandClass == IdentifierClasses.ParameterVariable && !allowParamVarAssignment))
                            {
                                AddErrorMessage(
                                    $"The variable {prevOperand.Text} is readonly, and cannot be assigned to.", prevOperand);
                            }
                        }
                        else
                        {
                            var prevOperator = exprStackList[prevStackIndex] as OperatorToken;
                            if (prevOperator == null || prevOperator.OperatorInfo.Definition != OperatorDefinitions.Dot)
                            {
                                AddErrorMessage("Assignment must be to a variable or property.", operatorToken);
                            }
                        }
                    }
                    //Check operator priority:
                    List<OperatorToken> stackOps = new List<OperatorToken>();
                    int opPri = operatorToken.OperatorInfo.Priority;
                    for (int i = prevStackIndex; i >= 0; i--)
                    {
                        OperatorToken stackOp = exprStackList[i] as OperatorToken;
                        if (stackOp == null)
                            break;
                        if (stackOp.OperatorInfo.Priority < opPri)
                            stackOps.Add(stackOp);
                    }
                    exprStackList.Add(operatorToken);
                    if (stackOps.Count > 0)
                    {
                        stackOps.Reverse();
                        foreach (OperatorToken stackOp in stackOps)
                        {
                            exprStackList.Remove(stackOp);
                        }
                        exprStackList.AddRange(stackOps);
                    }
                    operatorToken = null;
                }
                token = GetToken(tokenIndex);
                if (token != null && token.TokenType == Token.TokenTypes.Operator)
                {
                    operatorToken = GetOperatorToken(token, isUnary: false);
                }
                if (operatorToken != null)
                    tokenIndex++;
                else
                    break;
            }
            if (topLevel && Success)
            {
                int stackIndex = exprStackList.Count - 1;  //Top of stack.
                var expressionTypeInfo = ValidateTypes(ref stackIndex);
                expressionType = expressionTypeInfo.Type;
                if (Success && parseExpressionStacks != null && addToExprStacks)
                {
                    if (OptimizeExpressions && !forConstant)
                        OptimizeExpression();
                    parseExpressionStacks.Add(GetExpressionInfo(exprStackList.ToArray(), isInitialization));
                }
            }
            return expressionType;
        }

        private void OptimizeExpression()
        {
            int stackIndex = exprStackList.Count - 1;  //Top of stack.
            var constantInds = new List<StackIndices>();
            FindConstantExpressions(ref stackIndex, constantInds);
            Token[] expression = exprStackList.ToArray();
            foreach (StackIndices stackIndices in constantInds)
            {
                stackIndices.ExpressionValue = Expression.EvalExpression(expression, stackIndices.TopIndex);
                if (stackIndices.ExpressionValue == null)
                {
                    AddErrorMessage("Null value for constant expression.", exprStackList[stackIndices.TopIndex]);
                }
            }
            if (!Success || constantInds.Count == 0)
                return;
            constantInds.Reverse();  //Sorted from lowest to highest indices.
            var newExprStackList = new List<Token>();
            int bottomIndex = 0;
            for (int i = 0; i < constantInds.Count; i++)
            {
                StackIndices stackIndices = constantInds[i];
                for (int j = bottomIndex; j < stackIndices.BottomIndex; j++)
                {
                    newExprStackList.Add(exprStackList[j]);
                }
                bottomIndex = stackIndices.TopIndex + 1;
                if (!stackIndices.ExpressionIsConstant)
                {
                    bool isValid = false;
                    OperatorToken operatorToken = exprStackList[stackIndices.TopIndex] as OperatorToken;
                    if (operatorToken != null && operatorToken.OperatorInfo.Definition == OperatorDefinitions.Dot)
                    {
                        if (exprStackList[stackIndices.BottomIndex] is TypeToken)
                        {
                            FunctionToken methodToken = exprStackList[stackIndices.TopIndex - 1] as FunctionToken;
                            if (methodToken != null)
                            {
                                isValid = true;
                                methodToken.EvalMethodObject = false;
                                for (int j = stackIndices.BottomIndex + 1; j < stackIndices.TopIndex; j++)
                                {
                                    newExprStackList.Add(exprStackList[j]);
                                }
                            }
                        }
                    }
                    if (!isValid)
                        throw new Exception("Invalid structure for optimizing.");
                }
                else
                {
                    var tok = new Token(stackIndices.ExpressionValue.ToString(), Token.TokenTypes.Identifier,
                                        exprStackList[stackIndices.TopIndex].CharIndex);
                    var operandTok = new OperandToken(tok, stackIndices.ExpressionValue.GetType());
                    operandTok.OperandClass = IdentifierClasses.Constant;
                    operandTok.OperandValue = stackIndices.ExpressionValue;
                    newExprStackList.Add(operandTok);
                }
                if (i == constantInds.Count - 1)
                {
                    for (int j = stackIndices.TopIndex + 1; j < exprStackList.Count; j++)
                    {
                        newExprStackList.Add(exprStackList[j]);
                    }
                }
            }
            exprStackList = newExprStackList;
        }

        private class StackIndices
        {
            public int TopIndex { get; }
            public int BottomIndex { get; }
            public bool ExpressionIsConstant { get; }
            public object ExpressionValue { get; set; }

            public StackIndices(int topIndex, int bottomIndex, bool expressionIsConstant)
            {
                TopIndex = topIndex;
                BottomIndex = bottomIndex;
                ExpressionIsConstant = expressionIsConstant;
            }
        }

        private bool FindConstantExpressions(ref int stackIndex, List<StackIndices> constantInds, bool forSubExpr = false)
        {
            int startConstantIndsCount = constantInds.Count;
            int topIndex = stackIndex;
            Token token = stackIndex < 0 ? null : exprStackList[stackIndex--];
            bool expressionIsConstant = true;
            var operandTok = token as BaseOperandToken;
            bool optimize = false;
            if (operandTok != null)
            {
                if (operandTok.OperandClass == IdentifierClasses.Function)
                {
                    FunctionToken functionToken = (FunctionToken)operandTok;
                    if (functionToken.FunctionCategory != FunctionCategories.Normal || functionToken.FunctionMethodObject != null)
                        expressionIsConstant = false;
                    for (int i = functionToken.FunctionArgumentCount; i >= 1; i--)
                    {
                        if (!FindConstantExpressions(ref stackIndex, constantInds))
                            expressionIsConstant = false;
                        //if (i > 1)
                        //    stackIndex--;  //Skip comma.
                    }
                    optimize = expressionIsConstant;
                }
                else
                {
                    expressionIsConstant = operandTok.OperandClass == IdentifierClasses.Constant ||
                                           operandTok.OperandClass == IdentifierClasses.Literal;
                }
            }
            else if (token is TypeToken typeTok && typeTok.OperandClass == IdentifierClasses.Type)
            {
                return false;
            }
            else
            {
                var operatorTok = token as OperatorToken;
                if (operatorTok != null)
                {
                    for (int i = 0; i < (operatorTok.OperatorInfo.IsUnary ? 1 : 2); i++)
                    {
                        int topStackIndex = stackIndex;
                        if (!FindConstantExpressions(ref stackIndex, constantInds))
                        {
                            expressionIsConstant = false;
                            if (i == 1 && operatorTok.OperatorInfo.Definition == OperatorDefinitions.Dot)
                            {
                                var typeToken = exprStackList[topStackIndex] as TypeToken;
                                if (typeToken != null && typeToken.OperandClass == IdentifierClasses.Type)
                                    optimize = true;
                            }
                        }
                    }
                    if (expressionIsConstant)
                        optimize = true;
                }
                else if (token != null && token.IsLeftParen)
                {
                    if (!FindConstantExpressions(ref stackIndex, constantInds, forSubExpr: true))
                        expressionIsConstant = false;
                }
                else
                    throw new Exception("Unexpected end of expression stack.");
            }
            if (optimize)
            {
                int bottomIndex = stackIndex + 1;
                if (expressionIsConstant)
                {
                    if (forSubExpr)
                        topIndex++;  //Include Left Paren token
                    if (constantInds.Count > startConstantIndsCount)
                        constantInds.RemoveRange(startConstantIndsCount, constantInds.Count - startConstantIndsCount);
                }
                constantInds.Add(new StackIndices(topIndex, bottomIndex, expressionIsConstant));
            }
            return expressionIsConstant;
        }

        private TypeTokenInfo[] ValidateMethodTypes(ref int stackIndex, FunctionToken functionToken, Type parentType = null)
        {
            List<TypeTokenInfo> argTypes = new List<TypeTokenInfo>();
            for (int i = functionToken.FunctionArgumentCount; i >= 1; i--)
            {
                var argumentTypeInfo = ValidateTypes(ref stackIndex);
                argTypes.Add(argumentTypeInfo);
                //if (argumentTypeInfo.Type != typeof(double) && functionToken.FunctionCategory == FunctionCategories.Normal)
                //{
                //    AddErrorMessage("Normal function arguments must be numeric.", functionToken);
                //}
                //if (i > 1)
                //    stackIndex--;  //Skip comma.
            }
            argTypes.Reverse();
            if (functionToken.FunctionCategory == FunctionCategories.Normal ||
                functionToken.FunctionCategory == FunctionCategories.Custom)
            {
                var methodInfo = functionToken.MemberInfo as MethodInfo;
                if (methodInfo != null)
                {
                    if (!GetMethod.MethodMatches(methodInfo, argTypes.ToArray()))
                    {
                        AddErrorMessage($"Function {functionToken.Text} has invalid argument types.", functionToken);
                    }
                }
            }
            return argTypes.ToArray();
        }

        private bool IsNumericType(Type type)
        {
            return type == typeof(double) || type == typeof(int);
        }

        private TypeTokenInfo ValidateTypes(ref int stackIndex, bool forDotOperator = false)
        {
            var typeTokenInfo = new TypeTokenInfo();
            Token token = stackIndex < 0 ? null : exprStackList[stackIndex--];
            var operandTok = token as BaseOperandToken;
            if (operandTok != null)
            {
                if (operandTok.OperandClass == IdentifierClasses.Function)
                {
                    var functionToken = (FunctionToken)operandTok;
                    typeTokenInfo.ArgTypes = ValidateMethodTypes(ref stackIndex, functionToken);
                    if (functionToken.FunctionCategory == FunctionCategories.Constructor)
                    {
                        functionToken.MemberInfo = GetMethod.GetConstructorInfo(functionToken.OperandType,
                                                   typeTokenInfo.ArgTypes);
                        if (functionToken.MemberInfo == null)
                        {
                            AddErrorMessage($"Invalid argument types for constructor {functionToken.Text}.");
                        }
                    }
                    else if (functionToken.MemberInfo == null && !forDotOperator)
                    {
                        foreach (Type methodsType in StaticMethodsTypes)
                        {
                            MethodInfo methodInfo = GetMethod.GetMethodInfo(methodsType, functionToken.Text, typeTokenInfo.ArgTypes);
                            if (methodInfo != null)
                            {
                                functionToken.MemberInfo = methodInfo;
                                functionToken.ReturnType = methodInfo.ReturnType;
                                break;
                            }
                        }
                        if (functionToken.MemberInfo == null)
                        {
                            AddErrorMessage($"Invalid argument types for method {functionToken.Text}.");
                        }
                    }
                }
                else if (operandTok.OperandClass != IdentifierClasses.Member)
                {
                    OperandToken operandToken = (OperandToken)operandTok;
                    if ((operandToken.OperandClass == IdentifierClasses.Variable ||
                         operandToken.OperandClass == IdentifierClasses.ParameterVariable) &&
                        !operandToken.ValidIdentifier.IsReadOnly)
                    {
                        typeTokenInfo.IsLValue = true;
                    }
                    bool checkIndex = (operandToken.ValidIdentifier is ArrayVariable ||
                                        operandToken.BaseParameter is ArrayParameter)
                                && operandToken.OperandModifier != OperandModifiers.ArrayLength;
                    if (checkIndex)
                    {
                        //Check array index:
                        var indexType = ValidateTypes(ref stackIndex);
                        if (!IsNumericType(indexType.Type))
                        {
                            AddErrorMessage("Array index was not numeric.", operandTok);
                        }
                    }
                    if (operandTok.OperandClass == IdentifierClasses.Parameter)
                    {
                        if (RequireGetRandomValue)
                        {
                            var cprm = operandTok.BaseParameter as CustomParameter;
                            if (cprm != null && cprm.CustomType == CustomParameterTypes.RandomRange)
                            {
                                AddErrorMessage("Random parameter references must use GetRandomValue for this type of formula.", token);
                            }
                        }
                    }
                    typeTokenInfo.Token = operandToken;
                }
                typeTokenInfo.Type = operandTok.OperandType;
                return typeTokenInfo;
            }
            var typeToken = token as TypeToken;
            if (typeToken != null)
            {
                typeTokenInfo.Type = typeToken.ReturnType;
                return typeTokenInfo;
            }
            var operatorTok = token as OperatorToken;
            if (operatorTok != null)
            {
                FunctionToken functionToken = (stackIndex < 0 ? null : exprStackList[stackIndex]) as FunctionToken;
                Type requiredType = operatorTok.OperatorInfo.Operand1Type;
                TypeTokenInfo operand2Type;
                if (operatorTok.OperatorInfo.IsUnary)
                {
                    operand2Type = new TypeTokenInfo();
                    if (operatorTok.OperatorInfo.Definition == OperatorDefinitions.Cast)
                        operand2Type.Type = typeof(Type);
                    else
                        operand2Type.Type = requiredType;
                }
                else
                {
                    operand2Type = ValidateTypes(ref stackIndex, 
                                   forDotOperator: operatorTok.OperatorInfo.Definition == OperatorDefinitions.Dot);
                }
                int operand1Index = stackIndex;
                TypeTokenInfo operand1Type = ValidateTypes(ref stackIndex);
                if (operatorTok.OperatorInfo.Definition == OperatorDefinitions.Dot && functionToken != null && 
                    operand1Type.Type != null)
                {
                    bool isStatic = exprStackList[operand1Index] is TypeToken;
                    if (functionToken.OperandClass == IdentifierClasses.Member)
                    {
                        BindingFlags flag = isStatic ? BindingFlags.Static : BindingFlags.Instance;
                        functionToken.MemberInfo = operand1Type.Type.GetMember(functionToken.Text,
                                BindingFlags.Public | flag | BindingFlags.IgnoreCase).FirstOrDefault();
                        if (functionToken.MemberInfo == null)
                            AddErrorMessage($"Member {functionToken.Text} was not found.", functionToken);
                        else
                        {
                            Type type = null;
                            switch (functionToken.MemberInfo.MemberType)
                            {
                                case MemberTypes.Field:
                                    var fieldInfo = (FieldInfo)functionToken.MemberInfo;
                                    type = fieldInfo.FieldType;
                                    typeTokenInfo.IsLValue = !fieldInfo.IsLiteral;
                                    break;
                                case MemberTypes.Property:
                                    var propInfo = (PropertyInfo)functionToken.MemberInfo;
                                    type = propInfo.PropertyType;
                                    typeTokenInfo.IsLValue = propInfo.CanWrite && propInfo.SetMethod.IsPublic &&
                                                             propInfo.GetCustomAttribute<ReadOnlyAttribute>() == null;
                                    break;
                                default:
                                    AddErrorMessage($"Member Type {functionToken.MemberInfo.MemberType} is not supported.", 
                                                    functionToken);
                                    break;
                            }
                            functionToken.ReturnType = type;
                        }
                    }
                    else if (operand2Type.ArgTypes != null && operand1Type != null)
                    {
                        MethodInfo methodInfo = GetMethod.GetMethodInfo(operand1Type.Type, functionToken.Text,
                                                operand2Type.ArgTypes, isStatic, ignoreCase: true);
                        if (methodInfo != null)
                        {
                            functionToken.MemberInfo = methodInfo;
                            functionToken.ReturnType = methodInfo.ReturnType;
                        }
                        else
                        {
                            bool nameIsValid = operand1Type.Type.GetMethods().Where(mi => mi.Name.Equals(functionToken.Text, 
                                               StringComparison.OrdinalIgnoreCase)).Any();
                            if (nameIsValid)
                                AddErrorMessage($"Invalid types for function {functionToken.Text}.", functionToken);
                            else
                                AddErrorMessage($"Method {functionToken.Text} was not found.", functionToken);
                        }
                    }
                    operatorTok.ReturnType = functionToken.ReturnType;
                }
                else
                {
                    Type requiredType2;
                    if (operatorTok.OperatorInfo.Definition == OperatorDefinitions.Assignment)
                        requiredType2 = requiredType = operand1Type.Type;
                    else
                        requiredType2 = operatorTok.OperatorInfo.Operand2Type;
                    if (!(operand1Type.Type.IsOrIsSubclassOf(requiredType) && operand2Type.Type.IsOrIsSubclassOf(requiredType2)))
                    {
                        if (!SetOperatorMethod(operatorTok, operand1Type, operand2Type))
                        {
                            AddErrorMessage(
                          $"Invalid types for operator {operatorTok.Text}", operatorTok);
                        }
                    }
                }
                if (operatorTok.OperatorInfo.Definition == OperatorDefinitions.Assignment)
                {
                    if (!operand1Type.Type.IsAssignableFrom(operand2Type.Type))
                    {
                        AddErrorMessage("Operator '=' requires arguments of the same type.", operatorTok);
                    }
                    else
                        operatorTok.ReturnType = operand1Type.Type;
                    if (!operand1Type.IsLValue)
                    {
                        AddErrorMessage("Cannot assign to left of '=' operator.", operatorTok);
                    }
                }
                typeTokenInfo.Type = operatorTok.ReturnType;
                return typeTokenInfo;
            }
            if (token != null && token.IsLeftParen)
                return ValidateTypes(ref stackIndex);
            throw new Exception("Unexpected end of expression stack.");
        }

        private bool OperatorTypesMatch(ValidOperator op, TypeTokenInfo operand1Type, TypeTokenInfo operand2Type)
        {
            Type type1, type2;
            if (op.Definition == OperatorDefinitions.Assignment)
                type1 = type2 = operand1Type.Type;
            else
            {
                type1 = op.Operand1Type;
                type2 = op.Operand2Type;
            }
            return GetMethod.TypesAreCompatible(type1, operand1Type) &&
                   GetMethod.TypesAreCompatible(type2, operand2Type);
        }

        private bool SetOperatorMethod(OperatorToken operatorTok, TypeTokenInfo operand1Type, TypeTokenInfo operand2Type)
        {
            var validOps = GetValidOperators(operatorTok.Text, operatorTok.OperatorInfo.IsUnary);
            if (validOps != null)
            {
                var validOp = validOps.Find(op => OperatorTypesMatch(op, operand1Type, operand2Type));
                if (validOp != null)
                {
                    operatorTok.OperatorInfo = validOp;
                    return true;
                }
            }
            TypeTokenInfo[] operandTypes;
            if (operatorTok.OperatorInfo.IsUnary)
                operandTypes = new TypeTokenInfo[] { operand1Type };
            else
                operandTypes = new TypeTokenInfo[] { operand1Type, operand2Type };
            MethodInfo methodInfo = operatorTok.OperatorInfo.GetCustomMethodInfo(CustomTypes, operandTypes);
            if (methodInfo != null)
            {
                operatorTok.OperatorMethod = methodInfo;
                operatorTok.ReturnType = methodInfo.ReturnType;
            }
            return (methodInfo != null);
        }

        private void ParseOperand(bool forConstant, OperatorToken operatorToken = null)
        {
            Token token = GetToken(tokenIndex, required: true);
            if (token == null)
            {
                return;
            }
            tokenIndex++;
            int origTokenIndex = tokenIndex;
            if (token.Text == "@")
            {
                if (forConstant)
                {
                    AddErrorMessage("A constant expression cannot contain parameters.", token);
                }
                ParseParameterCall();
                return;
            }
            bool afterDot = operatorToken != null && operatorToken.OperatorInfo.Definition == OperatorDefinitions.Dot;
            if (afterDot && token.TokenType != Token.TokenTypes.Identifier)
            {
                AddErrorMessage("Expecting identifier following . operator.", operatorToken);
            }
            ReservedWords? reservedWord = GetReservedWord(token);
            bool forConstructor = reservedWord == ReservedWords.@new;
            if (forConstructor)
            {
                token = GetToken(tokenIndex++, required: true);
                if (token == null)
                    return;
            }
            BaseOperandToken operandTok = null;
            OperandToken operandToken = null;
            TypeToken typeToken = null;
            FunctionToken functionToken = null;
            switch (token.TokenType)
            {
                case Token.TokenTypes.Number:
                case Token.TokenTypes.String:
                case Token.TokenTypes.Identifier:
                    Type operandType = token.TokenType == Token.TokenTypes.String ? typeof(string) : typeof(double);
                    Token nextToken = GetToken(tokenIndex);
                    bool isFunction = (token.TokenType == Token.TokenTypes.Identifier && nextToken != null
                                       && nextToken.IsLeftParen);
                    if (token.TokenType == Token.TokenTypes.Identifier)
                    {
                        typeToken = GetTypeToken(token, out bool matchesForDeclaration);
                    }
                    if (isFunction)
                    {
                        FunctionCategories functionCategory;
                        if (forConstructor)
                        {
                            functionCategory = FunctionCategories.Constructor;
                            if (typeToken == null)
                                AddErrorMessage("A constructor must use a valid type.", token);
                            else
                                operandType = typeToken.ReturnType;
                        }
                        else if (afterDot)
                            functionCategory = FunctionCategories.Method;
                        else
                            functionCategory = FunctionCategories.Normal;
                        functionToken = new FunctionToken(token, operandType, functionCategory);
                        operandTok = functionToken;
                    }
                    else
                    {
                        if (afterDot && token.TokenType == Token.TokenTypes.Identifier)
                        {
                            functionToken = new FunctionToken(token, operandType, FunctionCategories.Method, IdentifierClasses.Member);
                            operandTok = functionToken;
                        }
                        else
                        {
                            if (typeToken == null)
                            {
                                operandToken = new OperandToken(token, operandType);
                                operandTok = operandToken;
                            }
                            else
                            {
                                if (nextToken.Text != ".")
                                    AddErrorMessage("A type must be followed by '.'", token);
                            }
                        }
                    }
                    if (token.TokenType == Token.TokenTypes.Identifier && (typeToken == null || forConstructor))
                    {
                        ValidIdentifier validIdentifier = null;
                        if (!(afterDot || forConstructor))
                        {
                            if (!(baseIdentifierDict.TryGetValue(token.Text,
                                                                 out validIdentifier)
                               || staticIdentifierDict.TryGetValue(token.Text,
                                                                   out validIdentifier)))
                            {
                                if (!isFunction)
                                    AddErrorMessage($"The identifier '{token.Text}' is not declared.", token);
                            }
                            else if (validIdentifier.TokenClass == IdentifierClasses.ReservedWord)
                            {
                                AddErrorMessage($"{token.Text} is a reserved word.", token);
                            }
                            else 
                            {
                                if (forConstant)
                                {
                                    if (validIdentifier.TokenClass == IdentifierClasses.Variable ||
                                        validIdentifier.TokenClass == IdentifierClasses.ParameterVariable)
                                    {
                                        AddErrorMessage("A constant expression cannot have variables.", token);
                                    }
                                }
                                if (isFunction != (validIdentifier.TokenClass == IdentifierClasses.Function))
                                {
                                    AddErrorMessage(
                                    $"The identifer '{token.Text}' is declared as a {validIdentifier.TokenClass}.", token);
                                }
                                else
                                    operandTok.SetFromValidIdentifier(validIdentifier);
                            }
                        }
                        if (isFunction)
                        {
                            tokenIndex++;
                            ParseFunctionCall(functionToken, forConstant);
                        }
                        else
                        {
                            ArrayVariable arrayVar = validIdentifier as ArrayVariable;
                            if (arrayVar != null)
                            {
                                operandToken.OperandModifier = ParseArrayModifiers();
                            }
                        }
                    }
                    if (typeToken != null && !forConstructor)
                        exprStackList.Add(typeToken);
                    else
                        exprStackList.Add(operandTok);
                    break;
                case Token.TokenTypes.Operator:
                    OperatorToken opTok = GetOperatorToken(token, isUnary: true);
                    if (opTok != null)
                    {
                        if (opTok.OperatorInfo.Definition == OperatorDefinitions.Cast)
                        {
                            Token leftParenToken = token;
                            token = GetToken(tokenIndex);
                            TypeToken castTypeToken = GetTypeToken(token, out bool matchesForDeclaration);
                            if (castTypeToken != null)
                            {
                                //Cast operator
                                opTok.Text = $"({token.Text})";
                                opTok.ReturnType = castTypeToken.ReturnType;
                                tokenIndex++;
                                token = GetToken(tokenIndex++);
                                if (token?.TokenType != Token.TokenTypes.RightParen)
                                {
                                    AddErrorMessage("Expecting ')' for type cast.", token);
                                }
                            }
                            else
                            {
                                //Treat token as left paren
                                leftParenToken.TokenType = Token.TokenTypes.LeftParen;
                                ParseExpr(forConstant: forConstant, forSubexpression: true);
                                if (Success)
                                {
                                    token = GetToken(tokenIndex, required: true);
                                    if (token != null)
                                    {
                                        if (token.TokenType == Token.TokenTypes.RightParen)
                                        {
                                            exprStackList.Add(leftParenToken);
                                            tokenIndex++;
                                        }
                                        else
                                        {
                                            AddErrorMessage("Expecting ')'", token);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        ParseOperand(forConstant);
                        if (Success)
                        {
                            bool addOperator = true;
                            if (opTok.OperatorInfo.Definition == OperatorDefinitions.UnaryMinus)
                            {
                                OperandToken lastToken = PeekToken() as OperandToken;
                                if (lastToken?.TokenType == Token.TokenTypes.Number)
                                {
                                    lastToken.OperandValue = -(double)lastToken.OperandValue;
                                    addOperator = false;
                                }
                            }
                            if (addOperator)
                                exprStackList.Add(opTok);
                        }
                    }
                    else
                    {
                        tokenIndex--;
                        AddErrorMessage($"Operator {token.Text} is not unary.", token);
                    }
                    break;
                default:
                    tokenIndex--;
                    AddErrorMessage("Invalid token: " + token.Text, token);
                    break;
            }
            if (forConstructor)
            {
                if (typeToken == null)
                    AddErrorMessage("The new keyword must be followed by a type.", token);
                else if (functionToken == null)
                    AddErrorMessage("The new keyword must be followed by a type and '('.", token);
            }
        }

        private void ParseParameterCall()
        {
            //Read parameter reference:
            Token token = GetToken(tokenIndex++);
            if (token?.TokenType != Token.TokenTypes.Identifier)
            {
                AddErrorMessage("Expecting parameter name after @", token);
                return;
            }
            BaseOperandToken operandTok = null;
            OperandToken operandToken = null;
            BaseParameter parameter;
            if (!parameterDict.TryGetValue(token.Text, out parameter))
            {
                AddErrorMessage($"The parameter @{token.Text} is not declared.", token);
            }
            else
            {
                Parameter prm = parameter as Parameter;
                if (prm != null && prm.HasChoices)
                {
                    token = GetToken(tokenIndex);
                    if (token?.Text == ".")
                    {
                        tokenIndex++;
                        token = GetToken(tokenIndex++);
                        ParameterChoice parameterChoice = null;
                        if (token?.TokenType == Token.TokenTypes.Identifier)
                        {
                            parameterChoice = prm.GetParameterChoice(token.Text);
                        }
                        if (parameterChoice == null)
                        {
                            AddErrorMessage($"{token?.Text} is not a valid choice for parameter {prm.ParameterName}.", token);
                        }
                        else
                        {
                            operandToken = new OperandToken(token);
                            operandToken.SetAsConstant(parameterChoice.Value);
                            operandTok = operandToken;
                        }
                        parameter = null;
                    }
                }
                if (parameter != null)
                {
                    var varFnParam = parameter as VarFunctionParameter;
                    FunctionToken functionToken = null;
                    if (varFnParam != null)
                    {
                        functionToken = new FunctionToken(token);
                        operandTok = functionToken;
                    }
                    else
                    {
                        operandToken = new OperandToken(token);
                        operandTok = operandToken;
                    }
                    operandTok.SetParameterObject(parameter);
                    if (varFnParam != null)
                    {
                        token = GetToken(tokenIndex++);
                        if (token == null || !token.IsLeftParen)
                        {
                            AddErrorMessage($"Expecting ( following VarFunction parameter {varFnParam.ParameterName}.", token);
                        }
                        else
                        {
                            ParseFunctionCall(functionToken, forConstant: false);
                        }
                    }
                    else
                    {
                        var arrayParam = parameter as ArrayParameter;
                        if (arrayParam != null)
                        {
                            operandToken.OperandModifier = ParseArrayModifiers();
                        }
                    }
                }
            }
            if (operandTok != null)
                exprStackList.Add(operandTok);
        }

        private OperandModifiers ParseArrayModifiers()
        {
            Token nextTok = GetToken(tokenIndex);
            OperandModifiers opMod = OperandModifiers.None;
            if (nextTok != null && nextTok.Text == ".")
            {
                nextTok = GetToken(++tokenIndex);
                ReservedWords? rWd = GetReservedWord(nextTok);
                if (rWd == ReservedWords.length)
                {
                    opMod = OperandModifiers.ArrayLength;
                    tokenIndex++;
                }
                else
                {
                    AddErrorMessage("Expecting .length for array variable", nextTok);
                }
            }
            else
            {
                //Array subscript expression goes below parameter operand in stack.
                ParseArraySubscript();
            }
            return opMod;
        }

        private void ParseArraySubscript()
        {
            Token token = GetToken(tokenIndex);
            if (token == null || token.TokenType != Token.TokenTypes.LeftBracket)
            {
                AddErrorMessage("Expecting [ for array variable.", token);
                return;
            }
            tokenIndex++;
            ParseExpr();
            token = GetToken(tokenIndex++);
            if (token == null || token.TokenType != Token.TokenTypes.RightBracket)
            {
                AddErrorMessage("Expecting ] after array index.", token);
                return;
            }
        }

        private void ParseDeclarations(ReservedWords decType, ReservedWords? modifierWord, Type dataType)
        {
            while (true)
            {
                ParseDeclaration(decType, dataType ?? typeof(double), modifierWord);
                if (!Success)
                    break;
                //if (exprStackList != null)
                //    parseExpressionStacks.Add(exprStackList.ToArray());
                Token token = GetToken(tokenIndex);
                if (token != null && token.TokenType == Token.TokenTypes.Comma)
                    tokenIndex++;
                else
                    break;
            } 
        }

        private void ParseDeclaration(ReservedWords decType,
                                      Type dataType,
                                      ReservedWords? modifierWord)
        {
            int startTokenIndex = tokenIndex;
            Token token = GetToken(tokenIndex);
            if (token == null || token.TokenType != Token.TokenTypes.Identifier)
            {
                AddErrorMessage("Expecting an identifier to declare.", token);
                return;
            }
            Token identToken = token;
            if (baseIdentifierDict.ContainsKey(identToken.Text) ||
                staticIdentifierDict.ContainsKey(identToken.Text))
            {
                if (GetReservedWord(identToken) != null)
                {
                    AddErrorMessage($"The identifier {identToken.Text} is a reserved word.", identToken);
                }
                else
                {
                    AddErrorMessage("The identifier " + identToken.Text + " is already declared.", identToken);
                }
                return;
            }
            tokenIndex++;
            token = GetToken(tokenIndex);
            OperatorToken assignmentOperator = null;
            if (token?.TokenType == Token.TokenTypes.Operator)
            {
                assignmentOperator = GetOperatorToken(token, isUnary: false);
                if (assignmentOperator != null)
                {
                    if (assignmentOperator.OperatorInfo.Definition == 
                                        OperatorDefinitions.Assignment)
                    {
                        if (modifierWord == ReservedWords.array)
                        {
                            AddErrorMessage(
                                "The '=' operator cannot appear in an array declaration.", token);
                            return;
                        }
                        else
                            tokenIndex++;
                    }
                    else
                    {
                        AddErrorMessage(
                            "Only the '=' operator can follow an identifier being declared.", token);
                        return;
                    }
                }
            }
            if (assignmentOperator == null && (decType == ReservedWords.@const || decType == ReservedWords.paramvar))
            {
                AddErrorMessage($"Expecting an '=' for {decType} declaration.", identToken);
            }
            object assignedValue = null;
            if (decType == ReservedWords.@const)
            {
                assignedValue = ParseAndEvalConstantExpression(dataType, out Token[] expression);
                if (assignedValue == null)
                {
                    AddErrorMessage(
                        $"Invalid type of expression assigned to Constant {identToken.Text}.", identToken);
                }
                else
                {
                    var valIdent = DeclareConstant(baseIdentifierDict, identToken.Text, dataType, assignedValue);
                    valIdent.InitializationExpression = expression;
                    valIdent.SourceToken = identToken;
                }
                return;
            }
            IdentifierClasses identifierClass = decType == ReservedWords.paramvar ? IdentifierClasses.ParameterVariable : IdentifierClasses.Variable;
            ValidIdentifier varIdent = DeclareVariable(identToken.Text, dataType, isGlobal: decType == ReservedWords.global,
                            modifierWord: modifierWord, identifierClass: identifierClass, removeOnParse: true, isExternal: false);
            varIdent.SourceToken = identToken;
            if (assignmentOperator != null)
            {
                //Parse the assignment to the variable:
                tokenIndex = startTokenIndex;
                int expressionIndex = Expression.ExpressionStacks.Count;
                if (ParseExpr(topLevel: true, addToExprStacks: decType == ReservedWords.var,
                              allowParamVarAssignment: decType == ReservedWords.paramvar,
                              isInitialization: true) != dataType)
                {
                    AddErrorMessage($"Invalid type of expression assigned to Variable {identToken.Text}.", identToken);
                }
                else
                {
                    Token[] expression = exprStackList.ToArray();
                    varIdent.InitializationExpression = expression;
                    if (decType == ReservedWords.var)
                    {
                        Expression.InitExpressionIndexes.Add(expressionIndex);
                    }
                    else
                    {
                        Token varTok = exprStackList.Find(
                            tok => tok != exprStackList.First() && tok is OperandToken &&
                            ((OperandToken)tok).OperandClass == IdentifierClasses.Variable);
                        if (varTok != null)
                        {
                            AddErrorMessage(
             $"A {(decType == ReservedWords.global ? "Global" : "ParamVar")} assignment cannot contain variables.", 
             varTok);
                        }
                        else
                        {
                            IEnumerable<OperandToken> paramToks =
                                expression.Where(tok => tok is OperandToken)
                                          .Select(tok => (OperandToken)tok)
                                          .Where(ot => ot.BaseParameter is IDependentExpressions);
                            if (!paramToks.Any())
                            {
                                if (decType == ReservedWords.paramvar)
                                    AddErrorMessage("A ParamVar assignment must contain at least one parameter.");
                            }
                            else
                            {
                                IEnumerable<IDependentExpressions> exprParams =
                                    paramToks.Select(ot => (IDependentExpressions)ot.BaseParameter).Distinct();
                                foreach (IDependentExpressions exprParam in exprParams)
                                {
                                    exprParam.AddDependentExpression(expression);
                                }
                            }
                            if (decType == ReservedWords.global)
                            {
                                Expression.InitialExpressionStacks.Add(
                                    GetExpressionInfo(expression, isInitialization: true));
                            }
                        }
                    }
                }
            }
        }

        private void ParseParameters()
        {
            Token token = GetToken(tokenIndex);
            if (token == null || token.TokenType != Token.TokenTypes.LeftBrace)
            {
                AddErrorMessage("Expecting { for parameters block.", token);
                return;
            }
            tokenIndex++;
            while (ParseParameterDeclaration())  //Consumes } at end.
            {
                if (!Success)
                    break;
            }
        }

        public enum ParameterAttributeNames
        {
            @default,
            min,
            max,
            improvmin,
            improvmax,
            array,
            custom,
            random,
            locked,
            decimals,
            integer,
            varfunction,
            parameters,
            @double,
            boolean,
            complex,
            influencepoint,
            choices,
            label,
            domain,
            output,
            visible
        }

        private static HashSet<ParameterAttributeNames> parameterTypeNames = new HashSet<ParameterAttributeNames>()
        {
            ParameterAttributeNames.array,
            ParameterAttributeNames.custom,
            ParameterAttributeNames.random,
            ParameterAttributeNames.boolean,
            ParameterAttributeNames.complex,
            ParameterAttributeNames.influencepoint,
            ParameterAttributeNames.varfunction
        };

        private static Dictionary<ParameterAttributeNames, HashSet<ParameterAttributeNames>> validParamAttrDict { get; } =
                   new Dictionary<ParameterAttributeNames, HashSet<ParameterAttributeNames>>();

        private static void PopulateValidParamAttrDict()
        {
            validParamAttrDict.Add(ParameterAttributeNames.@double, new HashSet<ParameterAttributeNames>() {
                    ParameterAttributeNames.@default, ParameterAttributeNames.choices, ParameterAttributeNames.decimals,
                    ParameterAttributeNames.improvmax, ParameterAttributeNames.improvmin, ParameterAttributeNames.integer,
                    ParameterAttributeNames.locked, ParameterAttributeNames.max, ParameterAttributeNames.min });
            validParamAttrDict.Add(ParameterAttributeNames.boolean, new HashSet<ParameterAttributeNames>() {
                    ParameterAttributeNames.@default });
            validParamAttrDict.Add(ParameterAttributeNames.complex, new HashSet<ParameterAttributeNames>() {
                    ParameterAttributeNames.@default, ParameterAttributeNames.choices });
            validParamAttrDict.Add(ParameterAttributeNames.varfunction, new HashSet<ParameterAttributeNames>() {
                    ParameterAttributeNames.@default, ParameterAttributeNames.choices, ParameterAttributeNames.domain,
                    ParameterAttributeNames.parameters });
        }

        private static HashSet<ParameterAttributeNames> GetValidParamAttrs(ParameterAttributeNames paramClass)
        {
            if (!validParamAttrDict.TryGetValue(paramClass, out HashSet<ParameterAttributeNames> validAttrs))
                validAttrs = new HashSet<ParameterAttributeNames>();
            validAttrs.Add(ParameterAttributeNames.label);
            //validAttrs.Add(ParameterAttributeNames.visible);
            return validAttrs;
        }

        private ParameterAttributeNames? ParseParamAttr(string attrNameText)
        {
            if (IgnoreCase)
                attrNameText = attrNameText.ToLower();
            if (Enum.TryParse(attrNameText, out ParameterAttributeNames attrName))
                return attrName;
            else
                return null;
        }

        public Dictionary<string, ParameterAttributeNames> ValidOutputParamsDict { get; private set; } =
            new Dictionary<string, ParameterAttributeNames>(StringComparer.OrdinalIgnoreCase);

        private ParameterAttributeNames? TryParseOutputParameterDeclaration(Token token, out string paramName)
        {
            ParameterAttributeNames? paramClass = null;
            paramName = null;
            if (ParseParamAttr(token.Text) == ParameterAttributeNames.output)
            {
                token = GetToken(tokenIndex++);
                if (token != null && ValidOutputParamsDict.TryGetValue(token.Text, out ParameterAttributeNames prmClass))
                {
                    paramClass = prmClass;
                    paramName = token.Text;
                }
                else
                {
                    AddErrorMessage($"Expecting output parameter name, one of: {string.Join(",", ValidOutputParamsDict.Keys)}",
                                    token);
                }
            }
            return paramClass;
        }

        private bool ParseParameterDeclaration()
        {
            ParameterAttributeNames paramClass = ParameterAttributeNames.@double;
            ParameterAttributeNames? outputParamClass = null;
            string parameterName = null;
            Token token = GetToken(tokenIndex++);
            if (token == null || token.TokenType == Token.TokenTypes.RightBrace)
                return false;
            Token parameterToken = token;
            if (token.Text == "@")
            {
                token = GetToken(tokenIndex++);
                if (token == null || token.TokenType != Token.TokenTypes.Identifier)
                {
                    AddErrorMessage("Expecting parameter name.", token);
                    return false;
                }
                parameterToken = token;
                parameterName = token.Text;
            }
            else
            {
                outputParamClass = TryParseOutputParameterDeclaration(token, out parameterName);
                if (outputParamClass == null)
                {
                    if (Success)
                        AddErrorMessage("Expecting @ParameterName or Output.", token);
                    return false;
                }
                else
                {
                    paramClass = (ParameterAttributeNames)outputParamClass;
                }
            }
            //if (IgnoreCase)
            //    parameterName = parameterName.ToLower();
            if (parameterDict.ContainsKey(parameterName))
            {
                AddErrorMessage($"The parameter {parameterName} is already declared.", token);
                return false;
            }
            int parameterCount = 1;
            bool hasAttributes = false;
            CustomParameterTypes customType = CustomParameterTypes.None;
            object defaultValue = null;
            object oAttrVal = null;
            //string varFunctionDefaultValue = null;
            //bool? boolDefaultValue = null;
            Token defaultValueToken = null;
            string label = null;
            List<ParameterChoice> choices = null;
            List<Type> varFunctionMethodTypes = null;
            HashSet<ParameterAttributeNames> validAttrs;
            if (outputParamClass == null)
                validAttrs = GetValidParamAttrs(paramClass);
            else
                validAttrs = new HashSet<ParameterAttributeNames>()
                { ParameterAttributeNames.@default, ParameterAttributeNames.visible, ParameterAttributeNames.label };
            Parameter parameter = null;
            bool visible = outputParamClass == null;  //Output parameters are hidden by default.
            while (Success)
            {
                token = GetToken(tokenIndex);
                if (token == null || token.TokenType != Token.TokenTypes.Identifier)
                    break;
                Token attributeToken = token;
                string attrNameText = token.Text;
                if (IgnoreCase)
                    attrNameText = attrNameText.ToLower();
                ParameterAttributeNames? attrNameOrNull = ParseParamAttr(attrNameText);
                if (attrNameOrNull == null)
                {
                    string attrList = string.Join(", ", 
                        Enum.GetNames(typeof(ParameterAttributeNames)));
                    AddErrorMessage($"Expecting parameter attribute, one of: {attrList}.", token);
                    return false;
                }
                ParameterAttributeNames attrName = (ParameterAttributeNames)attrNameOrNull;
                tokenIndex++;
                if (parameterTypeNames.Contains(attrName))
                {
                    if (paramClass != ParameterAttributeNames.@double || hasAttributes || outputParamClass != null)
                    {
                        AddErrorMessage($"The attribute {attributeToken.Text} of parameter @{parameterName} is not valid here.", 
                                        token);
                        return false;
                    }
                    else
                    {
                        paramClass = attrName;
                        validAttrs = GetValidParamAttrs(paramClass);
                        if (paramClass == ParameterAttributeNames.random)
                        {
                            paramClass = ParameterAttributeNames.custom;
                            customType = CustomParameterTypes.RandomRange;
                        }
                        else if (paramClass == ParameterAttributeNames.custom)
                        {
                            customType = CustomParameterTypes.None;
                            token = GetToken(tokenIndex++);
                            if (token != null && token.Text == ".")
                            {
                                token = GetToken(tokenIndex++);
                                if (token != null && token.TokenType == Token.TokenTypes.Identifier)
                                {
                                    customType = CustomParameter.ParseType(token.Text);
                                }
                            }
                            if (customType == CustomParameterTypes.None)
                            {
                                string customTypeList = string.Join(", ",
                                    (from CustomParameterTypes t in
                                         Enum.GetValues(typeof(CustomParameterTypes))
                                     where t != CustomParameterTypes.None
                                     select t).AsEnumerable());
                                AddErrorMessage(
                        $"Expecting '.' and custom parameter type, one of: {customTypeList}.", token);
                            }
                        }
                        continue;
                    }
                }
                else
                    hasAttributes = true;
                double attrValue = 0;
                if (!validAttrs.Contains(attrName))
                {
                    AddErrorMessage($"The attribute {attributeToken.Text} of parameter @{parameterName} is not valid here.",
                                    token);
                    return false;
                }
                //if (attrName == ParameterAttributeNames.parameters ||
                //    attrName == ParameterAttributeNames.domain)
                //{
                //    if (paramClass != ParameterAttributeNames.varfunction)
                //    {
                //        AddErrorMessage($"{attrNameText} is only valid for a VarFunction parameter.", attributeToken);
                //        break;
                //    }
                //}
                //else if (paramClass == ParameterAttributeNames.varfunction || paramClass == ParameterAttributeNames.boolean)
                //{
                //    if (attrName != ParameterAttributeNames.@default && attrName != ParameterAttributeNames.label)
                //    {
                //        if (!(paramClass == ParameterAttributeNames.varfunction && attrName == ParameterAttributeNames.choices))
                //        {
                //            AddErrorMessage($"{attrNameText} is not valid for parameter {parameterName}.", attributeToken);
                //            break;
                //        }
                //    }
                //}
                if (attrName == ParameterAttributeNames.choices)
                {
                    //if (!(paramClass == ParameterAttributeNames.@double || 
                    //      paramClass == ParameterAttributeNames.varfunction ||
                    //      paramClass == ParameterAttributeNames.complex))
                    //{
                    //    AddErrorMessage($"Choices are not valid for {paramClass} parameters.", attributeToken);
                    //    break;
                    //}
                    Type valueType = paramClass == ParameterAttributeNames.@double ? typeof(double) : 
                                     paramClass == ParameterAttributeNames.varfunction ? typeof(string) : 
                                     typeof(Complex);
                    choices = ParseParameterChoices(valueType);
                }
                else if (attrName != ParameterAttributeNames.locked &&
                         attrName != ParameterAttributeNames.integer)
                {
                    //Get = and value of attribute:
                    token = GetToken(tokenIndex++, required: true);
                    if (token?.Text != "=")
                    {
                        AddErrorMessage($"Expecting = after {attrNameText} for parameter.", token);
                        break;
                    }
                    Type dataType = attrName == ParameterAttributeNames.label ? typeof(string) : 
                                    attrName == ParameterAttributeNames.visible ? typeof(bool) :
                                    typeof(double);
                    if (attrName == ParameterAttributeNames.@default)
                    {
                        token = GetToken(tokenIndex, required: true);
                        if (token == null)
                            break;
                        defaultValueToken = token;
                        switch (paramClass)
                        {
                            case ParameterAttributeNames.boolean:
                                dataType = typeof(bool);
                                break;
                            case ParameterAttributeNames.complex:
                                dataType = typeof(Complex);
                                break;
                            case ParameterAttributeNames.varfunction:
                                dataType = typeof(string);
                                break;
                        }
                    }
                    if (attrName == ParameterAttributeNames.@default && paramClass == ParameterAttributeNames.varfunction)
                    {
                        oAttrVal = token.Text;
                        tokenIndex++;
                    }
                    else if (attrName == ParameterAttributeNames.domain)
                    {
                        token = GetToken(tokenIndex++, required: true);
                        if (token == null)
                            break;
                        Type methodType = GetTypeFromName(token.Text, out bool matchesForDeclaration, forDeclaration: false);
                        if (methodType != null)
                        {
                            varFunctionMethodTypes = new List<Type>();
                            varFunctionMethodTypes.Add(methodType);
                        }
                        else if (matchesForDeclaration)
                            AddErrorMessage("Expecting a type name.", token);
                        else
                            AddErrorMessage($"The type {token.Text} is not valid here.", token);
                    }
                    else
                    {
                        oAttrVal = ParseAndEvalConstantExpression(dataType);
                        if (oAttrVal is double)
                            attrValue = (double)oAttrVal;
                        else if (attrName == ParameterAttributeNames.label)
                            label = oAttrVal as string;
                        else if (attrName == ParameterAttributeNames.visible && oAttrVal is bool)
                            visible = (bool)oAttrVal;
                    }
                }
                if (paramClass == ParameterAttributeNames.@double && parameter == null)
                    parameter = new Parameter(parameterName, Expression);
                switch (attrName)
                {
                    case ParameterAttributeNames.@default:
                        defaultValue = oAttrVal;
                        break;
                    case ParameterAttributeNames.min:
                        parameter.MinValue = attrValue;
                        break;
                    case ParameterAttributeNames.max:
                        parameter.MaxValue = attrValue;
                        break;
                    case ParameterAttributeNames.decimals:
                        parameter.DecimalPlaces = (int)Math.Round(attrValue);
                        break;
                    case ParameterAttributeNames.improvmin:
                        parameter.ImprovMinValue = attrValue;
                        break;
                    case ParameterAttributeNames.improvmax:
                        parameter.ImprovMaxValue = attrValue;
                        break;
                    case ParameterAttributeNames.locked:
                        parameter.Locked = true;
                        break;
                    case ParameterAttributeNames.integer:
                        parameter.DecimalPlaces = 0;
                        break;
                    case ParameterAttributeNames.parameters:
                        parameterCount = (int)Math.Round(attrValue);
                        break;
                }
            }
            if (!Success)
                return false;
            if (token == null || token.TokenType != Token.TokenTypes.Semicolon)
            {
                AddErrorMessage("Expecting semicolon to end parameter declaration.", token);
                return false;
            }
            tokenIndex++;
            //else if (hasAttributes && paramClass != ParameterAttributeNames.@double && 
            //         paramClass != ParameterAttributeNames.varfunction && 
            //         paramClass != ParameterAttributeNames.boolean && 
            //         paramClass != ParameterAttributeNames.complex)
            //    AddErrorMessage("An array or custom parameter cannot have other attributes.", token);
            BaseParameter baseParameter;
            if (paramClass == ParameterAttributeNames.@double)
            {
                if (parameter == null)
                    parameter = new Parameter(parameterName, Expression);
                if (defaultValue is double)
                {
                    try
                    {
                        parameter.DefaultValue = (double)defaultValue;
                    }
                    catch (Exception ex)
                    {
                        AddErrorMessage(ex.Message, defaultValueToken);
                    }
                }
                baseParameter = parameter;
            }
            else if (paramClass == ParameterAttributeNames.array)
                baseParameter = new ArrayParameter(parameterName, Expression);
            else if (paramClass == ParameterAttributeNames.custom)
                baseParameter = new CustomParameter(parameterName, customType, Expression);
            else if (paramClass == ParameterAttributeNames.varfunction)
            {
                var varFunctionParameter = new VarFunctionParameter(parameterName, Expression);
                try
                {
                    varFunctionParameter.SetProperties(parameterCount, defaultValue as string, varFunctionMethodTypes);
                }
                catch (Exception ex)
                {
                    AddErrorMessage(ex.Message, defaultValueToken);
                }
                baseParameter = varFunctionParameter;
            }
            else if (paramClass == ParameterAttributeNames.boolean)
            {
                var boolParam = new BooleanParameter(parameterName, Expression);
                if (defaultValue != null)
                    boolParam.DefaultValue = (bool)defaultValue;
                baseParameter = boolParam;
            }
            else if (paramClass == ParameterAttributeNames.complex)
            {
                var complexParam = new ComplexParameter(parameterName, Expression);
                if (defaultValue is Complex)
                    complexParam.DefaultValue = (Complex)defaultValue;
                baseParameter = complexParam;
            }
            else if (paramClass == ParameterAttributeNames.influencepoint)
            {
                baseParameter = new GenericParameter<DoublePoint>(parameterName, Expression, GenericParameter<DoublePoint>.Categories.InfluencePoint);
            }
            else
                throw new Exception($"Unhandled paramClass value: {paramClass}.");
            if (label != null)
            {
                baseParameter.Label = label;
                visible = true;
            }
            if (choices != null)
            {
                foreach (var choice in choices)
                {
                    baseParameter.AddParameterChoice(choice.Value, choice.Text);
                }
            }
            baseParameter.IsOutputParameter = outputParamClass != null;
            baseParameter.Visible = visible;
            try
            {
                baseParameter.FinalizeParameter();
            }
            catch (Exception ex)
            {
                AddErrorMessage(ex.Message, parameterToken);
            }
            parameterDict.Add(baseParameter.ParameterName, baseParameter);
            return true;
        }

        private List<ParameterChoice> ParseParameterChoices(Type valueType)
        {
            List<ParameterChoice> choices = new List<ParameterChoice>();
            var token = GetToken(tokenIndex);
            if (token?.TokenType != Token.TokenTypes.LeftBrace)
            {
                AddErrorMessage("Expecting { following Choices keyword.", token);
                return choices;
            }
            tokenIndex++;
            while (true)
            {
                token = GetToken(tokenIndex++, required: true);
                if (token == null)
                    break;
                string choiceText = null;
                object choiceValue = null;
                if (token.TokenType == Token.TokenTypes.RightBrace)
                    break;
                else if (token.TokenType == Token.TokenTypes.Identifier)
                {
                    choiceText = token.Text;
                    token = GetToken(tokenIndex);
                }
                else if (token.TokenType == Token.TokenTypes.String)
                {
                    choiceText = ((StringToken)token).Value;
                    token = GetToken(tokenIndex);
                }
                bool parseValue = true;
                if (choiceText != null)
                {
                    if (token?.Text == "=")
                        tokenIndex++;
                    else
                        parseValue = false;
                }
                if (parseValue)
                    choiceValue = ParseAndEvalConstantExpression(valueType);
                if (Success)
                {
                    if (choiceText == null && choiceValue != null)
                        choiceText = choiceValue.ToString();
                    if (choiceText != null)
                    {
                        if (choices.Exists(ch => ch.Text == choiceText))
                        {
                            AddErrorMessage($"The choice {choiceText} is a duplicate.", token);
                        }
                        choices.Add(new ParameterChoice(choiceValue, choiceText));
                    }
                    token = GetToken(tokenIndex);
                }
                if (token != null && token.TokenType == Token.TokenTypes.Comma)
                    tokenIndex++;
                else if (token?.TokenType != Token.TokenTypes.RightBrace)
                {
                    AddErrorMessage($"Expecting ',' '=' or '}}' following parameter Choices value.", token);
                }
            }
            return choices;
        }

        private void ParseFunctionCall(FunctionToken functionToken, bool forConstant)
        {
            Token token = GetToken(tokenIndex, required: true);
            int argumentCount = 0;
            while (token != null)
            {
                if (token.TokenType == Token.TokenTypes.RightParen && argumentCount == 0)
                    break;
                ReservedWords? reservedWord = GetReservedWord(token);
                Token modifierToken = null;
                OperandModifiers modifier = OperandModifiers.None;
                if (reservedWord == ReservedWords.@ref)
                    modifier = OperandModifiers.RefParameter;
                else if (reservedWord == ReservedWords.parameter)
                    modifier = OperandModifiers.ParameterObject;
                if (modifier != OperandModifiers.None)
                {
                    modifierToken = token;
                    tokenIndex++;
                }
                ParseExpr(forConstant);  //Parse function argument.
                if (!Success)
                    break;
                Token topToken = PeekToken();
                OperandToken operandTok = topToken as OperandToken;  //Top token on expression stack.
                if (modifier == OperandModifiers.None)
                {
                    var methodInfo = functionToken.MemberInfo as MethodInfo;
                    if (methodInfo != null)
                    {
                        var methodParams = methodInfo.GetParameters();
                        if (argumentCount < methodParams.Length)
                        {
                            if (typeof(BaseParameter).IsAssignableFrom(methodParams[argumentCount].ParameterType))
                            {
                                modifier = OperandModifiers.ParameterObject;
                            }
                        }
                    }
                }
                if (modifier == OperandModifiers.RefParameter)
                {
                    if (operandTok == null ||
                        operandTok.OperandClass != IdentifierClasses.Variable ||
                        operandTok.OperandModifier == OperandModifiers.ArrayLength)
                    {
                        if (topToken?.Text == ".")
                        {
                            var dotToken = (OperatorToken)topToken;
                            dotToken.OperandModifier = OperandModifiers.RefParameter;
                        }
                        else
                        {
                            AddErrorMessage("Variable must follow ref keyword.", token);
                            break;
                        }
                    }
                    else if (operandTok.ValidIdentifier?.IsReadOnly == true)
                    {
                        AddErrorMessage(
                            $"The variable {operandTok.Text} is readonly, and cannot follow a ref keyword.", 
                            operandTok);
                    }
                }
                else if (modifier == OperandModifiers.ParameterObject)
                {
                    if (operandTok == null ||
                        operandTok.OperandClass != IdentifierClasses.Parameter ||
                        operandTok.OperandModifier == OperandModifiers.ArrayLength)
                    {
                        AddErrorMessage($"Parameter is required as argument for {functionToken.Text}.", token);
                        break;
                    }
                    else
                    {
                        operandTok.OperandClass = IdentifierClasses.ParameterObject;
                        operandTok.ReturnType = typeof(BaseParameter);
                        operandTok.OperandValue = operandTok.BaseParameter;
                    }
                }
                if (operandTok != null && modifier != OperandModifiers.None)
                {
                    if (functionToken.FunctionCategory == FunctionCategories.Custom)
                        AddErrorMessage($"Function {functionToken.Text} cannot use {modifierToken.Text} parameters.");
                    else
                        operandTok.OperandModifier = modifier;
                }
                argumentCount++;
                token = GetToken(tokenIndex);
                if (token != null && token.TokenType == Token.TokenTypes.Comma)
                {
                    //exprStackList.Add(token);
                    token = GetToken(++tokenIndex);
                }
                else
                    break;
            }
            if (token != null && token.TokenType == Token.TokenTypes.RightParen)
            {
                tokenIndex++;
                functionToken.FunctionArgumentCount = argumentCount;
                if (functionToken.ValidIdentifier != null)
                {
                    if (functionToken.RequiredFunctionParameterCount >= 0)
                    {
                        if (functionToken.RequiredFunctionParameterCount != argumentCount)
                        {
                            AddErrorMessage("The function '" + functionToken.Text + "' is declared with " +
                                functionToken.RequiredFunctionParameterCount.ToString() +
                                " parameters and called with " + argumentCount.ToString(), functionToken);
                        }
                    }
                    else
                    {
                        int minArgCount = -functionToken.RequiredFunctionParameterCount - 1;
                        if (argumentCount < minArgCount)
                        {
                            AddErrorMessage($"The function '{functionToken.Text}' must have at least {minArgCount} arguments.", functionToken);
                        }
                    }
                }
            }
            else
                AddErrorMessage("Expecting ')'", token);
        }

        private OperatorToken GetOperatorToken(Token token, bool isUnary)
        {
            OperatorToken opToken = null;
            if (validOperatorDict.TryGetValue(token.Text, out List<ValidOperator> validOps))
            {
                ValidOperator validOp = validOps.Find(x => x.IsUnary == isUnary);
                if (validOp != null)
                    opToken = new OperatorToken(token, validOp);
            }
            return opToken;
        }

        private List<ValidOperator> GetValidOperators(string opText, bool isUnary)
        {
            if (validOperatorDict.TryGetValue(opText, out List<ValidOperator> validOps))
            {
                validOps = validOps.FindAll(op => op.IsUnary == isUnary);
            }
            else
            {
                validOps = null;
            }
            return validOps;
        }

        private Token GetToken(int tokenInd, bool required = false)
        {
            Token token = null;
            if (tokenInd >= 0 && tokenInd < tokens.Count)
                token = tokens[tokenInd];
            else if (required)
                AddErrorMessage("Unexpected end of formula.");
            return token;
        }


        //private int stackIndex;

        public string ParenthesizeStatements()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                ParenthesizeStatementBlock(sb, null, 0);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Error: " + ex.Message);
            }
            return sb.ToString();
        }

        private string GetIndent(int level)
        {
            return string.Empty.PadRight(level * 3);
        }

        private void ParenthesizeStatementBlock(StringBuilder sb,
                                                ControlToken controlToken,
                                                int level)
        {
            int stackIndex;
            var expressionStacks = (controlToken == null ?
                Expression.ExpressionStacks : controlToken.ExpressionStacks).Select(es => es.ExpressionStack);
            string indent = GetIndent(level);
            if (controlToken != null)
            {
                //endInd = controlToken.EndStatementIndex;
                sb.Append(indent + controlToken.Text);
                if ((controlToken.ControlName == ReservedWords.@if ||
                     controlToken.ControlName == ReservedWords.@while) &&
                     expressionStacks.Any())
                {
                    expressionStack = expressionStacks.First();
                    stackIndex = expressionStack.Length - 1;
                    sb.Append(" (" + Parenthesize(ref stackIndex, topLevel: true) + ")");
                }
                sb.AppendLine(" {");
                indent = GetIndent(++level);
            }
            foreach (var exprStack in expressionStacks)
            {
                expressionStack = exprStack;
                stackIndex = expressionStack.Length - 1;
                ControlToken controlToken1 = expressionStack[stackIndex] as ControlToken;
                if (controlToken1 != null)
                {
                    if (controlToken1.ControlName == ReservedWords.@break)
                        sb.AppendLine(indent + controlToken1.Text + ";");
                    else
                    {
                        ParenthesizeStatementBlock(sb, controlToken1, level);
                        controlToken1 = controlToken1.ElseToken;
                        if (controlToken1 != null)
                        {
                            ParenthesizeStatementBlock(sb, controlToken1, level);
                        }
                    }
                }
                else
                {
                    sb.AppendLine(indent + Parenthesize(ref stackIndex, topLevel: true) + ";");
                }
            }
            if (controlToken != null)
            {
                indent = GetIndent(--level);
                sb.AppendLine(indent + "}");
            }
        }

        private string Parenthesize(ref int stackInd, bool topLevel = false)
        {
            string retVal = string.Empty;
            Token topToken = PopToken(ref stackInd);
            if (topToken is OperatorToken)
            {
                OperatorToken opToken = (OperatorToken)topToken;
                if (opToken.OperatorInfo.IsUnary)
                    retVal = opToken.Text + Parenthesize(ref stackInd);
                else
                {
                    string operand2 = Parenthesize(ref stackInd);
                    string operand1 = Parenthesize(ref stackInd);
                    retVal = operand1 + opToken.Text + operand2;
                    if (!topLevel)
                        retVal = "(" + retVal + ")";
                }
            }
            else if (topToken is BaseOperandToken)
            {
                BaseOperandToken operandTok = (BaseOperandToken)topToken;
                retVal = topToken.Text;
                if (operandTok.OperandClass == IdentifierClasses.Function)
                {
                    var functionToken = (FunctionToken)operandTok;
                    List<string> arguments = new List<string>();
                    for (int i = 1; i <= functionToken.FunctionArgumentCount; i++)
                    {
                        arguments.Insert(0, Parenthesize(ref stackInd));
                        if (i < functionToken.FunctionArgumentCount)
                            PopToken(ref stackInd);  //Pop comma.
                    }
                    retVal += "(" + string.Join(",", arguments) + ")";
                }
            }
            else if (topToken.IsLeftParen)
            {
                retVal = Parenthesize(ref stackInd);
            }
            return retVal;
        }
    }
}
