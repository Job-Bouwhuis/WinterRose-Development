using Raylib_cs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;

public enum ToastStackSide
{
    Top,
    Bottom
}

public enum ToastRegion
{
    Right,
    Center,
    Left
}

public static class Toasts
{
    private static readonly Dictionary<ToastRegion, ToastRegionManager> regions = new();

    internal static readonly float TOAST_WIDTH;
    internal static readonly float TOAST_HEIGHT;
    private static bool requestReorder;

    internal static InputContext Input { get; }

    static Toasts()
    {
        Input = new InputContext(new RaylibInputProvider(), 110002);
        // Baseline reference resolution
        const float REFERENCE_WIDTH = 1920f;
        const float REFERENCE_HEIGHT = 1080f;

        float windowWidth = ForgeWardenEngine.Current.Window.Width;
        float windowHeight = ForgeWardenEngine.Current.Window.Height;

        float scaleX = windowWidth / REFERENCE_WIDTH;
        float scaleY = windowHeight / REFERENCE_HEIGHT;

        TOAST_WIDTH = 350f * scaleX;
        TOAST_HEIGHT = 170f * scaleY;

        // Setup default regions
        regions[ToastRegion.Right] = new ToastRightRegionManager();
        regions[ToastRegion.Left] = new ToastLeftRegionManager();
        regions[ToastRegion.Center] = new ToastCenterRegionManager();
    }

    public static Toast ShowToast(Toast toast)
    {
        regions[toast.Region].EnqueueToast(toast);
        return toast;
    }

    internal static void RequestReorder()
    {
        requestReorder = true;
    }

    public static int GetNumberOfToastsActive()
    {
        return regions.Values.Select(r => r.NumberOfToasts).Sum();
    }

    private static int hoveredRegionIndex = -1; // -1 means no region hovered
    private static int lastHoveredRegionIndex = -1;
    private static float regionHeight = 300f; // adjustable height for all regions
    private static Color regionColor = Color.Gray;
    private static Color hoverBorderColor = Color.Yellow; // adjustable border color
    private static float borderThickness = 2f;

    private static float reorderTime = 2;
    private static float reorderTimer = 0; 

    public static void Update(float deltaTime)
    {
        reorderTimer += Time.deltaTime;
        if (ray.IsWindowResized() || requestReorder || reorderTimer >= reorderTime)
        {
            reorderTimer = 0;
            requestReorder = false;
            foreach (var region in regions.Values)
                region.RecalculatePositions();
        }

        bool anyHovered = false;

        foreach (var region in regions.Values)
        {
            region.Update();
            if (region.IsSomeoneHovered)
                anyHovered = true;
        }

        if (anyHovered /*&& Input.HighestPriorityMouseAbove == null*/)
            Input.IsRequestingMouseFocus = true;
        else
        {
            Input.IsRequestingMouseFocus = false;
        }
    }

    public static void Draw()
    {
        foreach (var region in regions.Values)
            region.Draw();
    }

    internal static void RemoveImmediately(Toast toast, ToastRegion region)
    {
        regions[region].RemoveImmediately(toast);
    }

    /// <summary>
    /// Shows a neutral toast with the given message
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="region">The region where the toast appears. Defaults to Right.</param>
    /// <param name="side">The stack side of the toast. Defaults to Top.</param>
    /// <returns>The created Toast instance.</returns>
    public static Toast Neutral(string message, ToastRegion region = ToastRegion.Right, ToastStackSide side = ToastStackSide.Top)
    {
        return ShowToast(new Toast(ToastType.Neutral, region, side).AddText(message, UIFontSizePreset.Title));
    }

    /// <summary>
    /// Shows a success toast with the given message
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="region">The region where the toast appears. Defaults to Right.</param>
    /// <param name="side">The stack side of the toast. Defaults to Top.</param>
    /// <returns>The created Toast instance.</returns>
    public static Toast Success(string message, ToastRegion region = ToastRegion.Right, ToastStackSide side = ToastStackSide.Top)
    {
        return ShowToast(new Toast(ToastType.Success, region, side).AddText(message, UIFontSizePreset.Title));
    }

    /// <summary>
    /// Shows an info toast with the given message
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="region">The region where the toast appears. Defaults to Right.</param>
    /// <param name="side">The stack side of the toast. Defaults to Top.</param>
    /// <returns>The created Toast instance.</returns>
    public static Toast Info(string message, ToastRegion region = ToastRegion.Right, ToastStackSide side = ToastStackSide.Top)
    {
        return ShowToast(new Toast(ToastType.Info, region, side).AddText(message, UIFontSizePreset.Title));
    }

    /// <summary>
    /// Shows a warning toast with the given message
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="region">The region where the toast appears. Defaults to Right.</param>
    /// <param name="side">The stack side of the toast. Defaults to Top.</param>
    /// <returns>The created Toast instance.</returns>
    public static Toast Warning(string message, ToastRegion region = ToastRegion.Right, ToastStackSide side = ToastStackSide.Top)
    {
        return ShowToast(new Toast(ToastType.Warning, region, side).AddText(message, UIFontSizePreset.Title));
    }

    /// <summary>
    /// Shows an error toast with the given message
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="region">The region where the toast appears. Defaults to Right.</param>
    /// <param name="side">The stack side of the toast. Defaults to Top.</param>
    /// <returns>The created Toast instance.</returns>
    public static Toast Error(string message, ToastRegion region = ToastRegion.Right, ToastStackSide side = ToastStackSide.Top)
    {
        return ShowToast(new Toast(ToastType.Error, region, side).AddText(message, UIFontSizePreset.Title));
    }

    /// <summary>
    /// Shows a fatal toast with the given message
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="region">The region where the toast appears. Defaults to Right.</param>
    /// <param name="side">The stack side of the toast. Defaults to Top.</param>
    /// <returns>The created Toast instance.</returns>
    public static Toast Fatal(string message, ToastRegion region = ToastRegion.Right, ToastStackSide side = ToastStackSide.Top)
    {
        return ShowToast(new Toast(ToastType.Fatal, region, side).AddText(message, UIFontSizePreset.Title));
    }

    /// <summary>
    /// Shows a highlight toast with the given message
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="region">The region where the toast appears. Defaults to Right.</param>
    /// <param name="side">The stack side of the toast. Defaults to Top.</param>
    /// <returns>The created Toast instance.</returns>
    public static Toast Highlight(string message, ToastRegion region = ToastRegion.Right, ToastStackSide side = ToastStackSide.Top)
    {
        return ShowToast(new Toast(ToastType.Highlight, region, side).AddText(message, UIFontSizePreset.Title));
    }
}
