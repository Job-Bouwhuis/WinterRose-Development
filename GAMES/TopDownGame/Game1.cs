using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
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
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Compiling;
using WinterRose.WinterForgeSerializing.Instructions;
using WinterRose.WinterForgeSerializing.Workers;

namespace TopDownGame;

public class Game1 : Application
{
    protected override World CreateWorld()
    {
        Constants.Init();
        WinterRose.Windows.OpenConsole();

        //using Stream human = File.OpenRead("staticCallHuamn.txt");
        //File.Delete("staticCallOpcodes.txt");
        //using Stream opcodes = File.Open("staticCallOpcodes.txt", FileMode.CreateNew, FileAccess.ReadWrite);

        //var parser = new HumanReadableParser();
        //parser.Parse(human, opcodes);
        //opcodes.Seek(0, SeekOrigin.Begin);

        //var instr = InstructionParser.ParseOpcodes(opcodes);

        //var exec = new WinterForgeVM();
        //var result = exec.Execute(instr);

        //WinterRose.Monogame.Debug.Log(CameraIndex, true);

        Hirarchy.Show = true;

        //// als fyschieke scherm 2k of meer is, maak game window 1920 x 1080. anders maak hem 1280 x 720
        //if (WinterRose.Windows.GetScreenSize().x >= 2560)
        //    MonoUtils.WindowResolution = new(1920, 1080);
        //else
        //    MonoUtils.WindowResolution = new(1280, 720);

        if (!AssetDatabase.AssetExists("box"))
        {
            LootTable table = LootTable.WithName("box");
            if (table.Table.Count == 0)
                table.Add([
                    new(.5f, new ResourceItem() { Item = new Crystal()}, 1, 2),
                    new(.5f, new ResourceItem() { Item = new Flesh()}, 1, 2)]);
            table.Save();
        }

        benchmark();

        World w = WinterForge.DeserializeFromFile<World>("Level 1");
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

    private void benchmark()
    {
        WinterForge.CompressedStreams = false;
        WinterForge.AllowCustomCompilers = true;
        WinterRose.Windows.OpenConsole();
        Console.WriteLine("Creating data files");
        World w1 = World.FromTemplate<Level1>();
        Console.WriteLine("Serialization speed test...");

        Stopwatch serializationSW = new();
        int i1 = 0;
        int max1 = 15;

        long bestSerializationTime = long.MaxValue;
        long worstSerializationTime = long.MinValue;
        long minSerializationMemory = long.MaxValue;
        long maxSerializationMemory = long.MinValue;

        while (i1++ < max1)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memBefore = GC.GetTotalMemory(true);

            serializationSW.Restart();
            WinterForge.SerializeToFile(w1, "Level 1");
            serializationSW.Stop();

            long memAfter = GC.GetTotalMemory(true);
            long usedMemory = memAfter - memBefore;

            long elapsed = serializationSW.ElapsedMilliseconds;
            if (elapsed < bestSerializationTime) bestSerializationTime = elapsed;
            if (elapsed > worstSerializationTime) worstSerializationTime = elapsed;

            if (usedMemory < minSerializationMemory) minSerializationMemory = usedMemory;
            if (usedMemory > maxSerializationMemory) maxSerializationMemory = usedMemory;

            Console.WriteLine($"pass {i1} - {elapsed} ms, RAM used: {usedMemory / 1024.0:N2} KB");
        }

        Console.WriteLine("\n\nDeserialization...");

        Stopwatch deserializationSW = new();
        int i2 = 0;
        int max2 = 15;

        long bestDeserializationTime = long.MaxValue;
        long worstDeserializationTime = long.MinValue;
        long minDeserializationMemory = long.MaxValue;
        long maxDeserializationMemory = long.MinValue;

        while (i2++ < max2)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memBefore = GC.GetTotalMemory(true);

            deserializationSW.Restart();
            World d = WinterForge.DeserializeFromFile<World>("Level 1");
            deserializationSW.Stop();

            long memAfter = GC.GetTotalMemory(true);
            long usedMemory = memAfter - memBefore;

            long elapsed = deserializationSW.ElapsedMilliseconds;
            if (elapsed < bestDeserializationTime) bestDeserializationTime = elapsed;
            if (elapsed > worstDeserializationTime) worstDeserializationTime = elapsed;

            if (usedMemory < minDeserializationMemory) minDeserializationMemory = usedMemory;
            if (usedMemory > maxDeserializationMemory) maxDeserializationMemory = usedMemory;

            demo(d);
            Console.WriteLine($"pass {i2} - {elapsed} ms, RAM used: {usedMemory / 1024.0:N2} KB");
        }

        Console.WriteLine("\n\nDone!");

        Console.WriteLine("\n\nResults:");
        StringBuilder sb = new();
        sb.AppendLine($"Serialization: Best = {bestSerializationTime} ms, Worst = {worstSerializationTime} ms, RAM used: Min = {minSerializationMemory / 1024.0:N2} KB, Max = {maxSerializationMemory / 1024.0:N2} KB");
        sb.AppendLine($"Deserialization: Best = {bestDeserializationTime} ms, Worst = {worstDeserializationTime} ms, RAM used: Min = {minDeserializationMemory / 1024.0:N2} KB, Max = {maxDeserializationMemory / 1024.0:N2} KB");

        sb.AppendLine("file size (bytes): " + new FileInfo("Level 1").Length);
        using FileStream compiledFile = File.OpenRead("Level 1");
        WinterForgeStreamInfo info = WinterForge.InspectStream(compiledFile);
        sb.AppendLine(info.ToString());

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