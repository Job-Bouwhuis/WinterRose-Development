using ChatThroughWinterRoseBot;
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

        float windowWidth = Application.Current.Window.Width;
        float windowHeight = Application.Current.Window.Height;

        float scaleX = windowWidth / REFERENCE_WIDTH;
        float scaleY = windowHeight / REFERENCE_HEIGHT;

        TOAST_WIDTH = 350f * scaleX;
        TOAST_HEIGHT = 170f * scaleY;

        // Setup default regions
        regions[ToastRegion.Right] = new ToastRightRegionManager();
        regions[ToastRegion.Left] = new ToastLeftRegionManager();
        regions[ToastRegion.Center] = new ToastCenterRegionManager();
    }

    public static Toast ShowToast<T>(T toast) where T : UIContainer
    {
        Forge.Expect(toast).OfType<Toast>();
        Toast t = toast as Toast;
        regions[t.Region].EnqueueToast(t);
        return t;
    }

    internal static void RequestReorder()
    {
        requestReorder = true;
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

        if (anyHovered && Input.HighestPriorityMouseAbove == null)
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
        return ShowToast(new Toast(ToastType.Neutral, region, side).AddText(message));
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
        return ShowToast(new Toast(ToastType.Success, region, side).AddText(message));
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
        return ShowToast(new Toast(ToastType.Info, region, side).AddText(message));
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
        return ShowToast(new Toast(ToastType.Warning, region, side).AddText(message));
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
        return ShowToast(new Toast(ToastType.Error, region, side).AddText(message));
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
        return ShowToast(new Toast(ToastType.Fatal, region, side).AddText(message));
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
        return ShowToast(new Toast(ToastType.Highlight, region, side).AddText(message));
    }
}


//public static class Toasts
//{
//    private static List<Toast> activeToasts = new();
//    private static Queue<Toast> queuedToasts = new();

//    internal const float TOAST_SPACING = 8f;
//    internal static readonly float TOAST_WIDTH;
//    internal static readonly float TOAST_HEIGHT;
//    internal const float TOAST_CONTENT_PADDING = 10;

//    static Toasts()
//    {
//        // Baseline reference resolution
//        const float REFERENCE_WIDTH = 1920f;
//        const float REFERENCE_HEIGHT = 1080f;

//        // Current window size (replace with your actual window size getters)
//        float windowWidth = Application.Current.Window.Width;
//        float windowHeight = Application.Current.Window.Height;

//        // Scale factors relative to the reference resolution
//        float scaleX = windowWidth / REFERENCE_WIDTH;
//        float scaleY = windowHeight / REFERENCE_HEIGHT;

//        // Scale toast size based on resolution
//        TOAST_WIDTH = 350f * scaleX;
//        TOAST_HEIGHT = 170f * scaleY;
//    }

//    public static void ShowToast(Toast toast)
//    {
//        queuedToasts.Enqueue(toast);
//    }

//    public static void Update(float deltaTime)
//    {
//        if (ray.IsWindowResized())
//            RecalculatePositions();

//        // Activate queued toasts if space available
//        int queueCount = queuedToasts.Count;

//        for (int i = 0; i < queueCount; i++)
//        {
//            Toast next = queuedToasts.Dequeue();

//            // Measure total height if this toast is added
//            float totalHeight = TOAST_SPACING; // bottom spacing
//            for (int j = activeToasts.Count - 1; j >= 0; j--)
//            {
//                totalHeight += activeToasts[j].Height + TOAST_SPACING;
//            }
//            totalHeight += next.Height; // include the candidate

//            // Check if it would go off screen
//            if (totalHeight > Application.Current.Window.Height)
//            {
//                // Put it back at the *end* of the queue
//                queuedToasts.Enqueue(next);
//                continue; // evaluate the rest this frame
//            }

//            activeToasts.Add(next);
//            PositionToast(next);
//        }

//        // Update active toasts
//        for (int i = activeToasts.Count - 1; i >= 0; i--)
//        {
//            Toast t = activeToasts[i];

//            // Animate towards target
//            t.AnimationElapsed += deltaTime;
//            float tNormalized = Math.Clamp(t.AnimationElapsed / t.AnimationDuration, 0f, 1f);
//            t.CurrentPosition.Position = Vector2.Lerp(t.CurrentPosition.Position, t.TargetPosition, Curves.EaseOutBackFar.Evaluate(tNormalized));

//            // Check if closing
//            if (t.IsClosing)
//            {
//                if(tNormalized >= 1f)
//                {
//                    activeToasts.Remove(t);
//                    RecalculatePositions();
//                }
//                continue;
//            }

//            t.UpdateBase();
//        }
//    }

//    private static void PositionToast(Toast toast)
//    {
//        float startY = Application.Current.Window.Height - toast.Height - TOAST_SPACING;

//        toast.CurrentPosition = new Rectangle(
//            Application.Current.Window.Width, startY,
//            TOAST_WIDTH, toast.Height);

//        toast.TargetPosition = new Vector2(
//            Application.Current.Window.Width - TOAST_WIDTH - TOAST_SPACING, startY);

//        toast.AnimationElapsed = 0f;

//        RecalculatePositions(toast);
//    }

//    private static void RecalculatePositions(params Toast[] toSkip)
//    {
//        float cursorY = Application.Current.Window.Height - TOAST_SPACING;

//        // Newest → Oldest (bottom → top)
//        for (int i = activeToasts.Count - 1; i >= 0; i--)
//        {
//            Toast toast = activeToasts[i];

//            cursorY -= toast.Height;

//            if (!toast.IsClosing && !toSkip.Contains(toast))
//            {
//                float newY = cursorY;

//                if (newY != toast.TargetPosition.Y)
//                {
//                    toast.TargetPosition = new Vector2(toast.TargetPosition.X, newY);
//                    toast.AnimationElapsed = 0f;
//                }
//            }

//            // Add spacing after this toast
//            cursorY -= TOAST_SPACING;
//        }
//    }


//    public static void DismissToast(Toast toast)
//    {
//        toast.Close();
//    }

//    internal static void Draw()
//    {
//        foreach (var toast in activeToasts)
//        {
//            toast.DrawBase();

//            //ray.DrawRectangleRec(toast.CurrentPosition, new Color(255, 255, 255, 120));
//            //ray.DrawRectangleLinesEx(toast.CurrentPosition, 4, new Color(255, 255, 255, 255));

//            //RichTextRenderer.DrawRichText(
//            //    toast.Message,
//            //    new Vector2(
//            //        toast.CurrentPosition.X + TOAST_CONTENT_PADDING,
//            //        toast.CurrentPosition.Y + TOAST_CONTENT_PADDING),
//            //    TOAST_WIDTH - TOAST_CONTENT_PADDING * 2);
//        }
//    }

//    internal static void RemoveImmediately(Toast toast)
//    {
//        activeToasts.Remove(toast);
//        RecalculatePositions();
//    }
//}
