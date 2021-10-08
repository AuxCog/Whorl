﻿using ParserEngine;
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

    public class FormulaSettings : BaseObject, IXml
    {
        //public Pattern ParentPattern { get; }

        public string Formula { get; private set; }

        public string LegacyFormula { get; private set; }

        public string FormulaName { get; set; } = "Formula1";

        public FormulaTypes FormulaType { get; set; }

        public bool IsCSharpFormula { get; set; }

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
                        testCSharpCompiledInfo = new CSharpCompiledInfo();
                    testCSharpCompiledInfo.UseCompiledType(_testCSharpEvalType);
                    _testEvalInstance = testCSharpCompiledInfo.CreateEvalInstance();
                    if (_evalInstance != null)
                        CopyCSharpParameters(_evalInstance.ParamsObj, _testEvalInstance);
                }
                SetEvalInstance();
            }
        }
        public CSharpCompiledInfo.EvalInstance EvalInstance { get; private set; }
        public TokensTransformInfo TokensTransformInfo { get; set; }
        private CSharpCompiledInfo.EvalInstance _evalInstance;
        private CSharpCompiledInfo.EvalInstance _testEvalInstance;

        private void SetEvalInstance()
        {
            EvalInstance = TestCSharpEvalType == null ? _evalInstance : _testEvalInstance;
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
        //public bool FormulaChanged { get; private set; }
        //public bool ParseOnChanges { get; set; }
        public bool IsValid
        {
            get
            {
                if (IsCSharpFormula)
                    return EvalInstance != null && !CSharpCompiledInfo.Errors.Any();
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
                return BaseParameters.Where(prm => prm is Parameter)
                                     .Select(prm => (Parameter)prm);
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

        public FormulaSettings(FormulaTypes formulaType, ExpressionParser parser = null)
        {
            FormulaType = formulaType;
            cSharpCompiledInfo = new CSharpCompiledInfo();
            if (parser == null)
                parser = CreateExpressionParser();
            Parser = parser;
        }

        public FormulaSettings(FormulaSettings source, configureDelegate configureParser = null, 
                               ExpressionParser parser = null)
        {
            FormulaType = source.FormulaType;
            FormulaName = source.FormulaName;
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
                    _evalInstance = cSharpCompiledInfo.CreateEvalInstance();
                    SetEvalInstance();
                    CopyCSharpParameters(SavedParamsObj);
                }
                else
                {
                    FormulaExpression = source.FormulaExpression;
                    Parse(source.Formula, ifChanged: false, displayWarnings: false);
                }
            }
        }

        public FormulaSettings GetCopy(configureDelegate configureParser = null, ExpressionParser parser = null)
        {
            return new FormulaSettings(this, configureParser, parser);
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
            ParseStatusValues parseStatus;
            bool isValid;
            bool preprocessorErrors = false;
            CSharpCompiledInfo compiledInfo = null;
            var processor = CSharpPreprocessor.Instance;
            ParserErrorsForm.ResetForm();
            string cSharpCode = null;
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
                    compiledInfo = new CSharpCompiledInfo();
                    compiledInfo.CompileCode(cSharpCode);
                    //compiledInfo = CSharpCompiler.Instance.CompileCode(cSharpCode);
                    if (!isModule)
                        this.cSharpCompiledInfo = compiledInfo;
                    isValid = compiledInfo.EvalClassType != null && !compiledInfo.Errors.Any();
                    if (isValid && !isModule)
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
                                CopyCSharpParameters(SavedParamsObj, _evalInstance);
                            SavedParamsObj = _evalInstance.ParamsObj;
                        }
                    }
                }
                if (!isValid && !isModule)
                {
                    _evalInstance = null;
                }
                SetEvalInstance();
            }
            else
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
                        else if (compiledInfo != null)
                            errMsg = compiledInfo.ErrorsText;
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
                        if (!preprocessorErrors && compiledInfo == null)
                        {
                            MessageBox.Show("Couldn't compile C# code.");
                        }
                        else
                        {
                            if (preprocessorErrors)
                                cSharpCode = formula;
                            if (preprocessorErrors || compiledInfo.Errors.Any())
                            {
                                frm.Initialize(this, formulaForm, preprocessorErrors, 
                                               compiledInfo, cSharpCode);
                                Tools.DisplayForm(frm);
                            }
                            else if (compiledInfo.EvalClassType == null)
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
                        InfluenceLinkParentCollection.InfluenceLinkParentsByParameterName.Clear();
                    }
                    else
                    {
                        InfluenceLinkParentCollection.ResolveReferences(throwException: false);
                    }
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

        public void InitializeGlobals()
        {
            if (IsCSharpFormula)
            {
                if (EvalInstance != null)
                    EvalInstance.InitializeForEval();
            }
            else
            {
                if (FormulaExpression != null)
                    FormulaExpression.InitializeGlobals();
            }
        }

        public bool EvalStatements(CSharpCompiledInfo.EvalInstance evalInstance, bool throwException = true)
        {
            if (IsCSharpFormula)
            {
                evalInstance.EvalFormula();
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
                        MessageBox.Show(messages);
                    return false;
                }
            }
            return true;
        }

        public bool EvalStatements(bool throwException = true)
        {
            //if (IsCSharpFormula && EvalInstance == null)
            //    throw new NullReferenceException("EvalInstance is null.");
            //if (!HaveParsedFormula)
            //    return false;
            return EvalStatements(EvalInstance, throwException);
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

        public void CopyCSharpParameters(object sourceParamsObj, CSharpCompiledInfo.EvalInstance evalInstance = null)
        {
            evalInstance = evalInstance ?? EvalInstance;
            if (!IsCSharpFormula || sourceParamsObj == null || evalInstance == null || evalInstance.ParamsObj == null)
                return;
            HashSet<string> handledPropertyNames = new HashSet<string>();
            CheckUpdateParameters(sourceParamsObj, handledPropertyNames, evalInstance);
            Type sourceType = sourceParamsObj.GetType();
            Type targetType = evalInstance.ParamsObj.GetType();
            bool typesMatch = sourceType == targetType;
            if (!typesMatch)
            {
                var arrayPropsWithAttrs = targetType.GetProperties()
                                          .Where(pi => pi.PropertyType.IsArray && 
                                          pi.GetCustomAttribute<ArrayBaseNameAttribute>() != null);
                foreach (PropertyInfo arrayPropInfo in arrayPropsWithAttrs)
                {
                    var arrayAttr = arrayPropInfo.GetCustomAttribute<ArrayBaseNameAttribute>();
                    object targetParam = arrayPropInfo.GetValue(evalInstance.ParamsObj);
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
                                CopyCSharpParameter(sourceElemParam, targetElemParam, null, evalInstance, targetArray, i);
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
                if (typesMatch || (targetPropInfo != null && Tools.TypesMatch(targetPropInfo.PropertyType, propInfo.PropertyType)))
                {
                    object sourceParam = propInfo.GetValue(sourceParamsObj);
                    object targetParam = targetPropInfo.GetValue(evalInstance.ParamsObj);
                    if (sourceParam != null && propInfo.PropertyType.IsArray && targetParam != null)
                    {
                        var sourceArray = (Array)sourceParam;
                        var targetArray = (Array)targetParam;
                        for (int i = 0; i < Math.Min(sourceArray.Length, targetArray.Length); i++)
                        {
                            object sourceElemParam = sourceArray.GetValue(i);
                            object targetElemParam = targetArray.GetValue(i);
                            CopyCSharpParameter(sourceElemParam, targetElemParam, null, evalInstance, targetArray, i);
                        }
                    }
                    else
                    {
                        CopyCSharpParameter(sourceParam, targetParam, targetPropInfo, evalInstance);
                    }
                }
            }
        }

        private void CopyCSharpParameter(object sourceParam, object targetParam, PropertyInfo targetPropInfo,
                                          CSharpCompiledInfo.EvalInstance evalInstance,
                                          Array paramArray = null, int index = 0)
        {
            var iOptionsParam = sourceParam as IOptionsParameter;
            if (iOptionsParam != null)
            {
                var iTargetOptionsParam = targetParam as IOptionsParameter;
                if (iTargetOptionsParam != null)
                {
                    object selOption = iTargetOptionsParam.GetOptionByText(iOptionsParam.SelectedText);
                    if (selOption != null)
                        iTargetOptionsParam.SelectedOptionObject = selOption;
                }
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
                    else
                        targetPropInfo.SetValue(evalInstance.ParamsObj, sourceParam);
                }
            }
        }

        private void CopyLegacyParametersToCSharp(IEnumerable<BaseParameter> baseParameters,
                                                  CSharpCompiledInfo.EvalInstance evalInstance)
        {
            if (!IsCSharpFormula || evalInstance?.ParamsObj == null || baseParameters == null)
                return;
            foreach (BaseParameter prm in baseParameters)
            {
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
                BaseParameter copyPrm = FormulaExpression.GetParameter(prm.ParameterName);
                if (copyPrm != null && copyPrm.GetType() == prm.GetType())
                {
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
                        if (copyPrm is Parameter)
                        {
                            ((Parameter)copyPrm).Guid = ((Parameter)prm).Guid;
                        }
                        SetParameterOrCustomValue(copyPrm, prm.Value);
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
            else if  (sourceFormulaSettings == this)
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
                        CopyCSharpParameters(sourceFormulaSettings.EvalInstance.ParamsObj);
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
                    else if (prm is VarFunctionParameter)
                    {
                        val = (string)Tools.GetXmlAttribute("Value", typeof(string), childNode);
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
            ParseCSharpParamsXml(node, out bool updateParams, forArrayParams: false);
            if (updateParams)
                EvalInstance.UpdateParameters();
            ParseCSharpParamsXml(node, out updateParams, forArrayParams: true);
        }

        private void ParseCSharpParamsXml(XmlNode node, out bool updateParams, bool forArrayParams)
        {
            updateParams = false;
            if (EvalInstance?.ParamsObj == null)
                return;
            foreach (XmlNode subNode in node.ChildNodes)
            {
                string parameterName =
                    (string)Tools.GetXmlAttribute("Name", typeof(string), subNode);
                var propInfo = EvalInstance.ParamsObj.GetType().GetProperty(parameterName);
                if (propInfo == null)
                    continue;
                if (forArrayParams != propInfo.PropertyType.IsArray)
                    continue;
                string typeName = Tools.GetXmlAttribute("TypeName", typeof(string), subNode,
                                                        required: false) as string;
                if (typeName != null && propInfo.PropertyType.Name != typeName)
                {
                    throw new Exception("Invalid parameter type read from XML file.");
                }
                object oParam = propInfo.GetValue(EvalInstance.ParamsObj);
                Array paramArray = null;
                if (forArrayParams)
                {
                    paramArray = oParam as Array;
                }
                ParseCSharpParamXml(subNode, oParam, propInfo, paramArray, ref updateParams);
            }
        }

        private void ParseCSharpParamXml(XmlNode subNode, object oParam, PropertyInfo propInfo, Array paramArray, ref bool updateParams)
        {
            int index = -1;
            Type paramType;
            ParameterInfoAttribute infoAttr;
            if (paramArray != null)
            {
                index = Tools.GetXmlAttribute<int>(subNode, -1, "Index");
                if (index == -1 || index >= paramArray.Length)
                    return;
                oParam = paramArray.GetValue(index);
                paramType = paramArray.GetType().GetElementType();
                infoAttr = null;
            }
            else
            {
                paramType = propInfo.PropertyType;
                infoAttr = propInfo.GetCustomAttribute<ParameterInfoAttribute>();
            }
            string sVal = (string)Tools.GetXmlAttribute("Value", typeof(string), subNode);
            var iOptionsParam = oParam as IOptionsParameter;
            //var iFnParam = oParam as IFuncParameter;
            if (iOptionsParam != null)
            {
                try
                {
                    object prevVal = iOptionsParam.SelectedOptionObject;
                    iOptionsParam.SelectedOptionObject = iOptionsParam.GetOptionByText(sVal);
                    if (infoAttr != null && infoAttr.UpdateParametersOnChange && !object.Equals(iOptionsParam.SelectedOptionObject, prevVal))
                        updateParams = true;
                }
                catch { }
            }
            else if (!(oParam is RandomParameter))
            {
                object oVal = Tools.GetCSharpParameterValue(sVal, paramType, out bool isValid);
                //try
                //{
                //    oVal = Convert.ChangeType(sVal, paramType);
                //}
                //catch
                //{
                //    return;
                //}
                if (isValid)
                {
                    if (paramArray != null)
                        paramArray.SetValue(oVal, index);
                    else
                    {
                        propInfo.SetValue(EvalInstance.ParamsObj, oVal);
                        if (infoAttr != null && infoAttr.UpdateParametersOnChange && !object.Equals(oVal, oParam))
                            updateParams = true;
                    }
                }
            }
        }

        //private const string randomRangeParamXmlNodeName = "RandomRangeParameter";

        private void AppendCSharpParametersToXml(XmlNode node, XmlTools xmlTools)
        {
            if (EvalInstance?.ParamsObj == null)
                return;
            XmlNode childNode = xmlTools.CreateXmlNode("Parameters");
            foreach (var propInfo in EvalInstance.ParamsObj.GetType().GetProperties())
            {
                object oVal = propInfo.GetValue(EvalInstance.ParamsObj);
                if (oVal == null)
                    continue;
                if (oVal.GetType().IsArray)
                {
                    var paramArray = (Array)oVal;
                    int index = 0;
                    foreach (object elem in paramArray)
                    {
                        AppendCSharpParameterToXml(childNode, elem, propInfo, xmlTools, index++);
                    }
                }
                else
                {
                    AppendCSharpParameterToXml(childNode, oVal, propInfo, xmlTools);
                }
                //string sValue;
                ////var iFnParam = oVal as IFuncParameter;
                ////if (iFnParam != null)
                ////    sValue = iFnParam.FunctionName;
                //var iOptionsParam = oVal as IOptionsParameter;
                //if (iOptionsParam != null)
                //{
                //    object selOption = iOptionsParam.SelectedOptionObject;
                //    if (selOption == null)
                //        continue;
                //    sValue = selOption.ToString();
                //}
                //else
                //    sValue = oVal.ToString();
                //subNode = xmlTools.CreateXmlNode("Parameter");
                //xmlTools.AppendXmlAttribute(subNode, "Name", propInfo.Name);
                //xmlTools.AppendXmlAttribute(subNode, "TypeName", propInfo.PropertyType.Name);
                //xmlTools.AppendXmlAttribute(subNode, "Value", sValue);
                //childNode.AppendChild(subNode);
            }
            node.AppendChild(childNode);
        }

        private void AppendCSharpParameterToXml(XmlNode childNode, object oVal, PropertyInfo propInfo, XmlTools xmlTools, int index = -1)
        {
            if (oVal == null || oVal is RandomParameter)
                return;
            XmlNode subNode;
            string sValue;
            var iOptionsParam = oVal as IOptionsParameter;
            if (iOptionsParam != null)
            {
                object selOption = iOptionsParam.SelectedOptionObject;
                if (selOption == null)
                    return;
                sValue = selOption.ToString();
            }
            else
                sValue = oVal.ToString();
            subNode = xmlTools.CreateXmlNode("Parameter");
            xmlTools.AppendXmlAttribute(subNode, "Name", propInfo.Name);
            xmlTools.AppendXmlAttribute(subNode, "TypeName", propInfo.PropertyType.Name);
            xmlTools.AppendXmlAttribute(subNode, "Value", sValue);
            if  (index != -1)
                xmlTools.AppendXmlAttribute(subNode, "Index", index);
            childNode.AppendChild(subNode);
        }

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
                    var parameter = prm as Parameter;
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