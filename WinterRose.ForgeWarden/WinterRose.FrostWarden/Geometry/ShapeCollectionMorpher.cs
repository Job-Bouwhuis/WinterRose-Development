using System.Drawing;
using WinterRose.ForgeWarden.Geometry.Animation;
using WinterRose.ForgeWarden.Geometry.Rendering;

namespace WinterRose.ForgeWarden.Geometry;

public sealed class ShapeCollectionSnapshot
{
    public readonly IReadOnlyList<ShapeSnapshot> Shapes;

    public ShapeCollectionSnapshot(IReadOnlyList<ShapeSnapshot> shapes)
    {
        Shapes = shapes;
    }
}

public sealed class ShapeSnapshot
{
    public readonly Vector2[] Points;
    public readonly Rendering.ShapeStyle Style;
    public readonly bool Closed;
    public readonly int Layer;

    public ShapeSnapshot(
        Vector2[] points,
        Rendering.ShapeStyle style,
        bool closed,
        int layer
    )
    {
        Points = points;
        Style = style;
        Closed = closed;
        Layer = layer;
    }

    public static implicit operator ShapePath(ShapeSnapshot snapshot)
    {
        if (snapshot is null)
            return null;
        return new ShapePath(
            snapshot.Points,
            snapshot.Closed,
            snapshot.Style,
            snapshot.Layer
        );
    }
}



public static class ShapeCollectionMorpher
{
    public sealed class MorphController
    {
        readonly List<MorphItem> items = new();

        float time;
        float duration;
        Animation.IEasingFunction easing;

        public float Progress
        {
            get
            {
                float t = duration <= 0f ? 1f : time / duration;
                t = Math.Clamp(t, 0f, 1f);
                return easing.Evaluate(t);
            }
        }

        public bool IsCompleted => Progress >= 1f;

        internal MorphController(float duration, Animation.IEasingFunction? easing)
        {
            this.duration = Math.Max(0.0001f, duration);
            this.easing = easing ?? Easing.CubicInOut;
        }

        public void Restart()
        {
            time = 0f;
        }

        public void Update()
        {
            time += Time.deltaTime;
        }

        public void Draw()
        {
            Update();
            float t = Progress;
                
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                ShapePath from = item.From;
                ShapePath to = item.To;


                if (t is 1)
                    to.Draw();

                int pointCount = Math.Max(from.Points.Count, to.Points.Count);

                var aResampled = from.Resample(pointCount);
                var bResampled = to.Resample(pointCount);

                var points = new Vector2[pointCount];
                for (int p = 0; p < pointCount; p++)
                    points[p] = MathUtil.Lerp(aResampled.Points[p], bResampled.Points[p], t);

                float scale = 1f;
                float alphaMul = 1f;

                if (item.IsAppearing)
                {
                    scale = t;
                    alphaMul = t;
                }
                else if (item.IsDisappearing)
                {
                    scale = 1f - t;
                    alphaMul = 1f - t;
                }

                Vector2 center = Centroid(points);
                for (int p = 0; p < points.Length; p++)
                    points[p] = center + (points[p] - center) * scale;

                var style = ShapeStyle.Lerp(item.FromStyle, item.ToStyle, t);
                style.WithColor(style.Color.WithAlpha(style.Color.A * alphaMul));

                int layer = (int)MathF.Round(
                    item.FromLayer + (item.ToLayer - item.FromLayer) * t
                );

                var path = new ShapePath(points, to.IsClosed, style, layer);
                path.Draw();
            }
        }

        internal void Add(MorphItem item)
        {
            items.Add(item);
        }

        public ShapeCollection Snapshot()
        {
            var collection = new ShapeCollection();
            float t = Progress;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                ShapePath from = item.From;
                ShapePath to = item.To;

                int pointCount = Math.Max(from.Points.Count, to.Points.Count);

                var aResampled = from.Resample(pointCount);
                var bResampled = to.Resample(pointCount);

                var points = new Vector2[pointCount];
                for (int p = 0; p < pointCount; p++)
                    points[p] = MathUtil.Lerp(aResampled.Points[p], bResampled.Points[p], t);

                float scale = 1f;
                float alphaMul = 1f;

                if (item.IsAppearing)
                {
                    scale = t;
                    alphaMul = t;
                }
                else if (item.IsDisappearing)
                {
                    scale = 1f - t;
                    alphaMul = 1f - t;
                }

                Vector2 center = Centroid(points);
                for (int p = 0; p < points.Length; p++)
                    points[p] = center + (points[p] - center) * scale;

                var style = ShapeStyle.Lerp(item.FromStyle, item.ToStyle, t);
                style.WithColor(style.Color.WithAlpha(style.Color.A * alphaMul));

                int layer = (int)MathF.Round(item.FromLayer + (item.ToLayer - item.FromLayer) * t);

                collection.Add(new ShapePath(points, to.IsClosed, style, layer));
            }

            return collection;
        }
    }

    internal sealed class MorphItem
    {
        public ShapePath From;
        public ShapePath To;

        public ShapeStyle FromStyle;
        public ShapeStyle ToStyle;

        public int FromLayer;
        public int ToLayer;

        public bool IsAppearing;
        public bool IsDisappearing;
    }

    public static MorphController CreateMorph(
        ShapeCollection from,
        ShapeCollection to,
        float duration,
        Animation.IEasingFunction easing = null
    )
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        var snapA = from.GetSnapshot().ToList();
        var snapB = to.GetSnapshot().ToList();

        int count = Math.Max(snapA.Count, snapB.Count);

        var controller = new MorphController(duration, easing);

        for (int i = 0; i < count; i++)
        {
            ShapePath a = i < snapA.Count ? snapA[i] : null;
            ShapePath b = i < snapB.Count ? snapB[i] : null;

            bool appearing = false;
            bool disappearing = false;

            if (a == null && b == null)
                continue;

            if (a == null)
            {
                appearing = true;
                a = CreateDegenerate(Centroid(b), b.Style, b.Layer);
            }

            if (b == null)
            {
                disappearing = true;
                b = CreateDegenerate(Centroid(a), a.Style, a.Layer);
            }

            controller.Add(new MorphItem
            {
                From = a,
                To = b,

                FromStyle = a.Style ?? ShapeStyle.Default,
                ToStyle = b.Style ?? ShapeStyle.Default,

                FromLayer = a.Layer,
                ToLayer = b.Layer,

                IsAppearing = appearing,
                IsDisappearing = disappearing
            });
        }

        return controller;
    }

    static ShapePath CreateDegenerate(Vector2 center, ShapeStyle style, int layer)
    {
        return new ShapePath(new[] { center }, false, style, layer);
    }

    static Vector2 Centroid(IReadOnlyList<Vector2> points)
    {
        if (points == null || points.Count == 0)
            return Vector2.Zero;

        float sx = 0f;
        float sy = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            sx += points[i].X;
            sy += points[i].Y;
        }

        return new Vector2(sx / points.Count, sy / points.Count);
    }

    static Vector2 Centroid(ShapePath path)
    {
        return path == null ? Vector2.Zero : Centroid(path.Points);
    }

    public static ShapeCollectionSnapshot Capture(
    ShapeCollection collection,
    int pointResolution = 64)
    {
        return new ShapeCollectionSnapshot(collection.GetSnapshot());
    }

}


