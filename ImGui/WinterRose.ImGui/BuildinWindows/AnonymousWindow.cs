namespace WinterRose.ImGuiApps;

internal class AnonymousWindow : ImGuiWindow
{
    Action<AnonymousWindow> content;

    public AnonymousWindow(string title, Action<AnonymousWindow> content) : base(title)
    {
        this.content = content;
    }

    public override void Render()
    {
        content(this);
    }
}
