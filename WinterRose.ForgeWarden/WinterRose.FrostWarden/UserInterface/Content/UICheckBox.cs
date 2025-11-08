using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.UserInterface.Content;
public class UICheckBox : UIContent
{
    // Visual / layout constants
    private const int CHECK_SIZE = 16;
    private const int PADDING_X = 8;
    private const int PADDING_Y = 4;
    private const int SPACING = 8;
    private const float ANIMATION_SPEED = 12f; // higher = snappier

    public RichText Text { get; set; }

    public bool ReadOnly { get; set; }

    // State
    private bool checkedState;
    public bool Checked
    {
        get => checkedState;
        set
        {
            if (ReadOnly)
                return;
            if (checkedState == value)
                return;
            checkedState = value;
            // set animation target
            targetProgress = checkedState ? 1f : 0f;

            // invoke multicast handlers
            OnCheckedChanged?.Invoke(owner, this, checkedState);
            OnCheckedChangedBasic?.Invoke(checkedState);
        }
    }

    public void SetCheckedNoEvent(bool value)
    {
        if (ReadOnly)
            return;
        if (checkedState == value) 
            return;
        checkedState = value;
        checkedState = value;
        targetProgress = checkedState ? 1f : 0f;
    }

    // Invocation hooks
    public MulticastVoidInvocation<UIContainer, UICheckBox, bool> OnCheckedChanged { get; set; } = new();
    public MulticastVoidInvocation<bool> OnCheckedChangedBasic { get; set; } = new();

    // Internal animation progress 0..1 (unchecked->checked)
    private float animationProgress = 0f;
    private float targetProgress = 0f;

    // Background for hover/click visuals
    private Color backgroundColor;

    // Constructors
    public UICheckBox(RichText text, VoidInvocation<bool>? onChanged, bool initial = false)
        : this(text, (VoidInvocation<UIContainer, UICheckBox, bool>?)null, initial)
    {
        if (onChanged != null)
            OnCheckedChangedBasic.Subscribe(onChanged);
    }

    public UICheckBox() : this("New Checkbox")
    {
    }

    public UICheckBox(RichText text, VoidInvocation<UIContainer, UICheckBox, bool>? onChanged = null, bool initial = false)
    {
        Text = text;
        checkedState = initial;
        targetProgress = checkedState ? 1f : 0f;
        if (onChanged != null)
            OnCheckedChanged.Subscribe(onChanged);
    }

    // Size / layout
    public override Vector2 GetSize(Rectangle availableArea) => CalculateSize(availableArea.Width).Size;
    protected internal override float GetHeight(float maxWidth) => GetHeight(maxWidth, Style.BaseButtonFontSize);

    public virtual Rectangle CalculateSize(float maxWidth, float baseFontScale = 1f)
    {
        int labelBaseSize = 12;
        int labelFontSize = (int)(labelBaseSize * baseFontScale);
        labelFontSize = Math.Clamp(labelFontSize, 12, 28);
        Text.FontSize = labelFontSize;

        // available width for text is maxWidth minus checkbox and paddings
        float textMax = Math.Max(0f, maxWidth - (PADDING_X * 2 + CHECK_SIZE + SPACING));
        Rectangle textSize = Text.CalculateBounds(textMax);

        int w = (int)(PADDING_X * 2 + CHECK_SIZE + SPACING + textSize.Width);
        int h = Math.Max(CHECK_SIZE + PADDING_Y * 2, (int)textSize.Height + PADDING_Y * 2);

        return new Rectangle(0, 0, w, h);
    }

    protected internal virtual float GetHeight(float maxWidth, float baseFontScale = 1f)
    {
        return CalculateSize(maxWidth, baseFontScale).Height;
    }

    // Interaction
    protected internal override void Update()
    {
        // hover / pressed visuals (reuse Style like UIButton)
        if (IsHovered && Input.IsDown(MouseButton.Left))
            backgroundColor = Style.ButtonClick;
        else if (IsHovered)
            backgroundColor = Style.ButtonHover;
        else
            backgroundColor = Style.ButtonBackground;

        // animate progress toward target
        float dt = Raylib_cs.Raylib.GetFrameTime();
        if (animationProgress < targetProgress)
        {
            animationProgress = MathF.Min(targetProgress, animationProgress + ANIMATION_SPEED * dt);
        }
        else if (animationProgress > targetProgress)
        {
            animationProgress = MathF.Max(targetProgress, animationProgress - ANIMATION_SPEED * dt);
        }
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        // toggle state on click
        Checked = !Checked;
    }

