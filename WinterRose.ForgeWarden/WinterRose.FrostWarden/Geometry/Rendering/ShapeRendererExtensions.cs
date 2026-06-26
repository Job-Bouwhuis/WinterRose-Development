using WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;

namespace WinterRose.ForgeWarden.Geometry.Rendering;

public static class ShapeSystemExtensions
{
    extension (ray)
    {
        public static void DrawShape(ShapePath path)
        {
            ForgeWardenEngine.Current.LayerStack.GetLayer<UiLayer>("UI").ShapeRenderer.DrawPath(path);
        }
    }
}
