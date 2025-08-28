using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace WinterRose.ForgeWarden.UserInterface;
public enum UIFontSizePreset
{
    Title,
    Message,
    Subtitle,
    Subtext
}

public class UITextContent : UIContent
{
    public RichText Text { get; private set; }
    public UIFontSizePreset Preset { get; private set; }

    // Base guideline sizes for each preset (can adjust as needed)
    private static readonly Dictionary<UIFontSizePreset, int> PresetBaseSizes = new()
    {
        { UIFontSizePreset.Title, 24 },
        { UIFontSizePreset.Subtitle, 18 },
        { UIFontSizePreset.Message, 14 },
        { UIFontSizePreset.Subtext, 7 }
    };

    public UITextContent(RichText message, UIFontSizePreset preset = UIFontSizePreset.Message)
    {
        Text = message;
        Preset = preset;
    }

    public UITextContent(string text, UIFontSizePreset preset = UIFontSizePreset.Message)
        : this(RichText.Parse(text), preset) { }


    public override Vector2 GetSize(Rectangle availableArea) => Text.CalculateBounds(availableArea.Width).Size;

    protected internal override float GetHeight(float width) => Text.CalculateBounds(width).Height;

    protected override void Draw(Rectangle bounds)
    {
        // Start with guideline size
        int guideline = PresetBaseSizes[Preset];
        Text.FontSize = guideline;

        // Measure the text bounds at guideline size
        Rectangle textSize = Text.CalculateBounds(bounds.Width);

        // Compute scaling factor
        float widthScale = bounds.Width / textSize.Width;
        float heightScale = bounds.Height / textSize.Height;
        float scale = Math.Min(widthScale, heightScale);

        // Apply scaling, but clamp to a reasonable range
        Text.FontSize = (int)Math.Clamp(guideline * scale, guideline * 0.5f, guideline * 2f);

        RichTextRenderer.DrawRichText(Text, bounds.Position, bounds.Width, Style.White, Input);
    }
}


