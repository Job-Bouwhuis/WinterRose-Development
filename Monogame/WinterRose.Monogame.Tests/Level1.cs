using Microsoft.Xna.Framework;
using WinterRose.Monogame.StatusSystem;
using WinterRose.Monogame.Tests;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.DamageSystem.Tests;

[WorldTemplate]
internal class Level1 : WorldTemplate
{
    public override void Build(in World world)
    {
        world.Name = "Level 1";
        WorldGrid.ChunkSize = 8;

        var player = world.CreateObject<SpriteRenderer>("Player", 10, 10, Color.Red);
        player.Origin = new(0.5f, 0.5f);
        player.AttachComponent<TopDownPlayerController>();

        player.AttachComponent<BallSpawner>(Sprite.Circle(10, Color.Cyan));

        world.CreateObject<SmoothCameraFollow>("cam", player.transform);

        Application.Current.CameraIndex = 0;
    }
}
