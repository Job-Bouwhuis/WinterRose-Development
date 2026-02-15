namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles spinner/animation escape sequences: \[spinner size=0.5;speed=2.5;color=red] or \[e ...]
/// Supports optional parameters: size, speed, color
/// </summary>
public class SpinnerSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "spinner";
    public override IEnumerable<string> AlternativeKeywords => new[] { "e" };

    private const float DefaultSpeed = 2.5f;
    private const float DefaultSize = 0.55f;

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        float spinnerSize = DefaultSize;
        float spinnerSpeed = DefaultSpeed;
        Raylib_cs.Color spinnerColor = currentColor;

        // Parse parameters if provided
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
                        case "size":
                            if (float.TryParse(value, out float size))
                                spinnerSize = size;
                            break;

                        case "speed":
                            if (float.TryParse(value, out float speed))
                                spinnerSpeed = speed;
                            break;

                        case "color":
                            spinnerColor = ParseColor(value, currentColor);
                            break;
                    }
                }
            }
        }

        elements.Add(new RichSpinner(spinnerSize, spinnerColor, spinnerSpeed));
        return currentPosition;
    }

    private Color ParseColor(string input, Color fallback)
    {
        if (input.StartsWith("#") && input.Length == 7)
        {
            return new Color(
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
            "green" => Raylib_cs.Color.Green,
            "white" => Raylib_cs.Color.White,
            "black" => Raylib_cs.Color.Black,
            "yellow" => Raylib_cs.Color.Yellow,
            _ => fallback
        };
    }
}
