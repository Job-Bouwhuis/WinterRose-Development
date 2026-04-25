using PuppeteerSharp;
using Raylib_cs;
using System.Diagnostics;
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
using WinterRose.ForgeWarden.EngineLayers;
using WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;
using WinterRose.ForgeWarden.EngineLayers.Events;
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
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Windowing;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.Recordium;
using static Raylib_cs.Raylib;

namespace WinterRose.ForgeWarden;

public abstract class ForgeWardenEngine
{
    protected bool WindowClosedButRunning { get; private set; } = false;
    private const int BACKGROUND_UPDATES = 10;
    private readonly System.Diagnostics.Stopwatch backgroundTimer = new();
    private long lastBackgroundTickMs = 0;

    private string savedWindowTitle = "";
    private int savedWindowWidth = 0;
    private int savedWindowHeight = 0;
    private ConfigFlags savedWindowConfigFlags = ConfigFlags.ResizableWindow;

    public LayerStack LayerStack { get; private set; }

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

    public bool ShowFPS { get; set; } = false;
    public Window Window { get; private set; }
    /// <summary>
    /// True when the engine is in the process of finishing up and close
    /// </summary>
    public bool GameIsClosing { get; private set; }

    private RuntimeLayer runtimeLayer;
    private EditorLayer editorLayer;

    /// <summary>
    /// Get or set whether the editor is enabled. When enabled, the EditorLayer is active and RuntimeLayer is disabled.
    /// </summary>
    public bool EditorEnabled
    {
        get => editorLayer?.Enabled ?? false;
        set
        {
            if (editorLayer != null) editorLayer.Enabled = value;
            if (runtimeLayer != null) runtimeLayer.Enabled = !value;
        }
    }

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
    Exception? exception = null;

    private InputContext EngineLevelInput = new(new RaylibInputProvider(), int.MaxValue)
    {
        HasKeyboardFocus = true,
        HasMouseFocus = true
    };

    /// <summary>
    /// Highest priority input. use only when really necessary
    /// </summary>
    public InputContext Input => EngineLevelInput;

    public bool FancyShutdown { get; }

    internal List<Action> debugDraws = [];

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

    public ForgeWardenEngine(bool UseBrowser = true, bool fancyShutdown = true, bool GracefulErrorHandling =
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
        FancyShutdown = fancyShutdown;
        gracefulErrorHandling = GracefulErrorHandling;
        log = new Log("Engine");

        LayerStack = new LayerStack();

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

            // save for reopen
            savedWindowTitle = title;
            savedWindowWidth = width;
            savedWindowHeight = height;
            savedWindowConfigFlags = flags;

            SetExitKey(KeyboardKey.Null);

            LoadDefaultFont();


            LayerStack.AddLayer(new EngineCoreLayer());
            LayerStack.AddLayer(new RenderLayer());
            LayerStack.AddLayer(new WorldRenderLayer());
            LayerStack.AddLayer(new UiLayer());
            runtimeLayer = new RuntimeLayer();
            editorLayer = new EditorLayer();
            LayerStack.AddLayer(runtimeLayer);
            LayerStack.AddLayer(editorLayer);

            editorLayer.Enabled = false;

            AfterWindowCreation();

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


            while (!Raylib.WindowShouldClose() && !GameIsClosing)
            {
                LayerStack.Update();

                Raylib.BeginDrawing();
                ClearBackground(ClearColor);
                LayerStack.Render();
                if (ShowFPS)
                    Raylib.DrawText($"FPS: {ray.GetFPS()} - Delta: {Time.deltaTime}", 10, 10, 18, Color.Magenta);
                Raylib.EndDrawing();
            }
            log.Info("Starting world disposal");
            Universe.CurrentWorld.Dispose();
            log.Info($"Destroying world took {Universe.CurrentWorld._Entities.Sum(e => e.GetAllComponents().Sum(c => c.destroyTime))}ms");

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
                Closing();

                log.Info("Releasing all resources");

                string ex = exception is null ? "" : $"Error of type {exception.GetType().Name} causing shutdown!";

                if (FancyShutdown)
                    DrawBlackScreenWithCenteredText("Unloading world...", ex);
                Universe.CurrentWorld.Dispose();
                if (FancyShutdown)
                    DrawBlackScreenWithCenteredText("Cleaning sprites...", ex);
                SpriteCache.DisposeAll();

                if (FancyShutdown)
                    DrawBlackScreenWithCenteredText("Disposing hardware connections...", ex);
                Raylib.CloseAudioDevice();
                if (FancyShutdown)
                    DrawBlackScreenWithCenteredText("Finalizing Asset Headers..", ex);
                Assets.FinalizeHeaders();
                if (FancyShutdown)
                    DrawBlackScreenWithCenteredText("Bye Bye!", ex);
                Task.Delay(exception is null ? 250 : 2000).Wait();

                log.Info("All resources released, Closing window");

                if (OperatingSystem.IsWindows())
                {
                    WinterRose.Windows.MyHandle.Minimize();
                    Task.Delay(250).Wait();
                }
                Window.Close();
            }
            catch (Exception ex)
            {
                log.Warning($"Exception of type {ex.GetType().Name} during unloading: {ex.Message}");
            }

            log.Info("End of 'run', Bye bye!");
        }
    }

    static void DrawBlackScreenWithCenteredText(params string[] lines)
    {
        Raylib.BeginDrawing();

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        Raylib.ClearBackground(Color.Black);

        float lineSpacing = 1; 
        float totalHeight = 0;
        Vector2[] sizes = new Vector2[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            sizes[i] = Raylib.MeasureTextEx(DefaultFont, lines[i], 18, lineSpacing);
            totalHeight += sizes[i].Y;
        }

        totalHeight += lineSpacing * (lines.Length - 1);

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

        savedWindowTitle = Window.Title;
        savedWindowWidth = Window.Width;
        savedWindowHeight = Window.Height;
        savedWindowConfigFlags = Window.ConfigFlags;

        Window.Close();
        WindowClosedButRunning = true;

        backgroundTimer.Restart();
        lastBackgroundTickMs = backgroundTimer.ElapsedMilliseconds;

    }

    public void ReopenWindowFull()
    {
        if (!WindowClosedButRunning)
            return;

        log.Info("Reopening window from background mode.");

        try
        {
            Window = new Window(savedWindowTitle, savedWindowConfigFlags);
            Window.Create(savedWindowWidth, savedWindowHeight);

            WindowClosedButRunning = false;
            backgroundTimer.Reset();
            lastBackgroundTickMs = 0;

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

    public virtual void AfterWindowCreation() { }
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
