namespace WinterRose.ForgeWarden.Geometry.Rendering;

public interface IShapeRenderer
{
    Matrix4x4 Transform { get; set; }

    void Begin();
    void End();

    void DrawPath(ShapePath path);
}

