using Raylib_cs;
using System;
using System.Numerics;
using System.Globalization;
using WinterRose.ForgeSignal;

namespace WinterRose.ForgeWarden.UserInterface;

public class UIColorPicker : UIContent
{
    public MulticastVoidInvocation<IUIContainer, UIColorPicker, Color> OnColorChanged { get; set; } = new();
    public MulticastVoidInvocation<Color> OnColorChangedBasic { get; set; } = new();

    // Layout
    private const float CONTROL_HEIGHT = 240f;
    private const float PADDING = 8f;
    private const float SLIDER_WIDTH = 26f;
    private const float TOGGLE_HEIGHT = 22f;
    private const float WHEEL_PAD = 6f;

    // wheel tessellation quality (tuneable)
    private const int HUE_STEPS = 120;     // angular slices
    private const int RADIAL_STEPS = 40;   // radial rings

    // Internal state (h: 0..360, s:0..1, l/v:0..1)
    private float hue = 0f;
    private float sat = 1f;
    private float bright = 0.5f; // L if HSL, V if RGB/HSV

    // rects for hit testing / layout
    private Rectangle lastBounds = new Rectangle();
    private Rectangle wheelRect = new Rectangle();
    private Rectangle sliderRect = new Rectangle();
    private Rectangle previewRect = new Rectangle();

    // inline text inputs (we render/update them manually)
    private UITextInput rInput;
    private UITextInput gInput;
    private UITextInput bInput;
    private UITextInput hexInput;

    // rectangles for those fields (used for focus/hit testing & layout)
    private Rectangle rFieldRect = new Rectangle();
    private Rectangle gFieldRect = new Rectangle();
    private Rectangle bFieldRect = new Rectangle();
    private Rectangle hexFieldRect = new Rectangle();

    // interaction state
    private bool isDraggingWheel = false;
    private bool isDraggingSlider = false;
    private bool isHoveringWheel = false;
    private bool isHoveringSlider = false;
    private bool isHoveringToggle = false;

    // ctor
    public UIColorPicker()
    {
        // default color: hue 0, sat 1, bright 0.5 (red)
        hue = 0f; sat = 1f; bright = 0.5f;
    }

    // API to get/set selected color via Raylib Color
    public Color SelectedColor
    {
        get
        {
                var (r, g, b) = HsvToRgb(hue, sat, bright);
                return new Color((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)255);
        }
        set
        {
            // convert incoming color to hue/sat/bright (use HSV as a default mapping)
            var (h, s, v) = RgbToHsv(value.R / 255f, value.G / 255f, value.B / 255f);
            hue = h; sat = s; bright = v;
            FireColorChanged();
        }
    }

    // UI lifecycle ---------------------------------------------------------
    public override Vector2 GetSize(Rectangle availableArea) => new Vector2(availableArea.Width, CONTROL_HEIGHT);
    protected internal override float GetHeight(float maxWidth) => CONTROL_HEIGHT;

