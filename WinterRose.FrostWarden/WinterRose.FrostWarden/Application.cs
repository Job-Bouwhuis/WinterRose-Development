using Raylib_cs;
using System.Numerics;
using System.Reflection;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
using WinterRose.FrostWarden.Worlds;
using WinterRose.Vectors;
using static Raylib_cs.Raylib;

namespace WinterRose.FrostWarden;

public abstract class Application
{
    public static Vector2I ScreenSize => new Vector2I(SCREEN_WIDTH, SCREEN_HEIGHT);

    const int SCREEN_WIDTH = 1280;
    const int SCREEN_HEIGHT = 720;

    public abstract World CreateWorld();

    public unsafe void Run()
    {
        InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "FrostWarden - Sprite Stress Test");
        World world = CreateWorld();

        SetTargetFPS(144);

        Camera camera = world.GetAll<Camera>().FirstOrDefault();

        while (!WindowShouldClose())
        {
            ClearBackground(Color.DarkGray);
            if (camera is null)
            {
                BeginDrawing();
                DrawText("No camera.", 200, 200, 30, Color.Red);
                EndDrawing();
                continue;
            }

            Input.Update();
            Time.Update();
            world.Update();

            BeginDrawing();

            BeginMode2D(camera.Camera2D);
            world.Draw(camera.ViewMatrix);
            EndMode2D();

            DrawFPS(10, 10);
            EndDrawing();
        }


        CloseWindow();
    }
}
