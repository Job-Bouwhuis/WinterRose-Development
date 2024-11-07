using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using WinterRose.Serialization;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting;

[DebuggerDisplay("Function: {Name}")]
[method: DefaultArguments("", "", AccessControl.Public)]
public class Function(string name, string description, AccessControl accessmodifiers)
{
    public Class DelcaredClass { get; internal set; }
    [IncludeWithSerialization]
    public string Name { get; protected set; } = name;
    [IncludeWithSerialization]
    public string Description { get; protected set; } = description;
    [IncludeWithSerialization]
    public Parameter[] Parameters { get; protected set; }
    [IncludeWithSerialization]
    public Block Body { get; protected set; } = new(null);
    [IncludeWithSerialization]
    public AccessControl AccessModifiers { get; protected set; }
    public bool ReturnsValue => IsCSharpFunction ? CSharpFunction.Method.ReturnType != typeof(void) : Body.Tokens.Any(x => x.Identifier == "return");
    public bool IsCSharpFunction => func != null;
    
    private Delegate func;
    /// <summary>
    /// The C# function that is called when the function is invoked.
    /// <br></br>
    /// When set, the functions body is not interpreted, and this delegate is called instead.
    /// </summary>
    public Delegate CSharpFunction
    {
        get => func;
        set => func = value;
    }

    public Variable Invoke()
    {
        if(IsCSharpFunction)
            return new Variable("C#FunctionResult", "A C# function executed and this was the result", func.DynamicInvoke());

        Block bodyCopy = Body.CreateCopy();

        return new Interpreter(bodyCopy).Interpret(DelcaredClass, new() { FromFunction = true });
    }
    public Variable Invoke(params Variable[] variables)
    {
        if(IsCSharpFunction)
        {
            var args = ValidateArgsForCSharp(variables);
            return new Variable("C#FunctionResult", "A C# function executed and this was the result", func.DynamicInvoke(args));
        }

        Block bodyCopy = Body.CreateCopy();

        foreach (int i in 0..variables.Length)
        {
            bodyCopy.DeclareVariable(new Variable(Parameters[i].Name, Parameters[i].Description, variables[i].Value, AccessControl.Private));
        }
        return new Interpreter(bodyCopy).Interpret(DelcaredClass, new() { FromFunction = true });
    }

    public object?[]? ValidateArgsForCSharp(Variable[] args)
    {
        object?[]? newArgs = new object[args.Length];
        ParameterInfo[] parameters = CSharpFunction.Method.GetParameters();

        if(parameters.Length != args.Length)
            throw new WinterThornExecutionError(ThornError.SyntaxError, "WT-0006", $"Invalid number of arguments for function {Name}.");

        for (int i = 0; i < args.Length; i++)
        {
            Variable? arg = args[i];
            ParameterInfo param = parameters[i];
            
            if(arg == null)
                throw new WinterThornExecutionError(ThornError.SyntaxError, "WT-0006", $"Invalid number of arguments for function {Name}.");

            if(param.ParameterType == typeof(Variable))
            {
                newArgs[i] = arg;
                continue;
            }

            Type[] valueTypes = 
            [
                typeof(int), 
                typeof(double), 
                typeof(bool), 
                typeof(string),
                typeof(Class)
            ];

            if(valueTypes.Contains(param.ParameterType))
            {
                newArgs[i] = arg.Value;
                continue;
            }

            if(arg.Value.GetType() == param.ParameterType)
            {
                newArgs[i] = arg.Value;
                continue;
            }

            throw new WinterThornExecutionError(ThornError.SyntaxError, "WT-0006", $"Invalid argument type for function {Name}.");
        }
        return newArgs;
    }

    /// <summary>
    /// Adds the body to the function.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public Function WithBody(Block body)
    {
        Body = body;
        return this;
    }
    /// <summary>
    /// Gives the function the specified parameters.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public Function WithParameterList(Parameter[] parameters)
    {
        Parameters = parameters;
        return this;
    }

    internal Function CreateCopy(Block newParent)
    {
        return new(Name, Description, AccessModifiers)
        {
            Body = Body.CreateCopy(newParent),
            Parameters = Parameters,
            DelcaredClass = DelcaredClass,
            CSharpFunction = CSharpFunction,
        };

    }

    internal void SetBody(Block body)
    {
        Body = body;
    }

    internal void SetParameters(params Parameter[] parameters)
    {
        Parameters = parameters;
    }
}