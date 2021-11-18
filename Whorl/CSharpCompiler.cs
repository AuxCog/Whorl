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

        private Dictionary<string, CSharpSharedCompiledInfo> compiledSharedDict { get; } =
            new Dictionary<string, CSharpSharedCompiledInfo>();

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

        public CSharpSharedCompiledInfo CompileFormula(string code)
        {
            if (!compiledSharedDict.TryGetValue(code, out CSharpSharedCompiledInfo sharedCompiledInfo))
            {
                CompilerResults results = csProvider.CompileAssemblyFromSource(compilerParameters, code);
                sharedCompiledInfo = new CSharpSharedCompiledInfo();
                sharedCompiledInfo.SetCompilerResults(results);
                compiledSharedDict.Add(code, sharedCompiledInfo);
            }
            return sharedCompiledInfo;
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
