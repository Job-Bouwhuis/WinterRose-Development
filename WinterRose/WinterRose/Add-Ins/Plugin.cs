using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinterRose.ConsoleExtentions;
using WinterRose.Exceptions;
using WinterRose.FileManagement;

namespace WinterRose.Plugins;

/// <summary>
/// A class that loads a plugin into the current app domain.
/// </summary>
public sealed class Plugin(string pluginName, bool IsAssembly = false)
{
    /// <summary>
    /// The optimization level of the plugin.<br></br>
    /// Depends on the output of the build
    /// </summary>
    public OptimizationLevel OptimizationLevel
    {
        get
        {
            if(optimizationLevel is not null)
                return optimizationLevel.Value;
#if DEBUG
            return OptimizationLevel.Debug;
#else
            return OptimizationLevel.Release;
#endif
        }
    }
    private OptimizationLevel? optimizationLevel = null;

    /// <summary>
    /// The load context of the plugin.
    /// </summary>
    public WeakReference<AssemblyLoadContext> PluginReference { get; private set; } = new(null);
    public AssemblyLoadContext? PluginContext
    {
        get
        {
            if (PluginReference is null)
                return null;
            if (PluginReference.TryGetTarget(out var context))
                return context;
            return null;
        }
        private set
        {
            PluginReference.SetTarget(value);
        }
    }

    private readonly List<PortableExecutableReference> references = [];
    private SyntaxTree[] syntaxTrees = [];

    private List<string> GetFilePaths(string sourcePath)
    {
        List<string> paths = [];
        foreach (var i in Directory.GetFiles(sourcePath))
        {
            paths.Add(i);
        }
        foreach (var i in Directory.GetDirectories(sourcePath))
        {
            paths.AddRange(GetFilePaths(i));
        }
        return paths;
    }

    private int LoadPluginSyntaxTrees(string sorucePath)
    {
        if(File.Exists(sorucePath))
        {
            syntaxTrees = [.. new SyntaxTree[] { SyntaxFactory.ParseSyntaxTree(FileManager.Read(sorucePath)) }];
            return 1;
        }
        var paths = GetFilePaths(sorucePath);
        try
        {
            if (paths.Count == 0) return 0;

            syntaxTrees = new SyntaxTree[paths.Count];
            int count = 0;
            foreach (var path in paths)
            {
                string script = FileManager.Read(path);
                syntaxTrees[count] = SyntaxFactory.ParseSyntaxTree(script.Trim());
                count++;
            }
            return 1;
        }
        catch
        {
            return -1;
        }
    }
    /// <summary>
    /// Loads the plugin from the given path.
    /// <br></br> If instructed to load an assembly, <paramref name="sourcePath"/> should be the path to the assembly.
    /// <br></br> If instructed to load a script, <paramref name="sourcePath"/> should be the path to the folder containing the script. all files in the folder will be loaded.
    /// <br></br> on first time loading of any plugin, it will take about 2-3 seconds to load. after that, it will take about 0.2=1 second to load.
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <returns>The loaded plugin as an assembly instance. or null if the plugin was not loaded correctly but did not have any diagnostic errors</returns>
    public Assembly? LoadPlugin(FilePath sourcePath)
    {
        PluginContext?.Unload();
        PluginContext = new(pluginName, true);

        InitializeLoading();

        if (!IsAssembly)
            LoadPluginSyntaxTrees(sourcePath);
        else
            return CompileAssembly(sourcePath);

        if(!ValidateSyntaxTrees())
        {
            Console.WriteLine("No plugin loaded.");
            return null;
        }
        return Compile();
    }
    /// <summary>
    /// Loads the plugin from the given syntax trees.
    /// </summary>
    /// <param name="syntaxTrees"></param>
    /// <returns></returns>
    public Assembly? LoadPlugin(List<SyntaxTree> syntaxTrees)
    {
        PluginContext?.Unload();

        InitializeLoading();

        this.syntaxTrees = [.. syntaxTrees];

        if (!ValidateSyntaxTrees())
        {
            Console.WriteLine("No plugin loaded.");
            return null;
        }
        return Compile();
    }

    private bool ValidateSyntaxTrees()
    {
        List<SyntaxTree> validatedTrees = [];
        syntaxTrees.Foreach(x =>
        {
            if (x is not null && x.Length is not 0)
                validatedTrees.Add(x);
        });
        syntaxTrees = [.. validatedTrees];

        if (syntaxTrees.Length == 0)
            return false;
        return true;
    }

