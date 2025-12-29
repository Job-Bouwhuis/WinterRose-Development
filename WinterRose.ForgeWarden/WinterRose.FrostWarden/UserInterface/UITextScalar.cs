using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.UserInterface;

public static class UITextScalar
{
    public static int ResolveFontSize(
        RichText text,
        int baseSize,
        Rectangle bounds,
        bool autoScale)
    {
        text.FontSize = baseSize;

        if (!autoScale)
            return baseSize;

        Rectangle measured = text.CalculateBounds(bounds.Width);

        float scale = 1f;
        if (measured.Width > bounds.Width || measured.Height > bounds.Height)
        {
            float widthScale = bounds.Width / measured.Width;
            float heightScale = bounds.Height / measured.Height;
            scale = Math.Min(widthScale, heightScale);
        }

        return (int)Math.Clamp(
            baseSize * scale,
            baseSize * 0.8f,
            baseSize * 2f
        );
    }

    public static Rectangle Measure(
        RichText text,
        int fontSize,
        float maxWidth)
    {
        text.FontSize = fontSize;
        return text.CalculateBounds(maxWidth);
    }
}
