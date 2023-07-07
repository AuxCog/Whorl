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
            private const string compilerDirectory = @"c:\";
            //private const string compilerDirectory = @"c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin";
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

        private string outputAssembliesFolder { get; }

        private Dictionary<string, CompilerResults> compiledDict { get; } =
            new Dictionary<string, CompilerResults>();

        private Dictionary<string, CSharpSharedCompiledInfo> compiledSharedDict { get; } =
            new Dictionary<string, CSharpSharedCompiledInfo>();

        private CSharpCompiler()
        {
            const string folderName = "CustomParameterClasses";

            csProvider = new CSharpCodeProvider(new CompilerSettings());
            compilerParameters = GetCompilerParameters(inMemory: true);
            outputAssembliesFolder = Path.Combine(WhorlSettings.Instance.FilesFolder, folderName);
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        private CompilerParameters GetCompilerParameters(bool inMemory)
        {
            const string options = "-langversion:8.0";

            var compilerParams = new CompilerParameters()
            { GenerateExecutable = false, GenerateInMemory = inMemory, CompilerOptions = options };
            compilerParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            compilerParams.ReferencedAssemblies.Add(typeof(System.Windows.Forms.Control).Assembly.Location);
            compilerParams.ReferencedAssemblies.Add(typeof(System.Drawing.Size).Assembly.Location);
            compilerParams.ReferencedAssemblies.Add(typeof(PixelRenderInfo).Assembly.Location);
            compilerParams.ReferencedAssemblies.Add(typeof(ParserEngine.OutlineMethods).Assembly.Location);
            return compilerParams;
        }

        public void AddReferencedAssembly(Assembly assembly)
        {
            compilerParameters.ReferencedAssemblies.Add(assembly.Location);
        }

        public CompilerResults CompileCustomParameterClasses(out string code, out Assembly assembly)
        {
            const string folderName = "CustomParameterClasses";
            string folder = outputAssembliesFolder;
            code = null;
            assembly = null;
            if (Directory.Exists(folder))
            {
                string filePath = Path.Combine(folder, $"{folderName}.cs");
                if (File.Exists(filePath))
                {
                    string assemblyPath = Path.Combine(folder, $"{folderName}.dll");
                    if (File.Exists(assemblyPath))
                    {
                        if (File.GetLastWriteTime(filePath) <= File.GetLastWriteTime(assemblyPath))
                        {
                            assembly = Assembly.LoadFrom(assemblyPath);
                            return null;
                        }
                    }
                    code = File.ReadAllText(filePath);
                    var compileParams = GetCompilerParameters(inMemory: false);
                    compileParams.OutputAssembly = assemblyPath;
                    var results = csProvider.CompileAssemblyFromSource(compileParams, code);
                    if (results.Errors.Count == 0)
                        assembly = results.CompiledAssembly;
                    return results;
                }
            }
            return null;
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

        public CSharpSharedCompiledInfo CompileFunctionFormula(string code)
        {
            CompilerResults results = CompileCode(code);
            var sharedCompiledInfo = new CSharpSharedCompiledInfo();
            sharedCompiledInfo.SetCompilerResults(results, forFormula: false);
            return sharedCompiledInfo;
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

        // Handle dependencies and assemblies we're not able to load
        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                return null;
            }
            var assemblyName = new AssemblyName(args.Name);
            string path = Path.Combine(outputAssembliesFolder, assemblyName.Name + ".dll");
            if (File.Exists(path))
            {
                try
                {
                    return Assembly.LoadFrom(path);
                }
                catch
                {
                }
            }
            return null;
        }
    }
}
