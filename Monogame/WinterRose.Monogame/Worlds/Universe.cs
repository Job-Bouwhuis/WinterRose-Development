using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WinterRose.Exceptions;
using WinterRose.Monogame.Audio;
using WinterRose.Monogame.EditorMode;
using WinterRose.Monogame.HealthChecks;
using WinterRose.Monogame.Imgui;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A manger for <see cref="World"/>s
/// </summary>
public static class Universe
{
    private static bool firstStart = true;
    /// <summary>
    /// Whether a transform has moved in the last update frame. If this is false the world will redraw the last frame
    /// </summary>
    public static bool RequestRender { get; set; } = true;
    /// <summary>
    /// Deault options for rendering the world when there is no camera in the world or the camera index is -1 when calling <see cref="World.Render(SpriteBatch, int)"/><br></br>
    /// ( <see cref="Render(int)"/> calls this method )
    /// </summary>
    public static WorldRenderingOptions RenderOptions => renderOptions;
    /// <summary>
    /// When true, stops draw calls when no transform has changed and <see cref="RequestRender"/> is false.<br></br>
    /// When false, the world will always be redrawn regardless of whether a transform has changed or not
    /// </summary>
    public static bool OptimizeDrawCalls { get; set; } = false;
    /// <summary>
    /// The time it took to render the ImGui stuff
    /// </summary>
    public static TimeSpan ImGuiRenderTime { get; private set; } = new();
    /// <summary>
    /// The current loaded world
    /// </summary>
    public static World? CurrentWorld
    {
        get
        {
            return curWorld;
        }
        set
        {
            if (curWorld == value)
                return;
            if (curWorld is not null)
            {
                curWorld.Destroy();
                if (MonoUtils.Graphics is null && MonoUtils.IsStopping)
                    return;

                MonoUtils.Graphics?.SetRenderTarget(null);

                3.Repeat(x => GC.Collect());

            }
            if (!firstStart)
                AudioConsumer.Reset();

            Timer.ClearAll();

            curWorld = value;
            curWorld?.WakeWorld();
            RequestRender = true;
            OnNewWorldLoaded();
            CurrentWorld.Initialized = true;
        }
    }
    /// <summary>
    /// Invoked when a new world has been loaded
    /// </summary>
    public static Action OnNewWorldLoaded { get; internal set; } = delegate { };

    public static List<Effect> RenderEffects { get; } = [];

    private static World? curWorld;
    private static RenderTarget2D? lastFrame;
    private static RenderTarget2D? lastUIFrame;
    internal static ImGuiRenderer imGuiRenderer;
    private static WorldRenderingOptions renderOptions = new();

    static Universe()
    {
        imGuiRenderer = new ImGuiRenderer(MonoUtils.MainGame).Initialize().RebuildFontAtlas();
    }
    /// <summary>
    /// Updates The <see cref="CurrentWorld"/> and other parts such as <see cref="Input"/> and <see cref="Time"/>
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static bool Update(GameTime time)
    {
        try
        {
            Time.Update(time);
            Input.UpdateState();
            Timer.UpdateAll();
            if (Debug.ExceptionThrown) return false;

            HandleIntegrityChecks();

            if (CurrentWorld is null) return false;

            if (Editor.Opened)
            {
                Editor.Update();
            }

            CurrentWorld?.Update();
            return true;
        }
        catch (Exception ex)
        {
            if (Debug.AllowThrow) throw;
            Debug.LogException(ex);
            return false;
        }
        finally
        {
        }
    }

    private static void HandleIntegrityChecks()
    {
        if (!HealthStatusManager.CheckGameIntegrity(out var checks))
        {
            foreach (var report in checks.AllEvaluatedChecks)
            {
                if (report.Status is not HealthStatus.Healthy)
                {
                    WinterException ex = new(report.Check.Message ?? "Unknown error", report.Exception);
                    string s = report.CheckName;
                    s += ":\t" + report.Status;

                    ex.SetStackTrace(s);
                    ex.SetSource(report.CheckName);
                    Debug.LogException(ex);
                }

                if (report.Exception != null)
                {
                    Debug.LogError(report.Exception);
                }
            }
        }
    }

