using ImGuiNET;
using System;
using System.Numerics;

namespace WinterRose.ImGuiApps;

/// <summary>
/// An ImGui Window to be rendered.
/// </summary>
public abstract class ImGuiWindow
{
    /// <summary>
    /// The title of the window.
    /// </summary>
    public string Title { get; set; } = "New Window";
    /// <summary>
    /// The size of the window.
    /// </summary>
    public WindowSizeOptions Size { get; set; } = new();
    /// <summary>
    /// The flags to use when creating the window. Default is <see cref="ImGuiWindowFlags.AlwaysAutoResize"/>
    /// </summary>
    public ImGuiWindowFlags Flags { get; set; } = ImGuiWindowFlags.AlwaysAutoResize;
    /// <summary>
    /// Whether the window is collapsed or not.
    /// </summary>
    public bool IsCollapsed { get; set; } = false;

    /// <summary>
    /// Function to set the color style used for the window.
    /// <br></br>Should return the number of styles pushed.
    /// </summary>
    public Func<int> ColorStyle { get; set; } = () => { return 0; };

    /// <summary>
    /// Function to set the style variables used for the window.
    /// <br></br>Should return the number of styles pushed.
    /// </summary>
    public Func<int> VarStyle { get; set; } = () => { return 0; };

    /// <summary>
    /// The application this window is attached to.
    /// </summary>
    protected internal Application Application { get; set; }

    /// <summary>
    /// Determines whether to render the window. Different from <c>isOpen</c> in that it only affects rendering, It does not remove the window from the list of windows for the application.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// If false, the window will be closed and removed from the app.<br></br>
    /// Exposed for use in custom window management.
    /// </summary>
    protected internal bool isOpen = true;
    /// <summary>
    /// When true, The window will have an 'X' button to close the window.
    /// </summary>
    public bool AllowUserClosing { get; set; } = true;

    /// <summary>
    /// Whether to let the framework handle the ImGui.Begin() and ImGui.End() calls for this window.
    /// </summary>
    public bool UseIntegratedBeginLogic { get; set; } = true;

    public ImGuiWindow()
    {
    }

    public ImGuiWindow(string title) : this()
    {
        Title = title;
    }

    public ImGuiWindow(string title, WindowSizeOptions size) : this(title)
    {
        Size = size;
    }

    /// <summary>
    /// Invoked when the window is being closed.
    /// </summary>
    public virtual void OnApplicationClose(ApplicationCloseEventArgs e) { }
    public virtual void OnWindowClose(WindowCloseEventArgs e) { }

    /// <summary>
    /// Called each frame to render the window. This method is not called when <see cref="IsVisible"/> is set to false.
    /// </summary>
    public abstract void Render();
    /// <summary>
    /// This render method is called regardless of visibility. Use this for routing to other render methods. You will have to manage visibility yourself.
    /// </summary>
    public virtual void SecretRender() { }

    /// <summary>
    /// Closes the window.
    /// </summary>
    public void Close()
    {
        isOpen = false;
    }

    /// <summary>
    /// Makes the window visible.
    /// </summary>
    public void Show()
    {
        IsVisible = true;
    }

    /// <summary>
    /// Hides the window.
    /// </summary>
    public void Hide()
    {
        IsVisible = false;
    }
}
