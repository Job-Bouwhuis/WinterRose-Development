using Raylib_cs;
using WinterRose.ForgeWarden;
using WinterRose.WIP.TestClasses;

public class Camera : Component
{
    public bool UseOrthographic { get; set; } = true;
    public float OrthographicSize { get; set; } = 540f; // Half of 1080p height

    public float near { get; set; } = 0.01f;
    public float far { get; set; } = 1000f;

    public bool is3D { get; set; } = false;
    public static Camera main { get; internal set; }

    public Vector2 Resolution { get; set; } = ForgeWardenEngine.Current.Window.Size;

    public Camera2D Camera2D => new Camera2D
    {
        Offset = Resolution / 2,
        Target = new Vector2(owner.transform.position.X, owner.transform.position.Y),
        Rotation = owner.transform.rotation.Z,
        Zoom = owner.transform.position.Z
    };

    public Camera3D Camera3D => new Camera3D
    {
        Position = owner.transform.position,
        Target = owner.transform.position + transform.forward,
        Up = new Vector3(0, 1, 0),
        FovY = 60f,
        Projection = CameraProjection.Perspective
    };

    public Matrix4x4 ViewMatrix
    {
        get
        {
            Matrix4x4.Invert(owner.transform.worldMatrix, out var view);
            return view;
        }
    }

    public Vector3 ScreenToWorld(Vector2 mouseScreenPos)
    {
        Vector2 worldPos = Raylib.GetScreenToWorld2D(mouseScreenPos, Camera2D);
        return new Vector3(worldPos.X, worldPos.Y, 0);
    }

    public Matrix4x4 GetProjectionMatrix(int width, int height)
    {
        if (UseOrthographic)
        {
            float halfWidth = OrthographicSize * width / height;
            float halfHeight = OrthographicSize;

            return Matrix4x4.CreateOrthographicOffCenter(
                -halfWidth, halfWidth,
                halfHeight, -halfHeight,
                near, far);
        }
        else
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(
                MathF.PI / 4f,
                width / (float)height,
                near, far);
        }
    }
}
