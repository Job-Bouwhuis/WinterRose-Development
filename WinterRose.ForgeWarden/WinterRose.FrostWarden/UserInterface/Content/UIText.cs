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
    Text,
    Subtitle,
    Subtext
}

public class UIText : UIContent
{
    public RichText Text
    {
        get => text;
        set
        {
            SetText(value);
        }
    }
    public UIFontSizePreset Preset
    {
        get => preset;
        set
        {
            preset = value;
        }
    }

    // Base guideline sizes for each preset (can adjust as needed)
    private static readonly Dictionary<UIFontSizePreset, int> PresetBaseSizes = new()
    {
        { UIFontSizePreset.Title, 24 },
        { UIFontSizePreset.Subtitle, 18 },
        { UIFontSizePreset.Text, 14 },
        { UIFontSizePreset.Subtext, 7 }
    };
    private RichText text;
    private UIFontSizePreset preset;

    public void SetText(RichText text)
    {
        this.text = text;
    }

    public UIText(RichText message, UIFontSizePreset preset = UIFontSizePreset.Text)
    {
        Text = message;
        Preset = preset;
    }

    public UIText(string text, UIFontSizePreset preset = UIFontSizePreset.Text)
        : this(RichText.Parse(text), preset) { }


    public override Vector2 GetSize(Rectangle availableArea)
    {
        int guideline = PresetBaseSizes[Preset];
        Text.FontSize = guideline;
        var size = Text.CalculateBounds(availableArea.Width).Size;
        return new Vector2(Math.Min(size.X, availableArea.Width), Math.Min(size.Y, availableArea.Height));
    }

    protected internal override float GetHeight(float width)
    {
        int guideline = PresetBaseSizes[Preset];
        Text.FontSize = guideline;
        return Text.CalculateBounds(width).Height;
    }

    protected override void Draw(Rectangle bounds)
    {
        // Start with guideline size
        int guideline = PresetBaseSizes[Preset];
        Text.FontSize = guideline;

        // Measure the text bounds at guideline size
        Rectangle textSize = Text.CalculateBounds(bounds.Width);

        // Compute scaling factor
        float scale = 1f; // default: no scaling
        if (textSize.Width > bounds.Width || textSize.Height > bounds.Height)
        {
            float widthScale = bounds.Width / textSize.Width;
            float heightScale = bounds.Height / textSize.Height;
            scale = Math.Min(widthScale, heightScale);
        }


        // Apply scaling, but clamp to a reasonable range
        Text.FontSize = (int)Math.Clamp(guideline * scale, guideline * 0.5f, guideline * 2f);

        RichTextRenderer.DrawRichText(Text, bounds.Position, bounds.Width, Style.White, Input);
    }
}


