using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;
public class ToastStyle : ContainerStyle
{
    #region Apply Defaults
    private void ApplyDefaults(ToastType type)
    {
        switch (type)
        {
            case ToastType.CriticalAction:
                Background = new Color(120, 20, 20, 220);  // deep red
                Border = new Color(255, 80, 80, 255);
                ContentTint = new Color(255, 255, 255, 255);
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
                ScrollbarTrack = new Color(60, 20, 20, 140);
                ScrollbarThumb = new Color(200, 80, 80, 220);
                break;

            case ToastType.Question:
                Background = new Color(40, 60, 100, 200);  // soft blue
                Border = new Color(120, 160, 200, 255);
                ContentTint = new Color(255, 255, 255, 255);
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
                ScrollbarTrack = new Color(40, 60, 100, 120);
                ScrollbarThumb = new Color(120, 180, 255, 220);
                break;

            case ToastType.Highlight:
                Background = new Color(60, 80, 140, 200);  // muted blue
                Border = new Color(120, 160, 220, 255);
                ContentTint = new Color(255, 255, 255, 255);
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
                ScrollbarTrack = new Color(60, 80, 140, 120);
                ScrollbarThumb = new Color(150, 200, 255, 220);
                break;

            case ToastType.Neutral:
                Background = new Color(50, 50, 50, 180);
                Border = new Color(200, 200, 200, 220);
                ContentTint = new Color(255, 255, 255, 255);
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
                ScrollbarTrack = new Color(50, 50, 50, 120);
                ScrollbarThumb = new Color(180, 180, 180, 220);
                break;

            case ToastType.Success:
                Background = new Color(30, 100, 50, 190);   // dark green base
                Border = new Color(80, 200, 120, 255);      // bright green accent
                ContentTint = new Color(255, 255, 255, 255);
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
                ScrollbarTrack = new Color(30, 100, 50, 120);
                ScrollbarThumb = new Color(100, 220, 150, 220);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;

                AlphaCurve = Curves.EaseOutBack;
                MoveAndScaleCurve = Curves.EaseOutBack;
                break;

            case ToastType.Info:
                Background = new Color(50, 100, 200, 180);
                Border = new Color(80, 160, 255, 255);
                ContentTint = new Color(255, 255, 255, 255);
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

                ScrollbarTrack = new Color(50, 100, 200, 120);
                ScrollbarThumb = new Color(100, 180, 255, 220);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;

                AlphaCurve = Curves.EaseOutBack;
                MoveAndScaleCurve = Curves.EaseOutBack;
                break;

            case ToastType.Warning:
                Background = new Color(200, 120, 0, 190);
                Border = new Color(255, 200, 80, 255);
                ContentTint = new Color(255, 255, 255, 255);
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

                ScrollbarTrack = new Color(200, 120, 0, 120);
                ScrollbarThumb = new Color(255, 200, 100, 220);

                ShadowSizeRight = 4f;
                ShadowSizeBottom = 4f;

                AlphaCurve = Curves.EaseOutBack;
                MoveAndScaleCurve = Curves.EaseOutBack;
                break;

            case ToastType.Error:
                Background = new Color(160, 40, 40, 200);
                Border = new Color(255, 90, 90, 255);
                ContentTint = new Color(255, 255, 255, 255);
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

                ScrollbarTrack = new Color(160, 40, 40, 120);
                ScrollbarThumb = new Color(255, 120, 120, 220);

                ShadowSizeRight = 5f;
                ShadowSizeBottom = 5f;

                AlphaCurve = Curves.Linear;
                MoveAndScaleCurve = Curves.Linear;
                break;

            case ToastType.Fatal:
                Background = new Color(25, 0, 0, 220);
                Border = new Color(255, 0, 0, 255);
                ContentTint = new Color(255, 255, 255, 255);
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

                ScrollbarTrack = new Color(80, 0, 0, 120);
                ScrollbarThumb = new Color(255, 50, 50, 220);

                ShadowSizeRight = 6f;
                ShadowSizeBottom = 6f;

                AlphaCurve = Curves.Linear;
                MoveAndScaleCurve = Curves.Linear;
                break;
        }
    }
    #endregion

    public ToastStyle(ToastType type)
    {
        ApplyDefaults(type);
        RaiseOnHover = true;
        AllowUserResizing = false;
        ShowVerticalScrollBar = false;
        TimeUntilAutoDismiss = 5;
    }

}

