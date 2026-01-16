namespace WinterRose.ForgeWarden.Geometry.Animation;

public sealed class ShapeSequence
{
    readonly List<(ShapePath from, ShapePath to, float duration, IEasingFunction easing)> steps = new();
    bool loop;

    public ShapeSequence Then(ShapePath from, ShapePath to, float duration, IEasingFunction easing)
    {
        steps.Add((from, to, duration, easing ?? Easing.CubicInOut));
        return this;
    }

    public ShapeSequence Loop()
    {
        loop = true;
        return this;
    }

    public AnimatedShape Build()
    {
        if (steps.Count == 0) throw new InvalidOperationException("No steps defined");
        var first = new ShapeMorph(steps[0].from, steps[0].to);
        var anim = new AnimatedShape(first, steps[0].duration);
        anim.Ease(steps[0].easing);
        return anim;
    }
}