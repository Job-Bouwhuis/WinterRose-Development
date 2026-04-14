using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using VerdantRequiem.Scripts.Player;
using VerdantRequiem.Scripts.Weapons;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.DamageSystem.WeaponSystem;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.TileMaps;
using WinterRose.ForgeWarden.Worlds;

namespace VerdantRequiem;

public partial class Worlds
{
    public static World DebugLevel()
    {
        World world = new World("Debug Level");
        var player = PlayerFactory.CreatePlayer(world);

        Entity wm = world.FindEntityByTag("WeaponMount")!;
        Weapon w = SMGFactory.CreateSMG(world);
        w.transform.parent = wm.transform;

        Camera cam = world.CreateEntity("cam", new Camera());
        cam.transform.position = cam.transform.position with
        {
            Z = 1.2f
        };
        var camFollow = cam.AddComponent<SmoothCamera2DMode>();
        camFollow.Target = player.transform;

        Entity tilemap = world.CreateEntity("TileMap");

        BiomeRegistry biomes = new();
        Biome plains = new(Sprite.CreateRectangle(32, 32, new Color(70, 150, 70, 255)));

        var grassLight = BiomeTileDefinition.FromSprite(Sprite.CreateRectangle(32, 32, new Color(90, 180, 90, 255)), layer: 0, id: "grass_light");
        var grassDark = BiomeTileDefinition.FromSprite(Sprite.CreateRectangle(32, 32, new Color(45, 120, 45, 255)), layer: 0, id: "grass_dark");
        var gravel = BiomeTileDefinition.FromSprite(Sprite.CreateRectangle(32, 32, new Color(120, 120, 120, 255)), layer: 0, id: "gravel");

        var path = BiomeTileDefinition.FromSprite(Sprite.CreateRectangle(32, 32, new Color(170, 130, 90, 255)), layer: 1, id: "path");
        path.AllowNoDetails();
        path
            .AddNeighbor(TileNeighborSide.Left, path, 0.35f)
            .AddNeighbor(TileNeighborSide.Right, path, 0.35f)
            .AddNeighbor(TileNeighborSide.Up, path, 0.35f)
            .AddNeighbor(TileNeighborSide.Down, path, 0.35f);

        var flowerYellow = BiomeTileDefinition.FromSprite(Sprite.CreateCircle(16, Color.Yellow), layer: 2, id: "flower_yellow");
        var flowerPink = BiomeTileDefinition.FromSprite(Sprite.CreateCircle(16, new Color(255, 105, 180, 255)), layer: 2, id: "flower_pink");
        var bush = BiomeTileDefinition.FromSprite(Sprite.CreateCircle(24, new Color(20, 90, 20, 255)), layer: 2, id: "bush");

        flowerYellow
            .AddNeighbor(TileNeighborSide.Left, flowerPink, 0.15f)
            .AddNeighbor(TileNeighborSide.Right, flowerPink, 0.15f)
            .AddNeighbor(TileNeighborSide.Up, flowerYellow, 0.10f)
            .AddNeighbor(TileNeighborSide.Down, flowerYellow, 0.10f);

        bush
            .AddNeighbor(TileNeighborSide.Left, bush, 0.20f)
            .AddNeighbor(TileNeighborSide.Right, bush, 0.20f)
            .AddNeighbor(TileNeighborSide.Up, flowerYellow, 0.10f)
            .AddNeighbor(TileNeighborSide.Down, flowerPink, 0.10f);

        plains
            .RegisterGroundVariant(new BiomeGroundVariant(grassLight, weight: 0.6f, minNoise: 0.25f, maxNoise: 1.0f))
            .RegisterGroundVariant(new BiomeGroundVariant(grassDark, weight: 0.35f, minNoise: 0.0f, maxNoise: 0.75f))
            .RegisterGroundVariant(new BiomeGroundVariant(gravel, weight: 0.15f, minNoise: 0.35f, maxNoise: 0.55f))
            .RegisterPathFeature(new BiomeFeatureDefinition("plains_path", path)
            {
                SpawnChance = 0.05f,
                MaxDepth = 28,
                ReplaceExistingOnLayer = true
            })
            .RegisterDetailFeature(new BiomeFeatureDefinition("flowers", flowerYellow)
            {
                SpawnChance = 0.18f,
                MinNoise = 0.2f,
                MaxNoise = 0.95f,
                MaxDepth = 4,
                ReplaceExistingOnLayer = false
            })
            .RegisterDetailFeature(new BiomeFeatureDefinition("bush_clusters", bush)
            {
                SpawnChance = 0.08f,
                MinNoise = 0.25f,
                MaxNoise = 0.80f,
                MaxDepth = 3,
                ReplaceExistingOnLayer = false
            });

        biomes.AddBiome(plains, 0.7f);

        var grassTileHeader = Assets.GetHeader("grasstile");
        using var grassTileSource = grassTileHeader.Source;
        var grassSheet = SpriteSheet.Load(grassTileSource.Name, 64, 64);

        var grassPatternTile = new BiomeTileDefinition(
            "grasstile_pattern",
            () => new PatternSheetTile(grassSheet, columns: 2, layer: 0),
            layer: 0);

        grassPatternTile.AllowNoDetails();

        Biome idk = new Biome(Assets.Load<Sprite>("grasstile"))
            .ClearGroundVariants()
            .RegisterGroundVariant(new BiomeGroundVariant(grassPatternTile, weight: 1f));
        biomes.AddBiome(idk, 0.3f);

        TileMap map = tilemap.AddComponent<TileMap>(biomes);

        return world;
    }
}
