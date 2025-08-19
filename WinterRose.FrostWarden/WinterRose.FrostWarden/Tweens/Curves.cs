using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Tweens;
public static class Curves
{
    // Linear
    public static readonly Curve Linear = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 1f, 1f }
    });

    // Quadratic
    public static readonly Curve EaseInQuad = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 1f, 1f } // actual curve calculation done in interpolation
    });
    public static readonly Curve EaseOutQuad = EaseInQuad; // reuse
    public static readonly Curve EaseInOutQuad = EaseInQuad; // reuse

    // Cubic
    public static readonly Curve EaseInCubic = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 1f, 1f }
    });
    public static readonly Curve EaseOutCubic = EaseInCubic; // reuse
    public static readonly Curve EaseInOutCubic = EaseInCubic; // reuse

    // Quart
    public static readonly Curve EaseInQuart = EaseInCubic; // reuse
    public static readonly Curve EaseOutQuart = EaseInCubic; // reuse
    public static readonly Curve EaseInOutQuart = EaseInCubic; // reuse

    // Quint
    public static readonly Curve EaseInQuint = EaseInCubic; // reuse
    public static readonly Curve EaseOutQuint = EaseInCubic; // reuse
    public static readonly Curve EaseInOutQuint = EaseInCubic; // reuse

    // Back / Overshoot
    public static readonly Curve EaseOutBack = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.7f, 1.2f },
        { 1f, 1f }
    }, 2);

    // Bounce
    public static readonly Curve Bounce = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.3f, 1.1f },
        { 0.5f, 0.8f },
        { 0.7f, 1.05f },
        { 1f, 1f }
    }, 2);

    // Elastic
    public static readonly Curve ElasticOut = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.3f, 1.2f },
        { 0.5f, 0.8f },
        { 0.7f, 1.05f },
        { 1f, 1f }
    }, 2);

    public static readonly Curve ElasticIn = ElasticOut; // reuse
    public static readonly Curve ElasticInOut = ElasticOut; // reuse
}


