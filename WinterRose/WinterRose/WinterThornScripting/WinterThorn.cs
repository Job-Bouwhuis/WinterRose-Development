using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WinterRose.FileManagement;
using WinterRose.WinterThornScripting.DefaultLibrary;
using WinterRose.WinterThornScripting.Factory;
using WinterRose.WinterThornScripting.Generation;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting;

/// <summary>
/// A script that can execute WinterThorn code.
/// <br></br> This is a collection of namespaces that do the execution. through this class you define the entire scope of the script by defining namespaces.
/// </summary>
public class WinterThorn
{
    /// <summary>
    /// The name of the script
    /// </summary>
    [WFInclude]
    public string Name { get; set; }
    /// <summary>
    /// A description of the script
    /// </summary>
    [WFInclude]
    public string Description { get; set; }
    /// <summary>
    /// The author of the script
    /// </summary>
    [WFInclude]
    public string Author { get; set; }
    /// <summary>
    /// The version of the script
    /// </summary>
    [WFInclude]
    public Version Version { get; set; }
    /// <summary>
    /// The namespaces defined in the script
    /// </summary>
    public Namespace[] Namespaces => GlobalBlock.Namespaces!;
    /// <summary>
    /// The global block of the script. here everything is defined.
    /// </summary>
    [WFInclude]
    public Block GlobalBlock { get; set; } = new Block(null);

    /// <summary>
    /// Creates a new script with the specified name, description, author, version and namespaces.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="version"></param>
    /// <param name="namespaces"></param>
    public WinterThorn(string name, string description, string author, Version version, Namespace[] namespaces)
        : this(name, description, author, version)
    {
        namespaces.Foreach(GlobalBlock.DefineNamespace);
    }
    /// <summary>
    /// Creates a new script with the specified name, description, author, version and code.
    /// </summary>
    /// <param name="code">Gets parsed to a namespace.</param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="version"></param>
    public WinterThorn(string code, string name, string description, string author, Version version)
        : this(name, description, author, version)
    {
        var ns = ThornFactory.ParseScript(code, GlobalBlock);
        ns?.Foreach(GlobalBlock.DefineNamespace);
    }
    /// <summary>
    /// Creates a new script with the given name, description, author, version and code.
    /// </summary>
    /// <param name="scriptsSource">The directory where to find all the source files. all files ending in ".thn" 
    /// will be included in the parsing and will be added to their respective namespace within the scripts scope</param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="version"></param>
    public WinterThorn(System.IO.DirectoryInfo scriptsSource, string name, string description, string author, Version version)
        : this(name, description, author, version)
    {
        var files = scriptsSource.GetFiles("*.thn");
        foreach (var file in files)
        {
            var ns = ThornFactory.ParseScript(File.ReadAllText(file.FullName), GlobalBlock);
            ns?.Foreach(GlobalBlock.DefineNamespace);
        }
    }
    /// <summary>
    /// Creates a new script with the given name, description, author, version. Defines the default library.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="version"></param>
    public WinterThorn(string name, string description, string author, Version version)
    {
        Name = name;
        Description = description;
        Author = author;
        Version = version;

        ConstructDefaultLibrary();
    }

    private WinterThorn(string name, string description, string author, Version version, bool withDefaultLibrary)
    {
        Name = name;
        Description = description;
        Author = author;
        Version = version;

        if (withDefaultLibrary)
            ConstructDefaultLibrary();
    }
    private WinterThorn() { } // exists for serialization

