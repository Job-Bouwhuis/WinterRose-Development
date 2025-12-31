using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.Content;

namespace WinterRose.ForgeWarden.UserInterface;

public class UIValueSlider<T> : UINumericControlBase<T> where T : INumber<T>, IMinMaxValue<T>
{
    // Layout constants
    private const float SLIDER_HEIGHT = 28f;
    private const float TRACK_HEIGHT = 6f;
    private const float HANDLE_RADIUS = 8f;

    // Animation smoothing (higher = snappier)
    private const double ANIMATION_RATE = 14.0;

    private float labelWidthReserved;

    private const float LABEL_PADDING = 8f;
    private Rectangle labelRect;
    private Rectangle sliderRect;

    public string Label { get; set; } = "";

    /// <summary>
    /// If true, the slider will snap to steps. Holding shift will override snap when HoldShiftToDisableSnap == true.
    /// </summary>
    public bool SnapToStep { get; set; } = true;

    /// <summary>
    /// When true, holding either Shift key disables snapping while dragging.
    /// </summary>
    public bool HoldShiftToDisableSnap { get; set; } = true;

    /// <summary>
    /// The event invoked when the value changes. the new value is passed as a parameter.
    /// </summary>
    public MulticastVoidInvocation<IUIContainer, UIValueSlider<T>, T> OnValueChanged { get; set; } = new();
    /// <summary>
    /// The event invoked when the value changes. the new value is passed as a parameter.
    /// </summary>
    public MulticastVoidInvocation<T> OnValueChangedBasic { get; set; } = new();

    // Internal state (store value as T but compute with doubles)
    public override T Value
    {
        get => base.Value;
        set
        {
            ClampAndSet(value, true);
        }
    }

    // Animated displayed value (double) to smooth the handle; target is the true Value
    private double animatedValueD;

    // Interaction state
    private bool isDragging = false;
    private bool isHovered = false;

    // Bounds: full control bounds (lastBounds) and the actual slider area (sliderBounds)
    private Rectangle lastBounds = new Rectangle();
    private Rectangle sliderBounds = new Rectangle();

    // Helpers for mouse dragging
    private float dragStartMouseX;
    private double dragStartValueD;

    // ctor
    public UIValueSlider(T min, T max, T initial)
    {
        MinValue = min;
        MaxValue = max;
        animatedValueD = Convert.ToDouble(initial);
        SetValue(initial, invokeCallbacks: false);

        TypedValueChanged.Subscribe(Invocation.Create((T t) =>
        {
            OnValueChanged?.Invoke(Owner, this, t);
            OnValueChangedBasic?.Invoke(t);
        }));
    }

    public UIValueSlider() : this((T)Convert.ChangeType(0, typeof(T)), (T)Convert.ChangeType(1, typeof(T)), (T)Convert.ChangeType(0, typeof(T)))
    {
    }

    // clamp and set internal value from a T; optionally invoke callbacks if changed
    private void ClampAndSet(T newVal, bool invokeCallbacks, bool snapAnimated = false)
    {
        if (MinValue > MaxValue)
        {
            var tmp = MinValue;
            MinValue = MaxValue;
            MaxValue = tmp;
        }

        T clamped = Clamp(newVal, MinValue, MaxValue);

        // snapping logic
        if (SnapToStep && Step > T.Zero)
        {
            bool shiftHeld = false;

            if (HoldShiftToDisableSnap && Input != null)
            {
                shiftHeld =
                    Input.IsDown(KeyboardKey.LeftShift) ||
                    Input.IsDown(KeyboardKey.RightShift);
            }
            if (!shiftHeld)
            {
                // snap to nearest step
                double relative = Convert.ToDouble(clamped - MinValue);
                double stepD = Convert.ToDouble(Step);
                if (stepD != 0.0)
                {
                    double snapped = Math.Round(relative / stepD) * stepD;
                    clamped = (T)Convert.ChangeType(Convert.ToDouble(MinValue) + snapped, typeof(T));
                    // clamp again for safety
                    clamped = Clamp(clamped, MinValue, MaxValue);
                }
            }
        }

        var newT = (T)Convert.ChangeType(clamped, typeof(T));
        bool changed = !Equals(valueBacking, newT);
        base.Value = newT;

        // only force the visual position when explicitly requested; otherwise let Update() smooth it
        if (snapAnimated)
            animatedValueD = Convert.ToDouble(base.Value);

        if (changed && invokeCallbacks)
        {
            OnValueChanged?.Invoke(Owner, this, valueBacking);
            OnValueChangedBasic?.Invoke(valueBacking);
        }
    }

    private T Clamp(T v, T minValue, T maxValue)
    {
        if (v < minValue)
            return minValue;
        else if (v > maxValue)
            return maxValue;
        return v;
    }

