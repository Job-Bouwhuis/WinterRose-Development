using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;
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

    public bool Disabled { get; set; } = false;

    private const float BUSY_SPEED = 2.0f;            // cycles per second (back and forth)
    private const float BUSY_SPHERE_RATIO = 0.30f;    // fraction of inner rect used for sphere diameter

    public bool IndicateBusy { get; set; } = false;
    private float busyPhase = 0f; // advances over time; ping-ponged in Update()

    // Modify Checked setter to respect Disabled
    public bool Checked
    {
        get => checkedState;
        set
        {
            if (Disabled || IndicateBusy)   // block while disabled or busy
                return;
            if (checkedState == value)
                return;
            checkedState = value;
            targetProgress = checkedState ? 1f : 0f;

            OnCheckedChanged?.Invoke(Owner, this, checkedState);
            OnCheckedChangedBasic?.Invoke(checkedState);
        }
    }

    private bool checkedState;

    public void SetChecked(bool value, bool events = false)
    {
        if (Disabled || IndicateBusy)
            return;
        if (checkedState == value)
            return;
        checkedState = value;
        targetProgress = checkedState ? 1f : 0f;

        if (events)
        {
            OnCheckedChanged?.Invoke(Owner, this, checkedState);
            OnCheckedChangedBasic?.Invoke(checkedState);
        }
    }

    /// <summary>
    /// Does not care whether the checkbox is disabled or not to set the checked state
    /// </summary>
    /// <param name="value"></param>
    /// <param name="events"></param>
    public void ForceSetChecked(bool value, bool events = false)
    {
        if (checkedState == value)
            return;
        checkedState = value;
        targetProgress = checkedState ? 1f : 0f;

        if(events)
        {
            OnCheckedChanged?.Invoke(Owner, this, checkedState);
            OnCheckedChangedBasic?.Invoke(checkedState);
        }
    }

    // Invocation hooks
    public MulticastVoidInvocation<IUIContainer, UICheckBox, bool> OnCheckedChanged { get; set; } = new();
    public MulticastVoidInvocation<bool> OnCheckedChangedBasic { get; set; } = new();

    // Internal animation progress 0..1 (unchecked->checked)
    private float animationProgress = 0f;
    private float targetProgress = 0f;

    // Background for hover/click visuals
    private Color backgroundColor;

    // Constructors
    public UICheckBox(RichText text, VoidInvocation<bool>? onChanged, bool initial = false)
        : this(text, (VoidInvocation<IUIContainer, UICheckBox, bool>?)null, initial)
    {
        if (onChanged != null)
            OnCheckedChangedBasic.Subscribe(onChanged);
    }

    public UICheckBox() : this("New Checkbox")
    {
    }

    public UICheckBox(RichText text, VoidInvocation<IUIContainer, UICheckBox, bool>? onChanged = null, bool initial = false)
    {
        Text = text;
        checkedState = initial;
        targetProgress = checkedState ? 1f : 0f;
        if (onChanged != null)
            OnCheckedChanged.Subscribe(onChanged);
    }

    // Size / layout
    public override Vector2 GetSize(Rectangle availableArea)
    {
        return CalculateSize(availableArea.Width).Size;
    }

    protected internal override float GetHeight(float maxWidth)
    {
        return CalculateSize(maxWidth).Height;
    }

    public virtual Rectangle CalculateSize(float maxWidth)
    {
        int baseFontSize = Style.BaseButtonFontSize;
        float textMax = Math.Max(0f, maxWidth - (PADDING_X * 2 + CHECK_SIZE + SPACING));

        int resolvedFontSize = UITextScalar.ResolveFontSize(
            Text,
            baseFontSize,
            new Rectangle(0, 0, textMax, float.MaxValue),
            autoScale: true
        );

        Rectangle textSize = UITextScalar.Measure(
            Text,
            resolvedFontSize,
            textMax
        );

        int w = (int)(PADDING_X * 2 + CHECK_SIZE + SPACING + textSize.Width);
        int h = Math.Max(
            CHECK_SIZE + PADDING_Y * 2,
            (int)textSize.Height + PADDING_Y * 2
        );

        return new Rectangle(0, 0, w, h);
    }

    protected override void Update()
    {
        float dt = Raylib_cs.Raylib.GetFrameTime();

        if (Disabled)
        {
            backgroundColor = Style.ButtonDisabled;
        }
        else if (IsHovered && Input.IsDown(MouseButton.Left))
        {
            backgroundColor = Style.ButtonClick;
        }
        else if (IsHovered)
        {
            backgroundColor = Style.ButtonHover;
        }
        else
        {
            backgroundColor = Style.ButtonBackground;
        }

        // animate checkbox checked progress
        if (animationProgress < targetProgress)
        {
            animationProgress = MathF.Min(targetProgress, animationProgress + ANIMATION_SPEED * dt);
        }
        else if (animationProgress > targetProgress)
        {
            animationProgress = MathF.Max(targetProgress, animationProgress - ANIMATION_SPEED * dt);
        }

        // advance busy phase if indicating busy
        if (IndicateBusy)
        {
            busyPhase += dt * BUSY_SPEED;
            // keep busyPhase in reasonable range
            if (busyPhase > 10000f) busyPhase %= 2f;
        }
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        if (Disabled || IndicateBusy) return;  // ignore clicks when disabled or busy
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
        float inset = 3f * (1f - animationProgress);
        var innerRect = new Rectangle(
            boxRect.X + inset,
            boxRect.Y + inset,
            MathF.Max(0f, boxRect.Width - inset * 2f),
            MathF.Max(0f, boxRect.Height - inset * 2f)
        );

        // fill color for the inner rect: show a visible background when indicating busy even if unchecked
        Color fillColor = Style.ButtonBorder ?? Style.White;
        float innerAlphaF;
        if (IndicateBusy)
        {
            // keep it visible while busy (but semi-transparent)
            innerAlphaF = MathF.Min(220f, fillColor.A * 0.65f);
        }
        else
        {
            innerAlphaF = fillColor.A * Math.Clamp(animationProgress, 0f, 1f);
        }
        byte innerAlpha = (byte)Math.Clamp(innerAlphaF, 0f, 255f);
        var innerFill = new Color(fillColor.R, fillColor.G, fillColor.B, innerAlpha);
        ray.DrawRectangleRec(innerRect, innerFill);

        // If IndicateBusy: draw the moving sphere instead of the checkmark
        if (IndicateBusy && innerRect.Width > 2f && innerRect.Height > 2f)
        {
            // ping-pong t in range 0..1
            float t = busyPhase % 2f;
            if (t > 1f) t = 2f - t;

            // apply quintic ease-in-out for desired speed profile
            float eased = EvaluateBusyEased(t);

            // sphere size and position (shrink travel bounds a bit so the ball doesn't touch edges)
            float diameter = MathF.Min(innerRect.Width, innerRect.Height) * BUSY_SPHERE_RATIO;
            float radius = diameter / 2f;

            // margin inside innerRect to avoid touching edges
            float margin = MathF.Max(2f, radius * 0.8f);

            float travelRange = Math.Max(0f, innerRect.Width - diameter - margin * 2f);
            float cx = innerRect.X + margin + eased * travelRange + radius;
            float cy = innerRect.Y + innerRect.Height / 2f;

            // color (dim if Disabled)
            Color sphereColor = Style.CheckBoxBusyIndicator;

            Raylib_cs.Raylib.DrawCircleV(new Vector2(cx, cy), radius, sphereColor);
        }
        else
        {
            // Draw a visible checkmark on top (white with animated alpha)
            if (animationProgress > 0f && innerRect.Width > 2f && innerRect.Height > 2f)
            {
                int alphaByte = Disabled ? 100 : (int)(255 * Math.Clamp(animationProgress, 0f, 1f));
                var checkColor = new Color(255, 255, 255, alphaByte);

                float p = animationProgress;
                float seg1End = MathF.Min(1f, p / 0.6f);
                float seg2Start = MathF.Max(0f, (p - 0.4f) / 0.6f);

                float left = innerRect.X;
                float top = innerRect.Y;
                float w = innerRect.Width;
                float h = innerRect.Height;

                var ax = left + w * 0.18f;
                var ay = top + h * 0.53f;
                var bx = left + w * 0.42f;
                var by = top + h * 0.77f;
                var cx = left + w * 0.86f;
                var cy = top + h * 0.25f;

                float thickness = MathF.Max(1.5f, 2.0f * (0.6f + 0.4f * animationProgress));

                if (seg1End > 0f)
                {
                    var midX = ax + (bx - ax) * seg1End;
                    var midY = ay + (by - ay) * seg1End;
                    Raylib_cs.Raylib.DrawLineEx(new Vector2(ax, ay), new Vector2(midX, midY), thickness, checkColor);
                }
                if (seg2Start > 0f)
                {
                    var endX = bx + (cx - bx) * seg2Start;
                    var endY = by + (cy - by) * seg2Start;
                    Raylib_cs.Raylib.DrawLineEx(new Vector2(bx, by), new Vector2(endX, endY), thickness, checkColor);
                }
            }
        }

        // draw label text to the right of the checkbox
        float textX = boxX + CHECK_SIZE + SPACING;

        int baseFontSize = Style.BaseButtonFontSize;

        Text.FontSize = UITextScalar.ResolveFontSize(
            Text,
            baseFontSize,
            new Rectangle(
                0,
                0,
                bounds.Width - (textX - bounds.X) - (PADDING_X * 2 + CHECK_SIZE + SPACING),
                bounds.Height
            ),
            autoScale: true
        );

        float textY = bounds.Y + (bounds.Height - Text.FontSize) / 2f;

        RichTextRenderer.DrawRichText(
            Text,
            new Vector2(textX, textY),
            bounds.Width - (textX - bounds.X),
            Style,
            null
        );
    }

    private float EvaluateBusyEased(float t)
    {
        // t in [0,1]
        // use a high-order ease-in-out (quintic) to make endpoints super slow and middle very fast
        if (t < 0.5f)
        {
            float x = 2f * t; // 0..1
            return 0.5f * MathF.Pow(x, 5); // very slow near 0, accelerates to fast
        }
        else
        {
            float x = 2f * (1f - t); // 1..0
            return 1f - 0.5f * MathF.Pow(x, 5); // symmetric decel to super slow near 1
        }
    }
}
