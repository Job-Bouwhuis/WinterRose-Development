using Raylib_cs;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors
{
    public sealed class UIContainerTooltipAnchor : TooltipAnchor
    {
        public UIContainer Target { get; set; }

        public UIContainerTooltipAnchor(UIContainer target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public override Rectangle GetAnchorBounds()
        {
            return Target.CurrentPosition;
        }

        public override bool IsAnchorValid()
        {
            return Target != null && !Target.IsClosing;
        }
    }
}