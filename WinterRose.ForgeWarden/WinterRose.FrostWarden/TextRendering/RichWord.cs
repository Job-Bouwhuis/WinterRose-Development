using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.TextRendering;
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
}
