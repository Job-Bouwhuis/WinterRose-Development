using Microsoft.Xna.Framework;
using System;
using TopDownGame.Components.Drops;
using TopDownGame.Components.Loot;
using TopDownGame.Components.Players;
using TopDownGame.Drops;
using TopDownGame.Items;
using TopDownGame.Players;
using TopDownGame.Resources;
using WinterRose;
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
        WorldGrid.ChunkSize = 48;

        // internal type loading of the 'WinterRose.Monogame' and 'WinterRose.Monogame.DamageSystem'  types
        _ = new Vitality().Equals(this);
        _ = new WorldObject().Equals(this);

        world.Name = "Level 1";

        var player = world.CreateObject<SpriteRenderer>("Player", 50, 50, Color.Red);
        player.AttachComponent<SquareCollider>().IgnoredFlags.Add("PlayerBullet");
        player.AttachComponent<ModifiablePlayerMovement>();
        player.AttachComponent<PlayerSprint>();
        var effector = player.AttachComponent<StatusEffector>();
        player.AttachComponent<Dash>();
        var vitals = player.AttachComponent<Vitality>();
        vitals.Health.MaxHealth = 2500;
        vitals.Armor.BaseArmor = 0.97f;
        player.owner.Flag = "Player";
        player.AttachComponent<Player>("Test");

        var gun = CreateSMG(world).owner;
        gun.FetchComponent<Weapon>().AvailableFireingMode = WeaponFireingMode.Auto;
        gun.FetchComponent<Weapon>().CurrentFiringMode = WeaponFireingMode.Auto;

        Mod<Weapon> mod = new("Hard Hitter", "Increases damage of the weapon");
        mod.AddAttribute<WeaponDamageMod>().DamageBoost = 5;

        var container = gun.FetchComponent<Weapon>().ModContainer;
        container.TotalModCapacity = 100;
        container.AddMod(mod);

        gun.transform.parent = player.transform;
        gun.transform.position = new();

        LootTable table = new("box");
        table.Add([
            new(.5f, new ResourceItem() { Item = new Crystal()}),
            new(.5f, new ResourceItem() { Item = new Flesh()})]);

        table.Save();

        world.CreateObject<SmoothCameraFollow>("cam", player.transform).Speed = 8;

        var box = world.CreateObject("box");
        box.transform.position = new(500, 100);
        var renderer = box.AttachComponent<SpriteRenderer>(200, 50, Color.Blue);
        box.AttachComponent<SquareCollider>(renderer);
        var boxhealth = box.AttachComponent<Vitality>();
        box.AttachComponent<DestroyOnDeath>();
        box.AttachComponent<DropOnDeath>().LootTable = LootTable.WithName("box");
        box.AttachComponent<StatusEffector>();

        //var itemObject = world.CreateObject<SpriteRenderer>("item", 5, 5, new Color(255, 150, 255));
        //itemObject.transform.position = new Vector2(500, 500);
        //ResourceItem item = new();
        //item.Item = new Flesh();
        //item.Count = 1;
        //itemObject.AttachComponent<ItemDrop>(item);

        //var itemObject2 = world.CreateObject<SpriteRenderer>("item", 5, 5, new Color(255, 150, 255));
        //itemObject2.transform.position = new Vector2(530, 500);
        //ResourceItem item2 = new();
        //item2.Item = new Flesh();
        //item2.Count = 1;
        //itemObject2.AttachComponent<ItemDrop>(item2);

        //Time.Timescale = 0.1f;

        // Spawning multiple items in a circle
        int itemCount = 100; 
        Vector2 center = new Vector2(500, 500);
        float spawnRadius = 1000;

        for (int i = 0; i < itemCount; i++)
        {
            Vector2 spawnPos = center + RandomPointInCircle(spawnRadius);

            ResourceItem item = new();
            Color col;
            if (Random.Shared.NextDouble() > .5)
            {
                item.Item = new Flesh();
                col = new Color(255, 80, 80);
            }
            else
            {
                col = new Color(255, 150, 255);
                item.Item = new Crystal();
            }
            item.Count = 1;

            ItemDrop.Create(spawnPos, item, world);
        }

        Application.Current.CameraIndex = 0;
    }

    public static Vector2 RandomPointInCircle(float radius)
    {
        float angle = new Random().NextFloat(0, MathF.PI * 2);
        float distance = MathF.Sqrt(new Random().NextFloat(0, 1)) * radius;
        return new Vector2(MathF.Cos(angle) * distance, MathF.Sin(angle) * distance);
    }

    Weapon CreatePistol(World world)
    {
        var bullet = world.CreateObject<SpriteRenderer>("PistolBullet", 25, 8, Color.Red);
        var collider = bullet.AttachComponent<SquareCollider>();
        collider.IgnoredFlags.Add("Player");
        collider.ResolveOverlaps = false;
        var proj = bullet.AttachComponent<Projectile>();
        proj.Speed = 1600;
        proj.Damage = new FireDamage(100);
        proj.Lifetime = 5;
        proj.StatusChance = 40;
        bullet.AttachComponent<DefaultProjectileHitAction>();
        bullet.owner.Flag = "PlayerBullet";
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
        var collider = bullet.AttachComponent<SquareCollider>();
        collider.IgnoredFlags.Add("Player");
        collider.ResolveOverlaps = false;
        var proj = bullet.AttachComponent<Projectile>();
        proj.Speed = 1600;
        proj.Damage = new FireDamage(10);
        proj.Lifetime = 5;
        proj.Spread = .1f;
        proj.StatusChance = 50;
        bullet.AttachComponent<DefaultProjectileHitAction>();
        bullet.owner.Flag = "PlayerBullet";
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
        var collider = bullet.AttachComponent<SquareCollider>();
        collider.IgnoredFlags.Add("Player");
        collider.ResolveOverlaps = false;
        var proj = bullet.AttachComponent<Projectile>();
        proj.Speed = 270;
        proj.Damage = new FireDamage(1);
        proj.Lifetime = 1.3f;
        proj.Spread = .5f;
        bullet.AttachComponent<DefaultProjectileHitAction>();
        bullet.owner.Flag = "PlayerBullet";
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
