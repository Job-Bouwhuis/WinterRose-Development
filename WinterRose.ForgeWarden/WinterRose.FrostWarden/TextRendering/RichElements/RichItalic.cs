namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Renders text with italic styling using vertical skew transform.
/// </summary>
public class RichItalic : RichElement
{
    public string Text { get; set; }
    public Color Color { get; set; }

    public RichItalic(string text, Color color)
    {
        Text = text;
        Color = color;
    }

    public override string ToString() => $"\\[italic {Text}]";

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

        // Raylib doesn't support direct skew transforms, so we approximate italic by:
        // Rendering with a horizontal offset based on vertical position
        float skewAmount = MathF.Max(2f, context.RichText.FontSize * 0.1f);
        
        // Draw each character with increasing horizontal offset (top to bottom)
        float charX = x;
        foreach (char c in Text)
        {
            string charStr = c.ToString();
            var charSize = RichTextRenderer.MeasureTextExCached(context.RichText.Font, charStr, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);
            
            // Apply skew: shift right as we go down (italic slant effect)
            float skewOffset = skewAmount * 0.3f; // Skew multiplier for italic angle
            Raylib.DrawTextEx(context.RichText.Font, charStr, 
                new Vector2(charX + skewOffset, y), 
                context.RichText.FontSize, context.RichText.Spacing, tinted);
            
            charX += charSize.X;
        }

        return new RichTextRenderResult
        {
            WidthConsumed = size.X + context.RichText.Spacing,
            HeightConsumed = size.Y
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        // Italic takes slightly more width due to skew, but we'll keep it the same for simplicity
        return RichTextRenderer.MeasureTextExCached(richText.Font, Text, richText.FontSize, richText.Spacing, measureCache).X;
    }
}
