using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors;

public class UIContentAnchoredBehavior : TooltipBehavior
{
    public float CloseGrace = 0.18f;

    public override bool AllowsInteraction => true;

    public override void Update()
    {
        if (Tooltip == null || Tooltip.Anchor is not UIContentAnchor contentAnchor)
            return;

        var content = contentAnchor.Content;
        if (content == null)
            return;

        bool anchorHovered = content.IsContentHovered(content.LastRenderBounds, false);
        bool tooltipHovered = Tooltip.IsHovered();

        if (anchorHovered || tooltipHovered)
            Tooltips.RegisterHoverExtender(content, Tooltip);
        else
        {
            Tooltip.OpenRequestTimer = 0f;
            Tooltip.CloseGraceTimer += Time.deltaTime;

            Tooltips.UnregisterHoverExtender(content, Tooltip);
            if (Tooltip.CloseGraceTimer >= CloseGrace)
            {
                Tooltips.Close(Tooltip, TooltipCloseReason.TargetHoverLost);
            }
        }
    }
}
