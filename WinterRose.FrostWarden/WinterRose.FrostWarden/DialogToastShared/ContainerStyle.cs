using Raylib_cs;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden
{
    public abstract class ContainerStyle
    {
        protected internal float ContentAlpha { get; set; } = 1f;

        private Color background;
        private Color border;
        private Color shadow;
        private Color contentColor;
        private Color buttonTextColor;
        private Color buttonBackground;
        private Color buttonBorder;
        private Color buttonHover;
        private Color buttonClick;
        private Color barBackground;
        private Color barFill;
        private Color barText;

        public Color Background
        {
            get => background.WithAlpha(ContentAlpha);
            set => background = value;
        }
        public Color Border
        {
            get => border.WithAlpha(ContentAlpha);
            set => border = value;
        }
        public Color Shadow
        {
            get => shadow.WithAlpha(ContentAlpha);
            set => shadow = value;
        }
        public Color ContentTint
        {
            get => contentColor.WithAlpha(ContentAlpha);
            set => contentColor = value;
        }
        public Color ButtonTextColor
        {
            get => buttonTextColor.WithAlpha(ContentAlpha);
            set => buttonTextColor = value;
        }
        public Color ButtonBackground
        {
            get => buttonBackground.WithAlpha(ContentAlpha);
            set => buttonBackground = value;
        }
        public Color ButtonBorder
        {
            get => buttonBorder.WithAlpha(ContentAlpha);
            set => buttonBorder = value;
        }
        public Color ButtonHover
        {
            get => buttonHover.WithAlpha(ContentAlpha);
            set => buttonHover = value;
        }
        public Color ButtonClick
        {
            get => buttonClick.WithAlpha(ContentAlpha);
            set => buttonClick = value;
        }
        public Color ProgressBarBackground
        {
            get => barBackground.WithAlpha(ContentAlpha);
            set => barBackground = value;
        }
        public Color ProgressBarFill
        {
            get => barFill.WithAlpha(ContentAlpha);
            set => barFill = value;
        }
        public Color BarText
        {
            get => barText.WithAlpha(ContentAlpha);
            set => barText = value;
        }

        public Color BackgroundRaw => background;
        public Color BorderRaw => border;
        public Color ShadowRaw => shadow;
        public Color ContentColorRaw => contentColor;

        public Color ButtonTextColorRaw => buttonTextColor;
        public Color ButtonBackgroundRaw => buttonBackground;
        public Color ButtonBorderRaw => buttonBorder;
        public Color ButtonHoverRaw => buttonHover;
        public Color ButtonClickRaw => buttonClick;

        public Color BarBackgroundRaw => barBackground;
        public Color BarFillRaw => barFill;
        public Color BarTextRaw => barText;

        // Common shadow sizing
        public float ShadowSizeLeft { get; set; }
        public float ShadowSizeTop { get; set; }
        public float ShadowSizeRight { get; set; } = 4f;
        public float ShadowSizeBottom { get; set; } = 4f;

        // Common animation curves
        public float AlphaSpeed { get; set; } = 0.5f;
        public Curve AlphaCurve { get; set; } = Curves.EaseOutBack;
        public Curve MoveAndScaleCurve { get; set; } = Curves.EaseOutBack;
        public float ContentMoveDuration { get; set; }
        public float ContentFadeDuration { get; set; }

        public bool ShowVerticalScrollBar { get; set; } = true;

        public bool AllowDragging { get; set; } = false;
        public float TitleBarHeight { get; set; } = 12;
        public RichText DragHintText { get; set; } = "Click to drag";
        public Color DragHintColor { get; set; } = Color.White;

        public Curve RaiseCurve { get; set; } = Curves.EaseOutBack;
        public float RaiseDuration { get; set; } = 0.25f;
        public bool RaiseOnHover { get; set; }
        public float HoverRaiseAmount { get; set; } = 4;
        internal float currentRaiseAmount;

        public float AnimateInDuration { get; set; } = 0.4f;
        public float AnimateOutDuration { get; set; } = 0.4f;

        /// <summary>
        /// Standard white faded to <see cref="ContentAlpha"/>
        /// </summary>
        public Color White => new Color(255, 255, 255, ContentAlpha);

        public float BaseButtonFontSize { get; set; } = 14;

        public Color TimerBarBackground { get; set; }
        public Color TimerBarFill { get; set; }
        public Font Font { get; set; }
        public float BorderSize { get; set; } = 2;
    }
}
