using Raylib_cs;
using WinterRose.ForgeWarden.Utility;

namespace WinterRose.ForgeWarden.UserInterface.Content;

public class HTMLContent : UIContent
{
    private UIProgress spinner;
    private bool loaded;
    private readonly string html;

    public HTMLContent(string html)
    {
        this.html = html;
        spinner = new UIProgress(-1, infiniteSpinText: "Processing HTML...")
        {
            allowPauseAutoDismissTimer = false
        };
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
                for (var index = contentes.Count - 1; index >= 0; index--)
                {
                    var content = contentes[index];
                    Owner.AddContent(this, content, myContentIndex);
                }

                if(contentes.Count == 0)
                {
                     Owner.AddContent(this, new UIText("\\c[red]Failed to load HTML content."), myContentIndex);
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
        return 0f;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        if (!loaded) return spinner.GetSize(availableArea);
        return new Vector2(availableArea.Width, 0);
    }

    protected override void Update()
    {
        if (!loaded)
            spinner._Update();
    }

    protected override void Draw(Rectangle bounds)
    {
        if (!loaded)
        {
            spinner.ForceDraw(bounds);
            return;
        }
    }
}