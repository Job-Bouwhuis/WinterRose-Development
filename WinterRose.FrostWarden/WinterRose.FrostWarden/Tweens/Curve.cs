using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Tweens;
public class Curve
{
    private readonly SortedDictionary<float, float> points;
    private readonly float smoothness;

    public Curve(SortedDictionary<float, float> points, float smoothness = 0f)
    {
        this.points = points;
        this.smoothness = Math.Clamp(smoothness, 0f, 1f); // 0 = sharp/linear, 1 = max smooth
    }

    // t is normalized [0..1]
    public float Evaluate(float t)
    {
        if (points.Count == 0) return t;
        if (t <= points.First().Key) return points.First().Value;
        if (t >= points.Last().Key) return points.Last().Value;

        var lower = points.LastOrDefault(p => p.Key <= t);
        var upper = points.FirstOrDefault(p => p.Key >= t);

        if (upper.Key == lower.Key) return lower.Value;

        float segmentT = (t - lower.Key) / (upper.Key - lower.Key);

        if (smoothness <= 0f)
        {
            // Pure linear
            return Lerp(lower.Value, upper.Value, segmentT);
        }

        // Smooth interpolation (Hermite-based ease in/out within each segment)
        float easedT = EaseInOut(segmentT, smoothness);
        return Lerp(lower.Value, upper.Value, easedT);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    // Smoothstep-like easing controlled by smoothness
    private static float EaseInOut(float t, float strength)
    {
        // strength 0 = linear
        // strength 1 = full smoothstep
        float smoothT = t * t * (3 - 2 * t); // standard smoothstep
        return Lerp(t, smoothT, strength);
    }
}

