namespace WinterRose.ForgeWarden.Geometry.Rendering;

public readonly struct ShapeDrawCommand
{
    public ShapePath Path { get; init; }

    public ShapeDrawCommand(ShapePath owner)
    {
        Path = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public ShapeDrawCommand()
    {
    }

    public ShapeStyle Style => Path.Style;
    public int Layer => Path.Layer;
}

