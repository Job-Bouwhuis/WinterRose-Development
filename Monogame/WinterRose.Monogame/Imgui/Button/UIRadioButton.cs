using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

public sealed class UIRadioButton : ImGuiItem
{
    public string Text { get; set; } = "New Radio Button";
    public Action OnClick { get; set; } = delegate { };
    public bool IsChecked { get; set; }

    public override void CreateItem()
    {
        if (gui.RadioButton(Text, IsChecked) && !IsChecked)
        {
            OnClick();
            IsChecked = true;
        }
    }
}
