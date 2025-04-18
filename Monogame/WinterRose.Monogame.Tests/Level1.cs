using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using WinterRose.Monogame.StatusSystem;
using WinterRose.Monogame.Tests;
using WinterRose.Monogame.Weapons;
using WinterRose.Monogame.Worlds;
using WinterRose.WinterForge;

namespace WinterRose.Monogame.DamageSystem.Tests;

[WorldTemplate]
internal class Level1 : WorldTemplate
{
    public override void Build(in World world)
    {
        world.Name = "Level 1";
        WorldGrid.ChunkSize = 8;

        var player = world.CreateObject<SpriteRenderer>("Player", 10, 10, Color.Red);
        player.Origin = new(0.5f, 0.5f);
        player.AttachComponent<TopDownPlayerController>();

        player.AttachComponent<BallSpawner>(Sprite.Circle(10, Color.Cyan));

        world.CreateObject<SmoothCameraFollow>("cam", player.transform);

        Application.Current.CameraIndex = 0;

        ObjectSerializer serializer = new();
        serializer.SerializeToFile(world, "SavedWorld.txt");

        List<Instruction> instructions;
        using (Stream reader = File.OpenRead("SavedWorld.txt"))
        using (Stream opcodes = new FileStream("SavedWorldCodes.txt", FileMode.Create, FileAccess.ReadWrite))
        {
            new HumanReadableParser().Parse(reader, opcodes);
            instructions = InstructionParser.ParseInstructions(opcodes);
        }
    }
}
