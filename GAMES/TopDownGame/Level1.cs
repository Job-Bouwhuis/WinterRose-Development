using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Linq.Expressions;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Monogame.Servers;
using WinterRose.Monogame.StatusSystem;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;
using WinterRose.Networking;

namespace TopDownGame.Levels;

[WorldTemplate]
internal class Level1 : WorldTemplate
{
    public override void Build(in World world)
    {
        // internal type loading of the 'WinterRose.Monogame' and 'WinterRose.Monogame.DamageSystem'  types
        _ = new Vitality().Equals(this);
        _ = new WorldObject().Equals(this);

        if (!AssetDatabase.AssetExists("Player"))
            CreatePrefabs(world);

        world.Name = "Level 1";

        var player = world.CreateObject(new WorldObjectPrefab("Player"));

        var gun = world.CreateObject(new WorldObjectPrefab("Pistol"));
        gun.transform.parent = player.transform;
        gun.transform.position = new();
        var pe = gun.AttachComponent<ParticleEmitter>();
        pe.ParticleSize = new([new(0, 10), new(0.8f, 10), new(1, 0)]);
        pe.MaxParticles = 10000;
        pe.ParticleSpeed = new([new(0, 10), new(0.8f, 10), new(1, 0)]);
        pe.Sprite = Sprite.Circle(10, Color.OrangeRed);
        pe.AutoEmit = false;
        player.AddUpdateBehavior(obj =>
        {
            if (Input.SpaceDown)
            {
                obj.FetchComponent<StatusEffector>().Apply<FireStatusEffect>(10, 1);
            }
            if (Input.MouseLeftPressed)
            {
                pe.Emit(1500);
            }
        });

        world.CreateObject<SmoothCameraFollow>("cam", player.transform).Speed = 100;

        Application.Current.CameraIndex = 0;
    }

    private void CreatePrefabs(World world)
    {
        var player = world.CreateObject<SpriteRenderer>("Player", 50, 50, Color.Red);
        player.AttachComponent<ModyfiablePlayerMovement>().BaseSpeed = 25;
        var vitals = player.AttachComponent<Vitality>();
        vitals.Health.MaxHealth = 740;
        vitals.Armor.BaseArmor = 0.5f;
        var effector = player.AttachComponent<StatusEffector>();

        WorldObjectPrefab.Create("Player", player.owner);

        CreatePistol();
        CreateSMG();

        if (Debugger.IsAttached)
        {
            WinterRose.Windows.MessageBox("Game must restart after creating prefabs.", "Attention");
            MonoUtils.RestartApp();
        }

        void CreatePistol()
        {
            var bullet = world.CreateObject<SpriteRenderer>("PistolBullet", 25, 8, Color.Red);
            bullet.AttachComponent<SquareCollider>();
            var proj = bullet.AttachComponent<Projectile>();
            proj.Speed = 800;
            proj.Damage = new FireDamage() { BaseDamage = 10 };
            proj.Lifetime = 5;
            bullet.AttachComponent<DefaultProjectileHitAction>();
            bullet.owner.CreatePrefab("PistolBullet");

            var gun = world.CreateObject("Pistol");
            gun.AttachComponent<SpriteRenderer>(35, 15, Color.Yellow);
            var weapon = gun.AttachComponent<Weapon>();
            var mag = gun.AttachOrFetchComponent<Magazine>();
            mag.BulletPrefab = new WorldObjectPrefab("PistolBullet");
            mag.MaxBullets = 9;
            mag.PoolOfProjectiles = 9 * 6;
            gun.AttachComponent<MouseLook>();
            gun.CreatePrefab("Pistol");
        }
        void CreateSMG()
        {
            var bullet = world.CreateObject<SpriteRenderer>("PistolBullet", 25, 8, Color.Red);
            bullet.AttachComponent<SquareCollider>();
            var proj = bullet.AttachComponent<Projectile>();
            proj.Speed = 800;
            proj.Damage = new FireDamage() { BaseDamage = 10 };
            proj.Lifetime = 5;
            bullet.AttachComponent<DefaultProjectileHitAction>();
            bullet.owner.CreatePrefab("SMGBullet");

            var gun = world.CreateObject("SMG");
            gun.AttachComponent<SpriteRenderer>(35, 15, Color.Orange);
            var weapon = gun.AttachComponent<Weapon>();
            var mag = gun.AttachOrFetchComponent<Magazine>();
            mag.BulletPrefab = new WorldObjectPrefab("SMGBullet");
            mag.MaxBullets = 25;
            gun.AttachComponent<MouseLook>();
            gun.CreatePrefab("SMG");
        }
    }
}
