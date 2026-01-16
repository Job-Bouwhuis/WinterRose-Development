namespace WinterRose.ForgeWarden.Geometry.Animation;

public static class Easing
{
    public static IEasingFunction Linear { get; } = new LinearEasing();
    public static IEasingFunction CubicInOut { get; } = new CubicInOutEasing();
    public static IEasingFunction BackOut { get; } = new BackOutEasing();
    public static IEasingFunction ElasticOut { get; } = new ElasticOutEasing();

    private sealed class LinearEasing : IEasingFunction { public float Evaluate(float t) => t; }
    private sealed class CubicInOutEasing : IEasingFunction { public float Evaluate(float t) => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f; }
    private sealed class BackOutEasing : IEasingFunction { public float Evaluate(float t) { const float c1 = 1.70158f; const float c3 = c1 + 1f; return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f); } }
    private sealed class ElasticOutEasing : IEasingFunction { public float Evaluate(float t) { if (t == 0f) return 0f; if (t == 1f) return 1f; const float c4 = (2f * MathF.PI) / 3f; return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * c4) + 1f; } }
}
