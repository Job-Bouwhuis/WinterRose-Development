using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

public sealed class UIButton : ImGuiItem
{
    public string Text { get; set; } = "New Button";
    public Action OnClick { get; set; } = delegate { };

    public override void CreateItem()
    {
        if (gui.Button(Text))
            OnClick();
    }
}
