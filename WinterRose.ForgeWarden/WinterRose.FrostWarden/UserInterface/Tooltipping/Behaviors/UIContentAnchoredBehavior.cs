using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors;

public class UIContentAnchoredBehavior : TooltipBehavior
{
    public float CloseGrace = 0.18f;

    public override bool AllowsInteraction => true; // tooltip should capture input while open

    public override void Update(Tooltip tooltip)
    {
        if (tooltip == null || tooltip.Anchor is not UIContentAnchor contentAnchor)
            return;

        var content = contentAnchor.Content;
        if (content == null)
            return;

        bool anchorHovered = content.IsContentHovered(content.LastRenderBounds, false); // includes hover extenders
        bool tooltipHovered = tooltip.IsHovered();

        // union of hover states: if either is hovered we treat the anchor as hovered
        if (anchorHovered || tooltipHovered)
            Tooltips.RegisterHoverExtender(content, tooltip);
        else
        {
            tooltip.OpenRequestTimer = 0f;
            tooltip.CloseGraceTimer += Time.deltaTime;

            Tooltips.UnregisterHoverExtender(content, tooltip);
            if (tooltip.CloseGraceTimer >= CloseGrace)
            {
                Tooltips.Close(tooltip, TooltipCloseReason.TargetHoverLost);
            }
        }
    }
}
