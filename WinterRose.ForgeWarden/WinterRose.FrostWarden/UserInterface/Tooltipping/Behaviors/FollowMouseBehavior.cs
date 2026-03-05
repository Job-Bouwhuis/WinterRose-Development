using Raylib_cs;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors
{
    public sealed class FollowMouseBehavior : TooltipBehavior
    {
        public override bool AllowsInteraction => false;
        public FollowMouseBehavior(Vector2 offsetFromMouse)
        {
            OffsetFromMouse = offsetFromMouse;
        }

        public Vector2 OffsetFromMouse { get; set; }

        public Vector2 Size { get; set; }
        public override void Update()
        {
            if(!Tooltip.Behavior.AllowsInteraction)
                Tooltip.Input.Update(); // we need the provider to update for mouse input, so if the input system doesnt do it, we will
            Vector2 mousePos = Tooltip.Input.Provider.MousePosition;
            Vector2 targetPos = mousePos + OffsetFromMouse;
            
            Tooltip.TargetPosition = targetPos;
        }
    }

}