using WinterRose.ForgeWarden.Geometry;

namespace WinterRose.ForgeWarden.Geometry.Animation;

public sealed class ShapeMorph
{
    public ShapePath From { get; }
    public ShapePath To { get; }

    public ShapeMorph(ShapePath from, ShapePath to)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        int count = Math.Max(from.Points.Count, to.Points.Count);
        if (count < 1) count = 1;

        From = from.Resample(count);
        To = to.Resample(count);
    }

    public ShapeMorph WithPointCount(int pointCount)
    {
        var f = From.Resample(pointCount);
        var t = To.Resample(pointCount);
        return new ShapeMorph(f, t);
    }

    public ShapeMorph OptimizeCorrespondence()
    {
        if (!From.IsClosed || !To.IsClosed) return this;

        int n = From.Points.Count;
        int bestShift = 0;
        float bestScore = float.MaxValue;

        for (int shift = 0; shift < n; shift++)
        {
            float score = 0f;
            for (int i = 0; i < n; i++)
            {
                int j = (i + shift) % n;
                score += Vector2.DistanceSquared(From.Points[i], To.Points[j]);
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestShift = shift;
            }
        }

        var shifted = new Vector2[n];
        for (int i = 0; i < n; i++)
            shifted[i] = To.Points[(i + bestShift) % n];

        return new ShapeMorph(From, new ShapePath(shifted, To.IsClosed, To.Style, From.Layer));
    }

    public void Center(Vector2 center)
    {
        From.WithCenter(center);
        To.WithCenter(center);
    }

    public AnimatedShape Animate(float duration)
    {
        return new AnimatedShape(this, duration);
    }
}