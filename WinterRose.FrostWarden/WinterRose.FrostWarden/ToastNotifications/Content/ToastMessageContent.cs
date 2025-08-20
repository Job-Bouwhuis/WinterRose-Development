using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.ToastNotifications;
public enum ToastMessageFontPreset
{
    Title,
    Message,
    Subtitle,
    Subtext
}

public class ToastMessageContent : ToastContent
{


    public RichText Message { get; private set; }
    public ToastMessageFontPreset Preset { get; private set; }

    // Base guideline sizes for each preset (can adjust as needed)
    private static readonly Dictionary<ToastMessageFontPreset, int> PresetBaseSizes = new()
    {
        { ToastMessageFontPreset.Title, 12 },
        { ToastMessageFontPreset.Message, 9 },
        { ToastMessageFontPreset.Subtitle, 7 },
        { ToastMessageFontPreset.Subtext, 5 }
    };

    public ToastMessageContent(string text, ToastMessageFontPreset preset = ToastMessageFontPreset.Message)
        : this(RichText.Parse(text), preset) { }

    public ToastMessageContent(RichText message, ToastMessageFontPreset preset = ToastMessageFontPreset.Message)
    {
        Message = message;
        Preset = preset;

        // Scale the font size based on toast width, using the preset as guideline
        int guideline = PresetBaseSizes[preset];
        float messageScale = Toasts.TOAST_WIDTH * 0.06f; // default scaling
        Message.FontSize = (int)Math.Clamp(messageScale, guideline, guideline * 2);
    }

    public override float GetHeight(float width)
    {
        return Message.CalculateBounds(width).Height;
    }

    public override void Draw(Rectangle bounds, float contentAlpha)
    {
        RichTextRenderer.DrawRichText(Message, bounds.Position, bounds.Width, new Color(255, 255, 255, contentAlpha));
    }

    public override void OnClick(MouseButton button) { }
    public override void OnHover() { }
}


