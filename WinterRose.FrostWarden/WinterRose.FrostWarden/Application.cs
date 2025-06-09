using Raylib_cs;
using System.Numerics;
using System.Reflection;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
using WinterRose.FrostWarden.Worlds;
using static Raylib_cs.Raylib;

namespace WinterRose.FrostWarden;

public abstract class Application
{
    public static WinterRose.Vectors.Vector2I ScreenSize => new(SCREEN_WIDTH, SCREEN_HEIGHT);

    const int SCREEN_WIDTH = 960;
    const int SCREEN_HEIGHT = 540;


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

            ui.Begin(new(100, 100));
            ui.Label("This is some fancy label");
            if (ui.Button("this is a button"))
            {
                Console.WriteLine("clicked");
            }

            b = ui.Checkbox("want this?", b);
            ui.Slider("a slider", f, 0, 1);
            BeginMode2D(camera.Camera2D);

            List<RichTextRenderer.RichChar> text = RichTextRenderer.ParseRichText("Hello rainbow world!", Color.White);

// Just tint a few characters for fun
            text[6].Color = Color.Red;
            text[7].Color = Color.Orange;
            text[8].Color = Color.Yellow;
            text[9].Color = Color.Green;
            text[10].Color = Color.Blue;

            RichTextRenderer.DrawRichText(text, new Vector2(50, 100), null, 24, 400);


            world.Draw(camera.ViewMatrix);
            EndMode2D();

            DialogBox.Draw();

            DrawFPS(10, 10);
            EndDrawing();
        }


        CloseWindow();
    }
}