    // Drawing
    protected override void Draw(Rectangle bounds)
    {
        // draw background for the row
        ray.DrawRectangleRec(bounds, backgroundColor);
        ray.DrawRectangleLinesEx(bounds, 1, Style.ButtonBorder);

        // compute checkbox rect
        float boxX = bounds.X + PADDING_X;
        float boxY = bounds.Y + (bounds.Height - CHECK_SIZE) / 2f;
        var boxRect = new Rectangle(boxX, boxY, CHECK_SIZE, CHECK_SIZE);

        // draw checkbox border / background
        ray.DrawRectangleRec(boxRect, Style.ButtonBackground);
        ray.DrawRectangleLinesEx(boxRect, 1, Style.ButtonBorder);

        // draw the inner filled box based on animationProgress
        // inner rect will scale slightly and fade in
        float inset = 3f * (1f - animationProgress); // when progress=1 inset=0, when 0 inset=3
        var innerRect = new Rectangle(
            boxRect.X + inset,
            boxRect.Y + inset,
            MathF.Max(0f, boxRect.Width - inset * 2f),
            MathF.Max(0f, boxRect.Height - inset * 2f)
        );

        // fill color for the inner rect (respect content alpha)
        var fillColor = Style.ButtonBorder;
        byte fillAlpha = (byte)(fillColor.A * Math.Clamp(animationProgress, 0f, 1f));
        var innerFill = new Color(fillColor.R, fillColor.G, fillColor.B, fillAlpha);
        ray.DrawRectangleRec(innerRect, innerFill);

        // Draw a visible checkmark on top (white with animated alpha)
        if (animationProgress > 0f && innerRect.Width > 2f && innerRect.Height > 2f)
        {
            // Checkmark progress: first segment draws from 0..0.6, second from 0.4..1.0 (overlap for nicer look)
            float p = animationProgress;
            float seg1End = MathF.Min(1f, p / 0.6f);               // 0..1 for first segment
            float seg2Start = MathF.Max(0f, (p - 0.4f) / 0.6f);    // 0..1 for second segment

            // Coordinates relative to innerRect
            float left = innerRect.X;
            float top = innerRect.Y;
            float w = innerRect.Width;
            float h = innerRect.Height;

            // Points for a classic check shape (A -> B -> C)
            var ax = left + w * 0.18f;
            var ay = top + h * 0.53f;
            var bx = left + w * 0.42f;
            var by = top + h * 0.77f;
            var cx = left + w * 0.86f;
            var cy = top + h * 0.25f;

            // Colors: white (or tinted by content alpha)
            int alphaByte = (int)(255 * Math.Clamp(animationProgress, 0f, 1f));
            var checkColor = new Color(255, 255, 255, alphaByte);

            float thickness = MathF.Max(1.5f, 2.0f * (0.6f + 0.4f * animationProgress));

            // Draw first segment (A -> interp(A,B,seg1End))
            if (seg1End > 0f)
            {
                var midX = ax + (bx - ax) * seg1End;
                var midY = ay + (by - ay) * seg1End;
                Raylib_cs.Raylib.DrawLineEx(new Vector2(ax, ay), new Vector2(midX, midY), thickness, checkColor);
            }

            // Draw second segment (B -> interp(B,C,seg2Start))
            if (seg2Start > 0f)
            {
                var endX = bx + (cx - bx) * seg2Start;
                var endY = by + (cy - by) * seg2Start;
                Raylib_cs.Raylib.DrawLineEx(new Vector2(bx, by), new Vector2(endX, endY), thickness, checkColor);
            }
        }

        // draw label text to the right of the checkbox
        float textX = boxX + CHECK_SIZE + SPACING;
        float textY = bounds.Y + (bounds.Height - Text.FontSize) / 2f;
        RichTextRenderer.DrawRichText(
            Text,
            new Vector2(textX, textY),
            bounds.Width - (textX - bounds.X),
            null);
    }

}
