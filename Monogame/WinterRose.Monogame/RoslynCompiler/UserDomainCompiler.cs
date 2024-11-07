using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Exceptions;
using WinterRose.FileManagement;

namespace WinterRose.Monogame.RoslynCompiler;

internal static class UserDomainCompiler
{
    public static string ScriptSource { get; set; } = MonoUtils.Content.RootDirectory + "/Scripts";
    public static string AssemblySource { get; set; } = MonoUtils.Content.RootDirectory + "/Mods";
    public static bool HasScriptsInSource
    {
        get
        {
            DirectoryInfo info = new DirectoryInfo(ScriptSource);
            if (!info.Exists) return false;
            return Directory.GetFiles(ScriptSource).Length > 0;
        }
    }
    private static OptimizationLevel UserDomainOptimizationLevel
    {
        get
        {
#if DEBUG
            return OptimizationLevel.Debug;
#else
            return OptimizationLevel.Release;
#endif
        }
    }
    public static AssemblyLoadContext UserDomainContext { get; private set; } = new("UserDomain", true);

    private static List<PortableExecutableReference> References = new();
    private static SyntaxTree[] syntaxTrees = Array.Empty<SyntaxTree>();

    public static int LoadUserDomain(Action<string>? progressReport = null)
    {
        try
        {
            if (!Directory.Exists(ScriptSource))
            {
                progressReport?.Invoke("No User scripts.");
                return 0;
            }
            var files = Directory.GetFiles(ScriptSource);
            syntaxTrees = new SyntaxTree[files.Length];
            foreach (var i in files.Length)
            {
                // F:\WinterData\Projects\Visual Studio\Class Library\WinterRoseLibrary\WinterRose\WinterRose.Monogame.Tests\Content\Scripts\PlayerMovement.cs
                progressReport?.Invoke(FileManager.PathFrom(files[i], "Scripts"));
                string script = FileManager.Read(files[i]);
                syntaxTrees[i] = SyntaxFactory.ParseSyntaxTree(script.Trim());
            }
            return 1;
        }
        catch
        {
            return -1;
        }
    }
    public static Assembly? CompileUserDomain(Action<Diagnostic>? DiagnosticsReport = null)
    {
        UserDomainContext.Unload();
        var aa = AppDomain.CurrentDomain.GetAssemblies();
        UserDomainContext = new("UserDomain", true);
        aa = AppDomain.CurrentDomain.GetAssemblies();

        List<SyntaxTree> validatedTrees = new();
        syntaxTrees.Foreach(x =>
        {
            if (x is not null && x.Length is not 0)
                validatedTrees.Add(x);
        });
        syntaxTrees = validatedTrees.ToArray();

        if (syntaxTrees.Length == 0)
        {
            Console.WriteLine("No User Domain loaded");
            return null;
        }

        CSharpCompilation compilation = CSharpCompilation.Create("UserDomain.dll")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: UserDomainOptimizationLevel))
            .AddSyntaxTrees(syntaxTrees)
            .WithReferences(References);

        Assembly result;
        Stream codeStream = null;
        try
        {
            using (codeStream = new MemoryStream())
            {
                EmitResult compilationResult = compilation.Emit(codeStream);

                ;
                foreach (var diag in compilationResult.Diagnostics)
                {
                    DiagnosticsReport?.Invoke(diag);
                    if (diag.Severity == DiagnosticSeverity.Error)
                    {
                        var e = new UserDomainCompilationErrorException("User script compilation error");
                        e.SetSource(string.IsNullOrWhiteSpace(diag.Location.SourceTree?.FilePath ?? "") ? "UserDoman/Unknown" : diag.Location.SourceTree?.FilePath ?? "shouldnt be reached");
                        e.SetStackTrace(diag.GetMessage());
                        Debug.LogException(e);
                    }
                    if(diag.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Hidden)
                    {
                        var loc = string.IsNullOrWhiteSpace(diag.Location.SourceTree?.FilePath ?? "") ? "UserDoman/Unknown" : diag.Location.SourceTree?.FilePath ?? "shouldnt be reached";
                        var msg = diag.GetMessage();

                        Debug.LogWarning($"Script Warning: {loc}. Message: {msg}", true);
                    }

                    //return null;
                }

                codeStream.Seek(0, SeekOrigin.Begin);
                UserDomainContext.LoadFromStream(codeStream);
                result = UserDomainContext.Assemblies.FirstOrDefault(x => x.FullName.Contains("UserDomain"));

                var allassemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            return result;
        }
        finally
        {
            codeStream?.Dispose();
        }
    }
    public static bool AddAssembly(string assemblyDll)
    {
        if (string.IsNullOrEmpty(assemblyDll)) return false;

        var file = Path.GetFullPath(assemblyDll);

        if (!File.Exists(file))
        {
            // check framework or dedicated runtime app folder
            var path = Path.GetDirectoryName(typeof(object).Assembly.Location);
            file = Path.Combine(path, assemblyDll);
            if (!File.Exists(file))
                return false;
        }

        if (References.Any(r => r.FilePath == file)) return true;

        try
        {
            var reference = MetadataReference.CreateFromFile(file);
            References.Add(reference);
        }
        catch
        {
            return false;
        }

        return true;
    }
    public static void AddAssemblies(params string[] assemblies) => assemblies.Foreach(x => AddAssembly(x));
    public static void AddNetCoreDefaultReferences()
    {
        var rtPath = Path.GetDirectoryName(typeof(object).Assembly.Location) +
                     Path.DirectorySeparatorChar;

        AddAssemblies(
            rtPath + "System.Private.CoreLib.dll",
            rtPath + "System.Runtime.dll",
            rtPath + "System.Console.dll",
            rtPath + "netstandard.dll",

            rtPath + "System.Text.RegularExpressions.dll", // IMPORTANT!
            rtPath + "System.Linq.dll",
            rtPath + "System.Linq.Expressions.dll", // IMPORTANT!

            rtPath + "System.IO.dll",
            rtPath + "System.Net.Primitives.dll",
            rtPath + "System.Net.Http.dll",
            rtPath + "System.Private.Uri.dll",
            rtPath + "System.Reflection.dll",
            rtPath + "System.ComponentModel.Primitives.dll",
            rtPath + "System.Globalization.dll",
            rtPath + "System.Collections.Concurrent.dll",
            rtPath + "System.Collections.NonGeneric.dll",
            rtPath + "System.Collections.Generic.dll",
            rtPath + "Microsoft.CSharp.dll"
        );

        AddAssembly(typeof(System.Diagnostics.Process));
        AddAssembly(typeof(System.Collections.Generic.List<>));

    }
    public static bool AddAssembly(Type type)
    {
        try
        {
            if (References.Any(r => r.FilePath == type.Assembly.Location))
                return true;

            var systemReference = MetadataReference.CreateFromFile(type.Assembly.Location);
            References.Add(systemReference);
        }
        catch
        {
            return false;
        }

        return true;
    }
    public static bool AddAssembly(Assembly assembly)
    {
        try
        {
            if (References.Any(x => x.FilePath == assembly.Location))
                return true;

            var systemReference = MetadataReference.CreateFromFile(assembly.Location);
            References.Add(systemReference);
        }
        catch { return false; }

        return true;
    }
}


[Serializable]
public class UserDomainCompilationErrorException : WinterException
{
    public UserDomainCompilationErrorException() { }
    public UserDomainCompilationErrorException(string message) : base(message) { }
    public UserDomainCompilationErrorException(string message, Exception inner) : base(message, inner) { }
}


[Serializable]
public class UserDomainCompilerErrorException : WinterException
{
    public UserDomainCompilerErrorException() { }
    public UserDomainCompilerErrorException(string message) : base(message) { }
    public UserDomainCompilerErrorException(string message, Exception inner) : base(message, inner) { }
}