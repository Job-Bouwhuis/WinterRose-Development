using Microsoft.Xna.Framework;
using System;
using System.Security.Principal;
using TopDownGame.Drops;
using TopDownGame.Enemies;
using TopDownGame.Enemies.Movement;
using TopDownGame.Inventories.Base;
using TopDownGame.Items;
using TopDownGame.Loot;
using TopDownGame.Players;
using TopDownGame.Resources;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Monogame.ModdingSystem;
using WinterRose.Monogame.StatusSystem;
using WinterRose.Monogame.UI;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;
using WinterRose.WinterForgeSerializing;

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

        //Rope r = world.CreateObject<Rope>("rope", new Vector2(100, 100), new Vector2(200, 100), 1, 0.2f);

        world.Name = "Level 1";

        var player = world.CreateObject<SpriteRenderer>("Player", 50, 50, Color.Red);
        player.AttachComponent<SquareCollider>().IgnoredFlags.Add("PlayerBullet");
        player.AttachComponent<ModifiablePlayerMovement>();
        player.AttachComponent<PlayerSprint>();
        var effector = player.AttachComponent<StatusEffector>();
        player.AttachComponent<Dash>();
        var vitals = player.FetchOrAttachComponent<Vitality>();
        vitals.Health.MaxHealth = 2500;
        vitals.Armor.BaseArmor = 0.97f;
        player.owner.Flag = "Player";
        player.AttachComponent<Player>("Test");

        world.CreateObject<SmoothCameraFollow>("camera").Target = player.transform;

        MonoUtils.TargetFramerate = 144;

        var gun = CreateSMG(world).owner;
        gun.FetchComponent<Weapon>().AvailableFireingMode = WeaponFireingMode.Auto;
        gun.FetchComponent<Weapon>().CurrentFiringMode = WeaponFireingMode.Auto;

        Mod<Weapon> mod = new("Hard Hitter", "Increases damage of the weapon");
        mod.AddAttribute<WeaponDamageMod>().DamageBoost = 5;

        var modContainer = gun.FetchComponent<Weapon>().ModContainer;
        modContainer.TotalModCapacity = 100;
        modContainer.AddMod(mod);

        gun.transform.parent = player.transform;
        gun.transform.position = new();

        var canvas = world.CreateObject<UICanvas>("Canvas");
        var button = world.CreateObject<Button>("text");
        button.text.text = "Some Weird Text";
        button.ButtonTints.Normal = Color.Cyan;
        button.transform.parent = canvas.transform;
        button.transform.position = new(200, 200);

        //var coeb = world.CreateObject<SpriteRenderer>("coeb", new Sprite(200, 200, Color.Pink));
        //coeb.transform.position = new(10, 10);
        //coeb.transform.parent = canvas.transform;

        var enemy = world.CreateObject("enemy");
        enemy.transform.position = new(400, 50);
        var renderer = enemy.AttachComponent<SpriteRenderer>(50, 50, Color.Blue);
        enemy.AttachComponent<SquareCollider>(renderer);
        enemy.AttachComponent<Vitality>();
        enemy.AttachComponent<DestroyOnDeath>();
        enemy.AttachComponent<DropOnDeath>().LootTable = LootTable.WithName("box");
        enemy.AttachComponent<StatusEffector>();
        var mc = enemy.AttachComponent<AIMovementController>();
        mc.AddMovement<IdleMovement>();
        mc.AddMovement<ChasePlayer>();
        mc.AddMovement<EvadePlayer>();
        mc.Target = player.transform;
        enemy.AttachComponent<Enemy>();
        enemy.CreatePrefab("Enemy");
        world.DestroyImmediately(enemy);

        // Spawning multiple items in a circle
        int itemCount = 1000;
        Vector2 center = new Vector2(500, 500);
        float spawnRadius = 2000;

        var loot = LootTable.WithName("box");
        Random rnd = new Random();
        for (int i = 0; i < itemCount; i++)
        {
            Vector2 spawnPos = center + rnd.RandomPointInCircle(spawnRadius);
            ResourceItem item = (ResourceItem)loot.Generate();
            ItemDrop.Create(spawnPos, item, world);
        }


        int enemyCount = 0;
        spawnRadius = 1000;

        WinterRose.Windows.OpenConsole(false);
        World w = world;
        int enemycount = 0;
        for (int i = 0; i < enemyCount; i++)
        {
            Console.WriteLine($"Dispatched {i} enemies");
            world.Instantiate(WorldObjectPrefab.Load("enemy", true),
                obj =>
                {
                    obj.Name += $"_{enemycount++}";
                    obj.transform.position = center + rnd.RandomPointInCircle(spawnRadius);
                    Enemy e = obj.FetchComponent<Enemy>()!;
                    var weaponObj = CreateEnemyPistol(w).owner;
                    e.Weapon = weaponObj.FetchComponent<Weapon>()!;
                    e.Weapon.FireRate += new Random().NextFloat(-0.1f, 0.1f);
                    weaponObj.transform.parent = obj.transform;
                    weaponObj.transform.localPosition = new();
                    Console.WriteLine($"Created {enemycount} enemies!");
                }, true);
        }
        

        world.SaveTemplate();

        Application.Current.CameraIndex = 0;
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
        //bullet.destroyImmediately();

        var gun = world.CreateObject("Pistol");
        gun.AttachComponent<SpriteRenderer>(35, 15, Color.Yellow);
        var weapon = gun.AttachComponent<Weapon>();
        var mag = gun.AttachOrFetchComponent<Magazine>();
        mag.BulletPrefab = new WorldObjectPrefab("PistolBullet");
        mag.MaxBullets = 9;
        mag.PoolOfProjectiles = 9 * 6;
        gun.AttachComponent<MouseLook>();
        gun.CreatePrefab("Pistol");
        //gun.DestroyImmediately();

        return weapon;
    }

    Weapon CreateEnemyPistol(World world)
    {
        var bullet = world.CreateObject<SpriteRenderer>("EnemyPistolBullet", 25, 8, Color.Red);
        var collider = bullet.AttachComponent<SquareCollider>();
        collider.IgnoredFlags.Add("Enemy");
        collider.ResolveOverlaps = false;
        var proj = bullet.AttachComponent<Projectile>();
        proj.Speed = 2000;
        proj.Damage = new FireDamage(100);
        proj.Lifetime = 5;
        proj.StatusChance = 40;
        bullet.AttachComponent<DefaultProjectileHitAction>();
        bullet.owner.Flag = "EnemyBullet";
        bullet.owner.CreatePrefab("EnemyPistolBullet");
        world.DestroyImmediately(bullet.owner);

        var gun = world.CreateObject("Pistol");
        gun.AttachComponent<SpriteRenderer>(35, 15, Color.Yellow);
        var weapon = gun.AttachComponent<Weapon>();
        weapon.FireRate = 0.2f;
        var mag = gun.AttachOrFetchComponent<Magazine>();
        mag.BulletPrefab = new WorldObjectPrefab("EnemyPistolBullet");
        mag.MaxBullets = int.MaxValue;
        mag.PoolOfProjectiles = 1;
        gun.AttachComponent<GunLookatPlayer>();
        gun.CreatePrefab("EnemyPistol");

        return weapon;
    }

    Weapon CreateSMG(World world)
    {
        var bullet = world.CreateObject<SpriteRenderer>("SMGBullet", 25, 8, Color.Red);
        var collider = bullet.AttachComponent<SquareCollider>();
        collider.IgnoredFlags.Add("Player");
        collider.ResolveOverlaps = false;
        var proj = bullet.AttachComponent<Projectile>();
        proj.Speed = 6000;
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
        weapon.Rarity = Rarity.CreateRarity(0, "awesome", Color.Red);
        weapon.AvailableFireingMode = WeaponFireingMode.Auto | WeaponFireingMode.Burst | WeaponFireingMode.Single;
        weapon.CurrentFiringMode = WeaponFireingMode.Auto;
        weapon.IsPlayerGun = true;
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
