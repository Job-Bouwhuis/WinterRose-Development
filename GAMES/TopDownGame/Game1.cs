using TopDownGame.Items;
using TopDownGame.Levels;
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

        World world = World.FromTemplate<Level1>();

        SerializerSettings settings = new()
        {
            IncludeType = true,
            CircleReferencesEnabled = true
        };

        string serialied = SnowSerializer.Serialize(world, settings);
        World deserialized = SnowSerializer.Deserialize<World>(serialied, settings).Result;

        return world;
    }
}
