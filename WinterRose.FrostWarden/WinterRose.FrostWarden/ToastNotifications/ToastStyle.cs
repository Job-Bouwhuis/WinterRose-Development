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
    private Color timerBarFillRaw = new Color(0, 250, 0, 100);

    // Button colors (optional)
    private Color buttonTextColorRaw = new Color(255, 255, 255, 255);
    private Color buttonBackgroundRaw = new Color(80, 80, 80, 220);
    private Color buttonBorderRaw = new Color(255, 255, 255, 255);
    private Color buttonHoverRaw = new Color(100, 100, 100, 255);
    private Color buttonClickRaw = new Color(0, 150, 255, 255);

    public ToastStyle(ToastType type)
    {
        ApplyDefaults(type);
    }

    private void ApplyDefaults(ToastType type)
    {
        switch (type)
        {
            case ToastType.CriticalAction:
                Background = new Color(120, 20, 20, 220);  // deep red
                Border = new Color(255, 80, 80, 255);
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 120);

                ProgressBarBackground = new Color(80, 20, 20, 180);
                ProgressBarFill = new Color(255, 60, 60, 255);

                TimerBarBackground = new Color(80, 20, 20, 40);
                TimerBarFill = new Color(255, 100, 100, 180);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(140, 30, 30, 230);
                ButtonBorder = new Color(255, 120, 120, 255);
                ButtonHover = new Color(200, 50, 50, 255);
                ButtonClick = new Color(255, 80, 80, 255);
                break;

            case ToastType.Question:
                Background = new Color(40, 60, 100, 200);  // soft blue
                Border = new Color(120, 160, 200, 255);
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 80);

                ProgressBarBackground = new Color(50, 50, 80, 170);
                ProgressBarFill = new Color(70, 140, 220, 255);

                TimerBarBackground = new Color(50, 50, 80, 40);
                TimerBarFill = new Color(90, 180, 255, 150);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(70, 100, 150, 220);
                ButtonBorder = new Color(120, 180, 255, 255);
                ButtonHover = new Color(90, 130, 200, 255);
                ButtonClick = new Color(70, 140, 255, 255);
                break;

            case ToastType.Highlight:
                Background = new Color(60, 80, 140, 200);  // muted blue
                Border = new Color(120, 160, 220, 255);
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 90);

                ProgressBarBackground = new Color(60, 60, 100, 170);
                ProgressBarFill = new Color(120, 180, 255, 255);

                TimerBarBackground = new Color(60, 60, 100, 40);
                TimerBarFill = new Color(150, 200, 255, 150);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(80, 110, 160, 220);
                ButtonBorder = new Color(150, 200, 255, 255);
                ButtonHover = new Color(100, 140, 200, 255);
                ButtonClick = new Color(120, 180, 255, 255);
                break;

            case ToastType.Neutral:
                Background = new Color(50, 50, 50, 180);
                Border = new Color(200, 200, 200, 220);
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 80);

                ProgressBarBackground = new Color(70, 70, 70, 160);
                ProgressBarFill = new Color(100, 150, 200, 255);

                TimerBarBackground = new Color(70, 70, 70, 40);
                TimerBarFill = new Color(100, 180, 255, 140);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(80, 80, 80, 220);
                ButtonBorder = new Color(180, 180, 180, 255);
                ButtonHover = new Color(100, 100, 100, 255);
                ButtonClick = new Color(80, 130, 180, 255);
                break;

            case ToastType.Success:
                Background = new Color(30, 100, 50, 190);   // dark green base
                Border = new Color(80, 200, 120, 255);      // bright green accent
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 100);

                ProgressBarBackground = new Color(40, 60, 40, 170);
                ProgressBarFill = new Color(60, 220, 130, 255); // vibrant green fill

                TimerBarBackground = new Color(40, 60, 40, 50);
                TimerBarFill = new Color(100, 255, 160, 170);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(50, 120, 70, 220);
                ButtonBorder = new Color(120, 240, 160, 255);
                ButtonHover = new Color(70, 150, 90, 255);
                ButtonClick = new Color(60, 220, 130, 255);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;

                AlphaCurve = Curves.EaseOutBack;
                PositionCurve = Curves.EaseOutBack;
                AnimateInDuration = 0.35f;
                AnimateOutDuration = 0.35f;
                break;


            case ToastType.Info:
                Background = new Color(50, 100, 200, 180);
                Border = new Color(80, 160, 255, 255);
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 100);

                ProgressBarBackground = new Color(50, 50, 50, 160);
                ProgressBarFill = new Color(0, 150, 255, 255);

                TimerBarBackground = new Color(50, 50, 50, 30);
                TimerBarFill = new Color(0, 200, 255, 140);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(70, 90, 130, 220);
                ButtonBorder = new Color(200, 220, 255, 255);
                ButtonHover = new Color(100, 130, 180, 255);
                ButtonClick = new Color(0, 150, 255, 255);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;

                AlphaCurve = Curves.EaseOutBack;
                PositionCurve = Curves.EaseOutBack;
                AnimateInDuration = 0.4f;
                AnimateOutDuration = 0.4f;
                break;

            case ToastType.Warning:
                Background = new Color(200, 120, 0, 190);
                Border = new Color(255, 200, 80, 255);
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 110);

                ProgressBarBackground = new Color(60, 40, 0, 170);
                ProgressBarFill = new Color(255, 200, 50, 255);

                TimerBarBackground = new Color(60, 40, 0, 40);
                TimerBarFill = new Color(255, 210, 80, 160);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(160, 100, 0, 220);
                ButtonBorder = new Color(255, 210, 120, 255);
                ButtonHover = new Color(190, 120, 20, 255);
                ButtonClick = new Color(255, 170, 0, 255);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;

                AlphaCurve = Curves.EaseOutBack;
                PositionCurve = Curves.EaseOutBack;
                AnimateInDuration = 0.35f;
                AnimateOutDuration = 0.35f;
                break;

            case ToastType.Error:
                Background = new Color(160, 40, 40, 200);
                Border = new Color(255, 90, 90, 255);
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 120);

                ProgressBarBackground = new Color(60, 20, 20, 180);
                ProgressBarFill = new Color(255, 70, 70, 255);

                TimerBarBackground = new Color(60, 20, 20, 40);
                TimerBarFill = new Color(255, 100, 100, 170);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(120, 40, 40, 230);
                ButtonBorder = new Color(255, 120, 120, 255);
                ButtonHover = new Color(170, 60, 60, 255);
                ButtonClick = new Color(255, 80, 80, 255);

                ShadowSizeRight = 5f;
                ShadowSizeBottom = 5f;

                AlphaCurve = Curves.ElasticOut;
                PositionCurve = Curves.ElasticOut;
                AnimateInDuration = 0.25f;
                AnimateOutDuration = 0.25f;
                break;

            case ToastType.Fatal:
                Background = new Color(25, 0, 0, 220);
                Border = new Color(255, 0, 0, 255);
                ContentColor = new Color(255, 255, 255, 255);
                Shadow = new Color(0, 0, 0, 140);

                ProgressBarBackground = new Color(40, 0, 0, 190);
                ProgressBarFill = new Color(255, 0, 0, 255);

                TimerBarBackground = new Color(40, 0, 0, 50);
                TimerBarFill = new Color(255, 0, 0, 180);

                ButtonTextColor = new Color(255, 255, 255, 255);
                ButtonBackground = new Color(80, 0, 0, 230);
                ButtonBorder = new Color(255, 50, 50, 255);
                ButtonHover = new Color(120, 0, 0, 255);
                ButtonClick = new Color(255, 0, 0, 255);

                ShadowSizeRight = 6f;
                ShadowSizeBottom = 6f;

                AlphaCurve = Curves.EaseOutCubic;
                PositionCurve = Curves.EaseOutCubic;
                AnimateInDuration = 0.45f;
                AnimateOutDuration = 0.45f;
                break;
        }
    }

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

