using PuppeteerSharp;
using Raylib_cs;
using System.ComponentModel;
using WinterRose.ForgeWarden.DialogBoxes;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.ToastNotifications;

public class Toast
{
    public ToastType Type;
    internal Rectangle CurrentPosition;
    internal Vector2 TargetPosition;
    internal Vector2 CurrentScale;
    internal Vector2 TargetScale;
    internal float AnimationElapsed;
    internal float AnimationDuration = 0.4f;
    internal bool IsClosing { get; private set; }
    protected internal bool IsMorphDrawing { get; internal set; }
    internal float Progress;
    internal float TimeShown;

    /// <summary>
    /// The time in seconds before the toast will automatically close
    /// </summary>
    public float TimeUntilAutoDismiss { get; set; } = 5f;

    protected internal ToastRegionManager ToastManager { get; internal set; }

    private float hoverElapsed = 0f;
    public Curve HoverCurve { get; set; } = Curves.EaseOutBack;
    public float HoverDuration { get; set; } = 0.2f;
    private bool isHoverTarget = false;
    private Func<Toast, int> continueWithSelector;
    private Toast[] continueWithOptions;

    public ToastStyle Style { get; set; }
    public ToastStackSide StackSide { get; internal set; }
    public ToastRegion Region { get; internal set; }
    internal List<ToastContent> Contents { get; } = new();

    public float Height => Contents.Sum(c => c.GetHeight(Toasts.TOAST_WIDTH))
                           + Toasts.TOAST_CONTENT_PADDING * (Contents.Count + 1);

    public Toast(ToastType type, ToastRegion region = ToastRegion.Right, ToastStackSide stackSide = ToastStackSide.Bottom)
    {
        Type = type;
        StackSide = stackSide;
        Style = new(type);
        Region = region;
    }

    public Toast(ToastType type, ToastStyle style, ToastRegion region = ToastRegion.Right, ToastStackSide stackSide = ToastStackSide.Bottom)
    {
        Type = type;
        StackSide = stackSide;
        Style = style;
        Region = region;
    }

    public Toast AddContent(ToastContent content)
    {
        Contents.Add(content);
        content.owner = this;
        return this;
    }

    public Toast AddContent(string text, ToastMessageFontPreset preset = ToastMessageFontPreset.Message)
        => AddContent(RichText.Parse(text, Color.White), preset);
    public Toast AddContent(RichText text, ToastMessageFontPreset preset = ToastMessageFontPreset.Message)
        => AddContent(new ToastMessageContent(text, preset));

    /// <summary>
    /// Adds a button to the toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick">Should return true when the toast should close, false if not</param>
    /// <returns></returns>
    public Toast AddButton(string text, Func<Toast, ToastButton, bool>? onClick) => AddButton(RichText.Parse(text, Color.White), onClick);

    public Toast AddButton(RichText text, Func<Toast, ToastButton, bool>? onClick = null)
    {
        ToastButtonContent? btns = null;
        foreach(var item in Contents)
        {
            if(item is ToastButtonContent b)
            {
                btns = b;
                break;
            }
        }

        if(btns is null)
        {
            btns = new();
            AddContent(btns);
        }

        btns.AddButton(text, onClick);
        return this;
    }

    /// <summary>
    /// Adds a progress bar to the toast
    /// </summary>
    /// <param name="initialProgress">The progress in a 0-1 range. set to -1 to have it do a infinite working animation</param>
    /// <param name="ProgressProvider">The function that provides further values</param>
    /// <param name="closesToastWhenComplete">When true, and the progress becomes 1, it requests the toast to close.</param>
    /// <returns></returns>
    public Toast AddProgressBar(float initialProgress, Func<float, float>? ProgressProvider = null, bool closesToastWhenComplete = true, string infiniteSpinText = "Working...")
    {
        return AddContent(new ToastProgressContent(initialProgress, ProgressProvider, closesToastWhenComplete, infiniteSpinText));
    }

    /// <summary>
    /// Adds the sprite to the dialog
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public Toast AddSprite(Sprite sprite) => AddContent(new ToastSpriteContent(sprite));

    internal void UpdateBase()
    {
        float contentOffsetY = CurrentPosition.Y + Toasts.TOAST_CONTENT_PADDING;

        foreach (var content in Contents)
        {
            float contentHeight = content.GetHeight(Toasts.TOAST_WIDTH);
            Rectangle contentBounds = new Rectangle(
                CurrentPosition.X + Toasts.TOAST_CONTENT_PADDING,
                contentOffsetY,
                CurrentPosition.Width - Toasts.TOAST_CONTENT_PADDING * 2,
                contentHeight
            );

            if (content.IsHovered(contentBounds))
                content.OnHover();

            content.Update();

            contentOffsetY += contentHeight + Toasts.TOAST_CONTENT_PADDING;
        }

        if (TimeUntilAutoDismiss > 0 && !Contents.Any(c => c.IsHovered(new Rectangle(CurrentPosition.X, CurrentPosition.Y, CurrentPosition.Width, Height))))
        {
            TimeShown += Time.deltaTime;

            if (TimeShown >= TimeUntilAutoDismiss && !IsClosing)
                Close();
        }
    }

