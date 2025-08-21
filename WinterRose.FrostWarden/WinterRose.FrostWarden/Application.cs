using PuppeteerSharp;
using Raylib_cs;
using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Physics;
using WinterRose.ForgeWarden.Resources;
using WinterRose.ForgeWarden.Shaders;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.Windowing;
using WinterRose.ForgeWarden.Worlds;
using static Raylib_cs.Raylib;

namespace WinterRose.ForgeWarden;

public abstract class Application
{
    public static Application Current { get; private set; }

    public bool ShowFPS { get; set; }
    public Window Window { get; private set; }

    // for on PC
    const int SCREEN_WIDTH = 1920;
    const int SCREEN_HEIGHT = 1080;
    private readonly bool useBrowser;
    private readonly bool gracefulErrorHandling;
    private Exception? capturedException = null;
    internal bool AllowThrow = false;

    private InputContext EngineLevelInput = new(new RaylibInputProvider(), int.MaxValue)
    {
        HasKeyboardFocus = true,
        HasMouseFocus = true
    };

    // for on laptop
    //const int SCREEN_WIDTH = 1280;
    //const int SCREEN_HEIGHT = 720;

    // for on steam deck
    //const int SCREEN_WIDTH = 960;
    //const int SCREEN_HEIGHT = 540;

    public Application(bool UseBrowser = true, bool GracefulErrorHandling =
#if RELEASE
         true) // use try catch with graceful error dialog box upon error rather than just closing without reason
#else
         false) // allow the game to throw errors so that stack trace and local variables are maintained in exception debugging
#endif
    {
        if (Current is not null)
            throw new InvalidOperationException("Application instance already exists. Only one instance is allowed.");
        Current = this;
        useBrowser = UseBrowser;
        gracefulErrorHandling = GracefulErrorHandling;
    }

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

        Window = new Window("FrostWarden - Sprite Stress Test", ConfigFlags.ResizableWindow);

        // wait with creating the window until the embedded browser is set up

        Window.Create(SCREEN_WIDTH, SCREEN_HEIGHT);

        SetExitKey(KeyboardKey.Null);

        if (!browserTask.IsCompleted)
        {
            Toasts.ShowToast(
                new Toast(ToastType.Info, ToastRegion.Left, ToastStackSide.Top)
                    .AddContent("Browser is being downloaded", ToastMessageFontPreset.Title)
                    .AddContent("This can take a while.\nhowever the game is still playable", ToastMessageFontPreset.Subtext)
                    .AddProgressBar(-1, pref => browserTask.IsCompleted ? 1 : -1, infiniteSpinText: "Waiting for browser download..."))
                .ContinueWith(t => browserTask.IsCompletedSuccessfully ? 0 : 1,
                new Toast(ToastType.Success, ToastRegion.Left, ToastStackSide.Top)
                    .AddContent("Browser Successfully Downloaded!")
                , new Toast(ToastType.Error, ToastRegion.Left, ToastStackSide.Top)
                    .AddContent("Browser download failed", ToastMessageFontPreset.Message));
        }

        BeginDrawing();
        ClearBackground(Color.Black);
        EndDrawing();

        World world;
        Universe.CurrentWorld = world = CreateWorld();
        Camera? camera = world.GetAll<Camera>().FirstOrDefault();
        Camera.main = camera;
        RenderTexture2D worldTex = Raylib.LoadRenderTexture(Window.Width, Window.Height);

        while (!Window.ShouldClose())
        {
            InputManager.Update();
            Time.Update();

            if(ray.IsWindowResized())
            {
                ray.UnloadRenderTexture(worldTex);
                worldTex = Raylib.LoadRenderTexture(Window.Width, Window.Height);
            }

            if(gracefulErrorHandling)
            {
                try
                {
                    MainApplicationLoop(world, camera, worldTex);

                }
                catch (Exception ex)
                {
                    capturedException = ex;
                    HandleException(worldTex, ex, ExceptionDispatchInfo.Capture(ex));
                    break;
                }
            }
            else
            {
                MainApplicationLoop(world, camera, worldTex);
            }
        }

        Console.WriteLine("INFO: Releasing all resources");

        SpriteCache.DisposeAll();

        Console.WriteLine("INFO: All resources released, Closing window");
        Window.Close();
        Console.WriteLine("INFO: Everything cleared up, Bye bye!");
    }

    private void MainApplicationLoop(World world, Camera? camera, RenderTexture2D worldTex)
    {
        world.Update();
        Dialogs.Update(Time.deltaTime);
        Toasts.Update(Time.deltaTime);
        ToastToDialogMorpher.Update();
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
            new Rectangle(0, 0, Window.Width, Window.Height),                      // dest rectangle fullscreen
            Vector2.Zero,
            0,
            Color.White);

        Toasts.Draw();
        ToastToDialogMorpher.Draw();
        Dialogs.Draw();
        
        if (ShowFPS)
            ray.DrawFPS(10, 10);
        ray.EndDrawing();
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
                        var size = text.CalculateBounds(Window.Width - 20);

                        RichTextRenderer.DrawRichText(
                            text,
                            new Vector2(
                                (Window.Width - size.Width) / 2,
                                (Window.Height - size.Height) / 2),
                            Window.Width - 20,
                            EngineLevelInput);
                    }
                    else if(worldTex is not null)
                    {
                        Raylib.DrawTexturePro(
                             worldTex.Value.Texture,
                             new Rectangle(0, 0, worldTex.Value.Texture.Width, -worldTex.Value.Texture.Height),  // src rectangle flipped Y
                             new Rectangle(0, 0, Window.Width, Window.Height),                      // dest rectangle fullscreen
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

            if (AllowThrow)
                throw;

            // deal with the inner exception that not even the exception dialog could handle.
            // preferably on windows a message box.
            // but its a cross-platform engine, so we need a multi platform solution,
            // that picks the best available option for the platform.
        }

    }
}
