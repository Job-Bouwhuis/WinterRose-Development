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
        Rectangle bounds = Content.LastRenderBounds;

        Vector2 windowSize = ForgeWardenEngine.Current.Window.Size;
        Rectangle viewport = new Rectangle(0, 0, (int)windowSize.X, (int)windowSize.Y);

        int tooltipWidth = (int)Tooltip.TargetSize.X;
        int tooltipHeight = (int)Tooltip.TargetSize.Y;

        int gap = 8;

        Rectangle right = new Rectangle(
            bounds.Right + gap,
            bounds.Top,
            tooltipWidth,
            tooltipHeight
        );

        Rectangle left = new Rectangle(
            bounds.Left - gap - tooltipWidth,
            bounds.Top,
            tooltipWidth,
            tooltipHeight
        );

        Rectangle below = new Rectangle(
            bounds.Left,
            bounds.Bottom + gap,
            tooltipWidth,
            tooltipHeight
        );

        Rectangle above = new Rectangle(
            bounds.Left,
            bounds.Top - gap - tooltipHeight,
            tooltipWidth,
            tooltipHeight
        );

        if (viewport.Contains(right))
            return right;

        if (viewport.Contains(left))
            return left;

        if (viewport.Contains(below))
            return below;

        if (viewport.Contains(above))
            return above;

        float x = Math.Clamp(right.X, viewport.Left, viewport.Right - tooltipWidth);
        float y = Math.Clamp(right.Y, viewport.Top, viewport.Bottom - tooltipHeight);

        return new Rectangle(x, y, tooltipWidth, tooltipHeight);
    }

    public override bool IsAnchorValid(bool tooltipHovered)
    {
        return true;
    }
}
