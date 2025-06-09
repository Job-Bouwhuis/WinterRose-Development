using Raylib_cs;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
using WinterRose.FrostWarden.Shaders;
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
        World world = new World();
        Entity entity = new Entity();
        world.AddEntity(entity);
        entity.transform.scale = new Vector3(4, 4, 1);
        var sr = new SpriteRenderer(new Sprite("crystalitem.png"));
        sr.SetShader(new FrostShader("shader.vert", "shader.frag"));
        entity.Add(sr);
        entity.Add(new Mover());

        Entity cam = new Entity();
        cam.Add(new Camera());
        world.AddEntity(cam);

        DialogBox.Show("The Title", "some awesome message", DialogType.ConfirmCancel,
            placement: DialogPlacement.LeftSmall);

        return world;
    }
}
