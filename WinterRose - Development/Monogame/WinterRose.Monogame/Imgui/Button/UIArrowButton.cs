using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

public sealed class UIArrowButton : ImGuiItem
{
    public string Text { get; set; } = "New Arrow Button";
    public Action OnClick { get; set; } = delegate { };
    public ImGuiDir ArrowDirection { get; set; } = ImGuiDir.Up;

    public override void CreateItem()
    {
        if (gui.ArrowButton(Text, ArrowDirection))
            OnClick();
    }
}
