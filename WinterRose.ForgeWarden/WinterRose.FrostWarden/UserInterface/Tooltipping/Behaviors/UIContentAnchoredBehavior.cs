using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors;

public class UIContentAnchoredBehavior : TooltipBehavior
{
    public override TooltipMode Mode => TooltipMode.FollowMouse; // informational only

    public float OpenDelay = 0.12f;
    public float CloseGrace = 0.08f;

    public override bool AllowsInteraction => true; // tooltip should capture input while open

    public override void Update(Tooltip tooltip)
    {
        if (tooltip == null || tooltip.Anchor is not UIContentAnchor contentAnchor)
            return;

        var content = contentAnchor.Content;
        if (content == null)
            return;

        bool anchorHovered = content.IsHovered; // includes hover extenders
        bool tooltipHovered = tooltip.IsHovered();

        // union of hover states: if either is hovered we treat the anchor as hovered
        if (anchorHovered || tooltipHovered)
        {
            Tooltips.RegisterHoverExtender(content, tooltip);

            // reset close timers; progress open timer
            tooltip.CloseTimer = 0f;
            // if your tooltip has a separate closeGraceTimer/openRequestTimer use them:
            tooltip.CloseGraceTimer = 0f;
            tooltip.OpenRequestTimer += Time.deltaTime;

            if (tooltip.OpenRequestTimer >= OpenDelay)
            {
                if (!tooltip.IsOpen)
                    Tooltips.Show(tooltip);
            }
        }
        else
        {
            // no hover anywhere: drop the extender and start grace/close logic
            Tooltips.UnregisterHoverExtender(content, tooltip);

            tooltip.OpenRequestTimer = 0f;
            tooltip.CloseGraceTimer += Time.deltaTime;

            if (tooltip.CloseGraceTimer >= CloseGrace)
            {
                Tooltips.Close(tooltip, TooltipCloseReason.TargetHoverLost);
            }
        }
    }
}
