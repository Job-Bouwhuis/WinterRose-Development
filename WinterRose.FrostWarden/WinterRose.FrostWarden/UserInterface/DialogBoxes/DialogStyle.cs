using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes
{

    public class DialogStyle : ContainerStyle
    {
        public Color TimeLabelColor { get; set; }

        public DialogAnimationMode AnimationMode { get; set; } = DialogAnimationMode.Curve;

        public float AlphaSpeed { get; set; } = 0.35f;
        public float ScaleWidthSpeed { get; set; } = 0.8f;
        public float ScaleHeightSpeed { get; set; } = 1.25f;
        public float ContentFadeDuration { get; set; } = 0.25f;
        public float ContentMoveDuration { get; set; } = 0.2f;

        public DialogStyle()
        {
            Background = new Color(28, 28, 32, 200);   // dark, with slight blue hint
            Border = new Color(90, 90, 95, 255);   // subtle, not pure white border
            Shadow = new Color(0, 0, 0, 100);      // slightly stronger shadow
            ContentTint = new Color(235, 235, 240);     // softer white for text
            ProgressBarBackground = new Color(60, 60, 65);        // darker background for contrast
            ProgressBarFill = new Color(0, 140, 230);       // calmer blue accent
            BarText = new Color(240, 240, 240);     // soft white, less harsh
            ButtonTextColor = new Color(240, 240, 240);
            ButtonBackground = new Color(70, 70, 75);        // softer, blends with background
            ButtonBorder = new Color(120, 120, 130);     // subtle border, not pure white
            ButtonHover = new Color(95, 95, 105);       // light hover contrast
            ButtonClick = new Color(0, 140, 230);       // matches barFill accent
            TimeLabelColor = new Color(130, 130, 135);     // muted gray for less attention
        }
    }
}
