using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UI;

/// <summary>
/// Color tints for buttons in its different states
/// </summary>
public sealed class ButtonTints
{
    /// <summary>
    /// The button when nothing happens to it. its idle color
    /// </summary>
    public Color Normal { get; set; }
    /// <summary>
    /// The color the button has when its being hovered
    /// </summary>
    public Color Hover { get; set; }
    /// <summary>
    /// The color the button has when its being clicked.
    /// </summary>
    public Color Clicked { get; set; }
}
