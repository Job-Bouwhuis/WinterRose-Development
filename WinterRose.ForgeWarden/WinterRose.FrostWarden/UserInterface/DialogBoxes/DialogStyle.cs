using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes
{

    public class DialogStyle : ContentStyle
    {
        public Color TimeLabelColor { get; set; }

        public DialogAnimationMode AnimationMode { get; set; } = DialogAnimationMode.Curve;

        public float AlphaSpeed { get; set; } = 0.35f;
        public float ScaleWidthSpeed { get; set; } = 0.8f;
        public float ScaleHeightSpeed { get; set; } = 1.25f;
        public float ContentFadeDuration { get; set; } = 0.25f;
        public float ContentMoveDuration { get; set; } = 0.2f;

        public DialogStyle(StyleBase styleBase) : base(styleBase)
        {
            Background = new Color(30, 26, 36, 200);        // dark charcoal with purple undertone
            Border = new Color(120, 100, 140, 255);         // muted purple-gray border
            Shadow = new Color(0, 0, 0, 110);               // slightly stronger shadow
            ContentTint = new Color(245, 235, 245, 255);    // warm soft white text

            ProgressBarBackground = new Color(50, 45, 60);  // dark muted purple background
            ProgressBarFill = new Color(215, 95, 185);      // pink accent fill
            BarText = new Color(250, 245, 250);             // gentle white

            ButtonTextColor = new Color(250, 245, 250);
            ButtonBackground = new Color(75, 65, 90);       // soft purple-gray
            ButtonBorder = new Color(160, 130, 190);        // pink-purple edge
            ButtonHover = new Color(105, 90, 130);          // lifted hover state
            ButtonClick = new Color(215, 95, 185);          // accent pink click

            TimeLabelColor = new Color(150, 130, 170);      // muted lavender, low attention
        }
    }
}
