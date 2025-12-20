namespace WinterRose.SilkEngine;

using ChatThroughWinterRoseBot;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Diagnostics;
using WinterRose.ForgeThread;
using WinterRose.SilkEngine.Windowing;

public class ForgeWarden
{
    private readonly List<EngineWindow> windows = new();
    private readonly ThreadLoom windowTP = new();

    public void Run()
    {
        // Run each window in its own thread
        foreach (var window in windows)
        {
            windowTP.RegisterWorkerThread(window.window.Title, isBackground: false);
            windowTP.InvokeOn(window.window.Title, window.RunLoop);
        }

        while (true)
        {
            bool allClosed = true;
            foreach (var window in windows)
            {
                if (!window.WindowClosed)
                {
                    allClosed = false;
                    break;
                }
                else
                    windowTP.UnregisterWorkerThread(window.window.Title);
            }
            if (allClosed)
            {
                break;
            }
            Thread.Sleep(100);
        }
    }

    public EngineWindow AddWindow(EngineWindow window)
    {
        windows.Add(window);
        return window;
    }
}



