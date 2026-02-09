using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping
{
    public sealed class Tooltip : UIContainer
    {
        private const int DEFAULT_ANIMATION_DURATION_MS = 180;

        public override InputContext Input { get; }

        public TooltipBehavior Behavior { get; internal set; }
        public TooltipAnchor Anchor { get; internal set; }

        public TooltipSizeConstraints SizeConstraints { get; } = new TooltipSizeConstraints();

        public bool IsOpen { get; internal set; }

        public bool LockOpen { get; set; }

        public Func<Tooltip, bool>? OpenOverride;
        public Func<Tooltip, TooltipCloseReason, bool>? CloseOverride;

        public Rectangle ExpandedCloseBounds { get; set; }
        public float CloseTimer { get; set; }

        public bool WasPreviouslyHoveringAnchor { get; set; } = false;
        public float OpenRequestTimer { get; set; } = 0f;
        public float CloseGraceTimer { get; set; } = 0f;

        public Tooltip(TooltipBehavior behavior, TooltipAnchor anchor)
        {
            // Create a proper InputContext with a RaylibInputProvider so we can read mouse position.
            // The priority is set to 0 (will be overridden by TooltipManager when registering).
            Input = new InputContext(new RaylibInputProvider(), 0, false);

            Behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
            Anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));

            // sensible defaults
            Style = new ContentStyle(new StyleBase());
            Style.AutoScale = true;
            Style.AllowUserResizing = false;
            Style.PauseAutoDismissTimer = true;

            TargetSize = new Vector2(SizeConstraints.MinSize.X, SizeConstraints.MinSize.Y);
            AnimationElapsed = 1f;   // already at final state
        }

        protected override void Update()
        {
            // run UIContainer core logic
            base.Update();

            // Update input only when interactive (so follow mouse tooltips remain passive)
            if (Behavior.AllowsInteraction)
                Input.Update();

            // behavior-specific update
            Behavior.Update(this);

            // lifecycle handling
            HandleLifecycle();
        }

        private void HandleLifecycle()
        {
            if (LockOpen)
                return;

            Behavior.Update(this);
        }


        public bool IsPointInside(Rectangle r, Vector2 p)
        {
            return p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height;
        }

        internal void ComputeExpandedCloseBounds()
        {
            var baseBounds = LastBorderBounds;
            float margin = 0f;
            if (Behavior is StaticTooltipBehavior sb)
                margin = sb.CloseMargin;

            ExpandedCloseBounds = new Rectangle(
                baseBounds.X - margin,
                baseBounds.Y - margin,
                baseBounds.Width + margin * 2f,
                baseBounds.Height + margin * 2f
            );
        }

        public void RequestReposition()
        {
            Tooltips.RequestReposition(this);
        }

        public override bool IsHovered()
        {
            if (Behavior.AllowsInteraction)
                return base.IsHovered();

            return false;
        }

        /// <summary>
        /// Called to close the tooltip explicitly by consumers.
        /// </summary>
        public void CloseTooltip()
        {
            IsOpen = false;
            Tooltips.Close(this, TooltipCloseReason.Explicit);
        }
    }
}