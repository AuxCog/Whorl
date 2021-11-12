using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using ParserEngine;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

namespace Whorl
{
    public class CSharpCompiledInfo
    {
        public class EvalInstance
        {
            public CSharpCompiledInfo CSharpCompiledInfo { get; }
            public object ClassInstance { get; }
            public object ParamsObj { get; }
            public Action EvalFormula { get; }

            public EvalInstance(CSharpCompiledInfo cSharpCompiledInfo)
            {
                CSharpCompiledInfo = cSharpCompiledInfo;
                ClassInstance = Activator.CreateInstance(CSharpCompiledInfo.EvalClassType);
                if (CSharpCompiledInfo.ParametersPropertyInfo != null)
                    ParamsObj = CSharpCompiledInfo.ParametersPropertyInfo.GetValue(ClassInstance);
                try
                {
                    EvalFormula = (Action)Delegate.CreateDelegate(typeof(Action), ClassInstance, 
                                          CSharpCompiledInfo.EvalMethodInfo);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating delegate: " + ex.Message);
                }
            }

            public void InitializeForEval()
            {
                if (CSharpCompiledInfo.InitMethodInfo != null)
                {
                    try
                    {
                        CSharpCompiledInfo.InitMethodInfo.Invoke(ClassInstance, null);
                    }
                    catch (Exception ex)
                    {
                        Tools.HandleException(ex);
                    }
                }
            }

            public void SetInfoObject(object info)
            {
                CSharpCompiledInfo.InfoPropertyInfo.SetValue(ClassInstance, info);
            }

            public bool UpdateParameters()
            {
                if (ParamsObj == null || CSharpCompiledInfo.UpdateParametersMethodInfo == null)
                    return false;
                CSharpCompiledInfo.UpdateParametersMethodInfo.Invoke(ParamsObj, null);
                return true;
            }

            public IEnumerable<PropertyInfo> GetParameterPropertyInfos(bool allowAllParams = true)
            {
                if (ParamsObj == null)
                    return null;
                else
                    return ParamsObj.GetType().GetProperties()
                                    .Where(pi => CSharpCompiledInfo.ParamPropInfoIsValid(pi, allowAllParams));
            }

            public List<ParamInfo> GetParamInfos()
            {
                var keys = new List<ParamInfo>();
                if (ParamsObj != null)
                {
                    foreach (var propertyInfo in GetParameterPropertyInfos(allowAllParams: false))
                    {
                        if (propertyInfo.PropertyType.IsArray)
                        {
                            object oValue = propertyInfo.GetValue(ParamsObj);
                            if (oValue != null)
                            {
                                var array = (Array)oValue;
                                keys.AddRange(Enumerable.Range(0, array.Length).Select(i => new ParamInfo(propertyInfo.Name, i)));
                            }
                        }
                        else
                            keys.Add(new ParamInfo(propertyInfo.Name));
                    }
                }
                return keys;
            }
        }

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
        public const string InitializeMethodName = "Initialize";

        public string SourceCode { get; private set; }
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

        public MethodInfo InitMethodInfo { get; set; }
        public PropertyInfo ParametersPropertyInfo { get; set; }
        public MethodInfo EvalMethodInfo { get; set; }
        public MethodInfo UpdateParametersMethodInfo { get; set; }

        private void AddError(string message)
        {
            Errors.Add(new ErrorInfo(message, errorType: ErrorType.Error));
        }

        private void AddWarning(string message)
        {
            Errors.Add(new ErrorInfo(message, errorType: ErrorType.Warning));
        }

        public static bool ParamPropInfoIsValid(PropertyInfo propInfo, bool allowAllParams = true)
        {
            return propInfo.CanRead &&
                   ((ParamTypeIsScalar(propInfo.PropertyType) && propInfo.CanWrite) ||
                     ParamTypeIsNonScalar(propInfo.PropertyType) ||
                     (allowAllParams && ParamPropIsNestedParams(propInfo)));
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
            return typeof(IOptionsParameter).IsAssignableFrom(paramType) ||
                   paramType == typeof(RandomParameter);
        }

