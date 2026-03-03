using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.HealthSystem;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.FrostWarden;

namespace VerdantRequiem.Scripts.Player;

internal static class PlayerFactory
{
    public static Entity CreatePlayer(World world)
    {
        if(world.FindEntityWithName("Player") is Entity existingPlayer)
            return existingPlayer;

        Entity player = world.CreateEntity("Player");
        player.AddComponent(new CharacterController());
        player.AddComponent(new Vitality());

        // temporary sprite for the player until theres an actual sprite
        player.AddComponent<SpriteRenderer>(Sprite.CreateRectangle(32, 32, Color.Green));

        Entity weaponMount = world.CreateEntity("WeaponMount");
        weaponMount.transform.parent = player.transform;
        weaponMount.Tags = ["WeaponMount"];

        return player;
    }
}