    protected internal override void Setup()
    {
        rInput = new UITextInput() { Placeholder = "R", IsPassword = false, Owner = Owner };
        gInput = new UITextInput() { Placeholder = "G", IsPassword = false, Owner = Owner };
        bInput = new UITextInput() { Placeholder = "B", IsPassword = false, Owner = Owner };
        hexInput = new UITextInput() { Placeholder = "#RRGGBB", IsPassword = false, Owner = Owner };

        rInput.OnSubmit += (ti, s) =>
        {
            if (int.TryParse(s, out int vr))
            {
                vr = Math.Clamp(vr, 0, 255);
                var c = SelectedColor;
                SelectedColor = new Color((byte)vr, c.G, c.B, (byte)255);
            }
            ti.Unfocus();
        };
        gInput.OnSubmit += (ti, s) =>
        {
            if (int.TryParse(s, out int vg))
            {
                vg = Math.Clamp(vg, 0, 255);
                var c = SelectedColor;
                SelectedColor = new Color(c.R, (byte)vg, c.B, (byte)255);
            }
            ti.Unfocus();
        };
        bInput.OnSubmit += (ti, s) =>
        {
            if (int.TryParse(s, out int vb))
            {
                vb = Math.Clamp(vb, 0, 255);
                var c = SelectedColor;
                SelectedColor = new Color(c.R, c.G, (byte)vb, (byte)255);
            }
            ti.Unfocus();
        };
        hexInput.OnSubmit += (ti, s) =>
        {
            string txt = (s ?? "").Trim();
            if (txt.StartsWith("#")) txt = txt[1..];
            if (txt.Length == 6 && int.TryParse(txt, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexVal))
            {
                byte rr = (byte)((hexVal >> 16) & 0xFF);
                byte gg = (byte)((hexVal >> 8) & 0xFF);
                byte bb = (byte)(hexVal & 0xFF);
                SelectedColor = new Color(rr, gg, bb, (byte)255);
            }
            ti.Unfocus();
        };

        // seed inputs with current color
        var c0 = SelectedColor;
        rInput.SetText(c0.R.ToString());
        gInput.SetText(c0.G.ToString());
        bInput.SetText(c0.B.ToString());
        hexInput.SetText($"#{c0.R:X2}{c0.G:X2}{c0.B:X2}");
    }

    protected internal override void Update()
    {
        // Let the inline text inputs run their update logic (caret blinking, key handling while focused)
        rInput?.UpdateInline();
        gInput?.UpdateInline();
        bInput?.UpdateInline();
        hexInput?.UpdateInline();

        // handle dragging states via Input provider
        var mp = Input.Provider.MousePosition;

        // update hover booleans for visual feedback
        isHoveringWheel = Raylib.CheckCollisionPointRec(mp, wheelRect);
        isHoveringSlider = Raylib.CheckCollisionPointRec(mp, sliderRect);

        // handle dragging release
        if (isDraggingWheel && !Input.IsDown(MouseButton.Left))
        {
            isDraggingWheel = false;
        }
        if (isDraggingSlider && !Input.IsDown(MouseButton.Left))
        {
            isDraggingSlider = false;
        }

        // if dragging, update values live
        if (isDraggingWheel)
        {
            UpdateWheelFromPoint(mp.X, mp.Y, commit: true);
        }
        else if (isDraggingSlider)
        {
            UpdateSliderFromPoint(mp.X, mp.Y, commit: true);
        }
    }

