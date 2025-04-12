using Microsoft.Xna.Framework;
using System.IO;
using TopDownGame.Enemies.Movement;
using TopDownGame.Items;
using TopDownGame.Levels;
using TopDownGame.Loot;
using TopDownGame.Resources;
using WinterRose;
using WinterRose.FileManagement;
using WinterRose.Monogame;
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

        LootTable table = LootTable.WithName("box");
        if (table.Table.Count == 0)
            table.Add([
                new(.5f, new ResourceItem() { Item = new Crystal()}, 1, 2),
                new(.5f, new ResourceItem() { Item = new Flesh()}, 1, 2)]);

        table.Save();

        World w = World.FromTemplate("Level 1");
        w.InstantiateExact(WorldObjectPrefab.Load("enemy"));
        Transform player = w.FindObjectWithFlag("Player")!.transform;
        w.FindObjectsWithFlag("enemy")
            .Foreach(o => o.FetchComponent<AIMovementController>()!.Target = player);

        w.InstantiateExact(WorldObjectPrefab.Load("HealthBar")).transform.position
            = new Vector2(200, 200);

        return w;
        World world = World.FromTemplate<Level1>();
        return world;
    }
}