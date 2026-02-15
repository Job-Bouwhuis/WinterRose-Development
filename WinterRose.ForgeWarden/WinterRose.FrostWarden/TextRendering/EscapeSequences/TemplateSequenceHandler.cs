namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Template for creating a new escape sequence handler.
/// 
/// Usage Example:
///   \[myKeyword parameter1=value1;parameter2=value2]
///   or
///   \[m value]  (if "m" is an alternative keyword)
/// </summary>
public class TemplateSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "myKeyword";
    
    public override IEnumerable<string> AlternativeKeywords => new[] { "m" };

    /// <summary>
    /// Main parsing method called when this escape sequence is encountered.
    /// 
    /// Parameters:
    ///   - content: The text between brackets (e.g., "value" from \[myKeyword value])
    ///   - currentColor: Current color context from the parser
    ///   - elements: List to add created RichElement objects to
    ///   - text: Original full text being parsed (for lookahead)
    ///   - currentPosition: Position after the closing bracket (for multi-part sequences)
    /// 
    /// Return:
    ///   The position to continue parsing from. Typically currentPosition unless your
    ///   handler needs to consume additional characters (like SpriteSequenceHandler's \! modifier).
    /// </summary>
    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        // TODO: Parse the content parameter
        // Example parsing key=value pairs:
        /*
        string param1Value = "";
        Raylib_cs.Color param2Value = currentColor;

        if (!string.IsNullOrEmpty(content))
        {
            var parameters = content.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var param in parameters)
            {
                var keyValue = param.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim().ToLowerInvariant();
                    string value = keyValue[1].Trim();

                    switch (key)
                    {
                        case "param1":
                            param1Value = value;
                            break;

                        case "param2":
                            param2Value = ParseColor(value, currentColor);
                            break;
                    }
                }
            }
        }
        */

        // TODO: Create your RichElement and add it to the elements list
        // Example:
        // elements.Add(new RichMyElement(param1Value, param2Value));

        // TODO: Check if additional parsing is needed (e.g., checking for \! modifier like sprites)
        // Example:
        /*
        int nextPos = currentPosition;
        if (text.Length >= nextPos + 2 && text[nextPos] == '\\' && text[nextPos + 1] == '!')
        {
            // Handle special modifier
            nextPos += 2;
        }
        return nextPos;
        */

        return currentPosition;
    }

    // TODO: Add any helper methods for parsing (e.g., color parsing, validation)
    /*
    private Raylib_cs.Color ParseColor(string input, Raylib_cs.Color fallback)
    {
        if (input.StartsWith("#") && input.Length == 7)
        {
            return new Raylib_cs.Color(
                Convert.ToInt32(input.Substring(1, 2), 16),
                Convert.ToInt32(input.Substring(3, 2), 16),
                Convert.ToInt32(input.Substring(5, 2), 16),
                255
            );
        }

        return input.ToLower() switch
        {
            "red" => Raylib_cs.Color.Red,
            "blue" => Raylib_cs.Color.Blue,
            _ => fallback
        };
    }
    */
}
