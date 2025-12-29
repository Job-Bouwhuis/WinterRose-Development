using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Tweens;
public static class Curves
{
    public static readonly Curve Linear = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 1f, 1f }
    });

    public static readonly Curve SlowFastSlow = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },     
        { 0.2f, 0.15f }, 
        { 0.5f, 0.8f }, 
        { 0.8f, 0.95f },
        { 1f, 1f }     
    });

    public static readonly Curve ExtraSlowFastSlow = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.3f, 0.15f },
        { 0.6f, 0.8f },
        { 0.8f, 0.95f },
        { 1f, 1f }
    });

    public static readonly Curve ExtraSlowFastSlowReversed = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 1f },
        { 0.2f, 0.05f },
        { 0.4f, 0.2f },
        { 0.7f, 0.85f },
        { 1f, 0f }
    });

    public static readonly Curve EaseOutBackLow = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.7f, 1.05f },
        { 1f, 1f }
    }, 2);

    public static readonly Curve EaseOutBack = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.7f, 1.2f },
        { 1f, 1f }
    }, 2);

    public static readonly Curve EaseOutBackFar = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.7f, 1.7f },
        { 1f, 1f }
    }, 2);

    public static readonly Curve Bounce = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.3f, 1.1f },
        { 0.5f, 0.8f },
        { 0.7f, 1.05f },
        { 1f, 1f }
    }, 2);

    public static readonly Curve ElasticLong = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.15f, 1.02f },
        { 0.3f, 0.97f },
        { 0.45f, 1.05f },
        { 0.6f, 0.98f },
        { 0.75f, 1.02f },
        { 0.9f, 0.995f },
        { 1f, 1f }
    }, 2f);



    public static readonly Curve Elast = new Curve(new SortedDictionary<float, float>
    {
        { 0f, 0f },
        { 0.3f, 1.5f },
        { 0.5f, 0.8f },
        { 0.7f, 1.2f },
        { 1f, 1f }
    }, 2);
}


