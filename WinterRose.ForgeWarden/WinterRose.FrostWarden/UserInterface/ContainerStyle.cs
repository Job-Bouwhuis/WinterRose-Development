using Raylib_cs;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface
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
        private Color timerBarBackground;
        private Color timerBarFill;
        private Color scrollbarTrack;
        private Color scrollbarThumb;

        // --- visuals ---
        private Color textBoxBackgroundColor = new Color(24, 24, 24, 230);
        private Color textBoxBorderColor = new Color(80, 80, 80, 255);
        private Color textBoxFocusedBorderColor = new Color(140, 140, 140, 255);
        private Color textBoxTextColor = Color.White;
        private Color textBoxCaretColor = Color.Yellow;

        public Color TextBoxBackground
        {
            get => textBoxBackgroundColor.WithAlpha(ContentAlpha);
            set => textBoxBackgroundColor = value;
        }

        public Color TextBoxBorder
        {
            get => textBoxBorderColor.WithAlpha(ContentAlpha);
            set => textBoxBorderColor = value;
        }

        public Color TextBoxFocusedBorder
        {
            get => textBoxFocusedBorderColor.WithAlpha(ContentAlpha);
            set => textBoxFocusedBorderColor = value;
        }

        public Color TextBoxText
        {
            get => textBoxTextColor.WithAlpha(ContentAlpha);
            set => textBoxTextColor = value;
        }

        public Color Caret
        {
            get => textBoxCaretColor.WithAlpha(ContentAlpha);
            set => textBoxCaretColor = value;
        }


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

        public Color TimerBarBackground
        {
            get => timerBarBackground.WithAlpha(ContentAlpha);
            set => timerBarBackground = value;
        }

        public Color TimerBarFill
        {
            get => timerBarFill.WithAlpha(ContentAlpha);
            set => timerBarFill = value;
        }

        public Color ScrollbarTrack
        {
            get => scrollbarTrack.WithAlpha(ContentAlpha);
            set => scrollbarTrack = value;
        }

        public Color ScrollbarThumb
        {
            get => scrollbarThumb.WithAlpha(ContentAlpha);
            set => scrollbarThumb = value;
        }

        private Color panelBackground = new Color(32, 32, 32, 255);
        public Color PanelBackground
        {
            get => panelBackground.WithAlpha(ContentAlpha);
            set => panelBackground = value;
        }
        public Color PanelBackgroundRaw => panelBackground;

        private Color panelBorder = new Color(80, 80, 80, 255);
        public Color PanelBorder
        {
            get => panelBorder.WithAlpha(ContentAlpha);
            set => panelBorder = value;
        }
        public Color PanelBorderRaw => panelBorder;

        private Color panelBackgroundDarker = new Color(20, 20, 20, 255);
        public Color PanelBackgroundDarker
        {
            get => panelBackgroundDarker.WithAlpha(ContentAlpha);
            set => panelBackgroundDarker = value;
        }
        public Color PanelBackgroundDarkerRaw => panelBackgroundDarker;

        private Color textSmall = new Color(200, 200, 200, 255);
        public Color TextSmall
        {
            get => textSmall.WithAlpha(ContentAlpha);
            set => textSmall = value;
        }
        public Color TextSmallRaw => textSmall;

        private Color tooltipBackground = new Color(40, 40, 40, 240);
        public Color TooltipBackground
        {
            get => tooltipBackground.WithAlpha(ContentAlpha);
            set => tooltipBackground = value;
        }
        public Color TooltipBackgroundRaw => tooltipBackground;

        private Color gridLine = new Color(60, 60, 60, 255);
        public Color GridLine
        {
            get => gridLine.WithAlpha(ContentAlpha);
            set => gridLine = value;
        }
        public Color GridLineRaw => gridLine;

        private Color axisLine = new Color(100, 100, 100, 255);
        public Color AxisLine
        {
            get => axisLine.WithAlpha(ContentAlpha);
            set => axisLine = value;
        }
        public Color AxisLineRaw => axisLine;

        private Color treeNodeBorder = new Color(180, 180, 180, 255);
        public Color TreeNodeBorder
        {
            get => treeNodeBorder.WithAlpha(ContentAlpha);
            set => treeNodeBorder = value;
        }
        public Color TreeNodeBorderRaw => treeNodeBorder;

        private Color treeNodeText = new Color(255, 255, 255, 255);
        public Color TreeNodeText
        {
            get => treeNodeText.WithAlpha(ContentAlpha);
            set => treeNodeText = value;
        }
        public Color TreeNodeTextRaw => treeNodeText;

        private Color treeNodeArrow = new Color(200, 200, 255, 255);
        public Color TreeNodeArrow
        {
            get => treeNodeArrow.WithAlpha(ContentAlpha);
            set => treeNodeArrow = value;
        }
        public Color TreeNodeArrowRaw => treeNodeArrow;

        private Color treeNodeHover = new Color(140, 140, 220, 255);
        public Color TreeNodeHover
        {
            get => treeNodeHover.WithAlpha(ContentAlpha);
            set => treeNodeHover = value;
        }
        public Color TreeNodeHoverRaw => treeNodeHover;

        private Color treeNodeBackground = new Color(20, 20, 20, 255);
        public Color TreeNodeBackground
        {
            get => treeNodeBackground.WithAlpha(ContentAlpha);
            set => treeNodeBackground = value;
        }
        public Color TreeNodeBackgroundRaw => treeNodeBackground;

        private Color radioGroupSeparator = new Color(20, 20, 200, 255);
        public Color RadioGroupAccent
        {
            get => radioGroupSeparator.WithAlpha(ContentAlpha);
            set => radioGroupSeparator = value;
        }
        public Color RadioGroupAccentRaw => radioGroupSeparator;

        private Color radioGroupBackground = new Color(20, 20, 100, 200);
        public Color RadioGroupBackground
        {
            get => radioGroupBackground.WithAlpha(ContentAlpha);
            set => radioGroupBackground = value;
        }
        public Color RadioGroupBackgroundRaw => radioGroupBackground;

        private Color sliderTick = new Color(200, 200, 200, 80);
        public Color SliderTick
        {
            get => sliderTick.WithAlpha(ContentAlpha);
            set => sliderTick = value;
        }
        public Color SliderTickRaw => sliderTick;

        private Color sliderFilled = new Color(20, 20, 100, 255);
        public Color SliderFilled
        {
            get => sliderFilled.WithAlpha(ContentAlpha);
            set => sliderFilled = value;
        }
        public Color SliderFilledRaw => sliderFilled;

        private Color radioGroupBorder = new Color(80, 80, 80, 255);
        public Color RadioGroupBorder
        {
            get => radioGroupBorder.WithAlpha(ContentAlpha);
            set => radioGroupBorder = value;
        }
        public Color RadioGroupBorderRaw => radioGroupBorder;

        public Color ScrollbarThumbRaw => scrollbarThumb;
        public Color ScrollbarTrackRaw => scrollbarTrack;
        public Color TimerBarBackgroundRaw => timerBarBackground;
        public Color TimerBarFillRaw => timerBarFill;
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

        public Color TextBoxBackgroundRaw => textBoxBackgroundColor;
        public Color TextBoxBorderRaw => textBoxBorderColor;
        public Color TextBoxFocusedBorderRaw => textBoxFocusedBorderColor;
        public Color TextBoxTextRaw => textBoxTextColor;
        public Color CaretRaw => textBoxCaretColor;

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

        public bool AllowUserResizing { get; set; } = false;
        public float TitleBarHeight { get; set; } = 12;

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

        public Font Font { get; set; }
        public float BorderSize { get; set; } = 2;
        public bool AutoScale { get; internal set; }
        public int TimeUntilAutoDismiss { get; set; }

        public float CaretBlinkingRate { get; set; } = 0.25f; // seconds
        public float CaretWidth { get; set; } = 2;
        public float TextBoxPadding { get; set; } = 6f;
        public float TextBoxFontSize { get; set; } = 16f;
        public float TextBoxTextSpacing { get; set; } = 2f;
        public float TextBoxMinHeight { get; set; } = 20f;
        public float TreeNodeHeight { get; set; } = 20;
        public float TreeNodeIndentWidth { get; set; } = 16;
        public double DoubleClickSeconds { get; set; } = 0.15;
    }
}
