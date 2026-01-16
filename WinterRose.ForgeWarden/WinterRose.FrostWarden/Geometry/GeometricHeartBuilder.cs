using Raylib_cs;
using System;
using System.Collections.Generic;

namespace WinterRose.ForgeWarden.Geometry;

public static class GeometricHeartBuilder
{
    public static ShapeCollection Build(
        Vector2 center,
        float size,
        Color heartColor,
        Color arrowColor,
        int layer
    )
    {
        var collection = new ShapeCollection();

        var heartPoints = GenerateHeartPoints(center, size);
        var heartStyle = new Rendering.ShapeStyle()
            .Filled()
            .WithColor(heartColor)
            .WithoutOutline();

        var heart = new ShapePath(heartPoints, true, heartStyle, layer);
        collection.Add(heart);

        AddLeftShading(collection, heartPoints, heartColor, layer + 1);


        AddHighlights(collection, center, size, layer + 2);

        return collection;
    }

    static IReadOnlyList<Vector2> GenerateHeartPoints(Vector2 center, float size)
    {
        int resolution = 64;
        var points = new Vector2[resolution];

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            float a = t * MathF.PI * 2f;

            float x = 16f * MathF.Pow(MathF.Sin(a), 3);
            float y =
                13f * MathF.Cos(a)
                - 5f * MathF.Cos(2f * a)
                - 2f * MathF.Cos(3f * a)
                - MathF.Cos(4f * a);

            points[i] = center + new Vector2(x, -y) * size * 0.05f;
        }

        return points;
    }

    // ======================================================
    // SHADING (LEFT SIDE)
    // ======================================================

    static void AddLeftShading(
        ShapeCollection collection,
        IReadOnlyList<Vector2> heart,
        Color baseColor,
        int layer
    )
    {
        int stripCount = 3;

        for (int i = 0; i < stripCount; i++)
        {
            float t0 = 0.55f + i * 0.08f;
            float t1 = t0 + 0.12f;

            var strip = new List<Vector2>();

            for (int p = 0; p < heart.Count; p++)
            {
                Vector2 pt = heart[p];
                if (pt.X < LerpHeartX(heart, t0))
                    strip.Add(pt);
            }

            if (strip.Count < 3)
                continue;

            var shadeColor = Darken(baseColor, 0.15f + i * 0.08f);
            shadeColor.A = 120;

            var style = new Rendering.ShapeStyle()
                .Filled()
                .WithColor(shadeColor)
                .WithoutOutline();

            collection.Add(new ShapePath(strip, true, style, layer));
        }
    }

    static float LerpHeartX(IReadOnlyList<Vector2> heart, float t)
    {
        int index = (int)(heart.Count * t);
        index = Math.Clamp(index, 0, heart.Count - 1);
        return heart[index].X;
    }

    // ======================================================
    // HIGHLIGHTS (TOP RIGHT)
    // ======================================================

    static void AddHighlights(
        ShapeCollection collection,
        Vector2 center,
        float size,
        int layer
    )
    {
        var highlightStyle = new Rendering.ShapeStyle()
            .Filled()
            .WithColor(new Color(255, 255, 255, 160))
            .WithoutOutline();

        collection.Add(MakeOval(
            center + new Vector2(size * 0.35f, -size * 0.35f),
            size * 0.25f,
            size * 0.15f,
            highlightStyle,
            layer
        ));

        collection.Add(MakeOval(
            center + new Vector2(size * 0.45f, -size * 0.15f),
            size * 0.12f,
            size * 0.08f,
            new Rendering.ShapeStyle()
                .Filled()
                .WithColor(new Color(255, 255, 255, 110))
                .WithoutOutline(),
            layer
        ));
    }

    static ShapePath MakeOval(
        Vector2 center,
        float radiusX,
        float radiusY,
        Rendering.ShapeStyle style,
        int layer
    )
    {
        int segments = 24;
        var pts = new Vector2[segments];

        for (int i = 0; i < segments; i++)
        {
            float a = i / (float)segments * MathF.PI * 2f;
            pts[i] = center + new Vector2(
                MathF.Cos(a) * radiusX,
                MathF.Sin(a) * radiusY
            );
        }

        return new ShapePath(pts, true, style, layer);
    }
    static Color Darken(Color color, float amount)
    {
        float f = Math.Clamp(1f - amount, 0f, 1f);
        return new Color(
            (byte)(color.R * f),
            (byte)(color.G * f),
            (byte)(color.B * f),
            color.A
        );
    }
}
