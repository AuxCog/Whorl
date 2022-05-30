using ParserEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Whorl
{
    public enum ParseStatusValues
    {
        Success,
        ParseErrors,
        PreprocessorErrors
    }

    public class ParsedEventArgs: EventArgs
    {
        public ParseStatusValues ParseStatus { get; }

        public ParsedEventArgs(ParseStatusValues parseStatus)
        {
            ParseStatus = parseStatus;
        }
    }

    public class FormulaSettings : GuidKey, IXml
    {
        public Pattern ParentPattern { get; }

        public string Formula { get; private set; }

        public string LegacyFormula { get; private set; }

        public string FormulaName { get; set; } = "Formula1";

        public FormulaTypes FormulaType { get; set; }

        public bool IsCSharpFormula { get; set; }

        public bool HasKeyEnumParameters
        {
            get
            {
                return IsCSharpFormula && ParentPattern != null &&
                       ParentPattern.InfluencePointInfoList.KeyEnumParamsDict.Values.Any(v => v.Parent.FormulaSettings == this);
            }
        }

        public EventHandler<ParsedEventArgs> ParsedEventHandler;

        public CSharpCompiledInfo CSharpCompiledInfo
        {
            get { return TestCSharpEvalType == null ? cSharpCompiledInfo : testCSharpCompiledInfo; }
        }

        public InfluenceLinkParentCollection InfluenceLinkParentCollection { get; set; }

        private CSharpCompiledInfo cSharpCompiledInfo { get; set; }
        private CSharpCompiledInfo testCSharpCompiledInfo { get; set; }

        private Type _testCSharpEvalType;
        public Type TestCSharpEvalType
        {
            get { return _testCSharpEvalType; }
            set
            {
                _testCSharpEvalType = value;
                if (_testCSharpEvalType != null)
                {
                    if (testCSharpCompiledInfo == null)
                        testCSharpCompiledInfo = new CSharpCompiledInfo(new CSharpSharedCompiledInfo());
                    testCSharpCompiledInfo.CSharpSharedCompiledInfo.UseCompiledType(_testCSharpEvalType);
                    _testEvalInstance = testCSharpCompiledInfo.CreateEvalInstance();
                    if (_evalInstance != null)
                        CopyCSharpParams(_evalInstance.ParamsObj, _testEvalInstance);
                }
                SetEvalInstance();
            }
        }
        public CSharpCompiledInfo.EvalInstance EvalInstance { get; private set; }
        public TokensTransformInfo TokensTransformInfo { get; set; }
        private CSharpCompiledInfo.EvalInstance _evalInstance;
        private CSharpCompiledInfo.EvalInstance _testEvalInstance;

        private CSharpCompiledInfo.EvalInstance GetEvalInstance()
        {
            return TestCSharpEvalType == null ? _evalInstance : _testEvalInstance;
        }

        private void SetEvalInstance()
        {
            EvalInstance = GetEvalInstance();
        }

        public void SetCSharpInfoInstance(object info)
        {
            if (IsCSharpFormula)
            {
                if (EvalInstance == null)
                    throw new Exception("EvalInstance is null.");
                EvalInstance.SetInfoObject(info);
            }
        }

        public ExpressionParser Parser { get; }
        public Expression FormulaExpression { get; private set; }

        public Parameter InfluenceValueParameter { get; private set; }

        //public bool FormulaChanged { get; private set; }
        //public bool ParseOnChanges { get; set; }
        public bool IsValid
        {
            get
            {
                if (IsCSharpFormula)
                {
                    var compiledInfo = CSharpCompiledInfo;
                    return EvalInstance != null && compiledInfo != null && !compiledInfo.CSharpSharedCompiledInfo.Errors.Any();
                }
                //return CSharpCompiledInfo.EvalClassType != null && !CSharpCompiledInfo.Errors.Any();
                else
                    return FormulaExpression != null;
            }
        }

        public bool HaveParsedFormula
        {
            get
            {
                if (IsCSharpFormula)
                    return IsValid;
                else
                    return FormulaExpression != null && FormulaExpression.HaveExpressionStacks;
            }
        }

        //{
        //    get { return formula; }
        //    private set
        //    {
        //        formula = value;
        //        //if (formula != value)
        //        //{
        //        //    formula = value;
        //        //    FormulaChanged = true;
        //        //    //if (ParseOnChanges)
        //        //    //    Parse();
        //        //}
        //    }
        //}

        public static ExpressionParser CreateExpressionParser()
        {
            var parser = new ExpressionParser();
            parser.OptimizeExpressions = WhorlSettings.Instance.OptimizeExpressions;
            return parser;
        }

        public BaseParameter GetParameter(string paramName, bool isOutputParameter = false)
        {
            BaseParameter baseParameter;
            if (FormulaExpression == null)
                baseParameter = null;
            else
            {
                baseParameter = FormulaExpression.GetParameter(paramName);
                if (baseParameter != null && baseParameter.IsOutputParameter != isOutputParameter)
                    baseParameter = null;
            }
            return baseParameter;
        }

        private IEnumerable<BaseParameter> GetBaseParameters(Predicate<BaseParameter> predicate)
        {
            if (FormulaExpression == null)
                return new List<BaseParameter>();
            else
                return FormulaExpression.Parameters.Where(p => predicate.Invoke(p)).SelectMany(p => p.GetParameters());
        }

        public IEnumerable<BaseParameter> BaseParameters
        {
            get
            {
                return GetBaseParameters(p => p.Visible);
            }
        }

        public IEnumerable<BaseParameter> OutputParameters
        {
            get
            {
                return GetBaseParameters(p => p.IsOutputParameter);
            }
        }

        public IEnumerable<Parameter> Parameters
        {
            get
            {
                return BaseParameters.Select(prm => prm as Parameter).Where(p => p != null);
            }
        }

        public IEnumerable<CustomParameter> CustomParameters
        {
            get
            {
                return BaseParameters.Where(prm => prm is CustomParameter)
                                     .Select(prm => (CustomParameter)prm);
            }
        }

        public IEnumerable<CustomParameter> RandomParameters
        {
            get
            {
                return CustomParameters.Where(cp => cp.CustomType == CustomParameterTypes.RandomRange);
            }
        }

        public delegate void configureDelegate(ExpressionParser parser);

        //private FormulaSettings(Pattern parentPattern)
        //{
        //    if (parentPattern == null)
        //        throw new NullReferenceException("parentPattern cannot be null.");
        //    ParentPattern = parentPattern;
        //}

        public FormulaSettings(FormulaTypes formulaType, ExpressionParser parser = null, Pattern pattern = null)
        {
            FormulaType = formulaType;
            ParentPattern = pattern;
            //cSharpCompiledInfo = new CSharpCompiledInfo();
            if (parser == null)
                parser = CreateExpressionParser();
            Parser = parser;
        }

        public FormulaSettings(FormulaSettings source, configureDelegate configureParser = null,
                               ExpressionParser parser = null, Pattern pattern = null): base(source)
        {
            FormulaType = source.FormulaType;
            FormulaName = source.FormulaName;
            ParentPattern = pattern;
            LegacyFormula = source.LegacyFormula;
            IsCSharpFormula = source.IsCSharpFormula;
            cSharpCompiledInfo = source.cSharpCompiledInfo;
            TestCSharpEvalType = source.TestCSharpEvalType;
            TokensTransformInfo = source.TokensTransformInfo;
            if (parser == null)
                Parser = CreateExpressionParser();
            else
                Parser = parser;
            configureParser?.Invoke(Parser);
            Formula = source.Formula;
            if (source.IsValid)
            {
                //Set properties so Parse() will copy parameters:
                if (IsCSharpFormula)
                {
                    SavedParamsObj = source.EvalInstance != null ?
                                     source.EvalInstance.ParamsObj : source.SavedParamsObj;
                    if (cSharpCompiledInfo != null)
                        _evalInstance = cSharpCompiledInfo.CreateEvalInstance();
                    SetEvalInstance();
                    CopyCSharpParams(SavedParamsObj);
                    RefreshKeyEnumParams(source);
                }
                else
                {
                    FormulaExpression = source.FormulaExpression;
                    Parse(source.Formula, ifChanged: false, displayWarnings: false);
                }
            }
        }

        public FormulaSettings GetCopy(configureDelegate configureParser = null, ExpressionParser parser = null, Pattern pattern = null)
        {
            return new FormulaSettings(this, configureParser, parser, pattern);
        }

        static FormulaSettings()
        {
            MethodInfo methodInfo = typeof(FormulaSettings).GetMethod(nameof(GetRandomValue),
                                    BindingFlags.Static | BindingFlags.Public);
            if (methodInfo != null)
            {
                ExpressionParser.DeclareFunction(methodInfo.Name, methodInfo.ReturnType, methodInfo);
            }
        }

        public void PopulateInsertTokensComboBox(ComboBox cboInsertTokens)
        {
            List<string> tokens = new List<string>() { string.Empty };
            tokens.AddRange(Parser.GetIdentifierTokens());
            if (FormulaExpression != null)
                tokens.AddRange(BaseParameters.Select(p => "@" + p.ParameterName));
            tokens.Add("Custom.RandomRange");
            tokens.AddRange(ExpressionParser.GetStaticTokens());
            tokens.AddRange(ExpressionParser.GetOperators());
            cboInsertTokens.DataSource = null;
            cboInsertTokens.DataSource = tokens;
        }

        public static void cboInsertTokensOnChanged(ComboBox cboInsertTokens, TextBox txtFormula)
        {
            string token = (string)cboInsertTokens.SelectedItem;
            Tools.InsertTextInTextBox(txtFormula, token);
            //if (string.IsNullOrEmpty(token))
            //    return;
            //string formula = txtFormula.Text;
            //if (formula == null)
            //    formula = string.Empty;
            //int caretPos = txtFormula.SelectionStart;
            //if (caretPos > formula.Length)
            //    return;
            //formula = formula.Substring(0, caretPos) + token + formula.Substring(caretPos);
            //txtFormula.Text = formula;
            //txtFormula.SelectionStart = caretPos + token.Length;
            //txtFormula.ScrollToCaret();
        }


        private string SetParameterOrCustomValue(BaseParameter prm, object val)
        {
            string errorMessage = null;
            var cprm = prm as CustomParameter;
            //if (cprm != null && cprm.CustomType == CustomParameterTypes.RandomRange && val != null)
            //{
            //    //if (cprm.Context == null)
            //    //    cprm.Context = new RandomRange();
            //    if (!(val is double))
            //        throw new Exception($"Type of value for CustomParameter is invalid: {val}");
            //    ((RandomRange)cprm.Context).KeepCount =
            //        Math.Max(1, (int)Math.Round((double)val));
            //}
            if (cprm == null)
            {
                try
                {
                    prm.Value = val;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }
            return errorMessage;
        }

        public List<BaseParameter> savedParameters { get; private set; }
        public object SavedParamsObj { get; private set; }

        public string PreprocessCSharp(string code)
        {
            return CSharpPreprocessor.Instance.Preprocess(code, GetFormulaInfoType(FormulaType));
        }

        /// <summary>
        /// Parse normal or C# formula.
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="throwException"></param>
        /// <param name="formulaForm"></param>
        /// <param name="ifChanged">Only parse if formula has changed.</param>
        /// <param name="initCSharpParams"></param>
        /// <param name="displayErrors"></param>
        /// <param name="displayWarnings"></param>
        /// <param name="isModule">True if formula is for a module to merge into a regular formula.</param>
        /// <returns></returns>
        public ParseStatusValues Parse(string formula, bool throwException = true, IFormulaForm formulaForm = null,
                          bool ifChanged = true, bool initCSharpParams = true,
                          bool displayErrors = true, bool displayWarnings = true, bool isModule = false,
                          bool resolveInfluenceReferences = true)
        {
            if (ifChanged && !isModule && IsValid && formula == Formula)
                return ParseStatusValues.Success;
            InfluenceValueParameter = null;
            ParseStatusValues parseStatus;
            bool isValid;
            bool preprocessorErrors = false;
            CSharpCompiledInfo compiledInfo = null;
            var processor = CSharpPreprocessor.Instance;
            ParserErrorsForm.ResetForm();
            string cSharpCode = null;
            CSharpSharedCompiledInfo sharedCompiledInfo = null;
            if (IsCSharpFormula)
            {
                cSharpCode = PreprocessCSharp(formula);
                preprocessorErrors = processor.ErrorMessages.Any();
                isValid = !preprocessorErrors;
                if (isValid)
                {
                    if (displayWarnings && processor.Warnings.Any())
                    {
                        MessageBox.Show(string.Join(Environment.NewLine, processor.Warnings), "Preprocessor Warnings");
                    }
                    sharedCompiledInfo = CSharpCompiler.Instance.CompileFormula(cSharpCode);
                    //sharedCompiledInfo.CompileCode(cSharpCode);
                    isValid = sharedCompiledInfo.EvalClassType != null && !sharedCompiledInfo.Errors.Any();
                    if (isValid)
                    {
                        compiledInfo = new CSharpCompiledInfo(sharedCompiledInfo);
                        if (!isModule)
                            cSharpCompiledInfo = compiledInfo;
                    }
                    if (!isModule)
                    {
                        if (isValid)
                        {
                            isValid = ValidateKeyedEnums();
                        }
                        if (isValid)
                        {
                            Formula = formula;
                            _evalInstance = compiledInfo.CreateEvalInstance();
                            if (_evalInstance?.ParamsObj != null)
                            {
                                if (SavedParamsObj == null)
                                {
                                    if (initCSharpParams)
                                        CopyLegacyParametersToCSharp(BaseParameters, _evalInstance);
                                }
                                else
                                    CopyCSharpParams(SavedParamsObj, _evalInstance);
                                SavedParamsObj = _evalInstance.ParamsObj;
                            }
                        }
                    }
                }
                if (!isValid && !isModule)
                {
                    _evalInstance = null;
                }
                SetEvalInstance();
            }
            else  //Legacy, not a C# formula.
            {
                if (FormulaExpression != null)
                {
                    savedParameters = BaseParameters.ToList();
                }
                isValid = Parser.ParseStatements(formula, TokensTransformInfo);
                if (isValid)
                {
                    LegacyFormula = Formula = formula;
                    FormulaExpression = Parser.Expression;
                    if (savedParameters != null)
                        CopyParameters(savedParameters);
                }
                else
                {
                    FormulaExpression = null;
                }
            }
            if (!isValid)
            {
                if (!isModule && string.IsNullOrEmpty(Formula))
                    Formula = formula;
                if (throwException || (formulaForm == null && displayErrors))
                {
                    string errMsg;
                    if (IsCSharpFormula)
                    {
                        if (preprocessorErrors)
                            errMsg = string.Join(Environment.NewLine, processor.ErrorMessages.Select(e => e.Message));
                        else if (sharedCompiledInfo != null)
                            errMsg = sharedCompiledInfo.ErrorsText;
                        else
                            errMsg = "Unknown error.";
                    }
                    else
                        errMsg = string.Join(Environment.NewLine, Parser.GetErrorMessages());
                    if (throwException)
                        throw new CustomException(errMsg);
                    else
                        MessageBox.Show(errMsg, "Parse Error Messages");
                }
                else if (displayErrors && formulaForm != null)
                {
                    var frm = ParserErrorsForm.DefaultForm;
                    if (IsCSharpFormula)
                    {
                        if (!preprocessorErrors && sharedCompiledInfo == null)
                        {
                            MessageBox.Show("Couldn't compile C# code.");
                        }
                        else
                        {
                            if (preprocessorErrors)
                                cSharpCode = formula;
                            if (preprocessorErrors || (sharedCompiledInfo != null && sharedCompiledInfo.Errors.Any()))
                            {
                                frm.Initialize(this, formulaForm, preprocessorErrors, sharedCompiledInfo, cSharpCode);
                                Tools.DisplayForm(frm);
                            }
                            else if (sharedCompiledInfo?.EvalClassType == null)
                            {
                                MessageBox.Show("Couldn't retrieve type of compiled object.");
                            }
                        }
                    }
                    else
                    {
                        frm.Initialize(this, formulaForm);
                        Tools.DisplayForm(frm);
                    }
                }
            }
            if (isValid)
            {
                parseStatus = ParseStatusValues.Success;
                if (resolveInfluenceReferences && InfluenceLinkParentCollection != null)
                {
                    if (InfluenceLinkParentCollection.IsCSharpFormula != IsCSharpFormula)
                    {
                        InfluenceLinkParentCollection.ClearLinkParents();
                    }
                    else
                    {
                        InfluenceLinkParentCollection.ResolveReferences(throwException: false);
                    }
                }
                if (IsCSharpFormula)
                {
                    PopulateKeyParamDictionary();
                }
                else
                {
                    //InfluenceParameters are not visible, so this.Parameters property does not include them.
                    InfluenceValueParameter = FormulaExpression.Parameters.Select(p => p as Parameter)
                                             .FirstOrDefault(p => p != null && p.ForInfluenceValue);
                }
            }
            else if (preprocessorErrors)
                parseStatus = ParseStatusValues.PreprocessorErrors;
            else
                parseStatus = ParseStatusValues.ParseErrors;
            if (!isModule)  //Not module to merge into a regular formula.
            {
                //Raise Parsed Event:
                ParsedEventHandler?.Invoke(this, new ParsedEventArgs(parseStatus));
            }
            return parseStatus;
        }

        public bool PreInitializeForEval()
        {
            if (IsCSharpFormula && EvalInstance != null)
                return EvalInstance.PreInitializeForEval();
            return true;
        }

        public bool InitializeGlobals()
        {
            if (IsCSharpFormula)
            {
                if (EvalInstance != null)
                    return EvalInstance.InitializeForEval();
            }
            else
            {
                if (FormulaExpression != null)
                    FormulaExpression.InitializeGlobals();
            }
            return true;
        }

        public bool Initialize2ForEval()
        {
            if (IsCSharpFormula && EvalInstance != null)
                return EvalInstance.Initialize2ForEval();
            return true;
        }

        /// <summary>
        /// Ensure keyed enum types don't have duplicate names within pattern.
        /// </summary>
        /// <returns></returns>
        private bool ValidateKeyedEnums()
        {
            if (ParentPattern == null)
                return true;
            var keyedEnumTypes = GetKeyedEnumTypes();
            if (!keyedEnumTypes.Any())
                return true;
            var errMessages = new List<string>();
            foreach (Type enumType in keyedEnumTypes)
            {
                Type paramsClassType = GetKeyEnumParamsClassType(enumType, out string className);
                if (paramsClassType == null)
                {
                    errMessages.Add($"The parameters class {className} was not found in the main formula class.");
                }
                else if (paramsClassType.GetConstructor(Type.EmptyTypes) == null)
                {
                    errMessages.Add($"The parameters class {className} does not have a public parameterless constructor.");
                }
            }
            foreach (var formulaSettings in ParentPattern.GetFormulaSettings()
                     .Where(fs => fs != this && fs.IsCSharpFormula && fs.IsValid))
            {
                foreach (Type enumType in keyedEnumTypes)
                {
                    Type enumType2 = formulaSettings.CSharpCompiledInfo.CSharpSharedCompiledInfo.EvalClassType.GetNestedType(enumType.Name);
                    if (enumType2?.GetCustomAttribute<KeyEnumAttribute>() != null)
                    {
                        errMessages.Add($"The keyed enum type name {enumType.Name} is a duplicate.");
                    }
                }
            }
            if (errMessages.Any() && CSharpCompiledInfo != null)
            {
                CSharpCompiledInfo.CSharpSharedCompiledInfo.Errors.AddRange(errMessages.Select(s => new CSharpSharedCompiledInfo.ErrorInfo(s)));
            }
            return errMessages.Count == 0;
        }

        private Type GetKeyEnumParamsClassType(Type enumType, out string className)
        {
            var attribute = enumType.GetCustomAttribute<KeyEnumAttribute>();
            className = attribute?.ParamsClassName;
            return GetKeyEnumParamsClassType(attribute);
        }

        private Type GetKeyEnumParamsClassType(KeyEnumAttribute attribute)
        {
            if (attribute.ParamsClassName == null || CSharpCompiledInfo == null)
                return null;
            else
                return CSharpCompiledInfo.CSharpSharedCompiledInfo.EvalClassType.GetNestedType(attribute.ParamsClassName);
        }

        private void PopulateKeyParamDictionary()
        {
            var influencePointList = ParentPattern?.InfluencePointInfoList;
            if (influencePointList == null)
                return;
            var keyedEnumTypes = GetKeyedEnumTypes();
            foreach (Type enumType in keyedEnumTypes)
            {
                var attribute = enumType.GetCustomAttribute<KeyEnumAttribute>();
                Type paramsClassType = GetKeyEnumParamsClassType(attribute);
                foreach (object enumValue in Enum.GetValues(enumType))
                {
                    var keyEnumInfo = new KeyEnumInfo(enumValue, paramsClassType, attribute.IsGlobal, attribute.Exclusive, this);
                    var keyParams = new KeyEnumParameters(keyEnumInfo, null, createParamsObject: keyEnumInfo.ParametersAreGlobal);
                    keyParams.UpdateDictionary(influencePointList.KeyEnumParamsDict);
                }
            }
        }

        private void RefreshKeyEnumParams(Dictionary<string, KeyEnumParameters> dict, FormulaSettings source)
        {
            foreach (var keyParams in dict.Values)
            {
                if (keyParams.Parent.FormulaSettings == source)
                {
                    keyParams.Parent.FormulaSettings = this;
                }
            }
        }

        private void RefreshKeyEnumParams(FormulaSettings source)
        {
            var influencePointList = ParentPattern?.InfluencePointInfoList;
            if (influencePointList == null)
                return;
            RefreshKeyEnumParams(influencePointList.KeyEnumParamsDict, source);
            foreach (var influencePoint in influencePointList.InfluencePointInfos)
            {
                RefreshKeyEnumParams(influencePoint.KeyEnumParamsDict, source);
            }
        }


        public IEnumerable<Type> GetKeyedEnumTypes()
        {
            var compiledInfo = CSharpCompiledInfo;
            if (!IsCSharpFormula || compiledInfo == null || compiledInfo.CSharpSharedCompiledInfo.EvalClassType == null)
                return new Type[] { };
            return compiledInfo.CSharpSharedCompiledInfo.EvalClassType.GetNestedTypes()
                   .Where(t => t.IsEnum && t.GetCustomAttribute<KeyEnumAttribute>() != null);
        }

        public bool EvalFormula(bool throwException = true)
        {
            if (IsCSharpFormula)
            {
                EvalInstance.EvalFormula();
            }
            else
            {
                FormulaExpression.EvalStatements();
                if (FormulaExpression.ErrorMessages.Any())
                {
                    string messages = string.Join(Environment.NewLine, FormulaExpression.ErrorMessages);
                    if (throwException)
                        throw new CustomException(messages);
                    else
                    {
                        MessageBox.Show(messages);
                        return false;
                    }
                }
            }
            return true;
        }

        //private void CopyProperties(FormulaSettings source)
        //{
        //    //ParseOnChanges = false;
        //    //this.Formula = source.Formula;
        //    //this.ParseOnChanges = source.ParseOnChanges;
        //    this.FormulaName = source.FormulaName;
        //    LegacyFormula = source.LegacyFormula;
        //    this.IsCSharpFormula = source.IsCSharpFormula;
        //    if (source.IsValid)
        //    {
        //        //Set properties so Parse() will copy parameters:
        //        if (IsCSharpFormula)
        //        {
        //            CSharpCompiledInfo = source.CSharpCompiledInfo;
        //            if (source.EvalInstance != null)
        //                SavedParamsObj = source.EvalInstance.ParamsObj;

        //        }
        //        else
        //        {
        //            FormulaExpression = source.FormulaExpression;
        //            Parse(source.Formula, initCSharpParams: false);
        //        }
        //    }
        //}

        private void CheckUpdateParameters(object sourceParams, HashSet<string> handledPropertyNames,
                                           CSharpCompiledInfo.EvalInstance evalInstance)
        {
            if (evalInstance == null || evalInstance.ParamsObj == null)
                return;
            bool updateParams = false;
            foreach (var propInfo in sourceParams.GetType().GetProperties())
            {
                if (propInfo.PropertyType.IsArray || propInfo.PropertyType == typeof(RandomParameter))
                    continue;
                var attr = propInfo.GetCustomAttribute<ParameterInfoAttribute>();
                if (attr == null || !attr.UpdateParametersOnChange)
                    continue;
                handledPropertyNames.Add(propInfo.Name);
                var targetPropInfo = evalInstance.ParamsObj.GetType().GetProperty(propInfo.Name);
                if (targetPropInfo != null && Tools.TypesMatch(targetPropInfo.PropertyType, propInfo.PropertyType))
                {
                    object sourceParam = propInfo.GetValue(sourceParams);
                    object targetParam = targetPropInfo.GetValue(evalInstance.ParamsObj);
                    if (sourceParam != null)
                    {
                        object sourceVal = Tools.GetCSharpParameterValue(sourceParam);
                        object targetVal = Tools.GetCSharpParameterValue(targetParam);
                        if (!object.Equals(sourceVal, targetVal))
                        {
                            updateParams = true;
                            var iOptParam = targetParam as IOptionsParameter;
                            if (iOptParam != null)
                            {
                                if (sourceVal != null)
                                    iOptParam.SelectedOptionObject = iOptParam.GetOptionByText(sourceVal.ToString());
                            }
                            else
                            {
                                targetPropInfo.SetValue(evalInstance.ParamsObj, sourceVal);
                            }
                        }
                    }
                }
            }
            if (updateParams)
                evalInstance.UpdateParameters();
        }

        public void CopyCSharpParameters(object sourceParamsObj, object targetParamsObj)
        {
            CopyCSharpParameters(sourceParamsObj, targetParamsObj, ParentPattern);
        }

        public static void CopyCSharpParameters(object sourceParamsObj, object targetParamsObj,
                                                Pattern parentPattern)
        {
            CopyCSharpParameters(sourceParamsObj, targetParamsObj, new HashSet<string>(), parentPattern);
        }

        private void CopyCSharpParams(object sourceParamsObj, CSharpCompiledInfo.EvalInstance evalInstance = null)
        {
            evalInstance = evalInstance ?? EvalInstance;
            object targetParamsObj = evalInstance?.ParamsObj;
            if (!IsCSharpFormula || sourceParamsObj == null || targetParamsObj == null)
                return;
            HashSet<string> handledPropertyNames = new HashSet<string>();
            CheckUpdateParameters(sourceParamsObj, handledPropertyNames, evalInstance);
            CopyCSharpParameters(sourceParamsObj, targetParamsObj, handledPropertyNames);
        }

        private static void CopyCSharpParameters(object sourceParamsObj, object targetParamsObj, HashSet<string> handledPropertyNames, 
                                          Pattern parentPattern = null) 
        { 
            Type sourceType = sourceParamsObj.GetType();
            Type targetType = targetParamsObj.GetType();
            bool typesMatch = sourceType == targetType;
            if (!typesMatch)
            {
                var arrayPropsWithAttrs = targetType.GetProperties()
                                          .Where(pi => pi.PropertyType.IsArray &&
                                          pi.GetCustomAttribute<ArrayBaseNameAttribute>() != null);
                foreach (PropertyInfo arrayPropInfo in arrayPropsWithAttrs)
                {
                    var arrayAttr = arrayPropInfo.GetCustomAttribute<ArrayBaseNameAttribute>();
                    object targetParam = arrayPropInfo.GetValue(targetParamsObj);
                    if (targetParam != null)
                    {
                        var targetArray = (Array)targetParam;
                        Type elemType = arrayPropInfo.PropertyType.GetElementType();
                        for (int i = 0; i < targetArray.Length; i++)
                        {
                            object targetElemParam = targetArray.GetValue(i);
                            string sourcePropName = $"{arrayAttr.BaseName}{i + arrayAttr.StartNumber}";
                            var sourcePropInfo = sourceType.GetProperty(sourcePropName);
                            if (sourcePropInfo != null && Tools.TypesMatch(sourcePropInfo.PropertyType, elemType))
                            {
                                object sourceElemParam = sourcePropInfo.GetValue(sourceParamsObj);
                                CopyCSharpParameter(sourceElemParam, targetElemParam, null, sourcePropInfo, 
                                                    targetParamsObj, targetArray, i, parentPattern);
                                handledPropertyNames.Add(sourcePropInfo.Name);
                            }
                        }
                    }
                }
            }
            foreach (var propInfo in sourceType.GetProperties())
            {
                if (handledPropertyNames.Contains(propInfo.Name))
                    continue;
                var targetPropInfo = typesMatch ? propInfo : targetType.GetProperty(propInfo.Name);
                if (targetPropInfo == null)
                    continue;
                object sourceParam = propInfo.GetValue(sourceParamsObj);
                object targetParam = targetPropInfo.GetValue(targetParamsObj);
                if (typesMatch || Tools.TypesMatch(targetPropInfo.PropertyType, propInfo.PropertyType))
                {
                    if (sourceParam != null && propInfo.PropertyType.IsArray && targetParam != null)
                    {
                        var sourceArray = (Array)sourceParam;
                        var targetArray = (Array)targetParam;
                        for (int i = 0; i < Math.Min(sourceArray.Length, targetArray.Length); i++)
                        {
                            object sourceElemParam = sourceArray.GetValue(i);
                            object targetElemParam = targetArray.GetValue(i);
                            CopyCSharpParameter(sourceElemParam, targetElemParam, null, propInfo, targetParamsObj, 
                                                targetArray, i, parentPattern);
                        }
                    }
                    else
                    {
                        CopyCSharpParameter(sourceParam, targetParam, targetPropInfo, propInfo, targetParamsObj, 
                                            parentPattern: parentPattern);
                    }
                }
            }
        }

        private static void CopyCSharpParameter(object sourceParam, object targetParam, PropertyInfo targetPropInfo, 
                                         PropertyInfo propertyInfo, object targetParamsObj, 
                                         Array paramArray = null, int index = 0, Pattern parentPattern = null)
        {
            bool hasNestedParams = propertyInfo.GetCustomAttribute<NestedParametersAttribute>() != null;
            object sourceNestedParamsObj = null, targetNestedParamsObj = null;
            var iOptionsParam = sourceParam as IOptionsParameter;
            if (iOptionsParam != null)
            {
                var iTargetOptionsParam = targetParam as IOptionsParameter;
                if (iTargetOptionsParam != null)
                {
                    iTargetOptionsParam.SelectedText = iOptionsParam.SelectedText;
                    if (!ConfigureCSharpInfluenceParameter(targetParam, parentPattern))
                    {
                        object selOption = iTargetOptionsParam.GetOptionByText(iOptionsParam.SelectedText);
                        if (selOption != null)
                            iTargetOptionsParam.SelectedOptionObject = selOption;
                    }
                    if (hasNestedParams)
                    {
                        var sourceInstances = iOptionsParam.GetInstances();
                        var targetInstances = iTargetOptionsParam.GetInstances();
                        if (sourceInstances != null && targetInstances != null)
                        {
                            sourceNestedParamsObj = sourceInstances.FirstOrDefault();
                            targetNestedParamsObj = targetInstances.FirstOrDefault();
                        }
                    }
                }
            }
            else if (sourceParam is CompiledDoubleFuncParameter compiledFuncParam)
            {
                if (targetParam is CompiledDoubleFuncParameter targetFuncParam)
                {
                    targetFuncParam.CompileFormula(compiledFuncParam.Formula, editOnError: false);
                    sourceNestedParamsObj = compiledFuncParam.ParamsObject;
                    targetNestedParamsObj = targetFuncParam.ParamsObject;
                }
            }
            else if (hasNestedParams)
            {
                sourceNestedParamsObj = sourceParam;
                targetNestedParamsObj = targetParam;
            }
            else
            {
                var rndParam = sourceParam as RandomParameter;
                if (rndParam != null)
                {
                    var targetRndParam = targetParam as RandomParameter;
                    if (targetRndParam != null)
                    {
                        foreach (PropertyInfo prpInfo in RandomRange.ParameterProperties)
                        {
                            prpInfo.SetValue(targetRndParam.RandomRange, prpInfo.GetValue(rndParam.RandomRange));
                        }
                    }
                }
                else
                {
                    if (paramArray != null)
                        paramArray.SetValue(sourceParam, index);
                    else if (targetPropInfo.CanWrite)
                        targetPropInfo.SetValue(targetParamsObj, sourceParam);
                }
            }
            if (sourceNestedParamsObj != null && targetNestedParamsObj != null)
            {
                CopyCSharpParameters(sourceNestedParamsObj, targetNestedParamsObj, parentPattern);
            }
        }

        private void CopyLegacyParametersToCSharp(IEnumerable<BaseParameter> baseParameters,
                                                  CSharpCompiledInfo.EvalInstance evalInstance)
        {
            if (!IsCSharpFormula || evalInstance?.ParamsObj == null || baseParameters == null)
                return;
            foreach (BaseParameter prm in baseParameters)
            {
                //var inflParam = prm as Parameter;
                //if (inflParam != null && inflParam.ForInfluenceValue)
                //    continue;
                var targetPropInfo = evalInstance.ParamsObj.GetType().GetProperty(prm.ParameterName);
                if (targetPropInfo != null)
                {
                    object value = prm.Value;
                    if (prm is VarFunctionParameter)
                    {
                        object targetParam = targetPropInfo.GetValue(evalInstance.ParamsObj);
                        var iOptionsParam = targetParam as IOptionsParameter;
                        if (iOptionsParam != null)
                        {
                            string functionName = prm.GetValueChoice(value).ToString();
                            try
                            {
                                iOptionsParam.SelectedOptionObject = iOptionsParam.GetOptionByText(functionName);
                            }
                            catch { }
                        }
                    }
                    else if (!(prm is CustomParameter))
                    {
                        try
                        {
                            targetPropInfo.SetValue(evalInstance.ParamsObj, value);
                        }
                        catch { }
                    }
                }
            }
        }

        public void CopyParameters(IEnumerable<BaseParameter> parameters)
        {
            if (IsCSharpFormula)
                return;
            if (FormulaExpression == null)
                throw new NullReferenceException("FormulaExpression is null.");
            foreach (BaseParameter prm in parameters)
            {
                //if (prm.ParameterName == "midPercent")
                //{
                //    Debug.WriteLine(prm.Value);
                //}
                //var inflParam = prm as Parameter;
                //if (inflParam != null && inflParam.ForInfluenceValue)
                //    continue;
                BaseParameter copyPrm = FormulaExpression.GetParameter(prm.ParameterName);
                if (copyPrm != null && copyPrm.GetType() == prm.GetType())
                {
                    copyPrm.SelectedText = prm.SelectedText;
                    CustomParameter cprm = prm as CustomParameter;
                    if (cprm != null)
                    {
                        var copyCprm = copyPrm as CustomParameter;
                        copyCprm.SelectedPropertyInfo = cprm.SelectedPropertyInfo;
                        if (cprm.CustomType == CustomParameterTypes.RandomRange &&
                            copyCprm.CustomType == CustomParameterTypes.RandomRange)
                        {
                            foreach (PropertyInfo propInfo in RandomRange.ParameterProperties)
                            {
                                propInfo.SetValue(copyCprm.Context, propInfo.GetValue(cprm.Context));
                            }
                        }
                    }
                    else
                    {
                        var oParam = copyPrm as Parameter;
                        if (oParam != null)
                        {
                            oParam.Guid = ((Parameter)prm).Guid;
                            SetParameterOrCustomValue(copyPrm, prm.Value);
                        }
                        else
                        {
                            if (!ConfigureInfluenceParameter(copyPrm))
                            {
                                if (copyPrm.HasChoices && prm.SelectedChoice != null)
                                {
                                    copyPrm.SetValueFromParameterChoice(prm.SelectedChoice);
                                }
                                else
                                    SetParameterOrCustomValue(copyPrm, prm.Value);
                            }
                        }
                    }
                }
            }
            //PopulateParametersDataTables();
        }

        public bool PasteParameters(FormulaSettings sourceFormulaSettings)
        {
            bool isValid = false;
            if (sourceFormulaSettings == null)
            {
                MessageBox.Show("You haven't copied parameters yet to paste.");
            }
            else if (sourceFormulaSettings == this)
            {
                MessageBox.Show("The target parameters are the same as the source.");
            }
            else if (!sourceFormulaSettings.IsValid)
            {
                MessageBox.Show("Source formula has errors.");
            }
            else if (sourceFormulaSettings.FormulaType != FormulaType)
            {
                MessageBox.Show("Formula types do not match.");
            }
            else if (sourceFormulaSettings.IsCSharpFormula && !IsCSharpFormula)
            {
                MessageBox.Show("Cannot copy C# parameters to legacy formula.");
            }
            else
            {
                isValid = true;
                if (sourceFormulaSettings.IsCSharpFormula)
                {
                    if (sourceFormulaSettings.EvalInstance == null)
                    {
                        isValid = false;
                        MessageBox.Show("Cannot copy parameters.");
                    }
                    else
                        CopyCSharpParams(sourceFormulaSettings.EvalInstance.ParamsObj);
                }
                else if (IsCSharpFormula)
                {
                    CopyLegacyParametersToCSharp(sourceFormulaSettings.BaseParameters, EvalInstance);
                }
                else
                {
                    CopyParameters(sourceFormulaSettings.BaseParameters);
                }
            }
            return isValid;
        }

        public bool ConfigureInfluenceParameter(BaseParameter parameter, bool setValue = true)
        {
            var influenceParam = parameter as GenericParameter<DoublePoint>;
            if (influenceParam != null)
            {
                if (influenceParam.Category != GenericParameter<DoublePoint>.Categories.InfluencePoint)
                    influenceParam = null;
                else
                {
                    var items = new List<InfluencePointInfo>() { null };
                    if (ParentPattern != null)
                    {
                        items.AddRange(ParentPattern.InfluencePointInfoList.InfluencePointInfos);
                    }
                    influenceParam.SetParameterChoices(items, nullText: "(none)", ip => ((InfluencePointInfo)ip).InfluencePoint);
                    if (setValue && influenceParam.SelectedText != null)
                    {
                        var choice = influenceParam.GetParameterChoice(influenceParam.SelectedText);
                        if (choice != null)
                        {
                            influenceParam.SetValueFromParameterChoice(choice);
                        }
                    }
                }
            }
            return influenceParam != null;
        }

        public bool ConfigureCSharpInfluenceParameter(object csharpParameter, bool setValue = true)
        {
            return ConfigureCSharpInfluenceParameter(csharpParameter, ParentPattern, setValue);
        }

        public static bool ConfigureCSharpInfluenceParameter(object csharpParameter, Pattern parentPattern, bool setValue = true)
        {
            var influenceParam = csharpParameter as OptionsParameter<InfluencePointInfo>;
            if (influenceParam != null)
            {
                var items = new List<InfluencePointInfo>() { null };
                if (parentPattern != null)
                {
                    items.AddRange(parentPattern.InfluencePointInfoList.InfluencePointInfos);
                }
                influenceParam.SetOptions(items, setSelected: false);
                if (setValue && influenceParam.SelectedText != null)
                {
                    var item = influenceParam.GetOptionByText(influenceParam.SelectedText);
                    if (item != null)
                    {
                        influenceParam.SelectedOptionObject = item;
                    }
                }
            }
            return influenceParam != null;
        }

        public void ConfigureAllInfluenceParameters()
        {
            if (IsCSharpFormula)
            {
                if (EvalInstance?.ParamsObj != null)
                {
                    foreach (var propInfo in EvalInstance.ParamsObj.GetType().GetProperties())
                    {
                        object oParam = propInfo.GetValue(EvalInstance.ParamsObj);
                        if (oParam != null)
                            ConfigureCSharpInfluenceParameter(oParam);
                    }
                }
            }
            else
            {
                foreach (var parm in BaseParameters)
                {
                    ConfigureInfluenceParameter(parm);
                }
            }
        }

        private void ParseParametersXml(XmlNode node)
        {
            if (FormulaExpression == null)
                return;
            foreach (XmlNode childNode in node.ChildNodes)
            {
                string parameterName =
                    (string)Tools.GetXmlAttribute("Name", typeof(string), childNode);
                BaseParameter prm = FormulaExpression.GetParameter(parameterName);
                if (prm != null)
                {
                    string typeName = Tools.GetXmlAttribute("TypeName", typeof(string), childNode,
                                                            required: false) as string;
                    if (typeName != null && prm.GetType().Name != typeName)
                    {
                        throw new Exception("Invalid parameter type read from XML file.");
                    }
                    //ConfigureInfluenceParameter(prm);
                    bool haveSetValue = false;
                    object val;
                    if (prm is DoubleParameter)
                    {
                        var customParam = prm as CustomParameter;
                        if (customParam != null)
                        {
                            Tools.GetXmlAttributesExcept(customParam.Context, childNode,
                                  excludedPropertyNames: new string[] { "Name", "TypeName", "Value",
                                                                        "RandomStrength", "CustomType" });
                            continue;
                        }
                        else
                            val = (double)Tools.GetXmlAttribute("Value", typeof(double), childNode);
                    }
                    else if (prm.HasChoices || prm is GenericParameter<DoublePoint>)
                    {
                        string valText = (string)Tools.GetXmlAttribute("Value", typeof(string), childNode);
                        prm.SelectedText = valText;
                        val = valText;
                        if (prm.HasChoices)
                        {
                            var choice = prm.GetParameterChoice(valText);
                            if (choice == null)
                            {
                                var fnParam = prm as VarFunctionParameter;
                                if (fnParam?.DefaultValue != null)
                                {
                                    choice = fnParam.GetParameterChoice(fnParam.DefaultValue);
                                }
                            }
                            if (choice != null)
                            {
                                prm.SetValueFromParameterChoice(choice);
                            }
                        }
                        haveSetValue = true;
                    }
                    else if (prm is BooleanParameter)
                    {
                        val = (bool)Tools.GetXmlAttribute("Value", typeof(bool), childNode);
                    }
                    else if (prm is ComplexParameter)
                    {
                        object re = Tools.GetXmlAttribute("Re", typeof(double), childNode);
                        object im = Tools.GetXmlAttribute("Im", typeof(double), childNode);
                        if (re is double && im is double)
                        {
                            val = new Complex((double)re, (double)im);
                        }
                        else
                            continue;
                    }
                    else
                    {
                        continue;
                    }
                    if (!haveSetValue)
                        SetParameterOrCustomValue(prm, val);
                    Parameter parameter = prm as Parameter;
                    if (parameter != null)
                    {
                        string guid = (string)Tools.GetXmlAttribute("Guid", typeof(string),
                                                                    childNode, required: false);
                        if (guid != null)
                            parameter.Guid = Guid.Parse(guid);
                    }
                }
            }
        }

        private void ParseCSharpParamsXml(XmlNode node)
        {
            object targetParamsObj = EvalInstance?.ParamsObj;
            if (targetParamsObj == null)
                return;
            ParseCSharpParamsXml(node, out bool updateParams, forArrayParams: false, targetParamsObj);
            if (updateParams)
                EvalInstance.UpdateParameters();
            ParseCSharpParamsXml(node, out updateParams, forArrayParams: true, targetParamsObj);
        }

        public static void ParseCSharpParamsXml(XmlNode node, object paramsObj)
        {
            ParseCSharpParamsXml(node, out _, forArrayParams: false, paramsObj);
        }

        private static void ParseCSharpParamsXml(XmlNode node, out bool updateParams, bool forArrayParams, object paramsObj)
        {
            updateParams = false;
            if (paramsObj == null)
                return;
            foreach (XmlNode subNode in node.ChildNodes)
            {
                string parameterName = Tools.GetXmlAttribute<string>(subNode, "Name");
                var propInfo = paramsObj.GetType().GetProperty(parameterName);
                if (propInfo == null)
                    continue;
                if (forArrayParams != propInfo.PropertyType.IsArray)
                    continue;
                string typeName = Tools.GetXmlAttribute<string>(subNode, defaultValue: null, "TypeName");
                if (typeName != null && propInfo.PropertyType.Name != typeName)
                {
                    if (typeName.Replace("Func1Parameter`1", "DoubleFuncParameter") != propInfo.PropertyType.Name)
                    {
                        throw new Exception("Invalid parameter type read from XML file.");
                    }
                }
                object oParam = propInfo.GetValue(paramsObj);
                Array paramArray = null;
                if (forArrayParams)
                {
                    paramArray = oParam as Array;
                }
                ParseCSharpParamXml(subNode, oParam, propInfo, paramArray, ref updateParams, paramsObj);
            }
        }

        private static void ParseCSharpParamXml(XmlNode subNode, object oParam, PropertyInfo propInfo, 
                                                Array paramArray, ref bool updateParams, object paramsObj)
        {
            bool isNestedParams = Tools.GetXmlAttribute(subNode, defaultValue: false, "NestedParameters");
            int index = -1;
            Type paramType;
            ParameterInfoAttribute infoAttr = propInfo.GetCustomAttribute<ParameterInfoAttribute>();
            if (infoAttr != null && !infoAttr.IsParameter)
                return;
            if (paramArray != null)
            {
                index = Tools.GetXmlAttribute<int>(subNode, -1, "Index");
                if (index == -1 || index >= paramArray.Length)
                    return;
                oParam = paramArray.GetValue(index);
                paramType = paramArray.GetType().GetElementType();
            }
            else
            {
                paramType = propInfo.PropertyType;
            }
            var iOptionsParam = oParam as IOptionsParameter;
            string sVal;
            bool valueIsString = paramType == typeof(string) ||
                                 paramType == typeof(CompiledDoubleFuncParameter);
            if (valueIsString)
            {
                XmlNode valNode = subNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "ValueString");
                if (valNode != null)
                    sVal = valNode.InnerText ?? string.Empty;
                else
                    sVal = null;
            }
            else
            {
                var valueAttr = subNode.Attributes["Value"];
                sVal = valueAttr?.Value;
            }
            if (!isNestedParams && !valueIsString && sVal == null)
                throw new Exception($"No value found for Xml Parameter {propInfo.Name}.");
            //var iFnParam = oParam as IFuncParameter;
            if (iOptionsParam != null)
            {
                if (sVal != null)
                {
                    try
                    {
                        iOptionsParam.SelectedText = sVal;
                        if (!(oParam is OptionsParameter<InfluencePointInfo>))
                        {
                            object prevVal = iOptionsParam.SelectedOptionObject;
                            iOptionsParam.SelectedOptionObject = iOptionsParam.GetOptionByText(sVal);
                            if (infoAttr != null && infoAttr.UpdateParametersOnChange && 
                                !object.Equals(iOptionsParam.SelectedOptionObject, prevVal))
                            {
                                updateParams = true;
                            }
                        }
                    }
                    catch { }
                }
            }
            else if (oParam is CompiledDoubleFuncParameter compiledFuncParam)
            {
                if (!string.IsNullOrEmpty(sVal))
                    compiledFuncParam.CompileFormula(sVal, editOnError: false);
            }
            else if (!isNestedParams && !(oParam is RandomParameter))
            {
                object oVal = Tools.GetCSharpParameterValue(sVal, paramType, out bool isValid);
                if (isValid)
                {
                    if (paramArray != null)
                        paramArray.SetValue(oVal, index);
                    else if (propInfo.CanWrite)
                    {
                        propInfo.SetValue(paramsObj, oVal);
                        if (infoAttr != null && infoAttr.UpdateParametersOnChange && !object.Equals(oVal, oParam))
                            updateParams = true;
                    }
                    else
                        throw new Exception($"Property {paramsObj.GetType().Name}.{propInfo.Name} is not writable.");
                }
            }
            if (isNestedParams)
            {
                if (oParam != null)
                {
                    XmlNode paramsNode = subNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "Parameters");
                    if (paramsNode == null)
                        throw new Exception("Expecting XmlNode named Parameters.");
                    object nestedParamsObj;
                    if (oParam is Func1Parameter<double> fnParam)
                        nestedParamsObj = fnParam.Instances?.FirstOrDefault();
                    else if (oParam is CompiledDoubleFuncParameter compiledFuncParam)
                        nestedParamsObj = compiledFuncParam.ParamsObject;
                    else
                        nestedParamsObj = oParam;
                    if (nestedParamsObj != null)
                        ParseCSharpParamsXml(paramsNode, paramsObj: nestedParamsObj);
                }
            }
        }

        //private const string randomRangeParamXmlNodeName = "RandomRangeParameter";

        private void AppendCSharpParametersToXml(XmlNode node, XmlTools xmlTools)
        {
            AppendCSharpParametersToXml(node, xmlTools, EvalInstance?.ParamsObj);
        }

        public static void AppendCSharpParametersToXml(XmlNode node, XmlTools xmlTools, object paramsObject)
        {
            if (paramsObject == null)
                return;
            XmlNode childNode = xmlTools.CreateXmlNode("Parameters");
            foreach (var propInfo in CSharpSharedCompiledInfo.GetDisplayedParameters(paramsObject))
            {
                object oVal = propInfo.GetValue(paramsObject);
                if (oVal == null)
                    continue;
                if (oVal is Array paramArray)
                {
                    //var paramArray = (Array)oVal;
                    for (int index = 0; index < paramArray.Length; index++)
                    {
                        AppendCSharpParameterToXml(childNode, paramArray.GetValue(index), propInfo, xmlTools, index);
                    }
                }
                else
                {
                    AppendCSharpParameterToXml(childNode, oVal, propInfo, xmlTools);
                }
            }
            node.AppendChild(childNode);
        }

        private static void AppendCSharpParameterToXml(XmlNode childNode, object paramValue, PropertyInfo propInfo, 
                                                       XmlTools xmlTools, int index = -1)
        {
            if (paramValue == null || paramValue is RandomParameter)
                return;
            bool isNestedParams = propInfo.GetCustomAttribute<NestedParametersAttribute>() != null;
            XmlNode subNode;
            string sValue;
            object nestedParamsObj = null;
            var iOptionsParam = paramValue as IOptionsParameter;
            if (iOptionsParam != null)
            {
                sValue = iOptionsParam.SelectedText;
                if (isNestedParams)
                {
                    var fnParam = iOptionsParam as Func1Parameter<double>;
                    if (fnParam?.Instances != null)
                        nestedParamsObj = fnParam.Instances.FirstOrDefault();
                }
            }
            else if (paramValue is CompiledDoubleFuncParameter compiledFuncParam)
            {
                sValue = compiledFuncParam.Formula;
                isNestedParams = compiledFuncParam.ParamsObject != null;
                if (isNestedParams)
                    nestedParamsObj = compiledFuncParam.ParamsObject;
            }
            else if (isNestedParams)
            {
                nestedParamsObj = paramValue;
                sValue = null;
            }
            else
            {
                sValue = paramValue.ToString();
            }
            subNode = xmlTools.CreateXmlNode("Parameter");
            xmlTools.AppendXmlAttribute(subNode, "Name", propInfo.Name);
            xmlTools.AppendXmlAttribute(subNode, "TypeName", propInfo.PropertyType.Name);
            if (sValue != null)
            {
                if (paramValue is string || paramValue is CompiledDoubleFuncParameter)
                    xmlTools.AppendChildNode(subNode, "ValueString", sValue);
                else
                    xmlTools.AppendXmlAttribute(subNode, "Value", sValue);
            }
            if  (index != -1)
                xmlTools.AppendXmlAttribute(subNode, "Index", index);
            if (isNestedParams && nestedParamsObj != null)
            {
                xmlTools.AppendXmlAttribute(subNode, "NestedParameters", true);
                AppendCSharpParametersToXml(subNode, xmlTools, paramsObject: nestedParamsObj);
            }
            childNode.AppendChild(subNode);
        }

        /// <summary>
        /// Note: Most parameter attributes are parsed from the formula, not saved in the xml node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xmlTools"></param>
        private void AppendParsedParametersToXml(XmlNode node, XmlTools xmlTools)
        {
            var parameters = BaseParameters; //.Where(p => 
                                             //p is Parameter || p is VarFunctionParameter || p is BooleanParameter || p is ComplexParameter);
            XmlNode childNode, subNode;
            if (parameters != null && parameters.Any())
            {
                childNode = xmlTools.CreateXmlNode("Parameters");
                foreach (BaseParameter prm in parameters)
                {
                    var parameter = prm as Parameter;
                    //if (parameter != null && parameter.ForInfluenceValue)
                    //    continue;
                    var randomRangeParam = prm as CustomParameter;
                    var complexParam = prm as ComplexParameter;
                    if (randomRangeParam != null)
                    {
                        if (randomRangeParam.CustomType != CustomParameterTypes.RandomRange)
                            continue;
                    }
                    else if (complexParam != null)
                    {
                        if (!(complexParam.Value is Complex))
                            continue;
                    }
                    else if (prm.Value is null)
                    {
                        continue;
                    }
                    subNode = xmlTools.CreateXmlNode(nameof(Parameter));
                    xmlTools.AppendXmlAttribute(subNode, "Name", prm.ParameterName);
                    xmlTools.AppendXmlAttribute(subNode, "TypeName", prm.GetType().Name);
                    if (parameter != null)
                    {
                        xmlTools.AppendXmlAttribute(subNode, "Value", parameter.ValueString);
                        xmlTools.AppendXmlAttribute(subNode, "Guid", parameter.Guid);
                    }
                    else
                    {
                        if (randomRangeParam != null)
                        {
                            foreach (PropertyInfo propInfo in RandomRange.ParameterProperties)
                            {
                                xmlTools.AppendXmlAttribute(subNode, propInfo.Name,
                                                            propInfo.GetValue(randomRangeParam.Context));
                            }
                        }
                        else if (complexParam != null)
                        {
                            Complex z = (Complex)complexParam.Value;
                            xmlTools.AppendXmlAttribute(subNode, "Re", z.Re);
                            xmlTools.AppendXmlAttribute(subNode, "Im", z.Im);
                        }
                        else if (prm.HasChoices)
                        {
                            string text = prm.SelectedChoice?.Text;
                            xmlTools.AppendXmlAttribute(subNode, "Value", text ?? string.Empty);
                        }
                        else
                        {
                            xmlTools.AppendXmlAttribute(subNode, "Value", prm.ValueString);
                        }
                    }
                    if (subNode != null)
                        childNode.AppendChild(subNode);
                }
                node.AppendChild(childNode);
            }
            //if (this.CustomParameters.Any())
            //{
            //    childNode = xmlTools.CreateXmlNode(nameof(CustomParameters));
            //    foreach (CustomParameter cprm in this.CustomParameters)
            //    {
            //        double? val;
            //        if (cprm.CustomType == CustomParameterTypes.RandomRange)
            //            val = ((RandomRange)cprm.Context).KeepCount;
            //        else
            //            val = (double?)cprm.Value;
            //        if (val != null)
            //        {
            //            subNode = xmlTools.CreateXmlNode(nameof(Parameter));
            //            xmlTools.AppendXmlAttribute(subNode, "Name", cprm.ParameterName);
            //            xmlTools.AppendXmlAttribute(subNode, "CustomType", cprm.CustomType);
            //            xmlTools.AppendXmlAttribute(subNode, "Value", val);
            //            childNode.AppendChild(subNode);
            //        }
            //    }
            //    node.AppendChild(childNode);
            //}

        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                throw new Exception("FormulaSettings xmlNodeName cannot be null.");
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttributes(node, this, nameof(IsValid), nameof(IsCSharpFormula), nameof(FormulaName));
            if (TestCSharpEvalType != null)
                xmlTools.AppendXmlAttribute(node, "TestCSharpEvalType", TestCSharpEvalType.FullName);
            xmlTools.AppendChildNode(node, nameof(Formula), Formula);
            if (IsCSharpFormula)
            {
                if (LegacyFormula != null)
                {
                    xmlTools.AppendChildNode(node, nameof(LegacyFormula), LegacyFormula);
                }
                AppendCSharpParametersToXml(node, xmlTools);
            }
            else
            {
                AppendParsedParametersToXml(node, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node, object parentObject)
        {
            XmlNode formulaNode = null, legacyFormulaNode = null, parametersNode = null, customParametersNode = null;
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "Formula")
                    formulaNode = childNode;
                else if (childNode.Name == "Parameters")
                    parametersNode = childNode;
                else if (childNode.Name == "CustomParameters" ||
                         childNode.Name == "RandomRangeParameters") //Legacy code
                    customParametersNode = childNode;
                else if (childNode.Name == "LegacyFormula")
                    legacyFormulaNode = childNode;
            }
            string typeName = Tools.GetXmlAttribute<string>(node, defaultValue: null, "TestCSharpEvalType");
            if (typeName != null)
                TestCSharpEvalType = Type.GetType(typeName, throwOnError: false);
            IsCSharpFormula = Tools.GetXmlAttribute<bool>(node, defaultValue: false, attrName: nameof(IsCSharpFormula));
            Formula = Tools.GetXmlNodeValue(formulaNode);
            if (legacyFormulaNode != null)
                LegacyFormula = Tools.GetXmlNodeValue(legacyFormulaNode);
            FormulaName = (string)Tools.GetXmlAttribute("FormulaName", typeof(string), node, required: false);
            if (string.IsNullOrEmpty(FormulaName))
                FormulaName = "Formula1";
            //bool isValid = (bool)Tools.GetXmlAttribute("IsValid", typeof(bool), node);
            if (Parse(Formula, throwException: false, ifChanged: false, initCSharpParams: false, 
                      displayWarnings: false, resolveInfluenceReferences: false) != ParseStatusValues.Success)
            {
                if (!IsCSharpFormula)
                    HandleParseErrors(Parser.GetErrorMessages(), parentObject);
            }
            else
            {
                if (parametersNode != null)
                {
                    if (IsCSharpFormula)
                        ParseCSharpParamsXml(parametersNode);
                    else
                        ParseParametersXml(parametersNode);
                }
                if (customParametersNode != null && !IsCSharpFormula)
                {
                    ParseParametersXml(customParametersNode);
                }
                //PopulateParametersDataTables();
            }
        }

        public static bool ShouldHandleErrors { get; set; } = true;

        private void HandleParseErrors(IEnumerable<string> errorMessages, object parentObject)
        {
            if (!ShouldHandleErrors)
                return;
            var basicOutline = parentObject as BasicOutline;
            var transform = parentObject as PatternTransform;
            var renderInfo = parentObject as Pattern.RenderingInfo;
            var ribbon = parentObject as Ribbon;
            if (basicOutline?.customOutline != null || transform != null || renderInfo != null || ribbon != null)
            {
                string errText = "Error parsing formula. Edit the formula?" + Environment.NewLine +
                                 string.Join(Environment.NewLine, errorMessages.Take(5));
                switch (MessageBox.Show(errText, "Confirm", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.No:
                        return;
                    case DialogResult.Cancel:
                        ShouldHandleErrors = false;
                        return;
                    case DialogResult.Yes:
                        break;
                }
                using (var outlineForm = new OutlineFormulaForm())
                {
                    if (basicOutline?.customOutline != null)
                    {
                        outlineForm.Initialize(basicOutline);
                    }
                    else if (transform != null)
                    {
                        outlineForm.Initialize(transform, transform.SequenceNumber + 1);
                    }
                    else if (renderInfo != null)
                    {
                        outlineForm.Initialize(renderInfo);
                    }
                    else if (ribbon != null)
                    {
                        outlineForm.Initialize(ribbon);
                    }
                    outlineForm.ShowDialog();
                }
            }
        }

        public void FromXml(XmlNode node)
        {
            FromXml(node, null);
        }

        /// <summary>
        /// Get value of Random parameter with index = ratio * (Count - 1), where ratio is adjusted to be in range 0 <= ratio <= 1.
        /// </summary>
        /// <param name="prm"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static double GetRandomValue(BaseParameter prm, double ratio)
        {
            double retVal = 0;
            CustomParameter cprm = prm as CustomParameter;
            RandomRange rr = cprm?.Context as RandomRange;
            if (rr != null)
            {
                ratio = Math.Min(Math.Abs(ratio), (double)int.MaxValue / rr.Count);
                if (ratio > 1D)
                {
                    double floor = Math.Floor(ratio);
                    ratio = ratio - floor;
                    if ((int)floor % 2 == 1)
                        ratio = 1D - ratio;
                }
                int ind = (int)Math.Round(ratio * (rr.Count - 1));
                retVal = rr.GetValue(ind);
            }
            return retVal;
        }

        public string TranslateToCSharp(FormulaTypes formulaType)
        {
            string code, message = null;
            Type infoType = null;
            if (IsCSharpFormula)
                message = "The formula is already in C#.";
            else
            {
                infoType = GetFormulaInfoType(formulaType);
                if (infoType == null)
                    message = "C# translation is not yet supported for this type of formula.";
            }
            if (message != null)
            {
                MessageBox.Show(message);
                code = null;
            }
            else
            {
                var translator = new TranslateToCSharp();
                code = translator.TranslateFormula(FormulaExpression, Parser, infoType, TokensTransformInfo);
            }
            return code;
        }

        public static Type GetFormulaInfoType(FormulaTypes formulaType)
        {
            Type type;
            switch (formulaType)
            {
                case FormulaTypes.PixelRender:
                    type = typeof(PixelRenderInfo);
                    break;
                case FormulaTypes.Transform:
                    type = typeof(PatternTransform.FormulaInfo);
                    break;
                case FormulaTypes.PathVertices:
                    type = typeof(PathOutline.PathOutlineVars);
                    break;
                case FormulaTypes.Outline:
                    type = typeof(BasicOutline.CustomOutline.CustomOutlineInfo);
                    break;
                default:
                    type = null;
                    break;
            }
            return type;
        }
    }
}
