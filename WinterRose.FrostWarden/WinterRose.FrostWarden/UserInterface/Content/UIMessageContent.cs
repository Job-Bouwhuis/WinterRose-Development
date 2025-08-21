using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace WinterRose.ForgeWarden.UserInterface.Content;
public enum ToastMessageFontPreset
{
    Title,
    Message,
    Subtitle,
    Subtext
}

public class UIMessageContent : UIContent
{
    public RichText Text { get; private set; }
    public ToastMessageFontPreset Preset { get; private set; }

    // Base guideline sizes for each preset (can adjust as needed)
    private static readonly Dictionary<ToastMessageFontPreset, int> PresetBaseSizes = new()
    {
        { ToastMessageFontPreset.Title, 12 },
        { ToastMessageFontPreset.Message, 9 },
        { ToastMessageFontPreset.Subtitle, 7 },
        { ToastMessageFontPreset.Subtext, 5 }
    };

    public UIMessageContent(RichText message, ToastMessageFontPreset preset = ToastMessageFontPreset.Message)
    {
        Text = message;
        Preset = preset;
    }

    public UIMessageContent(string text, ToastMessageFontPreset preset = ToastMessageFontPreset.Message)
        : this(RichText.Parse(text), preset) { }

    protected internal override void Setup()
    {
        if (owner is null)
            return;

        Rectangle contentArea = owner.ContentArea;

        // Start with guideline size
        int guideline = PresetBaseSizes[Preset];
        Text.FontSize = guideline;

        // Measure the text bounds at guideline size
        Rectangle textSize = Text.CalculateBounds(contentArea.Width);

        // Compute scaling factor
        float widthScale = contentArea.Width / textSize.Width;
        float heightScale = contentArea.Height / textSize.Height;
        float scale = Math.Min(widthScale, heightScale);

        // Apply scaling, but clamp to a reasonable range
        Text.FontSize = (int)Math.Clamp(guideline * scale, guideline * 0.5f, guideline * 2f);
    }


    public override Vector2 GetSize(Rectangle availableArea) => Text.CalculateBounds(availableArea.Width).Size;

    protected internal override float GetHeight(float width) => Text.CalculateBounds(width).Height;

    protected internal override void Draw(Rectangle bounds)
    {
        RichTextRenderer.DrawRichText(Text, bounds.Position, bounds.Width, Style.White, Input);
    }
}


