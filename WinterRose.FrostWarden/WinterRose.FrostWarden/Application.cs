using Raylib_cs;
using System.Numerics;
using System.Reflection;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
using WinterRose.FrostWarden.TextRendering;
using WinterRose.FrostWarden.Worlds;
using static Raylib_cs.Raylib;

namespace WinterRose.FrostWarden;

public abstract class Application
{
    public static WinterRose.Vectors.Vector2I ScreenSize => new(SCREEN_WIDTH, SCREEN_HEIGHT);

    const int SCREEN_WIDTH = 1280;
    const int SCREEN_HEIGHT = 720;

    //const int SCREEN_WIDTH = 960;
    //const int SCREEN_HEIGHT = 540;


    public abstract World CreateWorld();

    public unsafe void Run()
    {
        InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "FrostWarden - Sprite Stress Test");
        World world = CreateWorld();

        SetTargetFPS(144);

        UIContext ui = new();

        Camera camera = world.GetAll<Camera>().FirstOrDefault();
        bool b = false;
        float f = 0;
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
            DialogBox.Update(Time.deltaTime);

            BeginDrawing();
            BeginMode2D(camera.Camera2D);

            world.Draw(camera.ViewMatrix);

            EndMode2D();

            DialogBox.Draw();

            //DrawFPS(10, 10);
            EndDrawing();
        }


        CloseWindow();
    }
}