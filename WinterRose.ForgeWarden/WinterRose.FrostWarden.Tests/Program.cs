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
using WinterRose.ForgeWarden.UserInterface.Windowing;

namespace WinterRose.ForgeWarden.Tests;

internal class Program : Application
{
    // for on PC
    //const int SCREEN_WIDTH = 1920;
    //const int SCREEN_HEIGHT = 1080;

    // for on laptop
    const int SCREEN_WIDTH = 1280;
    const int SCREEN_HEIGHT = 720;

    // for on steam deck
    //const int SCREEN_WIDTH = 960;
    //const int SCREEN_HEIGHT = 540;

    [STAThread]
    static void Main(string[] args)
    {
        new Program().RunAsOverlay();

        //new Program().Run("ForgeWarden Tests", SCREEN_WIDTH, SCREEN_HEIGHT);
    }

    public override void Draw()
    {
        ShowFPS = true;
    }

    public override World CreateWorld()
    {
        ClearColor = Color.Beige;
        RichSpriteRegistry.RegisterSprite("star", new Sprite("bigimg"));

        World world = new World("testworld");

        var cam = world.CreateEntity<Camera>("cam");
        cam.AddComponent<Mover>();

        var entity = world.CreateEntity("entity");
        entity.transform.parent = cam.transform;
        entity.transform.scale = new();
        entity.AddComponent<ImportantComponent>();
        entity.AddComponent<SpriteRenderer>(Sprite.CreateRectangle(50, 50, Color.Red));

        UIWindow window = new UIWindow("Window 1", 300, 500);
        window.AddText("window 1", UIFontSizePreset.Title);
        window.AddButton("My Awesome Button 1", (c, b) =>
        {
            Toasts.Success("hmmm no");
        });
        window.AddButton("My Awesome Button 2", (c, b) =>
        {
            Toasts.Success("frictionless wipe");
        });
        window.AddButton("My Awesome Button 3", (c, b) =>
        {
            Toasts.Success("bubu love bubu waaaa");
        });
        window.AddButton("My Awesome Button 4", (c, b) =>
        {
            Toasts.Success("yes");
        });
        window.AddSprite(Assets.Load<Sprite>("bigimg"));
        window.AddSprite(Assets.Load<Sprite>("bigimg"));
        window.AddSprite(Assets.Load<Sprite>("bigimg"));
        window.AddSprite(Assets.Load<Sprite>("bigimg"));
        window.AddSprite(Assets.Load<Sprite>("bigimg"));
        window.Show();

        UIWindow window2 = new UIWindow("Window 2", 300, 500);
        window2.AddSprite(Assets.Load<Sprite>("bigimg"));
        window2.AddSprite(Assets.Load<Sprite>("bigimg"));
        window2.AddSprite(Assets.Load<Sprite>("bigimg"));
        window2.AddText("window 2", UIFontSizePreset.Title);
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.AddButton("My Awesome Button");
        window2.Show();
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

        ShowToast(ToastRegion.Right, ToastStackSide.Top);

        void ShowToast(ToastRegion r, ToastStackSide s)
        {
            var d = new Dialog("Horizontal Big",
                                "refer to \\L[https://github.com/Job-Bouwhuis/WinterRose.WinterForge|WinterForge github page] for info",
                                DialogPlacement.RightBig, priority: DialogPriority.High)
                            .AddContent(new UIButton("OK", (c, b) =>
                            {
                                ShowToast(r, s);
                                c.Close();
                            }))
                            .AddProgressBar(-1)
                            .AddSprite(Assets.Load<Sprite>("bigimg"));

            var w = new UIWindow("Test Window", 400, 500, 100, 100);
            //w.Style.ShowMaximizeButton = false;
            //w.Style.ShowCollapseButton = false;
            //w.Style.ShowCloseButton = false;

            Toast t = new Toast(ToastType.Info, r, s)
                    //.AddText("Right?\n\n\nYes")
                    //.AddButton("btn", (t, b) => ((Toast)t).OpenAsDialog(d))
                    //.AddButton("btn2", (t, b) => Toasts.Success("Worked!", ToastRegion.Right, ToastStackSide.Bottom))
                    .AddButton("show window normal", (c, b) => w.Show())
                    .AddButton("show window collapsed", (c, b) => w.ShowCollapsed())
                    .AddButton("show window maximized", (c, b) => w.ShowMaximized())
                    .AddButton("close window", (c, b) => w.Close())
                    //.AddProgressBar(-1, infiniteSpinText: "Waiting for browser download...")
                    //.AddSprite(Assets.Load<Sprite>("bigimg"))
                    //.AddContent(new HeavyFileDropContent())
                    ;


            t.Style.TimeUntilAutoDismiss = 0;
            Toasts.ShowToast(t);
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
