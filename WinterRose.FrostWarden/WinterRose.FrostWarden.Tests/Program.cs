using BulletSharp;
using Raylib_cs;
using WinterRose.FrostWarden.Components;
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

        Entity cam = new Entity();
        cam.Add(new Camera());
        cam.Add(new Mover());
        world.AddEntity(cam);
        cam.Add(new BallSpawner());

        //Entity ball = new Entity();
        //var sr = new SpriteRenderer(Sprite.CreateCircle(25, new Color(255, 0, 255)));
        //ball.Add(sr);
        //ball.transform.position = new Vector3(200, 200, 0);
        
        //world.AddEntity(ball);

        //var col = new Collider(new SphereShape(25 / 2));
        //ball.Add(new RigidBodyComponent(col, 250));

        Entity floor = new Entity();
        floor.transform.position = new Vector3(ScreenSize.x / 2, ScreenSize.y - 10, 0);
        floor.Add(new SpriteRenderer(Sprite.CreateRectangle(ScreenSize.x, 5, Color.Beige)));
        world.AddEntity(floor);
        floor.Add(new Collider(new Box2DShape(new BulletSharp.Math.Vector3(ScreenSize.x / 2, 15, 5))));
        return world;
    }
}
