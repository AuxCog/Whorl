using ParserEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Whorl
{
    public class CompiledDoubleFuncParameter: BaseCSharpParameter, IRenderingValues
    {
        private const string usingStatements =
@"using ParserEngine;
using System;
using Whorl;
";
        public string Formula { get; private set; }
        public string NewFormula { get; private set; }
        public Func<double, double> Function { get; private set; }
        public object ParamsObject { get; private set; }
        public string ParamsClassCodeFilePath { get; }
        public Type ParamsClassType { get; private set; }
        public string FunctionName { get; }
        private Tokenizer tokenizer { get; }
        //private PropertyInfo renderingValuesPropertyInfo { get; set; }
        private object classInstance { get; set; }
        private Type parametersClassType { get; set; }

        private RenderingValues _renderingValues;
        public RenderingValues RenderingValues
        {
            get => _renderingValues;
            set
            {
                _renderingValues = null;
                if (classInstance != null)
                {
                    var iRendering = classInstance as IRenderingValues;
                    if (iRendering != null)
                    {
                        _renderingValues = iRendering.RenderingValues = value;
                    }
                    else
                        throw new Exception("Class instance doesn't implement IRenderingValues.");
                }
            }
        }

        public CompiledDoubleFuncParameter(string functionName, string formula = null)
        {
            tokenizer = new Tokenizer(forCSharp: true, addOperators: true);
            FunctionName = functionName;
            Function = DefaultFunction;
            if (formula != null)
                InitFormula(formula);
        }

        public CompiledDoubleFuncParameter(string functionName, string paramsClassCodeFileName, string formula = null)
               : this(functionName)
        {
            if (paramsClassCodeFileName != null)
            {
                ParamsClassCodeFilePath = GetParamsFilePath(paramsClassCodeFileName);
                if (!File.Exists(ParamsClassCodeFilePath))
                    throw new Exception($"The file {ParamsClassCodeFilePath} was not found.");
            }
            InitFormula(formula);
        }

        public CompiledDoubleFuncParameter(string functionName, Type paramsClassType, string formula = null) 
               : this(functionName)
        {
            ParamsClassType = parametersClassType = paramsClassType;
            InitFormula(formula);
        }

        private void InitFormula(string formula)
        {
            CompileFormula(formula ?? "return x;", editOnError: false);
        }

        public static string GetParamsFilePath(string fileName)
        {
            return Path.Combine(WhorlSettings.Instance.FilesFolder, WhorlSettings.IncludeFilesFolder, fileName);
        }

        private double DefaultFunction(double x)
        {
            return x;
        }

        public bool EditFormula()
        {
            string newFormula = EditFormulaHelper(formTitle: $"Edit Function {FunctionName}");
            if (newFormula == null)
                return false; //User canceled.
            return CompileFormula(newFormula);
        }

        public bool CompileFormula(string formula, bool editOnError = true)
        {
            while (true)
            {
                NewFormula = formula;
                string errorList = CompileFormulaHelper(formula);
                if (errorList == null)
                {
                    Formula = formula;
                    break;  //Successful compile.
                }
                if (!editOnError)
                    return false;
                string newFormula = EditFormulaHelper(errorList, $"Errors for function {FunctionName}");
                if (newFormula == null)
                    return false; //User canceled.
                formula = newFormula;
            }
            return true;
        }

        public string GetCSharpCode()
        {
            return GetClassCode(Formula);
        }

        private string CompileFormulaHelper(string formula)
        {
            if (formula == Formula)
                return null;
            if (string.IsNullOrWhiteSpace(formula))
                return "Please enter a nonblank formula.";
            formula = formula.Replace("@", "Parms.");
            var tokens = tokenizer.TokenizeExpression(formula);
            if (!tokens.Exists(t => t.Text == "return"))
            {
                formula = "return " + formula;
            }
            if (!tokens.Exists(t => t.Text == ";"))
            {
                formula += ";";
            }
            string code = GetClassCode(formula);
            var sharedCompiledInfo = CSharpCompiler.Instance.CompileFunctionFormula(code);
            if (sharedCompiledInfo.Errors.Count != 0)
            {
                return sharedCompiledInfo.ErrorsText;
            }
            var compiledInfo = new CSharpCompiledInfo(sharedCompiledInfo);
            var evalInstance = compiledInfo.CreateEvalInstance(forFormula: false);
            classInstance = evalInstance.ClassInstance;
            if (ParamsClassType != null || ParamsClassCodeFilePath != null)
            {
                object sourceParams = ParamsObject;
                evalInstance.GetParametersObject();
                if (evalInstance.ParamsObj == null)
                    throw new Exception("Didn't find parameters object.");
                ParamsObject = evalInstance.ParamsObj;
                parametersClassType = ParamsObject.GetType();
                if (sourceParams != null)
                {
                    FormulaSettings.CopyCSharpParameters(sourceParams, ParamsObject, parentPattern: null);
                }
            }
            //renderingValuesPropertyInfo = classInstance.GetType().GetProperty("Info");
            var methodInfo = sharedCompiledInfo.EvalClassType.GetMethod(FunctionName, BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo == null)
                throw new Exception($"Couldn't retrieve method for function {FunctionName}.");
            Function = (Func<double, double>)Delegate.CreateDelegate(typeof(Func<double, double>), classInstance, methodInfo);
            return null;
        }

        private string GetClassCode(string formula)
        {
            var docF = new DocumentFormatter();
            var sb = new StringBuilder();

            docF.AppendLine(sb, usingStatements);
            docF.AppendLine(sb, $"namespace {CSharpSharedCompiledInfo.WhorlEvalNamespace}");
            docF.OpenBrace(sb);
            docF.AppendLine(sb, $"public class {CSharpSharedCompiledInfo.WhorlEvalClassName}: IRenderingValues");
            docF.OpenBrace(sb);

            if (ParamsClassCodeFilePath != null)
            {
                string paramsClassCode = ReadParamsClassCode();
                string paramsClassName = Path.GetFileNameWithoutExtension(ParamsClassCodeFilePath);
                docF.AppendLine(sb, paramsClassCode);
                docF.AppendLine(sb, $"public {paramsClassName} Parms {{ get; }} = new {paramsClassName}();");
            }
            else if (ParamsClassType != null)
            {
                string paramsClassName = ParamsClassType.Name;
                docF.AppendLine(sb, $"public {paramsClassName} Parms {{ get; }} = new {paramsClassName}();");
            }
            docF.AppendLine(sb, "public RenderingValues RenderingValues { get; set; }");
            docF.AppendLine(sb, "public RenderingValues Info => RenderingValues;");

            docF.AppendLine(sb, $"public double {FunctionName}(double x)");
            docF.OpenBrace(sb);
            docF.AppendLine(sb, formula);
            docF.CloseBrace(sb);

            docF.CloseBrace(sb);
            docF.CloseBrace(sb);
            return sb.ToString();
        }

        public string ReadParamsClassCode()
        {
            return File.ReadAllText(ParamsClassCodeFilePath);
        }

        private void ListProperties(Type objType, StringBuilder sb)
        {
            foreach (var propInfo in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .OrderBy(pi => pi.Name))
            {
                sb.AppendLine($"{propInfo.PropertyType.GetFriendlyName()} {propInfo.Name}" + " { get; set; }");
            }
        }

        private string ListPropertiesAndMethods(Type paramsObjType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("//=== Parameter Properties:");
            ListProperties(paramsObjType, sb);
            sb.AppendLine("//=== Info Properties:");
            ListProperties(typeof(RenderingValues), sb);
            sb.AppendLine("//=== Methods:");
            foreach (var methodInfo in paramsObjType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                     .OrderBy(mi => mi.Name))
            {
                if (methodInfo.DeclaringType == typeof(object) ||
                    methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))
                {
                    continue;
                }
                sb.AppendLine($"{methodInfo.ReturnType.GetFriendlyName()} {methodInfo.Name}(" +
                              string.Join(", ", 
                              methodInfo.GetParameters().Select(p => $"{p.ParameterType.GetFriendlyName()} {p.Name}")) 
                              + ")");
            }
            return sb.ToString();
        }

        private string EditFormulaHelper(string errorList = null, string formTitle = null)
        {
            using (var frm = new frmTextEditor())
            {
                if (formTitle != null)
                    frm.Text = formTitle;
                frm.DisplayText(NewFormula, autoSize: true);
                if (parametersClassType != null)
                {
                    frm.AddRelatedText(ListPropertiesAndMethods(parametersClassType),
                                       "Available Properties and Methods");
                }
                if (errorList != null)
                {
                    frm.AddRelatedText(GetCSharpCode(), "Complete C# Code");
                    if (Formula != null)
                        frm.AddRelatedText(Formula, "Previous Formula");
                    frm.AddRelatedText(errorList, "Error List");
                }
                if (frm.ShowDialog() != DialogResult.OK)
                    return null;
                else
                    return frm.EditedText;
            }
        }
    }
}
