using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WinterRose.ForgeWarden.Geometry.Rendering;

namespace WinterRose.ForgeWarden.Geometry;

public sealed class ShapePath(IReadOnlyList<Vector2> points, bool isClosed, Rendering.ShapeStyle style, int layer = 0)
{
    public IReadOnlyList<Vector2> Points { get; private set; } = points ?? throw new ArgumentNullException(nameof(points));
    public bool IsClosed { get; private set; } = isClosed;
    public ShapeStyle Style { get; private set; } = style ?? throw new ArgumentNullException(nameof(points));
    public int Layer { get; private set; } = layer;

    public ShapeStyle GetStyle()
    {
        return Style;
    }
    public ShapePath WithStyle(ShapeStyle style)
    {
        Style = style;
        return this;
    }
    public ShapePath WithLayer(int layer)
    {
        Layer = layer;
        return this;
    }
    public ShapePath Resample(int pointCount)
    {
        if (pointCount < 1) throw new ArgumentOutOfRangeException(nameof(pointCount));

        var src = Points;
        int srcCount = src.Count;

        if (srcCount == 0)
            return new ShapePath(Array.Empty<Vector2>(), IsClosed, Style, Layer);

        if (srcCount == 1)
        {
            var arr = Enumerable.Repeat(src[0], pointCount).ToArray();
            return new ShapePath(arr, IsClosed, Style, Layer);
        }

        var segmentCount = IsClosed ? srcCount : srcCount - 1;
        var lengths = new float[segmentCount];
        float total = 0f;

        for (int i = 0; i < segmentCount; i++)
        {
            int j = (i + 1) % srcCount;
            float segLen = Vector2.Distance(src[i], src[j]);
            lengths[i] = segLen;
            total += segLen;
        }

        if (total <= float.Epsilon)
        {
            var arr = Enumerable.Repeat(src[0], pointCount).ToArray();
            return new ShapePath(arr, IsClosed, Style, Layer);
        }

        var cumulative = new float[segmentCount + 1];
        cumulative[0] = 0f;
        for (int i = 0; i < segmentCount; i++)
            cumulative[i + 1] = cumulative[i] + lengths[i];

        var result = new Vector2[pointCount];

        if (IsClosed)
        {
            float step = total / pointCount;
            for (int k = 0; k < pointCount; k++)
            {
                float dist = k * step;
                int seg = Array.BinarySearch(cumulative, dist);
                if (seg < 0)
                {
                    seg = ~seg - 1;
                    if (seg < 0) seg = 0;
                    if (seg >= segmentCount) seg = segmentCount - 1;
                }

                float segStart = cumulative[seg];
                float segLen = lengths[seg];
                float t = segLen <= float.Epsilon ? 0f : (dist - segStart) / segLen;

                int a = seg;
                int b = (seg + 1) % srcCount;
                result[k] = MathUtil.Lerp(src[a], src[b], t);
            }
        }
        else // open path
        {
            if (pointCount == 1)
            {
                result[0] = src[0];
            }
            else
            {
                float step = total / (pointCount - 1);
                for (int k = 0; k < pointCount; k++)
                {
                    float dist = k == pointCount - 1 ? total : k * step;
                    int seg = Array.BinarySearch(cumulative, dist);
                    if (seg < 0)
                        seg = ~seg - 1;
                    seg = Math.Clamp(seg, 0, segmentCount - 1);


                    float segStart = cumulative[seg];
                    float segLen = lengths[seg];
                    float t = segLen <= float.Epsilon ? 0f : (dist - segStart) / segLen;

                    int a = seg;
                    int b = Math.Min(seg + 1, srcCount - 1);
                    result[k] = MathUtil.Lerp(src[a], src[b], t);
                }
            }
        }

        Points = result;
        return this;
    }
    public ShapePath Transform(Matrix3x2 transform)
    {
        var transformed = Points.Select(p => Vector2.Transform(p, transform)).ToArray();
        Points = transformed;
        return this;
    }

    public ShapePath WithCenter(float x, float y) => WithCenter(new Vector2(x, y));

    public ShapePath WithCenter(Vector2 newCenter)
    {
        if (Points.Count == 0) return this;

        var currentCenter = new Vector2(
            Points.Average(p => p.X),
            Points.Average(p => p.Y)
        );

        var offset = newCenter - currentCenter;

        var translated = Points.Select(p => p + offset).ToArray();

        Points = translated;
        return this;
    }

    public ShapePath WithBaseAtOrigin()
    {
        if (Points.Count == 0) return this;

        // anchor at the first point (the base)
        Vector2 basePoint = Points[0];

        var translated = Points.Select(p => p - basePoint).ToArray();

        return new ShapePath(translated, IsClosed, Style, Layer);
    }


    public void Draw()
    {
        ray.DrawShape(this);
    }

    internal ShapeSnapshot Snapshot() => new ShapeSnapshot([.. Points], Style, IsClosed, Layer);
}

