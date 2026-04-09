using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.Editor;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;

public class EngineCoreLayer : EngineLayer
{
    public EngineCoreLayer() : base("EngineCore") => Importance = int.MinValue + 10000;

    public override void OnUpdate()
    {
        InputManager.Update();
        Time.Update();
        GlobalHotkey.Update();
        Universe.Hirarchy.UpdateHirarchy();
        Engine.ShapeRenderer.Update();
        Engine.Update();
        Engine.GlobalThreadLoom.TickThread();
    }
}
