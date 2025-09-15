using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;

namespace WinterRose.ForgeWarden.UserInterface.Content;

public class TextInput : UIContent
{
    public MulticastVoidInvocation<TextInput, string> OnSubmit = new();
    public MulticastVoidInvocation<TextInput, string> OnInputChanged = new();

    // --- state ---
    private string text = "";
    private bool hasFocus = false;
    private int caretIndex = 0; // caret position in characters (0..text.Length)
    private float caretTimer = 0f;
    private bool caretVisible = true;

    public bool IsPassword { get; set; } = false;
    public char MaskChar { get; set; } = '*';

    public string Placeholder { get; set; } = "";
    private Color placeholderColor = new Color(160, 160, 160, 200);

    public string Text => text;

    public bool HasFocus => hasFocus;

    public void Focus()
    {
        hasFocus = true;
        Input.IsRequestingKeyboardFocus = true;
        caretVisible = true;
        caretTimer = 0f;
    }

    public void Blur()
    {
        hasFocus = false;
        Input.IsRequestingKeyboardFocus = false;
        caretVisible = false;
        caretTimer = 0f;
    }

    protected internal override void Setup()
    {
        base.Setup();
        // ensure caret start state
        caretTimer = 0f;
        caretVisible = true;
    }

    public void UpdateInline() => Update();
    public void RenderInline(Rectangle bounds) => Draw(bounds);

    // measure height wrapper
    public float MeasureHeight(float maxWidth) => GetHeight(maxWidth);

    protected internal override void Update()
    {
        // blink caret when focused
        if (hasFocus)
        {
            caretTimer += Time.deltaTime;
            if (caretTimer >= Style.CaretBlinkingRate)
            {
                caretTimer = 0f;
                caretVisible = !caretVisible;
            }

            // process special key presses
            // backspace
            if (Input.IsPressed(KeyboardKey.Backspace))
            {
                if (caretIndex > 0 && text.Length > 0)
                {
                    text = text.Remove(caretIndex - 1, 1);
                    caretIndex = Math.Max(0, caretIndex - 1);
                    OnInputChanged?.Invoke(this, text);
                    caretVisible = true;
                    caretTimer = 0f;
                }
            }

            // delete
            if (Input.IsPressed(KeyboardKey.Delete))
            {
                if (caretIndex < text.Length)
                {
                    text = text.Remove(caretIndex, 1);
                    OnInputChanged?.Invoke(this, text);
                    caretVisible = true;
                    caretTimer = 0f;
                }
            }

            // left / right
            if (Input.IsPressed(KeyboardKey.Left))
            {
                caretIndex = Math.Max(0, caretIndex - 1);
                caretVisible = true;
                caretTimer = 0f;
            }

            if (Input.IsPressed(KeyboardKey.Right))
            {
                caretIndex = Math.Min(text.Length, caretIndex + 1);
                caretVisible = true;
                caretTimer = 0f;
            }

            // enter -> submit
            if (Input.IsPressed(KeyboardKey.Enter))
            {
                OnSubmit?.Invoke(this, text);
            }

            // character input (handles unicode)
            int c;
            while ((c = Raylib.GetCharPressed()) > 0)
            {
                // ignore control characters
                if (c >= 32)
                {
                    char ch = (char)c;
                    text = text.Insert(caretIndex, ch.ToString());
                    caretIndex++;
                    OnInputChanged?.Invoke(this, text);
                    caretVisible = true;
                    caretTimer = 0f;
                }
            }
        }
        else
        {
            // not focused -> ensure caret hidden but keep a stable state
            caretVisible = false;
            caretTimer = 0f;
        }
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // width: use full available width; height: measured via GetHeight
        return new Vector2(availableArea.Width, GetHeight(availableArea.Width));
    }

    protected override void Draw(Rectangle bounds)
    {
        // draw background + border (existing)
        var bg = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        ray.DrawRectangleRec(bg, Style.TextBoxBackground);

        var borderCol = hasFocus ? Style.TextBoxFocusedBorder : Style.TextBoxBorder;
        ray.DrawRectangleLinesEx(bg, 1f, borderCol);

        // compute what we will render: either masked or raw text
        string displayText = IsPassword ? new string(MaskChar, text.Length) : text;

        var font = Raylib.GetFontDefault();
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        Vector2 textPos = new Vector2(bounds.X + Style.TextBoxTextSpacing, bounds.Y + (bounds.Height - fontSize) / 2f - 1f);

        Raylib.BeginScissorMode((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);

        // if empty and placeholder is set and not focused -> draw placeholder
        if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(Placeholder) && !hasFocus)
        {
            Raylib.DrawTextEx(font, Placeholder, textPos, fontSize, spacing, placeholderColor);
        }
        else
        {
            Raylib.DrawTextEx(font, displayText, textPos, fontSize, spacing, Style.TextBoxText);
        }

        // draw caret if focused & visible (caret position measured against displayText)
        if (hasFocus && caretVisible)
        {
            string left = caretIndex > 0 ? displayText[..Math.Min(caretIndex, displayText.Length)] : "";
            Vector2 measure = Raylib.MeasureTextEx(font, left, fontSize, spacing);
            float caretX = textPos.X + measure.X;
            float caretY1 = bounds.Y + (bounds.Height - fontSize) / 2f + 2f;
            float caretY2 = caretY1 + fontSize - 4f;
            ray.DrawLineEx(new Vector2(caretX, caretY1), new Vector2(caretX, caretY2), Style.CaretWidth, Style.Caret);
        }

        Raylib.EndScissorMode();
    }

    protected internal override float GetHeight(float maxWidth)
    {
        var font = Raylib.GetFontDefault();
        float fontSize = Style.TextBoxFontSize;
        Vector2 measured = Raylib.MeasureTextEx(font, "Ay", fontSize, Style.TextBoxTextSpacing);
        return MathF.Max(Style.TextBoxMinHeight, measured.Y + Style.TextBoxTextSpacing * 2f);
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        hasFocus = false;
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        if (button == MouseButton.Left)
        {
            Input.IsRequestingKeyboardFocus = true;

            caretVisible = true;
            caretTimer = 0f;
            hasFocus = true;

            var font = Raylib.GetFontDefault();
            float fontSize = Style.TextBoxFontSize;
            float spacing = Style.TextBoxTextSpacing;

            var localMouse = Input.MousePosition;
            float startX = LastRenderBounds.X + Style.TextBoxTextSpacing;
            float localX = localMouse.X - startX;

            int idx = 0;
            float accum = 0f;

            // when password mode, measure mask char widths instead of real characters
            for (int i = 0; i <= text.Length; i++)
            {
                if (i == 0)
                {
                    accum = 0f;
                }
                else
                {
                    char chToMeasure = IsPassword ? MaskChar : text[i - 1];
                    string s = chToMeasure.ToString();
                    Vector2 m = Raylib.MeasureTextEx(font, s, fontSize, spacing);
                    accum += m.X;
                }

                if (localX < accum + 0.5f)
                {
                    idx = i;
                    break;
                }

                idx = i;
            }

            caretIndex = Math.Clamp(idx, 0, text.Length);
        }
    }

    protected internal override void OnHover()
    {
        base.OnHover();
        // cursor could be changed here if you have a cursor manager
    }

    protected internal override void OnHoverEnd()
    {
        base.OnHoverEnd();
    }

    protected internal override void OnOwnerClosing()
    {
        base.OnOwnerClosing();
        hasFocus = false;
    }

    public void SetText(string v)
    {
        text = v ?? "";
        caretIndex = Math.Clamp(text.Length, 0, text.Length);
        OnInputChanged?.Invoke(this, text);
    }
}
