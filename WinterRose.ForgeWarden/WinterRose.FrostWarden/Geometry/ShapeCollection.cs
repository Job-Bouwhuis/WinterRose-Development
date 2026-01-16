using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeWarden.Geometry;

public sealed class ShapeCollection
{
    private readonly List<ShapePath> staticShapes = new();
    private readonly List<Animation.AnimatedShape> animatedShapes = new();

    public ShapeCollection() { }

    public ShapeCollection Add(ShapePath shape)
    {
        if (shape != null) staticShapes.Add(shape);
        return this;
    }

    public ShapeCollection Add(Animation.AnimatedShape animShape)
    {
        if (animShape != null) animatedShapes.Add(animShape);
        return this;
    }

    public ShapeCollection Remove(ShapePath shape)
    {
        staticShapes.Remove(shape);
        return this;
    }

    public ShapeCollection Remove(Animation.AnimatedShape animShape)
    {
        animatedShapes.Remove(animShape);
        return this;
    }

    public void Clear()
    {
        staticShapes.Clear();
        animatedShapes.Clear();
    }

    public void Draw()
    {
        foreach (var shape in staticShapes)
            shape.Draw();

        foreach (var anim in animatedShapes)
            anim.Draw();
    }

    /// <summary>
    /// Return a snapshot list with the current geometry of this collection (static + animated).
    /// Use this to align/match collections when morphing.
    /// </summary>
    public IReadOnlyList<ShapeSnapshot> GetSnapshot()
    {
        var list = new List<ShapeSnapshot>(staticShapes.Count + animatedShapes.Count);
        // static shapes as-is
        for (int i = 0; i < staticShapes.Count; i++)
            list.Add(staticShapes[i].Snapshot());

        for (int i = 0; i < animatedShapes.Count; i++)
            list.Add(animatedShapes[i].CurrentShape.Snapshot());

        return list;
    }

    // Count of items
    public int Count => staticShapes.Count + animatedShapes.Count;

    public Vector2 GetAverageCenter()
    {
        int totalPoints = 0;
        float sumX = 0f, sumY = 0f;

        // helper to accumulate points from a ShapePath
        void AccumulatePoints(IEnumerable<Vector2> points)
        {
            foreach (var p in points)
            {
                sumX += p.X;
                sumY += p.Y;
                totalPoints++;
            }
        }

        // static shapes
        for (int i = 0; i < staticShapes.Count; i++)
            AccumulatePoints(staticShapes[i].Points);

        // animated shapes: sample current geometry
        for (int i = 0; i < animatedShapes.Count; i++)
            AccumulatePoints(animatedShapes[i].CurrentShape.Points);

        if (totalPoints == 0)
            return new Vector2(0f, 0f); // fallback if empty

        return new Vector2(sumX / totalPoints, sumY / totalPoints);
    }

    internal void AddRange(IReadOnlyList<ShapeSnapshot> shapeSnapshots)
    {
        foreach (var snapshot in shapeSnapshots)
            staticShapes.Add(snapshot);
    }
}

