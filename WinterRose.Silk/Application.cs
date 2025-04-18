namespace WinterRose.SilkEngine;

using System;
using ChatThroughWinterRoseBot;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public abstract class Application
{
    private IWindow window;
    protected GL gl; // OpenGL context
    protected IInputContext input;
    private Painter painter;

    public void Run()
    {
        var windowOptions = Silk.NET.Windowing.WindowOptions.Default;
        windowOptions.Size = new Vector2D<int>(800, 600); // Default window size
        windowOptions.Title = "2D Game Engine";
        windowOptions.VSync = true;
        window = Window.Create(windowOptions);
        
        window.Load += Init;
        window.Render += Render;
        window.Update += Update;
        window.Closing += OnClose;

        window.Initialize();
        
        painter = new Painter(gl);
        input = window.CreateInput();

        Input.Initialize(input);

        window.Run();
    }

    protected virtual void Init()
    {
        gl = window.CreateOpenGL();
        Initialize();
    }

    protected abstract void Initialize();

    private void Render(double deltaTime)
    {
        gl.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));
        Render(painter);
    }

    protected abstract void Render(Painter painter);

    private void Update(double deltaTime)
    {
        Input.Update();
        Time.sinceLastFrame = (float)deltaTime;
        Update();
    }

    protected abstract void Update();

    protected virtual void OnClose()
    {
        Console.WriteLine("Application is closing.");
    }
}


