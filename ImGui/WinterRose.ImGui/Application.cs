global using gui = ImGuiNET.ImGui;
using WinterRose.ImGuiUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics;
using System.Diagnostics.CodeAnalysis;

namespace WinterRose.ImGuiApps;

/// <summary>
/// A base class for creating an application with ImGui.
/// </summary>
public class Application : Overlay
{
    /// <summary>
    /// The current application instance.
    /// </summary>
    public static Application Current { get; private set; }

    private List<ImGuiWindow> GuiWindows { get; } = [];
    private List<ImGuiContent> GuiContents { get; } = [];

    [Experimental("WRGUI_EXPERIMENTAL")]
    public Vector2 MaxWindowSize { get; set; } = new(1920, 1080);
    [Experimental("WRGUI_EXPERIMENTAL")]
    public Vector2 MinWindowSize { get; set; } = new(1000, 1000);

    /// <summary>
    /// When true, the application will ask the user before closing the application if there are no windows open.
    /// </summary>
    public bool AskBeforeExit { get; set; } = true;

    ImGuiWindow? LastClosedWindow = null;

    private DateTime lastFrame = DateTime.Now;

    bool once = true;

    public Application()
    {
        Current = this;
    }

    protected override Task Initialize()
    {
        var size = Windows.GetScreenSize();
        Size = new(size.x, size.y);

        return base.Initialize();
    }

    public override Task Run()
    {
        if(GuiWindows.Count is 0 && GuiContents.Count is 0)
        {
            AddWindow(new AnonymousWindow("Demo window", self => {
                gui.Text("Hello, World!");
                if (gui.Button("Close"))
                    self.Close();
            }));
        }
        return base.Run();
    }

    /// <summary>
    /// Adds a window to the application.<br></br>
    /// Will not add the window if a window with the same title already exists.
    /// </summary>
    /// <param name="window"></param>
    public void AddWindow(ImGuiWindow window)
    {
        foreach (var wind in GuiWindows)
        {
            if (wind.Title == window.Title)
            {
                gui.SetWindowFocus(wind.Title);
                gui.SetWindowCollapsed(wind.Title, false);
                return;
            }
        }

        GuiWindows.Add(window);
        window.Application = this;
    }

    /// <summary>
    /// Adds the content to the application.
    /// </summary>
    /// <param name="content"></param>
    public void AddContent(ImGuiContent content)
    {
        GuiContents.Add(content);
    }

    /// <summary>
    /// Gets the window with the specified title.
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public ImGuiWindow GetWindow(string title)
    {
        return GuiWindows.FirstOrDefault(x => x.Title == title);
    }

    /// <summary>
    /// Gets the first window of the specified type. If you need all of the same type see <see cref="GetAllWindowsByType{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetWindowByType<T>() where T : ImGuiWindow
    {
        return (T)GuiWindows.FirstOrDefault(x => x is T);
    }
    public T[] GetAllWindowsByType<T>() where T : ImGuiWindow
    {
        return GuiWindows.Where(x => x is T).Select(x => (T)x).ToArray();
    }

    public T GetContent<T>() where T : ImGuiContent
    {
        return (T)GuiContents.FirstOrDefault(x => x is T);
    }
    public ImGuiWindow[] GetAllWindows() => [.. GuiWindows];



