namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Renders text with bold styling using multi-pass rendering with offset strokes.
/// </summary>
public class RichBold : RichElement
{
    public string Text { get; set; }
    public Color Color { get; set; }

    public RichBold(string text, Color color)
    {
        Text = text;
        Color = color;
    }

    public override string ToString() => $"\\[bold {Text}]";

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        float x = position.X;
        float y = position.Y;

        Color tinted = new Color(
            (byte)(Color.R * context.OverallTint.R / 255),
            (byte)(Color.G * context.OverallTint.G / 255),
            (byte)(Color.B * context.OverallTint.B / 255),
            (byte)(Color.A * context.OverallTint.A / 255)
        );

        var size = RichTextRenderer.MeasureTextExCached(context.RichText.Font, Text, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);

        // Draw multiple passes with offset for bold effect
        float boldOffset = MathF.Max(1f, context.RichText.FontSize * 0.02f);
        
        // Background passes for stroke effect
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Raylib.DrawTextEx(context.RichText.Font, Text, 
                    new Vector2(x + dx * boldOffset, y + dy * boldOffset), 
                    context.RichText.FontSize, context.RichText.Spacing, tinted);
            }
        }

        // Main text
        Raylib.DrawTextEx(context.RichText.Font, Text, new Vector2(x, y), context.RichText.FontSize, context.RichText.Spacing, tinted);

        return new RichTextRenderResult
        {
            WidthConsumed = size.X + context.RichText.Spacing,
            HeightConsumed = size.Y
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        return RichTextRenderer.MeasureTextExCached(richText.Font, Text, richText.FontSize, richText.Spacing, measureCache).X;
    }
}
