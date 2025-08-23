using BulletSharp;
using Raylib_cs;
using System.Runtime.InteropServices;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Physics;
using WinterRose.ForgeWarden.Shaders;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Enums;
using WinterRose.ForgeWarden.UserInterface.DragDrop;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.Tests;

internal class Program : Application
{
    // for on PC
    const int SCREEN_WIDTH = 1920;
    const int SCREEN_HEIGHT = 1080;

    // for on laptop
    //const int SCREEN_WIDTH = 1280;
    //const int SCREEN_HEIGHT = 720;

    // for on steam deck
    //const int SCREEN_WIDTH = 960;
    //const int SCREEN_HEIGHT = 540;

    [STAThread]
    static void Main(string[] args)
    {
        var monitorsize = Windows.GetScreenSize();
        //new Program().Run("ForgeWarden Tests", SCREEN_WIDTH, SCREEN_HEIGHT 
        new Program().Run("ForgeWarden Tests", monitorsize.X, monitorsize.Y
            ,
            ConfigFlags.AlwaysRunWindow
            | ConfigFlags.MousePassthroughWindow
            | ConfigFlags.UndecoratedWindow
            | ConfigFlags.TransparentWindow
            | ConfigFlags.BorderlessWindowMode
            | ConfigFlags.TopmostWindow
            );
    }

    public override void Draw()
    {
    }

    public override World CreateWorld()
    {
        RichSpriteRegistry.RegisterSprite("star", new Sprite("bigimg"));

        //return World.FromFile("testworld");

        World world = new World("testworld");

        var cam = world.CreateEntity<Camera>("cam");
        cam.AddComponent<Mover>();

        var entity = world.CreateEntity("entity");
        entity.transform.parent = cam.transform;
        entity.transform.scale = new();
        entity.AddComponent<ImportantComponent>();
        entity.AddComponent<SpriteRenderer>(Sprite.CreateRectangle(50, 50, Color.Red));

        //ShowToast(ToastRegion.Left, ToastStackSide.Top);
        //ShowToast(ToastRegion.Left, ToastStackSide.Top);
        //ShowToast(ToastRegion.Right, ToastStackSide.Bottom);
        //ShowToast(ToastRegion.Right, ToastStackSide.Bottom);

        //Dialogs.Show(new Dialog("Horizontal Big",
        //    "refer to \\L[https://github.com/Job-Bouwhuis/WinterRose.WinterForge|WinterForge github page] for info",
        //    DialogPlacement.RightBig, priority: DialogPriority.High)
        //.AddContent(new UIButton("OK", (c, b) =>
        //{
        //    ShowToast(ToastRegion.Left, ToastStackSide.Top);
        //    c.Close();
        //})));

        void ShowToast(ToastRegion r, ToastStackSide s)
        {
            Toasts.ShowToast(
                new Toast(ToastType.Info, r, s)
                    .AddText("Right?")
                    .AddButton("btn", (t, b) => ((Toast)t).OpenAsDialog(
                            new Dialog("Horizontal Big",
                                "refer to \\L[https://github.com/Job-Bouwhuis/WinterRose.WinterForge|WinterForge github page] for info",
                                DialogPlacement.RightBig, priority: DialogPriority.High)
                            .AddContent(new UIButton("OK", (c, b) =>
                            {
                                ShowToast(r, s);
                                c.Close();
                            }))
                            .AddProgressBar(-1)))
                    .AddButton("btn2", (t, b) => Toasts.Success("Worked!", ToastRegion.Right, ToastStackSide.Top))
                    .AddButton("btn3", (c, b) => Application.Close())
                    .AddProgressBar(-1, infiniteSpinText: "Waiting for browser download...")
                    //.AddSprite(Assets.Load<Sprite>("bigimg"))
                    .AddContent(new HeavyFileDropContent()));
        }


        //Dialogs.Show(new Dialog("Vertical Big", "this is a cool dialog box\n\n\\s[star]\\!", DialogPlacement.HorizontalBig, priority: DialogPriority.AlwaysFirst).AddButton("Ok"));

        //Dialogs.Show(new Dialog("Dialog top left", "yes", DialogPlacement.TopLeft).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog top right", "yes", DialogPlacement.TopRight).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog bottom left", "yes", DialogPlacement.BottomLeft).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog bottom right", "yes", DialogPlacement.BottomRight).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog Center", "yes", DialogPlacement.CenterSmall).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog top small", "yes", DialogPlacement.TopSmall).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog left small", "yes", DialogPlacement.LeftSmall).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog right small", "yes", DialogPlacement.RightSmall).AddButton("Ok"));

        //Dialogs.Show(new Dialog("Dialog bottom small", "yes \\c[red] rode text \\c[white]  \\s[star]\\!", DialogPlacement.BottomSmall).AddButton("Ok"));

        //Dialogs.Show(new Dialog("Dialog right big", "yes", DialogPlacement.RightBig).AddButton("Ok"));



        //world.SaveTemplate();

        return world;
    }
}
