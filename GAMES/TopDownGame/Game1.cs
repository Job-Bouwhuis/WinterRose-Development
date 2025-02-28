using Microsoft.Xna.Framework;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Forms.Design;
using TopDownGame.Inventories;
using TopDownGame.Items;
using TopDownGame.Levels;
using WinterRose;
using WinterRose.Encryption;
using WinterRose.FileManagement;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;
using WinterRose.Plugins;
using WinterRose.Serialization;
using WinterRose.SourceGeneration.Serialization;
using WinterRose.WIP.TestClasses;

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

        SerializerSettings settings = new()
        {
            IncludeType = true,
            CircleReferencesEnabled = true,
        };

        const string path = "Content/Worlds/Level1.SerializedWorld";
        FileInfo file = new FileInfo(path);

        if (!file.Exists)
        {
            World world = World.FromTemplate<Level1>();
            string serialized = SnowSerializer.Serialize(world, settings);
            FileManager.Write(path, serialized, true);
            return world;
        }
        string e = FileManager.Read(path);

        World deserialized = SnowSerializer.Deserialize<World>(e, settings).Result;

        return deserialized;
    }
}

public class GenericTest<T>
{
    public T data;
}
