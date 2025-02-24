namespace WinterRose.SilkEngine;

using System;
using ChatThroughWinterRoseBot;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public abstract class Application
{
    private IWindow window;
    protected GL gl; // OpenGL context

    public void Run()
    {
        var windowOptions = Silk.NET.Windowing.WindowOptions.Default;
        windowOptions.Size = new Vector2D<int>(800, 600); // Default window size
        windowOptions.Title = "2D Game Engine";

        window = Window.Create(windowOptions);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Update += OnUpdate;
        window.Closing += OnClose;

        window.Run();
    }

    protected virtual void OnLoad()
    {
        gl = window.CreateOpenGL();
    }

    protected virtual void OnRender(double deltaTime)
    { 
        gl.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));
    }

    protected virtual void OnUpdate(double deltaTime)
    {
    }

    protected virtual void OnClose()
    {
        Console.WriteLine("Application is closing.");
    }
}


