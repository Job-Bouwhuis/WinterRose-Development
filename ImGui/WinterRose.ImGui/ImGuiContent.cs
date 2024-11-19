using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ImGuiApps;

/// <summary>
/// A base class for all content that can be displayed in an <see cref="Application"/>
/// </summary>
public abstract class ImGuiContent
{
    /// <summary>
    /// Whether the content is visible or not.
    /// </summary>
    public bool IsVisible { get; set; } = true;
    /// <summary>
    /// Renders the content.
    /// </summary>
    public abstract void Render();
    /// <summary>
    /// The application that this content is a part of.
    /// </summary>
    public Application Application { get; internal set; }
}
