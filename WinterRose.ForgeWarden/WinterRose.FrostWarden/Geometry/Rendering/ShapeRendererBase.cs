namespace WinterRose.ForgeWarden.Geometry.Rendering;

public abstract class ShapeRendererBase : IShapeRenderer
{
    public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

    private readonly List<ShapeDrawCommand> drawQueue = new();

    public void Draw()
    {
        Collect();
    }

    public abstract void Collect();

    public virtual void Begin()
    {
        drawQueue.Clear();
    }

    public virtual void End()
    {
        drawQueue.Sort(static (a, b) => a.Layer.CompareTo(b.Layer));

        foreach (var command in drawQueue)
            OnDrawPath(command.Path, command.Style);

        drawQueue.Clear();
    }

    protected void Enqueue(ShapePath path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        drawQueue.Add(new ShapeDrawCommand(path));
    }

    public void DrawPath(ShapePath path)
    {
        Enqueue(path);
    }

    protected abstract void OnDrawPath(
        ShapePath path,
        ShapeStyle style);
}

