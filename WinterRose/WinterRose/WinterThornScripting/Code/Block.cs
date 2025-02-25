using System.Collections.Generic;
using System.Linq;
using WinterRose.Serialization;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting;

/// <summary>
/// A block of code in ThornScript.
/// </summary>
public class Block
{
    /// <summary>
    /// The tokens that make up this block
    /// </summary>
    [IncludeWithSerialization]
    public List<Token> Tokens { get; internal set; } = [];
    /// <summary>
    /// The parent block of this block. This will be null if the block is the root block.
    /// </summary>
    public Block? Parent { get; set; }

    /// <summary>
    /// The functions declared in this block.
    /// </summary>
    [IncludeWithSerialization]
    public virtual Function[] Functions { get; private set; } = [];
    /// <summary>
    /// The variables declared in this block.
    /// </summary>
    [IncludeWithSerialization]
    public virtual List<Variable> Variables { get; private set; } = [];
    /// <summary>
    /// Access to the namespaces declared in this entire script. this will be null if the block is not the root block.
    /// </summary>
    [IncludeWithSerialization]
    public Namespace[]? Namespaces { get; internal set; } = [];

    /// <summary>
    /// Creates a new empty block.
    /// </summary>
    /// <param name="parent"></param>
    [DefaultArguments([null])]
    public Block(Block? parent)
    {
        Parent = parent;
    }
    /// <summary>
    /// Creates a new block with the specified source code.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="parent"></param>
    public Block(string source, Block? parent)
    {
        Tokens = Tokenizer.Tokenize(source);
        Parent = parent;
    }

    /// <summary>
    /// Declares a function in this block.
    /// </summary>
    /// <param name="function"></param>
    public void DeclareFunction(Function function)
    {
        Functions = [.. Functions, function];
    }
    /// <summary>
    /// Declares a variable in this block.
    /// </summary>
    /// <param name="variable"></param>
    public void DeclareVariable(Variable variable)
    {
        // check if the variable already exists
        // if so, override value
        Variable existing = Variables.FirstOrDefault(x => x.Name == variable.Name);
        if (existing is not null)
        {
            existing.Value = variable.Value;
            return;
        }
        Variables.Add(variable);
    }
    internal void DefineNamespace(Namespace @namespace)
    {
        Namespaces ??= [];
        Namespace existing = Namespaces.FirstOrDefault(x => x.Name == @namespace.Name);
        if (existing is not null)
        {
            existing.Merge(@namespace);
            return;
        }

        Namespaces = [.. Namespaces, @namespace];
    }
    /// <summary>
    /// Finds a variable in this block, or any parent blocks.
    /// <br></br>
    /// If the identifier is a number literal, a string literal, boolean literal, or null literal, a literal variable will be created for it.<br></br>
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The found or created variable, if none were found nor could be created, returns null</returns>
    public Variable? this[string name]
    {
        get
        {
            Variable var = Variables.FirstOrDefault(v => v.Name == name);
            if (var is not null)
            {
                return var;
            }

            Function func = Functions.FirstOrDefault(v => v.Name == name);
            if (func is not null)
            {
                return new("Function - " + func.Name, "", func, AccessControl.Private);
            }

            if (Namespaces is not null)
            {
                foreach (Namespace ns in Namespaces)
                {
                    Class? c = ns.GetClass(name);
                    if (c is not null)
                        return new Variable("Class: " + name, "", c, AccessControl.Private);
                }
            }

            if (Parent is not null)
            {
                return Parent[name];
            }

            if (name.All(x => x.IsNumber() || x is ','))
            {
                return new Variable("Number Literal", "", double.Parse(name), AccessControl.Private);
            }
            if(name is "null")
            {
                return new Variable("Null Literal", "", null, AccessControl.Private);
            }
            if(name is "true")
            {
                return new Variable("Boolean Literal", "", true, AccessControl.Private);
            }
            if(name is "false")
            {
                return new Variable("Boolean Literal", "", false, AccessControl.Private);
            }
            if (name.StartsWith('"') && name.EndsWith('"'))
            {
                return new Variable("String Literal", "", name[1..^1], AccessControl.Private);
            }

            return null;
        }
    }

    internal GotoBreak GetLabel(string identifier, int depth = 0)
    {
        foreach (var token in Tokens)
        {
            if (token.Type == TokenType.Label)
                if (token.Identifier.TrimEnd(':') == identifier)
                    return new(depth, Tokens.IndexOf(token));
        }

        if (Parent is not null)
            return Parent.GetLabel(identifier, depth + 1);

        return new(-1, depth);
    }
    internal Block CreateCopy(Block? newParent = null)
    {
        Block result = new(newParent ?? Parent);

        Functions.Foreach(x => result.DeclareFunction(x.CreateCopy(result)));
        Variables.Foreach(x => result.DeclareVariable(x.CreateCopy(result)));
        result.Tokens = Tokens;
        return result;
    }
}
