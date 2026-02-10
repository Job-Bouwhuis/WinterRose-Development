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

    public bool AutoScaleText { get; set; } = true;

    /// <summary>
    /// Sometimes, the text content may be generated dynamically or require special handling. 
    /// This delegate allows you to provide a function that returns the current RichText to be displayed. 
    /// If set, this function will be called during drawing and override the Text property,
    /// allowing for dynamic text content that can change each frame or based on specific 
    /// conditions without needing to manually update the Text property.
    /// </summary>
    public Func<RichText>? TextProvider { get; set; }

    // Base guideline sizes for each preset (can adjust as needed)
    private static readonly Dictionary<UIFontSizePreset, int> PresetBaseSizes = new()
    {
        { UIFontSizePreset.Title, 26 },
        { UIFontSizePreset.Subtitle, 20 },
        { UIFontSizePreset.Text, 16 },
        { UIFontSizePreset.Subtext, 10 }
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

        int resolvedSize = UITextScalar.ResolveFontSize(
            Text,
            guideline,
            availableArea,
            AutoScaleText
        );

        Rectangle size = UITextScalar.Measure(
            Text,
            resolvedSize,
            availableArea.Width
        );

        return new Vector2(
            Math.Min(size.Width, availableArea.Width),
            Math.Min(size.Height, availableArea.Height)
        );
    }


    protected internal override float GetHeight(float width)
    {
        int guideline = PresetBaseSizes[Preset];

        int resolvedSize = UITextScalar.ResolveFontSize(
            Text,
            guideline,
            new Rectangle(0, 0, width, float.MaxValue),
            AutoScaleText
        );

        return UITextScalar.Measure(Text, resolvedSize, width).Height;
    }


    protected override void Draw(Rectangle bounds)
    {
        if(TextProvider != null)
            Text = TextProvider();

        int guideline = PresetBaseSizes[Preset];

        Text.FontSize = UITextScalar.ResolveFontSize(
            Text,
            guideline,
            bounds,
            AutoScaleText
        );

        RichTextRenderer.DrawRichText(
            Text,
            bounds.Position,
            bounds.Width,
            Style.StyleBase.White,
            Input
        );
    }

}


