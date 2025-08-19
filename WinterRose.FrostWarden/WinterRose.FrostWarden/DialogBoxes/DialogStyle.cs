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

        private Color DialogBackgroundRaw = new(30, 30, 30, 180);
        private Color DialogBorderRaw = new(200, 200, 200, 255);
        private Color ShadowRaw = new(0, 0, 0, 80);
        private Color contentColorRaw = new(255, 255, 255);
        private Color fadedGrayRaw = new(180, 180, 180);
        private Color barBackgroundRaw = new(80, 80, 80);
        private Color barFillRaw = new(0, 150, 255);
        private Color BarTextColor = Color.White;
        private Color buttonTextColorRaw = new(255, 255, 255);
        private Color buttonBackgroundRaw = new(100, 100, 100);
        private Color buttonBorderRaw = new(255, 255, 255);
        private Color buttonHoverRaw = new(80, 80, 80);
        private Color buttonClickRaw = new(0, 150, 255);
        private Color timeLabelColorRaw = new(150, 150, 150);

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
