namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles button escape sequences: \button[label;functionName] or \btn[label;functionName;arg1=val1;arg2=val2]
/// </summary>
public class ButtonSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "button";
    public override IEnumerable<string> AlternativeKeywords => new[] { "btn" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        if (string.IsNullOrWhiteSpace(content))
            return currentPosition;

        // Parse button label, function name, and arguments from content
        // Format: label;functionName or label;functionName;arg1=value1;arg2=value2
        var parts = content.Split(';');
        
        if (parts.Length < 2)
            parts = [content, ""];

        string label = parts[0].Trim();
        string functionName = parts[1].Trim();
        var arguments = new Dictionary<string, string>();

        // Parse arguments if provided
        for (int i = 2; i < parts.Length; i++)
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
                arguments[$"arg{i - 2}"] = part;
            }
        }

        // Create and add the button element
        var buttonElement = new RichButton(label, functionName, arguments)
        {
            ActiveModifiers = null  // Buttons typically don't use modifiers
        };
        elements.Add(buttonElement);

        return currentPosition;
    }
}
