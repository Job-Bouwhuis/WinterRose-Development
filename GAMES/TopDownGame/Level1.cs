using Microsoft.Xna.Framework;
using System.Linq.Expressions;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Monogame.StatusSystem;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;

namespace TopDownGame.Levels;

[WorldTemplate]
internal class Level1 : WorldTemplate
{
    public override void Build(in World world)
    {
        _ = new Vitality().Equals(this);
        _ = new WorldObject().Equals(this);
        //CreatePrefabs(world);

        world.Name = "Level 1";

        var player = world.CreateObject(new WorldObjectPrefab("player"));
        player.AddUpdateBehavior(obj =>
        {
            if (Input.SpaceDown)
            {
                obj.FetchComponent<StatusEffector>().Apply<FireStatusEffect>(10, 1);
            }
        });

        var gun = world.CreateObject(new WorldObjectPrefab("Gun"));
        gun.transform.parent = player.transform;
        gun.transform.position = new();

        world.CreateObject<SmoothCameraFollow>("cam", player.transform);

        Application.Current.CameraIndex = 0;
    }

    private void CreatePrefabs(World world)
    {
        var playerp = world.CreateObject<SpriteRenderer>("Player", 50, 50, Color.Red);
        playerp.AttachComponent<ModyfiablePlayerMovement>().BaseSpeed = 25;
        var vitals = playerp.AttachComponent<Vitality>();
        vitals.Health.MaxHealth = 740;
        vitals.Armor.BaseArmor = 0.5f;
        var effector = playerp.AttachComponent<StatusEffector>();

        WorldObjectPrefab.Create("Player", playerp.owner);

        var gun = world.CreateObject("Gun");

        gun.AttachComponent<SpriteRenderer>(35, 15, Color.Yellow);
        var weapon = gun.AttachComponent<Weapon>();
        var mag = gun.AttachOrFetchComponent<Magazine>();
        mag.BulletPrefab = new WorldObjectPrefab("basicBullet");
        gun.AttachComponent<MouseLook>();

        gun.CreatePrefab("Gun");
    }
}