    protected override void Draw(Rectangle bounds)
    {
        lastBounds = bounds;

        // main area beneath toggle
        float innerY = bounds.Y;
        float innerH = bounds.Y + bounds.Height - innerY - PADDING;
        float innerW = bounds.Width - PADDING * 2;

        // wheel area on left (square)
        float wheelSize = Math.Min(innerH, innerW - SLIDER_WIDTH - 16);
        float wheelX = bounds.X + PADDING;
        float wheelY = innerY + (innerH - wheelSize) / 2f;
        wheelRect = new Rectangle(wheelX, wheelY, wheelSize, wheelSize);

        DrawColorWheel(wheelRect);

        // slider on right (unchanged)
        float sliderX = wheelRect.X + wheelRect.Width + 12;
        float sliderY = innerY;
        float sliderH = innerH;
        sliderRect = new Rectangle(sliderX, sliderY, SLIDER_WIDTH, sliderH);

        // draw brightness slider
        DrawBrightnessSlider(sliderRect);

        // draw slider knob
        float knobY = SliderPosForValue(bright);
        Vector2 kcenter = new Vector2(sliderRect.X + sliderRect.Width / 2f, knobY);
        Raylib.DrawCircle((int)kcenter.X, (int)kcenter.Y, 6f, Style.ButtonBackground);
        Raylib.DrawCircleLines((int)kcenter.X, (int)kcenter.Y, 6f, Style.ButtonBorder);
        if (isHoveringSlider || isDraggingSlider)
        {
            Raylib.DrawCircleLines((int)kcenter.X, (int)kcenter.Y, 10f, isDraggingSlider ? Style.ButtonClick : Style.ButtonHover);
        }

        // --- compute input field positions first (these remain where you had them) ---
        // place input column to the right of the slider (same as before, but no dependency on previewRect)
        float fieldX = sliderRect.X + sliderRect.Width + 10f; // to the right of slider
        float fieldW = 56f;
        float fieldH = 22f;
        // this Y matches the previous visual offset (previewRect.Y - 75 previously)
        float fieldY = sliderRect.Y + sliderRect.Height - 36 - 75f;

        rFieldRect = new Rectangle(fieldX, fieldY, fieldW, fieldH);
        gFieldRect = new Rectangle(fieldX, fieldY + (fieldH + 6f), fieldW, fieldH);
        bFieldRect = new Rectangle(fieldX, fieldY + 2f * (fieldH + 6f), fieldW, fieldH);
        hexFieldRect = new Rectangle(fieldX, fieldY + 3f * (fieldH + 6f), fieldW + 25f, fieldH);

        // --- compute preview so it sits directly above the red (R) input box ---
        float previewW = Math.Min(80f, bounds.Width - sliderRect.Width - SLIDER_WIDTH);
        float previewH = 28f;
        float previewX = rFieldRect.X; // align with the R field (above it)
        float previewY = rFieldRect.Y - previewH - 6f; // small gap above R field
        previewRect = new Rectangle(previewX, previewY, previewW, previewH);

        // Defensive: ensure preview isn't drawn above the control top
        if (previewRect.Y < sliderRect.Y)
        {
            previewRect.Y = sliderRect.Y;
        }

        // draw preview (now positioned above the R field)
        Color col = SelectedColor;
        ray.DrawRectangleRec(previewRect, col);
        ray.DrawRectangleLinesEx(previewRect, 1f, Style.ButtonBorder);

        // RenderInline will draw the TextInput in the provided rectangle (inputs remain at their computed positions)
        rInput.RenderInline(rFieldRect);
        gInput.RenderInline(gFieldRect);
        bInput.RenderInline(bFieldRect);
        hexInput.RenderInline(hexFieldRect);

        // draw wheel marker for the current hue/sat
        DrawWheelMarker(wheelRect, hue, sat);
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        if (button != MouseButton.Left) return;
        var mp = Input.Provider.MousePosition;

        // Text field clicks: focus the appropriate inline TextInput
        if (Raylib.CheckCollisionPointRec(mp, rFieldRect))
            rInput.Focus();
        else
            rInput.Unfocus();

        if (Raylib.CheckCollisionPointRec(mp, gFieldRect))
            gInput.Focus();
        else
            gInput.Unfocus();

        if (Raylib.CheckCollisionPointRec(mp, bFieldRect))
            bInput.Focus();
        else
            bInput.Unfocus();

        if (Raylib.CheckCollisionPointRec(mp, hexFieldRect))
            hexInput.Focus();
        else
            hexInput.Unfocus();

        if (Raylib.CheckCollisionPointRec(mp, wheelRect))
        {
            isDraggingWheel = true;
            UpdateWheelFromPoint(mp.X, mp.Y, commit: true);
            return;
        }

        if (Raylib.CheckCollisionPointRec(mp, sliderRect))
        {
            isDraggingSlider = true;
            UpdateSliderFromPoint(mp.X, mp.Y, commit: true);
            return;
        }
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        isDraggingWheel = false;
        isDraggingSlider = false;

        rInput?.Unfocus();
        gInput?.Unfocus();
        bInput?.Unfocus();
        hexInput?.Unfocus();
    }

