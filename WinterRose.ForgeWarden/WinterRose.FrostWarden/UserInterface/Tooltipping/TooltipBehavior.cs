using WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping
{
    public abstract class TooltipBehavior
    {
        public TooltipMode Mode => GetType().Name switch
        {
            nameof(FollowMouseBehavior) => TooltipMode.FollowMouse,
            _ => TooltipMode.Static
        };

        /// <summary>
        /// Whether this tooltip should accept interaction (mouse clicks / keyboard) when open.
        /// </summary>
        public virtual bool AllowsInteraction => false;

        /// <summary>
        /// Whether the tooltip should steal keyboard focus when opened (interactive tooltips typically do).
        /// </summary>
        public virtual bool StealsFocus => false;

        public Tooltip Tooltip { get; internal set; }

        /// <summary>
        /// Called to decide if the tooltip should open. Return false to veto opening.
        /// </summary>
        public virtual bool ShouldOpen(Tooltip tooltip) => true;

        /// <summary>
        /// Called to decide if the tooltip should close for a given reason. Return false to block closing.
        /// </summary>
        public virtual bool ShouldClose(Tooltip tooltip, TooltipCloseReason reason) => true;

        /// <summary>
        /// Per-frame update hook for behavior specific logic.
        /// </summary>
        public virtual void Update(Tooltip tooltip) { }
    }
}