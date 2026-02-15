namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Renders text with a wave animation effect using sinusoidal vertical offset.
/// </summary>
public class RichWave : RichElement
{
    public string Text { get; set; }
    public Color Color { get; set; }
    public float Amplitude { get; set; } = 3f;  // Pixel offset
    public float Speed { get; set; } = 2f;      // Oscillations per second
    public float Wavelength { get; set; } = 15f; // Horizontal distance between peaks

    public RichWave(string text, Color color, float amplitude = 3f, float speed = 2f, float wavelength = 15f)
    {
        Text = text;
        Color = color;
        Amplitude = amplitude;
        Speed = speed;
        Wavelength = wavelength;
    }

    public override string ToString() => $"\\[wave {Text}]";

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
        
        double time = Time.sinceStartup;
        float charX = x;

        // Render each character with wave offset
        foreach (char c in Text)
        {
            string charStr = c.ToString();
            var charSize = RichTextRenderer.MeasureTextExCached(context.RichText.Font, charStr, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);
            
            // Calculate wave offset based on horizontal position
            float phase = (charX - x) / Wavelength;
            float waveOffset = Amplitude * MathF.Sin((float)(2 * MathF.PI * (time * Speed + phase)));
            
            Raylib.DrawTextEx(context.RichText.Font, charStr,
                new Vector2(charX, y + waveOffset),
                context.RichText.FontSize, context.RichText.Spacing, tinted);
            
            charX += charSize.X;
        }

        return new RichTextRenderResult
        {
            WidthConsumed = size.X + context.RichText.Spacing,
            HeightConsumed = size.Y + Amplitude * 2 // Account for amplitude in height
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        return RichTextRenderer.MeasureTextExCached(richText.Font, Text, richText.FontSize, richText.Spacing, measureCache).X;
    }
}
