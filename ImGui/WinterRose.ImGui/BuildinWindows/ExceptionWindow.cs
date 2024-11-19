using WinterRose.ImGuiApps;

namespace WinterRose.ImGuiApps;

public class ExceptionWindow : ImGuiWindow
{
    private Exception exception;

    public ExceptionWindow(Exception exception, string title)
    {
        this.exception = exception;
        Title = title;
    }

    public override void OnApplicationClose(ApplicationCloseEventArgs e) { }

    public override void Render()
    {
        gui.SetWindowFontScale(1.4f);
        gui.TextColored(new Color(255, 200, 255), "Exception:");
        gui.SetWindowFontScale(1f);

        {
            if (exception is AggregateException aggregateException)
            {
                gui.Separator();
                gui.SetWindowFontScale(1.1f);
                gui.TextColored(new Color(255, 200, 255), "Aggregate Exception:");
                gui.SetWindowFontScale(1f);
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    gui.Text($"Type: " + innerException.GetType().Name);
                }
                gui.Separator();
            }
            else
                gui.Text($"Type: " + exception.GetType().Name);
        }
        gui.Text("");
        gui.Separator();
        gui.TextWrapped(exception.Message);
        gui.Separator();
        gui.Text("");

        {
            if (exception is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    gui.Text("Inner Exception:");
                    gui.TextWrapped(innerException.Message);
                    gui.Separator();
                }
            }
            else if (exception.InnerException != null)
            {
                gui.Text("Inner Exception:");
                gui.TextWrapped(exception.InnerException.Message);
                gui.Separator();
            }
        }
    }
}