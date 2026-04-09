using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;

public class WorldRenderLayer : EngineLayer
{
    public WorldRenderLayer() : base("WorldRender") => Importance = -450;

    public override void OnEvent<TEvent>(ref TEvent engineEvent)
    {
        if (engineEvent is not WorldDrawEvent e)
            return;

        var camera = e.Camera;

        if (camera != null)
        {
            if (camera.is3D)
                Raylib.BeginMode3D(camera.Camera3D);
            else
                Raylib.BeginMode2D(camera.Camera2D);
        }

        Universe.CurrentWorld.Draw(camera?.ViewMatrix ?? Matrix4x4.Identity);

        if (camera != null)
        {
            if (camera.is3D)
                Raylib.EndMode3D();
            else
                Raylib.EndMode2D();
        }
    }
}
