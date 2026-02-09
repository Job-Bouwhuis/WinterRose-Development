using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

public class UIContentAnchor : TooltipAnchor
{
    public required UIContent Content { get; set; }

    [SetsRequiredMembers]
    public UIContentAnchor(UIContent anchor)
    {
        Content = anchor;
    }

    private UIContentAnchor() { } // for serialization

    public override Rectangle GetAnchorBounds()
    {
        return Content.LastRenderBounds;
    }
    public override bool IsAnchorValid()
    {
        return !Content.Owner.IsClosing || !Content.IsHovered;
    }
}
