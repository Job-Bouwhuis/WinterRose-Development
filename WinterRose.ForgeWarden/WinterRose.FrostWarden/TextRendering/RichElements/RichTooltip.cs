namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Renders a tooltip region element that marks interactive areas.
/// Note: Full tooltip integration requires custom UIContent wrapper.
/// For now, renders text with visual indication of interactivity.
/// </summary>
public class RichTooltip : RichElement
{
    public string Text { get; set; }
    public Color Color { get; set; }
    public string TooltipContent { get; set; }

    public RichTooltip(string text, Color color, string tooltipContent)
    {
        Text = text;
        Color = color;
        TooltipContent = tooltipContent;
    }

    public override string ToString() => $"\\[tooltip {Text}|{TooltipContent}]";

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
        
        // Draw the text with a subtle underline to indicate it's interactive
        Raylib.DrawTextEx(context.RichText.Font, Text, new Vector2(x, y), context.RichText.FontSize, context.RichText.Spacing, tinted);
        Raylib.DrawLineEx(new Vector2(x, y + size.Y + 1), new Vector2(x + size.X, y + size.Y + 1), 1, new Color(tinted.R, tinted.G, tinted.B, (byte)(tinted.A * 0.5f)));

        // Create hitbox for tooltip interaction tracking
        var tooltipBounds = new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y);

        // Store bounds for external tooltip handling if needed
        // The actual tooltip display would be handled by a UI wrapper
        // that integrates this element into the tooltip system

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



