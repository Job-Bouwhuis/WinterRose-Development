namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

public abstract class RichElement 
{
    /// <summary>
    /// Snapshot of active modifiers at the time this element was emitted.
    /// Renderer reads modifiers from here, not global state.
    /// </summary>
    public ModifierSnapshot? ActiveModifiers { get; set; }

    public override abstract string ToString();

    /// <summary>
    /// Renders this element at the specified position within the given context.
    /// </summary>
    /// <param name="context">Rendering context containing all necessary information</param>
    /// <param name="position">Current rendering position (x, y)</param>
    /// <returns>Rendering result with metrics and state</returns>
    public abstract RichTextRenderResult Render(RichTextRenderContext context, Vector2 position);

    /// <summary>
    /// Measures the width this element will consume at the given font size.
    /// Used for text wrapping and layout calculations.
    /// </summary>
    public abstract float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache);
}
