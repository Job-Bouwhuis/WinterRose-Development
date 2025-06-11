using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.DialogBoxes
{
    public class DialogStyle
    {
        internal float contentAlpha = 1f;

        private Color contentColorRaw = new(255, 255, 255);
        private Color fadedGrayRaw = new(180, 180, 180);
        private Color barBackgroundRaw = new(80, 80, 80);
        private Color barFillRaw = new(0, 150, 255);
        private Color buttonTextColorRaw = new(255, 255, 255);
        private Color buttonBackgroundRaw = new(100, 100, 100);
        private Color buttonBorderRaw = new(255, 255, 255);
        private Color buttonHoverRaw = new(80, 80, 80);
        private Color buttonClickRaw = new(0, 150, 255);
        private Color timeLabelColorRaw = new(150, 150, 150);

        public DialogStyle()
        {
        }

        private Color WithAlpha(Color raw) => new Color(raw.R, raw.G, raw.B, (int)(255 * contentAlpha));

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
    }

}