    private Assembly Compile()
    {
        CSharpCompilation compilation = CSharpCompilation.Create(pluginName)
            .WithOptions(new CSharpCompilationOptions(allowUnsafe: true,
                outputKind: OutputKind.DynamicallyLinkedLibrary, 
                optimizationLevel: OptimizationLevel))
            .AddSyntaxTrees(syntaxTrees)
            .WithReferences(references);

        Assembly? result;
        Stream codeStream = null;
        try
        {
            using (codeStream = new MemoryStream())
            {
                EmitResult compilationResult = compilation.Emit(codeStream);
                foreach (var diag in compilationResult.Diagnostics)
                {
                    if (diag.Severity == DiagnosticSeverity.Error)
                    {
                        var e = new PluginCompilationErrorException("Plugin compilation error", [.. compilationResult.Diagnostics]);
                        e.SetSource(string.IsNullOrWhiteSpace(diag.Location.SourceTree?.FilePath ?? "") ? $"{pluginName}/Unknown" : diag.Location.SourceTree?.FilePath ?? "shouldnt be reached");
                        e.SetStackTrace(diag.GetMessage());
                        throw e;
                    }
                    if (diag.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Hidden)
                    {
                        var loc = string.IsNullOrWhiteSpace(diag.Location.ToString() ?? "") ? $"{pluginName}/Unknown" : diag.Location.SourceTree?.FilePath ?? "shouldnt be reached";
                        var msg = diag.GetMessage();

                        ConsoleS.WriteWarningLine($"Script Warning: {loc}.\tMessage: {msg}");
                    }
                }

                codeStream.Seek(0, SeekOrigin.Begin);
                result = PluginContext.LoadFromStream(codeStream);
            }
            return result;
        }
        finally
        {
            codeStream?.Dispose();
        }
    }

    private void InitializeLoading()
    {
        AddDefaultReferences();
    }

    private Assembly? CompileAssembly(string sourcePath)
    {
        PluginContext.LoadFromAssemblyPath(sourcePath);
        return PluginContext.Assemblies.FirstOrDefault(x => x.FullName.Contains(pluginName));
    }

    /// <summary>
    /// Adds the assembly at the given path to the list of assemblies to be referenced when compiling the plugin.
    /// </summary>
    /// <param name="assemblyDll"></param>
    /// <returns></returns>
    public bool AddAssembly(string assemblyDll)
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

        if (references.Any(r => r.FilePath == file)) return true;

        try
        {
            var reference = MetadataReference.CreateFromFile(file);
            references.Add(reference);
        }
        catch
        {
            return false;
        }

        return true;
    }
    /// <summary>
    /// Adds the given list of assembly paths to the list of assemblies to be referenced when compiling the plugin.
    /// </summary>
    /// <param name="assemblies"></param>
    public void AddAssemblies(params string[] assemblies) => assemblies.Foreach(x => AddAssembly(x));
    private void AddDefaultReferences()
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
        Assembly collections = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("System.Collections")).FirstOrDefault();
        Assembly systemRuntime = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("System.Runtime")).FirstOrDefault();
        AddAssembly(systemRuntime);
        AddAssembly(collections);
        AddAssembly(typeof(Plugin));

    }
    /// <summary>
    /// Adds the assembly in which the given type is defined to the list of assemblies to be referenced when compiling the plugin.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool AddAssembly(Type type)
    {
        try
        {
            if (references.Any(r => r.FilePath == type.Assembly.Location))
                return true;

            var systemReference = MetadataReference.CreateFromFile(type.Assembly.Location);
            references.Add(systemReference);
        }
        catch
        {
            return false;
        }

        return true;
    }
    /// <summary>
    /// Adds the given assembly to the list of assemblies to be referenced when compiling the plugin.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public bool AddAssembly(Assembly assembly)
    {
        try
        {
            if (references.Any(x => x.FilePath == assembly.Location))
                return true;

            var systemReference = MetadataReference.CreateFromFile(assembly.Location);
            references.Add(systemReference);
        }
        catch { return false; }

        return true;
    }

    /// <summary>
    /// Unloads the plugin from the current app domain.<br></br>
    /// Make sure that there are no references to the plugin before calling this method.
    /// </summary>
    public void Unload()
    {
        PluginContext.Unload();
        PluginContext.Unloading += x => Console.WriteLine("Unloading plugin: " + pluginName);
    }

    /// <summary>
    /// Adds the given assemblies
    /// </summary>
    /// <param name="assemblies"></param>
    public void AddAssembly(Assembly[] assemblies)
    {
        foreach(var a in assemblies)
        {
            AddAssembly(a);
        }
    }
}


/// <summary>
/// Thrown when something went wrong when compiling the plugin.
/// </summary>
[Serializable]
public class PluginCompilationErrorException : WinterException
{
    List<Diagnostic> Diagnostics { get; }
    /// <summary>
    /// Contains all the diagnostics that were generated when compiling the plugin. each on a seperate line.
    /// </summary>
    public string DiagnosticsString
    {
        get
        {
            StringBuilder sb = new();
            foreach (var d in Diagnostics)
            {
                sb.AppendLine(d.ToString());
                string text = d.Location.SourceTree.GetText().ToString();
                int classkeyword = text.IndexOf("class");
                int endofline = text.IndexOf("\r\n", classkeyword);
                string classname = text.Substring(classkeyword, endofline - classkeyword);
                sb.AppendLine($"In {classname}");
                sb.AppendLine($"Loc: {d.Location.GetLineSpan()}");
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
    public PluginCompilationErrorException() { }
    public PluginCompilationErrorException(string message, List<Diagnostic> diagnostics) : base(message) => Diagnostics = diagnostics; 
    public PluginCompilationErrorException(string message, Exception inner) : base(message, inner) { }
}