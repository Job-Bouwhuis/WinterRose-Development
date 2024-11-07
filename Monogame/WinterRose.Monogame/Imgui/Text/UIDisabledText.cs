using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

public sealed class UIDisabledText : ImGuiItem
{
    public string Text { get; set; } = "New Disabled Text";

    public override void CreateItem() => gui.TextDisabled(Text);
}
