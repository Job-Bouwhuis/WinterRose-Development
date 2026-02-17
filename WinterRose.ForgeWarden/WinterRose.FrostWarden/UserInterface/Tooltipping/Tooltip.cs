using Raylib_cs;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping;

public sealed class Tooltip : UIContainer
{
    public override InputContext Input { get; }

    public TooltipBehavior Behavior
    {
        get; internal set
        {
            field = value;
            field.Tooltip = this;
        }
    }
    public TooltipAnchor Anchor
    {
        get; internal set
        {
            field = value;
            field.Tooltip = this;
        }
    }

    public TooltipSizeConstraints SizeConstraints { get; set; } = new TooltipSizeConstraints();
    public bool IsOpen { get; internal set; }
    public bool LockOpen { get; set; }


    public Func<Tooltip, bool>? OpenOverride;
    public Func<Tooltip, TooltipCloseReason, bool>? CloseOverride;

    public Rectangle ExpandedCloseBounds { get; set; }
    public float CloseTimer { get; set; }

    public bool WasPreviouslyHoveringAnchor { get; set; } = false;
    public float OpenRequestTimer { get; set; } = 0f;
    public float CloseGraceTimer { get; set; } = 0f;

    public Tooltip(TooltipBehavior behavior, TooltipAnchor anchor)
    {
        Behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
        Anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));

        Style = new ContentStyle(new StyleBase());
        Style.AutoScale = true;
        Style.AllowUserResizing = false;
        Style.PauseAutoDismissTimer = true;
        Style.AnimateInDuration = 2f;
        Style.AnimateOutDuration = 1.6f;
        Style.MoveAndScaleCurve = Curves.EaseOutBack;
        Style.ContentAlpha = 0;
        Style.MaxAutoScaleHeight = 0;

        TargetSize = new Vector2(SizeConstraints.MinSize.X, SizeConstraints.MinSize.Y);
        AnimationElapsed = 1f;

        Input = new InputContext(new RaylibInputProvider(), 0, Behavior.AllowsInteraction);
    }

    protected override void Update()
    {
        base.Update();
        if (base.IsHovered())
            Input.IsRequestingMouseFocus = true;
        else
            Input.IsRequestingMouseFocus = false;

        Console.WriteLine(TargetSize);

        if (IsClosing)
        {
            float t = AnimationElapsedNormalized;

            TargetSize = Vector2.Zero;

            float fadeT = t / 0.1f;
            if (fadeT > 1f) fadeT = 1f;

            if (fadeT >= 1f)
            {
                Style.ShadowAll = 0;
                Style.ShowVerticalScrollBar = false;
                Style.Background = Color.Blank;
            }

            Style.ContentAlpha = 1f - fadeT;

            if (t >= 1f)
                Tooltips.ForceRemoveTooltip(this);
        }
        else
        {
            float t = AnimationElapsedNormalized;
            Style.ShowVerticalScrollBar = !IsClosing && t > 0.6f;

            float fadeT = (t - 0.6f) / 0.4f;

            if (fadeT < 0f) fadeT = 0f;
            if (fadeT > 0.99f) fadeT = 1f;

            Style.ContentAlpha = Math.Max(fadeT * 255, 255);
            if (Style.ContentAlpha < 0)
                Style.ContentAlpha = 0;

            HandleLifecycle();

        }
    }

    public override void Close()
    {
        base.Close();

        Vector2 center = new Vector2(
            CurrentPosition.X + CurrentPosition.Width * 0.5f,
            CurrentPosition.Y + CurrentPosition.Height * 0.5f
        );

        TargetSize = Vector2.Zero;
        TargetPosition = center;
        Style.AutoScale = true;
        AnimationElapsed = 0;

        IsOpen = false;
        InputManager.UnregisterContext(Input);
    }

    private void HandleLifecycle()
    {
        if (LockOpen)
            return;

        //if (base.IsHovered(true) && Behavior.AllowsInteraction)
        //    InputManager.RegisterContext(Input);
        //else
        //    InputManager.UnregisterContext(Input);


        Behavior.Update(this);
    }

    public bool IsPointInside(Rectangle r, Vector2 p)
    {
        return p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height;
    }

    internal void ComputeExpandedCloseBounds()
    {
        Rectangle baseBounds = new Rectangle(TargetPosition, TargetSize);
        float margin = 40f;

        Rectangle expanded = new Rectangle(
            baseBounds.X - margin,
            baseBounds.Y - margin,
            baseBounds.Width + margin * 2f,
            baseBounds.Height + margin * 2f
        );

        Vector2 windowSize = ForgeWardenEngine.Current.Window.Size;

        // --- phase 1: prefer moving ---
        float dx = 0f;
        float dy = 0f;

        if (expanded.X < 0f)
            dx = -expanded.X;
        else if (expanded.X + expanded.Width > windowSize.X)
            dx = windowSize.X - (expanded.X + expanded.Width);

        if (expanded.Y < 0f)
            dy = -expanded.Y;
        else if (expanded.Y + expanded.Height > windowSize.Y)
            dy = windowSize.Y - (expanded.Y + expanded.Height);

        expanded.X += dx;
        expanded.Y += dy;

        // --- phase 2: if moving wasn't enough, scale down ---
        float maxWidth = windowSize.X;
        float maxHeight = windowSize.Y;

        if (expanded.Width > maxWidth || expanded.Height > maxHeight)
        {
            float scaleX = maxWidth / expanded.Width;
            float scaleY = maxHeight / expanded.Height;
            float scale = Math.Min(scaleX, scaleY);

            Vector2 center = new Vector2(
                expanded.X + expanded.Width * 0.5f,
                expanded.Y + expanded.Height * 0.5f
            );

            expanded.Width *= scale;
            expanded.Height *= scale;

            expanded.X = center.X - expanded.Width * 0.5f;
            expanded.Y = center.Y - expanded.Height * 0.5f;

            // final clamp for safety
            expanded.X = Math.Clamp(expanded.X, 0f, windowSize.X - expanded.Width);
            expanded.Y = Math.Clamp(expanded.Y, 0f, windowSize.Y - expanded.Height);
        }

        ExpandedCloseBounds = expanded;
    }

    public override bool IsHovered()
    {
        if (!Behavior.AllowsInteraction)
            return false;

        ComputeExpandedCloseBounds();
        bool mouseOver = Input.IsMouseHovering(ExpandedCloseBounds);

        if (mouseOver)
        {
            //Console.WriteLine("tooltip is hovered by mouse");
            return true;
        }

        foreach (var c in Contents)
        {
            if (Tooltips.IsHoverExtended(c))
            {
                //Console.WriteLine("Tooltip is locked by hover extender");
                return true;
            }
        }
        //Console.WriteLine("tooltip not hovered");
        return false;
    }

    public void Show() => Tooltips.Show(this);
}