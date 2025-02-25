using Microsoft.Xna.Framework;
using System;
using WinterRose.WIP.TestClasses;

namespace WinterRose.Monogame.TextRendering;

public class Letter
{
    private bool randomColors = false;
    public char Character { get; set; } = '\0';
    public Color Color
    {
        get
        {
            if(randomColors)
            {
                Random r = new Random();
                return new Color(r.Next(0, 255), r.Next(0, 255), r.Next(255, 255));
            }
            return color;
        }
        set
        {
            randomColors = false;
            color = value;
        }
    }
    private Color color;
    public bool isSpace()
    {
        return Character == ' ';
    }

    public bool isNewline()
    {
        return Character == '\n';
    }

    public bool isReturn()
    {
        return Character == '\r';
    }

    public bool isSpaceNewlineOrReturn()
    {
        return isSpace() || isNewline() || isReturn();
    }

    internal void EnableRandomColors() => randomColors = true;
}