    internal void DrawBase()
    {
        bool isHovered = IsHovered();

        // Set hover target
        if (isHovered != isHoverTarget)
        {
            isHoverTarget = isHovered;
            hoverElapsed = 0f; // restart animation toward new target
        }

        // Animate hover
        if (hoverElapsed < HoverDuration)
            hoverElapsed += Time.deltaTime;

        float tNormalized = Math.Clamp(hoverElapsed / HoverDuration, 0f, 1f);
        float easedProgress = HoverCurve.Evaluate(isHoverTarget ? tNormalized : 1f - tNormalized);

        float hoverOffsetX = -4f * easedProgress;
        float hoverOffsetY = -4f * easedProgress;
        float shadowOffsetX = 4f * easedProgress;
        float shadowOffsetY = 4f * easedProgress;

        // Shadow
        ray.DrawRectangleRec(new Rectangle(
            CurrentPosition.X - Style.ShadowSizeLeft + shadowOffsetX,
            CurrentPosition.Y - Style.ShadowSizeTop + shadowOffsetY,
            CurrentPosition.Width + Style.ShadowSizeLeft + Style.ShadowSizeRight,
            CurrentPosition.Height + Style.ShadowSizeTop + Style.ShadowSizeBottom),
            Style.Shadow
        );

        // Background
        var backgroundBounds = new Rectangle(
            CurrentPosition.X + hoverOffsetX,
            CurrentPosition.Y + hoverOffsetY,
            CurrentPosition.Width,
            CurrentPosition.Height
        );

        // Adjust content alpha based on size
        float minSize = Math.Min(CurrentPosition.Width, CurrentPosition.Height);

        // Only fade when below 50
        if (minSize >= 50f)
        {
            Style.contentAlpha = 1f;
        }
        else if (minSize <= 20f)
        {
            Style.contentAlpha = 0f;
        }
        else
        {
            // Linear fade from 50 -> 20
            Style.contentAlpha = (minSize - 20f) / (50f - 20f);
        }

        ray.DrawRectangleRec(backgroundBounds, Style.Background);
        ray.DrawRectangleLinesEx(backgroundBounds, 2, Style.Border);

        if (TimeUntilAutoDismiss > 0)
            DrawCloseTimerBar(backgroundBounds);

        Rectangle contentArea = new Rectangle(
            backgroundBounds.X + Toasts.TOAST_CONTENT_PADDING,
            backgroundBounds.Y + Toasts.TOAST_CONTENT_PADDING,
            backgroundBounds.Width - Toasts.TOAST_CONTENT_PADDING * 2,
            backgroundBounds.Height - Toasts.TOAST_CONTENT_PADDING * 2
        );
        DrawContent(contentArea);
    }

    internal void DrawContent(Rectangle contentArea)
    {
        float offsetY = contentArea.Y;

        foreach (var content in Contents)
        {
            float contentHeight = content.GetHeight(contentArea.Width);
            Rectangle bounds = new Rectangle(contentArea.X, offsetY, contentArea.Width, contentHeight);
            content.Draw(bounds, Style.contentAlpha);
            offsetY += contentHeight + Toasts.TOAST_CONTENT_PADDING;
        }
    }

    private void DrawCloseTimerBar(Rectangle bounds)
    {
        float progressRatio = Math.Clamp(TimeShown / TimeUntilAutoDismiss, 0f, 1f);
        float barHeight = 3f; // adjustable height
        float yPos = bounds.Y + (bounds.Height - 2) - barHeight;

        // Background
        ray.DrawRectangle(
            (int)bounds.X + 2,
            (int)yPos,
            (int)bounds.Width - 4,
            (int)barHeight,
            Style.TimerBarBackground
        );

        // Fill
        ray.DrawRectangle(
            (int)bounds.X + 2,
            (int)yPos,
            (int)((bounds.Width - 4) * progressRatio),
            (int)barHeight,
            Style.TimerBarFill
        );
    }

    public bool IsHovered()
    {
        var dialogsOcupied = Dialogs.GetActiveDialogs().Select(dialog => Dialogs.GetDialogBounds(dialog.Placement));
        Vector2 mousePos = ray.GetMousePosition();
        bool isHovered = ray.CheckCollisionPointRec(mousePos, CurrentPosition);
        if (!isHovered) return false;
        if (dialogsOcupied.Any(rect => ray.CheckCollisionRecs(rect, CurrentPosition)))
            return false;
        return isHovered;
    }

    public void Close()
    {
        if (!IsClosing)
        {
            IsClosing = true;
            isHoverTarget = true;
            AnimationElapsed = 0f;

            foreach(var c in Contents)
            {
                c.OnToastClosing();
            }

            if (ToastManager != null)
            {
                Rectangle exitTarget = ToastManager.GetExitPositionAndScale(this);
                TargetPosition = exitTarget.Position;
                TargetScale = exitTarget.Size;
            }
            else
                TargetScale = new();
        }
    }

    public void OpenAsDialog(DialogBoxes.Dialog d, float morphDuration = 0.5f, float toastFadeStart = 0.05f, float toastFadeEnd = 0.4f, float dialogFadeStart = 0.55f)
    {
        ToastToDialogMorpher.TryStartMorph(this, d, morphDuration, toastFadeStart, toastFadeEnd, dialogFadeStart);
    }

    /// <summary>
    /// Call this to have one and only one continuation toast
    /// </summary>
    /// <param name="toast"></param>
    public void ContinueWith(Toast toast)
    {
        ContinueWith(t => 0, toast);
    }

    /// <summary>
    /// Call this to have multiple continuations based based on some condition evaluated when the continuation should happen
    /// </summary>
    /// <param name="continueWithSelector"></param>
    /// <param name="options"></param>
    public void ContinueWith(Func<Toast, int> continueWithSelector, params Toast[] options)
    {
        this.continueWithSelector = continueWithSelector;
        continueWithOptions = options;
    }

    internal Toast? GetContinueWithToast()
    {
        if (continueWithSelector == null || continueWithOptions == null || continueWithOptions.Length == 0)
            return null;

        int index;
        try
        {
            index = continueWithSelector(this);
        }
        catch
        {
            return null;
        }

        if (index < 0 || index >= continueWithOptions.Length)
            return null;

        return continueWithOptions[index];
    }

}



