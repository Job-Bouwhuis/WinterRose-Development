using Raylib_cs;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors
{
    public abstract class TooltipAnchor
    {
        public Tooltip Tooltip { get; internal set; }

        public abstract Rectangle GetAnchorBounds();
        public virtual bool IsAnchorValid(bool tooltipHovered) => tooltipHovered;
    }
}