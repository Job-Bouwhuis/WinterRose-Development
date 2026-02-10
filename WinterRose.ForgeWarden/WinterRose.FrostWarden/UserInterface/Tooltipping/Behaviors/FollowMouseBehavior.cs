using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors
{
    public sealed class FollowMouseBehavior : TooltipBehavior
    {
        public override bool AllowsInteraction => false;

        public override void Update(Tooltip tooltip) { }
    }

}