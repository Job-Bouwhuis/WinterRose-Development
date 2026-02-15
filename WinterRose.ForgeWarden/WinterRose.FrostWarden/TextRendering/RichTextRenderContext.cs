namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface;

/// <summary>
/// Context passed to rendering methods containing all necessary rendering information.
/// </summary>
public class RichTextRenderContext
{
    public required RichText RichText { get; init; }
    public required ContentStyle Style { get; init; }
    public required Vector2 Position { get; init; }
    public required float MaxWidth { get; init; }
    public required Color OverallTint { get; init; }
    public required InputContext? Input { get; init; }
    public required Dictionary<string, Vector2> MeasureCache { get; init; }
    public required Dictionary<string, Vector2> SpriteSizeCache { get; init; }
    public required List<(string Url, Rectangle Rect, Color Tint)> LinkHitboxes { get; init; }
    
    /// <summary>
    /// Additional context data that can be passed to rendering elements (e.g., FunctionRegistry).
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Result of rendering an element, containing metrics and state updates.
/// </summary>
public class RichTextRenderResult
{
    public required float WidthConsumed { get; init; }
    public required float HeightConsumed { get; init; }
    public bool ShouldContinue { get; set; } = true;
}
