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
    private class InjectedSegment
    {
        public int Index;    // logical index in `text` where the injected content should be drawn BEFORE the char at this index
        public string Content;
    }
    private List<InjectedSegment> injectedSegments = new();

    public MulticastVoidInvocation<UITextInput, string> OnSubmit = new();
    public MulticastVoidInvocation<UITextInput, string> OnInputChanged = new();

    private class InlineHandler
    {
        // Render: draws whatever the token represents, can modify ctx.x and ctx.color.
        // Returns the resulting color (but handlers should also mutate ctx.x / ctx.color directly).
        public Func<UITextInput, string, InlineDrawContext, Color> Render { get; set; }

        // Measure: returns the width in pixels that this token would occupy when rendered.
        // Called during measurement/caret math. Should not draw.
        public Func<UITextInput, string, float, float> Measure { get; set; }
    }
    static readonly Dictionary<char, InlineHandler> INLINE_HANDLERS = new()
    {
        // color token: \c[red] or \c[#FFAABB] etc.
        ['c'] = new InlineHandler
        {
            Render = (self, inner, ctx) =>
            {
                if (!string.IsNullOrWhiteSpace(inner) && TryParseColorToken(inner, out Color parsed))
                    ctx.color = parsed;
                return ctx.color;
            },
            Measure = (self, inner, fs) => 0f // color token consumes no width
        },

        // sprite token: \s[key] -> draw sprite inline and advance ctx.x
        ['s'] = new InlineHandler
        {
            Render = (self, inner, ctx) =>
            {
                if (!string.IsNullOrEmpty(inner))
                {
                    var sprite = RichSpriteRegistry.GetSprite(inner, false);
                    if (sprite != null)
                    {
                        var texture = sprite.Texture;
                        // keep same scale logic you used previously (sprite.BaseSize in relation to fontSize)
                        float spriteHeight = ctx.fontSize;
                        float scale = spriteHeight / texture.Height;

                        // draw with current color tint
                        Raylib.DrawTextureEx(texture, new Vector2(ctx.x, ctx.y), 0, scale, ctx.color);

                        // advance X by sprite width + spacing
                        ctx.x += texture.Width * scale + self.Style.TextBoxTextSpacing;
                    }
                }
                return ctx.color;
            },
            Measure = (self, inner, fontSize) =>
            {
                if (string.IsNullOrEmpty(inner)) return 0f;
                var sprite = RichSpriteRegistry.GetSprite(inner, false);
                if (sprite == null) return 0f;
                var texture = sprite.Texture;
                float spriteHeight = fontSize;
                float scale = spriteHeight / texture.Height;
                return texture.Width * scale + self.Style.TextBoxTextSpacing;
            }
        }
        // add more handlers by inserting new entries here keyed by the token char
    };

    // --- state ---
    private string text = "";
    private int caretIndex = 0;
    private bool hasFocus = false;
    private float caretTimer = 0f;
    private bool caretVisible = true;

    /// <summary>
    /// Whether multi-line mode is currently active for this control.
    /// Starts off false (single-line). Will become true automatically
    /// if the text contains '\n' or when the user activates multiline via Shift+Enter
    /// (provided AllowMultiline is true).
    /// </summary>
    public bool IsMultiline { get; private set; } = false;

    /// <summary>
    /// When <see cref="IsMultiline"/> = <see langword="true"/> this many lines is the minimum amount of lines the box will render as even if the lines arent occupied by characters
    /// </summary>
    public int MinLines { get; set; } = 1;

    /// <summary>
    /// If true, the control allows entering multiline mode via Shift+Enter.
    /// The control still starts single-line; Shift+Enter will insert a newline
    /// and activate multiline. When the text loses all newlines it will revert
    /// back to single-line automatically.
    /// </summary>
    public bool AllowMultiline { get; set; } = true;

    // selection state (flat indices into `text`)
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

    /// <summary>
    /// Inject a render-only string at logicalIndex (0..text.Length).
    /// NOTE: any '\r' or '\n' in content will be replaced with a single space.
    /// </summary>
    /// <param name="logicalIndex"></param>
    /// <param name="content"></param>
    public void InjectStringAt(int logicalIndex, string content)
    {
        if (string.IsNullOrEmpty(content)) return;
        // Avoid multi-line injections for now (keeps mapping simple)
        content = content.Replace("\r", " ").Replace("\n", " ");
        int idx = Math.Clamp(logicalIndex, 0, text.Length);
        injectedSegments.Add(new InjectedSegment { Index = idx, Content = content });

        // keep list sorted by index (stable)
        injectedSegments.Sort((a, b) => a.Index.CompareTo(b.Index));
    }

    void MoveCaretUp()
    {
        // find start of current line
        int lineStart = text.LastIndexOf('\n', Math.Max(caretIndex - 1, 0));
        int prevLineEnd = lineStart > 0 ? text.LastIndexOf('\n', lineStart - 1) : -1;
        int col = caretIndex - (lineStart + 1);
        int prevLineLength = lineStart - prevLineEnd - 1;
        caretIndex = Math.Max(0, prevLineEnd + 1 + Math.Min(col, prevLineLength));
    }

    void MoveCaretDown()
    {
        int lineEnd = text.IndexOf('\n', caretIndex);
        int nextLineEnd = lineEnd >= 0 ? text.IndexOf('\n', lineEnd + 1) : -1;
        int startIndex = Math.Max(caretIndex - 1, 0);
        int lineStart = text.LastIndexOf('\n', startIndex);
        int col = caretIndex - (lineStart + 1);
        int nextLineLength = (nextLineEnd == -1 ? text.Length : nextLineEnd) - lineEnd - 1;
        caretIndex = (lineEnd == -1) ? text.Length : lineEnd + 1 + Math.Min(col, nextLineLength);
    }

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

            if(Input.IsDown(KeyboardKey.Escape))
            {
                hasFocus = false;
                return;
            }

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
                    UpdateMultilineState();
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
                    UpdateMultilineState();
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
                    UpdateMultilineState();
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

            if (Input.IsPressed(KeyboardKey.Up))
            {
                // MoveCaretUp mutates caretIndex to the proper place
                MoveCaretUp();

                if (shift)
                    ExtendSelectionTo(caretIndex);
                else
                    SetCaretAndClearSelection(caretIndex);

                caretVisible = true;
                caretTimer = 0f;
            }

            if (Input.IsPressed(KeyboardKey.Down))
            {
                MoveCaretDown();

                if (shift)
                    ExtendSelectionTo(caretIndex);
                else
                    SetCaretAndClearSelection(caretIndex);

                caretVisible = true;
                caretTimer = 0f;
            }

            // Enter -> submit
            if (Input.IsPressed(KeyboardKey.Enter))
            {
                // `shift` is computed earlier in Update()
                bool shiftHeld = shift;

                if (!IsMultiline)
                {
                    if (AllowMultiline && shiftHeld)
                    {
                        // Activate multiline and insert newline
                        text = text.Insert(caretIndex, "\n");
                        caretIndex++;
                        IsMultiline = true;
                        ClearSelection();
                        UpdateMultilineState();
                        OnInputChanged?.Invoke(this, text);
                    }
                    else
                    {
                        // Single-line submit
                        OnSubmit?.Invoke(this, text);
                    }
                }
                else
                {
                    // Currently multiline:
                    // - plain Enter inserts newline
                    // - Shift+Enter submits
                    if (shiftHeld)
                    {
                        OnSubmit?.Invoke(this, text);
                    }
                    else
                    {
                        text = text.Insert(caretIndex, "\n");
                        caretIndex++;
                        ClearSelection();
                        UpdateMultilineState();
                        OnInputChanged?.Invoke(this, text);
                    }
                }

                caretVisible = true;
                caretTimer = 0f;
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
                    UpdateMultilineState();
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

    private static bool TryParseColorToken(string token, out Color color)
    {
        color = Color.White;
        if (string.IsNullOrWhiteSpace(token)) return false;
        token = token.Trim();

        // hex form: optional leading '#', must be 6 hex digits
        if (token.StartsWith("#")) token = token[1..];
        if (token.Length == 6 && int.TryParse(token, System.Globalization.NumberStyles.HexNumber, null, out int hex))
        {
            byte r = (byte)((hex >> 16) & 0xFF);
            byte g = (byte)((hex >> 8) & 0xFF);
            byte b = (byte)(hex & 0xFF);
            color = new Color(r, g, b, (byte)255);
            return true;
        }

        // named colors (expand if you want more)
        switch (token.ToLowerInvariant())
        {
            case "white": color = Color.White; return true;
            case "black": color = Color.Black; return true;
            case "red": color = Color.Red; return true;
            case "green": color = Color.Green; return true;
            case "blue": color = Color.Blue; return true;
            case "yellow": color = Color.Yellow; return true;
            case "cyan": color = new Color(0, 255, 255); return true;
            case "magenta": color = new Color(255, 0, 255, 255); return true;
            case "gray":
            case "grey": color = new Color(128, 128, 128, 255); return true;
            case "orange": color = new Color(255, 165, 0, 255); return true;
            default: return false;
        }
    }

    private static string RemoveColorTokensForMeasure(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;

        var sb = new System.Text.StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            // detect start of inline token: \x[...]
            if (s[i] == '\\' && i + 2 < s.Length && s[i + 2] == '[')
            {
                char tokenType = s[i + 1];
                // valid token chars: c, C, s, S, x, X, or others if desired
                if (char.IsLetter(tokenType))
                {
                    // skip until closing ']'
                    int j = i + 3;
                    while (j < s.Length && s[j] != ']') j++;

                    // if found ']', skip past it; otherwise, go to end
                    i = (j < s.Length) ? j : s.Length - 1;
                    continue;
                }
            }

            sb.Append(s[i]);
        }

        return sb.ToString();
    }


    private class InlineDrawContext
    {
        public Font font;
        public float fontSize;
        public float spacing;
        public float x;
        public float y;
        public Color color;
    }

    private bool IsInlineTokenStart(string source, int i)
    {
        if (i + 2 >= source.Length) return false;
        if (source[i] != '\\' || source[i + 2] != '[') return false;
        char key = source[i + 1];
        int closing = source.IndexOf(']', i + 3);
        if (closing == -1)
            return false;
        return INLINE_HANDLERS.ContainsKey(key);
    }

    private Color ProcessInjectedContentAtZeroWidth(Font font, float fontSize, float spacing, string content, ref float x, float y, Color currentColor)
    {
        if (string.IsNullOrEmpty(content)) return currentColor;

        int i = 0;
        var sb = new System.Text.StringBuilder();

        InlineDrawContext ctx = new InlineDrawContext
        {
            font = font,
            fontSize = fontSize,
            spacing = spacing,
            x = x,
            y = y,
            color = currentColor
        };

        void FlushAndDrawAccum()
        {
            if (sb.Length == 0) return;
            string run = sb.ToString();
            Raylib.DrawTextEx(ctx.font, run, new Vector2(ctx.x, ctx.y), ctx.fontSize, ctx.spacing, ctx.color);
            Vector2 m = Raylib.MeasureTextEx(ctx.font, run, ctx.fontSize, ctx.spacing);
            ctx.x += m.X;
            sb.Clear();
        }

        while (i < content.Length)
        {
            // inline token like \x[stuff]
            if (content[i] == '\\' && i + 2 < content.Length && content[i + 2] == '[')
            {
                char key = content[i + 1];
                int closing = content.IndexOf(']', i + 3);
                string inner = closing != -1 ? content[(i + 3)..closing] : string.Empty;

                if (INLINE_HANDLERS.TryGetValue(key, out var handler))
                {
                    // draw any accumulated text first
                    FlushAndDrawAccum();

                    // call handler.Render which may draw and update ctx.x / ctx.color
                    ctx.color = handler.Render(this, inner, ctx);

                    // advance i past the token (or to end if no closing)
                    i = closing != -1 ? closing + 1 : content.Length;
                    continue;
                }
                // else fallthrough to treat as literal
            }

            sb.Append(content[i]);
            i++;
        }

        FlushAndDrawAccum();

        // write back advanced x and color
        x = ctx.x;
        return ctx.color;
    }

    private bool TryHandleInlineToken(string source, ref int i, ref float x, ref Color currentColor,
    float y, Font font, float fontSize, float spacing)
    {
        if (i + 2 >= source.Length) return false;
        if (source[i] != '\\' || source[i + 2] != '[') return false;

        char key = source[i + 1];
        if (!INLINE_HANDLERS.TryGetValue(key, out var handler))
            return false;

        int closing = source.IndexOf(']', i + 3);
        // If there's no closing bracket, we still allow typing to show the literal until ']' is entered.
        string inner = closing != -1 ? source[(i + 3)..closing] : source[(i + 3)..]; // rest-of-string if no closing

        InlineDrawContext ctx = new InlineDrawContext
        {
            font = font,
            fontSize = fontSize,
            spacing = spacing,
            x = x,
            y = y,
            color = currentColor
        };

        // call render; handler is responsible for deciding what to draw for unterminated tokens (we pass inner as-is)
        currentColor = handler.Render(this, inner, ctx);
        x = ctx.x;

        // advance i: if no closing, consume everything to end (so typed sequences are visible while typing)
        i = closing != -1 ? closing + 1 : source.Length;
        return true;
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

        var font = Raylib.GetFontDefault();
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        float lineHeight = fontSize + spacing;

        // canonical logical text (may contain color tokens, but measurement will strip)
        string logical = text ?? "";

        // masked logical used for rendering/password
        string renderLogical = IsPassword
            ? new string((logical ?? "").Select(ch => ch == '\n' ? '\n' : MaskChar).ToArray())
            : logical;

        // measurement string: remove color tokens (so tokens don't affect measure/caret/selection)
        string measurementText = RemoveColorTokensForMeasure(renderLogical);

        // split per-line (tokens/injections do not introduce '\n')
        string[] logicalRenderLines = renderLogical.Split('\n');    // used for visible rendering and token parsing
        string[] logicalMeasureLines = measurementText.Split('\n'); // used for measurement & caret/selection

        Vector2 textPos;
        if (!IsMultiline || logicalRenderLines.Length == 1)
        {
            textPos = new Vector2(bounds.X + Style.TextBoxTextSpacing, bounds.Y + (bounds.Height - fontSize) / 2f - 1f);
        }
        else
        {
            textPos = new Vector2(bounds.X + Style.TextBoxTextSpacing, bounds.Y + Style.TextBoxTextSpacing);
        }

        ScissorStack.Push(bounds);

        // placeholder
        if (string.IsNullOrEmpty(logical) && !string.IsNullOrEmpty(Placeholder) && !hasFocus)
        {
            Raylib.DrawTextEx(font, Placeholder, textPos, fontSize, spacing, placeholderColor);
            ScissorStack.Pop();
            return;
        }

        // Prepare injectedSegments sorted (by Index)
        injectedSegments.Sort((a, b) => a.Index.CompareTo(b.Index));

        // iterate lines left->right, processing tokens and injections in-stream
        float y = textPos.Y;
        int globalLogicalBase = 0; // index into logical (renderLogical) for start of line

        int linesCount = Math.Min(logicalRenderLines.Length, logicalMeasureLines.Length);
        for (int li = 0; li < linesCount; li++)
        {
            string renderLine = logicalRenderLines[li];   // may contain \c[...] tokens
            string measureLine = logicalMeasureLines[li]; // token-stripped, used for measurement

            float x = textPos.X;
            Color curColor = Style.TextBoxText; // start color for this line

            int localIdx = 0; // index within renderLine (this walks raw characters including tokens)
            int measurePos = 0; // count of visible characters seen so far on this line (for mapping)
                                // maintain a quick pointer into injectedSegments
            int segPointer = 0;
            while (segPointer < injectedSegments.Count && injectedSegments[segPointer].Index < globalLogicalBase)
                segPointer++;

            // Stream through renderLine
            while (localIdx < renderLine.Length)
            {
                int globalIdx = globalLogicalBase + localIdx;

                // If there are injected segments at this global index, process them BEFORE the char at localIdx
                while (segPointer < injectedSegments.Count && injectedSegments[segPointer].Index == globalIdx)
                {
                    var seg = injectedSegments[segPointer];
                    // draw injection at zero width: compute prefix-based X (x already matches)
                    // injection may contain \c[...] tokens which should update curColor for subsequent text
                    curColor = ProcessInjectedContentAtZeroWidth(font, fontSize, spacing, seg.Content, ref x, y, curColor);

                    segPointer++;
                }

                // If token start (\x[...]), process via shared handler
                if (TryHandleInlineToken(renderLine, ref localIdx, ref x, ref curColor, y, font, fontSize, spacing))
                    continue;

                // Normal visible char: accumulate a run until next token or injection or EOL
                int runStart = localIdx;
                while (localIdx < renderLine.Length)
                {
                    int gl = globalLogicalBase + localIdx;

                    // break if an injection starts here
                    bool injectionAtHere = segPointer < injectedSegments.Count && injectedSegments[segPointer].Index == gl;
                    if (injectionAtHere)
                        break;

                    // break if the current position starts a known inline token (\x[...])
                    if (IsInlineTokenStart(renderLine, localIdx))
                        break;

                    localIdx++;
                }

                // draw the run [runStart..localIdx) as visible text with current color
                if (localIdx > runStart)
                {
                    string run = renderLine[runStart..localIdx];

                    // For measurement we must draw only the visible glyphs — but run may include escape sequences? We already guarded runs to stop at token start.
                    // Measure run and draw
                    Vector2 measure = Raylib.MeasureTextEx(font, run, fontSize, spacing);
                    Raylib.DrawTextEx(font, run, new Vector2(x, y), fontSize, spacing, curColor);
                    x += measure.X;

                    // advance measurePos by number of visible chars - but run contains only visible chars so:
                    measurePos += run.Length;
                }
            } // end while localIdx

            // Any injections after end-of-line (index == line end) should be processed
            while (segPointer < injectedSegments.Count && injectedSegments[segPointer].Index == globalLogicalBase + renderLine.Length)
            {
                var seg = injectedSegments[segPointer];
                curColor = ProcessInjectedContentAtZeroWidth(font, fontSize, spacing, seg.Content, ref x, y, curColor);
                segPointer++;
            }

            // Selection highlight for the logical line (use measureLine for measuring positions)
            if (HasSelection())
            {
                int logicalLineStart = globalLogicalBase;
                int logicalLineEnd = globalLogicalBase + measureLine.Length; // exclusive

                int s0 = Math.Clamp(selStart, logicalLineStart, logicalLineEnd);
                int s1 = Math.Clamp(selEnd, logicalLineStart, logicalLineEnd);

                if (s0 < s1)
                {
                    int localStart = s0 - logicalLineStart;
                    int localEnd = s1 - logicalLineStart;
                    string left = measureLine.Length >= localStart ? measureLine[..localStart] : "";
                    string sel = measureLine.Substring(localStart, Math.Max(0, Math.Min(localEnd - localStart, measureLine.Length - localStart)));

                    Vector2 leftMeasure = Raylib.MeasureTextEx(font, left, fontSize, spacing);
                    Vector2 selMeasure = Raylib.MeasureTextEx(font, sel, fontSize, spacing);

                    float selX = textPos.X + leftMeasure.X;
                    float selY = y - 2f;
                    float selH = fontSize + 4f;

                    Color selBg = new Color(80, 120, 200, 160);
                    Raylib.DrawRectangleRec(new Rectangle(selX, selY, selMeasure.X, selH), selBg);

                    // draw selected substring on top in white
                    Raylib.DrawTextEx(font, sel, new Vector2(selX, y), fontSize, spacing, Color.White);
                }
            }

            // advance to next line
            globalLogicalBase += measureLine.Length + 1; // +1 for '\n'
            y += lineHeight;
        }

        // draw caret measured against the token-stripped measurement lines (injected segments ignored)
        if (hasFocus && caretVisible)
        {
            string[] renderLines = renderLogical.Split('\n');    // contains tokens
                                                                 // find caret line using the actual render lines lengths (logical index is into text)
            int caretLine = 0;
            int baseIdx = 0;
            bool foundLine = false;
            for (int cli = 0; cli < renderLines.Length; cli++)
            {
                int len = renderLines[cli].Length;
                if (caretIndex >= baseIdx && caretIndex <= baseIdx + len)
                {
                    caretLine = cli;
                    foundLine = true;
                    break;
                }
                baseIdx += len + 1;
            }
            if (!foundLine)
            {
                caretLine = Math.Max(0, renderLines.Length - 1);
                baseIdx = Enumerable.Range(0, caretLine).Select(ii => renderLines[ii].Length + 1).Sum();
            }

            string renderLine = caretLine < renderLines.Length ? renderLines[caretLine] : "";
            int caretCharPos = Math.Clamp(caretIndex - baseIdx, 0, renderLine.Length);

            // prepare injectedSegments pointer for this line (they draw BEFORE the char at their Index)
            int segPointer = 0;
            while (segPointer < injectedSegments.Count && injectedSegments[segPointer].Index < baseIdx)
                segPointer++;

            // walk the renderLine and accumulate visible width up to caretCharPos
            float xAccum = 0f;
            int i = 0;
            while (i < renderLine.Length && i < caretCharPos)
            {
                int globalIdx = baseIdx + i;

                // process any injected segments that are at this global position (they occupy visual width)
                while (segPointer < injectedSegments.Count && injectedSegments[segPointer].Index == globalIdx)
                {
                    float iw = MeasureInjectedContentWidth(font, fontSize, spacing, injectedSegments[segPointer].Content);
                    xAccum += iw;
                    segPointer++;
                }

                // if an inline token starts here, handle it
                if (IsInlineTokenStart(renderLine, i))
                {
                    int close = renderLine.IndexOf(']', i + 3);
                    if (close == -1)
                    {
                        // unterminated token -> treat the remaining chars as plain until caretPos
                        int plainEnd = Math.Min(caretCharPos, renderLine.Length);
                        string plain = renderLine[i..plainEnd];
                        Vector2 m = Raylib.MeasureTextEx(font, plain, fontSize, spacing);
                        xAccum += m.X;
                        break;
                    }

                    char key = renderLine[i + 1];
                    int contentStart = i + 3;
                    int contentLen = Math.Max(0, close - contentStart);

                    // sequence entirely before caret -> add full measured width
                    if (close + 1 <= caretCharPos)
                    {
                        if (contentLen > 0 && INLINE_HANDLERS.TryGetValue(key, out var h))
                        {
                            string content = renderLine.Substring(contentStart, contentLen);
                            xAccum += h.Measure(this, content, fontSize);
                        }
                        // advance past token
                        i = close + 1;
                        continue;
                    }

                    // caret is inside this sequence -> measure only the portion before caret
                    if (contentStart < caretCharPos && caretCharPos <= close)
                    {
                        int charsBefore = Math.Clamp(caretCharPos - contentStart, 0, contentLen);
                        if (charsBefore > 0 && INLINE_HANDLERS.TryGetValue(key, out var h2))
                        {
                            string part = renderLine.Substring(contentStart, charsBefore);
                            xAccum += h2.Measure(this, part, fontSize);
                        }
                        break; // we've reached the caret
                    }

                    // token starts at/after caret -> nothing more to add
                    break;
                }

                // plain-character run: measure up to either next token or the caretCharPos
                int runStart = i;
                while (i < renderLine.Length && i < caretCharPos && !IsInlineTokenStart(renderLine, i))
                    i++;
                if (i > runStart)
                {
                    string run = renderLine[runStart..i];
                    Vector2 mm = Raylib.MeasureTextEx(font, run, fontSize, spacing);
                    xAccum += mm.X;
                }
            }

            // If caret is at end-of-line, ensure we include injections that sit at that index
            int endGlobal = baseIdx + renderLine.Length;
            if (caretCharPos == renderLine.Length)
            {
                while (segPointer < injectedSegments.Count && injectedSegments[segPointer].Index == endGlobal)
                {
                    float iw = MeasureInjectedContentWidth(font, fontSize, spacing, injectedSegments[segPointer].Content);
                    xAccum += iw;
                    segPointer++;
                }
            }

            float caretX = textPos.X + xAccum;
            float caretY1;
            if (!IsMultiline || renderLines.Length == 1)
            {
                caretY1 = bounds.Y + (bounds.Height - fontSize) / 2f + 2f;
            }
            else
            {
                caretY1 = textPos.Y + caretLine * lineHeight + 2f;
            }
            float caretY2 = caretY1 + fontSize - 4f;
            ray.DrawLineEx(new Vector2(caretX, caretY1), new Vector2(caretX, caretY2), Style.CaretWidth, Style.Caret);
        }

        ScissorStack.Pop();
    }

    protected internal override float GetHeight(float maxWidth)
    {
        var font = Raylib.GetFontDefault();
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        float lineHeight = fontSize + spacing;

        if (!IsMultiline)
        {
            Vector2 measured = Raylib.MeasureTextEx(font, "Ay", fontSize, spacing);
            return MathF.Max(Style.TextBoxMinHeight, measured.Y + Style.TextBoxTextSpacing * 2f);
        }

        // multiline: number of logical lines determines height, but respect MinLines
        int linesCount = string.IsNullOrEmpty(text) ? 1 : text.Split('\n').Length;
        linesCount = Math.Max(linesCount, Math.Max(1, MinLines));
        float height = MathF.Max(Style.TextBoxMinHeight, linesCount * lineHeight + Style.TextBoxTextSpacing * 2f);
        return height;
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        hasFocus = false;
        isSelectingWithMouse = false;
        ClearSelection();
    }

    private void UpdateMultilineState()
    {
        if (!AllowMultiline)
        {
            IsMultiline = false;
            return;
        }

        // If the text contains any newline, we're in multiline state.
        // Otherwise go back to single-line mode until user activates with Shift+Enter.
        IsMultiline = text.Contains('\n');
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
        UpdateMultilineState();
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
        UpdateMultilineState();
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

            // skip backwards over inline sequence if we landed inside one
            SkipOverInlineLeft(ref target);

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

            // skip forwards over inline sequence if we start at a sequence
            SkipOverInlineRight(ref target);

            SetCaretAndClearSelection(target);
        }

    }

    private void SkipOverInlineLeft(ref int index)
    {
        // find if there’s a sequence *ending* just before or overlapping index
        int seqStart, seqEnd;
        for (int i = Math.Max(0, index - 1); i >= 0; i--)
        {
            if (TryGetInlineSequenceAt(text, i, out seqStart, out seqEnd))
            {
                if (index > seqStart && index <= seqEnd)
                {
                    // jump left to before the sequence
                    index = seqStart;
                }
                break;
            }
        }
    }

    private void SkipOverInlineRight(ref int index)
    {
        int seqStart, seqEnd;
        if (TryGetInlineSequenceAt(text, index, out seqStart, out seqEnd))
        {
            // jump right to after the sequence
            index = seqEnd;
        }
    }

    private bool TryGetInlineSequenceAt(string line, int index, out int start, out int end)
    {
        start = -1;
        end = -1;

        if (!IsInlineTokenStart(line, index))
            return false;

        int close = line.IndexOf(']', index);
        if (close == -1)
            return false;

        start = index;
        end = close + 1; // exclusive
        return true;
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

    private int GetCaretIndexFromMousePosition(Vector2 mousePos)
    {
        Rectangle bounds = lastDrawBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            bounds = new Rectangle((int)(mousePos.X - 10), (int)(mousePos.Y - 10), 20, 20);
        }

        var font = Raylib.GetFontDefault();
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        float lineHeight = fontSize + spacing;

        // Use the actual render stream for hit-testing (tokens included)
        string renderText;
        if (IsPassword)
        {
            var chars = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
                chars[i] = text[i] == '\n' ? '\n' : MaskChar;
            renderText = new string(chars);
        }
        else
        {
            renderText = text ?? "";
        }

        string[] renderLines = renderText.Split('\n');

        Vector2 textPos;
        if (!IsMultiline || renderLines.Length == 1)
        {
            textPos = new Vector2(bounds.X + Style.TextBoxTextSpacing, bounds.Y + (bounds.Height - fontSize) / 2f - 1f);
        }
        else
        {
            textPos = new Vector2(bounds.X + Style.TextBoxTextSpacing, bounds.Y + Style.TextBoxTextSpacing);
        }

        // determine clicked line
        float localY = mousePos.Y - textPos.Y;
        int lineIndex = 0;
        if (IsMultiline && renderLines.Length > 1)
        {
            if (localY <= 0f) lineIndex = 0;
            else lineIndex = Math.Clamp((int)(localY / lineHeight), 0, renderLines.Length - 1);
        }
        else
        {
            lineIndex = 0;
        }

        string line = renderLines[Math.Clamp(lineIndex, 0, renderLines.Length - 1)];
        float localX = mousePos.X - textPos.X;

        // compute base index (start index of the selected line in the full text)
        int baseIndex = 0;
        for (int i = 0; i < lineIndex; i++)
            baseIndex += renderLines[i].Length + 1; // +1 for '\n'

        // quick bounds: left edge and right of line
        if (localX <= 0f) return baseIndex;

        // measure full width of this line by walking it (token-aware + injections)
        float fullW = 0f;
        {
            int j = 0;
            int segPointerFull = 0;
            while (segPointerFull < injectedSegments.Count && injectedSegments[segPointerFull].Index < baseIndex)
                segPointerFull++;

            while (j < line.Length)
            {
                int globalIdx = baseIndex + j;

                // account for injections at this position
                while (segPointerFull < injectedSegments.Count && injectedSegments[segPointerFull].Index == globalIdx)
                {
                    fullW += MeasureInjectedContentWidth(font, fontSize, spacing, injectedSegments[segPointerFull].Content);
                    segPointerFull++;
                }

                if (IsInlineTokenStart(line, j))
                {
                    int close = line.IndexOf(']', j + 3);
                    if (close == -1)
                    {
                        string rest = line[j..];
                        fullW += Raylib.MeasureTextEx(font, rest, fontSize, spacing).X;
                        break;
                    }

                    char key = line[j + 1];
                    int contentStart = j + 3;
                    int contentLen = Math.Max(0, close - contentStart);
                    if (contentLen > 0 && INLINE_HANDLERS.TryGetValue(key, out var h))
                        fullW += h.Measure(this, line.Substring(contentStart, contentLen), fontSize);
                    j = close + 1;
                    continue;
                }

                int runStart = j;
                while (j < line.Length && !IsInlineTokenStart(line, j)) j++;
                string run = line[runStart..j];
                fullW += Raylib.MeasureTextEx(font, run, fontSize, spacing).X;
            }

            // injections after line end
            int endGlobal = baseIndex + line.Length;
            while (segPointerFull < injectedSegments.Count && injectedSegments[segPointerFull].Index == endGlobal)
            {
                fullW += MeasureInjectedContentWidth(font, fontSize, spacing, injectedSegments[segPointerFull].Content);
                segPointerFull++;
            }
        }

        if (localX >= fullW) return baseIndex + line.Length;

        // walk the line and return index at the point where mouse hits midpoint (token-aware + injections)
        float xAccum = 0f;
        int localIdx = 0;
        int segPointer = 0;
        while (segPointer < injectedSegments.Count && injectedSegments[segPointer].Index < baseIndex)
            segPointer++;

        while (localIdx <= line.Length)
        {
            int globalIdx = baseIndex + localIdx;

            // handle injected segment at this global idx: it's a visual unit we can't place caret inside.
            while (segPointer < injectedSegments.Count && injectedSegments[segPointer].Index == globalIdx)
            {
                float iw = MeasureInjectedContentWidth(font, fontSize, spacing, injectedSegments[segPointer].Content);
                float midpoint = xAccum + iw * 0.5f;
                if (localX < midpoint)
                    return globalIdx; // caret before/at this injected content
                                      // else consume injected width and continue (caret considered after injection)
                xAccum += iw;
                segPointer++;
            }

            // if at end of line: decide whether to return this index
            if (localIdx == line.Length)
                return globalIdx;

            // if token starts here, compute token width and decide hit
            if (IsInlineTokenStart(line, localIdx))
            {
                int close = line.IndexOf(']', localIdx + 3);
                if (close == -1)
                {
                    string rest = line[localIdx..];
                    Vector2 mrest = Raylib.MeasureTextEx(font, rest, fontSize, spacing);
                    float nextX = xAccum + mrest.X;
                    float midpoint = xAccum + (nextX - xAccum) * 0.5f;
                    if (localX < midpoint) return globalIdx;
                    return baseIndex + line.Length;
                }

                char key = line[localIdx + 1];
                int contentStart = localIdx + 3;
                int contentLen = Math.Max(0, close - contentStart);
                float tokenW = 0f;
                if (contentLen > 0 && INLINE_HANDLERS.TryGetValue(key, out var handler))
                    tokenW = handler.Measure(this, line.Substring(contentStart, contentLen), fontSize);

                float nextXToken = xAccum + tokenW;
                float midpointToken = xAccum + (nextXToken - xAccum) * 0.5f;
                if (localX < midpointToken) return globalIdx;
                xAccum = nextXToken;
                localIdx = close + 1;
                continue;
            }

            // plain char: measure one char at a time (so caret can land between characters)
            string ch = line.Substring(localIdx, 1);
            float chW = Raylib.MeasureTextEx(font, ch, fontSize, spacing).X;
            float midpointChar = xAccum + chW * 0.5f;
            if (localX < midpointChar)
                return baseIndex + localIdx;
            // else advance past the character
            xAccum += chW;
            localIdx++;
        }

        // fallback
        return baseIndex + line.Length;
    }
    private float MeasureInjectedContentWidth(Font font, float fontSize, float spacing, string content)
    {
        if (string.IsNullOrEmpty(content)) return 0f;
        float x = 0f;
        int i = 0;
        while (i < content.Length)
        {
            // inline token like \x[stuff]
            if (content[i] == '\\' && i + 2 < content.Length && content[i + 2] == '[')
            {
                char key = content[i + 1];
                int closing = content.IndexOf(']', i + 3);
                string inner = closing != -1 ? content[(i + 3)..closing] : content[(i + 3)..];

                if (!string.IsNullOrEmpty(inner) && INLINE_HANDLERS.TryGetValue(key, out var handler))
                {
                    x += handler.Measure(this, inner, fontSize);
                }
                else if (closing == -1)
                {
                    // unterminated: fall through and measure remaining as plain
                    string rest = content[i..];
                    x += Raylib.MeasureTextEx(font, rest, fontSize, spacing).X;
                    break;
                }
                // advance i past token (or end)
                i = closing != -1 ? closing + 1 : content.Length;
                continue;
            }

            // plain character
            int runStart = i;
            while (i < content.Length && !(content[i] == '\\' && i + 2 < content.Length && content[i + 2] == '['))
                i++;
            string run = content[runStart..i];
            x += Raylib.MeasureTextEx(font, run, fontSize, spacing).X;
        }

        return x;
    }

}
