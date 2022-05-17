using ParserEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public class CompiledDoubleFuncParameter: BaseCSharpParameter
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
        public string ParamsClassName { get; }
        public string FunctionName { get; }
        private Tokenizer tokenizer { get; }

        public CompiledDoubleFuncParameter(string functionName, string paramsClassCodeFilePath = null)
        {
            tokenizer = new Tokenizer(forCSharp: true, addOperators: true);
            FunctionName = functionName;
            if (paramsClassCodeFilePath != null)
            {
                ParamsClassCodeFilePath = paramsClassCodeFilePath;
                if (!File.Exists(paramsClassCodeFilePath))
                    throw new Exception($"The file {paramsClassCodeFilePath} was not found.");
                ParamsClassName = Path.GetFileNameWithoutExtension(paramsClassCodeFilePath);
            }
            Function = DefaultFunction;
        }

        public static string GetParamFilePath(string paramsClassCodeFileName)
        {
            return Path.Combine(WhorlSettings.Instance.FilesFolder,
                                WhorlSettings.IncludeFilesFolder, paramsClassCodeFileName);
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
            return GetClassCode(Formula, isStatic: ParamsClassCodeFilePath == null);
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
            bool isStatic = ParamsClassCodeFilePath == null;
            string code = GetClassCode(formula, isStatic);
            var sharedCompiledInfo = CSharpCompiler.Instance.CompileFunctionFormula(code);
            if (sharedCompiledInfo.Errors.Count != 0)
            {
                return sharedCompiledInfo.ErrorsText;
            }
            object classInstance = null;
            if (!isStatic)
            {
                var compiledInfo = new CSharpCompiledInfo(sharedCompiledInfo);
                var evalInstance = compiledInfo.CreateEvalInstance(forFormula: false);
                classInstance = evalInstance.ClassInstance;
                object sourceParams = ParamsObject;
                evalInstance.GetParametersObject();
                if (evalInstance.ParamsObj == null)
                    throw new Exception("Didn't find parameters object.");
                ParamsObject = evalInstance.ParamsObj;
                if (sourceParams != null)
                {
                    FormulaSettings.CopyCSharpParameters(sourceParams, ParamsObject, parentPattern: null);
                }
            }
            var bindingFlags = BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            var methodInfo = sharedCompiledInfo.EvalClassType.GetMethod(FunctionName, bindingFlags);
            if (methodInfo == null)
                throw new Exception($"Couldn't retrieve method for function {FunctionName}.");
            Function = (Func<double, double>)Delegate.CreateDelegate(typeof(Func<double, double>), classInstance, methodInfo);
            return null;
        }

        private string GetClassCode(string formula, bool isStatic)
        {
            var docF = new DocumentFormatter();
            var sb = new StringBuilder();
            string staticText = isStatic ? "static " : string.Empty;

            docF.AppendLine(sb, usingStatements);
            docF.AppendLine(sb, $"namespace {CSharpSharedCompiledInfo.WhorlEvalNamespace}");
            docF.OpenBrace(sb);
            docF.AppendLine(sb, $"public {staticText}class {CSharpSharedCompiledInfo.WhorlEvalClassName}");
            docF.OpenBrace(sb);

            if (!isStatic && ParamsClassCodeFilePath != null)
            {
                string paramsClassCode = GetParamsClassCode();
                docF.AppendLine(sb, paramsClassCode);
                docF.AppendLine(sb, $"public {ParamsClassName} Parms {{ get; }} = new {ParamsClassName}();");
            }

            docF.AppendLine(sb, $"public {staticText}double {FunctionName}(double x)");
            docF.OpenBrace(sb);
            docF.AppendLine(sb, formula);
            docF.CloseBrace(sb);

            docF.CloseBrace(sb);
            docF.CloseBrace(sb);
            return sb.ToString();
        }

        public string GetParamsClassCode()
        {
            return File.ReadAllText(ParamsClassCodeFilePath);
        }

        private string EditFormulaHelper(string errorList = null, string formTitle = null)
        {
            using (var frm = new frmTextEditor())
            {
                if (formTitle != null)
                    frm.Text = formTitle;
                frm.DisplayText(NewFormula, autoSize: true);
                if (ParamsClassCodeFilePath != null)
                    frm.AddRelatedText(GetParamsClassCode(), "Parameters Class C# Code");
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
