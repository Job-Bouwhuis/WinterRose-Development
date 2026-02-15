namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;

/// <summary>
/// Registry for named colors used in RichText parsing.
/// Allows both built-in and custom named colors to be resolved.
/// </summary>
public static class ColorRegistry
{
    private static readonly Dictionary<string, Color> namedColors = new();
    private static bool initialized = false;

    /// <summary>
    /// Initialize the registry with built-in colors.
    /// Called once automatically on first use.
    /// </summary>
    private static void InitializeBuiltInColors()
    {
        if (initialized) return;

        // Primary colors
        RegisterColor("red", Color.Red);
        RegisterColor("green", Color.Green);
        RegisterColor("blue", Color.Blue);
        RegisterColor("white", Color.White);
        RegisterColor("black", Color.Black);
        RegisterColor("yellow", Color.Yellow);

        // Extended color palette
        RegisterColor("orange", new Color(255, 165, 0, 255));
        RegisterColor("purple", new Color(128, 0, 128, 255));
        RegisterColor("pink", new Color(255, 192, 203, 255));
        RegisterColor("magenta", new Color(255, 0, 255, 255));
        RegisterColor("cyan", new Color(0, 255, 255, 255));
        RegisterColor("lime", new Color(0, 255, 0, 255));
        RegisterColor("navy", new Color(0, 0, 128, 255));
        RegisterColor("teal", new Color(0, 128, 128, 255));
        RegisterColor("olive", new Color(128, 128, 0, 255));
        RegisterColor("maroon", new Color(128, 0, 0, 255));
        RegisterColor("aqua", new Color(0, 255, 255, 255));
        RegisterColor("silver", new Color(192, 192, 192, 255));
        RegisterColor("gray", new Color(128, 128, 128, 255));
        RegisterColor("grey", new Color(128, 128, 128, 255));
        RegisterColor("darkgray", new Color(169, 169, 169, 255));
        RegisterColor("darkgrey", new Color(169, 169, 169, 255));
        RegisterColor("lightgray", new Color(211, 211, 211, 255));
        RegisterColor("lightgrey", new Color(211, 211, 211, 255));

        // Warm tones
        RegisterColor("coral", new Color(255, 127, 80, 255));
        RegisterColor("salmon", new Color(250, 128, 114, 255));
        RegisterColor("tomato", new Color(255, 99, 71, 255));
        RegisterColor("gold", new Color(255, 215, 0, 255));
        RegisterColor("tan", new Color(210, 180, 140, 255));
        RegisterColor("brown", new Color(165, 42, 42, 255));

        // Cool tones
        RegisterColor("skyblue", new Color(135, 206, 235, 255));
        RegisterColor("steelblue", new Color(70, 130, 180, 255));
        RegisterColor("turquoise", new Color(64, 224, 208, 255));
        RegisterColor("indigo", new Color(75, 0, 130, 255));
        RegisterColor("violet", new Color(238, 130, 238, 255));

        // UI colors
        RegisterColor("transparent", new Color(0, 0, 0, 0));
        RegisterColor("darkred", new Color(139, 0, 0, 255));
        RegisterColor("darkgreen", new Color(0, 100, 0, 255));
        RegisterColor("darkblue", new Color(0, 0, 139, 255));

        initialized = true;
    }

    /// <summary>
    /// Register a custom named color.
    /// If the name already exists, it will be overwritten.
    /// </summary>
    public static void RegisterColor(string name, Color color)
    {
        string key = name.ToLowerInvariant();
        namedColors[key] = color;
    }

    /// <summary>
    /// Try to get a color by name.
    /// Returns true if found, false otherwise.
    /// </summary>
    public static bool TryGetColor(string name, out Color color)
    {
        InitializeBuiltInColors();
        return namedColors.TryGetValue(name.ToLowerInvariant(), out color);
    }

    /// <summary>
    /// Get a color by name, or return fallback if not found.
    /// </summary>
    public static Color GetColor(string name, Color fallback)
    {
        if (TryGetColor(name, out var color))
            return color;
        return fallback;
    }

    /// <summary>
    /// Check if a color name is registered.
    /// </summary>
    public static bool ContainsColor(string name)
    {
        InitializeBuiltInColors();
        return namedColors.ContainsKey(name.ToLowerInvariant());
    }

    /// <summary>
    /// Get all registered color names.
    /// </summary>
    public static IEnumerable<string> GetAllColorNames()
    {
        InitializeBuiltInColors();
        return namedColors.Keys;
    }

    /// <summary>
    /// Clear all custom registered colors (keeps built-in colors).
    /// </summary>
    public static void ClearCustomColors()
    {
        InitializeBuiltInColors();
        
        // We could track which are built-in vs custom, but for simplicity,
        // we'll just reinitialize to restore built-ins
        var builtInCount = namedColors.Count;
        namedColors.Clear();
        initialized = false;
        InitializeBuiltInColors();
    }
}
