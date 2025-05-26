using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Drops;
using TopDownGame.Enemies;
using TopDownGame.Enemies.Movement;
using TopDownGame.Items;
using TopDownGame.Levels;
using TopDownGame.Loading;
using TopDownGame.Loot;
using TopDownGame.Resources;
using WinterRose;
using WinterRose.FileManagement;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace TopDownGame;

public class Game1 : Application
{
    protected override World CreateWorld()
    {
        Constants.Init();
        Hirarchy.Show = true;

        // als fyschieke scherm 2k of meer is, maak game window 1920 x 1080. anders maak hem 1280 x 720
        if (WinterRose.Windows.GetScreenSize().x >= 2560)
            MonoUtils.WindowResolution = new(1920, 1080);
        else
            MonoUtils.WindowResolution = new(1280, 720);

        if(!AssetDatabase.AssetExists("box"))
        {
            LootTable table = LootTable.WithName("box");
            if (table.Table.Count == 0)
                table.Add([
                    new(.5f, new ResourceItem() { Item = new Crystal()}, 1, 2),
                    new(.5f, new ResourceItem() { Item = new Flesh()}, 1, 2)]);
            table.Save();
        }

        benchmark();

        World w = World.FromTemplate<LoadingLevel>("Level 1");
        return w;


        ////w.InstantiateExact(WorldObjectPrefab.Load("enemy", false));
        //w.InstantiateExact(WorldObjectPrefab.Load("HealthBar", false)).transform.position
        //    = new Vector2(200, 200);

        //int itemCount = 0;
        //Vector2 center = new Vector2(500, 500);
        //float spawnRadius = 2000;

        //var loot = LootTable.WithName("box");
        //Random rnd = new Random();
        //for (int i = 0; i < itemCount; i++)
        //{
        //    Vector2 spawnPos = center + rnd.RandomPointInCircle(spawnRadius);
        //    ResourceItem item = (ResourceItem)loot.Generate();
        //    ItemDrop.Create(spawnPos, item, w);
        //}



        //WinterRose.Windows.OpenConsole(false);
        //int enemycounter = 0;

        //int enemyCount = 200;
        //spawnRadius = 1000;
        //for (int i = 0; i < enemyCount; i++)
        //{
        //    Console.WriteLine($"Dispatched {i} enemies");
        //    WorldObjectPrefab fab = new("enemy", true);
        //    w.SchedulePrefabSpawn(fab,
        //        obj =>
        //        {
        //            obj.Name += $"_{enemycounter++}";
        //            obj.transform.position = center + rnd.RandomPointInCircle(spawnRadius);
        //            Enemy e = obj.FetchComponent<Enemy>()!;
        //            var enemyPistol = new WorldObjectPrefab("EnemyPistol", true);
        //            w.SchedulePrefabSpawn(enemyPistol, weaponObj =>
        //            {
        //                e.Weapon = weaponObj.FetchComponent<Weapon>()!;
        //                e.Weapon.FireRate += new Random().NextFloat(-0.1f, 0.1f);
        //                weaponObj.transform.parent = obj.transform;
        //                weaponObj.transform.localPosition = new();
        //            });
        //        });
        //}


        //WinterRose.Windows.CloseConsole();

        //return w;

        World world = World.FromTemplate<Level1>();
        return world;
    }

    static byte[] AES_KEY = new byte[]
    {
            0x3F, 0x7A, 0xC9, 0x11, 0x5D, 0xA2, 0xB3, 0x4E,
            0x8F, 0x01, 0xDD, 0x67, 0x3C, 0x9B, 0x20, 0xF5,
            0x6E, 0x44, 0x19, 0xB7, 0xD2, 0x53, 0x8C, 0xA9,
            0xE3, 0x0F, 0x5B, 0x7D, 0x02, 0x84, 0xC6, 0x1A
    };

    static byte[] AES_IV = new byte[]
        {
        0x7C, 0x21, 0x9D, 0xE5, 0x44, 0x3B, 0x6F, 0x8A,
        0x10, 0xCF, 0x52, 0x77, 0xA1, 0xE0, 0x33, 0x6D
        };

    private void benchmark()
    {
        WinterRose.Windows.OpenConsole();
        Console.WriteLine("Creating data files");
        World w1 = World.FromTemplate<Level1>();
        Console.WriteLine("Serialization speed test...");

        Stopwatch serializationSW = new();
        int i1 = 0;
        int max1 = 20;

        long bestSerializationTime = long.MaxValue;
        long worstSerializationTime = long.MinValue;
        using var aes = Aes.Create();
        aes.Key = AES_KEY;
        aes.IV = AES_IV;
        while (i1++ < max1)
        {
            serializationSW.Restart();

            
            using FileStream file = File.Open("Level 1", FileMode.Create, FileAccess.Write);
            using CryptoStream crypto = new CryptoStream(file, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using GZipStream decompressed = new GZipStream(crypto, CompressionLevel.SmallestSize);
            WinterForge.SerializeToStream(w1, decompressed);
            serializationSW.Stop();

            long elapsed = serializationSW.ElapsedMilliseconds;
            if (elapsed < bestSerializationTime) bestSerializationTime = elapsed;
            if (elapsed > worstSerializationTime) worstSerializationTime = elapsed;

            Console.WriteLine("pass " + i1);
        }

        Console.WriteLine("\n\nDeserialization...");

        Stopwatch deserializationSW = new();
        int i2 = 0;
        int max2 = 20;

        long bestDeserializationTime = long.MaxValue;
        long worstDeserializationTime = long.MinValue;

        while (i2++ < max2)
        {
            deserializationSW.Restart();
            using FileStream file = File.Open("Level 1", FileMode.Open, FileAccess.Read);
            using CryptoStream crypto = new CryptoStream(file, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using GZipStream decompressed = new GZipStream(crypto, CompressionMode.Decompress);
            World d = WinterForge.DeserializeFromStream<World>(decompressed);
            deserializationSW.Stop();

            long elapsed = deserializationSW.ElapsedMilliseconds;
            if (elapsed < bestDeserializationTime) bestDeserializationTime = elapsed;
            if (elapsed > worstDeserializationTime) worstDeserializationTime = elapsed;

            demo(d);
            Console.WriteLine("pass " + i2);
        }

        Console.WriteLine("\n\nDone!");

        Console.WriteLine("\n\nResults:");
        StringBuilder sb = new();
        sb.AppendLine($"Serialization: Best = {bestSerializationTime} ms, Worst = {worstSerializationTime} ms");
        sb.AppendLine($"Deserialization: Best = {bestDeserializationTime} ms, Worst = {worstDeserializationTime} ms");

        int lines = FileManager.ReadAllLines("Level 1").Length;
        sb.AppendLine("Total instructions: " + lines);
        sb.AppendLine("file size (bytes): " + new FileInfo("Level 1").Length);
        
        Console.WriteLine(sb.ToString());

        Console.WriteLine("Press enter to copy to clipboard and close");

        Console.ReadLine();

        WinterRose.Windows.Clipboard.WriteString(sb.ToString());

        Environment.Exit(0);
    }

    void demo(World w)
    {
        w.GetCamera(0);
    }
}