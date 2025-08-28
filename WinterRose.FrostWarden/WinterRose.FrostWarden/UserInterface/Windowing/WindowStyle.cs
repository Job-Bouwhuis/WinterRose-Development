using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface.Windowing;

public class WindowStyle : ContainerStyle
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

    public WindowStyle()
    {
        RaiseCurve = Curves.Linear;
        RaiseDuration = 0.1f;
        // --- Base container defaults (slightly stronger than dialog) ---
        Background = new Color(32, 32, 36, 220);         // dark background, a little less transparent
        Border = new Color(255, 255, 255, 255);          // subtle border, just a touch brighter
        Shadow = new Color(0, 0, 0, 120);                // slightly deeper shadow for window presence
        ContentTint = new Color(240, 240, 245);          // soft near-white for text and content

        // --- Progress bar ---
        ProgressBarBackground = new Color(65, 65, 70);   // dark muted background
        ProgressBarFill = new Color(0, 150, 240);        // strong blue accent
        BarText = new Color(245, 245, 245);              // softer white for text

        // --- Buttons ---
        ButtonTextColor = new Color(245, 245, 245);
        ButtonBackground = new Color(75, 75, 80);        // muted but distinct from background
        ButtonBorder = new Color(130, 130, 135);         // subtle but visible
        ButtonHover = new Color(100, 100, 110);          // slightly lighter hover
        ButtonClick = new Color(0, 150, 240);            // matches progress bar accent

        // --- Title bar ---
        titleBarBackground = new Color(36, 36, 42, 230); // slightly darker to frame content
        titleBarTextColor = new Color(250, 250, 250);    // crisp white for contrast

        // --- Close button ---
        closeButtonBackground = new Color(180, 60, 60);  // softer red (not too aggressive)
        closeButtonHover = new Color(200, 80, 80);       // lighter on hover
        closeButtonClick = new Color(220, 40, 40);       // sharper on click

        // --- Maximize button ---
        maximizeButtonBackground = new Color(70, 130, 210);   // blue, in line with accent
        maximizeButtonHover = new Color(90, 150, 230);
        maximizeButtonClick = new Color(50, 110, 190);

        // --- Collapse button ---
        collapseButtonBackground = new Color(70, 200, 130);   // green for minimize/collapse
        collapseButtonHover = new Color(90, 220, 150);
        collapseButtonClick = new Color(50, 180, 110);

        // --- Behavior defaults ---
        AllowDragging = true;
        TitleBarHeight = 30;
        DragHintText = "Drag window";
        DragHintColor = new Color(180, 180, 185);        // softer gray hint
        RaiseOnHover = false;

        ShadowSizeTop = 0;
        ShadowSizeLeft = 0;
        ShadowSizeBottom = ShadowSizeRight;
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
}


