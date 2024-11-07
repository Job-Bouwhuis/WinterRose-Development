using Microsoft.Xna.Framework.Graphics;
using System;
using WinterRose.Monogame.Imgui;

namespace WinterRose.Monogame;

/// <summary>
/// A component base class for ImGui layouts. inherits from <see cref="ObjectBehavior"/>.
/// </summary>
public abstract class ImGuiLayout : ObjectBehavior
{
    /// <summary>
    /// Use <see cref="ImGuiNET.ImGui"/> to draw your layouts
    /// </summary>
    public abstract void RenderLayout();
}
