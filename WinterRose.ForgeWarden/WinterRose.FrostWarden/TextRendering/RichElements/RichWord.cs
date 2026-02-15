using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeWarden;

namespace WinterRose.ForgeWarden.TextRendering.RichElements;
public class RichWord : RichElement
{
    public string Text { get; set; }
    public Color Color { get; set; }
    public string? LinkUrl { get; set; }

    public RichWord(string text, Color color, string? linkUrl = null)
    {
        Text = text;
        Color = color;
        LinkUrl = linkUrl;
    }

    public override string ToString() => Text;

    
    /// <summary>
    /// The character index where this word starts in the typewriter sequence.
    /// Set by the rendering system to track position.
    /// </summary>
    public int TypewriterSequenceStartIndex { get; set; } = 0;

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        float x = position.X;
        float y = position.Y;

        // Determine the color to use (from modifier if present, else from element)
        Color finalColor = Color;
        if (ActiveModifiers?.PersistentModifiers.TryGetValue("color", out var colorMod) == true)
        {
            if (colorMod is ColorModifier cm)
                finalColor = cm.Color;
        }

        Color tinted = new Color(
            (byte)(finalColor.R * context.OverallTint.R / 255),
            (byte)(finalColor.G * context.OverallTint.G / 255),
            (byte)(finalColor.B * context.OverallTint.B / 255),
            (byte)(finalColor.A * context.OverallTint.A / 255)
        );

        var size = RichTextRenderer.MeasureTextExCached(context.RichText.Font, Text, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);
        
        // Check for stackable modifiers and apply rendering effects
        bool isBold = ActiveModifiers?.StackableModifiers.Any(m => m is BoldModifier) ?? false;
        bool isItalic = ActiveModifiers?.StackableModifiers.Any(m => m is ItalicModifier) ?? false;
        bool isWaving = ActiveModifiers?.StackableModifiers.Any(m => m is WaveModifier) ?? false;
        bool isShaking = ActiveModifiers?.StackableModifiers.Any(m => m is ShakeModifier) ?? false;
        bool isTypewriter = ActiveModifiers?.StackableModifiers.Any(m => m is TypewriterModifier) ?? false;

        // Handle typewriter effect
        if (isTypewriter)
        {
            var twModifier = ActiveModifiers!.StackableModifiers.OfType<TypewriterModifier>().FirstOrDefault();
            if (twModifier != null)
            {
                // Initialize global typewriter sequence on first render
                if (double.IsNaN(twModifier.StartTime))
                {
                    twModifier.StartTime = Time.sinceStartup;
                }

                // Calculate total characters revealed across the entire sequence
                double elapsed = Time.sinceStartup - twModifier.StartTime;
                Console.WriteLine(elapsed);
                int totalCharsRevealed = (int)(elapsed / twModifier.CharacterDelay);

                // Calculate how many characters of THIS word should be visible
                int wordStartIndex = TypewriterSequenceStartIndex;
                int wordEndIndex = TypewriterSequenceStartIndex + Text.Length;
                
                // Only show characters of this word if we've reached its position in the sequence
                int visibleCharsInThisWord = 0;
                if (totalCharsRevealed >= wordStartIndex)
                {
                    visibleCharsInThisWord = Math.Min(totalCharsRevealed - wordStartIndex, Text.Length);
                }

                // Don't render if we're not at this word yet
                if (visibleCharsInThisWord == 0 && totalCharsRevealed < wordStartIndex)
                {
                    return new RichTextRenderResult
                    {
                        WidthConsumed = 0,
                        HeightConsumed = size.Y
                    };
                }

                string visibleText = visibleCharsInThisWord > 0 ? Text[..visibleCharsInThisWord] : "";

                // Draw visible portion
                if (!string.IsNullOrEmpty(visibleText))
                {
                    Raylib.DrawTextEx(context.RichText.Font, visibleText, new Vector2(x, y), context.RichText.FontSize, context.RichText.Spacing, tinted);
                }

                // If we're still animating this word, render a blinking cursor
                if (visibleCharsInThisWord < Text.Length && totalCharsRevealed >= wordStartIndex)
                {
                    var visibleSize = visibleCharsInThisWord > 0 
                        ? RichTextRenderer.MeasureTextExCached(context.RichText.Font, visibleText, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache)
                        : new Vector2(0, 0);
                    
                    float cursorX = x + visibleSize.X;
                    float cursorAlpha = MathF.Sin((float)(Time.sinceStartup * 4 * MathF.PI)) * 0.5f + 0.5f;
                    var cursorColor = new Color(tinted.R, tinted.G, tinted.B, (byte)(tinted.A * cursorAlpha));
                    Raylib.DrawTextEx(context.RichText.Font, "_", new Vector2(cursorX, y), context.RichText.FontSize, context.RichText.Spacing, cursorColor);
                }

                // Return actual width consumed
                float consumedWidth = visibleCharsInThisWord > 0 
                    ? RichTextRenderer.MeasureTextExCached(context.RichText.Font, visibleText, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache).X
                    : 0;

                return new RichTextRenderResult
                {
                    WidthConsumed = consumedWidth + context.RichText.Spacing,
                    HeightConsumed = size.Y
                };
            }
        }

