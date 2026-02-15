namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles function invocation escape sequences: \function[name] or \function[name;arg1=val1;arg2=val2]
/// </summary>
public class FunctionSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "function";
    public override IEnumerable<string> AlternativeKeywords => new[] { "func", "fn" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        if (string.IsNullOrWhiteSpace(content))
            return currentPosition;

        // Parse function name and arguments from content
        // Format: functionName or functionName;arg1=value1;arg2=value2
        var parts = content.Split(';');
        string functionName = parts[0].Trim();
        var arguments = new Dictionary<string, string>();

        // Parse arguments if provided
        for (int i = 1; i < parts.Length; i++)
        {
            var part = parts[i].Trim();
            if (part.Contains('='))
            {
                var kv = part.Split('=', 2);
                string key = kv[0].Trim().ToLowerInvariant();
                string value = kv[1].Trim();
                arguments[key] = value;
            }
            else
            {
                // Positional argument (indexed)
                arguments[$"arg{i - 1}"] = part;
            }
        }

        // Create and add the function element
        var functionElement = new RichFunction(functionName, arguments)
        {
            ActiveModifiers = null  // Functions typically don't use modifiers
        };
        elements.Add(functionElement);

        return currentPosition;
    }
}
