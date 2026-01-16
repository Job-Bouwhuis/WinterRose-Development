namespace WinterRose.ForgeWarden.Geometry.Rendering;

public static class ShapeSystemExtensions
{
    extension(IShapeRenderer renderer)
    {
    }

    extension (ray)
    {
        public static void DrawShape(ShapePath path)
        {
            ForgeWardenEngine.Current.ShapeRenderer.DrawPath(path);
        }
    }
}
