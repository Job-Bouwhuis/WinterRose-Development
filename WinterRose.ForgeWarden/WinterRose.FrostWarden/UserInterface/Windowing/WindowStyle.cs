using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface.Windowing;

public class WindowStyle : ContentStyle
{
    private Color titleBarBackground;
    private Color titleBarTextColor;

    private Color closeButtonBackground;
    private Color closeButtonHover;
    private Color closeButtonClick;

    private Color maximizeButtonBackground;
    private Color maximizeButtonHover;
    private Color maximizeButtonClick;

    private Color collapseButtonBackground;
    private Color collapseButtonHover;
    private Color collapseButtonClick;

    public WindowStyle(StyleBase baseStyle) : base(baseStyle)
    {
        RaiseDuration = 0.1f;

        // --- Base container ---
        Background = new Color(28, 24, 32, 230);          // deep charcoal with purple undertone
        Border = new Color(140, 90, 160, 255);            // muted purple border
        Shadow = new Color(0, 0, 0, 140);
        ContentTint = new Color(245, 235, 245);           // warm near-white

        // --- Progress bar ---
        ProgressBarBackground = new Color(55, 48, 62);
        ProgressBarFill = new Color(220, 90, 180);        // strong pink accent
        ProgressBarText = new Color(250, 245, 250);

        // --- Timer bar ---
        TimerBarBackground = new Color(50, 44, 58);
        TimerBarFill = new Color(200, 120, 210);          // pink-purple blend

        // --- Scrollbar ---
        ScrollbarTrack = new Color(45, 40, 52);
        ScrollbarThumb = new Color(120, 90, 135);

        // --- Buttons ---
        ButtonTextColor = new Color(250, 245, 250);
        ButtonBackground = new Color(70, 55, 80);         // dark purple-gray
        ButtonBorder = new Color(150, 110, 170);
        ButtonHover = new Color(95, 70, 115);
        ButtonClick = new Color(220, 90, 180);             // same pink accent as progress

        // --- Title bar ---
        titleBarBackground = new Color(34, 28, 42, 240);  // darker purple frame
        titleBarTextColor = new Color(255, 245, 255);

        // --- Close button ---
        closeButtonBackground = new Color(200, 70, 110);  // pink-red (less angry than pure red)
        closeButtonHover = new Color(220, 95, 135);
        closeButtonClick = new Color(240, 55, 95);

        // --- Maximize button ---
        maximizeButtonBackground = new Color(170, 90, 210); // violet-magenta
        maximizeButtonHover = new Color(190, 120, 230);
        maximizeButtonClick = new Color(150, 70, 190);

        // --- Collapse button ---
        collapseButtonBackground = new Color(180, 130, 210); // softer lavender
        collapseButtonHover = new Color(200, 155, 230);
        collapseButtonClick = new Color(160, 110, 190);

        // --- Behavior ---
        AllowUserResizing = true;
        TitleBarHeight = 30;
        RaiseOnHover = false;

        ShadowSizeTop = 0;
        ShadowSizeLeft = 0;
        ShadowSizeBottom = ShadowSizeRight;

        AutoScale = false;
        AllowUserResizing = true;
    }


    // --- Title Bar ---
    public Color TitleBarBackground
    {
        get => titleBarBackground.WithAlpha(ContentAlpha);
        set => titleBarBackground = value;
    }
    public Color TitleBarBackgroundRaw => titleBarBackground;

    public Color TitleBarTextColor
    {
        get => titleBarTextColor.WithAlpha(ContentAlpha);
        set => titleBarTextColor = value;
    }

    public Color TitleBarTextColorRaw => titleBarTextColor;

    // --- Close Button ---
    public Color CloseButtonBackground
    {
        get => closeButtonBackground.WithAlpha(ContentAlpha);
        set => closeButtonBackground = value;
    }
    public Color CloseButtonBackgroundRaw => closeButtonBackground;

    public Color CloseButtonHover
    {
        get => closeButtonHover.WithAlpha(ContentAlpha);
        set => closeButtonHover = value;
    }
    public Color CloseButtonHoverRaw => closeButtonHover;

    public Color CloseButtonClick
    {
        get => closeButtonClick.WithAlpha(ContentAlpha);
        set => closeButtonClick = value;
    }
    public Color CloseButtonClickRaw => closeButtonClick;

    // --- Maximize Button ---
    public Color MaximizeButtonBackground
    {
        get => maximizeButtonBackground.WithAlpha(ContentAlpha);
        set => maximizeButtonBackground = value;
    }
    public Color MaximizeButtonBackgroundRaw => maximizeButtonBackground;

    public Color MaximizeButtonHover
    {
        get => maximizeButtonHover.WithAlpha(ContentAlpha);
        set => maximizeButtonHover = value;
    }
    public Color MaximizeButtonHoverRaw => maximizeButtonHover;

    public Color MaximizeButtonClick
    {
        get => maximizeButtonClick.WithAlpha(ContentAlpha);
        set => maximizeButtonClick = value;
    }
    public Color MaximizeButtonClickRaw => maximizeButtonClick;

    // --- Collapse Button ---
    public Color CollapseButtonBackground
    {
        get => collapseButtonBackground.WithAlpha(ContentAlpha);
        set => collapseButtonBackground = value;
    }
    public Color CollapseButtonBackgroundRaw => collapseButtonBackground;

    public Color CollapseButtonHover
    {
        get => collapseButtonHover.WithAlpha(ContentAlpha);
        set => collapseButtonHover = value;
    }
    public Color CollapseButtonHoverRaw => collapseButtonHover;

    public Color CollapseButtonClick
    {
        get => collapseButtonClick.WithAlpha(ContentAlpha);
        set => collapseButtonClick = value;
    }
    public Color CollapseButtonClickRaw => collapseButtonClick;

    public bool ShowCloseButton { get; set; } = true;
    public bool ShowMaximizeButton { get;set; } = true;
    public bool ShowCollapseButton { get; set; } = true;
    public Color White => StyleBase.White;
}


