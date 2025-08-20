using BulletSharp;
using Raylib_cs;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.DialogBoxes;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Physics;
using WinterRose.ForgeWarden.Shaders;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.ToastNotifications;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.Tests;

internal class Program : Application
{
    static void Main(string[] args)
    {
        new Program().Run();
    }

    UIContext ui = new();

    public override void Draw()
    {
        //ui.Begin(new(100, 100), whote)
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

        Toasts.ShowToast(
            new Toast(ToastType.Info, ToastRegion.Left, ToastStackSide.Top)
                .AddContent("Right?")
                .AddButton("btn", (t, b) =>
                    {
                        Console.WriteLine("button clicked");
                        t.OpenAsDialog(new DefaultDialog("Horizontal Big",
                        "refer to \\L[https://github.com/Job-Bouwhuis/WinterRose.WinterForge|WinterForge github page] for info",
                        DialogPlacement.HorizontalBig, buttons: ["Ok"], priority: DialogPriority.High));

                        return false;
                    })
                //.AddButton("btn2", (t, b) =>
                //{
                //    Console.WriteLine("button clicked");
                //    t.OpenAsDialog(new DefaultDialog("Horizontal Big",
                //    "refer to \\L[https://github.com/Job-Bouwhuis/WinterRose.WinterForge|WinterForge github page] for info",
                //    DialogPlacement.HorizontalBig, buttons: ["Ok"], priority: DialogPriority.High)
                //    {
                //        Style = new()
                //        {
                //            DialogBackground = Color.Red
                //        }
                //    });

                //    return false;
                //})
                .AddProgressBar(-1, infiniteSpinText: "Waiting for browser download...")
                .AddSprite(Assets.Load<Sprite>("bigimg")));

        Dialogs.Show(new DefaultDialog("Vertical Big", "this is a cool dialog box\n\n\\s[star]\\!", DialogPlacement.VerticalBig, buttons: ["Ok"], priority: DialogPriority.AlwaysFirst));

        Dialogs.Show(new DefaultDialog("Dialog top left", "yes", DialogPlacement.TopLeft, buttons: ["Ok"]));
        Dialogs.Show(new DefaultDialog("Dialog top right", "yes", DialogPlacement.TopRight, buttons: ["Ok"]));
        Dialogs.Show(new DefaultDialog("Dialog bottom left", "yes", DialogPlacement.BottomLeft, buttons: ["Ok"]));
        Dialogs.Show(new DefaultDialog("Dialog bottom right", "yes", DialogPlacement.BottomRight, buttons: ["Ok"]));
        Dialogs.Show(new DefaultDialog("Dialog Center", "yes", DialogPlacement.CenterSmall, buttons: ["Ok"]));
        Dialogs.Show(new DefaultDialog("Dialog top small", "yes", DialogPlacement.TopSmall, buttons: ["Ok"]));
        Dialogs.Show(new DefaultDialog("Dialog left small", "yes", DialogPlacement.LeftSmall, buttons: ["Ok"]));
        Dialogs.Show(new DefaultDialog("Dialog right small", "yes", DialogPlacement.RightSmall, buttons: ["Ok"]));

        Dialogs.Show(new DefaultDialog("Dialog bottom small", "yes \\c[red] rode text \\c[white]  \\s[star]\\!",
        DialogPlacement.BottomSmall, buttons: ["Ok"]));

        Dialogs.Show(new DefaultDialog("Dialog right big", "yes", DialogPlacement.RightBig, buttons: ["Ok"]));

        //world.SaveTemplate();

        return world;
    }
}
