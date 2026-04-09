using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

public class FollowMouseAnchor : TooltipAnchor
{
    public FollowMouseAnchor(Vector2 size)
    {
        Size = size;
    }

    public Vector2 Size { get; set; }

    public override Rectangle GetAnchorBounds()
    {
        Vector2 mousePos = Tooltip.Input.Provider.MousePosition;

        return new Rectangle(Size.X,  Size.Y, Size.X, Size.Y);
    }

    public override bool IsAnchorValid(bool tooltipHovered)
    {
        // FollowMouseAnchor is always valid, external code must close the tooltip
        return true;
    }
}

