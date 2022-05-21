using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Whorl
{
    public class CSharpPreprocessor : ParserEngine.DocumentFormatter
    {
        public enum ParamCategories
        {
            Scalar,
            Enumerated,
            Function,
            ArrayLength,
            Random,
            InfluencePoint,
            NestedParameters
        }

        public enum ReservedWords
        {
            None,
            Function,
            CustomFunction,
            ParametersFile,
            Enum,
            Params,
            Instance,
            ParametersClass,
            SetProperty,
            Inherit,
            Influence,
            From,
            ArrayLength,
            Random,
            Previous,
            Base0,
            Include,
            InsertStart,
            InsertEnd,
            Namespace,
            Parameters,
            KeyParameters,
            KeyEnum,
            Exclusive,
            Global,
            Main,
            Update,
            Initialize,
            Eval,
            Append,
            Min,
            Max,
            Label,
            DistanceCount,
            True,
            False,
            NestedParameters
        }

        private enum MethodNames
        {
            Update,
            Initialize,
            Eval
        }

        public enum ElementTypes
        {
            NameSpace,
            Class,
            Method,
            Constructor
        }
        private enum FunctionTypes
        {
            Normal,
            Derivative,
            Custom
        }
        private const string ParametersPropertyName = "Parms";
        private const string ParmsPropertyAttributeName = "ParmsProperty";

        public abstract class BaseParamInfo
        {
            public ParamCategories Category { get; }
            public string ParameterName { get; }
            public string TypeName { get; set; }
            public int StartCharIndex { get; }
            public int EndCharIndex { get; set; }
            public string Initializer { get; protected set; }
            public string DecTypeName { get; protected set; }
            public string Label { get; set; }

            public BaseParamInfo(string name, string typeName, int startCharIndex, string initializer, 
                                 ParamCategories category)
            {
                ParameterName = name;
                TypeName = typeName ?? "double";
                StartCharIndex = startCharIndex;
                Initializer = initializer;
                Category = category;
            }
        }

        public class ParamInfo: BaseParamInfo
        {
            public string DefaultValue { get; }
            public string MinValue { get; set; }
            public string MaxValue { get; set; }
            public string SetProperty { get; set; }
            public int ArrayLength { get; set; } = -1;
            public string ArrayLengthParamName { get; set; }
            public ParameterSources ParameterSource { get; set; }
            public string PreviousName { get; set; }
            public int PreviousStartNumber { get; set; }
            public bool HasNestedParameters { get; protected set; }

            public string GetDecType()
            {
                string decType = DecTypeName;
                if (IsArray)
                    decType += "[]";
                return decType;
            }

            public string GetArrayLength()
            {
                return ArrayLength == -1 ? ArrayLengthParamName : ArrayLength.ToString();
            }

            public string GetArrayInitializer()
            {
                return $"new {DecTypeName}[{GetArrayLength()}]";
            }

            public bool IsArray
            {
                get
                {
                    return ArrayLength != -1 || !string.IsNullOrEmpty(ArrayLengthParamName);
                }
            }

            public ParamInfo(string name, string typeName, int startCharIndex,
                             string initializer = null, string defaultValue = null,
                             ParamCategories category = ParamCategories.Scalar):
                   base(name, typeName, startCharIndex, initializer, category)
            {
                DefaultValue = defaultValue;
            }

            public virtual void Complete()
            {
                switch (Category)
                {
                    case ParamCategories.Scalar:
                    case ParamCategories.ArrayLength:
                        DecTypeName = TypeName;
                        break;
                    case ParamCategories.Enumerated:
                        DecTypeName = $"EnumValuesParameter<{TypeName}>";
                        string arg;
                        if (DefaultValue == null)
                            arg = "null";
                        else if (DefaultValue.Contains('.'))
                            arg = DefaultValue;
                        else
                            arg = $"{TypeName}.{DefaultValue}";
                        Initializer = $"new {DecTypeName}({arg})";
                        break;
                    case ParamCategories.InfluencePoint:
                        DecTypeName = "OptionsParameter<InfluencePointInfo>";
                        Initializer = $"new {DecTypeName}(\"(none)\")";
                        break;
                    case ParamCategories.Random:
                        DecTypeName = nameof(RandomParameter);
                        Initializer = $"new {DecTypeName}()";
                        break;
                }
            }
        }

        private class FunctionParamInfo : ParamInfo
        {
            public int ParamsCount { get; }
            public Type MethodsClass { get; }
            public FunctionTypes FunctionType { get; }
            public MathFunctionTypes MathFunctionType { get; }
            public List<string> InstancePropertyNames { get; set; }
            public string ParametersClassName { get; set; }
            public bool AddDefaultMethodTypes { get; set; }

            public FunctionParamInfo(string name, string defaultMethodName, int paramsCount,
                                     string typeName, int startCharIndex,
                                     Type methodsClass, FunctionTypes functionType,
                                     MathFunctionTypes mathFunctionType) :
                base(name, typeName, startCharIndex, null, defaultMethodName, ParamCategories.Function)
            {
                ParamsCount = paramsCount;
                MethodsClass = methodsClass;
                FunctionType = functionType;
                MathFunctionType = mathFunctionType;
            }

            public override void Complete()
            {
                HasNestedParameters = ParametersClassName != null;
                string paramTypeName;
                string typeArgs = $"<{TypeName}>";
                if (FunctionType == FunctionTypes.Derivative)
                {
                    paramTypeName = "DerivFuncParameter";
                }
                else if (ParamsCount == 1 && (TypeName == "double" || TypeName == "Double"))
                {
                    paramTypeName = "DoubleFuncParameter";
                    typeArgs = string.Empty;
                }
                else
                {
                    paramTypeName = $"Func{ParamsCount}Parameter";
                }
                DecTypeName = $"{paramTypeName}{typeArgs}";
                List<string> args = new List<string>();
                args.Add(DefaultValue == null ? null : $"\"{DefaultValue}\"");
                args.Add(MethodsClass == null ? null : $"typeof({MethodsClass.FullName})");
                if (FunctionType == FunctionTypes.Normal)
                {
                    args.Add(MathFunctionType == MathFunctionTypes.Normal ? null :
                                $"{nameof(MathFunctionTypes)}.{MathFunctionType}");
                    if (InstancePropertyNames != null)
                    {
                        args.Add("instances: new object[] { " + string.Join(", ", InstancePropertyNames) + " }");
                    }
                    else if (ParametersClassName != null)
                    {
                        args.Add("instances: new object[] { " + $"new {ParametersClassName}()" + " }");
                    }
                    else
                        args.Add(null);
                    if (!AddDefaultMethodTypes)
                        args.Add("addDefaultMethodTypes: false");
                }
                int lastI = args.Count - 1;
                while (lastI >= 0 && args[lastI] == null)
                {
                    lastI--;
                }
                string allArgs = string.Join(", ", args.Take(lastI + 1).Select(s => s ?? "null"));
                Initializer = $"new {DecTypeName}({allArgs})";
            }
        }

        private class CustomFunctionParamInfo: ParamInfo
        {
            public string ParamsCodeFileName { get; set; }
            public string ParamsClassName { get; set; }

            public CustomFunctionParamInfo(string name, int startCharIndex, string defaultValue): 
                    base(name, "double", startCharIndex, null, defaultValue, ParamCategories.Function)
            {
                DecTypeName = nameof(CompiledDoubleFuncParameter);
            }

            public override void Complete()
            {
                var args = new List<string>();
                args.Add(TranslateToCSharp.GetCSharpString(ParameterName));
                HasNestedParameters = !string.IsNullOrEmpty(ParamsCodeFileName) ||
                                      !string.IsNullOrEmpty(ParamsClassName);
                if (HasNestedParameters)
                {
                    if (!string.IsNullOrEmpty(ParamsCodeFileName))
                        args.Add(TranslateToCSharp.GetCSharpString(ParamsCodeFileName));
                    else
                        args.Add($"typeof({ParamsClassName})");
                }
                if (DefaultValue != null)
                    args.Add(DefaultValue);
                Initializer = $"new {DecTypeName}({string.Join(", ", args)})";
            }
        }

        private class NestedParamsParamInfo: BaseParamInfo
        {
            public NestedParamsParamInfo(string name, string typeName, int startCharIndex)
                   : base(name, typeName, startCharIndex, initializer: null, ParamCategories.NestedParameters)
            {
            }
        }

        public class ElemDecInfo
        {
            public enum ElemTypes
            {
                Enum,
                Method
            }
            public ElemTypes ElemType { get; }
            public string Name { get; }
            public int StartCharIndex { get; }
            public int EndCharIndex { get; }
            public string FullName { get; }
            public int Occurrences { get; set; }

            public ElemDecInfo(ElemTypes elemType, string name, int startCharIndex, int endCharIndex,
                               Stack<ElementInfo> elemInfoStack)
            {
                ElemType = elemType;
                Name = name;
                StartCharIndex = startCharIndex;
                EndCharIndex = endCharIndex;
                FullName = string.Join(".", 
                    elemInfoStack.Reverse().Select(ei => ei.ElementName)
                                 .Concat(new string[] { name }));
            }
        }

        public class InsertDirectiveInfo
        {
            public ReservedWords Category { get; set; }
            public int StartCharIndex { get; set; }
            public int EndCharIndex { get; set; } = -1;
            public bool Append { get; set; }
        }

        public class ElementInfo
        {
            public string ElementName { get; set; }
            public ElementTypes ElementType { get; set; }
            public int Nesting { get; set; }
            public int StartCharIndex { get; set; } = -1;
            public int EndCharIndex { get; set; } = -1;
            public int CodePartsIndex { get; set; }
            public int InsertCharIndex { get; set; }
            public int AppendCharIndex { get; set; }
            public ElementInfo Parent { get; set; }

            public override string ToString()
            {
                return $"{ElementName}: {ElementType} [{Nesting}]";
            }
        }

        /// <summary>
        /// Class for KeyParameters info.
        /// </summary>
        private class ParamsClassInfo
        {
            public string ClassName { get; }
            public Dictionary<string, BaseParamInfo> ParamsDict { get; }

            public ParamsClassInfo(string className)
            {
                ClassName = className;
                ParamsDict = CreateDictionary<BaseParamInfo>();
            }
        }

        private enum TokenDirectives
        {
            None,
            Directive,
            ParameterDec,
            ParameterRef,
            EscapedChar
        }
        private static Lazy<CSharpPreprocessor> lazy = new Lazy<CSharpPreprocessor>(() => new CSharpPreprocessor());
        public static CSharpPreprocessor Instance
        {
            get { return lazy.Value; }
        }

        public List<ErrorInfo> ErrorMessages { get; } = new List<ErrorInfo>();
        public List<string> Warnings { get; } = new List<string>();

        private const string directivePattern = "@@";
        private const string parameterDecPattern = "@#";
        private const string parameterRefPattern = "@";
        private const string colon = ":";

        //private Trie tokenTrie { get; }
        private Type infoType { get; set; }
        private Tokenizer tokenizer { get; }
        private Dictionary<string, ReservedWords> reservedWordsDict { get; }
        private Dictionary<string, FunctionTypes> functionTypesDict { get; }
        public Dictionary<string, BaseParamInfo> ParamsDict { get; }
        public Dictionary<string, ElemDecInfo> ElemDecsDict { get; }
        private Dictionary<string, Type> methodClassesDict { get; }
        //private Dictionary<ReservedWords, string> keptCodeDict { get; } = new Dictionary<ReservedWords, string>();
        private string parametersClassName { get; set; }
        private string mainClassName { get; set; }
        private string sourceCode { get; set; }
        private List<string> newCodeParts { get; } = new List<string>();
        private List<Token> codeTokens { get; set; }
        private int codeCharIndex { get; set; }
        private int nesting { get; set; }
        private int bracketLevel { get; set; }

        private List<ElementInfo> elemInfoList { get; } = new List<ElementInfo>();
        private Stack<ElementInfo> elemInfoStack { get; } = new Stack<ElementInfo>();
        private ElementInfo currentBaseClassInfo { get; set; }
        private List<InsertDirectiveInfo> InsertDirectives { get; } = new List<InsertDirectiveInfo>();
        private InsertDirectiveInfo insertDirectiveInfo { get; set; }
        private static readonly Token nullToken = new Token(string.Empty, Token.TokenTypes.EndOfStream, 0);

        public IEnumerable<InsertDirectiveInfo> GetInsertDirectiveInfos()
        {
            return InsertDirectives;
        }

        private void AddError(Token token, string message)
        {
            var errInfo = new ErrorInfo(token ?? nullToken, message);
            ErrorMessages.Add(errInfo);
        }

        private void AddError(string message)
        {
            AddError(nullToken, message);
        }

        public CSharpPreprocessor()
        {
            tokenizer = new Tokenizer(forCSharp: true, addOperators: false);
            Trie tokenTrie = tokenizer.TokenTrie;
            tokenTrie.AddToken("[", Token.TokenTypes.LeftBracket);
            tokenTrie.AddToken("]", Token.TokenTypes.RightBracket);
            tokenTrie.AddToken(directivePattern, Token.TokenTypes.Custom);
            tokenTrie.AddToken(parameterDecPattern, Token.TokenTypes.Custom);
            tokenTrie.AddToken(parameterRefPattern, Token.TokenTypes.Custom);
            //tokenTrie.AddToken("//", Token.TokenTypes.LineComment);
            //tokenTrie.AddToken("/*", Token.TokenTypes.OpenComment);
            //tokenTrie.AddToken("*/", Token.TokenTypes.CloseComment);
            //tokenizer.Initialize(tokenTrie, checkCommentChar: false, closeCommentText: "*/");
            reservedWordsDict = Tools.GetCaseInsensitiveEnumDictionary<ReservedWords>();
            functionTypesDict = Tools.GetCaseInsensitiveEnumDictionary<FunctionTypes>();
            ParamsDict = CreateDictionary<BaseParamInfo>();
            //Case sensitive dictionary:
            ElemDecsDict = new Dictionary<string, ElemDecInfo>();
            methodClassesDict = CreateDictionary<Type>();
            methodClassesDict.Add("Outlines", typeof(OutlineMethods));
            methodClassesDict.Add(nameof(OutlineMethods), typeof(OutlineMethods));
            methodClassesDict.Add("Complex", typeof(Complex));
            methodClassesDict.Add("ComplexMethods", typeof(Complex));
        }

        private void Initialize(bool initial = true)
        {
            newCodeParts.Clear();
            elemInfoList.Clear();
            elemInfoStack.Clear();
            //keptCodeDict.Clear();
            codeCharIndex = 0;
            nesting = 0;
            insertDirectiveInfo = null;
            currentBaseClassInfo = null;
            if (initial)
            {
                ParamsDict.Clear();
                ElemDecsDict.Clear();
                ErrorMessages.Clear();
                Warnings.Clear();
                parametersClassName = null;
                InsertDirectives.Clear();
            }
        }

        public string Preprocess(string code, Type infoType = null)
        {
            this.infoType = infoType;
            code = ProcessIncludes(code);
            string newCode = PreprocessCode(code);
            Initialize(initial: false); //Free memory.
            return newCode;
        }

        private string ProcessIncludes(string code)
        {
            ErrorMessages.Clear();
            Warnings.Clear();
            int charIndex = 0;

            codeTokens = tokenizer.TokenizeExpression(code);
            var sbCode = new StringBuilder();
            int i = 0;
            while (i < codeTokens.Count)
            {
                Token token = codeTokens[i++];
                if (token.TokenType == Token.TokenTypes.Custom && 
                    GetTokenDirective(token, i) == TokenDirectives.Directive)
                {
                    Token directiveToken = GetToken(i++);
                    if (ParseReservedWord(directiveToken.Text, ReservedWords.Include))
                    {
                        string includeCode = ProcessIncludeDirective(ref i);
                        if (includeCode != null)
                        {
                            sbCode.Append(code.Substring(charIndex, token.CharIndex - charIndex));
                            sbCode.Append(includeCode);
                            token = GetToken(i - 1);
                            charIndex = token.CharIndex + token.Text.Length;
                        }
                    }
                }
            }
            if (sbCode.Length == 0)
                return code;
            if (charIndex < code.Length)
                sbCode.Append(code.Substring(charIndex));
            return sbCode.ToString();
        }

        private string PreprocessCode(string code)
        {
            Initialize();
            sourceCode = code;
            codeTokens = tokenizer.TokenizeExpression(code);
            int tokenIndex = 0;
            bool checkForMethod = true;
            bool foundParmsAttribute = false;
            bool requireClassDec = false;
            var parameterRefClasses = new HashSet<string>();
            bracketLevel = 0;
            ReservedWords classDecDirectiveWord = ReservedWords.None;
            //var keyParamClassInfos = new List<ParamsClassInfo>();
            ParamsClassInfo currentParamClassInfo = null;
            string className = null;
            bool classDecError = false;

            while (tokenIndex < codeTokens.Count)
            {
                int savedTokenIndex = tokenIndex;
                Token tok = codeTokens[tokenIndex++];
                if (IsCSharpReservedWord(tok, savedTokenIndex, "enum"))
                {
                    if (requireClassDec)
                        classDecError = true;
                    Token enumStartTok = tok;
                    if (savedTokenIndex > 0)
                    {
                        Token modTok = codeTokens[savedTokenIndex - 1];
                        if (IsAccessToken(modTok, savedTokenIndex - 1))
                        {
                            enumStartTok = modTok;
                        }
                    }
                    Token enumNameTok = GetToken(tokenIndex++);
                    if (enumNameTok.TokenType != Token.TokenTypes.Identifier)
                        AddError(enumNameTok, "Expecting enum name.");
                    else
                    {
                        Token enumEndTok = FindNextToken(ref tokenIndex, tkn => tkn.Text == "}");
                        if (enumEndTok != null)
                        {
                            var enumDecInfo = new ElemDecInfo(
                                ElemDecInfo.ElemTypes.Enum,
                                enumNameTok.Text, enumStartTok.CharIndex,
                                enumEndTok.CharIndex + enumEndTok.Text.Length,
                                elemInfoStack);
                            if (ElemDecsDict.ContainsKey(enumDecInfo.FullName))
                                AddError(enumNameTok, $"The enum {enumDecInfo.FullName} is a duplicate.");
                            else
                                ElemDecsDict.Add(enumDecInfo.FullName, enumDecInfo);
                            tokenIndex++;
                            savedTokenIndex = tokenIndex;
                        }
                    }
                    tok = GetToken(tokenIndex++);
                }
                if (infoType != null && tok.TokenType == Token.TokenTypes.Identifier &&
                    tok.Text == "Info" && tokenIndex < codeTokens.Count - 1)
                {
                    if (requireClassDec)
                        classDecError = true;
                    if (codeTokens[tokenIndex].Text == ".")
                    {
                        Token identTok = codeTokens[tokenIndex + 1];
                        if (identTok.TokenType == Token.TokenTypes.Identifier)
                        {
                            if (infoType.GetMember(identTok.Text).Length == 0)
                            {
                                var memberInfos = infoType.GetMember(identTok.Text,
                                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                                if (memberInfos.Any())
                                {
                                    newCodeParts.Add(sourceCode.Substring(codeCharIndex, 
                                                     identTok.CharIndex - codeCharIndex));
                                    newCodeParts.Add(memberInfos.First().Name);
                                    codeCharIndex = identTok.CharIndex + identTok.Text.Length;
                                }
                            }
                        }
                    }
                }
                bool handled = tok.TokenType == Token.TokenTypes.LeftBracket ||
                               tok.TokenType == Token.TokenTypes.RightBracket;
                if (handled)
                {
                    if (requireClassDec)
                        classDecError = true;
                    if (tok.TokenType == Token.TokenTypes.LeftBracket)
                    {
                        bracketLevel++;
                    }
                    else //if (tok.TokenType == Token.TokenTypes.RightBracket)
                    {
                        //if (bracketLevel == 0)
                        //    AddError(tok, "Extra ']' found.");
                        //else
                        if (bracketLevel > 0)
                            bracketLevel--;
                    }
                }
                else if (tok.TokenType == Token.TokenTypes.Custom)
                {
                    TokenDirectives tokenDirective = GetTokenDirective(tok, savedTokenIndex);
                    if (tokenDirective != TokenDirectives.None)
                    {
                        handled = true;
                        if (requireClassDec)
                            classDecError = true;
                        Token lastTok;
                        if (tokenDirective == TokenDirectives.EscapedChar)
                        {
                            lastTok = codeTokens[savedTokenIndex - 1];
                            newCodeParts.Add(sourceCode.Substring(codeCharIndex, lastTok.CharIndex - codeCharIndex));
                            codeCharIndex = lastTok.CharIndex + lastTok.Text.Length;
                        }
                        else
                        {
                            newCodeParts.Add(sourceCode.Substring(codeCharIndex, tok.CharIndex - codeCharIndex));
                            if (bracketLevel > 0 && tokenDirective != TokenDirectives.ParameterRef)
                            {
                                AddError($"The directive {tok.Text} is not allowed within [].");
                            }
                            else
                            {
                                bool isValid;
                                switch (tokenDirective)
                                {
                                    case TokenDirectives.Directive:
                                        isValid = ProcessDirective(ref tokenIndex, out ReservedWords directive);
                                        if (isValid)
                                        {
                                            if (directive == ReservedWords.KeyParameters || directive == ReservedWords.NestedParameters)
                                            {
                                                classDecDirectiveWord = directive;
                                                requireClassDec = true;
                                                className = null;
                                                classDecError = false;
                                                if (classDecDirectiveWord == ReservedWords.NestedParameters)
                                                {
                                                    newCodeParts.Add("[NestedParameters]");
                                                }
                                            }
                                        }
                                        break;
                                    case TokenDirectives.ParameterDec:
                                        isValid = ProcessParameterDec(ref tokenIndex, tok,
                                                  currentParamClassInfo != null ? currentParamClassInfo.ParamsDict :  ParamsDict);
                                        break;
                                    case TokenDirectives.ParameterRef:
                                        isValid = ProcessParameterRef(ref tokenIndex);
                                        if (isValid)
                                            parameterRefClasses.Add(GetCurrentClassName());
                                        break;
                                    default:
                                        throw new Exception($"Invalid directive encountered: {tokenDirective}.");
                                }
                                if (isValid)
                                {
                                    lastTok = codeTokens[tokenIndex - 1];
                                    codeCharIndex = lastTok.CharIndex + lastTok.Text.Length;
                                }
                            }
                        }
                    }
                }
                if (!handled && bracketLevel == 0)
                {
                    int tokInd = tokenIndex - 1;
                    handled = ProcessNonDirectiveTokens(tok, ref tokInd, classDecDirectiveWord, ref checkForMethod, 
                                                        ref foundParmsAttribute, out ElementInfo poppedElemInfo,
                                                        out className);
                    if (handled)
                    {
                        tokenIndex = tokInd + 1;
                        if (requireClassDec)
                        {
                            if (className != null)
                            {
                                currentParamClassInfo = new ParamsClassInfo(className);
                                //keyParamClassInfos.Add(currentKeyParamClassInfo);
                                classDecDirectiveWord = ReservedWords.None;
                                classDecError = false;
                            }
                            else
                                classDecError = true;
                            requireClassDec = false;
                        }
                        if (poppedElemInfo != null && poppedElemInfo.ElementType == ElementTypes.Class)
                        {
                            if (currentParamClassInfo != null && currentParamClassInfo.ClassName == poppedElemInfo.ElementName)
                            {
                                currentParamClassInfo = null;
                            }
                        }
                    }
                }
                if (classDecError)
                {
                    AddError(tok, "Expecting: public class <className> following " + GetDirectiveText(classDecDirectiveWord));
                    classDecDirectiveWord = ReservedWords.None;
                    classDecError = requireClassDec = false;
                }
            }   //while (tokenIndex < codeTokens.Count)
            if (codeCharIndex < sourceCode.Length)
                newCodeParts.Add(sourceCode.Substring(codeCharIndex));
            if (parameterRefClasses.Any())
            {
                if (mainClassName == null)
                    AddError(null, "The main class does not have a Parms property, so @ parameter references are not allowed.");
                else
                {
                    if (parameterRefClasses.Count > 1 || !parameterRefClasses.Contains(mainClassName))
                    {
                        AddError(null, "Only the main class can have @ parameter references.");
                    }
                }
            }
            if (parametersClassName != null)
            {
                ElementInfo paramsClassInfo = elemInfoList.Find(
                    ei => ei.ElementType == ElementTypes.Class && ei.ElementName == parametersClassName);
                if (paramsClassInfo == null)
                    throw new Exception($"Couldn't retrieve info for class {parametersClassName}");
                ProcessUpdateMethod(paramsClassInfo);
                ProcessParametersConstructor(paramsClassInfo);
            }
            else if (ParamsDict.Any())
            {
                AddError(null, "Couldn't find parameters class name.");
            }
            if (InsertDirectives.Any(dir =>
                                 dir.EndCharIndex < dir.StartCharIndex || dir.EndCharIndex > code.Length - 1))
            {
                AddError("Some @@InsertStart directives did not have @@InsertEnd directives.");
            }
            return string.Concat(newCodeParts);
        }

        private class MergeInsertInfo
        {
            public int TargetStartCharIndex { get; set; }
            public string CodePart { get; set; }
        }

        public string MergeModule(string code, string moduleCode)
        {
            Initialize();
            var moduleProcessor = new CSharpPreprocessor();
            moduleProcessor.Preprocess(moduleCode);
            if (moduleProcessor.ErrorMessages.Any())
            {
                ErrorMessages.AddRange(moduleProcessor.ErrorMessages);
                return code;
            }
            else if (!moduleProcessor.InsertDirectives.Any())
            {
                Warnings.Add("No @@InsertStart directives were found in the module to merge.");
                return code;
            }
            PreprocessCode(code);
            newCodeParts.Clear();  //Free memory.
            if (ErrorMessages.Any())
            {
                return code;
            }
            var duplicateEnumInfos = new List<ElemDecInfo>();
            foreach (var enumInfo in moduleProcessor.ElemDecsDict.Values)
            {
                if (moduleProcessor.InsertDirectives.Exists(
                            dir => dir.StartCharIndex <= enumInfo.StartCharIndex &&
                                   enumInfo.EndCharIndex <= dir.EndCharIndex))
                {
                    if (ElemDecsDict.TryGetValue(enumInfo.FullName, out ElemDecInfo dupEnumInfo))
                    {
                        duplicateEnumInfos.Add(dupEnumInfo);
                    }
                }
            }
            duplicateEnumInfos = duplicateEnumInfos.OrderBy(ed => ed.StartCharIndex).ToList();
            var sb = new StringBuilder();
            int targetCharIndex = 0;
            var mergeInfoList = GetMergeInsertInfoList(moduleProcessor, moduleCode);
            foreach (var mergeInfo in mergeInfoList.OrderBy(mi => mi.TargetStartCharIndex))
            {
                AppendCode(sb, code, targetCharIndex, mergeInfo.TargetStartCharIndex, 
                           duplicateEnumInfos);
                //sb.Append(code.Substring(targetCharIndex, mergeInfo.TargetStartCharIndex - targetCharIndex));
                sb.Append(mergeInfo.CodePart);
                targetCharIndex = mergeInfo.TargetStartCharIndex;
            }
            if (targetCharIndex < code.Length)
            {
                AppendCode(sb, code, targetCharIndex, code.Length, duplicateEnumInfos);
                //sb.Append(code.Substring(targetCharIndex));
            }
            Initialize(initial: false); //Free memory.
            //Remove blank lines:
            return Regex.Replace(sb.ToString(), @"^\s+$[\r\n]*", 
                                 string.Empty, RegexOptions.Multiline);
        }

        private void AppendCode(StringBuilder sb, string code, int startInd, int endInd, 
                                List<ElemDecInfo> duplicateEnumInfos)
        {
            int startI = startInd;
            foreach (ElemDecInfo dupEnumInfo in duplicateEnumInfos)
            {
                if (dupEnumInfo.StartCharIndex >= startI && dupEnumInfo.EndCharIndex <= endInd)
                {
                    //Remove merged enum from target code:
                    sb.Append(code.Substring(startI, dupEnumInfo.StartCharIndex - startI));
                    startI = dupEnumInfo.EndCharIndex;
                }
            }
            if (startI < endInd)
            {
                sb.Append(code.Substring(startI, endInd - startI));
            }
        }

        private void AppendModuleCode(StringBuilder sb, string moduleCode,
                                      int startInd, int endInd,
                                      List<BaseParamInfo> duplicateParamInfos)
        {
            int startI = startInd;
            foreach (ParamInfo dupParamInfo in duplicateParamInfos)
            {
                if (dupParamInfo.StartCharIndex >= startI && dupParamInfo.EndCharIndex <= endInd)
                {
                    //Do not copy duplicate merged parameter from module:
                    sb.Append(moduleCode.Substring(startI, dupParamInfo.StartCharIndex - startI));
                    startI = dupParamInfo.EndCharIndex;
                }
            }
            if (startI < endInd)
            {
                sb.Append(moduleCode.Substring(startI, endInd - startI));
            }
        }

        private List<MergeInsertInfo> GetMergeInsertInfoList(
                        CSharpPreprocessor moduleProcessor, string moduleCode)
        {
            var insertDirectives = moduleProcessor.InsertDirectives;
            var mergeInfoList = new List<MergeInsertInfo>();
            var duplicateParamInfos = new List<BaseParamInfo>();
            foreach (var paramInfo in moduleProcessor.ParamsDict.Values)
            {
                if (insertDirectives.Exists(dir => dir.StartCharIndex <= paramInfo.StartCharIndex &&
                                                   paramInfo.EndCharIndex <= dir.EndCharIndex))
                {
                    if (ParamsDict.ContainsKey(paramInfo.ParameterName))
                    {
                        duplicateParamInfos.Add(paramInfo);
                    }
                }
            }
            duplicateParamInfos = duplicateParamInfos.OrderBy(
                                  pi => pi.StartCharIndex).ToList();
            ElementInfo paramClassInfo = elemInfoList.Find(
                         ei => ei.ElementType == ElementTypes.Class && 
                         ei.ElementName == parametersClassName);
            ElementInfo mainClassInfo = elemInfoList.Find(
                         ei => ei.ElementType == ElementTypes.Class && 
                         ei.ElementName == mainClassName);
            foreach (ReservedWords category in insertDirectives.Select(dir => dir.Category).Distinct())
            {
                bool createMethod = false;  //True if should insert complete code for method.
                string methodName = null;
                ElementInfo elementInfo = null;
                switch (category)
                {
                    case ReservedWords.Namespace:
                        elementInfo = elemInfoList.Find(ei => ei.ElementType == ElementTypes.NameSpace);
                        break;
                    case ReservedWords.Parameters:
                        elementInfo = paramClassInfo;
                        break;
                    case ReservedWords.Main:
                        elementInfo = mainClassInfo;
                        break;
                    case ReservedWords.Update:
                    case ReservedWords.Initialize:
                    case ReservedWords.Eval:
                        methodName = category.ToString();
                        ElementInfo parentClassInfo = category == ReservedWords.Update ? 
                                    paramClassInfo : mainClassInfo;
                        if (parentClassInfo != null)
                        {
                            elementInfo = elemInfoList.Find(
                                          ei => ei.ElementType == ElementTypes.Method &&
                                                ei.ElementName == methodName &&
                                                ei.Parent == parentClassInfo);
                            if (elementInfo == null)
                            {
                                elementInfo = parentClassInfo;
                                createMethod = true;
                            }
                        }
                        break;
                }
                if (elementInfo == null)
                {
                    AddError($"Couldn't process InsertStart directive for {category}.");
                    continue;
                }
                bool[] appendRange;
                if (createMethod)
                    appendRange = new bool[] { false };
                else
                    appendRange = new bool[] { false, true };
                foreach (bool append in appendRange)
                {
                    var catInserts = insertDirectives.Where(
                        dir => dir.Category == category && (dir.Append == append || createMethod))
                        .OrderBy(dir => dir.StartCharIndex);
                    if (catInserts.Any())
                    {
                        var sb = new StringBuilder();
                        foreach (var catIns in catInserts)
                        {
                            AppendModuleCode(sb, moduleCode, catIns.StartCharIndex, 
                                             catIns.EndCharIndex, duplicateParamInfos);
                        }
                        var mergeInfo = new MergeInsertInfo()
                        {
                            CodePart = sb.ToString()
                        };
                        if (createMethod)
                        {
                            mergeInfo.CodePart = 
$@"public void {methodName}()
{{
    {mergeInfo.CodePart}
}}
";
                        }
                        if (append)
                            mergeInfo.TargetStartCharIndex = elementInfo.AppendCharIndex;
                        else
                            mergeInfo.TargetStartCharIndex = elementInfo.InsertCharIndex;
                        mergeInfoList.Add(mergeInfo);
                    }
                }
            }
            return mergeInfoList;
        }

        private string GetCurrentClassName()
        {
            string className = string.Empty;
            if (elemInfoStack.Any())
            {
                ElementInfo elementInfo = elemInfoStack.Peek();
                if (elementInfo.ElementType == ElementTypes.Class)
                    className = elementInfo.ElementName;
                else if (elementInfo.Parent != null)
                    className = elementInfo.Parent.ElementName;
            }
            return className;
        }

        private void CheckToken(Token tok, ref bool checkForMethod, out ElementInfo poppedElemInfo)
        {
            poppedElemInfo = null;
            if (tok.Text == "{")
            {
                if (elemInfoStack.Any())
                {
                    ElementInfo elementInfo = elemInfoStack.Peek();
                    if (elementInfo.Nesting == nesting)
                    {
                        elementInfo.InsertCharIndex = tok.CharIndex + tok.Text.Length;
                        if (elementInfo.ElementType == ElementTypes.Class)
                            checkForMethod = true;
                    }
                }
                nesting++;
            }
            else if (tok.Text == "}")
            {
                nesting--;
                if (elemInfoStack.Any())
                {
                    ElementInfo elementInfo = elemInfoStack.Peek();
                    if (elementInfo.Nesting == nesting)
                    {
                        //string elemName = elementInfo.ElementName;
                        poppedElemInfo = elementInfo;
                        elemInfoStack.Pop();
                        if (elementInfo == currentBaseClassInfo)
                            currentBaseClassInfo = null;
                        else if (currentBaseClassInfo != null &&
                                 elementInfo.ElementType == ElementTypes.Class)
                        {
                            if (currentBaseClassInfo.Nesting == nesting - 1)
                                currentBaseClassInfo.InsertCharIndex = tok.CharIndex + 1;
                        }
                        newCodeParts.Add(sourceCode.Substring(codeCharIndex,
                                         tok.CharIndex - codeCharIndex));
                        codeCharIndex = tok.CharIndex;
                        elementInfo.CodePartsIndex = newCodeParts.Count;
                        elementInfo.AppendCharIndex = tok.CharIndex;
                        elementInfo.EndCharIndex = tok.CharIndex + tok.Text.Length;
                        if (elementInfo.ElementType == ElementTypes.Method ||
                            elementInfo.ElementType == ElementTypes.Constructor)
                        {
                            checkForMethod = true;
                            if (elementInfo.ElementType == ElementTypes.Method &&
                                elementInfo.StartCharIndex != -1 &&
                                elementInfo.EndCharIndex != -1)
                            {
                                var elemDecInfo = new ElemDecInfo(
                                    ElemDecInfo.ElemTypes.Method,
                                    elementInfo.ElementName,
                                    elementInfo.StartCharIndex,
                                    elementInfo.EndCharIndex,
                                    elemInfoStack);
                                if (ElemDecsDict.TryGetValue(elemDecInfo.FullName,
                                                             out var currInfo))
                                {
                                    currInfo.Occurrences++;
                                }
                                else
                                {
                                    ElemDecsDict[elemDecInfo.FullName] = elemDecInfo;
                                }
                            }
                        }
                    }
                }
            }
        }

        private Token CheckGetToken(int tokenIndex, out ElementInfo poppedElemInfo, bool checkBrackets = true)
        {
            bool check = false;
            Token tok = GetToken(tokenIndex);
            if (tok.TokenType == Token.TokenTypes.Other)
            {
                CheckToken(tok, ref check, out poppedElemInfo);
            }
            else
            {
                poppedElemInfo = null;
                if (checkBrackets)
                {
                    if (tok.TokenType == Token.TokenTypes.LeftBracket)
                        bracketLevel++;
                    else if (tok.TokenType == Token.TokenTypes.RightBracket)
                        bracketLevel--;
                }
            }
            return tok;
        }

        private bool ProcessNonDirectiveTokens(Token tok, ref int tokInd, ReservedWords getClassDirective, ref bool checkForMethod, 
                                               ref bool foundParmsAttribute, out ElementInfo poppedElemInfo, out string className)
        {
            bool handled = true;
            Token startToken = null;
            Token accessToken = null;
            int accessTokenInd = -1;
            poppedElemInfo = null;
            className = null;
            if (tok.TokenType == Token.TokenTypes.Other)
            {
                CheckToken(tok, ref checkForMethod, out poppedElemInfo);
                if (tok.Text == "=") // || tok.Text == "(")
                    checkForMethod = false;
                else if (tok.Text == ";")
                    checkForMethod = true;
                return handled;
                //else if (tok.Text == "[")
                //{
                //    int level = 1;
                //    while (level != 0)
                //    {
                //        Token nextTok = FindNextToken(ref tokenIndex, t => t.Text == "]" || t.Text == "[");
                //        if (nextTok != null)
                //        {
                //            tokenIndex++;
                //            if (nextTok.Text == "]")
                //                level--;
                //            else
                //                level++;
                //        }
                //        else
                //            break;
                //    }
                //}
            }
            if (!elemInfoStack.Any() && IsCSharpReservedWord(tok, tokInd, "using"))
            {
                tokInd++;
                FindNextToken(ref tokInd, tkn => tkn.Text == ";");
                return handled;
            }
            if (IsAccessToken(tok, tokInd))
            {
                accessToken = tok;
                accessTokenInd = tokInd;
                startToken = tok;
                checkForMethod = true;
                tok = CheckGetToken(++tokInd, out _);
            }
            if (IsCSharpReservedWord(tok, tokInd, "static"))
            {
                tok = CheckGetToken(++tokInd, out _);
                checkForMethod = true;
                if (startToken == null)
                    startToken = tok;
            }
            if (IsCSharpReservedWord(tok, tokInd, "class") ||
                IsCSharpReservedWord(tok, tokInd, "namespace"))
            {
                if (startToken == null)
                    startToken = tok;
                ElementTypes elementType = tok.Text == "class" ?
                             ElementTypes.Class : ElementTypes.NameSpace;
                tok = CheckGetToken(++tokInd, out _);
                if (tok.TokenType == Token.TokenTypes.Identifier)
                {
                    var elemInfo = new ElementInfo()
                    {
                        ElementName = tok.Text,
                        ElementType = elementType,
                        Nesting = nesting
                    };
                    if (startToken != null)
                        elemInfo.StartCharIndex = startToken.CharIndex;
                    if (elementType == ElementTypes.Class)
                    {
                        if (!elemInfoStack.Any() || elemInfoStack.Peek().ElementType ==
                                                    ElementTypes.NameSpace)
                            currentBaseClassInfo = elemInfo;
                        if (getClassDirective != ReservedWords.None)
                        {
                            if (accessToken?.Text == "public")
                            {
                                className = elemInfo.ElementName;
                            }
                            if (getClassDirective == ReservedWords.KeyParameters &&
                                (!elemInfoStack.Any() || elemInfoStack.Peek() != currentBaseClassInfo))
                            {
                                AddError(tok, "Key Parameters class must be nested within the main formula class.");
                            }
                        }
                    }
                    elemInfoList.Add(elemInfo);
                    elemInfoStack.Push(elemInfo);
                }
            }
            else if (!elemInfoStack.Any())
            {
                if (tok.TokenType == Token.TokenTypes.Identifier && ParamsDict.Any())
                {
                    AddError(tok, "Element stack is empty.");
                }
            }
            else if (tok.TokenType == Token.TokenTypes.Identifier &&
                     elemInfoStack.Peek().ElementType == ElementTypes.Class)
            {
                int tokI = tokInd;
                bool? isType = TryParseType(ref tokI, out List<Token> typeTokens);
                if (isType == false || typeTokens.Count == 0)
                    return false;
                if (startToken == null)
                    startToken = typeTokens.First();
                Token nameTok = null;
                int saveNesting = nesting;
                int saveBracket = bracketLevel;
                tok = CheckGetToken(tokI, out _);
                ElementInfo classInfo = elemInfoStack.Peek();
                ElementTypes elementType = ElementTypes.Method;
                bool haveType = tok.TokenType == Token.TokenTypes.Identifier;
                if (haveType)
                {
                    nameTok = tok;
                    tok = GetToken(++tokI);
                }
                else if (isType == null)
                {
                    nameTok = typeTokens.First();
                    if (nameTok.Text == classInfo.ElementName)
                    {
                        elementType = ElementTypes.Constructor;
                    }
                    else
                        handled = false;
                }
                else
                    handled = false;
                if (handled)
                {
                    if (tok.Text == "(")
                    {
                        int i = tokI + 1;
                        Token braceTok = FindNextToken(ref i, t => t.Text == "{" || t.Text == ";");
                        if (braceTok?.Text == "{")
                        {
                            var info = new ElementInfo()
                            {
                                ElementName = nameTok.Text,
                                ElementType = elementType,
                                Nesting = nesting,
                                Parent = classInfo
                            };
                            if (startToken != null)
                                info.StartCharIndex = startToken.CharIndex;
                            elemInfoList.Add(info);
                            elemInfoStack.Push(info);
                            tokInd = tokI;
                        }
                        else
                            handled = false;
                    }
                    else if (tok.Text == "{")
                    {
                        int propNesting = nesting;
                        nesting++;
                        Token braceToken = tok;
                        tok = GetToken(++tokI);
                        if (tok.Text == "get")
                        {
                            if (!foundParmsAttribute && nameTok.Text == ParametersPropertyName)
                            {
                                this.parametersClassName = typeTokens.Last().Text;
                                this.mainClassName = classInfo.ElementName;
                                if (accessToken?.Text != "public")
                                {
                                    AddError(accessToken,
                                        $"The {ParametersPropertyName} property must be public.");
                                }
                                if (accessToken != null)
                                {
                                    int i = accessTokenInd - 1;
                                    if (MatchPrevTokens(ref i, "[", ParmsPropertyAttributeName, "]"))
                                        foundParmsAttribute = true;
                                }
                            }
                            tokI++;
                            while (nesting > propNesting)
                            {
                                tok = CheckGetToken(tokI++, out _);
                                if (tok.TokenType == Token.TokenTypes.EndOfStream)
                                    break;
                            }
                            if (nesting == propNesting)
                                tokInd = tokI;
                            else
                            {
                                AddError(braceToken, "Closing '}' was not found.");
                            }
                        }
                        else
                            handled = false;
                    }
                    //int tokI = tokInd + 1;
                    //if (MatchNextTokens(ref tokI, "{", "get"))
                    //{
                    //    tokI = tokInd - 2;
                    //    if (IsCSharpReservedWord(codeTokens[tokI], tokI, "public"))
                    //    {
                    //        this.parametersClassName = tok.Text;
                    //        this.mainClassName = classInfo.ElementName;
                    //        tokI--;
                    //        if (MatchPrevTokens(ref tokI, "[", ParmsPropertyAttributeName, "]"))
                    //            foundParmsAttribute = true;
                    //    }
                    //}

                }
                else
                    handled = false;
                if (!handled)
                {
                    nesting = saveNesting;
                    bracketLevel = saveBracket;
                }

                //else if (checkForMethod && tok.TokenType == Token.TokenTypes.Identifier &&
                //         !IsAccessToken(tok, indexOfTok) &&
                //         elemInfoStack.Any() && elemInfoStack.Peek().ElementType == ElementTypes.Class)
                //{
                //    ElementInfo classInfo = elemInfoStack.Peek();
                //    ElementTypes elementType;
                //    Token nextTok = CheckGetToken(tokenIndex);
                //    if (nextTok.Text == "(")
                //    {
                //        int prevI = tokenIndex - 2;
                //        tokenIndex++;
                //        //if (Enum.TryParse(tok.Text, out MethodNames methodName))
                //        //{
                //        //    if (IsCSharpReservedWord(codeTokens[prevI], prevI, "void"))
                //        //        addElement = true;
                //        //}
                //        Token prevTok = codeTokens[prevI];
                //        bool addElement = true;
                //        if (tok.Text == classInfo.ElementName)
                //        {
                //            elementType = ElementTypes.Constructor;
                //            if (IsCSharpReservedWord(prevTok, prevI, "new"))
                //                addElement = false;
                //        }
                //        else
                //        {
                //            elementType = ElementTypes.Method;
                //        }
                //        if (addElement)
                //        {
                //            int tokI = tokenIndex;
                //            Token braceTok = FindNextToken(ref tokI, t => t.Text == "{" || t.Text == ";");
                //            if (braceTok?.Text == "{")
                //            {
                //                var info = new ElementInfo()
                //                {
                //                    ElementName = tok.Text,
                //                    ElementType = elementType,
                //                    Nesting = nesting,
                //                    Parent = classInfo
                //                };
                //                elemInfoList.Add(info);
                //                elemInfoStack.Push(info);
                //            }
                //        }
                //    }
            }
            else
                handled = false;
            return handled;
        }

        private bool IsCSharpReservedWord(Token token, int tokenIndex, string word)
        {
            bool isReserved = false;
            if (token.TokenType == Token.TokenTypes.Identifier && token.Text == word)
            {
                if (tokenIndex == 0 || codeTokens[tokenIndex - 1].Text != "@")
                    isReserved = true;
            }
            return isReserved;
        }

        private bool MatchNextTokens(ref int tokenIndex, params string[] texts)
        {
            int i = 0;
            while (i < texts.Length && tokenIndex < codeTokens.Count && 
                   codeTokens[tokenIndex].Text == texts[i])
            {
                tokenIndex++;
                i++;
            }
            return i == texts.Length;
        }

        private bool MatchPrevTokens(ref int tokenIndex, params string[] texts)
        {
            int i = texts.Length - 1;
            while (i >= 0 && tokenIndex >= 0 && codeTokens[tokenIndex].Text == texts[i])
            {
                tokenIndex--;
                i--;
            }
            return i == -1;
        }

        private Token FindNextToken(ref int tokenIndex, Predicate<Token> predicate)
        {
            Token foundTok = null;
            while (tokenIndex < codeTokens.Count)
            {
                Token tok = codeTokens[tokenIndex];
                if (predicate(tok))
                {
                    foundTok = tok;
                    break;
                }
                tokenIndex++;
            }
            return foundTok;
        }

        private string GetDirectiveText(ReservedWords directive)
        {
            return $"{directivePattern}{directive}";
        }

        private string ProcessIncludeDirective(ref int tokenIndex)
        {
            string includeCode = null;
            Token token = GetToken(tokenIndex++);
            if (token.TokenType == Token.TokenTypes.String)
            {
                string formulaName = ((StringToken)token).Value;
                var formulaEntry = MainForm.PatternChoices.FormulaEntryList.GetFormulaByName(formulaName);
                if (formulaEntry != null && formulaEntry.FormulaUsage == FormulaUsages.Include)
                {
                    TryParseTokenText(ref tokenIndex, ";");  //Optional semicolon.
                    includeCode = Environment.NewLine + formulaEntry.Formula + Environment.NewLine;
                }
                else
                {
                    AddError(token, $"Did not find include formula names {formulaName}.");
                }
            }
            else
            {
                AddError(token, $"Expecting quoted formula name following {GetDirectiveText(ReservedWords.Include)}.");
            }
            return includeCode;
        }

        private bool ProcessDirective(ref int tokenIndex, out ReservedWords directive)
        {
            Token directiveToken = GetToken(tokenIndex - 1); //"@@" token.
            Token token = GetToken(tokenIndex);
            bool isValid = false;
            if (ParseReservedWord(token.Text, out directive,
                                  vws => AddError(token, "Expecting one of: " + string.Join(", ", vws.Select(rw => GetDirectiveText(rw)))),
                                  ReservedWords.InsertStart, ReservedWords.InsertEnd, ReservedWords.KeyParameters,
                                  ReservedWords.KeyEnum, ReservedWords.NestedParameters))
            {
                tokenIndex++;
                switch (directive)
                {
                    case ReservedWords.KeyParameters:
                    case ReservedWords.NestedParameters:
                        isValid = true;
                        break;
                    case ReservedWords.InsertStart:
                        if (this.insertDirectiveInfo != null)
                            AddError(token, $"Cannot nest {GetDirectiveText(ReservedWords.InsertStart)} directives.");
                        else
                            isValid = ProcessInsertStart(ref tokenIndex);
                        break;
                    case ReservedWords.InsertEnd:
                        if (this.insertDirectiveInfo == null)
                            AddError(token,
                                $"No previous {GetDirectiveText(ReservedWords.InsertStart)} for {GetDirectiveText(ReservedWords.InsertEnd)}.");
                        else
                        {
                            this.insertDirectiveInfo.EndCharIndex = directiveToken.CharIndex;
                            isValid = true;
                            this.insertDirectiveInfo = null;  //Was already added to list.
                            TryParseTokenText(ref tokenIndex, ";");  //Optional semicolon.
                        }
                        break;
                    case ReservedWords.KeyEnum:
                        token = GetToken(tokenIndex++);
                        bool exclusive = true, global = true;
                        if (token.TokenType == Token.TokenTypes.Identifier)
                        {
                            string className = token.Text;
                            token = GetToken(tokenIndex);
                            isValid = true;
                            var validWords = new HashSet<ReservedWords>() { ReservedWords.Exclusive, ReservedWords.Global };
                            while (ParseReservedWordOption(token.Text, out ReservedWords word, validWords))
                            {
                                tokenIndex++;
                                if (TryParseTokenText(ref tokenIndex, "=") != null)
                                {
                                    token = GetToken(tokenIndex++);
                                    if (ParseReservedWord(token.Text, out var trueOrFalse, ReservedWords.True, ReservedWords.False))
                                    {
                                        if (word == ReservedWords.Exclusive)
                                            exclusive = trueOrFalse == ReservedWords.True;
                                        else
                                            global = trueOrFalse == ReservedWords.True;
                                    }
                                    else
                                    {
                                        AddError(token, $"Expecting True or False following {word} = .");
                                        isValid = false;
                                    }
                                }
                                else
                                {
                                    AddError(token, $"Expecting = following {word}.");
                                    isValid = false;
                                }
                            }
                            if (isValid)
                            {
                                newCodeParts.Add($"[KeyEnum(nameof({className}), {exclusive.ToString().ToLower()}, {global.ToString().ToLower()})]");
                            }
                        }
                        else
                        {
                            AddError(token, "Expecting parameters class name following " + GetDirectiveText(directive) + ".");
                        }
                        break;
                }
            }
            return isValid;
        }

        private Token TryParseTokenText(ref int tokenIndex, string text)
        {
            Token token = GetToken(tokenIndex);
            if (token.Text == text)
                tokenIndex++;
            else
                token = null;
            return token;
        }

        private bool ProcessInsertStart(ref int tokenIndex)
        {
            Token token;
            if (!GetSpecificToken(tokenIndex, out token, ":"))
                return false;
            token = GetToken(++tokenIndex);
            var validCategories = new ReservedWords[] {
                    ReservedWords.Namespace, ReservedWords.Parameters, ReservedWords.Main,
                    ReservedWords.Update, ReservedWords.Initialize, ReservedWords.Eval };
            if (!ParseReservedWord(token.Text, out ReservedWords category,
                vws => AddError(token, $"Expecting {GetDirectiveText(ReservedWords.InsertStart)} category, one of: " + 
                                       $"{string.Join(", ", vws)}"), 
                validCategories))
            {
                //AddError(token, $"Expecting {GetDirectiveText(ReservedWords.InsertStart)} category, one of: {string.Join(",", validCategories)}");
                return false;
            }
            token = GetToken(++tokenIndex);
            bool append = ParseReservedWord(token.Text, ReservedWords.Append);
            if (append)
            {
                token = GetToken(++tokenIndex);
                if (token.TokenType == Token.TokenTypes.EndOfStream)
                    return false;
            }
            if (token.Text == ";")
                ++tokenIndex;
            token = GetToken(tokenIndex - 1);
            this.insertDirectiveInfo = new InsertDirectiveInfo()
            {
                Category = category,
                StartCharIndex = token.CharIndex + token.Text.Length,
                Append = append
            };
            this.InsertDirectives.Add(insertDirectiveInfo);
            return true;
        }

        private List<string> ParseInstancePropertyNames(ref int tokenIndex)
        {
            var propNames = new List<string>();
            Token tok = GetToken(tokenIndex);
            if (tok.Text != ":")
            {
                propNames.Add("this");
                return propNames;
            }
            tokenIndex++;
            while (true)
            {
                tok = GetToken(tokenIndex);
                if (tok.TokenType == Token.TokenTypes.Identifier)
                {
                    propNames.Add(tok.Text);
                    tokenIndex++;
                }
                else
                {
                    AddError(tok, "Expecting instance property name, following 'Instance:'.");
                    return null;
                }
                tok = GetToken(tokenIndex);
                if (tok.Text == ",")
                    tokenIndex++;
                else
                    break;
            }
            return propNames;
        }

        private bool ProcessParameterDec(ref int tokenIndex, Token directiveToken, Dictionary<string, BaseParamInfo> parametersDict)
        {
            ParamCategories paramCategory = ParamCategories.Scalar;
            ParamInfo paramInfo = null;
            int arrayLength = -1;
            string arrayLengthParamName = null;
            string typeName = null;
            string initializer = null;
            string defaultValue = null;
            string minValue = null;
            string maxValue = null;
            string previousName = null;
            string label = null;
            string setPropertyName = null;
            ParameterSources parameterSource = ParameterSources.None;
            bool isValid;
            bool isNestedParamsParam = false;
            int previousStartNumber = 1;
            bool allowMinMax = true;
            bool allowArrayParam = parametersDict == ParamsDict;
            int prevErrorCount = ErrorMessages.Count;
            var validWords = new HashSet<ReservedWords>()
            {
                ReservedWords.ArrayLength,
                ReservedWords.Previous,
                ReservedWords.Enum,
                ReservedWords.CustomFunction,
                ReservedWords.Function,
                ReservedWords.Influence,
                ReservedWords.Random,
                ReservedWords.Min,
                ReservedWords.Max,
                ReservedWords.Label,
                ReservedWords.DistanceCount,
                ReservedWords.SetProperty
            };
            int startCharIndex = directiveToken.CharIndex;
            Token nameTok = GetToken(tokenIndex++);
            if (nameTok.Text == "@")
            {
                nameTok = GetToken(tokenIndex++);
                if (!ParseReservedWord(nameTok.Text, out _, ReservedWords.NestedParameters))
                {
                    AddError(nameTok, "Expecting @NestedParameters.");
                    return false;
                }
                isNestedParamsParam = true;
                nameTok = GetToken(tokenIndex++);
            }
            if (nameTok.TokenType != Token.TokenTypes.Identifier)
            {
                AddError(nameTok, "Expecting name of parameter.");
                return false;
            }
            string parameterName = nameTok.Text;
            Token tok = GetToken(tokenIndex);
            if (isNestedParamsParam)
            {
                isValid = false;
                if (tok.Text == ":")
                {
                    tokenIndex++;
                    tok = GetToken(tokenIndex++);
                    if (tok.TokenType == Token.TokenTypes.Identifier)
                    {
                        typeName = tok.Text;
                        isValid = true;
                    }
                }
                if (!isValid)
                {
                    AddError(nameTok, "Expecting <Name>: <NestedParametersClassname> = <NestedParametersObject>");
                }
            }
            else
            {
                if (tok.TokenType == Token.TokenTypes.LeftBracket)
                {
                    if (!allowArrayParam)
                    {
                        AddError(tok, "Other parameter classes cannot have array parameters.");
                    }
                    allowMinMax = false;
                    tokenIndex++;
                    isValid = false;
                    tok = GetToken(tokenIndex++);
                    if (tok.TokenType == Token.TokenTypes.Number)
                    {
                        if (int.TryParse(tok.Text, out arrayLength))
                            isValid = true;
                    }
                    else if (tok.TokenType == Token.TokenTypes.Identifier)
                    {
                        if (parametersDict.TryGetValue(tok.Text, out BaseParamInfo pInfo))
                        {
                            if (pInfo.Category == ParamCategories.ArrayLength)
                            {
                                arrayLengthParamName = pInfo.ParameterName;
                                isValid = true;
                            }
                        }
                        if (!isValid)
                            AddError(tok, $"The ArrayLength parameter {tok.Text} is undefined.");
                    }
                    if (isValid)
                    {
                        tok = GetToken(tokenIndex++);
                        if (tok.TokenType != Token.TokenTypes.RightBracket)
                            isValid = false;
                        else
                        {
                            tok = GetToken(tokenIndex);
                            validWords.Remove(ReservedWords.ArrayLength);
                        }
                    }
                    if (!isValid)
                    {
                        AddError(nameTok, "Invalid array declaration.");
                        return false;
                    }
                    if (!allowArrayParam)
                        return false;
                }
                if (tok.Text == colon)
                {
                    tokenIndex++;
                    typeName = ParseType(ref tokenIndex, "Expecting type of parameter.");
                    if (typeName == null)
                        return false;
                    tok = GetToken(tokenIndex);
                }
                isValid = true;
                while (ParseReservedWordOption(tok.Text, out ReservedWords reservedWord, validWords))
                {
                    tokenIndex++;
                    switch (reservedWord)
                    {
                        case ReservedWords.ArrayLength:
                        case ReservedWords.DistanceCount:
                            if (typeName == null)
                                typeName = "int";
                            paramCategory = ParamCategories.ArrayLength;
                            if (reservedWord == ReservedWords.DistanceCount)
                                parameterSource = ParameterSources.DistanceCount;
                            break;
                        case ReservedWords.Previous:
                            previousName = parameterName;
                            tok = GetToken(tokenIndex);
                            if (tok.Text == ":")
                            {
                                tok = GetToken(++tokenIndex);
                                if (tok.TokenType == Token.TokenTypes.Identifier)
                                {
                                    previousName = tok.Text;
                                    tok = GetToken(++tokenIndex);
                                }
                                else
                                {
                                    AddError(tok, "Expecting an unquoted identifier for previous name.");
                                    return false;
                                }
                            }
                            if (ParseReservedWord(tok.Text, ReservedWords.Base0))
                            {
                                previousStartNumber = 0;
                                tokenIndex++;
                            }
                            break;
                        case ReservedWords.CustomFunction:
                            paramInfo = ParseCustomFunctionParam(nameTok, ref tokenIndex, directiveToken);
                            break;
                        case ReservedWords.Enum:
                        case ReservedWords.Function:
                        case ReservedWords.Random:
                        case ReservedWords.Influence:
                            allowMinMax = false;
                            validWords.RemoveWhere(w => w == ReservedWords.Enum ||
                                                        w == ReservedWords.Function ||
                                                        w == ReservedWords.Influence ||
                                                        w == ReservedWords.Random);
                            switch (reservedWord)
                            {
                                case ReservedWords.Enum:
                                    isValid = typeName == null;
                                    paramCategory = ParamCategories.Enumerated;
                                    typeName = ParseType(ref tokenIndex, "Expecting enumerated type.", allowGeneric: false);
                                    if (typeName == null)
                                        return false;
                                    break;
                                case ReservedWords.Function:
                                    paramCategory = ParamCategories.Function;
                                    paramInfo = ParseFunctionParam(nameTok, ref tokenIndex, typeName, directiveToken);
                                    if (paramInfo == null)
                                        return false;
                                    break;
                                case ReservedWords.Influence:
                                    paramCategory = ParamCategories.InfluencePoint;
                                    break;
                                case ReservedWords.Random:
                                    isValid = typeName == null;
                                    paramCategory = ParamCategories.Random;
                                    paramInfo = new ParamInfo(parameterName, null, startCharIndex, category: paramCategory);
                                    break;
                            }
                            break;
                        case ReservedWords.Min:
                        case ReservedWords.Max:
                            if (!GetSpecificToken(tokenIndex, out tok, "="))
                                break;
                            string sVal;
                            tok = GetToken(++tokenIndex);
                            if (tok.Text == "-")
                            {
                                sVal = "-";
                                tok = GetToken(++tokenIndex);
                            }
                            else
                                sVal = string.Empty;
                            if (tok.TokenType == Token.TokenTypes.Number)
                            {
                                sVal += tok.Text;
                                if (reservedWord == ReservedWords.Min)
                                    minValue = sVal;
                                else
                                    maxValue = sVal;
                                tokenIndex++;
                            }
                            else
                                AddError($"Expecting number for {reservedWord}.");
                            break;
                        case ReservedWords.Label:
                            if (!GetSpecificToken(tokenIndex, out tok, "="))
                                break;
                            var stringToken = GetToken(++tokenIndex) as StringToken;
                            if (stringToken == null)
                                AddError("Expecting quoted string for label.");
                            else
                                label = stringToken.Value;
                            tokenIndex++;
                            break;
                        case ReservedWords.SetProperty:
                            bool setPropertyValid = true;
                            Token setPropertyTok = tok;
                            tok = GetToken(tokenIndex++);
                            setPropertyValid = tok.Text == ":";
                            if (setPropertyValid)
                            {
                                tok = GetToken(tokenIndex++);
                                setPropertyValid = tok.TokenType == Token.TokenTypes.Identifier;
                                if (setPropertyValid)
                                {
                                    string instanceName = tok.Text;
                                    tok = GetToken(tokenIndex++);
                                    setPropertyValid = tok.Text == ".";
                                    if (setPropertyValid)
                                    {
                                        tok = GetToken(tokenIndex++);
                                        setPropertyValid = tok.TokenType == Token.TokenTypes.Identifier;
                                        if (setPropertyValid)
                                        {
                                            setPropertyName = $"{instanceName}.{tok.Text}";
                                        }
                                    }
                                }
                            }
                            if (!setPropertyValid)
                            {
                                AddError(setPropertyTok, "Expecting 'SetProperty: InstancePropertyName.PropertyName'.");
                            }
                            break;
                    }
                    tok = GetToken(tokenIndex);
                }
                if (!isValid)
                    AddError(tok, $"The token {tok.Text} is invalid here.");
                else if (!allowMinMax && (minValue != null || maxValue != null))
                    AddError(nameTok, "Min or Max are invalid here.");
                isValid = true;
                if (tok.Text == "=")
                {
                    if (paramInfo != null)
                        isValid = false;
                    else
                    {
                        tokenIndex++;
                        if (paramCategory == ParamCategories.Enumerated)
                        {
                            defaultValue = ParseType(ref tokenIndex, "Expecting enumerated value.", allowGeneric: false);
                        }
                        else
                        {
                            //var initTokens = new List<Token>();
                            int startInd = -1;
                            while (tokenIndex < codeTokens.Count)
                            {
                                tok = codeTokens[tokenIndex];
                                if (tok.Text == ";")
                                {
                                    if (startInd == -1)
                                        AddError(tok, "Expecting initializer expression.");
                                    else
                                    {
                                        int endInd = tok.CharIndex;
                                        if (setPropertyName == null)
                                        {
                                            initializer = sourceCode.Substring(startInd, endInd - startInd);
                                        }
                                        else
                                        {
                                            AddError(tok, "Properties with SetProperty specified cannot have initializers.");
                                        }
                                    }
                                    break;
                                }
                                else if (tok.TokenType == Token.TokenTypes.Custom ||
                                         tok.TokenType == Token.TokenTypes.EndOfStream ||
                                         tok.Text == "}")
                                {
                                    isValid = false;
                                    break;
                                }
                                if (startInd == -1)
                                    startInd = tok.CharIndex;
                                //initTokens.Add(tok);
                                tokenIndex++;
                            }
                            //initializer = string.Join(string.Empty, initTokens.Select(t => t.Text));
                        }
                    }
                }
            }
            isValid = ErrorMessages.Count == prevErrorCount;
            tok = GetToken(tokenIndex);
            if (tok.Text == ";")
                tokenIndex++;
            else if (isValid)
            {
                isValid = false;
                AddError(tok, $"The token {tok.Text} is invalid here (expecting ';').");
            }
            if (!isValid)
                return false;
            BaseParamInfo baseParamInfo;
            string paramCode;
            if (isNestedParamsParam)
            {
                var nestedParamsInfo = new NestedParamsParamInfo(parameterName, typeName, startCharIndex);
                paramCode = GetParameterCode(nestedParamsInfo);
                baseParamInfo = nestedParamsInfo;
            }
            else
            {
                if (paramInfo == null)
                    paramInfo = new ParamInfo(parameterName, typeName, startCharIndex,
                                              initializer, defaultValue, paramCategory);
                paramInfo.ArrayLength = arrayLength;
                paramInfo.ArrayLengthParamName = arrayLengthParamName;
                paramInfo.ParameterSource = parameterSource;
                paramInfo.PreviousName = previousName;
                paramInfo.PreviousStartNumber = previousStartNumber;
                paramInfo.Label = label;
                paramInfo.MinValue = minValue;
                paramInfo.MaxValue = maxValue;
                paramInfo.SetProperty = setPropertyName;
                if (paramInfo.IsArray && paramCategory == ParamCategories.ArrayLength)
                {
                    AddError(nameTok, "ArrayLength parameters cannot be arrays.");
                    return false;
                }
                paramInfo.Complete();
                paramCode = GetParameterCode(paramInfo);
                baseParamInfo = paramInfo;
            }
            baseParamInfo.EndCharIndex = tok.CharIndex + tok.Text.Length;
            if (ErrorMessages.Count == prevErrorCount)
            {
                newCodeParts.Add(paramCode);
                if (parametersDict.ContainsKey(parameterName))
                {
                    AddError(nameTok, $"{parameterName} is a duplicate parameter name.");
                }
                else
                {
                    parametersDict.Add(parameterName, baseParamInfo);
                }
            }
            return ErrorMessages.Count == prevErrorCount;
        }

        private string ParseType(ref int tokenIndex, string errMsg, bool allowGeneric = true)
        {
            int startIndex = tokenIndex;
            var parts = new List<string>();
            while (true)
            {
                Token tok = GetToken(tokenIndex++);
                if (tok.TokenType != Token.TokenTypes.Identifier)
                {
                    AddError(tok, errMsg);
                    return null;
                }
                parts.Add(tok.Text);
                tok = GetToken(tokenIndex);
                if (tok.Text != ".")
                {
                    if (tok.Text == "<")
                    {   //Parse generic arguments.
                        parts.Add(tok.Text);
                        tokenIndex++;
                        while (tok.Text != ">")
                        {
                            string subtype = ParseType(ref tokenIndex, errMsg, allowGeneric: true);
                            if (subtype == null)
                                return null;
                            else
                            {
                                tok = GetToken(tokenIndex);
                                if (tok.Text != ">" && tok.Text != ",")
                                {
                                    AddError(tok, "Expecting '>' or ',' for generic type.");
                                    return null;
                                }
                                parts.Add(subtype);
                                tokenIndex++;
                                parts.Add(tok.Text);
                            }
                        }
                    }
                    break;
                }
                else
                {
                    parts.Add(tok.Text);
                    tokenIndex++;
                }
            }
            return string.Concat(parts);
        }

        private bool? TryParseType(ref int tokenIndex, out List<Token> tokens)
        {
            tokens = new List<Token>();
            int tokInd = tokenIndex;
            bool? isType = TryParseType(tokens, ref tokInd, forGeneric: false);
            if (isType != false)
                tokenIndex = tokInd;
            return isType;
        }

        private bool? TryParseType(List<Token> tokens, ref int tokInd,
                                   bool forGeneric)
        {
            bool? isType = null;
            while (true)
            {
                Token tok = GetToken(tokInd);
                if (tok.TokenType != Token.TokenTypes.Identifier ||
                    tok.Text == "enum" || tok.Text == "new")
                {
                    isType = false;
                    break;
                }
                tokens.Add(tok);
                tok = GetToken(++tokInd);
                if (tok.Text == ".")
                {
                    isType = true;
                    tokInd++;
                }
                else if (tok.Text == "<")
                {
                    tokens.Add(tok);
                    isType = true;
                    tokInd++;
                    while (true)
                    {
                        if (TryParseType(tokens, ref tokInd, forGeneric: true) == false)
                        {
                            isType = false;
                            break;
                        }
                        tok = GetToken(tokInd);
                        if (tok.Text == ",")
                            tokens.Add(tok);
                        else
                        {
                            if (tok.Text == ">")
                            {
                                tokens.Add(tok);
                                tokInd++;
                            }
                            else
                                isType = false;
                            break;
                        }
                    }
                    break;
                }
                else
                    break;
                tokens.Add(tok);
            }
            return isType;
        }

        private CustomFunctionParamInfo ParseCustomFunctionParam(Token nameTok, ref int tokenIndex, Token directiveToken)
        {
            string paramsFileName = null, paramsClassName = null, defaultValue = null;
            Token token = GetToken(tokenIndex);
            if (ParseReservedWord(token.Text, out ReservedWords reservedWord, 
                                  ReservedWords.ParametersFile, ReservedWords.ParametersClass))
            {
                token = GetToken(++tokenIndex);
                if (token.Text != "=")
                {
                    AddError(token, $"Expecting '=' after {reservedWord}.");
                    return null;
                }
                tokenIndex++;
                if (reservedWord == ReservedWords.ParametersFile)
                {
                    token = GetToken(tokenIndex);
                    if (token.TokenType != Token.TokenTypes.String)
                    {
                        AddError(token, "Expecting quoted file name after ParametersFile = .");
                        return null;
                    }
                    paramsFileName = ((StringToken)token).Value;
                    string filePath = CompiledDoubleFuncParameter.GetParamsFilePath(paramsFileName);
                    if (!System.IO.File.Exists(filePath))
                    {
                        AddError(token, $"Parameters File not found: {filePath}.");
                        return null;
                    }
                    tokenIndex++;
                }
                else //if (reservedWord == ReservedWords.ParametersClass)
                {
                    if (TryParseType(ref tokenIndex, out List<Token> typeTokens) == false)
                    { 
                        AddError(token, "Expecting name of parameters class after ParametersClass = .");
                        return null;
                    }
                    paramsClassName = string.Concat(typeTokens.Select(t => t.Text));
                }
            }
            token = GetToken(tokenIndex);
            if (token.Text == "=")
            {
                tokenIndex++;
                token = GetToken(tokenIndex++);
                if (token.TokenType != Token.TokenTypes.String)
                {
                    AddError(token, "Expecting quoted formula following =");
                    return null;
                }
                defaultValue = TranslateToCSharp.GetCSharpString(((StringToken)token).Value);
            }
            var paramInfo = new CustomFunctionParamInfo(nameTok.Text, directiveToken.CharIndex, defaultValue);
            paramInfo.ParamsCodeFileName = paramsFileName;
            paramInfo.ParamsClassName = paramsClassName;
            return paramInfo;
        }

        private FunctionParamInfo ParseFunctionParam(Token nameTok, ref int tokenIndex, string typeName,
                                                     Token directiveToken)
        {
            var reservedWords = new HashSet<ReservedWords>() { ReservedWords.From, ReservedWords.Params, ReservedWords.Instance, 
                                                               ReservedWords.ParametersClass, ReservedWords.Inherit };

            Token defaultMethod = null, paramsTok = null;
            int paramsCount = 1;
            Type methodsClass = null;
            var mathType = MathFunctionTypes.Normal;
            FunctionTypes functionType = FunctionTypes.Normal;
            List<string> instancePropertyNames = null;
            string paramClassName = null;
            bool? inherit = null;
            Token tok;
            while (true)
            {
                tok = GetToken(tokenIndex);
                if (tok.TokenType != Token.TokenTypes.Identifier)
                    break;
                tokenIndex++;
                bool isValid = true;
                if (ParseReservedWordOption(tok.Text, out ReservedWords reservedWord, reservedWords))
                {
                    switch (reservedWord)
                    {
                        case ReservedWords.From:
                            tok = GetToken(tokenIndex++);
                            if (tok.TokenType == Token.TokenTypes.Identifier)
                            {
                                if (!methodClassesDict.TryGetValue(tok.Text, out methodsClass))
                                    methodsClass = null;
                            }
                            if (methodsClass == null)
                            {
                                AddError(tok, "Expecting registered class name such as OutlineMethods following From.");
                                return null;
                            }
                            break;
                        case ReservedWords.Params:
                            paramsTok = tok;
                            tok = GetToken(tokenIndex++);
                            if (tok.Text != "=")
                            {
                                AddError(tok, "Expecting '=' following Params.");
                            }
                            paramsCount = -1;
                            tok = GetToken(tokenIndex++);
                            if (tok.TokenType == Token.TokenTypes.Number)
                            {
                                if (!int.TryParse(tok.Text, out paramsCount))
                                    paramsCount = -1;
                            }
                            if (paramsCount != 1 && paramsCount != 2)
                            {
                                AddError(tok, "Expecting 1 or 2 for params count.");
                                return null;
                            }
                            break;
                        case ReservedWords.Instance:
                            reservedWords.Remove(ReservedWords.ParametersClass);
                            instancePropertyNames = ParseInstancePropertyNames(ref tokenIndex);
                            break;
                        case ReservedWords.ParametersClass:
                            reservedWords.Remove(ReservedWords.Instance);
                            bool isCurrValid = false;
                            tok = GetToken(tokenIndex++);
                            if (tok.Text == ":")
                            {
                                tok = GetToken(tokenIndex++);
                                if (tok.TokenType == Token.TokenTypes.Identifier)
                                {
                                    paramClassName = tok.Text;
                                    isCurrValid = true;
                                }
                            }
                            if (!isCurrValid)
                            {
                                AddError(tok, "Expecting 'ParametersClass: className'.");
                            }
                            break;
                        case ReservedWords.Inherit:
                            tok = GetToken(tokenIndex++);
                            if (tok.Text == ":")
                            {
                                tok = GetToken(tokenIndex++);
                                string text = tok.Text.ToLower();
                                if (text == "true" || text == "false")
                                {
                                    inherit = text == "true";
                                }
                                else
                                {
                                    AddError(tok, "Expecting True or False following 'Inherit:'.");
                                }
                            }
                            else
                            {
                                AddError(tok, "Expecting ':' following Inherit.");
                            }
                            break;
                    }
                }
                else if (functionTypesDict.TryGetValue(tok.Text, out FunctionTypes fnType))
                {
                    functionType = fnType;
                }
                else if (Enum.TryParse(tok.Text, out MathFunctionTypes mathFunctionType))
                {
                    mathType = mathFunctionType;
                }
                else
                    isValid = false;
                if (!isValid)
                {
                    AddError(tok, "Invalid function declaration.");
                    return null;
                }
            }
            if (functionType == FunctionTypes.Derivative)
            {
                if (paramsCount != 1)
                    AddError(paramsTok, "Derivative function must have only 1 parameter.");
                if (typeName == null)
                    typeName = nameof(Complex);
            }
            if (tok.Text == "=")
            {
                tokenIndex++;
                defaultMethod = GetToken(tokenIndex++);
                if (defaultMethod.TokenType != Token.TokenTypes.Identifier)
                {
                    AddError(tok, "Expecting unquoted name following '=', for default function.");
                    return null;
                }
            }
            if (inherit == null)
                inherit = instancePropertyNames == null;
            return new FunctionParamInfo(nameTok.Text, defaultMethod?.Text, paramsCount,
                                         typeName, directiveToken.CharIndex, methodsClass,
                                         functionType, mathType)
            {
                InstancePropertyNames = instancePropertyNames,
                ParametersClassName = paramClassName,
                AddDefaultMethodTypes = (bool)inherit
            };
        }

        private string GetParameterCode(NestedParamsParamInfo nestedParamsParam)
        {
            string className = nestedParamsParam.TypeName;
            return "[NestedParameters] " +
                   $"public {className} {nestedParamsParam.ParameterName} {{ get; }} = new {className}();";
        }

        private string GetParameterCode(ParamInfo paramInfo)
        {
            StringBuilder sb = new StringBuilder();
            var paramInfoParams = new List<string>();
            string minValue = paramInfo.MinValue;
            if (minValue == null && paramInfo.Category == ParamCategories.ArrayLength)
                minValue = "0";
            if (minValue != null)
                paramInfoParams.Add($"MinValue = {minValue}");
            if (paramInfo.MaxValue != null)
                paramInfoParams.Add($"MaxValue = {paramInfo.MaxValue}");
            if (paramInfo.Category == ParamCategories.ArrayLength)
                paramInfoParams.Add("UpdateParametersOnChange = true");
            if (paramInfo.ParameterSource != ParameterSources.None)
            {
                paramInfoParams.Add(
                    $"ParameterSource={paramInfo.ParameterSource.GetType().Name}.{paramInfo.ParameterSource}");
            }
            if (paramInfo.HasNestedParameters)
                sb.Append("[NestedParameters] ");
            if (paramInfo.Label != null)
                sb.Append($"[ParameterLabel(@\"{paramInfo.Label.Replace("\"", "\\\"")}\")] ");
            if (paramInfoParams.Count > 0)
                sb.Append($"[ParameterInfo({string.Join(", ", paramInfoParams)})] ");
            if (paramInfo.PreviousName != null)
                sb.Append($"[ArrayBaseName(\"{paramInfo.PreviousName}\", {paramInfo.PreviousStartNumber})] ");
            sb.Append($"public {paramInfo.GetDecType()} {paramInfo.ParameterName} ");
            bool hasSet = paramInfo.Category == ParamCategories.Scalar || 
                          paramInfo.IsArray || 
                          paramInfo.Category == ParamCategories.ArrayLength;
            if (hasSet)
            {
                if (paramInfo.SetProperty == null)
                    sb.Append("{ get; protected set; }");
                else
                {
                    sb.Append($"{{ get => {paramInfo.SetProperty};");
                    sb.Append($" protected set => {paramInfo.SetProperty} = value; }}");
                }
            }
            else
            {
                sb.Append("{ get; }");
            }
            //if (paramInfo.IsArray)
            //    sb.Append("{ get; private set; }");
            //else if (paramInfo.Category == ParamCategories.Scalar || paramInfo.Category == ParamCategories.ArrayLength)
            //    //Parameters are set via reflection, and readonly for the Eval method.
            //    sb.Append("{ get; private set; }");
            //else
            //    sb.Append("{ get; }");
            if (paramInfo.Initializer != null && !paramInfo.IsArray && paramInfo.SetProperty == null)
            {
                var functionParamInfo = paramInfo as FunctionParamInfo;
                if (functionParamInfo == null || functionParamInfo.InstancePropertyNames == null)
                    sb.Append($" = {paramInfo.Initializer};");
            }
            return sb.ToString();
        }

        private void ProcessParametersConstructor(ElementInfo paramsClassInfo)
        {
            var arrayParams = GetParamInfos<ParamInfo>(ParamsDict.Values).Where(pi => pi.IsArray);
            //var nestedParams = GetParamInfos<NestedParamsParamInfo>(ParamsDict.Values);
            var instanceFunctionParams = GetParamInfos<FunctionParamInfo>(ParamsDict.Values)
                                         .Where(p => p.InstancePropertyNames != null);
            if (!(arrayParams.Any() || instanceFunctionParams.Any())) //|| nestedParams.Any()))
                return;
            ElementInfo constructorInfo = elemInfoList.Find(
                        ei => ei.ElementType == ElementTypes.Constructor && ei.Parent == paramsClassInfo);
            int codePartsIndex = constructorInfo != null ? constructorInfo.CodePartsIndex : paramsClassInfo.CodePartsIndex;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            indentLevel = paramsClassInfo.Nesting;
            if (constructorInfo == null)
            {
                AppendLine(sb, $"public {paramsClassInfo.ElementName}()");
                OpenBrace(sb);
            }
            //AppendLines(sb, nestedParams.Select(pi => $"{pi.ParameterName} = {pi.Initializer};"));
            AppendLines(sb, instanceFunctionParams.Select(pi => $"{pi.ParameterName} = {pi.Initializer};"));
            AppendLines(sb, arrayParams.Select(pi => $"{pi.ParameterName} = {pi.GetArrayInitializer()};"));
            var distinctLens = arrayParams.Select(pi => pi.GetArrayLength()).Distinct();
            foreach (string len in distinctLens)
            {
                var prms = arrayParams.Where(pi => pi.Initializer != null && pi.GetArrayLength() == len);
                AppendLine(sb, $"for (int i = 0; i < {len}; i++)");
                OpenBrace(sb);
                foreach (ParamInfo prm in prms)
                {
                    AppendLine(sb, $"{prm.ParameterName}[i] = {prm.Initializer};");
                }
                CloseBrace(sb);
            }
            if (constructorInfo == null)
            {
                CloseBrace(sb);
            }
            newCodeParts.Insert(codePartsIndex, sb.ToString());
            //newCodeParts.Insert(codePartsIndex, "/* " + sb.ToString() + " */");
        }

        private IEnumerable<T> GetParamInfos<T>(IEnumerable<BaseParamInfo> baseParamInfos) where T : BaseParamInfo
        {
            return baseParamInfos.Select(p => p as T).Where(p => p != null);
        }

        private void ProcessUpdateMethod(ElementInfo paramsClassInfo)
        {
            const string methodName = "Update";
            var arrayParams = GetParamInfos<ParamInfo>(ParamsDict.Values).Where(pi => pi.ArrayLengthParamName != null);
            if (!arrayParams.Any())
                return;
            ElementInfo updateInfo = elemInfoList.Find(
                        ei => ei.ElementType == ElementTypes.Method && ei.ElementName == methodName);
            int codePartsIndex = updateInfo != null ? updateInfo.CodePartsIndex : paramsClassInfo.CodePartsIndex;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            indentLevel = paramsClassInfo.Nesting;
            if (updateInfo == null)
            {
                AppendLine(sb, $"public void {methodName}()");
                OpenBrace(sb);
            }
            AppendLine(sb, "int previousLength;");
            var distinctLens = arrayParams.Select(pi => pi.ArrayLengthParamName).Distinct();
            foreach (string len in distinctLens)
            {
                var prms = arrayParams.Where(pi => pi.ArrayLengthParamName == len);
                var firstParm = prms.First();
                AppendLine(sb, $"previousLength = {firstParm.ParameterName}.Length;");
                AppendLine(sb, $"if ({len} != previousLength)");
                OpenBrace(sb);
                foreach (ParamInfo prm in prms)
                {
                    // WaveFreq = Tools.RedimArray(WaveFreq, WavesCount, 1.0);
                    var args = new List<string>() { prm.ParameterName, len };
                    if (prm.Initializer != null && prm.Category == ParamCategories.Scalar)
                    {
                        args.Add(prm.Initializer);
                    }
                    AppendLine(sb, $"{prm.ParameterName} = Tools.RedimArray({string.Join(", ", args)});");
                }
                prms = prms.Where(pi => pi.Category != ParamCategories.Scalar);
                if (prms.Any())
                {
                    AppendLine(sb, $"for (int i = previousLength; i < {len}; i++)");
                    OpenBrace(sb);
                    foreach (ParamInfo prm in prms)
                    {
                        AppendLine(sb, $"{prm.ParameterName}[i] = {prm.Initializer};");
                    }
                    CloseBrace(sb);
                }
                CloseBrace(sb);
            }
            if (updateInfo == null)
            {
                CloseBrace(sb);
            }
            newCodeParts.Insert(codePartsIndex, sb.ToString());
            //newCodeParts.Insert(codePartsIndex, "/* " + sb.ToString() + " */");
        }

        private bool ProcessParameterRef(ref int tokenIndex)
        {
            Token nameTok = GetToken(tokenIndex);
            if (nameTok.TokenType != Token.TokenTypes.Identifier)
            {
                return false;
            }
            string paramName;
            BaseParamInfo paramInfo;
            if (ParamsDict.TryGetValue(nameTok.Text, out paramInfo))
            {
                paramName = paramInfo.ParameterName;
            }
            else
            {
                paramInfo = null;
                paramName = nameTok.Text;
            }
            tokenIndex++;
            int startIndex = nameTok.CharIndex + nameTok.Text.Length;
            int endIndex = startIndex;
            Token token = GetToken(tokenIndex);
            if (token.TokenType == Token.TokenTypes.LeftBracket)
            {
                tokenIndex++;
                int level = 1;
                while (level != 0)
                {
                    token = GetToken(tokenIndex++);
                    if (token.TokenType == Token.TokenTypes.RightBracket)
                        level--;
                    else if (token.TokenType == Token.TokenTypes.LeftBracket)
                        level++;
                    else if (token.TokenType == Token.TokenTypes.EndOfStream)
                        break;
                }
                if (level != 0)
                {
                    AddError(token, "Invalid array subscript.");
                    return false;
                }
                endIndex = token.CharIndex + token.Text.Length;
            }
            string refCode = $"{ParametersPropertyName}.{paramName}";
            if (startIndex < endIndex)
                refCode += sourceCode.Substring(startIndex, endIndex - startIndex);
            if (paramInfo != null)
            {
                if (paramInfo.Category == ParamCategories.Function)
                    refCode += ".Function";
                else if (paramInfo.Category == ParamCategories.Enumerated || paramInfo.Category == ParamCategories.InfluencePoint)
                    refCode += ".SelectedValue";
                else if (paramInfo.Category == ParamCategories.Random)
                    refCode += ".Value";
            }
            newCodeParts.Add(refCode);
            return true;
        }

        private bool ParseReservedWordOption(string text, out ReservedWords reservedWord, 
                                             HashSet<ReservedWords> validWords)
        {
            bool isValid = reservedWordsDict.TryGetValue(text, out reservedWord);
            if (isValid)
            {
                isValid = validWords.Contains(reservedWord);
                if (isValid)
                    validWords.Remove(reservedWord);
            }
            else
                reservedWord = ReservedWords.None;
            return isValid;
        }

        private bool ParseReservedWord(string text, ReservedWords validWord)
        {
            return ParseReservedWord(text, out ReservedWords r, validWord);
        }

        private bool ParseReservedWord(string text, out ReservedWords reservedWord, params ReservedWords[] validWords)
        {
            return ParseReservedWordOption(text, out reservedWord, new HashSet<ReservedWords>(validWords));
        }

        private bool ParseReservedWord(string text, out ReservedWords reservedWord, Action<ReservedWords[]> errorAction, params ReservedWords[] validWords)
        {
            bool isValid = ParseReservedWordOption(text, out reservedWord, new HashSet<ReservedWords>(validWords));
            if (!isValid & errorAction != null)
            {
                errorAction(validWords);
            }
            return isValid;
        }

        private Token GetToken(int tokenIndex, bool required = true)
        {
            if (tokenIndex >= codeTokens.Count)
            {
                if (required)
                    AddError("Unexpected end of file.");
                return nullToken;
            }
            return codeTokens[tokenIndex];
        }

        private bool GetSpecificToken(int tokenIndex, out Token token, params string[] texts)
        {
            bool isValid = tokenIndex < codeTokens.Count;
            token = nullToken;
            if (isValid)
            {
                token = codeTokens[tokenIndex];
                isValid = texts.Contains(token.Text);
            }
            if (!isValid)
            {
                string message;
                if (texts.Length > 1)
                    message = $"Expecting one of: {string.Join(",", texts.Select(txt => $"'{txt}'"))}";
                else
                    message = $"Expecting: '{texts.FirstOrDefault()}'";
                AddError(token, message);
                token = nullToken;
            }
            return isValid;
        }

        private bool IsAccessToken(Token tok, int tokenIndex)
        {
            if (tok.TokenType != Token.TokenTypes.Identifier)
                return false;
            if (tokenIndex > 0 && codeTokens[tokenIndex - 1].Text == "@")
                return false;
            return tok.Text == "public" || tok.Text == "private" || 
                   tok.Text == "internal" || tok.Text == "protected";
        }

        public static Dictionary<string, T> CreateDictionary<T>()
        {
            return new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        }

        private TokenDirectives GetTokenDirective(Token token, int tokenIndex)
        {
            TokenDirectives tokenDirective;
            if (token.TokenType == Token.TokenTypes.Custom)
            {
                switch (token.Text)
                {
                    case parameterRefPattern:
                        if (tokenIndex > 0 && codeTokens[tokenIndex - 1].Text == "\\")
                            tokenDirective = TokenDirectives.EscapedChar;
                        else
                            tokenDirective = TokenDirectives.ParameterRef;
                        break;
                    case parameterDecPattern:
                        tokenDirective = TokenDirectives.ParameterDec;
                        break;
                    case directivePattern:
                        tokenDirective = TokenDirectives.Directive;
                        break;
                    default:
                        throw new Exception($"Invalid custom token '{token.Text}'");
                }
            }
            else
                tokenDirective = TokenDirectives.None;
            return tokenDirective;
        }

    }
}
