using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.ToastNotifications;
public class ToastStyle
{
    internal float contentAlpha = 1f; // For interactions with dialog system

    private Color backgroundRaw = new Color(255, 255, 255, 120);
    private Color borderRaw = new Color(255, 255, 255, 255);
    private Color shadowRaw = new Color(0, 0, 0, 100);
    private Color contentColorRaw = new Color(255, 255, 255, 255);

    // Progress bar colors
    private Color progressBarBackgroundRaw = new Color(50, 50, 50, 180);
    private Color progressBarFillRaw = new Color(0, 150, 255, 255);

    // Timer bar colors
    private Color timerBarBackgroundRaw = new Color(50, 50, 50, 30);
    private Color timerBarFillRaw = new Color(0, 90, 0, 100);

    // Button colors (optional)
    private Color buttonTextColorRaw = new Color(255, 255, 255, 255);
    private Color buttonBackgroundRaw = new Color(80, 80, 80, 220);
    private Color buttonBorderRaw = new Color(255, 255, 255, 255);
    private Color buttonHoverRaw = new Color(100, 100, 100, 255);
    private Color buttonClickRaw = new Color(0, 150, 255, 255);

    public ToastStyle() { }

    private Color WithAlpha(Color raw) => new Color(raw.R, raw.G, raw.B, (byte)Math.Clamp(raw.A * contentAlpha, 0f, 255f));

    // Basic toast colors
    public Color Background
    {
        get => WithAlpha(backgroundRaw);
        set => backgroundRaw = value;
    }

    public Color Border
    {
        get => WithAlpha(borderRaw);
        set => borderRaw = value;
    }

    public Color Shadow
    {
        get => WithAlpha(shadowRaw);
        set => shadowRaw = value;
    }

    public Color ContentColor
    {
        get => WithAlpha(contentColorRaw);
        set => contentColorRaw = value;
    }

    // Progress bar
    public Color ProgressBarBackground
    {
        get => WithAlpha(progressBarBackgroundRaw);
        set => progressBarBackgroundRaw = value;
    }

    public Color ProgressBarFill
    {
        get => WithAlpha(progressBarFillRaw);
        set => progressBarFillRaw = value;
    }

    // Timer bar
    public Color TimerBarBackground
    {
        get => WithAlpha(timerBarBackgroundRaw);
        set => timerBarBackgroundRaw = value;
    }

    public Color TimerBarFill
    {
        get => WithAlpha(timerBarFillRaw);
        set => timerBarFillRaw = value;
    }

    // Buttons
    public Color ButtonTextColor
    {
        get => WithAlpha(buttonTextColorRaw);
        set => buttonTextColorRaw = value;
    }

    public Color ButtonBackground
    {
        get => WithAlpha(buttonBackgroundRaw);
        set => buttonBackgroundRaw = value;
    }

    public Color ButtonBorder
    {
        get => WithAlpha(buttonBorderRaw);
        set => buttonBorderRaw = value;
    }

    public Color ButtonHover
    {
        get => WithAlpha(buttonHoverRaw);
        set => buttonHoverRaw = value;
    }

    public Color ButtonClick
    {
        get => WithAlpha(buttonClickRaw);
        set => buttonClickRaw = value;
    }

    // Shadow sizes
    public float ShadowSizeLeft { get; set; }
    public float ShadowSizeTop { get; set; }
    public float ShadowSizeRight { get; set; } = 4f;
    public float ShadowSizeBottom { get; set; } = 4f;

    // Animation curves
    public Curve AlphaCurve { get; set; } = Curves.EaseOutBack;
    public Curve PositionCurve { get; set; } = Curves.EaseOutBack;
    public float AnimateInDuration { get; set; } = 0.4f;
    public float AnimateOutDuration { get; set; } = 0.4f;
}

