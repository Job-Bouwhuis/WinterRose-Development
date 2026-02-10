using Raylib_cs;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

internal class StaticPositionAnchor : TooltipAnchor
{
    private Vector2 position;
    private readonly Vector2 size;
    private bool hasMouseEnteredTooltip;

    public StaticPositionAnchor(Vector2 position, Vector2 size)
    {
        this.position = position;
        this.size = size;
    }

    public override Rectangle GetAnchorBounds()
    {
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }

    public override bool IsAnchorValid(bool tooltipHovered)
    {
        if(!hasMouseEnteredTooltip)
        {
            if(tooltipHovered)
                hasMouseEnteredTooltip = true;
            return true;
        }
        return base.IsAnchorValid(tooltipHovered);
    }
}