        // Apply wave or shake with per-character rendering
        if (isWaving || isShaking)
        {
            float charX = x;
            var waveModifier = isWaving ? ActiveModifiers.StackableModifiers.OfType<WaveModifier>().FirstOrDefault() : null;
            var shakeModifier = isShaking ? ActiveModifiers.StackableModifiers.OfType<ShakeModifier>().FirstOrDefault() : null;
            int charIndex = 0;

            foreach (char c in Text)
            {
                string charStr = c.ToString();
                var charSize = RichTextRenderer.MeasureTextExCached(context.RichText.Font, charStr, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);
                Vector2 charRenderPos = new Vector2(charX, y);

                // Apply wave animation with per-character phase
                if (isWaving && waveModifier != null)
                {
                    double time = Time.sinceStartup;
                    float phase = charIndex; // Each character gets its own phase
                    float waveOffset = waveModifier.Amplitude * MathF.Sin((float)(2 * MathF.PI * (time * waveModifier.Speed + phase / waveModifier.Wavelength)));
                    charRenderPos = new Vector2(charX, y + waveOffset);
                }

                // Apply shake animation
                if (isShaking && shakeModifier != null)
                {
                    float noise = GetStableNoise(charIndex, (float)(Time.sinceStartup * shakeModifier.Speed));
                    float shakeX = (noise - 0.5f) * shakeModifier.Intensity * 2f;
                    
                    noise = GetStableNoise(charIndex + 1000, (float)(Time.sinceStartup * shakeModifier.Speed));
                    float shakeY = (noise - 0.5f) * shakeModifier.Intensity * 2f;
                    
                    charRenderPos = new Vector2(charRenderPos.X + shakeX, charRenderPos.Y + shakeY);
                }

                // Apply bold
                if (isBold)
                {
                    float boldOffset = MathF.Max(1f, context.RichText.FontSize * 0.05f);
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            Raylib.DrawTextEx(context.RichText.Font, charStr,
                                new Vector2(charRenderPos.X + dx * boldOffset, charRenderPos.Y + dy * boldOffset),
                                context.RichText.FontSize, context.RichText.Spacing, tinted);
                        }
                    }
                }

                // Apply italic
                if (isItalic)
                {
                    float skewAmount = MathF.Max(2f, context.RichText.FontSize * 0.1f);
                    float skewOffset = skewAmount * 0.3f;
                    Raylib.DrawTextEx(context.RichText.Font, charStr,
                        new Vector2(charRenderPos.X + skewOffset, charRenderPos.Y),
                        context.RichText.FontSize, context.RichText.Spacing, tinted);
                }
                else
                {
                    // Normal rendering
                    Raylib.DrawTextEx(context.RichText.Font, charStr, charRenderPos, context.RichText.FontSize, context.RichText.Spacing, tinted);
                }

                charX += charSize.X;
                charIndex++;
            }
        }
        else
        {
            // No wave/shake - render as block
            Vector2 renderPos = new Vector2(x, y);

            // Apply bold (multi-pass stroke)
            if (isBold)
            {
                float boldOffset = MathF.Max(1f, context.RichText.FontSize * 0.05f);
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        Raylib.DrawTextEx(context.RichText.Font, Text,
                            new Vector2(renderPos.X + dx * boldOffset, renderPos.Y + dy * boldOffset),
                            context.RichText.FontSize, context.RichText.Spacing, tinted);
                    }
                }
            }

            // Apply italic (character skew)
            if (isItalic)
            {
                float skewAmount = MathF.Max(2f, context.RichText.FontSize * 0.1f);
                float charX = renderPos.X;
                foreach (char c in Text)
                {
                    string charStr = c.ToString();
                    var charSize = RichTextRenderer.MeasureTextExCached(context.RichText.Font, charStr, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);
                    float skewOffset = skewAmount * 0.3f;
                    Raylib.DrawTextEx(context.RichText.Font, charStr,
                        new Vector2(charX + skewOffset, renderPos.Y),
                        context.RichText.FontSize, context.RichText.Spacing, tinted);
                    charX += charSize.X;
                }
            }
            else
            {
                // Draw normal text
                Raylib.DrawTextEx(context.RichText.Font, Text, renderPos, context.RichText.FontSize, context.RichText.Spacing, tinted);
            }
        }

        if (!string.IsNullOrEmpty(LinkUrl))
        {
            Raylib.DrawLineEx(new Vector2(x, y + size.Y + 2), new Vector2(x + size.X, y + size.Y + 2), 1, tinted);
            context.LinkHitboxes.Add((LinkUrl, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), tinted));
        }

        return new RichTextRenderResult
        {
            WidthConsumed = size.X + context.RichText.Spacing,
            HeightConsumed = size.Y + (isWaving ? (ActiveModifiers?.StackableModifiers.OfType<WaveModifier>().FirstOrDefault()?.Amplitude ?? 0) * 2 : 0)
        };
    }

    /// <summary>
    /// Simple hash-based pseudo-random noise for stable, deterministic animation values.
    /// </summary>
    private float GetStableNoise(int seed, float t)
    {
        int timeInt = (int)(t * 1000);
        uint hash = 0;
        uint x = (uint)seed ^ (uint)timeInt;
        
        hash = 2166136261u;
        hash ^= x;
        hash *= 16777619;
        hash ^= (x >> 8);
        hash *= 16777619;
        
        return (hash % 1000) / 1000f;
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        return RichTextRenderer.MeasureTextExCached(richText.Font, Text, richText.FontSize, richText.Spacing, measureCache).X;
    }
}
