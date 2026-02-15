using WinterRose;
using WinterRose.ForgeWarden;

namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

public class RichGlyph : RichElement
{
    public char Character;
    public Color Color;

    /// <summary>
    /// Set whenever this glyph is part of a link URL sentence
    /// </summary>
    public string? GlyphLinkUrl;

    public RichGlyph(char character, Color color)
    {
        Character = character;
        Color = color;
    }

    public override string ToString() => Character.ToString();

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        // Glyphs typically render as part of glyph runs in RichTextRenderer
        // This method handles single glyph rendering with modifiers support
        float x = position.X;
        float y = position.Y;

        // Determine the color to use (from modifier if present, else from glyph)
        Color finalColor = Color;
        if (ActiveModifiers?.PersistentModifiers.TryGetValue("color", out var colorMod) == true)
        {
            if (colorMod is ColorModifier cm)
                finalColor = cm.Color;
        }

        Color glyphTint = new(
            (byte)(finalColor.R * context.OverallTint.R / 255),
            (byte)(finalColor.G * context.OverallTint.G / 255),
            (byte)(finalColor.B * context.OverallTint.B / 255),
            (byte)(finalColor.A * context.OverallTint.A / 255)
        );

        var size = RichTextRenderer.MeasureTextExCached(context.RichText.Font, Character.ToString(), context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);

        // Check for modifiers
        bool isBold = ActiveModifiers?.StackableModifiers.Any(m => m is BoldModifier) ?? false;
        bool isItalic = ActiveModifiers?.StackableModifiers.Any(m => m is ItalicModifier) ?? false;
        bool isWaving = ActiveModifiers?.StackableModifiers.Any(m => m is WaveModifier) ?? false;
        bool isShaking = ActiveModifiers?.StackableModifiers.Any(m => m is ShakeModifier) ?? false;

        Vector2 renderPos = new Vector2(x, y);

        // Apply wave animation
        if (isWaving)
        {
            var waveModifier = ActiveModifiers.StackableModifiers.OfType<WaveModifier>().FirstOrDefault();
            if (waveModifier != null)
            {
                double time = Time.sinceStartup;
                // Use character hash for unique phase per character
                float phase = Character.GetHashCode() % 100 / 10f;
                float waveOffset = waveModifier.Amplitude * MathF.Sin((float)(2 * MathF.PI * (time * waveModifier.Speed + phase / waveModifier.Wavelength)));
                renderPos = new Vector2(x, y + waveOffset);
            }
        }

        // Apply shake animation
        if (isShaking)
        {
            var shakeModifier = ActiveModifiers.StackableModifiers.OfType<ShakeModifier>().FirstOrDefault();
            if (shakeModifier != null)
            {
                float noise = GetStableNoise(Character.GetHashCode(), (float)(Time.sinceStartup * shakeModifier.Speed));
                float shakeX = (noise - 0.5f) * shakeModifier.Intensity * 2f;
                
                noise = GetStableNoise(Character.GetHashCode() + 1000, (float)(Time.sinceStartup * shakeModifier.Speed));
                float shakeY = (noise - 0.5f) * shakeModifier.Intensity * 2f;
                
                renderPos = new Vector2(renderPos.X + shakeX, renderPos.Y + shakeY);
            }
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
                    Raylib.DrawTextEx(context.RichText.Font, Character.ToString(),
                        new Vector2(renderPos.X + dx * boldOffset, renderPos.Y + dy * boldOffset),
                        context.RichText.FontSize, context.RichText.Spacing, glyphTint);
                }
            }
        }

        // Apply italic
        if (isItalic)
        {
            float skewAmount = MathF.Max(2f, context.RichText.FontSize * 0.1f);
            float skewOffset = skewAmount * 0.3f;
            Raylib.DrawTextEx(context.RichText.Font, Character.ToString(),
                new Vector2(renderPos.X + skewOffset, renderPos.Y),
                context.RichText.FontSize, context.RichText.Spacing, glyphTint);
        }
        else
        {
            // Normal rendering
            Raylib.DrawTextEx(context.RichText.Font, Character.ToString(), renderPos, context.RichText.FontSize, context.RichText.Spacing, glyphTint);
        }

        if (GlyphLinkUrl is not null && context.LinkHitboxes is not null)
        {
            Raylib.DrawLineEx(new Vector2(x, y + size.Y + 2), new Vector2(x + size.X, y + size.Y + 2), 1, glyphTint);
            context.LinkHitboxes.Add((GlyphLinkUrl, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), glyphTint));
        }

        return new RichTextRenderResult
        {
            WidthConsumed = size.X + (Character == ' ' ? 0 : context.RichText.Spacing),
            HeightConsumed = size.Y + (isWaving ? (ActiveModifiers?.StackableModifiers.OfType<WaveModifier>().FirstOrDefault()?.Amplitude ?? 0) * 2 : 0)
        };
    }

    /// <summary>
    /// Simple hash-based pseudo-random noise for stable animation.
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
        return RichTextRenderer.MeasureTextExCached(richText.Font, Character.ToString(), richText.FontSize, richText.Spacing, measureCache).X;
    }
}
