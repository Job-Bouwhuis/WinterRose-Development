using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace WinterRose.ForgeWarden.Geometry;

public sealed class ShapePath
{
    public IReadOnlyList<Vector2> Points { get; }
    public bool IsClosed { get; }

    public ShapePath(IReadOnlyList<Vector2> points, bool isClosed)
    {
    }

    public ShapePath Resample(int pointCount)
    {
        throw new NotImplementedException();
    }
    public ShapePath Transform(Matrix3x2 transform)
    {
        throw new NotImplementedException();
    }
}
