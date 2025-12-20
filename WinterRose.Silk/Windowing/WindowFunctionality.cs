using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.Recordium;
using WinterRose.SilkEngine.Rendering;

namespace WinterRose.SilkEngine.Windowing;

public abstract class WindowFunctionality
{
    public EngineWindow Window { get; internal set; }
    public IRenderDevice RenderDevice { get; internal set; }
    public Log log { get; internal set; }

    public virtual void OnLoad() { }
    public virtual void OnRender(double deltaTime) { }
    public virtual void OnUpdate(double deltaTime) { }
    public virtual void OnResize(int width, int height) { }
    public virtual void OnClose() { }
}