    // Map a mouse X coordinate to a value (double) using the provided slider bounds
    private double MouseXToValue(Rectangle bounds, float mouseX)
    {
        float left = bounds.X + UIConstants.CONTENT_PADDING + HANDLE_RADIUS;
        float right = bounds.X + bounds.Width - UIConstants.CONTENT_PADDING - HANDLE_RADIUS;
        float usable = Math.Max(1f, right - left);

        float clampedX = Math.Clamp(mouseX, left, right);
        float t = (clampedX - left) / usable; // 0..1
        double minD = Convert.ToDouble(MinValue);
        double maxD = Convert.ToDouble(MaxValue);
        return minD + t * (maxD - minD);
    }

    private float ValueToPosition(Rectangle bounds, double valueD)
    {
        double minD = Convert.ToDouble(MinValue);
        double maxD = Convert.ToDouble(MaxValue);
        if (maxD - minD == 0) return bounds.X + bounds.Width * 0.5f;

        float left = bounds.X + UIConstants.CONTENT_PADDING + HANDLE_RADIUS;
        float right = bounds.X + bounds.Width - UIConstants.CONTENT_PADDING - HANDLE_RADIUS;
        float usable = Math.Max(1f, right - left);
        float t = (float)((valueD - minD) / (maxD - minD));
        return left + t * usable;
    }