    /// <summary>
    /// Renders the <see cref="CurrentWorld"/> to the screen. and renders other things such as <see cref="Debug"/> and <see cref="Hirarchy"/>
    /// </summary>
    /// <param name="cameraIndex">The index in world hirargy of what camera to use when rendering, pass -1 to render using no camera</param>
    /// <returns></returns>
    public static bool Render(int cameraIndex)
    {
        try
        {
            if (Debug.ExceptionThrown) return false;

            if (CurrentWorld is null)
            {
                if (MonoUtils.DefaultFont is null)
                {
                    Debug.LogWarning("No world to render, and no font to show onscreen text. \nconsider adding a default font");
                    return false;
                }
                MonoUtils.Graphics.Clear(Color.Black);
                var batch = MonoUtils.SpriteBatch;
                RenderFrame(cameraIndex);

                return false;
            }

            if (RequestRender || !OptimizeDrawCalls)
            {
                var frames = CurrentWorld.Render(MonoUtils.SpriteBatch, cameraIndex);
                lastUIFrame?.Dispose();
                // why not dispose of LastFrame? 
                // its a reference taken from the Camera
                // if we dispose of this, the Camera cant set its render target anymore
                lastFrame = frames.world;
                lastUIFrame = frames.UI;

                RenderFrame(cameraIndex);

                RequestRender = false;
            }
            else
            {
                if (lastFrame is null)
                {
                    MonoUtils.Graphics.Clear(Color.CornflowerBlue);
                    var frames = CurrentWorld.Render(MonoUtils.SpriteBatch, cameraIndex);
                    lastFrame = frames.world;
                    lastUIFrame = frames.UI;
                }

                RenderFrame(cameraIndex);

                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
        finally
        {
            var sw = Stopwatch.StartNew();
            FinalizeRendering();
            sw.Stop();
            ImGuiRenderTime = sw.Elapsed;
        }
    }
    /// <summary>
    /// Allows you to change the default options for rendering the world when there is no camera in the world or the camera index is -1 when calling <see cref="World.Render(SpriteBatch, int)"/>
    /// </summary>
    /// <param name="options"></param>
    public static void WithRenderingOptions(Action<WorldRenderingOptions> options) => options(renderOptions);
    private static void RenderFrame(int cameraIndex)
    {
        if (lastFrame is null)
        {
            Debug.LogWarning("No frame to render");
            return;
        }

        Vector2I scale = new(1, 1);
        var cameras = CurrentWorld.FindComponents<Camera>();
        if (cameras.Length is not 0)
            if (cameraIndex != -1 && CurrentWorld is not null)
            {

                if (cameras.Length is 0)
                {
                    Debug.LogWarning("No cameras in world");
                    return;
                }

                var camera = cameras[cameraIndex];

                // calculate a vector2 that will scale the image to the window size
                scale = new(MonoUtils.WindowResolution.X / camera.Bounds.X, MonoUtils.WindowResolution.Y / camera.Bounds.Y);
            }

        var batch = MonoUtils.SpriteBatch;


        batch.Begin();
        batch.Draw(
                lastFrame, Vector2.Zero, null, Color.White, 0, new Vector2(), scale, SpriteEffects.None, 0);

        if (lastUIFrame != null)
        {
            batch.Draw(
                lastUIFrame, Vector2.Zero, null, Color.White, 0, new Vector2(), scale, SpriteEffects.None, 0);
        }

        batch.End();
    }


    private static void FinalizeRendering()
    {
        try
        {
            if (Time.time is null)
            {
                GC.Collect();
                return;
            }

            imGuiRenderer.BeginLayout(gameTime: Time.time);
            try
            {
                Debug.RenderLayout();
            }
            catch (Exception e)
            {
                if (Debug.AllowThrow)
                    throw;
                if (Debugger.IsAttached)
                {
                    var result = Windows.MessageBox("An error happened in the Debug class. " +
                        "this causes the buildin exception screen to fail. hence this message box.\n\n" +
                        $"Type: {e.GetType()}\n" +
                        $"Message: {e.Message}\n\n" +
                        "Throw this error?\n\n" +
                        "" +
                        ") Yes\n" +
                        "       The error will be thrown.\n" +
                        ") No\n" +
                        "       The game will close.", "Fatal error", Windows.MessageBoxButtons.YesNo, Windows.MessageBoxIcon.Error);

                    if (result is Windows.DialogResult.Yes)
                    {
                        Debug.AllowThrow = true;
                        throw;
                    }
                    Environment.Exit(1000);
                }
                Windows.MessageBox("A fatal error has occured that caused the game to crash. normally there is a details screen, " +
                    "but in this case this details screen has errors.\n" +
                    "Please relay the following information to the developer of the game you were playing:\n\n" +
                    "$\"Type: {e.GetType()}\n" +
                    "$\"Message: {e.Message}\n\n" +
                    "Thanks for your support. -Devs", "Fatal error", Windows.MessageBoxButtons.OK, Windows.MessageBoxIcon.Error);
                Environment.Exit(1000);
            }

            if (Debug.ExceptionThrown)
            {
                imGuiRenderer.EndLayout();
                return;
            }

            if (CurrentWorld is not null)
                for (int i = 0; i < (CurrentWorld?.Objects.Count ?? 0); i++)
                {
                    WorldObject? obj = CurrentWorld.Objects[i];
                    obj.FetchComponents().Foreach(x =>
                    {
                        if (x is ImGuiLayout lay)
                            lay.RenderLayout();
                    });
                }

            Hirarchy.RenderLayout();

            if (WorldEditor.Show)
                WorldEditor.RenderLayout();

            if (Editor.Opened)
                Editor.Render();

            imGuiRenderer.EndLayout();
        }
        catch (Exception e)
        {
            if (Debug.AllowThrow) throw;
            Debug.LogException(e);
        }
    }
    internal static void Destroy(WorldObject worldObject) => CurrentWorld?.Destroy(worldObject);
    internal static void StartNoCameraSpritebatch(SpriteBatch batch)
    {
        MonoUtils.Graphics.Clear(RenderOptions.ClearColor);
        batch.Begin(sortMode: RenderOptions.SortMode,
            blendState: RenderOptions.BlendState,
            samplerState: RenderOptions.SamplerState,
            depthStencilState: RenderOptions.DepthStencilState,
            rasterizerState: RenderOptions.RasterizerState,
            effect: RenderOptions.Effect);
    }
}