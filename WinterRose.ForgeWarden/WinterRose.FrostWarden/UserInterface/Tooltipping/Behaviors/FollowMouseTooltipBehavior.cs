using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors
{
    public sealed class FollowMouseTooltipBehavior : TooltipBehavior
    {
        public override TooltipMode Mode => TooltipMode.FollowMouse;

        public Vector2 MouseOffset = new Vector2(12f, 18f);
        public float OpenDelay = 0.15f;
        public float CloseGrace = 0.08f;

        public override void Update(Tooltip tooltip)
        {
            if (tooltip.Anchor is not UIContainerTooltipAnchor uanch)
                return;

            bool isHoveringTarget = uanch.Target.IsHovered();

            if (isHoveringTarget && !tooltip.WasPreviouslyHoveringAnchor)
            {
                tooltip.OpenRequestTimer = 0f;
                tooltip.WasPreviouslyHoveringAnchor = true;
            }

            if (isHoveringTarget)
            {
                tooltip.OpenRequestTimer += Time.deltaTime;
                tooltip.CloseGraceTimer = 0f;
            }
            else
            {
                tooltip.CloseGraceTimer += Time.deltaTime;

                if (tooltip.CloseGraceTimer >= CloseGrace)
                    Tooltips.Close(tooltip, TooltipCloseReason.TargetHoverLost);

                tooltip.WasPreviouslyHoveringAnchor = false;
            }
        }
    }

}