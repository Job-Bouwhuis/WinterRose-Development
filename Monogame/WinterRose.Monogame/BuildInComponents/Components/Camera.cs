using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame;

/// <summary>
/// Camera component for the world. Worlds are not required to have a camera, however if you wish to move the point of view in the world, you need this
/// </summary>
public sealed class Camera : ObjectComponent
{
    /// <summary>
    /// Creates a new instance of the camera class
    /// </summary>
    private Camera() { }

    /// <summary>
    /// The index of the camera in the world.
    /// </summary>
    [Hide]
    public int CameraIndex { get; internal set; } = -1;

    /// <summary>
    /// The camera that is currently being used to render the world. If there is no camera being used, this will be null
    /// </summary>
    [Hide]
    public static Camera? current
    {
        get
        {
            if (Application.Current.CameraIndex == -1)
                return null;
            return Universe.CurrentWorld?.GetCamera(Application.Current.CameraIndex);
        }
    }

    /// <summary>
    /// The bounds of the camera. This is the size of the render target. Set to (0, 0) to use the window size
    /// </summary>
    [Hide]
    public Vector2I Bounds
    {
        get => renderTarget == null ? MonoUtils.WindowResolution : new(renderTarget.Bounds.Width, renderTarget.Bounds.Height);
        set
        {
            renderTarget?.Dispose();
            renderTarget = new(MonoUtils.Graphics, value.X, value.Y);
        }
    }

    /// <summary>
    /// The zoom of the camera
    /// </summary>
    public float Zoom { get; set; } = 1;

    /// <summary>
    /// Gets the top left corner of the camera in world space
    /// </summary>
    //[Hide]
    public Vector2 TopLeft => Transform.ScreenToWorldPos(new(0, 0), this);
    /// <summary>
    /// The top right corner of the camera in world space
    /// </summary>
    //[Hide]
    public Vector2 TopRight => Transform.ScreenToWorldPos(new(MonoUtils.WindowResolution.X, 0), this);
    /// <summary>
    /// The bottom left corner of the camera in world space
    /// </summary>
    //[Hide]
    public Vector2 BottomLeft => Transform.ScreenToWorldPos(new(0, MonoUtils.WindowResolution.Y), this);
    /// <summary>
    /// The bottom right corner of the camera in world space
    /// </summary>
    //[Hide]
    public Vector2 BottomRight => Transform.ScreenToWorldPos(MonoUtils.WindowResolution, this);

    public SpriteSortMode SpriteSorting { get; set; } = SpriteSortMode.FrontToBack;
    /// <summary>
    /// Copies the current frame into a new texture for you to use in whatever way you see fit. Perhaps save it?
    /// </summary>
    [Hide, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Texture2D Screenshot
    {
        get
        {
            Texture2D tex = new(MonoUtils.Graphics, renderTarget.Width, renderTarget.Height);

            Color[] colors = new Color[tex.Width * tex.Height];
            renderTarget.GetData(colors);
            tex.SetData(colors);
            return tex;
        }
    }

    public Matrix TransformMatrix => trMatrix;

    [Hide]
    private Matrix CalculateTransformMatrix
    {
        get
        {
            CalculateMatrix();
            return trMatrix;
        }
    }

    [Hide]
    [ExcludeFromSerialization]
    private SpriteBatch batch;
    [Hide]
    [ExcludeFromSerialization]
    private RenderTarget2D renderTarget;
    [Hide]
    [ExcludeFromSerialization]
    private Matrix trMatrix;

    protected override void Awake()
    {
        batch = new(MonoUtils.Graphics);
        var (x, y) = Bounds;
        renderTarget = new(MonoUtils.Graphics, x, y);
        CalculateMatrix();

        MonoUtils.GameWindow.ClientSizeChanged += (s, e) =>
        {
            var (x, y) = Bounds;
            renderTarget = new(MonoUtils.Graphics, x, y);
        };
    }
    private void CalculateMatrix()
    {
        Matrix pos = Matrix.CreateTranslation(
            -transform.position.X,
            -transform.position.Y,
            0);
        var rot = Matrix.CreateRotationZ(transform.rotation);
        var zoomMatrix = Matrix.CreateScale(Zoom);

        trMatrix = (pos * rot * zoomMatrix) * Matrix.CreateTranslation(Bounds.X / 2, Bounds.Y / 2, 0);
    }

    internal (RenderTarget2D view, List<WorldObject> UIObjects)
        GetCameraView(System.Func<SpriteBatch, List<WorldObject>> renderMethod)
    {
        MonoUtils.Graphics.SetRenderTarget(renderTarget);

        RasterizerState raster = new()
        {
            ScissorTestEnable = true,
            Name = "scissor" 
        };

        batch.GraphicsDevice.RasterizerState = raster;

        batch.Begin(
                transformMatrix: CalculateTransformMatrix,
                sortMode: SpriteSorting,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                rasterizerState: raster);

        Vector2I screenTopLeft = (Vector2I)Vector2.Transform(TopLeft, trMatrix);
        Vector2I screenBottomRight = (Vector2I)Vector2.Transform(BottomRight, trMatrix);
        MonoUtils.Graphics.ScissorRectangle =
            new Rectangle(
                screenTopLeft.X,
                screenTopLeft.Y,
                screenBottomRight.X - screenTopLeft.X,
                screenBottomRight.Y - screenTopLeft.Y);

        var uiObjs = renderMethod(batch);
        batch.End();

        MonoUtils.Graphics.SetRenderTarget(null);
        return (renderTarget, uiObjs);
    }

    /// <summary>
    /// Transforms the given rectangle into screenspace, rather than world space.<br></br>
    /// Used for mask clip rendering
    /// </summary>
    /// <param name="worldRect"></param>
    /// <returns></returns>
    public Rectangle WorldToScreenRectangle(Rectangle worldRect)
    {
        Vector2 worldTopLeft = new Vector2(worldRect.X, worldRect.Y);
        Vector2 worldBottomRight = new Vector2(worldRect.X + worldRect.Width, worldRect.Y + worldRect.Height);

        Vector2 screenTopLeft = Vector2.Transform(worldTopLeft, trMatrix);
        Vector2 screenBottomRight = Vector2.Transform(worldBottomRight, trMatrix);

        return new Rectangle(
            (int)screenTopLeft.X,
            (int)screenTopLeft.Y,
            (int)(screenBottomRight.X - screenTopLeft.X),
            (int)(screenBottomRight.Y - screenTopLeft.Y)
        );
    }

    internal void BeginWorldSpaceSpriteBatch(SpriteBatch batch)
    {
        batch.Begin(
            transformMatrix: CalculateTransformMatrix,
            sortMode: SpriteSorting,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp);
    }
}