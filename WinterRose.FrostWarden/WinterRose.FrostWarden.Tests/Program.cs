using Raylib_cs;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
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
        RichSpriteRegistry.RegisterSprite("star", new Sprite("crystalitem.png"));

        World world = new World();
        Entity entity = new Entity();
        world.AddEntity(entity);
        entity.transform.scale = new Vector3(4, 4, 1);
        var sr = new SpriteRenderer(new Sprite("crystalitem.png"));
        entity.Add(sr);

        Entity cam = new Entity();
        cam.Add(new Camera());
        world.AddEntity(cam);
        cam.Add(new Mover());
        bool b = false;
        DialogBox.Show("\\c[red]The Title", "some \\c[yellow]awesome message", DialogType.ConfirmCancel,
            placement: DialogPlacement.CenterBig, onImGui: ui =>
            {
                ui.Label("my label");
                if(ui.Button("this button will change your life!"))
                {
                    Console.WriteLine("clicked!");
                }
                b = ui.Checkbox("check!", b);
            });
        return world;
    }
}
