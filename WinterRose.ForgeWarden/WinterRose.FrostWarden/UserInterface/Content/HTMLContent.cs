using Raylib_cs;
using WinterRose.ForgeWarden.Utility;

namespace WinterRose.ForgeWarden.UserInterface.Content;

public class HTMLContent : UIContent
{
    private UISprite sprite = null!;
    private UIProgress spinner;
    private bool loaded;
    private readonly string html;

    public HTMLContent(string html)
    {
        this.html = html;
        spinner = new UIProgress(-1, infiniteSpinText: "Processing HTML...");
        _ = LoadAndSwapAsync();
    }

    protected internal override void Setup()
    {
        spinner.Owner = Owner;
        spinner.Setup();
    }

    private async Task LoadAndSwapAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                List<UIContent> contentes = HtmlToUiTranslator.TranslateDocument(html);
                int myContentIndex = Owner.GetContentIndex(this);
                for (var index = contentes.Count - 1; index > 0; index--)
                {
                    var content = contentes[index];
                    Owner.AddContent(this, content, myContentIndex);
                }

                Owner.RemoveContent(this);
            }
            catch
            {
                // swallow - we'll leave spinner gone and show empty spacer
            }
            finally
            {
                loaded = true;
            }
        });
    }

    protected internal override float GetHeight(float width)
    {
        if (!loaded) return spinner.GetHeight(width);
        if (sprite != null) return sprite.GetHeight(width);
        return 0f;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        if (!loaded) return spinner.GetSize(availableArea);
        if (sprite != null) return sprite.GetSize(availableArea);
        return new Vector2(availableArea.Width, 0);
    }

    protected internal override void Update()
    {
        if (!loaded)
            spinner.Update();
    }

    protected override void Draw(Rectangle bounds)
    {
        if (!loaded)
        {
            spinner.ForceDraw(bounds);
            return;
        }
        if (sprite != null)
        {
            sprite.ForceDraw(bounds);
            return;
        }

        // nothing loaded -> optionally draw a subtle placeholder / nothing
    }
}