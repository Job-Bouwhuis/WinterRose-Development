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

    public FollowMouseAnchor(Vector2 size, float followSpeed) : this(size)
    {
        //FollowSpeed = followSpeed;
    }

    public Vector2 Size { get; set; }
    public Vector2 OffsetFromMouse { get; set; }
    public float FollowSpeed { get; set; } = 1f;

    public override Rectangle GetAnchorBounds()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        Vector2 targetPos = mousePos + OffsetFromMouse;

        return new Rectangle(targetPos.X - Size.X, targetPos.Y - Size.Y, Size.X, Size.Y);
    }

    public override bool IsAnchorValid(bool tooltipHovered)
    {
        // FollowMouseAnchor is always valid, external code must close the tooltip
        return true;
    }
}

