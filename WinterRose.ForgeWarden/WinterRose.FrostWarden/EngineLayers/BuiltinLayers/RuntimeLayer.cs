using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;

public class RuntimeLayer : EngineLayer
{
    public RuntimeLayer() : base("Runtime") => Importance = 0;

    public override void OnUpdate() => Universe.CurrentWorld.Update();

    public override void OnRender()
    {
        // world
        Raylib.DrawTexturePro(
            this.worldTexture.Texture,
            new Rectangle(0, 0, this.worldTexture.Texture.Width, -this.worldTexture.Texture.Height),
            new Rectangle(0, 0, Engine.Window.Width, Engine.Window.Height),
            Vector2.Zero,
            0,
            Color.White);

        // UI overlay
        Raylib.DrawTexturePro(
            this.uiTexture.Texture,
            new Rectangle(0, 0, this.uiTexture.Texture.Width, -this.uiTexture.Texture.Height),
            new Rectangle(0, 0, Engine.Window.Width, Engine.Window.Height),
            Vector2.Zero,
            0,
            Color.White);
    }

    private RenderTexture2D worldTexture;
    private RenderTexture2D uiTexture;

    public override void OnEvent<TEvent>(ref TEvent engineEvent)
    {
        if (engineEvent is not FrameCompleteEvent e)
            return;

        worldTexture = e.WorldTexture;
        uiTexture = e.UiTexture;
    }
}
