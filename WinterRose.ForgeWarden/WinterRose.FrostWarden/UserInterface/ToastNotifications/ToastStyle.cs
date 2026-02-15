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
        // Updated: fill in missing stylized defaults for toast style colors (add to your existing switch)
        switch (type)
        {
            case ToastType.CriticalAction:
                Background = new Color(70, 20, 35, 220);          // dark wine red
                Border = new Color(220, 80, 120, 255);            // hot pink-red
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 130);

                ProgressBarBackground = new Color(50, 20, 30, 180);
                ProgressBarFill = new Color(255, 100, 150, 255);  // POP: brighter, full alpha
                ProgressBarText = Color.White;

                TimerBarBackground = new Color(50, 20, 30, 40);
                TimerBarFill = new Color(255, 120, 160, 180);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(110, 30, 50, 230);
                ButtonBorder = new Color(255, 120, 170, 255);
                ButtonHover = new Color(160, 50, 80, 255);
                ButtonClick = new Color(255, 90, 140, 255);

                ScrollbarTrack = new Color(40, 20, 30, 140);
                ScrollbarThumb = new Color(200, 80, 120, 220);

                TextSmall = new Color(230, 210, 215, 220);
                TextBoxBackground = new Color(40, 18, 28, 220);
                TextBoxBorder = new Color(110, 45, 65, 200);
                TextBoxFocusedBorder = new Color(255, 90, 140, 255);
                TextBoxText = Color.White;
                Caret = new Color(255, 120, 160, 255);

                PanelBackground = new Color(50, 22, 32, 230);
                PanelBorder = new Color(140, 60, 85, 220);
                PanelBackgroundDarker = new Color(30, 12, 20, 240);

                TooltipBackground = new Color(48, 18, 28, 220);

                GridLine = new Color(80, 40, 50, 140);
                AxisLine = new Color(200, 90, 120, 200);

                TreeNodeBorder = new Color(190, 80, 120, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(220, 100, 140, 220);
                TreeNodeHover = new Color(180, 70, 100, 200);
                TreeNodeBackground = new Color(36, 14, 22, 220);

                RadioGroupAccent = new Color(230, 90, 130, 220);
                RadioGroupBackground = new Color(48, 18, 28, 200);
                RadioGroupBorder = new Color(140, 55, 80, 200);

                SliderTick = new Color(220, 170, 190, 100);
                SliderFilled = new Color(255, 100, 150, 240);

                SeperatorLineColor = new Color(70, 30, 40, 160);
                break;

            case ToastType.Question:
                Background = new Color(50, 40, 80, 200);          // purple inquiry
                Border = new Color(170, 120, 220, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 90);

                ProgressBarBackground = new Color(40, 35, 60, 170);
                ProgressBarFill = new Color(200, 130, 255, 255);  // POP
                ProgressBarText = Color.White;

                TimerBarBackground = new Color(40, 35, 60, 40);
                TimerBarFill = new Color(210, 150, 255, 150);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(80, 60, 120, 220);
                ButtonBorder = new Color(200, 150, 255, 255);
                ButtonHover = new Color(110, 85, 160, 255);
                ButtonClick = new Color(190, 120, 255, 255);

                ScrollbarTrack = new Color(50, 40, 80, 120);
                ScrollbarThumb = new Color(180, 140, 240, 220);

                TextSmall = new Color(220, 210, 235, 210);
                TextBoxBackground = new Color(38, 34, 60, 220);
                TextBoxBorder = new Color(140, 110, 180, 200);
                TextBoxFocusedBorder = new Color(200, 140, 255, 255);
                TextBoxText = Color.White;
                Caret = new Color(190, 130, 255, 255);

                PanelBackground = new Color(46, 36, 68, 220);
                PanelBorder = new Color(150, 120, 200, 220);
                PanelBackgroundDarker = new Color(30, 26, 48, 240);

                TooltipBackground = new Color(44, 36, 64, 220);

                GridLine = new Color(70, 62, 95, 120);
                AxisLine = new Color(170, 120, 220, 200);

                TreeNodeBorder = new Color(180, 140, 220, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(200, 150, 255, 220);
                TreeNodeHover = new Color(160, 120, 200, 200);
                TreeNodeBackground = new Color(32, 28, 50, 220);

                RadioGroupAccent = new Color(180, 130, 255, 220);
                RadioGroupBackground = new Color(44, 36, 64, 200);
                RadioGroupBorder = new Color(140, 110, 180, 200);

                SliderTick = new Color(200, 170, 240, 100);
                SliderFilled = new Color(200, 130, 255, 240);

                SeperatorLineColor = new Color(68, 58, 90, 140);
                break;

            case ToastType.Highlight:
                Background = new Color(55, 45, 90, 200);          // subdued purple
                Border = new Color(180, 140, 240, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 90);

                ProgressBarBackground = new Color(100, 70, 150, 255);
                ProgressBarFill = new Color(220, 170, 255, 255);  // POP
                ProgressBarText = Color.Black;

                TimerBarBackground = new Color(50, 40, 70, 40);
                TimerBarFill = new Color(220, 180, 255, 150);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(90, 70, 130, 220);
                ButtonBorder = new Color(210, 170, 255, 255);
                ButtonHover = new Color(120, 95, 170, 255);
                ButtonClick = new Color(200, 150, 255, 255);

                ScrollbarTrack = new Color(55, 45, 90, 120);
                ScrollbarThumb = new Color(190, 150, 245, 220);

                TextSmall = new Color(235, 230, 245, 210);
                TextBoxBackground = new Color(42, 36, 64, 220);
                TextBoxBorder = new Color(150, 120, 185, 200);
                TextBoxFocusedBorder = new Color(220, 170, 255, 255);
                TextBoxText = Color.White;
                Caret = new Color(210, 160, 255, 255);

                PanelBackground = new Color(48, 40, 72, 220);
                PanelBorder = new Color(170, 130, 210, 220);
                PanelBackgroundDarker = new Color(30, 24, 44, 240);

                TooltipBackground = new Color(44, 36, 64, 220);

                GridLine = new Color(80, 70, 105, 120);
                AxisLine = new Color(190, 150, 240, 200);

                TreeNodeBorder = new Color(200, 160, 240, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(215, 170, 250, 220);
                TreeNodeHover = new Color(180, 140, 220, 200);
                TreeNodeBackground = new Color(34, 28, 52, 220);

                RadioGroupAccent = new Color(210, 160, 255, 220);
                RadioGroupBackground = new Color(40, 34, 60, 200);
                RadioGroupBorder = new Color(150, 120, 185, 200);

                SliderTick = new Color(210, 180, 245, 100);
                SliderFilled = new Color(220, 170, 255, 240);

                SeperatorLineColor = new Color(72, 60, 96, 140);
                break;

            case ToastType.Neutral:
                Background = new Color(45, 40, 50, 180);          // charcoal purple
                Border = new Color(160, 140, 170, 220);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 80);

                ProgressBarBackground = new Color(60, 55, 65, 160);
                ProgressBarFill = new Color(225, 130, 190, 255);  // POP but tasteful

                TimerBarBackground = new Color(60, 55, 65, 40);
                TimerBarFill = new Color(200, 140, 240, 140);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(75, 65, 85, 220);
                ButtonBorder = new Color(180, 160, 200, 255);
                ButtonHover = new Color(95, 85, 105, 255);
                ButtonClick = new Color(180, 120, 220, 255);

                ScrollbarTrack = new Color(45, 40, 50, 120);
                ScrollbarThumb = new Color(170, 150, 190, 220);

                TextSmall = new Color(215, 210, 220, 200);
                TextBoxBackground = new Color(36, 32, 40, 220);
                TextBoxBorder = new Color(120, 110, 130, 200);
                TextBoxFocusedBorder = new Color(200, 150, 220, 240);
                TextBoxText = Color.White;
                Caret = new Color(200, 140, 230, 240);

                PanelBackground = new Color(42, 36, 46, 220);
                PanelBorder = new Color(130, 115, 140, 200);
                PanelBackgroundDarker = new Color(24, 20, 28, 240);

                TooltipBackground = new Color(40, 36, 44, 220);

                GridLine = new Color(80, 75, 90, 100);
                AxisLine = new Color(150, 140, 165, 180);

                TreeNodeBorder = new Color(160, 140, 170, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(180, 160, 190, 200);
                TreeNodeHover = new Color(140, 120, 150, 180);
                TreeNodeBackground = new Color(30, 26, 34, 220);

                RadioGroupAccent = new Color(180, 150, 200, 200);
                RadioGroupBackground = new Color(34, 30, 40, 200);
                RadioGroupBorder = new Color(120, 105, 130, 200);

                SliderTick = new Color(200, 180, 205, 90);
                SliderFilled = new Color(190, 130, 230, 220);

                SeperatorLineColor = new Color(60, 55, 70, 140);
                break;

            case ToastType.Success:
                Background = new Color(30, 90, 60, 190);          // dark emerald
                Border = new Color(120, 240, 180, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 100);

                ProgressBarBackground = new Color(35, 60, 50, 170);
                ProgressBarFill = new Color(80, 255, 180, 255);   // POP: very bright green-teal
                ProgressBarText = Color.Black; // high contrast for bright fill

                TimerBarBackground = new Color(35, 60, 50, 50);
                TimerBarFill = new Color(120, 255, 200, 170);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(50, 110, 80, 220);
                ButtonBorder = new Color(140, 255, 200, 255);
                ButtonHover = new Color(70, 150, 100, 255);
                ButtonClick = new Color(90, 240, 180, 255);

                ScrollbarTrack = new Color(30, 90, 60, 120);
                ScrollbarThumb = new Color(120, 240, 180, 220);

                TextSmall = new Color(225, 245, 235, 220);
                TextBoxBackground = new Color(30, 70, 48, 220);
                TextBoxBorder = new Color(90, 200, 140, 200);
                TextBoxFocusedBorder = new Color(120, 255, 200, 255);
                TextBoxText = Color.White;
                Caret = new Color(120, 255, 200, 255);

                PanelBackground = new Color(30, 66, 46, 220);
                PanelBorder = new Color(100, 210, 150, 220);
                PanelBackgroundDarker = new Color(18, 44, 30, 240);

                TooltipBackground = new Color(28, 60, 42, 220);

                GridLine = new Color(60, 110, 85, 110);
                AxisLine = new Color(140, 230, 170, 200);

                TreeNodeBorder = new Color(120, 240, 180, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(160, 255, 200, 220);
                TreeNodeHover = new Color(120, 200, 150, 200);
                TreeNodeBackground = new Color(22, 48, 36, 220);

                RadioGroupAccent = new Color(100, 240, 170, 220);
                RadioGroupBackground = new Color(28, 54, 40, 200);
                RadioGroupBorder = new Color(100, 210, 150, 200);

                SliderTick = new Color(200, 245, 220, 100);
                SliderFilled = new Color(90, 255, 180, 240);

                SeperatorLineColor = new Color(46, 90, 66, 140);
                break;

            case ToastType.Info:
                Background = new Color(45, 60, 120, 180);         // indigo info
                Border = new Color(140, 170, 255, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 100);

                ProgressBarBackground = new Color(45, 45, 60, 160);
                ProgressBarFill = new Color(120, 190, 255, 255);  // POP
                ProgressBarText = Color.Black;

                TimerBarBackground = new Color(45, 45, 60, 30);
                TimerBarFill = new Color(170, 200, 255, 140);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(70, 80, 130, 220);
                ButtonBorder = new Color(190, 210, 255, 255);
                ButtonHover = new Color(100, 120, 180, 255);
                ButtonClick = new Color(140, 170, 255, 255);

                ScrollbarTrack = new Color(45, 60, 120, 120);
                ScrollbarThumb = new Color(150, 190, 255, 220);

                TextSmall = new Color(225, 235, 250, 210);
                TextBoxBackground = new Color(38, 44, 70, 220);
                TextBoxBorder = new Color(140, 160, 200, 200);
                TextBoxFocusedBorder = new Color(160, 190, 255, 255);
                TextBoxText = Color.White;
                Caret = new Color(140, 170, 255, 255);

                PanelBackground = new Color(40, 48, 78, 220);
                PanelBorder = new Color(140, 170, 220, 220);
                PanelBackgroundDarker = new Color(24, 28, 46, 240);

                TooltipBackground = new Color(36, 44, 70, 220);

                GridLine = new Color(80, 90, 120, 110);
                AxisLine = new Color(140, 170, 220, 200);

                TreeNodeBorder = new Color(170, 190, 235, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(160, 190, 255, 220);
                TreeNodeHover = new Color(130, 150, 210, 200);
                TreeNodeBackground = new Color(32, 36, 56, 220);

                RadioGroupAccent = new Color(140, 170, 255, 220);
                RadioGroupBackground = new Color(36, 44, 70, 200);
                RadioGroupBorder = new Color(120, 140, 180, 200);

                SliderTick = new Color(200, 215, 245, 90);
                SliderFilled = new Color(120, 190, 255, 240);

                SeperatorLineColor = new Color(56, 64, 96, 140);
                break;

            case ToastType.Warning:
                Background = new Color(140, 80, 0, 190);          // amber warning
                Border = new Color(255, 200, 120, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 110);

                ProgressBarBackground = new Color(60, 40, 0, 170);
                ProgressBarFill = new Color(255, 220, 90, 255);   // POP: bright amber
                ProgressBarText = Color.Black;

                TimerBarBackground = new Color(60, 40, 0, 40);
                TimerBarFill = new Color(255, 220, 130, 160);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(160, 100, 0, 220);
                ButtonBorder = new Color(255, 220, 140, 255);
                ButtonHover = new Color(200, 130, 20, 255);
                ButtonClick = new Color(255, 190, 80, 255);

                ScrollbarTrack = new Color(140, 80, 0, 120);
                ScrollbarThumb = new Color(255, 210, 130, 220);

                TextSmall = new Color(245, 235, 210, 220);
                TextBoxBackground = new Color(80, 44, 8, 220);
                TextBoxBorder = new Color(220, 170, 90, 200);
                TextBoxFocusedBorder = new Color(255, 220, 120, 255);
                TextBoxText = Color.White;
                Caret = new Color(255, 200, 80, 255);

                PanelBackground = new Color(120, 68, 8, 220);
                PanelBorder = new Color(220, 160, 70, 220);
                PanelBackgroundDarker = new Color(80, 40, 4, 240);

                TooltipBackground = new Color(110, 58, 8, 220);

                GridLine = new Color(200, 150, 100, 80);
                AxisLine = new Color(255, 200, 120, 200);

                TreeNodeBorder = new Color(255, 200, 120, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(255, 210, 140, 220);
                TreeNodeHover = new Color(220, 160, 60, 200);
                TreeNodeBackground = new Color(88, 46, 6, 220);

                RadioGroupAccent = new Color(255, 200, 110, 220);
                RadioGroupBackground = new Color(100, 52, 6, 200);
                RadioGroupBorder = new Color(220, 160, 70, 200);

                SliderTick = new Color(255, 230, 180, 100);
                SliderFilled = new Color(255, 220, 90, 240);

                SeperatorLineColor = new Color(140, 80, 20, 160);
                break;

            case ToastType.Error:
                Background = new Color(120, 30, 50, 200);         // crimson-pink error
                Border = new Color(255, 90, 140, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 120);

                ProgressBarBackground = new Color(60, 20, 30, 180);
                ProgressBarFill = new Color(255, 90, 140, 255);   // POP
                ProgressBarText = Color.White;

                TimerBarBackground = new Color(60, 20, 30, 40);
                TimerBarFill = new Color(255, 120, 170, 170);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(110, 40, 60, 230);
                ButtonBorder = new Color(255, 120, 170, 255);
                ButtonHover = new Color(170, 60, 90, 255);
                ButtonClick = new Color(255, 90, 140, 255);

                ScrollbarTrack = new Color(120, 30, 50, 120);
                ScrollbarThumb = new Color(255, 120, 170, 220);

                TextSmall = new Color(240, 230, 235, 220);
                TextBoxBackground = new Color(60, 24, 36, 220);
                TextBoxBorder = new Color(180, 70, 100, 200);
                TextBoxFocusedBorder = new Color(255, 110, 150, 255);
                TextBoxText = Color.White;
                Caret = new Color(255, 100, 140, 255);

                PanelBackground = new Color(64, 22, 36, 220);
                PanelBorder = new Color(200, 80, 120, 220);
                PanelBackgroundDarker = new Color(34, 10, 18, 240);

                TooltipBackground = new Color(56, 18, 30, 220);

                GridLine = new Color(120, 60, 80, 110);
                AxisLine = new Color(240, 100, 140, 200);

                TreeNodeBorder = new Color(255, 110, 150, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(255, 120, 170, 220);
                TreeNodeHover = new Color(200, 80, 110, 200);
                TreeNodeBackground = new Color(44, 16, 26, 220);

                RadioGroupAccent = new Color(255, 120, 170, 220);
                RadioGroupBackground = new Color(58, 20, 32, 200);
                RadioGroupBorder = new Color(180, 70, 100, 200);

                SliderTick = new Color(255, 200, 220, 100);
                SliderFilled = new Color(255, 110, 150, 230);

                SeperatorLineColor = new Color(95, 30, 50, 160);
                break;

            case ToastType.Fatal:
                Background = new Color(20, 0, 10, 220);           // near-black with red tint
                Border = new Color(255, 0, 80, 255);
                ContentTint = Color.White;
                Shadow = new Color(0, 0, 0, 140);

                ProgressBarBackground = new Color(40, 0, 10, 190);
                ProgressBarFill = new Color(255, 0, 90, 255);     // POP: vivid magenta-red
                ProgressBarText = Color.White;

                TimerBarBackground = new Color(40, 0, 10, 50);
                TimerBarFill = new Color(255, 40, 120, 180);

                ButtonTextColor = Color.White;
                ButtonBackground = new Color(80, 0, 20, 230);
                ButtonBorder = new Color(255, 60, 120, 255);
                ButtonHover = new Color(120, 0, 40, 255);
                ButtonClick = new Color(255, 0, 80, 255);

                ScrollbarTrack = new Color(80, 0, 20, 120);
                ScrollbarThumb = new Color(255, 60, 120, 220);

                TextSmall = new Color(220, 200, 210, 220);
                TextBoxBackground = new Color(28, 6, 12, 220);
                TextBoxBorder = new Color(180, 40, 80, 200);
                TextBoxFocusedBorder = new Color(255, 60, 120, 255);
                TextBoxText = Color.White;
                Caret = new Color(255, 60, 120, 255);

                PanelBackground = new Color(24, 4, 10, 220);
                PanelBorder = new Color(200, 30, 70, 220);
                PanelBackgroundDarker = new Color(12, 2, 6, 240);

                TooltipBackground = new Color(30, 6, 12, 220);

                GridLine = new Color(90, 30, 50, 110);
                AxisLine = new Color(255, 40, 110, 200);

                TreeNodeBorder = new Color(255, 60, 120, 200);
                TreeNodeText = Color.White;
                TreeNodeArrow = new Color(255, 80, 140, 220);
                TreeNodeHover = new Color(210, 50, 100, 200);
                TreeNodeBackground = new Color(18, 2, 8, 220);

                RadioGroupAccent = new Color(255, 60, 120, 220);
                RadioGroupBackground = new Color(28, 4, 10, 200);
                RadioGroupBorder = new Color(180, 40, 80, 200);

                SliderTick = new Color(240, 180, 200, 90);
                SliderFilled = new Color(255, 40, 110, 240);

                SeperatorLineColor = new Color(48, 8, 18, 160);
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

