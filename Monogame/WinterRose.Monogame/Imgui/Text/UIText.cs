using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

public sealed class UIText : ImGuiItem
{
    public string Text { get; set; } = "New UI Text";

    public override void CreateItem()
    {
        gui.Text(Text);
    }
}
