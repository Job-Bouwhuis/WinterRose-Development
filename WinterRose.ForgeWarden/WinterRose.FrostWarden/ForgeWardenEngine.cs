using PuppeteerSharp;
using Raylib_cs;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WinterRose.ForgeSignal;
using WinterRose.ForgeThread;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.Editor;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Physics;
using WinterRose.ForgeWarden.Resources;
using WinterRose.ForgeWarden.Shaders;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Windowing;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.Recordium;
using static Raylib_cs.Raylib;

namespace WinterRose.ForgeWarden;

public abstract class ForgeWardenEngine
{
    public static ForgeWardenEngine Current { get; private set; }

    public static Font DefaultFont { get; set; }

    public const string ENGINE_POOL_NAME = "EnginePool";
    protected Log log { get; private set; }

    /// <summary>
    /// Called when the game is about to close either gracefully or by unhandled exception
    /// </summary>
    public MulticastVoidInvocation GameClosing { get; } = new();

    public ThreadLoom GlobalThreadLoom { get; } = new();

    public bool ShowFPS { get; set; }
    public Window Window { get; private set; }
    /// <summary>
    /// True when the engine is in the process of finishing up and close
    /// </summary>
    public bool GameIsClosing { get; private set; }

    private Color clearColor = Color.LightGray;

    /// <summary>
    /// Get or set the clear color of the graphics window. If started as transparent window, this will always be <see cref="Color.Blank"/>
    /// </summary>
    public Color ClearColor
    {
        get
        {
            if (Window.ConfigFlags.HasFlag(ConfigFlags.TransparentWindow)) return new Color(0, 0, 0, 0);
            return clearColor;
            

        }
        set
        {
            clearColor = value;
        }
    }

    private readonly bool useBrowser;
    private readonly bool gracefulErrorHandling;
    internal bool AllowThrow = false;

    private InputContext EngineLevelInput = new(new RaylibInputProvider(), int.MaxValue)
    {
        HasKeyboardFocus = true,
        HasMouseFocus = true
    };

    /// <summary>
    /// Highest priority input. use only when really necessary
    /// </summary>
    public InputContext GlobalInput => EngineLevelInput;
    private List<Action> debugDraws = [];

