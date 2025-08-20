using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.DialogBoxes
{
    public class DialogStyle
    {
        internal float contentAlpha = 1f;

        internal Color DialogBackgroundRaw = new(28, 28, 32, 200);   // dark, with slight blue hint
        internal Color DialogBorderRaw = new(90, 90, 95, 255);       // subtle, not pure white border
        internal Color ShadowRaw = new(0, 0, 0, 100);                // slightly stronger shadow
        internal Color contentColorRaw = new(235, 235, 240);         // softer white for text
        internal Color fadedGrayRaw = new(160, 160, 165);            // more natural muted gray
        internal Color barBackgroundRaw = new(60, 60, 65);           // darker background for contrast
        internal Color barFillRaw = new(0, 140, 230);                // calmer blue accent
        internal Color BarTextColor = new(240, 240, 240);            // soft white, less harsh
        internal Color buttonTextColorRaw = new(240, 240, 240);
        internal Color buttonBackgroundRaw = new(70, 70, 75);        // softer, blends with background
        internal Color buttonBorderRaw = new(120, 120, 130);         // subtle border, not pure white
        internal Color buttonHoverRaw = new(95, 95, 105);            // light hover contrast
        internal Color buttonClickRaw = new(0, 140, 230);            // matches barFill accent
        internal Color timeLabelColorRaw = new(130, 130, 135);       // muted gray for less attention


        public DialogStyle()
        {
        }

        private Color WithAlpha(Color raw) => new Color(raw.R, raw.G, raw.B, (byte)Math.Clamp(raw.A * contentAlpha, 0f, 255f));
        
        public Color DialogBackground
        {
            get => WithAlpha(DialogBackgroundRaw);
            set => DialogBackgroundRaw = value;
        }

        public Color DialogBorder
        {
            get => WithAlpha(DialogBorderRaw);
            set => DialogBorderRaw = value; 
        }

        public Color Shadow
        {
            get => WithAlpha(ShadowRaw);
            set => ShadowRaw = value;
        }

        public Color ContentColor
        {
            get => WithAlpha(contentColorRaw);
            set => contentColorRaw = value;
        }

        public Color FadedGray
        {
            get => WithAlpha(fadedGrayRaw);
            set => fadedGrayRaw = value;
        }

        public Color BarBackground
        {
            get => WithAlpha(barBackgroundRaw);
            set => barBackgroundRaw = value;
        }

        public Color BarFill
        {
            get => WithAlpha(barFillRaw);
            set => barFillRaw = value;
        }

        public Color BarText
        {
            get => WithAlpha(BarTextColor);
            set => BarTextColor = value;
        }

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

        public Color TimeLabelColor
        {
            get => WithAlpha(timeLabelColorRaw);
            set => timeLabelColorRaw = value;
        }

        public DialogAnimationMode AnimationMode { get; set; } = DialogAnimationMode.Curve;

        public Curve WidthCurve { get; set; } = Curves.EaseOutBack;
        public Curve HeightCurve { get; set; } = Curves.EaseOutBack;
        public Curve AlphaCurve { get; set; } = Curves.EaseOutBack;

        public float AnimateInDuration { get; set; } = 0.45f;
        public float AnimateOutDuration { get; set; } = 0.85f;
        public float AlphaSpeed { get; set; } = 0.35f;
        public float ScaleWidthSpeed { get; set; } = 0.8f;
        public float ScaleHeightSpeed { get; set; } = 1.25f;
        public float ContentFadeDuration { get; set; } = 0.25f;
        public float ContentMoveDuration { get; set; } = 0.2f;
        public float ShadowSizeLeft { get; internal set; }
        public float ShadowSizeTop { get; internal set; }
        public float ShadowSizeBottom { get; internal set; } = 4;
        public float ShadowSizeRight { get; internal set; } = 4;
    }

}
