namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors
{
    public sealed class StaticTooltipBehavior : TooltipBehavior
    {
        public override TooltipMode Mode => TooltipMode.Static;

        /// <summary>
        /// Number of pixels to expand bounds for the purpose of detecting "left the tooltip".
        /// </summary>
        public float CloseMargin = 10f;

        /// <summary>
        /// Delay (seconds) after leaving expanded bounds before the tooltip actually closes.
        /// </summary>
        public float CloseDelay = 0.12f;

        public override bool AllowsInteraction => true;
        public override bool StealsFocus => true;


        public override void Update(Tooltip tooltip)
        {
            tooltip.ComputeExpandedCloseBounds();
            var mouse = tooltip.Input.MousePosition;

            if (!tooltip.IsPointInside(tooltip.ExpandedCloseBounds, mouse))
            {
                tooltip.CloseTimer += Time.deltaTime;

                if (tooltip.CloseTimer >= CloseDelay)
                    Tooltips.Close(tooltip, TooltipCloseReason.MouseLeftBounds);
            }
            else
            {
                tooltip.CloseTimer = 0f;
            }
        }
    }
}