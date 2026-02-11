using WinterRose.EventBusses;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace WinterRose.ForgeWarden.UserInterface;

public interface IUIContainer
{
    IUIContainer Owner { get; }
    IUIContainer Root 
    { 
        get
        {
            IUIContainer? c = Owner;
            while (c != null)
                c = c.Owner;

            if (c != null)
                return c;

            return this;
        } 
    }
    InputContext Input { get; }
    bool IsVisible { get; }
    bool IsClosing { get; }
    bool IsBeingDragged { get; }
    bool PauseDragMovement { get; }
    Rectangle CurrentPosition { get; }
    float Height { get; }
    ContentStyle Style { get; }

    IReadOnlyList<UIContent> Contents { get; }

    /// <summary>
    /// Adds content to the container.
    /// </summary>
    IUIContainer AddContent(UIContent content);

    /// <summary>
    /// Inserts content at a specific index in the container.
    /// </summary>
    IUIContainer AddContent(UIContent content, int index);

    virtual IUIContainer AddButton(RichText text, VoidInvocation<IUIContainer, UIButton>? onClick = null)
    {
        AddContent(new UIButton(text, (c, b) => onClick?.Invoke(this, b)));
        return this;
    }

    virtual IUIContainer AddButton(string text, VoidInvocation<IUIContainer, UIButton>? onClick)
        => AddButton(RichText.Parse(text, Color.White), onClick);

    virtual IUIContainer AddProgressBar(float initialProgress, Func<float, float>? ProgressProvider = null, string infiniteSpinText = "Working...")
    {
        return AddContent(new UIProgress(initialProgress, ProgressProvider, infiniteSpinText));
    }

    virtual IUIContainer AddSprite(Sprite sprite)
        => AddContent(new UISprite(sprite));

    virtual IUIContainer AddTitle(string text, UIFontSizePreset preset = UIFontSizePreset.Title)
        => AddText(RichText.Parse(text, Color.White), preset);

    virtual IUIContainer AddTitle(RichText text, UIFontSizePreset preset = UIFontSizePreset.Title)
        => AddContent(new UIText(text, preset));

    virtual IUIContainer AddText(RichText text, UIFontSizePreset preset = UIFontSizePreset.Text)
        => AddContent(new UIText(text, preset));

    virtual IUIContainer AddText(string text, UIFontSizePreset preset = UIFontSizePreset.Text) =>
        AddText(RichText.Parse(text, Color.White), preset);

    IUIContainer AddContent(UIContent reference, UIContent contentToAdd);
    IUIContainer AddContent(UIContent reference, UIContent contentToAdd, int index);

    /// <summary>
    /// Removes the specified content element from the container.
    /// </summary>
    void RemoveContent(UIContent element);

    /// <summary>
    /// Adds a list of content elements to the container.
    /// </summary>
    void AddAll(List<UIContent> contents);

    void AddAll(UIContent reference, List<UIContent> contents);

    /// <summary>
    /// Returns the index of a specific content element.
    /// </summary>
    int GetContentIndex(UIContent content);
    void Close();
}