    // UI lifecycle implementations ------------------------------------------

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new Vector2(availableArea.Width, SLIDER_HEIGHT);
    }

    protected internal override float GetHeight(float maxWidth) => SLIDER_HEIGHT;

    protected internal override void Setup()
    {
        var font = ForgeWardenEngine.DefaultFont;
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        float labelPadding = 8f;

        // Use max value string to reserve enough space
        string referenceText;
        if (MaxValue is int)
            referenceText = ToStringFormatted(MaxValue);
        else
            referenceText = ToStringFormatted(MaxValue) + ".000";
        Vector2 refSize = Raylib.MeasureTextEx(font, referenceText, fontSize, spacing);
        labelWidthReserved = refSize.X + labelPadding * 2f;
    }

    protected internal override void Update()
    {
        // Smooth animatedValueD toward the true backing value (exponential smoothing)
        double target = Convert.ToDouble(valueBacking);
        float dt = Raylib_cs.Raylib.GetFrameTime();
        // smoothing factor (time constant style)
        double alpha = 1.0 - Math.Exp(-ANIMATION_RATE * dt);
        animatedValueD += (target - animatedValueD) * alpha;

        // If dragging, update value based on current mouse position
        if (isDragging)
        {
            if (!Input.IsDown(MouseButton.Left))
            {
                // mouse released -> stop dragging
                isDragging = false;
            }
            else
            {
                double newVal = MouseXToValue(sliderBounds, Input.Provider.MousePosition.X);
                if (SnapToStep && Step > T.Zero && HoldShiftToDisableSnap &&
                   (Input.Provider.IsDown(new InputBinding(InputDeviceType.Keyboard, (int)KeyboardKey.LeftShift))
                   || Input.Provider.IsDown(new InputBinding(InputDeviceType.Keyboard, (int)KeyboardKey.RightShift))))
                {
                    // temporarily disable snap while calling clamp
                    bool oldSnap = SnapToStep;
                    SnapToStep = false;
                    ClampAndSet((T)Convert.ChangeType(newVal, typeof(T)), invokeCallbacks: true, snapAnimated: false);
                    SnapToStep = oldSnap;
                }
                else
                {
                    ClampAndSet((T)Convert.ChangeType(newVal, typeof(T)), invokeCallbacks: true, snapAnimated: false);
                }
            }
        }

        // standard hover detection (based on sliderBounds)
        float mx = Input.Provider.MousePosition.X;
        float my = Input.Provider.MousePosition.Y;
        bool wasHovered = isHovered;
        isHovered = mx >= sliderBounds.X && mx <= sliderBounds.X + sliderBounds.Width && my >= sliderBounds.Y && my <= sliderBounds.Y + sliderBounds.Height;

        if (isHovered && !wasHovered)
            OnHover();
        else if (!isHovered && wasHovered)
            OnHoverEnd();
    }

    protected override void Draw(Rectangle bounds)
    {
        lastBounds = bounds;

        // --- compute value label area on the left ---
        var font = ForgeWardenEngine.DefaultFont;
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        string valueText = ToStringFormatted(valueBacking);
        Vector2 textSize = Raylib.MeasureTextEx(font, valueText, fontSize, spacing);
        float labelPadding = 8f;
        float labelWidth = labelWidthReserved; // constant width now
        sliderBounds = new Rectangle(bounds.X + (int)labelWidth, bounds.Y, bounds.Width - (int)labelWidth, bounds.Height);

        // draw label on left (right-aligned within label area)
        Vector2 labelPos = new Vector2(bounds.X + labelPadding, bounds.Y + (bounds.Height - textSize.Y) / 2f);
        Raylib.DrawTextEx(font, valueText, labelPos, fontSize, spacing, Style.TextBoxText);

        // track rect inside sliderBounds
        float trackY = sliderBounds.Y + sliderBounds.Height / 2f;
        float left = sliderBounds.X + UIConstants.CONTENT_PADDING + HANDLE_RADIUS;
        float right = sliderBounds.X + sliderBounds.Width - UIConstants.CONTENT_PADDING - HANDLE_RADIUS;
        float trackTop = trackY - TRACK_HEIGHT / 2f;
        float trackHeight = TRACK_HEIGHT;
        var trackRect = new Rectangle(left, trackTop, Math.Max(1f, right - left), trackHeight);

        // draw track background
        ray.DrawRectangleRec(trackRect, Style.TextBoxBackground);
        ray.DrawRectangleLinesEx(trackRect, 1f, Style.TextBoxBorder);

        // draw filled portion using animated value (smooth visual)
        float fillRight = ValueToPosition(sliderBounds, animatedValueD);
        var fillRect = new Rectangle(trackRect.X, trackRect.Y, Math.Max(0f, fillRight - trackRect.X), trackRect.Height);
        ray.DrawRectangleRec(fillRect, Style.SliderFilled);

        // draw handle at animated position (less jitter)
        int handleX = (int)ValueToPosition(sliderBounds, animatedValueD);
        int handleY = (int)trackY;
        Color handleColor = isDragging ? Style.ButtonClick : (isHovered ? Style.ButtonHover : Style.ButtonBackground);
        Raylib_cs.Raylib.DrawCircle(handleX, handleY, HANDLE_RADIUS, handleColor);
        Raylib_cs.Raylib.DrawCircleLines(handleX, handleY, HANDLE_RADIUS, Style.ButtonBorder);

        // draw small tick marks for steps (optional visual - drawn lightly when Step > 0)
        if (Step > T.Zero)
        {
            double minD = Convert.ToDouble(MinValue);
            double maxD = Convert.ToDouble(MaxValue);
            double stepD = Convert.ToDouble(Step);
            if (stepD > 0 && maxD > minD)
            {
                int steps = (int)Math.Floor((maxD - minD) / stepD);
                for (int s = 0; s <= steps; s++)
                {
                    double val = minD + s * stepD;
                    float x = ValueToPosition(sliderBounds, val);
                    ray.DrawLineEx(
                        new Vector2(x, trackRect.Y - 2), 
                        new Vector2(x, trackRect.Y + trackRect.Height + 2), 
                        1f,
                        Style.SliderTick);
                }
            }
        }
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        if (button != MouseButton.Left) return;

        float mx = Input.Provider.MousePosition.X;
        float my = Input.Provider.MousePosition.Y;

        // if click is inside handle area -> start dragging
        double curVal = Convert.ToDouble(valueBacking);
        float handleCX = ValueToPosition(sliderBounds, curVal);
        float handleCY = sliderBounds.Y + sliderBounds.Height / 2f;
        var handleRect = new Rectangle(handleCX - HANDLE_RADIUS, handleCY - HANDLE_RADIUS, HANDLE_RADIUS * 2, HANDLE_RADIUS * 2);

        if (mx >= handleRect.X && mx <= handleRect.X + handleRect.Width && my >= handleRect.Y && my <= handleRect.Y + handleRect.Height)
        {
            isDragging = true;
            dragStartMouseX = mx;
            dragStartValueD = curVal;
            return;
        }

        // otherwise clicking on slider track jumps to that position and starts dragging
        if (mx >= sliderBounds.X && mx <= sliderBounds.X + sliderBounds.Width && my >= sliderBounds.Y && my <= sliderBounds.Y + sliderBounds.Height)
        {
            double newVal = MouseXToValue(sliderBounds, mx);
            ClampAndSet((T)Convert.ChangeType(newVal, typeof(T)), invokeCallbacks: true, snapAnimated: false);
            // start dragging so user can continue adjusting
            isDragging = true;
            dragStartMouseX = mx;
            dragStartValueD = Convert.ToDouble(valueBacking);
        }
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        // clicking outside cancels dragging
        isDragging = false;
    }

    // helper to format numeric display
    private string ToStringFormatted(T v)
    {
        // default formatting: try to preserve reasonable number of decimal places for floats/doubles/decimals
        if (v is float f) return f.ToString("0.###");
        if (v is double d) return d.ToString("0.###");
        if (v is decimal m) return m.ToString("0.###");
        return v.ToString();
    }
}