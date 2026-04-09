using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.EngineLayers.Events;

namespace WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;

public class RenderLayer : EngineLayer
{
    private RenderTexture2D worldTex;
    private RenderTexture2D uiTex;

    private int width;
    private int height;
    private bool initialized;

    public RenderLayer() : base("Render") => Importance = int.MaxValue - 10000;

    public override void OnUpdate()
    {
        Resize();
    }

    public override void OnRender()
    {
        Raylib.BeginTextureMode(worldTex);
        Raylib.ClearBackground(Engine.ClearColor);

        WorldDrawEvent worldEvent = new()
        {
            RenderTexture = worldTex,
            Camera = Camera.main
        };

        LayerStack.Dispatch(ref worldEvent);

        Raylib.EndTextureMode();

        Raylib.BeginTextureMode(uiTex);
        Raylib.ClearBackground(Color.Blank);

        UiDrawEvent uiEvent = new()
        {
            RenderTexture = uiTex
        };

        LayerStack.Dispatch(ref uiEvent);

        Raylib.EndTextureMode();

        FrameCompleteEvent frameEvent = new()
        {
            WorldTexture = worldTex,
            UiTexture = uiTex
        };

        LayerStack.Dispatch(ref frameEvent);
    }

    private void Resize()
    {
        int newWidth = Engine.Window.Width;
        int newHeight = Engine.Window.Height;

        if (initialized && newWidth == width && newHeight == height)
            return;

        if (initialized)
        {
            Raylib.UnloadRenderTexture(worldTex);
            Raylib.UnloadRenderTexture(uiTex);
        }

        worldTex = Raylib.LoadRenderTexture(newWidth, newHeight);
        uiTex = Raylib.LoadRenderTexture(newWidth, newHeight);

        width = newWidth;
        height = newHeight;
        initialized = true;
    }
}

public struct WorldDrawEvent : IEngineEvent
{
    public bool Handled { get; set; }
    public RenderTexture2D RenderTexture;
    public Camera? Camera;
}

public struct UiDrawEvent : IEngineEvent
{
    public bool Handled { get; set; }
    public RenderTexture2D RenderTexture;
}

public struct FrameCompleteEvent : IEngineEvent
{
    public bool Handled { get; set; }
    public RenderTexture2D WorldTexture;
    public RenderTexture2D UiTexture;
}