    /// <summary>
    /// Adds the specified namespace to the script.
    /// </summary>
    /// <param name="namespace"></param>
    /// <returns>This script instance</returns>
    public WinterThorn DefineNamespace(Namespace @namespace)
    {
        GlobalBlock.DefineNamespace(@namespace);
        return this;
    }
    /// <summary>
    /// Gets the class with the specified name.
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public Class? GetClass(string typeName)
    {
        foreach (var ns in Namespaces)
        {
            var type = ns.GetClass(typeName);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }
    /// <summary>
    /// Gets the class with the specified name in the specified namespace.
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public Class? GetClass(string namespaceName, string typeName)
    {
        foreach (var ns in Namespaces)
        {
            if (ns.Name == namespaceName)
            {
                var type = ns.GetClass(typeName);
                if (type != null)
                {
                    return type;
                }
            }
        }

        return null;
    }
    /// <summary>
    /// Gets an instance of the class, if it is found, uses <paramref name="args"/> as the arguments for the constructor
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="namespaceName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public Class? GetInstancedClass(string namespaceName, string typeName, params Variable[] args)
    {
        Class? c = GetClass(namespaceName, typeName);
        return c?.CreateInstance(args);
    }

    /// <summary>
    /// Generates a set of .md files with the names of classes, their variables, and their functions, with possible provided description
    /// </summary>
    /// <param name="destination"></param>
    public void GenerateDocumentation(System.IO.DirectoryInfo destination)
    {

        if (destination.Exists)
            destination.Delete(true);
        destination.Create();

        foreach (var ns in Namespaces)
        {
            System.IO.DirectoryInfo nsDir = destination.CreateSubdirectory(ns.Name);
            if (!nsDir.Exists)
                nsDir.Create();

            foreach (Class c in ns.Classes)
            {
                System.IO.FileInfo classFile = new(System.IO.Path.Combine(nsDir.FullName, c.Name + ".md"));
                if (!classFile.Exists)
                    classFile.Create().Close();
                StringBuilder content = new StringBuilder();

                content.AppendLine("# Class: " + c.Name);
                if (c.Description is "")
                    content.AppendLine("No description provided.");
                else
                    content.AppendLine(c.Description);

                content.AppendLine();
                content.AppendLine();

                foreach (Variable var in c.Block.Variables)
                {
                    Variable(content, var);
                }

                content.AppendLine();
                content.AppendLine();

                foreach (Constructor constructor in c.Constructors)
                {
                    Constructor(content, constructor);
                }
                content.AppendLine();
                content.AppendLine();

                foreach (Function func in c.Block.Functions)
                {
                    Function(content, func);
                }

                FileManager.Write(classFile.FullName, content.ToString(), true);
            }
        }

        {
            System.IO.FileInfo globalFile = new(System.IO.Path.Combine(destination.FullName, "Global.md"));
            if (!globalFile.Exists)
                globalFile.Create().Close();

            StringBuilder content = new StringBuilder();
            content.AppendLine("# The global section describes functions and variables available wherever in the code");
            content.AppendLine("- These can be overriden by defining a function or variable with the same name within the context. However it is adviced not to override any function or variable starting with a double underline \"__\"");

            foreach (Variable var in GlobalBlock.Variables)
            {
                Variable(content, var);
            }

            foreach (Function var in GlobalBlock.Functions)
            {
                Function(content, var);
            }

            FileManager.Write(globalFile.FullName, content.ToString(), true);
        }

        static void Variable(StringBuilder content, Variable var)
        {
            content.AppendLine("## Variable: " + var.Name);
            if (var.Description is "")
                content.AppendLine("No description provided.");
            else
                content.AppendLine(var.Description);
            if (var.Type == VariableType.CSharpDelegate)
                if (var.Setter == null)
                    content.AppendLine("This variable gets it value from C# code, and can not be assigned a value");
                else
                    content.AppendLine("This variable gets and sets it value to and from C# code.");

            content.AppendLine();
        }

        static void Constructor(StringBuilder content, Constructor constructor)
        {
            content.AppendLine("## Constructor: " + constructor.Name);
            if (constructor.Description is "")
                content.AppendLine("No description provided.");
            else
                content.AppendLine(constructor.Description);
            content.AppendLine();
            if (constructor.IsCSharpFunction)
            {
                content.AppendLine("This function its code is implemented in C#");
                ParameterInfo[] parameters = constructor.CSharpFunction.GetMethodInfo().GetParameters();
                content.AppendLine("**Parameters:**");
                foreach (ParameterInfo param in parameters)
                    content.AppendLine($" - C# > {param.Name}: {param.ParameterType}");
            }
            else
            {
                if (constructor.Parameters != null && constructor.Parameters.Length > 0)
                {
                    content.AppendLine("**Parameters:**");
                    foreach (Parameter param in constructor.Parameters)
                    {
                        content.AppendLine($" - {param.Name}: {param.Class}");
                        if (param.Description is "")
                            content.AppendLine("   - No description provided.");
                        else
                            content.AppendLine($"   - {param.Description}");
                    }
                }
            }
            content.AppendLine();
        }

        static void Function(StringBuilder content, Function func)
        {
            content.AppendLine("## Function: " + func.Name);
            if (func.Description is "")
                content.AppendLine("No description provided.");
            else
                content.AppendLine(func.Description);
            content.AppendLine();
            if (func.IsCSharpFunction)
            {
                content.AppendLine("This function its code is implemented in C#");
                ParameterInfo[] parameters = func.CSharpFunction.GetMethodInfo().GetParameters();
                content.AppendLine("**Parameters:**");
                foreach (ParameterInfo param in parameters)
                    content.AppendLine($" - C# > {param.Name}: {param.ParameterType}");
            }
            else
            {
                if (func.Parameters != null && func.Parameters.Length > 0)
                {
                    content.AppendLine("**Parameters:**");
                    foreach (Parameter param in func.Parameters)
                    {
                        content.AppendLine($" - {param.Name}: {param.Class}");
                        if (param.Description is "")
                            content.AppendLine("   - No description provided.");
                        else
                            content.AppendLine($"   - {param.Description}");
                    }
                }
            }

            if (func.ReturnsValue)
                content.AppendLine("This function returns a value.");
            else
                content.AppendLine("This function does not return a value.");

            content.AppendLine();
        }
    }
    private void ConstructDefaultLibrary()
    {
        Collection col = new();
        Class colc = col.GetClass();
        Console console = new();
        Class consolec = console.GetClass();
        Math math = new();
        Class mathc = math.GetClass();
        Random rnd = new();
        Class randomc = rnd.GetClass();
        DefaultLibrary.FileSystem.File file = new();
        Class filec = file.GetClass();
        DefaultLibrary.FileSystem.Directory dir = new();
        Class dirc = dir.GetClass();

        Namespace Thorn = new("Thorn - The default library for WinterThorn", [colc, consolec, mathc, randomc, filec, dirc]);
        GlobalBlock.DefineNamespace(Thorn);

        CreateGlobalAssets();
    }

    private void CreateGlobalAssets()
    {
        Function numberCollectionFunc = new Function("__Collection", "Creates a collection of the specified integer, if a collection itself is passed, that same collection is returned", AccessControl.Public);
        numberCollectionFunc.CSharpFunction = (Variable numVar, Variable stepsVar) =>
        {
            if (numVar.Type == VariableType.Class)
            {
                Class c = (Class)numVar.Value;
                var validFunction = c.Block.Functions.FirstOrDefault(func => func.Name == "Get");
                if (validFunction == null || !validFunction.ReturnsValue)
                {
                    throw new WinterThornExecutionError(ThornError.InvalidParameters, "WR-365", "Parameter num is a class that does not implement the right GET function. should be 'Get number index' and return a value");
                }
                return numVar;
            }
            else if (numVar.Type != VariableType.Number)
                throw new WinterThornExecutionError(ThornError.InvalidType, "WR-365", "Parameter num must be a number or class that implements 'function Get number index'");
            if (stepsVar.Type != VariableType.Number)
                throw new WinterThornExecutionError(ThornError.InvalidType, "WR-365", "Parameter steps must be a number");

            int num = int.Parse(numVar.Value.ToString());
            int steps = int.Parse(stepsVar.Value.ToString());

            List<double> nums = [];
            for (double d = 0; d < num; d += steps)
            {
                nums.Add(d);
            }

            Class cls = new Collection().GetClass();
            ((Collection)cls.CSharpClass).AddMany(nums.Select(x => new Variable("collectionvar", "", x, AccessControl.Public)).ToArray());
            return new Variable("Number Collection result", "", cls, AccessControl.Public);
        };

        Function asString = new Function("ToString", "Transforms whatever argument is given into a string. Calls a \"ToString\" function in a class if defined.", AccessControl.Public)
        {
            CSharpFunction = (Variable var) =>
            {
                if (var.Type is VariableType.Number or VariableType.String or VariableType.Boolean)
                    return var.Value?.ToString()!;
                if (var.Type == VariableType.Null)
                    return null;

                if (var.Type == VariableType.Class)
                {
                    Class c = (Class)var.Value!;
                    Function? toStringFunc = c.Block.Functions.FirstOrDefault(x => x.Name == "ToString" && x.Parameters == null && x.ReturnsValue);
                    if (toStringFunc is null)
                    {
                        return $"Class: {c.Namespace}.{c.Name}";
                    }
                    return toStringFunc.Invoke();
                }
                return var.Value?.ToString() ?? "null";
            }
        };

        Function booleanInverse = new Function("__BooleanInverse", "Inverses the boolean value passed. true > false | false > true (used internally, but available outside)", AccessControl.Public);
        booleanInverse.SetParameters(new Parameter("bool", "The boolean value that gets inverted", "Boolean"));
        Block body = new Block(
            """
            r = true;
            if bool
            {
                r = false;
            }
            return r;
            """, GlobalBlock);
        booleanInverse.SetBody(body);

        Variable workingDir = new Variable("__workingDirectory", "Gets or sets the working directory of the application",
            () => new DefaultLibrary.FileSystem.Directory().GetClass().CreateInstance([new Variable("workingDir", "", System.IO.Directory.GetCurrentDirectory(), AccessControl.Public)]), AccessControl.Public)
        {
            Setter = (string path) => System.IO.Directory.SetCurrentDirectory(path)
        };

        Variable exeFile = new Variable("__executableFile", "Gets a file class instance for the exe that started the application",
            () => new DefaultLibrary.FileSystem.File().GetClass().CreateInstance([new Variable("exePath", "", System.Environment.ProcessPath, AccessControl.Public)]));

        GlobalBlock.DeclareVariable(workingDir);
        GlobalBlock.DeclareVariable(exeFile);

        GlobalBlock.DeclareFunction(numberCollectionFunc);
        GlobalBlock.DeclareFunction(booleanInverse);
        GlobalBlock.DeclareFunction(asString);
    }
}
