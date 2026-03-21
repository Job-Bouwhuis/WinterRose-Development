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

        TileMap tiles = world.CreateEntity<TileMap>("Map");
        tiles.Biomes.AddBiome(new Biome(Assets.Load<Sprite>("grassTile")), 1);
        tiles.Initialize(32);

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


        return world;
    }
}
