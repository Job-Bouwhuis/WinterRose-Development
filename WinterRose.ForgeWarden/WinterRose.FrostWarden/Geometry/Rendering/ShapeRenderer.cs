using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeWarden.Geometry.Rendering;

public sealed class ShapeRenderer : ShapeRendererBase
{
    private readonly ShapeAnimationSystem system;
    private readonly List<ShapeDrawCommand> drawQueue = new();

    public ShapeRenderer(ShapeAnimationSystem system)
    {
        this.system = system ?? throw new ArgumentNullException(nameof(system));
    }

    public override void Collect()
    {
        foreach (var anim in system.ActiveAnimations)
        {
            Enqueue(anim.CurrentShape);
        }
        drawQueue.Clear();
    }


    protected override void OnDrawPath(ShapePath path, ShapeStyle style)
    {
        if (path.Points.Count < 2) return;

        var points = path.Points;

        if (style.HasOutline)
        {
            for (int i = 0; i < points.Count - 1; i++)
                Raylib_cs.Raylib.DrawLineEx(points[i], points[i + 1], style.Thickness, style.OutlineColor.WithAlpha(255));

            if (path.IsClosed)
                Raylib_cs.Raylib.DrawLineEx(points[^1], points[0], style.Thickness, style.OutlineColor.WithAlpha(255));
        }

        if (style.IsFill && points.Count >= 3)
        {
            var first = points[0];
            for (int i = 1; i < points.Count - 1; i++)
            {
                var second = points[i];
                var third = points[i + 1];
                Raylib_cs.Raylib.DrawTriangle(third, second, first, style.Color.WithAlpha(255));
            }
        }
    }



    internal void Update() => system.Update();
}