    private void DrawColorWheel(Rectangle rect)
    {
        float cx = rect.X + rect.Width / 2f;
        float cy = rect.Y + rect.Height / 2f;
        int radius = (int)(rect.Width / 2f);

        int radialStep = 3;
        int angSteps = Math.Max(48, HUE_STEPS);

        for (int r = 0; r <= radius; r += radialStep)
        {
            float s = radius > 0 ? (float)r / radius : 0f;
            for (int a = 0; a < angSteps; a++)
            {
                float ang = a * (2.0f * MathF.PI) / angSteps - MathF.PI / 2.0f;
                float x = cx + MathF.Cos(ang) * r;
                float y = cy + MathF.Sin(ang) * r;

                float hueDeg = (float)(a * 360.0 / angSteps);
                Color col = HsvToRgbColor(hueDeg, s, 1f);

                Raylib.DrawRectangle((int)x, (int)y, radialStep, radialStep, col);
            }
        }

        Raylib.DrawCircle((int)cx, (int)cy, Math.Max(2, radius / 40), Color.White);
    }

    private void DrawBrightnessSlider(Rectangle rect)
    {
        int steps = 80;
        for (int i = 0; i < steps; i++)
        {
            float t0 = i / (float)steps;
            float t1 = (i + 1) / (float)steps;
            float y0 = rect.Y + t0 * rect.Height;
            float y1 = rect.Y + t1 * rect.Height;

            float val = 1f - (t0 + t1) * 0.5f;
            Color c = HsvToRgbColor(hue, sat, val);
            Raylib.DrawRectangleRec(new Rectangle(rect.X, y0, rect.Width, y1 - y0 + 1), c);
        }

        // slider border
        ray.DrawRectangleLinesEx(rect, 1f, Style.TextBoxBorder);
    }

