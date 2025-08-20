using Raylib_cs;
using WinterRose.ForgeWarden.DialogBoxes;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.ToastNotifications;

public class Toast
{
    public RichText Message;
    public ToastType Type;
    public Rectangle CurrentPosition;
    public Vector2 TargetPosition;
    internal float AnimationElapsed;
    public float AnimationDuration = 0.4f;
    internal bool IsClosing { get; private set; }
    public float Progress;
    public float TimeUntilAutoDismiss = 5f;
    internal float TimeShown;


    private float hoverElapsed = 0f;
    public Curve HoverCurve { get; set; } = Curves.EaseOutBack;
    public float HoverDuration { get; set; } = 0.2f;
    private bool isHoverTarget = false;

    public float Height { get; }

    public ToastStyle Style { get; set; } = new ToastStyle();

    public Toast(RichText message, ToastType type, float? customHeight = null)
    {
        Message = message;
        Type = type;
        Height = customHeight ?? Toasts.TOAST_HEIGHT;

        float messageScale = Toasts.TOAST_WIDTH * 0.06f;
        message.FontSize = (int)Math.Clamp(messageScale, 8, 24);
    }

    /// <summary>
    /// Override this method to implement click behavior.
    /// </summary>
    public virtual void OnClick(MouseButton button)
    {
        var d = new DefaultDialog("Test Morph", "this is an awesome morphing toast to dialog", DialogPlacement.HorizontalBig);
        d.Buttons.Add(new DialogButton("Close"));
        ToastToDialogMorpher.TryStartMorph(this, d);
    }

    /// <summary>
    /// Override this method to implement hover behavior.
    /// </summary>
    public virtual void OnHover() 
    {
        if(ray.IsMouseButtonReleased(MouseButton.Left))
        {
            OnClick(MouseButton.Left);
        }
        if (ray.IsMouseButtonReleased(MouseButton.Right))
        {
            OnClick(MouseButton.Right);
        }
        if (ray.IsMouseButtonReleased(MouseButton.Middle))
        {
            OnClick(MouseButton.Middle);
        }
        if (ray.IsMouseButtonReleased(MouseButton.Forward))
        {
            OnClick(MouseButton.Forward);
        }
        if (ray.IsMouseButtonReleased(MouseButton.Back))
        {
            OnClick(MouseButton.Back);
        }
    }

    internal void UpdateBase()
    {
        Vector2 mousePos = ray.GetMousePosition();
        bool isHovered = ray.CheckCollisionPointRec(mousePos, CurrentPosition);

        if (!isHovered)
            TimeShown += Time.deltaTime;
        else
            OnHover();

        if (TimeShown >= TimeUntilAutoDismiss && !IsClosing)
        {
            Close();
        }

        Update();
    }

    /// <summary>
    /// For toast-specific logic.
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// For drawing toast-specific content.
    /// The contentArea defines the inner rectangle inside the toast.
    /// </summary>
    public virtual void Draw(Rectangle contentArea) { }

    internal void DrawBase()
    {
        Vector2 mousePos = ray.GetMousePosition();
        bool isHovered = ray.CheckCollisionPointRec(mousePos, CurrentPosition);

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

        // Apply easing
        float easedProgress = HoverCurve.Evaluate(isHoverTarget ? tNormalized : 1f - tNormalized);

        // Compute offsets
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
        ray.DrawRectangleRec(backgroundBounds, Style.Background);

        // Border
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

    /// <summary>
    /// Close the toast.
    /// </summary>
    public void Close()
    {
        if (!IsClosing)
        {
            IsClosing = true;
            isHoverTarget = true;
            AnimationElapsed = 0f;

            TargetPosition = TargetPosition with
            {
                X = Application.Current.Window.Width
            };
        }
    }

    internal void DrawContent(Rectangle contentArea)
    {
        RichTextRenderer.DrawRichText(Message, contentArea.Position, contentArea.Width, new Color(255, 255, 255, Style.contentAlpha));

        // Call subclass draw
        Draw(contentArea);
    }
}