    protected override void Render()
    {
        if (DateTime.Now - lastFrame < TimeSpan.FromMilliseconds(16))
            return;

        gui.DockSpaceOverViewport(gui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoResize);
        if (once)
        {
            Style.ApplyDefault();
            once = false;
        }
        for (int i = 0; i < GuiWindows.Count; i++)
        {
            var window = GuiWindows[i];
            window.Application ??= this;

            if (!window.isOpen)
            {
                WindowCloseEventArgs args = new();
                window.OnWindowClose(args);

                if (args.Cancel)
                {
                    window.isOpen = true;
                }
                else
                {
                    GuiWindows.Remove(window);
                    LastClosedWindow = window;
                    continue;
                }
            }

            int stylesSet = window.ColorStyle();
            int styleVarSet = window.VarStyle();

            if (!window.UseIntegratedBeginLogic)
            {
                window.Render();
                continue;
            }

            bool open;

            if (window.AllowUserClosing)
                open = gui.Begin(window.Title, ref window.isOpen, window.Flags);
            else
                open = gui.Begin(window.Title, window.Flags);

            ImGuiCond cond = window.Size.Params switch
            {
                WindowSizeOptionParams.Always => ImGuiCond.Always,
                WindowSizeOptionParams.FirstUseEver => ImGuiCond.FirstUseEver,
                WindowSizeOptionParams.Appearing => ImGuiCond.Appearing,
                _ => ImGuiCond.Appearing
            };

            gui.SetWindowSize(window.Size, cond);

            if (open)
                window.Render();

            window.SecretRender();

            gui.PopStyleColor(stylesSet);
            gui.PopStyleVar(styleVarSet);

            gui.End();
        }

        for (int i = 0; i < GuiContents.Count; i++)
        {
            var content = GuiContents[i];
            content.Application ??= this;

            if (content.IsVisible)
                content.Render();
        }

        if (GuiWindows.Count == 0)
        {
            if (!AskBeforeExit)
            {
                Close();
                return;
            }

            Windows.DialogResult result = Windows.MessageBox("Are you sure you want to exit?", "Attention", Windows.MessageBoxButtons.YesNo, Windows.MessageBoxIcon.Question);
            if (result == Windows.DialogResult.Yes)
                Close();
            else
            {
                GuiWindows.Add(LastClosedWindow!);
                LastClosedWindow.isOpen = true;
            }
        }
        else if (LastClosedWindow is not null)
        {
            LastClosedWindow.Application = null;
            LastClosedWindow = null;
        }
    }


    /// <summary>
    /// Closes the specified windows. If no windows are left, the application will close.
    /// </summary>
    /// <param name="windowTitles"></param>
    public void CloseWindows(params string[] windowTitles)
    {
        foreach (var title in windowTitles)
        {
            var window = GuiWindows.FirstOrDefault(x => x.Title == title);
            if (window is not null)
                window.isOpen = false;
        }

        if (GuiWindows.Count == 0)
            AskBeforeExit = false;
    }

    /// <summary>
    /// Closes all windows except the ones specified. If no windows are left, the application will close.
    /// </summary>
    /// <param name="windowTitles"></param>
    public void CloseAllWindowsBut(params string[] windowTitles)
    {
        foreach (var window in GuiWindows)
        {
            if (!windowTitles.Contains(window.Title))
                window.isOpen = false;
        }

        if (GuiWindows.Count == 0)
            AskBeforeExit = false;
    }

    /// <summary>
    /// Closes all windows and exits the application.
    /// </summary>
    public void CloseAllWindows()
    {
        AskBeforeExit = false;
        foreach (var window in GuiWindows)
            window.isOpen = false;
    }

    /// <summary>
    /// Closes the specified windows. If no windows are left, the application will close.
    /// </summary>
    /// <param name="windows"></param>
    public void CloseWindows(params ImGuiWindow[] windows)
    {
        foreach (var window in windows)
            window.isOpen = false;

        if (GuiWindows.Count == 0)
            AskBeforeExit = false;
    }

    /// <summary>
    /// Closes all windows except the ones specified. If no windows are left, the application will close.
    /// </summary>
    /// <param name="windows"></param>
    public void CloseAllWindowsBut(params ImGuiWindow[] windows)
    {
        foreach (var window in GuiWindows)
        {
            if (!windows.Contains(window))
                window.isOpen = false;
        }

        if (GuiWindows.Count == 0)
            AskBeforeExit = false;
    }

    /// <summary>
    /// Closes all windows and exits the application.
    /// </summary>
    /// <param name="windows"></param>
    public void CloseAllWindows(params ImGuiWindow[] windows)
    {
        AskBeforeExit = false;
        foreach (var window in windows)
            window.isOpen = false;
    }

    /// <summary>
    /// Closes all windows and the application if no windows are left, and or no window request cancelation.
    /// </summary>
    public override void Close()
    {
        ApplicationCloseEventArgs closeArgs = new();
        foreach (var window in GuiWindows)
            window.OnApplicationClose(closeArgs);

        if (closeArgs.Cancel)
            return;

        GuiWindows.Foreach(x => x.Close());
        GifRenderer.Dispose();
        base.Close();
    }

    internal static void AddAnonymousWindow(string title, Action content)
    {
        throw new NotImplementedException();
    }
}
