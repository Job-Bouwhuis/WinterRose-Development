using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Utility;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace WinterRose.ForgeWarden.UserInterface;

public class UITextInput : UIContent
{
    public MulticastVoidInvocation<UITextInput, string> OnSubmit = new();
    public MulticastVoidInvocation<UITextInput, string> OnInputChanged = new();

    // --- state ---
    private string text = "";
    private bool hasFocus = false;
    private int caretIndex = 0; // caret position in characters (0..text.Length)
    private float caretTimer = 0f;
    private bool caretVisible = true;

    // selection state
    private int selStart = 0; // inclusive
    private int selEnd = 0;   // exclusive
    private int selAnchor = 0; // where selection started
    private bool isSelectingWithMouse = false;

    // click counts for double/triple click
    private double lastClickTime = 0.0;
    private int clickCount = 0;
    private const double DOUBLE_CLICK_THRESHOLD = 0.2;

    // storage of last mouse-down index to support dragging
    private int mouseDownIndex = 0;

    public bool IsPassword { get; set; } = false;
    public char MaskChar { get; set; } = '*';

    public string Placeholder { get; set; } = "";
    private Color placeholderColor = new Color(160, 160, 160, 200);

    public string Text
    {
        get => text;
        set => SetText(value);
    }

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
        caretVisible = false;
        caretTimer = 0f;
    }

    protected internal override void Setup()
    {
        caretTimer = 0f;
        caretVisible = true;
    }

    // store last drawn bounds so mouse->index mapping uses the exact rect used for rendering
    private Rectangle lastDrawBounds = new Rectangle();

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // width: use full available width; height: measured via GetHeight
        return new Vector2(availableArea.Width, GetHeight(availableArea.Width));
    }

    public void UpdateInline() => Update();
    public void RenderInline(Rectangle bounds) => Draw(bounds);

    // measure height wrapper
    public float MeasureHeight(float maxWidth) => GetHeight(maxWidth);

    protected internal override void Update()
    {
        // caret blinking and keyboard handling when focused
        if (hasFocus)
        {
            Input.IsRequestingKeyboardFocus = true;
            caretTimer += Time.deltaTime;
            if (caretTimer >= Style.CaretBlinkingRate)
            {
                caretTimer = 0f;
                caretVisible = !caretVisible;
            }

            bool shift = Input.IsDown(KeyboardKey.LeftShift) || Input.IsDown(KeyboardKey.RightShift);
            bool ctrl = Input.IsDown(KeyboardKey.LeftControl) || Input.IsDown(KeyboardKey.RightControl);

            // Clipboard handlers placeholders
            if (ctrl && Input.IsPressed(KeyboardKey.C))
            {
                string selected = HasSelection() ? text[selStart..selEnd] : "";
                if (OperatingSystem.IsWindows())
                {
                    Windows.Clipboard.WriteString(selected);
                }
                else
                {
                    Toasts.Error("Copying on any platform that isnt windows is not yet supported");
                }
            }
            if (ctrl && Input.IsPressed(KeyboardKey.V))
            {
                if (OperatingSystem.IsWindows())
                {
                    string fromClipboard = Windows.Clipboard.ReadString();

                    if (HasSelection())
                        DeleteSelection();

                    text = text.Insert(caretIndex, fromClipboard);
                    caretIndex += fromClipboard.Length;
                    ClearSelection();
                    OnInputChanged?.Invoke(this, text);
                }
                else
                {
                    Toasts.Error("Pasting on any platform that isnt windows is not yet supported");
                }
            }

            // navigation & editing keys
            if (Input.IsPressed(KeyboardKey.Backspace))
            {
                if (HasSelection())
                {
                    DeleteSelection();
                    OnInputChanged?.Invoke(this, text);
                }
                else if (caretIndex > 0 && text.Length > 0)
                {
                    text = text.Remove(caretIndex - 1, 1);
                    caretIndex = Math.Max(0, caretIndex - 1);
                    ClearSelection();
                    OnInputChanged?.Invoke(this, text);
                }
                caretVisible = true;
                caretTimer = 0f;
            }

            if (Input.IsPressed(KeyboardKey.Delete))
            {
                if (HasSelection())
                {
                    DeleteSelection();
                    OnInputChanged?.Invoke(this, text);
                }
                else if (caretIndex < text.Length)
                {
                    text = text.Remove(caretIndex, 1);
                    OnInputChanged?.Invoke(this, text);
                }
                caretVisible = true;
                caretTimer = 0f;
            }

            // Left / Right navigation with modifiers
            if (Input.IsPressed(KeyboardKey.Left))
            {
                HandleLeftArrow(ctrl, shift);
                caretVisible = true;
                caretTimer = 0f;
            }
            if (Input.IsPressed(KeyboardKey.Right))
            {
                HandleRightArrow(ctrl, shift);
                caretVisible = true;
                caretTimer = 0f;
            }

            // Enter -> submit
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
                    if (HasSelection())
                    {
                        // replace selection with typed text
                        DeleteSelection();
                    }

                    text = text.Insert(caretIndex, ch.ToString());
                    caretIndex++;
                    ClearSelection();
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

        // mouse-based dragging update: if user is dragging selection with mouse, update selection
        if (isSelectingWithMouse)
        {
            if (Input.IsDown(MouseButton.Left))
            {
                Vector2 mp = Input.Provider.MousePosition;
                int idx = GetCaretIndexFromMousePosition(mp);
                ExtendSelectionTo(idx);
            }
            else
            {
                // mouse released
                isSelectingWithMouse = false;
            }
        }
    }

    protected override void Draw(Rectangle bounds)
    {
        // save bounds for mouse->index mapping
        lastDrawBounds = bounds;

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

        ScissorStack.Push(bounds);

        // draw placeholder if empty & not focused
        if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(Placeholder) && !hasFocus)
        {
            Raylib.DrawTextEx(font, Placeholder, textPos, fontSize, spacing, placeholderColor);
        }
        else
        {
            Raylib.DrawTextEx(font, displayText, textPos, fontSize, spacing, Style.TextBoxText);

            // draw selection highlight if present
            if (HasSelection())
            {
                int s0 = Math.Clamp(selStart, 0, displayText.Length);
                int s1 = Math.Clamp(selEnd, 0, displayText.Length);
                if (s0 < s1)
                {
                    string left = displayText[..s0];
                    string sel = displayText[s0..s1];

                    Vector2 leftMeasure = Raylib.MeasureTextEx(font, left, fontSize, spacing);
                    Vector2 selMeasure = Raylib.MeasureTextEx(font, sel, fontSize, spacing);

                    float selX = textPos.X + leftMeasure.X;
                    float selY = textPos.Y - 2f;
                    float selH = fontSize + 4f;

                    // selection background (semi-transparent)
                    Color selBg = new Color(80, 120, 200, 160);
                    Raylib.DrawRectangleRec(new Rectangle(selX, selY, selMeasure.X, selH), selBg);

                    // draw the selected substring on top in highlight color
                    Raylib.DrawTextEx(font, sel, new Vector2(selX, textPos.Y), fontSize, spacing, Color.White);
                }
            }
        }

        // draw caret if focused & visible (caret position measured against displayText)
        if (hasFocus && caretVisible)
        {
            string left = caretIndex > 0 ? (IsPassword ? new string(MaskChar, Math.Min(caretIndex, text.Length)) : displayText[..Math.Min(caretIndex, displayText.Length)]) : "";
            Vector2 measure = Raylib.MeasureTextEx(font, left, fontSize, spacing);
            float caretX = textPos.X + measure.X;
            float caretY1 = bounds.Y + (bounds.Height - fontSize) / 2f + 2f;
            float caretY2 = caretY1 + fontSize - 4f;
            ray.DrawLineEx(new Vector2(caretX, caretY1), new Vector2(caretX, caretY2), Style.CaretWidth, Style.Caret);
        }

        ScissorStack.Pop();
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
        isSelectingWithMouse = false;
        ClearSelection();
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        if (button != MouseButton.Left) return;

        Input.IsRequestingKeyboardFocus = true;
        caretVisible = true;
        caretTimer = 0f;
        hasFocus = true;

        // compute caret index at mouse
        Vector2 mp = Input.Provider.MousePosition;
        int idx = GetCaretIndexFromMousePosition(mp);

        double now = Raylib.GetTime();
        if (now - lastClickTime <= DOUBLE_CLICK_THRESHOLD)
        {
            clickCount++;
        }
        else
        {
            clickCount = 1;
        }
        lastClickTime = now;

        if (clickCount == 1)
        {
            // single click: place caret and start possible drag
            SetCaretAndClearSelection(idx);
            selAnchor = caretIndex;
            mouseDownIndex = idx;
            isSelectingWithMouse = true;
        }
        else if (clickCount == 2)
        {
            // double click: select token separated by whitespace (space-separated section)
            SelectTokenAt(idx);
            // don't start drag on double-click
            isSelectingWithMouse = false;
        }
        else // triple-or-more
        {
            // triple click: select all
            SelectAll();
            isSelectingWithMouse = false;
            clickCount = 0; // reset
        }
    }

    protected internal override void OnHover()
    {
        base.OnHover();
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
        ClearSelection();
        OnInputChanged?.Invoke(this, text);
    }

    // ---------------------- selection helpers ----------------------------

    private bool HasSelection() => selEnd > selStart;

    private void ClearSelection()
    {
        selStart = selEnd = caretIndex;
        selAnchor = caretIndex;
    }

    private void SetSelectionRange(int a, int b)
    {
        int s = Math.Clamp(Math.Min(a, b), 0, text.Length);
        int e = Math.Clamp(Math.Max(a, b), 0, text.Length);
        selStart = s;
        selEnd = e;
    }

    private void SetCaretAndClearSelection(int index)
    {
        caretIndex = Math.Clamp(index, 0, text.Length);
        selAnchor = caretIndex;
        ClearSelection();
    }

    private void StartSelectionAt(int index)
    {
        selAnchor = Math.Clamp(index, 0, text.Length);
        ExtendSelectionTo(index);
        isSelectingWithMouse = true;
    }

    private void ExtendSelectionTo(int index)
    {
        index = Math.Clamp(index, 0, text.Length);
        caretIndex = index;
        SetSelectionRange(selAnchor, index);
    }

    private void DeleteSelection()
    {
        if (!HasSelection()) return;
        text = text.Remove(selStart, selEnd - selStart);
        caretIndex = selStart;
        ClearSelection();
    }

    private void SelectAll()
    {
        selStart = 0;
        selEnd = text.Length;
        caretIndex = selEnd;
        selAnchor = selStart;
    }

    private void SelectTokenAt(int idx)
    {
        // select non-whitespace token around idx (space-separated)
        if (string.IsNullOrEmpty(text))
        {
            ClearSelection();
            return;
        }

        idx = Math.Clamp(idx, 0, text.Length);
        // if idx == text.Length treat as end-1 for token detection
        int i = idx;
        if (i > 0 && i == text.Length) i = text.Length - 1;

        // if current char is whitespace, expand out to the next non-space (makes double click behave sensibly)
        if (i >= 0 && i < text.Length && char.IsWhiteSpace(text[i]))
        {
            // search left for a non-space, if none, search right
            int left = i;
            while (left > 0 && char.IsWhiteSpace(text[left])) left--;
            if (left == 0 && char.IsWhiteSpace(text[left]))
            {
                int right = i;
                while (right < text.Length && char.IsWhiteSpace(text[right])) right++;
                SetSelectionRange(i, right);
                caretIndex = right;
                selAnchor = i;
                return;
            }
            i = left;
        }

        // find token boundaries by whitespace
        int start = i;
        while (start > 0 && !char.IsWhiteSpace(text[start - 1])) start--;
        int end = i;
        while (end < text.Length && !char.IsWhiteSpace(text[end])) end++;

        SetSelectionRange(start, end);
        caretIndex = end;
        selAnchor = start;
    }

    // ---------------------- keyboard navigation -------------------------

    private void HandleLeftArrow(bool ctrl, bool shift)
    {
        if (ctrl && shift)
        {
            // ctrl+shift => select/deselect any space-separated token to left
            int target = MoveLeftToPreviousSpaceToken(caretIndex);
            if (shift) ExtendSelectionTo(target); else SetCaretAndClearSelection(target);
        }
        else if (ctrl)
        {
            // ctrl (no shift) => jump left by contiguous letters (a-zA-Z)
            int target = MoveLeftByLetterGroup(caretIndex);
            if (shift) ExtendSelectionTo(target); else SetCaretAndClearSelection(target);
        }
        else if (shift)
        {
            // shift only: extend selection by one character left
            int target = Math.Max(0, caretIndex - 1);
            ExtendSelectionTo(target);
        }
        else
        {
            // plain left: move one char left and clear selection
            int target = Math.Max(0, caretIndex - 1);
            SetCaretAndClearSelection(target);
        }
    }

    private void HandleRightArrow(bool ctrl, bool shift)
    {
        if (ctrl && shift)
        {
            // ctrl+shift => select/deselect any space-separated token to right
            int target = MoveRightToNextSpaceToken(caretIndex);
            if (shift) ExtendSelectionTo(target); else SetCaretAndClearSelection(target);
        }
        else if (ctrl)
        {
            // ctrl (no shift) => jump right by contiguous letters (a-zA-Z)
            int target = MoveRightByLetterGroup(caretIndex);
            if (shift) ExtendSelectionTo(target); else SetCaretAndClearSelection(target);
        }
        else if (shift)
        {
            // shift only: extend selection by one character right
            int target = Math.Min(text.Length, caretIndex + 1);
            ExtendSelectionTo(target);
        }
        else
        {
            // plain right: move one char right and clear selection
            int target = Math.Min(text.Length, caretIndex + 1);
            SetCaretAndClearSelection(target);
        }
    }

    // Move left to previous group of letters (a-zA-Z). If currently inside non-letter, skip them first then skip letters.
    private int MoveLeftByLetterGroup(int idx)
    {
        if (idx <= 0) return 0;
        int i = idx - 1;
        // skip non-letter to reach a letter if present
        while (i >= 0 && !IsLetterChar(text[i])) i--;
        // now skip letters to the left to the start of this group
        while (i >= 0 && IsLetterChar(text[i])) i--;
        return Math.Max(0, i + 1);
    }

    // Move right to next group of letters (a-zA-Z). If currently inside non-letter, skip them first then skip letters.
    private int MoveRightByLetterGroup(int idx)
    {
        if (idx >= text.Length) return text.Length;
        int i = idx;
        // skip non-letter
        while (i < text.Length && !IsLetterChar(text[i])) i++;
        // skip letters
        while (i < text.Length && IsLetterChar(text[i])) i++;
        return Math.Min(text.Length, i);
    }

    // ctrl+shift: move to previous space-separated token boundary (start of previous token)
    private int MoveLeftToPreviousSpaceToken(int idx)
    {
        if (idx <= 0) return 0;
        int i = idx - 1;
        // skip whitespace
        while (i >= 0 && char.IsWhiteSpace(text[i])) i--;
        // skip token (non-space) to its start
        while (i >= 0 && !char.IsWhiteSpace(text[i])) i--;
        return Math.Max(0, i + 1);
    }

    private int MoveRightToNextSpaceToken(int idx)
    {
        if (idx >= text.Length) return text.Length;
        int i = idx;
        // skip whitespace
        while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
        // skip token to its end
        while (i < text.Length && !char.IsWhiteSpace(text[i])) i++;
        return Math.Min(text.Length, i);
    }

    private static bool IsLetterChar(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    // ---------------------- mouse -> index mapping -----------------------

    // Maps current mouse position to an index in the text for caret placement.
    // Uses lastDrawBounds (set at Draw time) to determine where the last Render happened.
    private int GetCaretIndexFromMousePosition(Vector2 mousePos)
    {
        // Use the bounds from the most recent Draw call if available.
        Rectangle bounds = lastDrawBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            // fallback: make a small box centered on mouse (defensive)
            bounds = new Rectangle((int)(mousePos.X - 10), (int)(mousePos.Y - 10), 20, 20);
        }

        var font = Raylib.GetFontDefault();
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        Vector2 textPos = new Vector2(bounds.X + Style.TextBoxTextSpacing, bounds.Y + (bounds.Height - fontSize) / 2f - 1f);
        string displayText = IsPassword ? new string(MaskChar, text.Length) : text;

        float localX = mousePos.X - textPos.X;

        // if clicked at or before left edge -> position 0
        if (localX <= 0f) return 0;

        // measure full width for quick right-side test
        Vector2 fullMeasure = Raylib.MeasureTextEx(font, displayText, fontSize, spacing);
        if (localX >= fullMeasure.X) return displayText.Length;

        // iterate by computing prefix widths (accurate with kerning)
        // We compute prefix widths with MeasureTextEx on substrings to match draw exactly.
        // For each index i we compute width(prefix 0..i) and width(prefix 0..i+1) and take midpoint.
        float prevWidth = 0f;
        for (int i = 0; i < displayText.Length; i++)
        {
            // width of prefix ending at i (characters [0..i])
            // We already have prevWidth (width of 0..i-1). To be accurate with kerning we measure both prefix widths.
            string prefix = displayText[..i];
            string prefixNext = displayText[..(i + 1)];
            Vector2 pw = Raylib.MeasureTextEx(font, prefix, fontSize, spacing);
            Vector2 pwn = Raylib.MeasureTextEx(font, prefixNext, fontSize, spacing);
            prevWidth = pw.X;
            float nextWidth = pwn.X;

            float midpoint = prevWidth + (nextWidth - prevWidth) * 0.5f;
            if (localX < midpoint)
            {
                return i;
            }
        }

        // fallback: end of text
        return displayText.Length;
    }
}
