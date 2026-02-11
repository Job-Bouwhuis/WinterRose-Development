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
        return Content.LastRenderBounds with 
        { 
            X = Content.LastRenderBounds.X - 5,
            Y = Content.LastRenderBounds.Y - Content.LastRenderBounds.Height
        };
    }
    public override bool IsAnchorValid(bool tooltipHovered)
    {
        return true;
        if (!Content.IsContentHovered() && !tooltipHovered)
            ;

        bool res = !(Content.Owner.IsClosing || !Content.IsContentHovered());
        if (!res)
            ;
        return res;
    }
}
