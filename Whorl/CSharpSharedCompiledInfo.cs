﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ParserEngine;
using System.CodeDom.Compiler;

namespace Whorl
{
    public class CSharpSharedCompiledInfo
    {
        public enum ErrorType
        {
            Error,
            Warning
        }
        public class ErrorInfo
        {
            public ErrorType ErrorType { get; }
            public string Message { get; }
            public int Line { get; }
            public int Column { get; }

            public ErrorInfo(string message, int line = -1, int column = -1, ErrorType errorType = ErrorType.Error)
            {
                Message = message;
                Line = line;
                Column = column;
                ErrorType = errorType;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append(ErrorType.ToString());
                if (Line >= 0)
                    sb.Append($" (ln {Line}, col {Column})");
                sb.Append(": ");
                sb.Append(Message);
                return sb.ToString();
            }
        }

        public const string WhorlEvalNamespace = "WhorlEval";
        public const string WhorlEvalClassName = "WhorlEvalClass";
        public const string EvalMethodName = "Eval";
        public const string InfoPropertyName = "Info";
        public const string ParametersPropertyName = "Parms";
        public const string UpdateParametersMethodName = "Update";
        public const string PreInitializeMethodName = "PreInitialize";
        public const string InitializeMethodName = "Initialize";
        public const string Initialize2MethodName = "Initialize2";

        //public string SourceCode { get; private set; }

        public Type EvalClassType { get; private set; }
        public PropertyInfo InfoPropertyInfo { get; private set; }
        public int ErrorCount
        {
            get { return Errors.Count(e => e.ErrorType == ErrorType.Error); }
        }
        public List<ErrorInfo> Errors { get; } = new List<ErrorInfo>();
        public string ErrorsText
        {
            get { return string.Join(Environment.NewLine, Errors); }
        }


        //private System.CodeDom.Compiler.CodeDomProvider compiler { get; set; }

        public MethodInfo PreInitMethodInfo { get; private set; }
        public MethodInfo InitMethodInfo { get; private set; }
        public MethodInfo Init2MethodInfo { get; private set; }
        public PropertyInfo ParametersPropertyInfo { get; private set; }
        public MethodInfo EvalMethodInfo { get; private set; }
        public MethodInfo UpdateParametersMethodInfo { get; private set; }

        private void AddError(string message)
        {
            Errors.Add(new ErrorInfo(message, errorType: ErrorType.Error));
        }

        private void AddWarning(string message)
        {
            Errors.Add(new ErrorInfo(message, errorType: ErrorType.Warning));
        }

        public static bool PropertyIsParameter(PropertyInfo propInfo)
        {
            var attr = propInfo.GetCustomAttribute<ParameterInfoAttribute>();
            return attr == null || attr.IsParameter;
        }

        public static bool IncludeParameter(PropertyInfo propInfo, bool allowAllParams = true,
                                                bool allowSettingsParams = false)
        {
            if (!ParamPropInfoIsValid(propInfo))
                return false;
            if (!allowAllParams && ParamPropIsNestedParams(propInfo))
                return false;
            var attr = propInfo.GetCustomAttribute<ParameterInfoAttribute>();
            return attr == null || attr.IsParameter || (allowSettingsParams && attr.IsSettings);
        }

        public static IEnumerable<PropertyInfo> GetIncludedParametersForType(Type paramsObjType, bool allowAllParams = true,
                                                bool allowSettingsParams = false)
        {
            return paramsObjType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(pi => IncludeParameter(pi, allowAllParams, allowSettingsParams));
        }

        public static IEnumerable<PropertyInfo> GetIncludedParameters(object paramsObj, bool allowAllParams = true,
                                                bool allowSettingsParams = false)
        {
            if (paramsObj == null)
                return null;
            else
                return GetIncludedParametersForType(paramsObj.GetType(), allowAllParams, allowSettingsParams);
        }


        public static bool ParamPropInfoIsValid(PropertyInfo propInfo)
        {
            bool isValid = propInfo.CanRead &&
                   (ParamTypeIsNonScalar(propInfo.PropertyType) ||
                   (ParamTypeIsScalar(propInfo.PropertyType) && propInfo.CanWrite) ||
                    ParamPropIsNestedParams(propInfo));
            if (!isValid && propInfo.PropertyType.IsClass)
            {
                var attr = propInfo.GetCustomAttribute<ParameterInfoAttribute>();
                if (attr != null && attr.IsSettings)
                    isValid = true;
            }
            return isValid;
        }

        public static bool ParamPropIsNestedParams(PropertyInfo propInfo)
        {
            return propInfo.GetCustomAttribute<NestedParametersAttribute>() != null;
        }

        public static bool ParamTypeIsScalar(Type paramType)
        {
            if (paramType.IsArray)
                paramType = paramType.GetElementType();
            return paramType == typeof(double) ||
                   paramType == typeof(bool) ||
                   paramType == typeof(int) ||
                   paramType == typeof(string) ||
                   paramType.GetTryParseMethod() != null;
        }

        public static bool ParamTypeIsNonScalar(Type paramType)
        {
            if (paramType.IsArray)
                paramType = paramType.GetElementType();
            return typeof(BaseCSharpParameter).IsAssignableFrom(paramType);
            //return typeof(IOptionsParameter).IsAssignableFrom(paramType) ||
            //       paramType == typeof(RandomParameter);
        }

