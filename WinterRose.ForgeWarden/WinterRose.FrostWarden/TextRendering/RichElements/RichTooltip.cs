namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;

/// <summary>
/// Renders a tooltip region element that marks interactive areas.
/// Note: Full tooltip integration requires custom UIContent wrapper.
/// For now, renders text with visual indication of interactivity.
/// </summary>
public class RichTooltip : RichElement
{
    public string Text { get; set; }
    public Color Color { get; set; }
    public string TooltipContent { get; set; }

    Tooltip tooltip;

    public RichTooltip(string text, Color color, string tooltipContent)
    {
        Text = text;
        Color = color;
        TooltipContent = tooltipContent;
    }

    public override string ToString() => $"\\[tooltip {Text}|{TooltipContent}]";

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        float x = position.X;
        float y = position.Y;

        Color tinted = new Color(
            (byte)(Color.R * context.OverallTint.R / 255),
            (byte)(Color.G * context.OverallTint.G / 255),
            (byte)(Color.B * context.OverallTint.B / 255),
            (byte)(Color.A * context.OverallTint.A / 255)
        );

        var size = RichTextRenderer.MeasureTextExCached(context.RichText.Font, Text, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);

        // Draw the text with a subtle underline to indicate it's interactive
        Raylib.DrawTextEx(context.RichText.Font, Text, new Vector2(x, y), context.RichText.FontSize, context.RichText.Spacing, tinted);
        Raylib.DrawLineEx(new Vector2(x, y + size.Y + 1), new Vector2(x + size.X, y + size.Y + 1), 1, new Color(tinted.R, tinted.G, tinted.B, (byte)(tinted.A * 0.5f)));

        // Create hitbox for tooltip interaction tracking
        var tooltipBounds = new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y);
        ForgeWardenEngine.Current.AddDebugDraw(() =>
        {
            ray.DrawRectangleLinesEx(tooltipBounds, 1, Color.Magenta);
            ray.DrawCircle((int)x, (int)y, 3, Color.Yellow);
            ray.DrawCircle((int)context.Input.MousePosition.X, (int)context.Input.MousePosition.Y, 3, Color.Yellow);
        });

        if (ray.CheckCollisionPointRec(context.Input.MousePosition, tooltipBounds))
        {
            if (tooltip == null)
            {
                var ttsize = RichTextRenderer.MeasureTextExCached(context.RichText.Font, TooltipContent, context.RichText.FontSize, context.RichText.Spacing, context.MeasureCache);
                ttsize += new Vector2(UIConstants.CONTENT_PADDING * 2, UIConstants.CONTENT_PADDING * 2);

                tooltip = Tooltips.MouseFollow(ttsize);
                tooltip.AddText(TooltipContent);

                tooltip.Show();
            }
        }
        else if (tooltip != null)
        {
            tooltip.Close();
            tooltip = null;
        }

        return new RichTextRenderResult
        {
            WidthConsumed = size.X + context.RichText.Spacing,
            HeightConsumed = size.Y
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        return RichTextRenderer.MeasureTextExCached(richText.Font, Text, richText.FontSize, richText.Spacing, measureCache).X;
    }
}



