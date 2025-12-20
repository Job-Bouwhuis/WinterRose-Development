using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.Recordium;
using WinterRose.SilkEngine.Rendering;

namespace WinterRose.SilkEngine.Windowing;

public class EngineWindow
{
    private static readonly object GlfwLock = new();
    private Log log;
    public IWindow window { get; }
    private WindowFunctionality logic;

    public GL gl { get; private set; }
    public IRenderDevice RenderDevice { get; private set; }
    public bool WindowClosed { get; private set; }

    public EngineWindow(string title, int width, int height, WindowFunctionality logic)
    {
        var options = Silk.NET.Windowing.WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        options.VSync = true;

        log = new Log("EngineWindow:" + title);
        logic.log = log;

        lock (GlfwLock)
        {
            window = Window.Create(options);
        }
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Update += OnUpdate;
        window.Closing += OnClose;
        this.logic = logic;
    }

    private void OnClose()
    {
        WindowClosed = true;
        logic.OnClose();
    }

    private void OnLoad()
    {
        gl = window.CreateOpenGL();
        RenderDevice = new OpenGLRenderDevice(gl);

        logic.Window = this;
        logic.RenderDevice = RenderDevice;

        // Set viewport
        var size = window.Size;
        RenderDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);

        window.FramebufferResize += framebufferSize =>
        {
            RenderDevice.SetViewport(0, 0, (uint)framebufferSize.X, (uint)framebufferSize.Y);
        };
        logic.OnLoad();
    }

    private void OnRender(double deltaTime) => logic.OnRender(deltaTime);
    private void OnUpdate(double deltaTime) => logic.OnUpdate(deltaTime);

    public void RunLoop()
    {
        //try
        //{
            window.Run();

        //}
        //catch (Exception ex)
        //{
        //    log.Critical(ex);
        //}
    }

}