        private void ValidateParameters(Type paramsObjType)
        {
            bool requiresUpdateMethod = false;
            foreach (PropertyInfo propertyInfo in paramsObjType.GetProperties())
            {
                var attr = propertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
                if (attr != null && !attr.IsParameter)
                    continue;
                if (!(propertyInfo.CanWrite && propertyInfo.CanRead) && 
                     (!propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType == typeof(string)))
                {
                    AddWarning($"Parameter {propertyInfo.Name} must be read/write.");
                }
                else if (!ParamPropInfoIsValid(propertyInfo))
                    AddWarning($"Parameter {propertyInfo.Name} is of invalid type or is read-only.");
                if (attr != null && attr.UpdateParametersOnChange)
                    requiresUpdateMethod = true;
            }
            if (requiresUpdateMethod)
            {
                MethodInfo methodInfo = paramsObjType.GetMethod(UpdateParametersMethodName);
                if (methodInfo == null)
                    AddWarning("There must be an Update method in your parameters class.");
            }
        }

        private void InitCompile()
        {
            Errors.Clear();
            EvalClassType = null;
            InfoPropertyInfo = null;
            InitMethodInfo = null;
            PreInitMethodInfo = null;
            EvalMethodInfo = null;
            UpdateParametersMethodInfo = null;
            ParametersPropertyInfo = null;
        }

        private void SetCompiledProperties(bool forFormula)
        {
            if (forFormula)
            {
                InfoPropertyInfo = EvalClassType.GetProperty(InfoPropertyName);
                if (InfoPropertyInfo == null)
                {
                    AddError($"Couldn't find property {InfoPropertyName}.");
                }
            }
            ParametersPropertyInfo = EvalClassType.GetProperty(ParametersPropertyName);
            if (ParametersPropertyInfo != null)
            {
                Type parametersClassType = ParametersPropertyInfo.PropertyType;
                ValidateParameters(parametersClassType);
                UpdateParametersMethodInfo = GetActionMethod(UpdateParametersMethodName,
                                             parametersClassType);
            }
            if (forFormula)
            {
                PreInitMethodInfo = GetActionMethod(PreInitializeMethodName, EvalClassType);
                InitMethodInfo = GetActionMethod(InitializeMethodName, EvalClassType);
                Init2MethodInfo = GetActionMethod(Initialize2MethodName, EvalClassType);
                EvalMethodInfo = GetActionMethod(EvalMethodName, EvalClassType,
                                                 required: true, returnType: typeof(void));
            }
        }

        private MethodInfo GetActionMethod(string methodName, Type declaringType,
                                           bool required = false, Type returnType = null)
        {
            var methodInfo = declaringType.GetMethod(methodName);
            if (methodInfo != null)
            {
                if (returnType != null && methodInfo.ReturnType != returnType)
                {
                    AddError($"Method {declaringType.Name}.{methodName} must return {returnType.Name}.");
                }
                if (methodInfo.GetParameters().Any())
                {
                    AddError($"Method {declaringType.Name}.{methodName} cannot have parameters.");
                }
            }
            else if (required)
                AddError($"Didn't find method {declaringType.Name}.{methodName}.");
            return methodInfo;
        }

        public void UseCompiledType(Type evalClassType)
        {
            //SourceCode = string.Empty;
            InitCompile();
            EvalClassType = evalClassType;
            SetCompiledProperties(forFormula: true);
        }

        public static bool CompileCustomParameterClasses()
        {
            CompilerResults results = CSharpCompiler.Instance.CompileCustomParameterClasses(
                                      out string code, out Assembly assembly);
            if (assembly != null)
            {
                CSharpCompiler.Instance.AddReferencedAssembly(assembly);
                return results != null;
            }
            else if (results != null)
            {
                string errorList = GetErrorList(results);
                if (errorList != null)
                {
                    var frm = new frmTextEditor();
                    frm.Text = "Compiler errors for custom parameter classes.";
                    frm.DisplayText(errorList, readOnly: true);
                    frm.AddRelatedText(code, "Parameter Classes Code");
                    frm.ShowDialog();
                }
            }
            return false;
        }

        public static IEnumerable<ErrorInfo> GetErrors(CompilerResults compilerResults)
        {
            return from CompilerError err in compilerResults.Errors
                   select new ErrorInfo(err.ErrorText, err.Line, err.Column, ErrorType.Error);
        }

        public static string GetErrorList(CompilerResults compilerResults)
        {
            var errors = GetErrors(compilerResults);
            return errors.Any() ? string.Join(Environment.NewLine, errors) : null;
        }

        public void SetCompilerResults(CompilerResults compilerResults, bool forFormula = true)
        {
            InitCompile();
            Errors.AddRange(from CompilerError err in compilerResults.Errors
                            select new ErrorInfo(err.ErrorText, err.Line, err.Column, ErrorType.Error));
            if (Errors.Count == 0)
            {
                try
                {
                    EvalClassType = compilerResults.CompiledAssembly.GetType($"{WhorlEvalNamespace}.{WhorlEvalClassName}",
                                    throwOnError: true);
                    SetCompiledProperties(forFormula);
                }
                catch (Exception ex)
                {
                    AddError(ex.Message);
                }
            }
        }

        //public void CompileCode(string code) /*, CSharpCodeProvider csProvider, CompilerParameters parameters) */
        //{
        //    SourceCode = code;
        //    InitCompile();
        //    CompilerResults results = CSharpCompiler.Instance.CompileCode(code);
        //    //CompilerResults results = csProvider.CompileAssemblyFromSource(parameters, code);
        //    Errors.AddRange(from CompilerError err in results.Errors
        //                    select new ErrorInfo(err.ErrorText, err.Line, err.Column, ErrorType.Error));
        //    if (Errors.Count == 0)
        //    {
        //        try
        //        {
        //            EvalClassType = results.CompiledAssembly.GetType($"{WhorlEvalNamespace}.{WhorlEvalClassName}",
        //                            throwOnError: true);
        //            SetCompiledProperties();
        //        }
        //        catch (Exception ex)
        //        {
        //            AddError(ex.Message);
        //        }
        //    }
        //}
    }
}
