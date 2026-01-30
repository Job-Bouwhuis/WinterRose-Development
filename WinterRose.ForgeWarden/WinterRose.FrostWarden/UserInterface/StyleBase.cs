using Microsoft.VisualStudio.TextManager.Interop;
using Raylib_cs;
using System.Diagnostics;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface
{
    public class StyleBase
    {
        public virtual void ResetState()
        {
            ContentAlpha = 1f;
            currentRaiseAmount = 0f;
        }

        protected internal float ContentAlpha { get; set; } = 1f;

        private Color background = new Color(28, 24, 32, 255);          // deep charcoal with purple undertone
        private Color border = new Color(120, 90, 140, 255);            // muted purple-gray border
        private Color shadow = new Color(0, 0, 0, 170);                 // slightly stronger shadow
        private Color contentColor = new Color(245, 235, 245, 255);     // warm near-white text

        private Color buttonTextColor = new Color(255, 245, 255, 255);  // soft white
        private Color buttonBackground = new Color(60, 48, 72, 255);    // dark purple-gray
        private Color buttonBorder = new Color(150, 110, 170, 255);     // pink-purple edge
        private Color buttonHover = new Color(85, 65, 105, 255);        // lifted purple hover
        private Color buttonClick = new Color(220, 90, 180, 255);       // strong pink accent

        private Color barBackground = new Color(44, 38, 52, 255);       // dark muted purple
        private Color barFill = new Color(255, 65, 225, 255);           // vibrant pink fill
        private Color barText = new Color(250, 245, 250, 255);          // high contrast text

        private Color timerBarBackground = new Color(48, 42, 56, 255);  // slightly lifted base
        private Color timerBarFill = new Color(200, 120, 210, 255);     // pink-purple urgency

        private Color scrollbarTrack = new Color(40, 35, 48, 255);      // subtle track
        private Color scrollbarThumb = new Color(120, 90, 140, 255);    // visible but calm thumb

        private Color textBoxBackgroundColor = new Color(22, 18, 28, 235); // deep inset purple-black
        private Color textBoxBorderColor = new Color(100, 80, 120, 255);  // soft purple border
        private Color textBoxFocusedBorderColor = new Color(210, 120, 190, 255); // pink focus glow
        private Color textBoxTextColor = new Color(250, 240, 250, 255);   // clean readable text
        private Color textBoxCaretColor = new Color(230, 110, 200, 255); // bright pink caret

        private Color panelBackground = new Color(30, 26, 36, 255);          // deep purple-charcoal panel
        private Color panelBorder = new Color(110, 85, 130, 255);            // muted lavender border
        private Color panelBackgroundDarker = new Color(20, 18, 26, 255);    // recessed panel depth

        private Color textSmall = new Color(210, 200, 215, 255);             // soft lavender-gray text

        private Color gridLine = new Color(70, 60, 85, 255);                 // subtle purple grid
        private Color axisLine = new Color(120, 95, 145, 255);               // clearer axis emphasis

        private Color tooltipBackground = new Color(36, 30, 44, 240);        // floating dark purple tooltip

        private Color treeNodeBorder = new Color(135, 105, 165, 255);        // lavender node outline
        private Color treeNodeText = new Color(245, 235, 245, 255);          // warm white text
        private Color treeNodeArrow = new Color(190, 150, 215, 255);         // glowing lavender arrow
        private Color treeNodeHover = new Color(170, 130, 200, 255);         // hover glow
        private Color treeNodeBackground = new Color(26, 22, 32, 255);       // node background

        private Color radioGroupSeparator = new Color(160, 110, 190, 255);   // pink-lavender divider
        private Color radioGroupBackground = new Color(32, 26, 44, 220);     // grouped dark purple
        private Color radioGroupBorder = new Color(110, 85, 140, 255);       // soft purple frame

        private Color sliderTick = new Color(200, 170, 215, 90);             // subtle lavender ticks
        private Color sliderFilled = new Color(200, 95, 185, 255);           // matches barFill accent

        private Color buttonDisabled = new Color(80, 70, 95, 255);   // muted purple-gray, clearly inactive
        private Color seperatorLineColor = new Color(60, 50, 75, 255); // subtle dark purple divider

        private Color checkBoxBusyIndicator = Color.White;

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
            set
            {
                if (value.A < 10)
                    ;
                background = value;
            }
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

        
        public Color PanelBackground
        {
            get => panelBackground.WithAlpha(ContentAlpha);
            set => panelBackground = value;
        }
        public Color PanelBackgroundRaw => panelBackground;

        public Color PanelBorder
        {
            get => panelBorder.WithAlpha(ContentAlpha);
            set => panelBorder = value;
        }
        public Color PanelBorderRaw => panelBorder;

        public Color PanelBackgroundDarker
        {
            get => panelBackgroundDarker.WithAlpha(ContentAlpha);
            set => panelBackgroundDarker = value;
        }
        public Color PanelBackgroundDarkerRaw => panelBackgroundDarker;

        public Color TextSmall
        {
            get => textSmall.WithAlpha(ContentAlpha);
            set => textSmall = value;
        }
        public Color TextSmallRaw => textSmall;

        public Color TooltipBackground
        {
            get => tooltipBackground.WithAlpha(ContentAlpha);
            set => tooltipBackground = value;
        }
        public Color TooltipBackgroundRaw => tooltipBackground;

        public Color GridLine
        {
            get => gridLine.WithAlpha(ContentAlpha);
            set => gridLine = value;
        }
        public Color GridLineRaw => gridLine;

        public Color AxisLine
        {
            get => axisLine.WithAlpha(ContentAlpha);
            set => axisLine = value;
        }
        public Color AxisLineRaw => axisLine;

        public Color TreeNodeBorder
        {
            get => treeNodeBorder.WithAlpha(ContentAlpha);
            set => treeNodeBorder = value;
        }
        public Color TreeNodeBorderRaw => treeNodeBorder;

        public Color TreeNodeText
        {
            get => treeNodeText.WithAlpha(ContentAlpha);
            set => treeNodeText = value;
        }
        public Color TreeNodeTextRaw => treeNodeText;

        public Color TreeNodeArrow
        {
            get => treeNodeArrow.WithAlpha(ContentAlpha);
            set => treeNodeArrow = value;
        }
        public Color TreeNodeArrowRaw => treeNodeArrow;

        public Color TreeNodeHover
        {
            get => treeNodeHover.WithAlpha(ContentAlpha);
            set => treeNodeHover = value;
        }
        public Color TreeNodeHoverRaw => treeNodeHover;

        public Color TreeNodeBackground
        {
            get => treeNodeBackground.WithAlpha(ContentAlpha);
            set => treeNodeBackground = value;
        }
        public Color TreeNodeBackgroundRaw => treeNodeBackground;

        public Color RadioGroupAccent
        {
            get => radioGroupSeparator.WithAlpha(ContentAlpha);
            set => radioGroupSeparator = value;
        }
        public Color RadioGroupAccentRaw => radioGroupSeparator;

        public Color RadioGroupBackground
        {
            get => radioGroupBackground.WithAlpha(ContentAlpha);
            set => radioGroupBackground = value;
        }
        public Color RadioGroupBackgroundRaw => radioGroupBackground;

        public Color SliderTick
        {
            get => sliderTick.WithAlpha(ContentAlpha);
            set => sliderTick = value;
        }
        public Color SliderTickRaw => sliderTick;

        public Color SliderFilled
        {
            get => sliderFilled.WithAlpha(ContentAlpha);
            set => sliderFilled = value;
        }
        public Color SliderFilledRaw => sliderFilled;

        public Color RadioGroupBorder
        {
            get => radioGroupBorder.WithAlpha(ContentAlpha);
            set => radioGroupBorder = value;
        }
        public Color RadioGroupBorderRaw => radioGroupBorder;


        public Color ButtonDisabled
        {
            get => buttonDisabled.WithAlpha(ContentAlpha);
            set => buttonDisabled = value;
        }

        public Color SeperatorLineColor
        {
            get => seperatorLineColor.WithAlpha(ContentAlpha);
            set => seperatorLineColor = value;
        }

        public Color CheckBoxBusyIndicator
        {
            get => seperatorLineColor.WithAlpha(ContentAlpha);
            set => seperatorLineColor = value;
        }

        public Color CheckBoxBusyIndicatorRaw => checkBoxBusyIndicator;
        public Color ButtonDisabledRaw => buttonDisabled;
        public Color SeperatorLineColorRaw => seperatorLineColor;
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
        public Curve AlphaCurve { get; set; }
        public Curve MoveAndScaleCurve { get; set; }
        public float ContentMoveDuration { get; set; }
        public float ContentFadeDuration { get; set; }

        public bool ShowVerticalScrollBar { get; set; } = true;

        public bool AllowUserResizing { get; set; } = false;
        public float TitleBarHeight { get; set; } = 12;

        public Curve RaiseCurve { get; set; }

        public StyleBase()
        {
            AlphaCurve = Curves.EaseOutBack;
            MoveAndScaleCurve = Curves.ExtraSlowFastSlow;
            RaiseCurve = Curves.ExtraSlowFastSlow;
        }

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

        public int BaseButtonFontSize { get; set; } = 18;

        public Font Font { get; set; }
        public float BorderSize { get; set; } = 2;

        public bool AutoScale
        {
            get;
            set;
        }

        /// <summary>
        /// time, in seconds, until the container automatically closes
        /// </summary>
        public float TimeUntilAutoDismiss { get; set; }

        public float CaretBlinkingRate { get; set; } = 0.25f; // seconds
        public float CaretWidth { get; set; } = 2;
        public float TextBoxPadding { get; set; } = 6f;
        public float TextBoxFontSize { get; set; } = 16f;
        public float TextBoxTextSpacing { get; set; } = 2f;
        public float TextBoxMinHeight { get; set; } = 20f;
        public float TreeNodeHeight { get; set; } = 25;
        public float TreeNodeIndentWidth { get; set; } = 16;
        public double DoubleClickSeconds { get; set; } = 0.15;
        public bool PauseAutoDismissTimer { get; set; }
        public int MaxAutoScaleWidth { get; set; } = 500;
        public int MaxAutoScaleHeight { get; set; } = 700;
        public float AutoScrollSpeed { get; set; } = 50;
    }

    public class ContentStyle
    {
        internal StyleBase StyleBase { get; set; }
        public float ContentAlpha
        {
            get => StyleBase.ContentAlpha;
            set => StyleBase.ContentAlpha = value;
        }
        public bool FollowContainerDefaults { get; set; } = true;

        // Colors
        public Overridable<Color> Background
        {
            get;
            set
            {
                StyleBase.Background = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if(field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> Border
        {
            get;
            set
            {
                StyleBase.Border = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> Shadow
        {
            get;
            set
            {
                StyleBase.Shadow = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ContentTint
        {
            get;
            set
            {
                StyleBase.ContentTint = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ButtonTextColor
        {
            get;
            set
            {
                StyleBase.ButtonTextColor = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ButtonBackground
        {
            get;
            set
            {
                StyleBase.ButtonBackground = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ButtonBorder
        {
            get;
            set
            {
                StyleBase.ButtonBorder = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ButtonHover
        {
            get;
            set
            {
                StyleBase.ButtonHover = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ButtonClick
        {
            get;
            set
            {
                StyleBase.ButtonClick = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TextSmall
        {
            get;
            set
            {
                StyleBase.TextSmall = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Font> Font
        {
            get;
            set
            {
                StyleBase.Font = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<int> BaseButtonFontSize
        {
            get;
            set
            {
                StyleBase.BaseButtonFontSize = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TextBoxBackground
        {
            get;
            set
            {
                StyleBase.TextBoxBackground = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TextBoxBorder
        {
            get;
            set
            {
                StyleBase.TextBoxBorder = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TextBoxFocusedBorder
        {
            get;
            set
            {
                StyleBase.TextBoxFocusedBorder = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TextBoxText
        {
            get;
            set
            {
                StyleBase.TextBoxText = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> Caret
        {
            get;
            set
            {
                StyleBase.Caret = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ProgressBarBackground
        {
            get;
            set
            {
                StyleBase.ProgressBarBackground = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ProgressBarFill
        {
            get;
            set
            {
                StyleBase.ProgressBarFill = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> BarText
        {
            get;
            set
            {
                StyleBase.BarText = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TimerBarBackground
        {
            get;
            set
            {
                StyleBase.TimerBarBackground = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TimerBarFill
        {
            get;
            set
            {
                StyleBase.TimerBarFill = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ScrollbarThumb
        {
            get;
            set
            {
                StyleBase.ScrollbarThumb = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> ScrollbarTrack
        {
            get;
            set
            {
                StyleBase.ScrollbarTrack = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> PanelBackground
        {
            get;
            set
            {
                StyleBase.PanelBackground = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> PanelBorder
        {
            get;
            set
            {
                StyleBase.PanelBorder = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> PanelBackgroundDarker
        {
            get;
            set
            {
                StyleBase.PanelBackgroundDarker = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TooltipBackground
        {
            get;
            set
            {
                StyleBase.TooltipBackground = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> GridLine
        {
            get;
            set
            {
                StyleBase.GridLine = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> AxisLine
        {
            get;
            set
            {
                StyleBase.AxisLine = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TreeNodeBorder
        {
            get;
            set
            {
                StyleBase.TreeNodeBorder = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TreeNodeText
        {
            get;
            set
            {
                StyleBase.TreeNodeText = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TreeNodeArrow
        {
            get;
            set
            {
                StyleBase.TreeNodeArrow = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TreeNodeHover
        {
            get;
            set
            {
                StyleBase.TreeNodeHover = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> TreeNodeBackground
        {
            get;
            set
            {
                StyleBase.TreeNodeBackground = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> RadioGroupAccent
        {
            get;
            set
            {
                StyleBase.RadioGroupAccent = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> RadioGroupBackground
        {
            get;
            set
            {
                StyleBase.RadioGroupBackground = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> RadioGroupBorder
        {
            get;
            set
            {
                StyleBase.RadioGroupBorder = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> SliderTick
        {
            get;
            set
            {
                StyleBase.SliderTick = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> SliderFilled
        {
            get;
            set
            {
                StyleBase.SliderFilled = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Color> SeperatorLineColor
        {
            get;
            set
            {
                StyleBase.SeperatorLineColor = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
             
        // Numbers, floats, ints
        public Overridable<float> ShadowSizeLeft
        {
            get;
            set
            {
                StyleBase.ShadowSizeLeft = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> ShadowSizeTop
        {
            get;
            set
            {
                StyleBase.ShadowSizeTop = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> ShadowSizeRight
        {
            get;
            set
            {
                StyleBase.ShadowSizeRight = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> ShadowSizeBottom
        {
            get;
            set
            {
                StyleBase.ShadowSizeBottom = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> AlphaSpeed
        {
            get;
            set
            {
                StyleBase.AlphaSpeed = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Curve> AlphaCurve
        {
            get;
            set
            {
                StyleBase.AlphaCurve = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Curve> MoveAndScaleCurve
        {
            get;
            set
            {
                StyleBase.MoveAndScaleCurve = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> ContentMoveDuration
        {
            get;
            set
            {
                StyleBase.ContentMoveDuration = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> ContentFadeDuration
        {
            get;
            set
            {
                StyleBase.ContentFadeDuration = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<bool> ShowVerticalScrollBar
        {
            get;
            set
            {
                StyleBase.ShowVerticalScrollBar = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<bool> AllowUserResizing
        {
            get;
            set
            {
                StyleBase.AllowUserResizing = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> TitleBarHeight
        {
            get;
            set
            {
                StyleBase.TitleBarHeight = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<Curve> RaiseCurve
        {
            get;
            set
            {
                StyleBase.RaiseCurve = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> RaiseDuration
        {
            get;
            set
            {
                StyleBase.RaiseDuration = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<bool> RaiseOnHover
        {
            get;
            set
            {
                StyleBase.RaiseOnHover = value;

                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> HoverRaiseAmount
        {
            get;
            set
            {
                StyleBase.HoverRaiseAmount = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> AnimateInDuration
        {
            get;
            set
            {
                StyleBase.AnimateInDuration = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> AnimateOutDuration
        {
            get;
            set
            {
                StyleBase.AnimateOutDuration = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> TimeUntilAutoDismiss
        {
            get;
            set
            {
                StyleBase.TimeUntilAutoDismiss = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> CaretBlinkingRate
        {
            get;
            set
            {
                StyleBase.CaretBlinkingRate = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> CaretWidth
        {
            get;
            set
            {
                StyleBase.CaretWidth = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> TextBoxPadding
        {
            get;
            set
            {
                StyleBase.TextBoxPadding = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> TextBoxFontSize
        {
            get;
            set
            {
                StyleBase.TextBoxFontSize = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> TextBoxTextSpacing
        {
            get;
            set
            {
                StyleBase.TextBoxTextSpacing = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> TextBoxMinHeight
        {
            get;
            set
            {
                StyleBase.TextBoxMinHeight = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> TreeNodeHeight
        {
            get;
            set
            {
                StyleBase.TreeNodeHeight = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> TreeNodeIndentWidth
        {
            get;
            set
            {
                StyleBase.TreeNodeIndentWidth = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<double> DoubleClickSeconds
        {
            get;
            set
            {
                StyleBase.DoubleClickSeconds = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<bool> PauseAutoDismissTimer
        {
            get;
            set
            {
                StyleBase.PauseAutoDismissTimer = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<int> MaxAutoScaleWidth
        {
            get;
            set
            {
                StyleBase.MaxAutoScaleWidth = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<int> MaxAutoScaleHeight
        {
            get;
            set
            {
                StyleBase.MaxAutoScaleHeight = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> AutoScrollSpeed
        {
            get;
            set
            {
                StyleBase.AutoScrollSpeed = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<bool> AutoScale
        {
            get;
            set
            {
                StyleBase.AutoScale = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Overridable<float> BorderSize
        {
            get;
            set
            {
                StyleBase.BorderSize = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }

        public Overridable<Color> ButtonDisabled
        {
            get;
            set
            {
                StyleBase.ButtonDisabled = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }

        public Overridable<Color> CheckBoxBusyIndicator
        {
            get;
            set
            {
                StyleBase.CheckBoxBusyIndicator = value;
                if (field is not null)
                {
                    var oldOverride = field.Override;
                    var oldFallback = field.fallbackResolver;

                    if (field.IsOverridden)
                        value.Override = oldOverride;
                    value.fallbackResolver = oldFallback;

                    field = value;
                }
                else
                    field = value;
            }
        }
        public Color White => StyleBase.White;

        

        public ContentStyle(StyleBase parent)
        {
            StyleBase = parent;

            // Colors
            Background = new Overridable<Color>(() => FollowContainerDefaults ? parent.BackgroundRaw : Color.White);
            Border = new Overridable<Color>(() => FollowContainerDefaults ? parent.BorderRaw : Color.Black);
            Shadow = new Overridable<Color>(() => FollowContainerDefaults ? parent.ShadowRaw : Color.Black);
            ContentTint = new Overridable<Color>(() => FollowContainerDefaults ? parent.ContentColorRaw : Color.White);

            ButtonTextColor = new Overridable<Color>(() => FollowContainerDefaults ? parent.ButtonTextColorRaw : Color.White);
            ButtonBackground = new Overridable<Color>(() => FollowContainerDefaults ? parent.ButtonBackgroundRaw : Color.Gray);
            ButtonBorder = new Overridable<Color>(() => FollowContainerDefaults ? parent.ButtonBorderRaw : Color.DarkGray);
            ButtonHover = new Overridable<Color>(() => FollowContainerDefaults ? parent.ButtonHoverRaw : Color.LightGray);
            ButtonClick = new Overridable<Color>(() => FollowContainerDefaults ? parent.ButtonClickRaw : Color.Gray);

            TextSmall = new Overridable<Color>(() => FollowContainerDefaults ? parent.TextSmallRaw : Color.White);
            Font = new Overridable<Font>(() => FollowContainerDefaults ? parent.Font : Font.Default);
            BaseButtonFontSize = new Overridable<int>(() => FollowContainerDefaults ? parent.BaseButtonFontSize : 14);

            TextBoxBackground = new Overridable<Color>(() => FollowContainerDefaults ? parent.TextBoxBackgroundRaw : Color.White);
            TextBoxBorder = new Overridable<Color>(() => FollowContainerDefaults ? parent.TextBoxBorderRaw : Color.Black);
            TextBoxFocusedBorder = new Overridable<Color>(() => FollowContainerDefaults ? parent.TextBoxFocusedBorderRaw : Color.Gray);
            TextBoxText = new Overridable<Color>(() => FollowContainerDefaults ? parent.TextBoxTextRaw : Color.White);
            Caret = new Overridable<Color>(() => FollowContainerDefaults ? parent.CaretRaw : Color.Yellow);

            ProgressBarBackground = new Overridable<Color>(() => FollowContainerDefaults ? parent.BarBackgroundRaw : Color.Black);
            ProgressBarFill = new Overridable<Color>(() => FollowContainerDefaults ? parent.BarFillRaw : Color.White);
            BarText = new Overridable<Color>(() => FollowContainerDefaults ? parent.BarTextRaw : Color.White);

            TimerBarBackground = new Overridable<Color>(() => FollowContainerDefaults ? parent.TimerBarBackgroundRaw : Color.Black);
            TimerBarFill = new Overridable<Color>(() => FollowContainerDefaults ? parent.TimerBarFillRaw : Color.White);

            ScrollbarThumb = new Overridable<Color>(() => FollowContainerDefaults ? parent.ScrollbarThumbRaw : Color.Gray);
            ScrollbarTrack = new Overridable<Color>(() => FollowContainerDefaults ? parent.ScrollbarTrackRaw : Color.DarkGray);

            PanelBackground = new Overridable<Color>(() => FollowContainerDefaults ? parent.PanelBackgroundRaw : Color.Black);
            PanelBorder = new Overridable<Color>(() => FollowContainerDefaults ? parent.PanelBorderRaw : Color.Gray);
            PanelBackgroundDarker = new Overridable<Color>(() => FollowContainerDefaults ? parent.PanelBackgroundDarkerRaw : Color.DarkGray);

            TooltipBackground = new Overridable<Color>(() => FollowContainerDefaults ? parent.TooltipBackgroundRaw : Color.Black);
            GridLine = new Overridable<Color>(() => FollowContainerDefaults ? parent.GridLineRaw : Color.Gray);
            AxisLine = new Overridable<Color>(() => FollowContainerDefaults ? parent.AxisLineRaw : Color.Gray);

            TreeNodeBorder = new Overridable<Color>(() => FollowContainerDefaults ? parent.TreeNodeBorderRaw : Color.Gray);
            TreeNodeText = new Overridable<Color>(() => FollowContainerDefaults ? parent.TreeNodeTextRaw : Color.White);
            TreeNodeArrow = new Overridable<Color>(() => FollowContainerDefaults ? parent.TreeNodeArrowRaw : Color.LightGray);
            TreeNodeHover = new Overridable<Color>(() => FollowContainerDefaults ? parent.TreeNodeHoverRaw : Color.LightGray);
            TreeNodeBackground = new Overridable<Color>(() => FollowContainerDefaults ? parent.TreeNodeBackgroundRaw : Color.DarkGray);

            RadioGroupAccent = new Overridable<Color>(() => FollowContainerDefaults ? parent.RadioGroupAccentRaw : Color.Blue);
            RadioGroupBackground = new Overridable<Color>(() => FollowContainerDefaults ? parent.RadioGroupBackgroundRaw : Color.DarkBlue);
            RadioGroupBorder = new Overridable<Color>(() => FollowContainerDefaults ? parent.RadioGroupBorderRaw : Color.Gray);

            SliderTick = new Overridable<Color>(() => FollowContainerDefaults ? parent.SliderTickRaw : Color.Gray);
            SliderFilled = new Overridable<Color>(() => FollowContainerDefaults ? parent.SliderFilledRaw : Color.Blue);
            CheckBoxBusyIndicator = new Overridable<Color>(() => FollowContainerDefaults ? parent.CheckBoxBusyIndicatorRaw : Color.White);

            // Numbers, floats, ints
            ShadowSizeLeft = new Overridable<float>(() => FollowContainerDefaults ? parent.ShadowSizeLeft : 0f);
            ShadowSizeTop = new Overridable<float>(() => FollowContainerDefaults ? parent.ShadowSizeTop : 0f);
            ShadowSizeRight = new Overridable<float>(() => FollowContainerDefaults ? parent.ShadowSizeRight : 4f);
            ShadowSizeBottom = new Overridable<float>(() => FollowContainerDefaults ? parent.ShadowSizeBottom : 4f);

            AlphaSpeed = new Overridable<float>(() => FollowContainerDefaults ? parent.AlphaSpeed : 0.5f);
            AlphaCurve = new Overridable<Curve>(() => FollowContainerDefaults ? parent.AlphaCurve : Curves.EaseOutBack);
            MoveAndScaleCurve = new Overridable<Curve>(() => FollowContainerDefaults ? parent.MoveAndScaleCurve : Curves.EaseOutBack);
            ContentMoveDuration = new Overridable<float>(() => FollowContainerDefaults ? parent.ContentMoveDuration : 0f);
            ContentFadeDuration = new Overridable<float>(() => FollowContainerDefaults ? parent.ContentFadeDuration : 0f);

            ShowVerticalScrollBar = new Overridable<bool>(() => !FollowContainerDefaults || parent.ShowVerticalScrollBar);

            AllowUserResizing = new Overridable<bool>(() => FollowContainerDefaults && parent.AllowUserResizing);
            TitleBarHeight = new Overridable<float>(() => FollowContainerDefaults ? parent.TitleBarHeight : 12f);

            RaiseCurve = new Overridable<Curve>(() => FollowContainerDefaults ? parent.RaiseCurve : Curves.EaseOutBack);
            RaiseDuration = new Overridable<float>(() => FollowContainerDefaults ? parent.RaiseDuration : 0.25f);
            RaiseOnHover = new Overridable<bool>(() => FollowContainerDefaults && parent.RaiseOnHover);
            HoverRaiseAmount = new Overridable<float>(() => FollowContainerDefaults ? parent.HoverRaiseAmount : 4f);

            AnimateInDuration = new Overridable<float>(() => FollowContainerDefaults ? parent.AnimateInDuration : 0.4f);
            AnimateOutDuration = new Overridable<float>(() => FollowContainerDefaults ? parent.AnimateOutDuration : 0.4f);

            TimeUntilAutoDismiss = new Overridable<float>(() => FollowContainerDefaults ? parent.TimeUntilAutoDismiss : 0f);

            CaretBlinkingRate = new Overridable<float>(() => FollowContainerDefaults ? parent.CaretBlinkingRate : 0.25f);
            CaretWidth = new Overridable<float>(() => FollowContainerDefaults ? parent.CaretWidth : 2f);
            TextBoxPadding = new Overridable<float>(() => FollowContainerDefaults ? parent.TextBoxPadding : 6f);
            TextBoxFontSize = new Overridable<float>(() => FollowContainerDefaults ? parent.TextBoxFontSize : 16f);
            TextBoxTextSpacing = new Overridable<float>(() => FollowContainerDefaults ? parent.TextBoxTextSpacing : 2f);
            TextBoxMinHeight = new Overridable<float>(() => FollowContainerDefaults ? parent.TextBoxMinHeight : 20f);
            TreeNodeHeight = new Overridable<float>(() => FollowContainerDefaults ? parent.TreeNodeHeight : 20f);
            TreeNodeIndentWidth = new Overridable<float>(() => FollowContainerDefaults ? parent.TreeNodeIndentWidth : 16f);
            DoubleClickSeconds = new Overridable<double>(() => FollowContainerDefaults ? parent.DoubleClickSeconds : 0.15);
            PauseAutoDismissTimer = new Overridable<bool>(() => FollowContainerDefaults && parent.PauseAutoDismissTimer);
            MaxAutoScaleWidth = new Overridable<int>(() => FollowContainerDefaults ? parent.MaxAutoScaleWidth : 500);
            MaxAutoScaleHeight = new Overridable<int>(() => FollowContainerDefaults ? parent.MaxAutoScaleHeight : 700);
            AutoScrollSpeed = new Overridable<float>(() => FollowContainerDefaults ? parent.AutoScrollSpeed : 50f);
            AutoScale = new Overridable<bool>(() => FollowContainerDefaults && parent.AutoScale);
            BorderSize = new Overridable<float>(() => FollowContainerDefaults ? parent.BorderSize : 2f);
        }
    }


    public static class Ext
    {
        extension(Overridable<Font> f)
        {
            public Font Default => ForgeWardenEngine.DefaultFont;
        }
    }

    [DebuggerDisplay("Value={Value}")]
    public class Overridable<T>
    {
        private T? overrideValue;
        internal Func<T> fallbackResolver;

        public Overridable(Func<T> fallback)
        {
            fallbackResolver = fallback;
        }

        public static implicit operator T(Overridable<T> o)
        {
            if (o == null)
                return default;
            return o.Value;
        }

        public static implicit operator Overridable<T>(T t) => new(() => t);

        public T Value
        {
            get
            {

                if (!IsOverridden)
                    return fallbackResolver();
                return overrideValue;
            }
        }

        public bool IsOverridden
        {
            get
            {
                if (overrideValue is T t && !t.Equals(default))
                    return false;
                if (typeof(T).IsClass)
                    if (overrideValue is null)
                        return false;
                return true;
            }
        }

        public override string ToString() => Value?.ToString() ?? "null";

        public T? Override
        {
            get => overrideValue;
            set => overrideValue = value;
        }
    }

}
