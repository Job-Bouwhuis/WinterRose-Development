using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

public abstract class ImGuiItem
{
    public ImGuiItem? owner { get; set; }
    public int RenderOrder { get; set; } = 0;
    public ImGuiStyle Style { get; set; }
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.Zero;
    public bool Enabled { get; set; } = true;

    public abstract void CreateItem();
}
