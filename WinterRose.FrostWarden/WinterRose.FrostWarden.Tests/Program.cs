using BulletSharp;
using Raylib_cs;
using WinterRose.FrostWarden.AssetPipeline;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.DialogBoxes;
using WinterRose.FrostWarden.DialogBoxes.Boxes;
using WinterRose.FrostWarden.Entities;
using WinterRose.FrostWarden.Physics;
using WinterRose.FrostWarden.Shaders;
using WinterRose.FrostWarden.TextRendering;
using WinterRose.FrostWarden.Worlds;

namespace WinterRose.FrostWarden.Tests;

internal class Program : Application
{
    static void Main(string[] args)
    {
        new Program().Run();
    }

    public override World CreateWorld()
    {
        RichSpriteRegistry.RegisterSprite("star", new Sprite("bigimg.png"));
        World world = new World();

        Entity cam = new Entity("cam");
        cam.AddComponent(new Camera());
        cam.AddComponent(new Mover());
        world.AddEntity(cam);

        Entity entity = new Entity("entity");
        entity.AddComponent(new Mover());
        entity.AddComponent(new SpriteRenderer(Assets.Load<Sprite>("bigimg")));
        world.AddEntity(entity);

        Entity gife = new Entity("gif");
        world.AddEntity(gife);
        gife.transform.scale = new Vector3(2, 2, 1);
        var sr = new SpriteRenderer(Assets.Load<Sprite>("egg gun"));
        gife.AddComponent(sr);

        //Dialogs.Show(new DefaultDialog("Dialog top left", "yes", DialogPlacement.TopLeft, buttons: ["Ok"]));
        //Dialogs.Show(new DefaultDialog("Dialog top right", "yes", DialogPlacement.TopRight, buttons: ["Ok"]));
        //Dialogs.Show(new DefaultDialog("Dialog bottom left", "yes", DialogPlacement.BottomLeft, buttons: ["Ok"]));
        //Dialogs.Show(new DefaultDialog("Dialog bottom right", "yes", DialogPlacement.BottomRight, buttons: ["Ok"]));
        //Dialogs.Show(new DefaultDialog("Dialog Center", "yes", DialogPlacement.CenterSmall, buttons: ["Ok"]));
        //Dialogs.Show(new DefaultDialog("Dialog top small", "yes", DialogPlacement.TopSmall, buttons: ["Ok"]));
        //Dialogs.Show(new DefaultDialog("Dialog left small", "yes", DialogPlacement.LeftSmall, buttons: ["Ok"]));
        //Dialogs.Show(new DefaultDialog("Dialog right small", "yes", DialogPlacement.RightSmall, buttons: ["Ok"]));

        //Dialogs.Show(new DefaultDialog("Dialog bottom small", "yes \\c[red] rode text \\c[white]  \\s[star]\\!",
        //DialogPlacement.BottomSmall, buttons: ["Ok"]));

        //Dialogs.Show(new DefaultDialog("Dialog right big", "yes", DialogPlacement.RightBig, buttons: ["Ok"]));

        return world;
    }
}
