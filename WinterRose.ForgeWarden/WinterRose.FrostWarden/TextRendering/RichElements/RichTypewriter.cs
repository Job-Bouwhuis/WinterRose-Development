namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Renders text with a typewriter reveal effect that progressively shows characters over time.
/// </summary>
public class RichTypewriter : RichElement
{
    public string Text { get; set; }
    public Color Color { get; set; }
    public float CharacterDelay { get; set; } = 0.1f; // Seconds per character
    public double StartTime { get; set; } = double.NaN; // Initialized on first render

    public RichTypewriter(string text, Color color, float characterDelay = 0.1f)
    {
        Text = text;
        Color = color;
        CharacterDelay = characterDelay;
    }

    public override string ToString() => $"\\typewriter[{Text}]";

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        float x = position.X;
        float y = position.Y;

        // Initialize start time on first render
        if (double.IsNaN(StartTime))
        {
            StartTime = Time.sinceStartup;
        }

        Color tinted = new Color(
            (byte)(Color.R * context.OverallTint.R / 255),
            (byte)(Color.G * context.OverallTint.G / 255),
            (byte)(Color.B * context.OverallTint.B / 255),
            (byte)(Color.A * context.OverallTint.A / 255)
        );

        var fullSize = RichTextRenderer.MeasureTextExCached(context.RichText.Font, Text, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);

        // Calculate how many characters should be visible
        double elapsed = Time.sinceStartup - StartTime;
        int visibleChars = Math.Min((int)(elapsed / CharacterDelay) + 1, Text.Length);
        
        string visibleText = Text[..visibleChars];

        // Draw visible portion
        Raylib.DrawTextEx(context.RichText.Font, visibleText, new Vector2(x, y), context.RichText.FontSize, context.RichText.Spacing, tinted);

        var visibleSize = RichTextRenderer.MeasureTextExCached(context.RichText.Font, visibleText, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);

        // If we're still animating, render a cursor or underscore at the end
        if (visibleChars < Text.Length)
        {
            float cursorX = x + visibleSize.X;
            float cursorAlpha = MathF.Sin((float)(Time.sinceStartup * 4 * MathF.PI)) * 0.5f + 0.5f; // Blinking cursor
            var cursorColor = new Color(tinted.R, tinted.G, tinted.B, (byte)(tinted.A * cursorAlpha));
            Raylib.DrawTextEx(context.RichText.Font, "_", new Vector2(cursorX, y), context.RichText.FontSize, context.RichText.Spacing, cursorColor);
        }

        return new RichTextRenderResult
        {
            WidthConsumed = (visibleChars >= Text.Length ? fullSize.X : visibleSize.X) + context.RichText.Spacing,
            HeightConsumed = fullSize.Y
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        // Return the full text width so layout is static
        return RichTextRenderer.MeasureTextExCached(richText.Font, Text, richText.FontSize, richText.Spacing, measureCache).X;
    }

    /// <summary>
    /// Reset the typewriter animation.
    /// </summary>
    public void Reset()
    {
        StartTime = double.NaN;
    }
}
