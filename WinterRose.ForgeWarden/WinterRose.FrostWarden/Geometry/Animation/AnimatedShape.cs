using Raylib_cs;
using WinterRose.ForgeWarden.Geometry.Rendering;

namespace WinterRose.ForgeWarden.Geometry.Animation;

public sealed class AnimatedShape
{
    readonly ShapeMorph morph;
    readonly float duration;
    IEasingFunction easing = Easing.CubicInOut;

    float elapsed;
    public bool IsPlaying { get; private set; }

    public event Action Started;
    public event Action Completed;

    // inside Animation.AnimatedShape class (add these members)
    public float Progress
    {
        get => duration <= float.Epsilon ? 1f : Math.Clamp(elapsed / duration, 0f, 1f);
    }

    /// <summary>
    /// The eased progress (uses the easing function currently used by this animation).
    /// This matches the eased t used for geometry interpolation inside CurrentShape.
    /// </summary>
    public float EasedProgress => easing?.Evaluate(Progress) ?? Progress;


    public AnimatedShape(ShapeMorph morph, float duration)
    {
        this.morph = morph ?? throw new ArgumentNullException(nameof(morph));
        this.duration = MathF.Max(0.0001f, duration);
        elapsed = 0f;
        IsPlaying = false;
    }

    public AnimatedShape WithCenter(Vector2 center)
    {
        morph.Center(center);
        return this;
    }

    public ShapePath CurrentShape
    {
        get
        {
            float t = duration <= float.Epsilon ? 1f : Math.Clamp(elapsed / duration, 0f, 1f);
            float e = easing.Evaluate(t);

            var aPts = morph.From.Points;
            var bPts = morph.To.Points;
            int n = aPts.Count;
            var outPts = new Vector2[n];
            for (int i = 0; i < n; i++)
                outPts[i] = MathUtil.Lerp(aPts[i], bPts[i], e);

            // interpolate styles
            var interpolatedStyle = ShapeStyle.Lerp(morph.From.Style, morph.To.Style, e);

            // optionally, interpolate layers (round lerp)
            int layer = morph.From.Layer + (int)MathF.Round((morph.To.Layer - morph.From.Layer) * e);

            return new ShapePath(outPts, morph.From.IsClosed, interpolatedStyle, layer);
        }
    }

    public AnimatedShape Ease(IEasingFunction easingFunction)
    {
        easing = easingFunction ?? throw new ArgumentNullException(nameof(easingFunction));
        return this;
    }

    public AnimatedShape Play()
    {
        if (!IsPlaying)
        {
            IsPlaying = true;
            Started?.Invoke();
        }
        return this;
    }

    public AnimatedShape Stop()
    {
        IsPlaying = false;
        return this;
    }

    internal void Update()
    {
        if (!IsPlaying) return;
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            elapsed = duration;
            IsPlaying = false;
            Completed?.Invoke();
        }
    }

    public void Draw()
    {
        Update();
        CurrentShape.Draw();
    }

    public AnimatedShape OnStart(Action callback)
    {
        Started += callback;
        return this;
    }

    public AnimatedShape OnComplete(Action callback)
    {
        Completed += callback;
        return this;
    }
}
