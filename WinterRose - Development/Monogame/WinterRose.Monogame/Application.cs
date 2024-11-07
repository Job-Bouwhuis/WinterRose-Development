using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;
using WinterRose.SourceGeneration;

namespace WinterRose.Monogame;

/// <summary>
/// A class that represents a Monogame Application. Implements <see cref="Game"/>. <br><br></br></br>
/// 
/// This class helps <see cref="MonoUtils"/> to work properly and saves you from writing a bunch of setup boilerplate code. <br></br>
/// </summary>
public abstract class Application : Game, IDisposable
{
    public static Application Current { get; private set; }

    /// <summary>
    /// The camera index that will be requested to render its view to the window. <br></br>
    /// -1 means no camera will be used and the window will be rendered as is. <br></br>
    /// </summary>
    public int CameraIndex { get; set; }
    /// <summary>
    /// Creates a new instance of the application class
    /// </summary>
    public Application()
    {
        _ = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Current = this;
    }
    private bool initialized = false;

    /// <summary>
    /// The string representation of this application
    /// </summary>
    /// <returns></returns>
    public override string? ToString()
    {
        return Window.Title;
    }

    /// <summary>
    /// Called when the game is beginning to draw. <br></br>
    /// Should return true to proceed with drawing, false to skip drawing this frame. <br></br>
    /// </summary>
    /// <returns></returns>
    protected override bool BeginDraw()
    {
        return base.BeginDraw();
    }

    /// <summary>
    /// Called when the game is beginning to run. <br></br>
    /// </summary>
    protected override void BeginRun()
    {
        base.BeginRun();
    }

    /// <summary>
    /// Called when the game is disposing.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Universe.CurrentWorld = null;
    }

    /// <summary>
    /// Called when the game is drawing.
    /// </summary>
    /// <param name="gameTime"></param>
    protected override void Draw(GameTime gameTime)
    {
        if (!initialized) return;
        Universe.Render(CameraIndex);
    }
    /// <summary>
    /// Called each frame of the game
    /// </summary>
    /// <param name="gameTime"></param>
    protected override void Update(GameTime gameTime)
    {
        if (!initialized)
        {
            initialized = true;
            Universe.CurrentWorld = CreateWorld();
        }
        Universe.Update(gameTime);
    }
    /// <summary>
    /// Called at the end of the draw cycle
    /// </summary>
    protected override void EndDraw()
    {
        try
        {
            base.EndDraw();
        }
        catch (Exception e)
        {
            MonoUtils.Graphics.SetRenderTarget(null);
            try
            {
                base.EndDraw();
            }
            catch (Exception ee)
            {
                Debug.AllowThrow = true;
                throw;
            }
        }
    }
    /// <summary>
    /// Called at the end of the run cycle
    /// </summary>
    protected override void EndRun()
    {
        base.EndRun();
    }

    /// <summary>
    /// If you override this make sure to call 'base.Initialize()' before any of your code.
    /// </summary>
    protected override void Initialize()
    {
        MonoUtils.Initialize(this, new SpriteBatch(GraphicsDevice), "Font");

        MonoUtils.AutoUserdomainUpdate = true;
        if (MonoUtils.DefaultFont is not null)
            MonoUtils.DefaultFont.DefaultCharacter = '#';
        MonoUtils.WindowResolution = new(1280, 720);
        WorldEditor.Show = false;
        ExitHelper.SetCloseMethod(Exit);

        Style.ApplyDefault();
        base.Initialize();
    }

    /// <summary>
    /// A method that will be called once the application is fully loaded and ready to run.
    /// <br></br> use it to create or load your world.
    /// </summary>
    protected abstract World CreateWorld();

    /// <summary>
    /// Called when the game is loading content.
    /// </summary>
    protected override void LoadContent()
    {
        base.LoadContent();
    }

    /// <summary>
    /// Called when the game is gaining focus.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected override void OnActivated(object sender, EventArgs args)
    {
        MonoUtils.Activated();
        base.OnActivated(sender, args);
    }

    /// <summary>
    /// Called when the game is losing focus
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected override void OnDeactivated(object sender, EventArgs args)
    {
        MonoUtils.Deactivated();
        base.OnDeactivated(sender, args);
    }

    /// <summary>
    /// Called when the game is exiting
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected override void OnExiting(object sender, EventArgs args)
    {
        MonoUtils.IsStopping = true;
        ExitHelper.InvokeGameClosingEvent();
    }

    /// <summary>
    /// Called when the game is unloading content.
    /// </summary>
    protected override void UnloadContent()
    {
        base.UnloadContent();
    }
}
