using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

public sealed class UIFormatText : ImGuiItem
{
    public Func<UIFormatText, object?, string> Text { get; set; } = delegate { return "New Formatted Text"; };
    public Vector4 Color { get; set; } = Vector4.One;
    public object? Arg { get; set; } = null;

    public override void CreateItem()
    {
        gui.TextColored(Color, Text(this, Arg));
    }
}
