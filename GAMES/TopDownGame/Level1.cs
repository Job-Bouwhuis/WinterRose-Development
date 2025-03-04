using Microsoft.Xna.Framework;
using TopDownGame.Components.Players;
using TopDownGame.Drops;
using TopDownGame.Items;
using TopDownGame.Players;
using TopDownGame.Resources;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Monogame.ModdingSystem;
using WinterRose.Monogame.StatusSystem;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;

namespace TopDownGame.Levels;

[WorldTemplate]
internal class Level1 : WorldTemplate
{
    public override void Build(in World world)
    {
        // internal type loading of the 'WinterRose.Monogame' and 'WinterRose.Monogame.DamageSystem'  types
        _ = new Vitality().Equals(this);
        _ = new WorldObject().Equals(this);

        world.Name = "Level 1";

        var player = world.CreateObject<SpriteRenderer>("Player", 50, 50, Color.Red);
        player.AttachComponent<Player>("Dajuska");
        player.AttachComponent<ModifiablePlayerMovement>(5);
        player.AttachComponent<PlayerSprint>();
        var effector = player.AttachComponent<StatusEffector>();
        player.AttachComponent<Dash>();
        var vitals = player.AttachComponent<Vitality>();
        vitals.Health.MaxHealth = 2500;
        vitals.Armor.BaseArmor = 0.97f;
        player.owner.Flag = "Player";

        var gun = CreateSMG(world).owner;
        gun.FetchComponent<Weapon>().AvailableFireingMode = WeaponFireingMode.Auto;
        gun.FetchComponent<Weapon>().CurrentFiringMode = WeaponFireingMode.Auto;

        Mod<Weapon> mod = new("Hard Hitter", "Increases damage of the weapon");
        mod.AddAttribute<WeaponDamageMod>();

        var container = gun.FetchComponent<Weapon>().ModContainer;
        container.TotalModCapacity = 100;
        container.AddMod(mod);

        gun.transform.parent = player.transform;
        gun.transform.position = new();

        world.CreateObject<SmoothCameraFollow>("cam", player.transform).Speed = 8;

        var box = world.CreateObject("box");
        box.transform.position = new(500, 100);
        var renderer = box.AttachComponent<SpriteRenderer>(200, 50, Color.Blue);
        box.AttachComponent<SquareCollider>(renderer);
        var boxhealth = box.AttachComponent<Vitality>();
        box.AttachComponent<DestroyOnDeath>();
        box.AttachComponent<StatusEffector>();

        var itemObject = world.CreateObject<SpriteRenderer>("item", 50, 50, new Color(0, 255, 0));
        itemObject.transform.position = new Vector2(500, 500);
        ResourceItem item = new();
        item.Item = new Flesh();
        item.Count = 1;
        itemObject.owner.AttachComponent<ItemDrop>(item);

        Application.Current.CameraIndex = 0;
    }

    Weapon CreatePistol(World world)
    {
        var bullet = world.CreateObject<SpriteRenderer>("PistolBullet", 25, 8, Color.Red);
        bullet.AttachComponent<SquareCollider>();
        var proj = bullet.AttachComponent<Projectile>();
        proj.Speed = 1600;
        proj.Damage = new FireDamage(100);
        proj.Lifetime = 5;
        proj.StatusChance = 40;
        bullet.AttachComponent<DefaultProjectileHitAction>();
        bullet.owner.Flag = "Bullet";
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

        return weapon;
    }
    Weapon CreateSMG(World world)
    {
        var bullet = world.CreateObject<SpriteRenderer>("SMGBullet", 25, 8, Color.Red);
        bullet.AttachComponent<SquareCollider>();
        var proj = bullet.AttachComponent<Projectile>();
        proj.Speed = 1600;
        proj.Damage = new FireDamage(10);
        proj.Lifetime = 5;
        proj.Spread = .01f;
        bullet.AttachComponent<DefaultProjectileHitAction>();
        bullet.owner.Flag = "Bullet";
        bullet.owner.CreatePrefab("SMGBullet");
        world.DestroyImmediately(bullet.owner);

        var gun = world.CreateObject("SMG");
        gun.AttachComponent<SpriteRenderer>(35, 15, Color.Orange);
        var weapon = gun.AttachComponent<Weapon>();
        weapon.AvailableFireingMode = WeaponFireingMode.Auto | WeaponFireingMode.Burst | WeaponFireingMode.Single;
        weapon.CurrentFiringMode = WeaponFireingMode.Auto;
        var mag = gun.AttachOrFetchComponent<Magazine>();
        mag.BulletPrefab = new WorldObjectPrefab("SMGBullet");
        mag.MaxBullets = 25;
        gun.AttachComponent<MouseLook>();
        gun.CreatePrefab("SMG");
        return weapon;
    }
    Weapon CreateFlameThrower(World world)
    {
        var bullet = world.CreateObject<SpriteRenderer>("FlamePart", 25, 8, Color.Red);
        bullet.AttachComponent<SquareCollider>();
        var proj = bullet.AttachComponent<Projectile>();
        proj.Speed = 270;
        proj.Damage = new FireDamage(1);
        proj.Lifetime = 1.3f;
        proj.Spread = .5f;
        bullet.AttachComponent<DefaultProjectileHitAction>();
        bullet.owner.Flag = "Bullet";
        bullet.owner.CreatePrefab("FlamePart");

        var gun = world.CreateObject("FlameThrower");
        gun.AttachComponent<SpriteRenderer>(35, 15, Color.Orange);
        var weapon = gun.AttachComponent<Weapon>();
        weapon.AvailableFireingMode = WeaponFireingMode.Auto;
        weapon.CurrentFiringMode = WeaponFireingMode.Auto;
        weapon.FireRate = 45;
        var mag = gun.AttachOrFetchComponent<Magazine>();
        mag.BulletPrefab = new WorldObjectPrefab("FlamePart");
        mag.MaxBullets = 500;
        mag.PoolOfProjectiles = 5000;
        mag.BulletsPerShot = 2;
        gun.AttachComponent<MouseLook>();
        gun.CreatePrefab("FlameThrower");
        return weapon;
    }
}
