using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeWarden.TextRendering;

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

}
