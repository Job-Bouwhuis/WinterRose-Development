using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace WinterRose.ForgeWarden.TextRendering.RichElements;

public class RichSpinner : RichElement
{
    public float BaseSize { get; set; }
    public Color Tint { get; set; }
    public float Speed { get; set; }
    public float HorizontalPaddingMultiplier { get; set; } = 2.5f;

    public RichSpinner() { }

    public RichSpinner(float baseSize, Color tint, float speed)
    {
        BaseSize = baseSize;
        Tint = tint;
        Speed = speed;
    }

    public override string ToString()
    {
        return $"\\e[size={BaseSize};color={ColorToHex};speed={Speed}]";
    }

    private string ColorToHex()
    {
        if (Tint.A == 255)
            return $"#{Tint.R:X2}{Tint.G:X2}{Tint.B:X2}";
        return $"#{Tint.R:X2}{Tint.G:X2}{Tint.B:X2}{Tint.A:X2}";
    }

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        float x = position.X;
        float y = position.Y;

        // compute spinner pixel size relative to font
        float spinnerHeight = BaseSize * context.RichText.FontSize;
        float diameter = spinnerHeight;
        float widthAllocated = diameter * HorizontalPaddingMultiplier;

        Color tintedSpinner = new Color(
            (byte)(Tint.R * context.OverallTint.R / 255),
            (byte)(Tint.G * context.OverallTint.G / 255),
            (byte)(Tint.B * context.OverallTint.B / 255),
            (byte)(Tint.A * context.OverallTint.A / 255)
        );

        // compute ping-pong phase t in [0,1]
        double time = Time.sinceStartup;
        float raw = (float)((time * Speed) % 2.0);
        float t = raw > 1f ? 2f - raw : raw;

        // quintic ease-in-out
        float eased = EvaluateBusyEased(t);

        // small margin so ball doesn't touch edges
        float margin = MathF.Max(1f, diameter * 0.25f);
        float travelRange = MathF.Max(0f, widthAllocated - diameter - (margin * 2f));

        // compute center position for the little ball
        float cx = x + margin + eased * travelRange + diameter * 0.5f;
        float cy = y + (context.RichText.FontSize - spinnerHeight) / 2f + spinnerHeight * 0.5f;

        // background dot (subtle)
        var bgAlpha = (byte)(tintedSpinner.A * 0.20f);
        var bg = new Color(tintedSpinner.R, tintedSpinner.G, tintedSpinner.B, bgAlpha);
        Raylib.DrawCircleV(new Vector2(x + widthAllocated * 0.5f, cy), diameter * 0.45f, bg);

        // draw the moving sphere
        Raylib.DrawCircleV(new Vector2(cx, cy), diameter * 0.45f, tintedSpinner);

        return new RichTextRenderResult
        {
            WidthConsumed = widthAllocated + context.RichText.Spacing,
            HeightConsumed = spinnerHeight
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        float spinnerHeight = BaseSize * richText.FontSize;
        float diameter = spinnerHeight;
        return diameter * HorizontalPaddingMultiplier;
    }

    private static float EvaluateBusyEased(float t)
    {
        if (t < 0.5f)
        {
            float x = 2f * t;
            return 0.5f * MathF.Pow(x, 5);
        }
        else
        {
            float x = 2f * (1f - t);
            return 1f - 0.5f * MathF.Pow(x, 5);
        }
    }
}