    private void DrawWheelMarker(Rectangle rect, float h, float s)
    {
        float cx = rect.X + rect.Width / 2f;
        float cy = rect.Y + rect.Height / 2f;
        float radius = rect.Width / 2f;

        float angle = (h / 360f) * (float)(2 * Math.PI) - MathF.PI / 2f;
        float r = s * radius;
        Vector2 pos = new Vector2(cx + MathF.Cos(angle) * r, cy + MathF.Sin(angle) * r);

        Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, 8, Style.ButtonBorder);
        Raylib.DrawCircle((int)pos.X, (int)pos.Y, 4, Color.White);
    }

    // interaction helpers -------------------------------------------------
    private void UpdateWheelFromPoint(float px, float py, bool commit)
    {
        float cx = wheelRect.X + wheelRect.Width / 2f;
        float cy = wheelRect.Y + wheelRect.Height / 2f;
        float dx = px - cx;
        float dy = py - cy;
        float dist = MathF.Sqrt(dx * dx + dy * dy);
        float radius = wheelRect.Width / 2f;

        // clamp dist
        dist = MathF.Min(dist, radius);

        // compute hue from angle (0..360)
        double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
        double shifted = (angle + 90.0 + 360.0) % 360.0;
        hue = (float)shifted;

        // saturation from radius (0..1)
        sat = radius > 0 ? (dist / radius) : 0f;
        sat = Math.Clamp(sat, 0f, 1f);

        if (commit) FireColorChanged();
    }

    private void UpdateSliderFromPoint(float px, float py, bool commit)
    {
        // slider vertical: top = 1, bottom = 0
        float t = (py - sliderRect.Y) / sliderRect.Height;
        float v = 1f - t;
        bright = Math.Clamp(v, 0f, 1f);

        if (commit) FireColorChanged();
    }

    private float SliderPosForValue(float val)
    {
        // convert value (0..1) to y coordinate
        float y = sliderRect.Y + (1f - val) * sliderRect.Height;
        return y;
    }

    private void FireColorChanged()
    {
        // ensure inputs reflect canonical color values
        var c = SelectedColor;
        rInput?.SetText(c.R.ToString());
        gInput?.SetText(c.G.ToString());
        bInput?.SetText(c.B.ToString());
        hexInput?.SetText($"#{c.R:X2}{c.G:X2}{c.B:X2}");

        OnColorChanged?.Invoke(Owner, this, c);
        OnColorChangedBasic?.Invoke(c);
    }

    // color conversion helpers --------------------------------------------
    // HSV <-> RGB
    private static (float r, float g, float b) HsvToRgb(float h, float s, float v)
    {
        h %= 360f;
        if (h < 0) h += 360f;
        float c = v * s;
        float x = c * (1 - MathF.Abs(((h / 60f) % 2f) - 1f));
        float m = v - c;
        float rr, gg, bb;
        if (h < 60) { rr = c; gg = x; bb = 0; }
        else if (h < 120) { rr = x; gg = c; bb = 0; }
        else if (h < 180) { rr = 0; gg = c; bb = x; }
        else if (h < 240) { rr = 0; gg = x; bb = c; }
        else if (h < 300) { rr = x; gg = 0; bb = c; }
        else { rr = c; gg = 0; bb = x; }
        return (rr + m, gg + m, bb + m);
    }

    private static Color HsvToRgbColor(float h, float s, float v)
    {
        var (r, g, b) = HsvToRgb(h, s, v);
        return new Color((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)255);
    }

    private static (float h, float s, float v) RgbToHsv(float r, float g, float b)
    {
        r = Math.Clamp(r, 0f, 1f);
        g = Math.Clamp(g, 0f, 1f);
        b = Math.Clamp(b, 0f, 1f);

        float max = MathF.Max(r, MathF.Max(g, b));
        float min = MathF.Min(r, MathF.Min(g, b));
        float delta = max - min;

        float h = 0f;
        if (delta != 0f)
        {
            if (max == r) h = 60f * ((g - b) / delta % 6f);
            else if (max == g) h = 60f * ((b - r) / delta + 2f);
            else h = 60f * ((r - g) / delta + 4f);
        }
        if (h < 0f) h += 360f;

        float s = max == 0f ? 0f : delta / max;
        float v = max;

        return (h, s, v);
    }

    private static (float r, float g, float b) HslToRgb(float h, float s, float l)
    {
        h %= 360f;
        if (h < 0f) h += 360f;
        s = Math.Clamp(s, 0f, 1f);
        l = Math.Clamp(l, 0f, 1f);

        float c = (1f - MathF.Abs(2f * l - 1f)) * s;
        float hh = h / 60f;
        float x = c * (1f - MathF.Abs(hh % 2f - 1f));

        float r1, g1, b1;
        if (hh < 1f) (r1, g1, b1) = (c, x, 0f);
        else if (hh < 2f) (r1, g1, b1) = (x, c, 0f);
        else if (hh < 3f) (r1, g1, b1) = (0f, c, x);
        else if (hh < 4f) (r1, g1, b1) = (0f, x, c);
        else if (hh < 5f) (r1, g1, b1) = (x, 0f, c);
        else (r1, g1, b1) = (c, 0f, x);

        float m = l - c * 0.5f;
        return (r1 + m, g1 + m, b1 + m);
    }

    private static (float h, float s, float l) RgbToHsl(float r, float g, float b)
    {
        r = Math.Clamp(r, 0f, 1f);
        g = Math.Clamp(g, 0f, 1f);
        b = Math.Clamp(b, 0f, 1f);

        float max = MathF.Max(r, MathF.Max(g, b));
        float min = MathF.Min(r, MathF.Min(g, b));
        float delta = max - min;

        float l = (max + min) * 0.5f;
        float s = delta == 0f ? 0f : delta / (1f - MathF.Abs(2f * l - 1f));

        float h = 0f;
        if (delta != 0f)
        {
            if (max == r) h = 60f * ((g - b) / delta % 6f);
            else if (max == g) h = 60f * ((b - r) / delta + 2f);
            else h = 60f * ((r - g) / delta + 4f);
        }
        if (h < 0f) h += 360f;

        return (h, s, l);
    }
}
