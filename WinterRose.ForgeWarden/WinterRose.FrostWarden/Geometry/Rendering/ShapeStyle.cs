using Raylib_cs;
using System.IO.IsolatedStorage;

namespace WinterRose.ForgeWarden.Geometry.Rendering;

public sealed class ShapeStyle
{
    public bool HasOutline { get; private set; }
    public Color OutlineColor { get; private set; }
    public bool IsFill { get; private set; }
    public Color Color { get; private set; }
    public float Thickness { get; private set; }
    public FillRule FillRule { get; private set; }

    internal float StrokeAlpha => 1;
    internal float FillAlpha => IsFill ? 1f : 0f;

    public static ShapeStyle Default => new()
    {
        HasOutline = true,
        IsFill = true,
        Color = Color.Magenta,
        OutlineColor = Color.Black,
        Thickness = 2,
        FillRule = FillRule.NonZero
    };

    public ShapeStyle() { }

    internal ShapeStyle(bool isStroke, bool isFill, Color interpolatedColor, float thickness, FillRule fillRule)
    {
        HasOutline = isStroke;
        IsFill = isFill;
        Thickness = thickness;
        FillRule = fillRule;
        Color = interpolatedColor;
    }

    public ShapeStyle WithOutline()
    {
        HasOutline = true;
        return this;
    }

    public ShapeStyle WithOutline(Color c)
    {
        HasOutline = true;
        OutlineColor = c;
        return this;
    }

    public ShapeStyle WithoutOutline()
    {
        HasOutline = false;
        return this;
    }

    public ShapeStyle Filled()
    {
        IsFill = true;
        return this;
    }

    public ShapeStyle NotFilled()
    {
        IsFill = false;
        return this;
    }

    public ShapeStyle WithColor(Color color)
    {
        Color = color;
        return this;
    }

    public ShapeStyle WithThickness(float thickness)
    {
        Thickness = MathF.Max(0f, thickness);
        return this;
    }

    public static ShapeStyle Lerp(ShapeStyle a, ShapeStyle b, float t)
    {
        if (a == null) return b;
        if (b == null) return a;

        // interpolate color - FIXED: Do math in float, then convert to byte
        float rFloat = a.Color.R + (b.Color.R - a.Color.R) * t;
        float gFloat = a.Color.G + (b.Color.G - a.Color.G) * t;
        float bFloat = a.Color.B + (b.Color.B - a.Color.B) * t;
        float aFloat = a.Color.A + (b.Color.A - a.Color.A) * t;

        byte r = (byte)Math.Clamp((int)rFloat, 0, 255);
        byte g = (byte)Math.Clamp((int)gFloat, 0, 255);
        byte bl = (byte)Math.Clamp((int)bFloat, 0, 255);
        byte alpha = (byte)Math.Clamp((int)aFloat, 0, 255);

        // interpolate thickness
        float thickness = a.Thickness + (b.Thickness - a.Thickness) * t;

        // interpolate stroke/fill alpha separately
        float strokeAlpha = a.StrokeAlpha + (b.StrokeAlpha - a.StrokeAlpha) * t;
        if(strokeAlpha < .1)
        {

        }
        float fillAlpha = a.FillAlpha + (b.FillAlpha - a.FillAlpha) * t;
        
        bool isStroke = b?.HasOutline ?? a.HasOutline;
        bool isFill = b?.IsFill ?? a.IsFill;

        // Create color with properly typed bytes
        Color interpolatedColor = new Color(r, g, bl, alpha);

        // keep FillRule from a if equal, else pick b
        FillRule fillRule = a.FillRule == b.FillRule ? a.FillRule : (t < 0.5f ? a.FillRule : b.FillRule);

        return new ShapeStyle(isStroke, isFill, interpolatedColor, thickness, fillRule)
        {
            OutlineColor = b?.OutlineColor ?? a.OutlineColor
        };
    }
}

