using Raylib_cs;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors
{
    public abstract class TooltipAnchor
    {
        public abstract Rectangle GetAnchorBounds();
        public abstract bool IsAnchorValid();
    }
}