        private void ValidateParameters(Type paramsObjType)
        {
            bool requiresUpdateMethod = false;
            foreach (PropertyInfo propertyInfo in paramsObjType.GetProperties())
            {
                if (!(propertyInfo.CanWrite && propertyInfo.CanRead))
                {
                    if (!propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType == typeof(string))
                        AddWarning($"Parameter {propertyInfo.Name} must be read/write.");
                }
                if (!ParamPropInfoIsValid(propertyInfo))
                    AddWarning($"Parameter {propertyInfo.Name} is of invalid type or is read-only.");
                var attr = propertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
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

        public EvalInstance CreateEvalInstance()
        {
            if (ErrorCount != 0)
                return null;
            return new EvalInstance(this);
        }

        private void InitCompile()
        {
            Errors.Clear();
            EvalClassType = null;
            InfoPropertyInfo = null;
            InitMethodInfo = null;
            EvalMethodInfo = null;
            UpdateParametersMethodInfo = null;
            ParametersPropertyInfo = null;
        }

        private void SetCompiledProperties()
        {
            InfoPropertyInfo = EvalClassType.GetProperty(InfoPropertyName);
            if (InfoPropertyInfo == null)
            {
                AddError($"Couldn't find property {InfoPropertyName}.");
            }
            ParametersPropertyInfo = EvalClassType.GetProperty(ParametersPropertyName);
            if (ParametersPropertyInfo != null)
            {
                Type parametersClassType = ParametersPropertyInfo.PropertyType;
                ValidateParameters(parametersClassType);
                UpdateParametersMethodInfo = GetActionMethod(UpdateParametersMethodName,
                                             parametersClassType);
            }
            InitMethodInfo = GetActionMethod(InitializeMethodName, EvalClassType);
            EvalMethodInfo = GetActionMethod(EvalMethodName, EvalClassType, 
                                             required: true, returnType: typeof(void));
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
            SourceCode = string.Empty;
            InitCompile();
            EvalClassType = evalClassType;
            SetCompiledProperties();
        }

        public void CompileCode(string code) /*, CSharpCodeProvider csProvider, CompilerParameters parameters) */
        {
            SourceCode = code;
            InitCompile();
            CompilerResults results = CSharpCompiler.Instance.CompileCode(code);
            //CompilerResults results = csProvider.CompileAssemblyFromSource(parameters, code);
            Errors.AddRange(from CompilerError err in results.Errors
                            select new ErrorInfo(err.ErrorText, err.Line, err.Column, ErrorType.Error));
            if (Errors.Count == 0)
            {
                try
                {
                    EvalClassType = results.CompiledAssembly.GetType($"{WhorlEvalNamespace}.{WhorlEvalClassName}", 
                                    throwOnError: true);
                    SetCompiledProperties();
                }
                catch (Exception ex)
                {
                    AddError(ex.Message);
                }
            }
        }
    }

    public class CSharpCompiler
    {
        class CompilerSettings : ICompilerSettings
        {
            private const string compilerDirectory = @"c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin";
            private readonly CompilerLanguage _compilerLang;
            private readonly string _rootDirectory;
            private string _compilerPath => _compilerLang == CompilerLanguage.CSharp
                                                    ? @"roslyn\csc.exe"
                                                    : @"roslyn\vbc.exe";
            public CompilerSettings(CompilerLanguage compiler = CompilerLanguage.CSharp, bool useCustomDirectory = true)
            {
                _compilerLang = compiler;
                if (useCustomDirectory)
                    _rootDirectory = compilerDirectory;
                else
                    _rootDirectory = new Uri(Assembly.GetCallingAssembly().CodeBase).LocalPath;
            }
            public string CompilerFullPath => Path.Combine(_rootDirectory, _compilerPath);
            public int CompilerServerTimeToLive => 10; //60 * 300;
            public enum CompilerLanguage
            {
                CSharp,
                VB
            }
        }

        private static Lazy<CSharpCompiler> Lazy = new Lazy<CSharpCompiler>(() => new CSharpCompiler());

        public static CSharpCompiler Instance
        {
            get { return Lazy.Value; }
        }

        private CSharpCodeProvider csProvider { get; }
        private CompilerParameters compilerParameters { get; }

        private Dictionary<string, CompilerResults> compiledDict { get; } =
            new Dictionary<string, CompilerResults>();

        private CSharpCompiler()
        {
            //csProvider = new CSharpCodeProvider(new CompilerSettings());
            //compiler = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
            //compiler = Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider.CreateProvider("cs", 
            //           new Dictionary<string, string> { { "CompilerDirectoryPath", Path.Combine(Environment.CurrentDirectory, "roslyn") }});
            csProvider = new CSharpCodeProvider(new CompilerSettings());

            string options = "-langversion:8.0";
            compilerParameters = new CompilerParameters() 
                { GenerateExecutable = false, GenerateInMemory = true, CompilerOptions = options };
            compilerParameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            compilerParameters.ReferencedAssemblies.Add(typeof(System.Windows.Forms.Control).Assembly.Location);
            compilerParameters.ReferencedAssemblies.Add(typeof(System.Drawing.Size).Assembly.Location);
            compilerParameters.ReferencedAssemblies.Add(typeof(PixelRenderInfo).Assembly.Location);
            compilerParameters.ReferencedAssemblies.Add(typeof(ParserEngine.OutlineMethods).Assembly.Location);
        }

        public CompilerResults CompileCode(string code)
        {
            if (!compiledDict.TryGetValue(code, out CompilerResults results))
            {
                results = csProvider.CompileAssemblyFromSource(compilerParameters, code);
                compiledDict.Add(code, results);
            }
            return results;
        }

        //public CSharpCompiledInfo CompileCode(string code)
        //{
        //    string options = "-langversion:8.0";
        //    var compilerParameters = new CompilerParameters() { GenerateExecutable = false, GenerateInMemory = true, CompilerOptions = options };
        //    compilerParameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
        //    compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
        //    compilerParameters.ReferencedAssemblies.Add(typeof(System.Windows.Forms.Control).Assembly.Location);
        //    compilerParameters.ReferencedAssemblies.Add(typeof(System.Drawing.Size).Assembly.Location);
        //    compilerParameters.ReferencedAssemblies.Add(typeof(PixelRenderInfo).Assembly.Location);
        //    compilerParameters.ReferencedAssemblies.Add(typeof(ParserEngine.OutlineMethods).Assembly.Location);

        //    var info = new CSharpCompiledInfo();
        //    info.CompileCode(code, csProvider, compilerParameters);
        //    return info;
        //}
    }
}
