using Raylib_cs;
using WinterRose.FrostWarden;
using WinterRose.WIP.TestClasses;

public class Camera : Component
{
    public bool useOrthographic = true;
    public float orthographicSize = 540f; // Half of 1080p height

    public float near = 0.01f;
    public float far = 1000f;

    public bool is3D = false;
    public static Camera main { get; internal set; }

    public Vector2 Resolution { get; set; } = Application.ScreenSize;

    // Returns a Raylib-style Camera2D for 2D rendering
    public Camera2D Camera2D => new Camera2D
    {
        Offset = Resolution / 2,
        Target = new Vector2(owner.transform.position.X, owner.transform.position.Y),
        Rotation = owner.transform.rotation.Z,
        Zoom = owner.transform.scale.Z
    };

    // Prepares a Raylib Camera3D struct from the Transform
    public Camera3D Camera3D => new Camera3D
    {
        Position = owner.transform.position,
        Target = owner.transform.position + transform.forward,
        Up = new Vector3(0, 1, 0),
        FovY = 60f,
        Projection = CameraProjection.Perspective
    };

    // Optional matrix-level API if doing custom shaders later
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
        if (useOrthographic)
        {
            float halfWidth = orthographicSize * width / height;
            float halfHeight = orthographicSize;

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
