namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Renders text with a shake/jitter animation effect using stable pseudo-random noise.
/// </summary>
public class RichShake : RichElement
{
    public string Text { get; set; }
    public Color Color { get; set; }
    public float Intensity { get; set; } = 2f;  // Maximum pixel offset
    public float Speed { get; set; } = 10f;     // Frequency of shake

    public RichShake(string text, Color color, float intensity = 2f, float speed = 10f)
    {
        Text = text;
        Color = color;
        Intensity = intensity;
        Speed = speed;
    }

    public override string ToString() => $"\\[shake {Text}]";

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
        int charIndex = 0;

        // Render each character with shake offset
        foreach (char c in Text)
        {
            string charStr = c.ToString();
            var charSize = RichTextRenderer.MeasureTextExCached(context.RichText.Font, charStr, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);
            
            // Generate stable pseudo-random noise based on char index and time
            float noise = GetStableNoise(charIndex, (float)(time * Speed));
            float shakeX = (noise - 0.5f) * Intensity * 2f;
            
            noise = GetStableNoise(charIndex + 1000, (float)(time * Speed));
            float shakeY = (noise - 0.5f) * Intensity * 2f;
            
            Raylib.DrawTextEx(context.RichText.Font, charStr,
                new Vector2(charX + shakeX, y + shakeY),
                context.RichText.FontSize, context.RichText.Spacing, tinted);
            
            charX += charSize.X;
            charIndex++;
        }

        return new RichTextRenderResult
        {
            WidthConsumed = size.X + context.RichText.Spacing,
            HeightConsumed = size.Y + Intensity * 2
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        return RichTextRenderer.MeasureTextExCached(richText.Font, Text, richText.FontSize, richText.Spacing, measureCache).X;
    }

    /// <summary>
    /// Simple hash-based pseudo-random noise for stable, deterministic shake values.
    /// </summary>
    private float GetStableNoise(int seed, float t)
    {
        // Combine seed and time for stable but animated noise
        int timeInt = (int)(t * 1000);
        uint hash = 0;
        uint x = (uint)seed ^ (uint)timeInt;
        
        // Simple hash function (FNV-1a style)
        hash = 2166136261u;
        hash ^= x;
        hash *= 16777619;
        hash ^= (x >> 8);
        hash *= 16777619;
        
        // Map to [0, 1]
        return (hash % 1000) / 1000f;
    }
}
