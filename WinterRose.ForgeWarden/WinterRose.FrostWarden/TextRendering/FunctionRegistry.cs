namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents the result of a function invocation.
/// </summary>
public class FunctionResult
{
    /// <summary>
    /// Whether the function executed successfully.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// The return value from the function (can be any type).
    /// </summary>
    public object? ReturnValue { get; set; }

    /// <summary>
    /// Error message if the function failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Width consumed by the function result (for UI functions).
    /// </summary>
    public float WidthConsumed { get; set; } = 0;

    /// <summary>
    /// Height consumed by the function result (for UI functions).
    /// </summary>
    public float HeightConsumed { get; set; } = 0;
}

/// <summary>
/// Delegate for function implementations.
/// </summary>
public delegate FunctionResult FunctionDelegate(
    string functionName,
    Dictionary<string, string> arguments,
    RichTextRenderContext context,
    Vector2 position
);

/// <summary>
/// Defines a function that can be invoked from RichText.
/// </summary>
public class FunctionDefinition
{
    /// <summary>
    /// Name of the function (used in \function[name] syntax).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of what the function does.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Expected parameter names (for documentation/validation).
    /// </summary>
    public IList<string> ParameterNames { get; set; } = new List<string>();

    /// <summary>
    /// The implementation of this function.
    /// </summary>
    public FunctionDelegate Implementation { get; set; }

    public FunctionDefinition(
        string name,
        FunctionDelegate implementation,
        string description = "")
    {
        Name = name;
        Implementation = implementation;
        Description = description;
    }

    public FunctionDefinition(
        string name,
        FunctionDelegate implementation,
        IList<string> parameterNames,
        string description = "")
    {
        Name = name;
        Implementation = implementation;
        ParameterNames = parameterNames;
        Description = description;
    }
}

/// <summary>
/// Registry of function definitions that can be invoked from RichText.
/// Pass an instance to RichTextRenderer.Draw to enable function invocation.
/// </summary>
public class FunctionRegistry
{
    private readonly Dictionary<string, FunctionDefinition> functions = new();
    private readonly Dictionary<string, object> globalContext = new();

    /// <summary>
    /// Register a function definition.
    /// </summary>
    public void RegisterFunction(FunctionDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));
        
        functions[definition.Name.ToLowerInvariant()] = definition;
    }

    /// <summary>
    /// Register a function with a simple delegate.
    /// </summary>
    public void RegisterFunction(string name, FunctionDelegate implementation)
    {
        RegisterFunction(new FunctionDefinition(name, implementation));
    }

    /// <summary>
    /// Register a function with parameters and description.
    /// </summary>
    public void RegisterFunction(
        string name,
        FunctionDelegate implementation,
        IList<string> parameterNames,
        string description = "")
    {
        RegisterFunction(new FunctionDefinition(name, implementation, parameterNames, description));
    }

    /// <summary>
    /// Try to get a function definition by name.
    /// </summary>
    public bool TryGetFunction(string name, out FunctionDefinition? definition)
    {
        return functions.TryGetValue(name.ToLowerInvariant(), out definition);
    }

    /// <summary>
    /// Invoke a function by name with arguments.
    /// </summary>
    public FunctionResult InvokeFunction(
        string functionName,
        Dictionary<string, string> arguments,
        RichTextRenderContext context,
        Vector2 position)
    {
        if (!TryGetFunction(functionName, out var definition))
        {
            return new FunctionResult
            {
                Success = false,
                ErrorMessage = $"Function '{functionName}' not found"
            };
        }

        try
        {
            return definition.Implementation(functionName, arguments, context, position);
        }
        catch (Exception ex)
        {
            return new FunctionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Store a global context value accessible to all functions.
    /// </summary>
    public void SetContextValue(string key, object value)
    {
        globalContext[key] = value;
    }

    /// <summary>
    /// Get a global context value.
    /// </summary>
    public bool TryGetContextValue(string key, out object? value)
    {
        return globalContext.TryGetValue(key, out value);
    }

    /// <summary>
    /// Get all registered function names.
    /// </summary>
    public IEnumerable<string> GetFunctionNames()
    {
        return functions.Keys;
    }

    /// <summary>
    /// Check if a function is registered.
    /// </summary>
    public bool HasFunction(string name)
    {
        return functions.ContainsKey(name.ToLowerInvariant());
    }
}
