using Raylib_cs;
using System.Numerics;
using System.Reflection;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
using WinterRose.FrostWarden.Physics;
using WinterRose.FrostWarden.TextRendering;
using WinterRose.FrostWarden.Worlds;
using static Raylib_cs.Raylib;

namespace WinterRose.FrostWarden;

public abstract class Application
{
    public static WinterRose.Vectors.Vector2I ScreenSize => new(SCREEN_WIDTH, SCREEN_HEIGHT);

    // for on PC
    const int SCREEN_WIDTH = 1920;
    const int SCREEN_HEIGHT = 1080;

    // for on laptop
    //const int SCREEN_WIDTH = 1280;
    //const int SCREEN_HEIGHT = 720;

    // for on steam deck
    //const int SCREEN_WIDTH = 960;
    //const int SCREEN_HEIGHT = 540;

    public abstract World CreateWorld();

    public unsafe void Run()
    {
        if (!TryLoadBulletSharp())
            return;

        InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "FrostWarden - Sprite Stress Test");
        World world = CreateWorld();

        SetTargetFPS(144);

        UIContext ui = new();

        Camera? camera = world.GetAll<Camera>().FirstOrDefault();

        while (!WindowShouldClose())
        {
            ClearBackground(Color.DarkGray);

            Input.Update();
            Time.Update();

            world.Update(); 
            DialogBox.Update(Time.deltaTime);

            BeginDrawing();
            if (camera is not null)
                BeginMode2D(camera.Camera2D);

            world.Draw(camera?.ViewMatrix ?? Matrix4x4.Identity);

            if (camera is not null)
                EndMode2D();

            DialogBox.Draw();
            EndDrawing();
        }

        CloseWindow();
    }

    private static bool TryLoadBulletSharp()
    {
        try
        {
            Assembly a = Assembly.Load("BulletSharp");
            string b = a.Location.Replace('\\', '/');
            return true;
        }
        catch (Exception e)
        {
            Windows.MessageBox(e.ToString(), "Error loading BulletSharp", Windows.MessageBoxButtons.OK, Windows.MessageBoxIcon.Error);
            return false;
        }
    }
}