    static ForgeWardenEngine()
    {
        LogDestinations.AddDestination(new ConsoleLogDestination());
        LogDestinations.AddDestination(new FileLogDestination("logs"));
        RaylibLog.Setup();

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Current!.GameClosing.Invoke();
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            Current!.GameClosing.Invoke();
        };


    }

    public ForgeWardenEngine(bool UseBrowser = true, bool GracefulErrorHandling =
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
        log = new Log("Engine");

        GlobalThreadLoom.RegisterMainThread();
        GlobalThreadLoom.CreatePool(ENGINE_POOL_NAME, Environment.ProcessorCount / 2);
    }

    /// <summary>
    /// The world that the engine will start up with, 
    /// </summary>
    /// <returns></returns>
    public abstract World CreateFirstWorld();

    public void Run(string title, int width, int height, ConfigFlags flags = ConfigFlags.ResizableWindow)
    {
        try
        {
            if (!BulletPhysicsLoader.TryLoadBulletSharp())
                return;

            Task browserTask;
            if (useBrowser)
                browserTask = SetupEmbeddedBrowser();
            else
                browserTask = Task.CompletedTask;
            Assets.BuildAssetIndexes();

            Raylib.InitAudioDevice();

            SetTargetFPS(0);

            Window = new Window(title, flags);
            Window.Create(width, height);

            SetExitKey(KeyboardKey.Null);

            LoadDefaultFont();

            if (!browserTask.IsCompleted)
            {
                Toast t = null;
                Toasts.ShowToast(
                    t = new Toast(ToastType.Info, ToastRegion.Left, ToastStackSide.Top)
                        .AddText("Browser is being downloaded", UIFontSizePreset.Title)
                        .AddText("This can take a while.\nhowever the game is still playable", UIFontSizePreset.Text)
                    .AddProgressBar(-1, pref =>
                    {
                    if (browserTask.IsCompleted)
                        t!.Close();
                    return browserTask.IsCompleted ? 1 : -1;
                }, infiniteSpinText: "Waiting for browser download..."))
                    .ContinueWith(t => browserTask.IsCompletedSuccessfully ? 0 : 1,
                    new Toast(ToastType.Success, ToastRegion.Left, ToastStackSide.Top)
                        .AddText("Browser Successfully Downloaded!"),
                    new Toast(ToastType.Error, ToastRegion.Left, ToastStackSide.Top)
                        .AddText("Browser download failed", UIFontSizePreset.Text));
            }

            BeginDrawing();
            ClearBackground(ClearColor);
            EndDrawing();

            Universe.CurrentWorld = CreateFirstWorld();
            Camera? camera = Universe.CurrentWorld.GetAll<Camera>().FirstOrDefault();
            Camera.main = camera;
            RenderTexture2D worldTex = Raylib.LoadRenderTexture(Window.Width, Window.Height);

            while (!Window.ShouldClose() && !GameIsClosing)
            {
                InputManager.Update();
                Time.Update();
                GlobalHotkey.Update();
                GlobalThreadLoom.ProcessPendingActions(maxItems: 10);

                if (ray.IsWindowResized())
                {
                    ray.UnloadRenderTexture(worldTex);
                    worldTex = Raylib.LoadRenderTexture(Window.Width, Window.Height);
                }

                if (gracefulErrorHandling)
                {
                    try
                    {
                        MainApplicationLoop(camera, ref worldTex);
                    }
                    catch (Exception ex)
                    {
                        HandleException(worldTex, ex, ExceptionDispatchInfo.Capture(ex));
                        break;
                    }
                }
                else
                {
                    MainApplicationLoop(camera, ref worldTex);
                }
            }

            log.Info("Releasing all resources");

            Closing();
            SpriteCache.DisposeAll();
            Raylib.UnloadRenderTexture(worldTex);
            Universe.CurrentWorld.Dispose();
            Raylib.CloseAudioDevice();
            Assets.FinalizeHeaders();

            log.Info("All resources released, Closing window");
        }
        finally
        {
            try
            {
                Assets.FinalizeHeaders();
                Window.Close();
            } 
            catch { /* ignore */ }

            log.Info("End of 'run', Bye bye!");
        }
    }

    private void LoadDefaultFont()
    {
        Font f = Assets.Load<Font>("WinterRose.ForgeWarden.CascadiaCode");
        if (f.GlyphCount > 0)
            DefaultFont = f;
        else
            DefaultFont = ray.GetFontDefault();
    }

    private void MainApplicationLoop(Camera? camera, ref RenderTexture2D worldTex)
    {
        if (!Window.ConfigFlags.HasFlag(ConfigFlags.TransparentWindow))
            Universe.CurrentWorld.Update();

        Universe.Hirarchy.UpdateHirarchy();

        WindowManager.Update();
        Dialogs.Update(Time.deltaTime);
        ToastToDialogMorpher.Update();
        Toasts.Update(Time.deltaTime);
        Update();

        BeginDrawing();
        Raylib.ClearBackground(ClearColor);
        Raylib.DrawRectangle(0, 0, Window.Width, Window.Height, new Color(0, 0, 0, 1));

        ray.BeginBlendMode(BlendMode.Alpha);


        if (!Window.ConfigFlags.HasFlag(ConfigFlags.TransparentWindow))
        {
            Raylib.BeginTextureMode(worldTex);
            Raylib.ClearBackground(ClearColor);
            Raylib.DrawRectangle(0, 0, Window.Width, Window.Height, new Color(0, 0, 0, 1));

            if (camera != null)
            {
                if (camera.is3D)
                    Raylib.BeginMode3D(camera.Camera3D);
                else
                    Raylib.BeginMode2D(camera.Camera2D);
            }

            Universe.CurrentWorld.Draw(camera?.ViewMatrix ?? Matrix4x4.Identity);

            if (camera != null)
            {
                if (camera.is3D)
                    Raylib.EndMode3D();
                else
                    Raylib.EndMode2D();
            }

            Raylib.EndTextureMode();

            Raylib.DrawTexturePro(
                worldTex.Texture,
                new Rectangle(0, 0, worldTex.Texture.Width, -worldTex.Texture.Height),
                new Rectangle(0, 0, Window.Width, Window.Height),
                Vector2.Zero,
                0,
                Color.White);
        }

        WindowManager.Draw();
        Dialogs.Draw();
        ToastToDialogMorpher.Draw();
        Toasts.Draw();
        Draw();

        for (int i = 0; i < debugDraws.Count; i++)
        {
            Action? dd = debugDraws[i];
            dd();
        }

        debugDraws.Clear();

        if (ShowFPS)
            ray.DrawFPS(10, 10);
        ray.EndBlendMode();
        ray.EndDrawing();
    }

    public virtual void Update() { }
    public virtual void Draw() { }
    public virtual void Closing() { }

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
                log.Error($"Failed to initialize embedded browser: {ex.Message}");
            }
        });
    }

    private void HandleException(RenderTexture2D? worldTex, Exception ex, ExceptionDispatchInfo info)
    {
        try
        {
            log.Critical(ex, "Unhandled exception during execution of main engine loop");


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
                ray.ClearBackground(ClearColor);

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
                             new Rectangle(0, 0, worldTex.Value.Texture.Width, -worldTex.Value.Texture.Height),
                             new Rectangle(0, 0, Window.Width, Window.Height),
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

    public void Close()
    {
        GameIsClosing = true;
    }

    /// <summary>
    /// Runs the game engine as UI only over the entire screen. Disables the entire game ECS <br></br>
    /// so no <see cref="World"/>, 
    /// or <see cref="Entity"/>. <br></br>
    /// only handles <see cref="Toast"/>
    /// <see cref="Dialog"/>, 
    /// and <see cref="UIWindow"/>
    /// </summary>
    [SupportedOSPlatform("windows")]
    protected void RunAsOverlay(string title = "Overlay", int monitorIndex = 0)
    {
        var monitorsize = Windows.GetScreenSize(monitorIndex);
        Run(title, monitorsize.X, monitorsize.Y,
            ConfigFlags.TransparentWindow | 
            ConfigFlags.UndecoratedWindow | 
            ConfigFlags.BorderlessWindowMode | 
            ConfigFlags.TopmostWindow | 
            ConfigFlags.AlwaysRunWindow
            );
    }

    internal void AddDebugDraw(Action value) => debugDraws.Add(value);
}
