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
        OffsetFromMouse = new Vector2(30, 22);
    }

    public FollowMouseAnchor(Vector2 size, Vector2 offsetFromMouse)
    {
        Size = size;
        OffsetFromMouse = offsetFromMouse;
    }

    public Vector2 Size { get; set; }
    public Vector2 OffsetFromMouse { get; set; }

    public override Rectangle GetAnchorBounds()
    {
        if(!Tooltip.Behavior.AllowsInteraction)
            Tooltip.Input.Update(); // we need the provider to update for mouse input, so if the input system doesnt do it, we will
        Vector2 mousePos = Tooltip.Input.Provider.MousePosition;
        Vector2 targetPos = mousePos + OffsetFromMouse;

        return new Rectangle(targetPos.X - Size.X, targetPos.Y - Size.Y, Size.X, Size.Y);
    }

    public override bool IsAnchorValid(bool tooltipHovered)
    {
        // FollowMouseAnchor is always valid, external code must close the tooltip
        return true;
    }
}

