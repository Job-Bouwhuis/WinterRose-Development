using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    [Hidden]
    public int CameraIndex { get; internal set; } = -1;

    /// <summary>
    /// The camera that is currently being used to render the world. If there is no camera being used, this will be null
    /// </summary>
    [Hidden]
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
    [Hidden]
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
    [Hidden]
    public Vector2 TopLeft => Transform.ScreenToWorldPos(new(0, 0), this);
    /// <summary>
    /// The top right corner of the camera in world space
    /// </summary>
    [Hidden]
    public Vector2 TopRight => Transform.ScreenToWorldPos(new(MonoUtils.WindowResolution.X, 0), this);
    /// <summary>
    /// The bottom left corner of the camera in world space
    /// </summary>
    [Hidden]
    public Vector2 BottomLeft => Transform.ScreenToWorldPos(new(0, MonoUtils.WindowResolution.Y), this);
    /// <summary>
    /// The bottom right corner of the camera in world space
    /// </summary>
    [Hidden]
    public Vector2 BottomRight => Transform.ScreenToWorldPos(MonoUtils.WindowResolution, this);

    public SpriteSortMode SpriteSorting { get; set; } = SpriteSortMode.FrontToBack;
    /// <summary>
    /// Copies the current frame into a new texture for you to use in whatever way you see fit. Perhaps save it?
    /// </summary>
    [Hidden, DebuggerBrowsable(DebuggerBrowsableState.Never)]
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

    [Hidden]
    private Matrix TransformMatrix
    {
        get
        {
            CalculateMatrix();
            return trMatrix;
        }
    }

    [Hidden]
    [IgnoreInTemplateCreation] private SpriteBatch batch;
    [Hidden]
    [IgnoreInTemplateCreation] private RenderTarget2D renderTarget;
    [Hidden]
    [IgnoreInTemplateCreation] private Matrix trMatrix;

    private void Awake()
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
        var zoomMatrix = Matrix.CreateScale(Zoom);

        trMatrix = (pos * zoomMatrix) * Matrix.CreateTranslation(Bounds.X / 2, Bounds.Y / 2, 0);
    }

    internal RenderTarget2D GetCameraView(System.Action<SpriteBatch> renderMethod)
    {
        MonoUtils.Graphics.SetRenderTarget(renderTarget);

        batch.Begin(
            transformMatrix: TransformMatrix,
            sortMode: SpriteSorting,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp);

        renderMethod(batch);
        batch.End();

        MonoUtils.Graphics.SetRenderTarget(null);
        return renderTarget;
    }

    internal void BeginWorldSpaceSpriteBatch(SpriteBatch batch)
    {
        batch.Begin(
            transformMatrix: TransformMatrix,
            sortMode: SpriteSorting,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp);
    }
}