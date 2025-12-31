using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;
public class ToastStyle : ContentStyle
{
    #region Apply Defaults
    private void ApplyDefaults(ToastType type)
    {
        switch (type)
        {
            case ToastType.CriticalAction:
                Background = new Color(70, 20, 35, 220);          // dark wine red
                Border = new Color(220, 80, 120, 255);            // hot pink-red
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 130);

                ProgressBarBackground = new Color(50, 20, 30, 180);
                ProgressBarFill = new Color(255, 90, 130, 255);

                TimerBarBackground = new Color(50, 20, 30, 40);
                TimerBarFill = new Color(255, 120, 160, 180);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(110, 30, 50, 230);
                ButtonBorder = new Color(255, 120, 170, 255);
                ButtonHover = new Color(160, 50, 80, 255);
                ButtonClick = new Color(255, 90, 140, 255);

                ScrollbarTrack = new Color(40, 20, 30, 140);
                ScrollbarThumb = new Color(200, 80, 120, 220);
                break;

            case ToastType.Question:
                Background = new Color(50, 40, 80, 200);          // purple inquiry
                Border = new Color(170, 120, 220, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 90);

                ProgressBarBackground = new Color(40, 35, 60, 170);
                ProgressBarFill = new Color(190, 120, 255, 255);

                TimerBarBackground = new Color(40, 35, 60, 40);
                TimerBarFill = new Color(210, 150, 255, 150);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(80, 60, 120, 220);
                ButtonBorder = new Color(200, 150, 255, 255);
                ButtonHover = new Color(110, 85, 160, 255);
                ButtonClick = new Color(190, 120, 255, 255);

                ScrollbarTrack = new Color(50, 40, 80, 120);
                ScrollbarThumb = new Color(180, 140, 240, 220);
                break;

            case ToastType.Highlight:
                Background = new Color(55, 45, 90, 200);          // subdued purple
                Border = new Color(180, 140, 240, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 90);

                ProgressBarBackground = new Color(50, 40, 70, 170);
                ProgressBarFill = new Color(200, 150, 255, 255);

                TimerBarBackground = new Color(50, 40, 70, 40);
                TimerBarFill = new Color(220, 180, 255, 150);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(90, 70, 130, 220);
                ButtonBorder = new Color(210, 170, 255, 255);
                ButtonHover = new Color(120, 95, 170, 255);
                ButtonClick = new Color(200, 150, 255, 255);

                ScrollbarTrack = new Color(55, 45, 90, 120);
                ScrollbarThumb = new Color(190, 150, 245, 220);
                break;

            case ToastType.Neutral:
                Background = new Color(45, 40, 50, 180);          // charcoal purple
                Border = new Color(160, 140, 170, 220);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 80);

                ProgressBarBackground = new Color(60, 55, 65, 160);
                ProgressBarFill = new Color(180, 120, 220, 255);

                TimerBarBackground = new Color(60, 55, 65, 40);
                TimerBarFill = new Color(200, 140, 240, 140);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(75, 65, 85, 220);
                ButtonBorder = new Color(180, 160, 200, 255);
                ButtonHover = new Color(95, 85, 105, 255);
                ButtonClick = new Color(180, 120, 220, 255);

                ScrollbarTrack = new Color(45, 40, 50, 120);
                ScrollbarThumb = new Color(170, 150, 190, 220);
                break;

            case ToastType.Success:
                Background = new Color(30, 90, 60, 190);          // dark emerald
                Border = new Color(120, 240, 180, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 100);

                ProgressBarBackground = new Color(35, 60, 50, 170);
                ProgressBarFill = new Color(90, 240, 180, 255);

                TimerBarBackground = new Color(35, 60, 50, 50);
                TimerBarFill = new Color(120, 255, 200, 170);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(50, 110, 80, 220);
                ButtonBorder = new Color(140, 255, 200, 255);
                ButtonHover = new Color(70, 150, 100, 255);
                ButtonClick = new Color(90, 240, 180, 255);

                ScrollbarTrack = new Color(30, 90, 60, 120);
                ScrollbarThumb = new Color(120, 240, 180, 220);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;
                AlphaCurve = Curves.EaseOutBack;
                MoveAndScaleCurve = Curves.EaseOutBack;
                break;

