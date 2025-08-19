using PuppeteerSharp;
using Raylib_cs;
using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.DialogBoxes;
using WinterRose.ForgeWarden.DialogBoxes.Boxes;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Physics;
using WinterRose.ForgeWarden.Resources;
using WinterRose.ForgeWarden.Shaders;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Windowing;
using WinterRose.ForgeWarden.Worlds;
using static Raylib_cs.Raylib;

namespace WinterRose.ForgeWarden;

public abstract class Application
{
    public static Application Current { get; private set; }

    public static WinterRose.Vectors.Vector2I ScreenSize => new(SCREEN_WIDTH, SCREEN_HEIGHT);

    // for on PC
    const int SCREEN_WIDTH = 1920;
    const int SCREEN_HEIGHT = 1080;
    private readonly bool useBrowser;
    private Exception? capturedException = null;
    internal bool AllowThrow = false;
    // for on laptop
    //const int SCREEN_WIDTH = 1280;
    //const int SCREEN_HEIGHT = 720;

    // for on steam deck
    //const int SCREEN_WIDTH = 960;
    //const int SCREEN_HEIGHT = 540;

    public Application(bool UseBrowser = true)
    {
        if (Current is not null)
            throw new InvalidOperationException("Application instance already exists. Only one instance is allowed.");
        Current = this;
        useBrowser = UseBrowser;
    }

    internal volatile IBrowser browser;

    public abstract World CreateWorld();

    public void Run()
    {
        if (!BulletPhysicsLoader.TryLoadBulletSharp())
            return;

        Task browserTask;
        if(useBrowser)
            browserTask = SetupEmbeddedBrowser();
        else 
            browserTask = Task.CompletedTask;
        Assets.BuildAssetIndexes();

        SetTargetFPS(144);

        Window window = new Window(SCREEN_WIDTH, SCREEN_HEIGHT, "FrostWarden - Sprite Stress Test");

        // wait with creating the window until the embedded browser is set up

        window.Create();

        SetExitKey(KeyboardKey.Null);

        if (!browserTask.IsCompleted)
        {
            DefaultDialog dialog = new("Loading embedded browser...", "This may take a while",
           DialogPlacement.CenterSmall, DialogPriority.EngineNotifications);
            Dialogs.Show(dialog);
            while (!browserTask.IsCompleted)
            {
                BeginDrawing();
                Time.Update();
                ClearBackground(Color.Black);
                Dialogs.Update(Time.deltaTime);
                Dialogs.Draw();
                DrawFPS(10, 10);
                EndDrawing();
                Task.Delay(16).Wait();
            }
            dialog.Close();
            if (browserTask.IsFaulted)
            {
                var ex = browserTask.Exception.InnerExceptions.FirstOrDefault();
                if (ex != null)
                {
                    HandleException(null, ex, ExceptionDispatchInfo.Capture(ex));
                }
                
            }
        }

        BeginDrawing();
        ClearBackground(Color.Black);
        EndDrawing();

        World world;
        Universe.CurrentWorld = world = CreateWorld();
        Camera? camera = world.GetAll<Camera>().FirstOrDefault();
        Camera.main = camera;
        RenderTexture2D worldTex = Raylib.LoadRenderTexture(SCREEN_WIDTH, SCREEN_HEIGHT);

        while (!window.ShouldClose())
        {
            Input.Update();
            Time.Update();

            //try
            //{
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
                ray.EndDrawing();
                ray.DrawFPS(10, 10);
                
            //}
            //catch (Exception ex)
            //{
              //  throw;
              //  capturedException = ex;
              //  HandleException(worldTex, ex, ExceptionDispatchInfo.Capture(ex));
              //  break;
            //}
        }

        Console.WriteLine("INFO: Releasing all resources");

        SpriteCache.DisposeAll();
        browser.Dispose();

        Console.WriteLine("INFO: All resources released, Closing window");
        window.Close();
        Console.WriteLine("INFO: Everything cleared up, Bye bye!");
    }

    public virtual void Update() { }
    public virtual void Draw() { }
    private Task SetupEmbeddedBrowser()
    {
        return Task.Run(async () =>
        {
            try
            {
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                if (OperatingSystem.IsWindows())
                    Windows.ApplicationExit += () => browser.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to initialize embedded browser: {ex.Message}");
            }
        });
    }

    private void HandleException(RenderTexture2D? worldTex, Exception ex, ExceptionDispatchInfo info)
    {
        try
        {
            ray.EndDrawing();
            Dialogs.CloseAll(true);
            Dialogs.Show(new ExceptionDialog(ex, info));
            while (Dialogs.OpenDialogs > 0)
            {
                if (ray.WindowShouldClose())
                    break;

                Time.Update();
                Dialogs.Update(Time.deltaTime);

                ray.BeginDrawing();
                ray.ClearBackground(Color.Black);

                if (Dialogs.GetActiveDialogs().FirstOrDefault() is ExceptionDialog exDialog)
                {
                    if (exDialog.IsClosing)
                    {
                        var text = RichText.Parse("An error occurred, the application will now close.", Color.Red, 50);
                        var size = text.CalculateBounds(SCREEN_WIDTH - 20);

                        RichTextRenderer.DrawRichText(
                            text,
                            new Vector2(
                                (SCREEN_WIDTH - size.Width) / 2,
                                (SCREEN_HEIGHT - size.Height) / 2),
                            SCREEN_WIDTH - 20);
                    }
                    else if(worldTex is not null)
                    {
                        Raylib.DrawTexturePro(
                             worldTex.Value.Texture,
                             new Rectangle(0, 0, worldTex.Value.Texture.Width, -worldTex.Value.Texture.Height),  // src rectangle flipped Y
                             new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT),                      // dest rectangle fullscreen
                             Vector2.Zero,
                             0,
                             Color.White);
                    }
                }

                Dialogs.Draw();
                ray.EndDrawing();
            }
        }
        catch (Exception innerEx)
        {
            SpriteCache.DisposeAll();
            browser.Dispose();

            if (AllowThrow)
                throw;

            // deal with the inner exception that not even the exception dialog could handle.
            // preferably on windows a message box.
            // but its a cross-platform engine, so we need a multi platform solution,
            // that picks the best available option for the platform.
        }

    }
}
