using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.Geometry.Rendering;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.ForgeWarden.UserInterface.Windowing;

namespace WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;

public class UiLayer : EngineLayer
{
    public ShapeRenderer ShapeRenderer { get; } = new(new ShapeAnimationSystem());

    public UiLayer() : base("UI") => Importance = -400;

    public override void OnEvent<TEvent>(ref TEvent engineEvent)
    {
        if (engineEvent is not UiDrawEvent)
            return;

        UIWindowManager.Draw();
        Dialogs.Draw();
        ToastToDialogMorpher.Draw();
        Toasts.Draw();
        Tooltips.Draw();
        ShapeRenderer.Draw();
    }

    public override void OnUpdate()
    {
        UIWindowManager.Update();
        Dialogs.Update();
        ToastToDialogMorpher.Update();
        Toasts.Update();
        Tooltips.Update();
        ShapeRenderer.Update();
    }
}
