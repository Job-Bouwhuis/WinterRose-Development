using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.IO;
using TopDownGame.Drops;
using TopDownGame.Enemies;
using TopDownGame.Enemies.Movement;
using TopDownGame.Items;
using TopDownGame.Levels;
using TopDownGame.Loot;
using TopDownGame.Resources;
using WinterRose;
using WinterRose.FileManagement;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;
using WinterRose.Serialization;

namespace TopDownGame;

public class Game1 : Application
{
    protected override World CreateWorld()
    {
        Hirarchy.Show = true;

        // als fyschieke scherm 2k of meer is, maak game window 1920 x 1080. anders maak hem 1280 x 720
        if (WinterRose.Windows.GetScreenSize().x >= 2560)
            MonoUtils.WindowResolution = new(1920, 1080);
        else
            MonoUtils.WindowResolution = new(1280, 720);


        //LootTable table = LootTable.WithName("box");
        //if (table.Table.Count == 0)
        //    table.Add([
        //        new(.5f, new ResourceItem() { Item = new Crystal()}, 1, 2),
        //        new(.5f, new ResourceItem() { Item = new Flesh()}, 1, 2)]);

        //table.Save();

        World w = World.FromTemplate("Level 1");
        //w.InstantiateExact(WorldObjectPrefab.Load("enemy", false));
        w.InstantiateExact(WorldObjectPrefab.Load("HealthBar", false)).transform.position
            = new Vector2(200, 200);

        int itemCount = 0;
        Vector2 center = new Vector2(500, 500);
        float spawnRadius = 2000;

        var loot = LootTable.WithName("box");
        Random rnd = new Random();
        for (int i = 0; i < itemCount; i++)
        {
            Vector2 spawnPos = center + rnd.RandomPointInCircle(spawnRadius);
            ResourceItem item = (ResourceItem)loot.Generate();
            ItemDrop.Create(spawnPos, item, w);
        }



        WinterRose.Windows.OpenConsole(false);
        int enemycounter = 0;

        int enemyCount = 200;
        spawnRadius = 1000;
        for (int i = 0; i < enemyCount; i++)
        {
            Console.WriteLine($"Dispatched {i} enemies");
            WorldObjectPrefab fab = new("enemy", true);
            w.SchedulePrefabSpawn(fab,
                obj =>
                {
                    obj.Name += $"_{enemycounter++}";
                    obj.transform.position = center + rnd.RandomPointInCircle(spawnRadius);
                    Enemy e = obj.FetchComponent<Enemy>()!;
                    var enemyPistol = new WorldObjectPrefab("EnemyPistol", true);
                    w.SchedulePrefabSpawn(enemyPistol, weaponObj =>
                    {
                        e.Weapon = weaponObj.FetchComponent<Weapon>()!;
                        e.Weapon.FireRate += new Random().NextFloat(-0.1f, 0.1f);
                        weaponObj.transform.parent = obj.transform;
                        weaponObj.transform.localPosition = new();
                    });
                });
        }


        WinterRose.Windows.CloseConsole();

        return w;

        World world = World.FromTemplate<Level1>();
        return world;
    }
}