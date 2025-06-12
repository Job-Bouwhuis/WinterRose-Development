using Raylib_cs;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.DialogBoxes;
using WinterRose.FrostWarden.Entities;
using WinterRose.FrostWarden.Physics;
using WinterRose.FrostWarden.Shaders;
using WinterRose.FrostWarden.TextRendering;
using WinterRose.FrostWarden.Windowing;
using WinterRose.FrostWarden.Worlds;
using static Raylib_cs.Raylib;
using static WinterRose.Windows;

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

    private List<Window> windows;

    public abstract World CreateWorld();

    public void Run()
    {
        if (!TryLoadBulletSharp())
            return;

        SetExitKey(KeyboardKey.Null);

        SetTargetFPS(144);

        UIContext ui = new();

        SetExitKey(KeyboardKey.Null);

        WindowHooks.RegisterHandler(WindowHooks.Messages.KeyDown, msg =>
        {
            Console.WriteLine((char)msg.WParam.ToInt32());
        });

        Window window = new Window(SCREEN_WIDTH, SCREEN_HEIGHT, "FrostWarden - Sprite Stress Test");

        window.Create();

        BeginDrawing();
        ClearBackground(Color.Black);
        DrawText("Initializing Engine...", 0, 0, 60, Color.White);
        EndDrawing();

        World world;
        Universe.CurrentWorld = world = CreateWorld();
        Camera? camera = world.GetAll<Camera>().FirstOrDefault();

        RenderTexture2D worldTex = Raylib.LoadRenderTexture(SCREEN_WIDTH, SCREEN_HEIGHT);

        while (!window.ShouldClose())
        {
            Input.Update();
            Time.Update();

            world.Update();
            Dialogs.Update(Time.deltaTime);

            BeginDrawing();

            Raylib.ClearBackground(Color.Black);
            Raylib.BeginTextureMode(worldTex);
            Raylib.ClearBackground(Color.DarkGray);

            if (camera != null)
                Raylib.BeginMode2D(camera.Camera2D);

            world.Draw(camera?.ViewMatrix ?? Matrix4x4.Identity);

            if (camera != null)
                Raylib.EndMode2D();

            Raylib.EndTextureMode();

            Raylib.DrawTexturePro(
                worldTex.Texture,
                new Rectangle(0, 0, worldTex.Texture.Width, -worldTex.Texture.Height),  // src rectangle flipped Y
                new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT),                      // dest rectangle fullscreen
                Vector2.Zero,
                0,
                Color.White);

            Dialogs.Draw();

            Raylib.EndDrawing();
        }


        window.Close();
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