            case ToastType.Info:
                Background = new Color(45, 60, 120, 180);         // indigo info
                Border = new Color(140, 170, 255, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 100);

                ProgressBarBackground = new Color(45, 45, 60, 160);
                ProgressBarFill = new Color(140, 170, 255, 255);

                TimerBarBackground = new Color(45, 45, 60, 30);
                TimerBarFill = new Color(170, 200, 255, 140);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(70, 80, 130, 220);
                ButtonBorder = new Color(190, 210, 255, 255);
                ButtonHover = new Color(100, 120, 180, 255);
                ButtonClick = new Color(140, 170, 255, 255);

                ScrollbarTrack = new Color(45, 60, 120, 120);
                ScrollbarThumb = new Color(150, 190, 255, 220);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;
                AlphaCurve = Curves.EaseOutBack;
                MoveAndScaleCurve = Curves.EaseOutBack;
                break;

            case ToastType.Warning:
                Background = new Color(140, 80, 0, 190);          // amber warning
                Border = new Color(255, 200, 120, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 110);

                ProgressBarBackground = new Color(60, 40, 0, 170);
                ProgressBarFill = new Color(255, 210, 90, 255);

                TimerBarBackground = new Color(60, 40, 0, 40);
                TimerBarFill = new Color(255, 220, 130, 160);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(160, 100, 0, 220);
                ButtonBorder = new Color(255, 220, 140, 255);
                ButtonHover = new Color(200, 130, 20, 255);
                ButtonClick = new Color(255, 190, 80, 255);

                ScrollbarTrack = new Color(140, 80, 0, 120);
                ScrollbarThumb = new Color(255, 210, 130, 220);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;
                AlphaCurve = Curves.EaseOutBack;
                MoveAndScaleCurve = Curves.EaseOutBack;
                break;

            case ToastType.Error:
                Background = new Color(120, 30, 50, 200);         // crimson-pink error
                Border = new Color(255, 90, 140, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 120);

                ProgressBarBackground = new Color(60, 20, 30, 180);
                ProgressBarFill = new Color(255, 90, 140, 255);

                TimerBarBackground = new Color(60, 20, 30, 40);
                TimerBarFill = new Color(255, 120, 170, 170);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(110, 40, 60, 230);
                ButtonBorder = new Color(255, 120, 170, 255);
                ButtonHover = new Color(170, 60, 90, 255);
                ButtonClick = new Color(255, 90, 140, 255);

                ScrollbarTrack = new Color(120, 30, 50, 120);
                ScrollbarThumb = new Color(255, 120, 170, 220);

                ShadowSizeRight = 5f;
                ShadowSizeBottom = 5f;
                AlphaCurve = Curves.Linear;
                MoveAndScaleCurve = Curves.Linear;
                break;

            case ToastType.Fatal:
                Background = new Color(20, 0, 10, 220);           // near-black with red tint
                Border = new Color(255, 0, 80, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 140);

                ProgressBarBackground = new Color(40, 0, 10, 190);
                ProgressBarFill = new Color(255, 0, 80, 255);

                TimerBarBackground = new Color(40, 0, 10, 50);
                TimerBarFill = new Color(255, 40, 120, 180);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(80, 0, 20, 230);
                ButtonBorder = new Color(255, 60, 120, 255);
                ButtonHover = new Color(120, 0, 40, 255);
                ButtonClick = new Color(255, 0, 80, 255);

                ScrollbarTrack = new Color(80, 0, 20, 120);
                ScrollbarThumb = new Color(255, 60, 120, 220);

                ShadowSizeRight = 6f;
                ShadowSizeBottom = 6f;
                AlphaCurve = Curves.Linear;
                MoveAndScaleCurve = Curves.Linear;
                break;
        }
    }
    #endregion

    public ToastStyle(ToastType type, StyleBase baseStyle) : base(baseStyle)
    {
        ApplyDefaults(type);
        RaiseOnHover = true;
        AllowUserResizing = false;
        ShowVerticalScrollBar = false;
        TimeUntilAutoDismiss = 5;
    }

}

