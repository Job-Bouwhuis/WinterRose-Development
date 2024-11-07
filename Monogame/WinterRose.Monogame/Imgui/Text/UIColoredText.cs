using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UserInterface.ImGuiInterface
{
    internal class UIColoredText : ImGuiItem
    {
        public string Text { get; set; } = "New Colored Text";
        public Vector4 Color { get; set; } = Vector4.One;

        public override void CreateItem() => gui.TextColored(Color, Text);
    }
}
