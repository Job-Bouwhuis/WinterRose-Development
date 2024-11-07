using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

public sealed class UIColorButton : ImGuiItem
{
    public string Text { get; set; } = "New Color Button";
    public Action OnClick { get; set; } = delegate { };
    public Vector4 Color { get; set; } = Vector4.One;
    public bool WithColorPicker { get; set; }
    public bool WithToolTip { get; set; }

    public override void CreateItem()
    {
        ImGuiColorEditFlags flags = ImGuiColorEditFlags.None;
        if (!WithColorPicker)
            flags |= ImGuiColorEditFlags.NoPicker;
        if (!WithToolTip)
            flags |= ImGuiColorEditFlags.NoTooltip;

        if (gui.ColorButton(Text, Color, flags))
            OnClick();
    }
}
