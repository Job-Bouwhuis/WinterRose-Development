using Microsoft.Xna.Framework;
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
        world.Name = "Level 1";
        var player = world.CreateObject<SpriteRenderer>("Player", 50, 50, Color.Red);
        player.AttachComponent<ModyfiablePlayerMovement>();
        var vitals = player.AttachComponent<Vitality>();
        vitals.Health.MaxHealth = 740;
        vitals.Armor.BaseArmor = 0.5f;
        var effector = player.AttachComponent<StatusEffector>();
        player.owner.AddUpdateBehavior(obj =>
        {
            if (Input.SpaceDown)
            {
                effector.Apply<FireStatusEffect>(10, 1);
            }
        });
        player.AttachComponent<MouseLook>();
        var weapon = player.AttachComponent<Weapon>();
        var mag = player.AttachOrFetchComponent<Magazine>();
        mag.BulletPrefab = new WorldObjectPrefab("basicBullet");

        world.CreateObject<SmoothCameraFollow>("cam", player.transform);

        Application.Current.CameraIndex = 0;
    }
}
