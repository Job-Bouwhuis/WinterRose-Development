namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Renders an inline progress bar element that respects text flow and line height.
/// </summary>
public class RichProgressBar : RichElement
{
    public float Value { get; set; }           // Current value (0-1 if using normalized, or raw if using max)
    public float? MaxValue { get; set; }       // Optional max value for normalization
    public float Width { get; set; } = 100f;   // Bar width in pixels
    public Color FillColor { get; set; } = Color.Green;
    public Color BackgroundColor { get; set; } = new Color(50, 50, 50, 255);
    public Color BorderColor { get; set; } = Color.White;
    public float Height { get; set; } = 12f;   // Height relative to font

    public RichProgressBar(float value, float? maxValue = null, float width = 100f)
    {
        Value = value;
        MaxValue = maxValue;
        Width = width;
    }

    public override string ToString() => $"\\[progress value={Value} max={MaxValue}]";

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        float x = position.X;
        float y = position.Y;

        // Calculate normalized value
        float normalizedValue = MaxValue.HasValue ? Value / MaxValue.Value : Value;
        normalizedValue = Math.Clamp(normalizedValue, 0f, 1f);

        // Calculate bar dimensions - centered vertically in text
        float barHeight = Math.Min(Height, context.RichText.FontSize * 0.8f);
        float barY = y + (context.RichText.FontSize - barHeight) / 2f;

        // Apply overall tint to colors
        Color tingedFill = new Color(
            (byte)(FillColor.R * context.OverallTint.R / 255),
            (byte)(FillColor.G * context.OverallTint.G / 255),
            (byte)(FillColor.B * context.OverallTint.B / 255),
            (byte)(FillColor.A * context.OverallTint.A / 255)
        );

        Color tingedBackground = new Color(
            (byte)(BackgroundColor.R * context.OverallTint.R / 255),
            (byte)(BackgroundColor.G * context.OverallTint.G / 255),
            (byte)(BackgroundColor.B * context.OverallTint.B / 255),
            (byte)(BackgroundColor.A * context.OverallTint.A / 255)
        );

        Color tingedBorder = new Color(
            (byte)(BorderColor.R * context.OverallTint.R / 255),
            (byte)(BorderColor.G * context.OverallTint.G / 255),
            (byte)(BorderColor.B * context.OverallTint.B / 255),
            (byte)(BorderColor.A * context.OverallTint.A / 255)
        );

        // Draw background
        var bgRect = new Rectangle(x, barY, Width, barHeight);
        Raylib.DrawRectangleRec(bgRect, tingedBackground);

        // Draw fill
        float fillWidth = Width * normalizedValue;
        var fillRect = new Rectangle(x, barY, fillWidth, barHeight);
        Raylib.DrawRectangleRec(fillRect, tingedFill);

        // Draw border
        Raylib.DrawRectangleLinesEx(bgRect, 1, tingedBorder);

        return new RichTextRenderResult
        {
            WidthConsumed = Width + context.RichText.Spacing,
            HeightConsumed = context.RichText.FontSize
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        return Width;
    }
}
