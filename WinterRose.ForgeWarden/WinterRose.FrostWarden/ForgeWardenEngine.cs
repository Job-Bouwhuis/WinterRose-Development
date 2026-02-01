using PuppeteerSharp;
using Raylib_cs;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WinterRose.EventBusses;
using WinterRose.ForgeThread;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.Editor;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Geometry.Rendering;
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
    protected bool WindowClosedButRunning { get; private set; } = false;
    private const int BACKGROUND_UPS = 10; // updates per second while window is closed
    private readonly System.Diagnostics.Stopwatch backgroundTimer = new();
    private long lastBackgroundTickMs = 0;

    // store initial window params so we can recreate the window later
    private string savedWindowTitle = "";
    private int savedWindowWidth = 0;
    private int savedWindowHeight = 0;
    private ConfigFlags savedWindowConfigFlags = ConfigFlags.ResizableWindow;

    public static ForgeWardenEngine Current { get; private set; }

    public static Font DefaultFont { get; set; }

    public ShapeRenderer ShapeRenderer { get; } = new(new ShapeAnimationSystem());

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
    public InputContext Input => EngineLevelInput;
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
        Exception? exception = null;
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

            // create window and remember params so we can recreate later
            Window = new Window(title, flags);
            Window.Create(width, height);

            // save for reopen
            savedWindowTitle = title;
            savedWindowWidth = width;
            savedWindowHeight = height;
            savedWindowConfigFlags = flags;

            SetExitKey(KeyboardKey.Null);

            LoadDefaultFont();

            if (!browserTask.IsCompleted)
            {
                Toast t = null;
                Toasts.ShowToast(
                        t = new Toast(ToastType.Info, ToastRegion.Left, ToastStackSide.Top)
                            .AddText("Browser is being downloaded", UIFontSizePreset.Title)
                            .AddText("This can take a while.\nhowever the game is still playable",
                                UIFontSizePreset.Text)
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

            // ----- MAIN ENGINE LOOP -----
            while (!GameIsClosing)
            {
                // if window was closed via OS/user, enter background update mode instead of terminating the engine
                if (Window != null && Window.ShouldClose())
                    GameIsClosing = true;

                // background mode: only keep Time.Update() and the abstract Update() running at BACKGROUND_UPS
                if (WindowClosedButRunning)
                {
                    long nowMs = backgroundTimer.ElapsedMilliseconds;
                    if (nowMs - lastBackgroundTickMs >= (1000 / BACKGROUND_UPS))
                    {
                        Time.Update(); // Time.deltaTime will reflect the elapsed time between these calls
                        Update(); // only engine-level update callback runs while backgrounded
                        lastBackgroundTickMs = nowMs;
                    }

                    // light sleep to avoid burning CPU while waiting for next tick or a call to ReopenWindowFull()
                    Thread.Sleep(1);
                    continue;
                }

                // Regular foreground frame (unchanged pipeline)
                InputManager.Update();
                Time.Update();
                GlobalHotkey.Update();
                GlobalThreadLoom.TickThread(maxItems: 10);

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

                // If loop naturally exits due to GameIsClosing set elsewhere, we'll fall out and finish cleanup
                if (GameIsClosing)
                    break;
            }

            Raylib.UnloadRenderTexture(worldTex);

        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            try
            {
                log.Info("Releasing all resources");

                string ex = exception is null ? "" : $"Error of type {exception.GetType().Name} causing shutdown:";

                DrawBlackScreenWithCenteredText("Unloading world...", ex);
                Universe.CurrentWorld.Dispose();
                DrawBlackScreenWithCenteredText("Clearing sprites...", ex);
                SpriteCache.DisposeAll();
                DrawBlackScreenWithCenteredText("Disposing hardware connections...", ex);

                Raylib.CloseAudioDevice();
                DrawBlackScreenWithCenteredText("Finalizing Asset Headers..", ex);
                Assets.FinalizeHeaders();
                DrawBlackScreenWithCenteredText("Bye Bye!", ex);
                Task.Delay(exception is null ? 250 : 2000).Wait();

                Closing();

                log.Info("All resources released, Closing window");

                if (OperatingSystem.IsWindows())
                {
                    WinterRose.Windows.MyHandle.Minimize();
                    Task.Delay(250).Wait();
                }
                Window.Close();
            }
            catch  (Exception ex)
            {
                log.Warning($"Exception of type {ex.GetType().Name} during unloading: {ex.Message}");
            }

            log.Info("End of 'run', Bye bye!");
        }
    }

    public static void DrawBlackScreenWithCenteredText(params string[] lines)
    {
        Raylib.BeginDrawing();

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        Raylib.ClearBackground(Color.Black);

        // Measure total height
        float lineSpacing = 1; // space between lines
        float totalHeight = 0;
        Vector2[] sizes = new Vector2[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            sizes[i] = Raylib.MeasureTextEx(DefaultFont, lines[i], 18, lineSpacing);
            totalHeight += sizes[i].Y;
        }

        // Add spacing between lines
        totalHeight += lineSpacing * (lines.Length - 1);

        // Start Y so the block is vertically centered
        float startY = (screenHeight - totalHeight) / 2;

        for (int i = 0; i < lines.Length; i++)
        {
            float posX = (screenWidth - sizes[i].X) / 2;
            Raylib.DrawTextEx(DefaultFont, lines[i], new Vector2(posX, startY), 18, lineSpacing, Color.White);
            startY += sizes[i].Y + lineSpacing;
        }

        Raylib.EndDrawing();
    }




    public void CloseWindowToBackground()
    {
        if (WindowClosedButRunning)
            return;
        log.Info("Closing window to background mode.");

        // save current window params for later recreation
        savedWindowTitle = Window.Title;
        savedWindowWidth = Window.Width;
        savedWindowHeight = Window.Height;
        savedWindowConfigFlags = Window.ConfigFlags;

        // close window but keep engine running
        Window.Close();
        WindowClosedButRunning = true;

        // start background timer
        backgroundTimer.Restart();
        lastBackgroundTickMs = backgroundTimer.ElapsedMilliseconds;

    }

    public void ReopenWindowFull()
    {
        if (!WindowClosedButRunning)
            return;

        log.Info("Reopening window from background mode.");

        // recreate window using saved params
        try
        {
            Window = new Window(savedWindowTitle, savedWindowConfigFlags);
            Window.Create(savedWindowWidth, savedWindowHeight);

            // reset background flags and timers
            WindowClosedButRunning = false;
            backgroundTimer.Reset();
            lastBackgroundTickMs = 0;

            // perform an initial clear so the first frame is clean
            BeginDrawing();
            ClearBackground(ClearColor);
            EndDrawing();
        }
        catch (Exception ex)
        {
            log.Error($"Failed to reopen window: {ex.Message}");
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
        ShapeRenderer.Update();
        Update();

        BeginDrawing();
        ShapeRenderer.Begin();
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
        ShapeRenderer.Draw();
        Draw();

        for (int i = 0; i < debugDraws.Count; i++)
        {
            Action? dd = debugDraws[i];
            dd();
        }

        debugDraws.Clear();

        if (ShowFPS)
            ray.DrawFPS(10, 10);

        ShapeRenderer.End();
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
