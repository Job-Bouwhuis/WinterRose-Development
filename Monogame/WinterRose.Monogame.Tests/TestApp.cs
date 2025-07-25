﻿using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.IO;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem.Tests;
using WinterRose.Monogame.TerrainGeneration;
using WinterRose.Monogame.Tests;
using WinterRose.Monogame.Tests.Scripts;
using WinterRose.Monogame.UI;
using WinterRose.Monogame.Worlds;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

internal class TestApp : Application
{
    protected override World CreateWorld()
    {
        if (WinterRose.Windows.GetScreenSize().x >= 2560)
            MonoUtils.WindowResolution = new(1920, 1080);
        else
            MonoUtils.WindowResolution = new(1280, 720);

        Hirarchy.Show = true;

        World.FromTemplate<Level1>(); // to resave
        using (Stream opcodes = new FileStream("SavedWorldCodes.txt", FileMode.Open, FileAccess.ReadWrite))
        {
           
            var instructions = InstructionParser.ParseOpcodes(opcodes);
            World ww = (World)new InstructionExecutor().Execute(instructions);
            return ww;
        }

        return World.FromTemplate<Level1>();

        World w = new World("simworld");
        CameraIndex = 0;
        {
            var player = w.CreateObject("player");
            player.AttachComponent<SpriteRenderer>(50, 50, Color.Yellow);
            player.AttachComponent<TopDownPlayerController>().transform.position = (Vector2)MonoUtils.ScreenCenter;
            player.AttachComponent<InputRotator>();

            w.CreateObject<SmoothCameraFollow>("cam", player.transform);

            var wall = w.CreateObject<SpriteRenderer>("button", 25, 25, Color.Blue).owner;
            wall.transform.parent = player.transform;

            wall.transform.localPosition = new Vector2(0, 50);

            return w;
        }

        {
            TerrainMap map = new(MonoUtils.Graphics, 3, 3, 1441667, 0.02f, 8, 0.002f, new());
            w.CreateObject<MapRenderer>("map", map);

            var player = w.CreateObject("player");
            player.AttachComponent<SpriteRenderer>(Sprite.Circle(25, Color.Magenta));
            player.AttachComponent<TopDownPlayerController>().transform.position = (Vector2)MonoUtils.ScreenCenter;

            w.CreateObject<SmoothCameraFollow>("cam", player.transform).owner.AddUpdateBehavior(obj =>
            {
                if (Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
                {
                    Camera cam = Camera.current;
                    Sprite s = cam.Screenshot;
                    s.Save("Screenshot.png");
                }
            });
        }

        return w;
        var rope = w.CreateObject<Rope>("Rope", new Vector2(400, 100), new Vector2(0, 100), 80, 5);
        rope.owner.AddUpdateBehavior(x =>
        {
            foreach (var node in rope.Nodes)
            {
                node.ApplyForce(new(0, 130));
            }
        });
        rope.AttachComponent<MouseDrag>();


        var cloth = w.CreateObject<Cloth>("Cloth", new Vector2(500, 100), 25, 25, 10);
        cloth.owner.AddUpdateBehavior(x =>
        {
            cloth.ApplyForceToAll(new(0, 130));
        });

        return w;
    }

    public static void TestPerlinNoise()
    {
        PerlinNoise noise = new PerlinNoise();
        for (int i = 0; i < 100; i++)
        {
            float value = noise.Generate(i * 0.1f); // Adjust multiplier to vary input
            Console.WriteLine($"Noise value at {i * 0.1f}: {value}");
        }
    }
}