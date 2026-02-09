using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;

namespace WinterRose.ForgeWarden.UserInterface;

public class UINumericUpDown<T> : UINumericControlBase<T>
    where T : INumber<T>, IMinMaxValue<T>
{
    // Layout constants
    private const float CONTROL_HEIGHT = 28f;
    private const float PADDING_X = 6f;
    private const float BUTTON_WIDTH = 20f;         // width of the stacked ^/v button column
    private const float BUTTON_SPACING = 2f;
    private const float LABEL_PADDING = 8f;

    public bool ReadOnly { get; set; }

    public T MinValue { get; set; }
    public T MaxValue { get; set; }

    /// <summary> Step used when pressing the up/down buttons. </summary>
    public T Step { get; set; } = T.One;

    /// <summary> Number of decimal places to round input to. Use 0 for integer-style rounding. </summary>
    public int DecimalPlaces { get; set; } = 3;

    /// <summary> Events raised when the value changes. </summary>
    public MulticastVoidInvocation<IUIContainer, UINumericUpDown<T>, T> OnValueChanged { get; set; } = new();
    public MulticastVoidInvocation<T> OnValueChangedBasic { get; set; } = new();

    // Inline text editor for typing values
    private UITextInput valueInput = new();
    private bool isEditing = false;

    // Hit / layout rects cached per-draw
    private Rectangle lastBounds = new Rectangle();
    private Rectangle labelRect = new Rectangle();
    private Rectangle inputRect = new Rectangle();
    private Rectangle buttonUpRect = new Rectangle();
    private Rectangle buttonDownRect = new Rectangle();

    // Reserved label width so layout doesn't move
    private float labelWidthReserved = 0f;

    public float DragPixelsForFullRange { get; set; } = 150f;

    // label drag state
    private bool isLabelDragging = false;
    private float labelDragStartX;
    private double labelDragStartValueD;
    private bool isLabelHovered = false;

    public UINumericUpDown(T min, T max, T initial)
    {
        MinValue = min;
        MaxValue = max;
        if (initial is int)
            DecimalPlaces = -1;
        SetValue(initial, false);
        TypedValueChanged.Subscribe(Invocation.Create((T t) =>
        {
            OnValueChanged?.Invoke(Owner, this, t);
            OnValueChangedBasic?.Invoke(t);
        }));
    }

    public UINumericUpDown() : this(T.MinValue, T.MaxValue, T.Zero)
    {
    }

    private string ToStringFormatted(T v)
    {
        if (v is float f) return f.ToString("0." + new string('#', Math.Clamp(DecimalPlaces, 0, 10)), CultureInfo.InvariantCulture);
        if (v is double d) return d.ToString("0." + new string('#', Math.Clamp(DecimalPlaces, 0, 15)), CultureInfo.InvariantCulture);
        if (v is decimal m) return m.ToString("0." + new string('#', Math.Clamp(DecimalPlaces, 0, 28)), CultureInfo.InvariantCulture);
        return v.ToString() ?? "";
    }

    // parse user text into T; returns whether parse succeeded
    private bool TryParseToT(string text, out T result)
    {
        result = T.Zero;
        if (string.IsNullOrWhiteSpace(text)) return false;

        // try double parse first (invariant)
        if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d))
        {
            // apply rounding to parsed value
            if (DecimalPlaces >= 0)
                d = Math.Round(d, DecimalPlaces);

            try
            {
                result = (T)Convert.ChangeType(d, typeof(T), CultureInfo.InvariantCulture)!;
                return true;
            }
            catch
            {
                // fallthrough
            }
        }

        // last ditch: try Convert.ChangeType directly (integers etc.)
        try
        {
            result = (T)Convert.ChangeType(text, typeof(T), CultureInfo.InvariantCulture)!;
            return true;
        }
        catch
        {
            return false;
        }
    }

    // UI lifecycle ----------------------------------------------------------

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new Vector2(availableArea.Width, CONTROL_HEIGHT);
    }

    protected internal override float GetHeight(float maxWidth) => CONTROL_HEIGHT;

    protected internal override void Setup()
    {
        // Prepare the value input
        valueInput.Owner = Owner;
        valueInput.Text = ToStringFormatted(valueBacking);

        // subscribe to submit so typed values commit
        valueInput.OnSubmit.Subscribe(Invocation.Create<UITextInput, string>((_, text) =>
        {
            if (TryParseToT(text, out var parsed))
            {
                SetValue(parsed, invokeCallbacks: true);
            }
            else
            {
                // revert to current formatted value if parse fails
                valueInput.Text = ToStringFormatted(valueBacking);
            }

            valueInput.Unfocus();
            isEditing = false;
        }));

        // keep text in sync during inline typing if desired (no immediate commit)
        valueInput.OnInputChanged.Subscribe(Invocation.Create<UITextInput, string>((_, txt) =>
        {
            // noop for now; we only commit on submit/blurs
        }));

        valueInput.Setup();

        // reserve label width based on label text and some padding so it doesn't jitter
        var font = ForgeWardenEngine.DefaultFont;
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;

        string refLabel = string.IsNullOrEmpty(Label) ? " " : Label;
        Vector2 labelSz = Raylib.MeasureTextEx(font, refLabel, fontSize, spacing);
        labelWidthReserved = labelSz.X + LABEL_PADDING * 2f;

        base.Setup();
    }

    protected override void Update()
    {
        // let the inline editor run when editing
        valueInput.UpdateInline();

        // label hover detection (labelRect is set during Draw, so keep in sync)
        var mouse = Input.Provider.MousePosition;
        isLabelHovered = mouse.X >= labelRect.X && mouse.X <= labelRect.X + labelRect.Width
                         && mouse.Y >= labelRect.Y && mouse.Y <= labelRect.Y + labelRect.Height;

        // label dragging: adjust value based on horizontal delta
        if (isLabelDragging)
        {
            Input.IsRequestingMouseFocus = true;
            if (!Input.IsDown(MouseButton.Left))
            {
                isLabelDragging = false;
            }
            else
            {
                float dx = mouse.X - labelDragStartX;
                double minD = Convert.ToDouble(MinValue);
                double maxD = Convert.ToDouble(MaxValue);
                double range = maxD - minD;

                if (range == 0.0)
                    return;

                // --- magnitude-aware base scaling ---
                double startMagnitude = Math.Max(1.0, Math.Abs(labelDragStartValueD));
                double baseScale = startMagnitude / DragPixelsForFullRange;

                // --- modifier keys ---
                bool shiftHeld = Input.IsDown(KeyboardKey.LeftShift) || Input.IsDown(KeyboardKey.RightShift);
                bool ctrlHeld = Input.IsDown(KeyboardKey.LeftControl) || Input.IsDown(KeyboardKey.RightControl);

                if (shiftHeld)
                    baseScale *= 0.1;   // fine control
                else if (ctrlHeld)
                    baseScale *= 5.0;   // coarse control

                // compute delta and target value
                double deltaValue = dx * baseScale;
                double target = labelDragStartValueD + deltaValue;

                // clamp to min/max
                target = Math.Clamp(target, minD, maxD);

                try
                {
                    var tVal = (T)Convert.ChangeType(target, typeof(T));
                    SetValue(tVal, invokeCallbacks: true);
                }
                catch
                {
                }
            }
        }
    }

    protected override void Draw(Rectangle bounds)
    {
        lastBounds = bounds;

        // fixed label area on the left to avoid jitter
        float labelWidth = labelWidthReserved;
        labelRect = new Rectangle(bounds.X, bounds.Y, (int)labelWidth, bounds.Height);

        // button column at right
        float buttonColX = bounds.X + bounds.Width - BUTTON_WIDTH;
        buttonUpRect = new Rectangle((int)buttonColX, bounds.Y, (int)BUTTON_WIDTH, (int)(bounds.Height / 2f - BUTTON_SPACING / 2f));
        buttonDownRect = new Rectangle((int)buttonColX, (int)(bounds.Y + bounds.Height / 2f + BUTTON_SPACING / 2f), (int)BUTTON_WIDTH, (int)(bounds.Height / 2f - BUTTON_SPACING / 2f));

        // input area between label and buttons (leave padding)
        float inputX = labelRect.X + labelRect.Width + PADDING_X;
        float inputW = Math.Max(40f, bounds.Width - labelRect.Width - BUTTON_WIDTH - PADDING_X * 2f);
        inputRect = new Rectangle((int)inputX, bounds.Y + 2, (int)inputW, bounds.Height - 4);

        if (isLabelDragging || isLabelHovered)
        {
            var chevronColor = isLabelDragging ? Style.ButtonClick : Style.ButtonHover;

            // chevron sizing and positions
            float chevSize = MathF.Min(12f, labelRect.Height * 0.4f);
            float centerY = labelRect.Y + labelRect.Height * 0.5f;
            float stroke = 1f;
            float edgeInset = -2f; // how far from the label edge the chevrons sit

            // left chevron '<'
            float leftEdgeX = labelRect.X + edgeInset + 1;
            var lA = new Vector2(leftEdgeX + chevSize, centerY - chevSize);
            var lB = new Vector2(leftEdgeX, centerY);
            var lC = new Vector2(leftEdgeX + chevSize, centerY + chevSize);
            ray.DrawLineEx(lA, lB, stroke, chevronColor);
            ray.DrawLineEx(lB, lC, stroke, chevronColor);

            // right chevron '>'
            float rightEdgeX = labelRect.X + labelRect.Width - edgeInset;
            var rA = new Vector2(rightEdgeX - chevSize, centerY - chevSize);
            var rB = new Vector2(rightEdgeX, centerY);
            var rC = new Vector2(rightEdgeX - chevSize, centerY + chevSize);
            ray.DrawLineEx(rA, rB, stroke, chevronColor);
            ray.DrawLineEx(rB, rC, stroke, chevronColor);
        }
        // draw label (left)
        var font = ForgeWardenEngine.DefaultFont;
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        Vector2 labelPos = new Vector2(labelRect.X + LABEL_PADDING, bounds.Y + (bounds.Height - fontSize) / 2f);
        Raylib.DrawTextEx(font, Label ?? "", labelPos, fontSize, spacing, Style.TextBoxText);

        // draw input box background and border
        ray.DrawRectangleRec(inputRect, Style.TextBoxBackground);
        ray.DrawRectangleLinesEx(inputRect, 1f, Style.TextBoxBorder);

        // if editing render the inline text input, else draw the value text
        if (isEditing)
        {
            valueInput.RenderInline(inputRect);
        }
        else
        {
            // center-left the displayed value
            string display = ToStringFormatted(valueBacking);
            Vector2 txtSz = Raylib.MeasureTextEx(font, display, fontSize, spacing);
            Vector2 txtPos = new Vector2(inputRect.X + PADDING_X, inputRect.Y + (inputRect.Height - txtSz.Y) / 2f - 1f);
            Raylib.DrawTextEx(font, display, txtPos, fontSize, spacing, Style.TextBoxText);
        }

        // up/down labels using text instead of triangles
        string downSymbol = "v";

        Vector2 upSize = Raylib.MeasureTextEx(font, downSymbol, 16, spacing);
        Vector2 dnSize = Raylib.MeasureTextEx(font, downSymbol, 16, spacing);

        // center text inside the button rects
        Vector2 upPos = new(
            buttonUpRect.X + (buttonUpRect.Width - upSize.X) / 2f,
            buttonUpRect.Y + (buttonUpRect.Height - upSize.Y) / 2f
        );

        Vector2 dnPos = new(
            buttonDownRect.X + (buttonDownRect.Width - dnSize.X) / 2f,
            buttonDownRect.Y + (buttonDownRect.Height - dnSize.Y) / 2f
        );

        // detect hover and click states for each button
        bool upHovered = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, buttonUpRect);
        bool downHovered = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, buttonDownRect);

        bool upClicked = upHovered && Input.IsDown(MouseButton.Left);
        bool downClicked = downHovered && Input.IsDown(MouseButton.Left);

        // choose colors based on state
        Color upColor = upClicked ? Style.ButtonClick : (upHovered ? Style.ButtonHover : Style.ButtonBackground);
        Color dnColor = downClicked ? Style.ButtonClick : (downHovered ? Style.ButtonHover : Style.ButtonBackground);

        // draw backgrounds and borders with state colors
        ray.DrawRectangleRec(buttonUpRect, upColor);
        ray.DrawRectangleLinesEx(buttonUpRect, 1f, Style.ButtonBorder);
        ray.DrawRectangleRec(buttonDownRect, dnColor);
        ray.DrawRectangleLinesEx(buttonDownRect, 1f, Style.ButtonBorder);

        // draw the symbols (rotate for up arrow)
        Raylib.DrawTextPro(font, downSymbol, upPos, upSize, 180, 16, spacing, Style.TextBoxText);
        Raylib.DrawTextEx(font, downSymbol, dnPos, 16, spacing, Style.TextBoxText);

    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        if (button != MouseButton.Left) return;

        float mx = Input.Provider.MousePosition.X;
        float my = Input.Provider.MousePosition.Y;

        // clicking the label area starts a drag operation (left/right to change value)
        if (mx >= labelRect.X && mx <= labelRect.X + labelRect.Width &&
            my >= labelRect.Y && my <= labelRect.Y + labelRect.Height)
        {
            isLabelDragging = true;
            labelDragStartX = mx;
            labelDragStartValueD = Convert.ToDouble(valueBacking);
            return;
        }

        // up button
        if (mx >= buttonUpRect.X && mx <= buttonUpRect.X + buttonUpRect.Width &&
            my >= buttonUpRect.Y && my <= buttonUpRect.Y + buttonUpRect.Height)
        {
            T next = valueBacking + Step;
            SetValue(next, invokeCallbacks: true);
            return;
        }

        // down button
        if (mx >= buttonDownRect.X && mx <= buttonDownRect.X + buttonDownRect.Width &&
            my >= buttonDownRect.Y && my <= buttonDownRect.Y + buttonDownRect.Height)
        {
            T next = valueBacking - Step;
            SetValue(next, invokeCallbacks: true);
            return;
        }

        // clicking the input area begins editing
        if (mx >= inputRect.X && mx <= inputRect.X + inputRect.Width &&
            my >= inputRect.Y && my <= inputRect.Y + inputRect.Height)
        {
            // put current formatted value into input and focus
            valueInput.Text = ToStringFormatted(valueBacking);
            valueInput.Focus();
            isEditing = true;
            return;
        }
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        // cancel label drag if active
        isLabelDragging = false;

        // existing behavior: commit & end editing
        if (isEditing)
        {
            if (TryParseToT(valueInput.Text, out var parsed))
            {
                SetValue(parsed, invokeCallbacks: true);
            }
            else
            {
                valueInput.Text = ToStringFormatted(valueBacking);
            }

            valueInput.Unfocus();
            isEditing = false;
        }
    }

    protected internal override void OnOwnerClosing()
    {
        // ensure input is blurred / cleaned
        valueInput.Unfocus();
    }

    public void Focus() => valueInput.Focus();
}

