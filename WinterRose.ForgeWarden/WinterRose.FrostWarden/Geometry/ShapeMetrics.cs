namespace WinterRose.ForgeWarden.Geometry
{
    public sealed class ShapeMetrics
    {
        public Vector2 Centroid { get; }
        public float Perimeter { get; }

        private ShapeMetrics(Vector2 centroid, float perimeter)
        {
            Centroid = centroid;
            Perimeter = perimeter;
        }

        public static ShapeMetrics From(ShapePath path)
        {
            var pts = path.Points;
            if (pts.Count == 0) return new ShapeMetrics(Vector2.Zero, 0f);

            float perimeter = 0f;
            for (int i = 0; i < (path.IsClosed ? pts.Count : pts.Count - 1); i++)
            {
                int j = (i + 1) % pts.Count;
                perimeter += Vector2.Distance(pts[i], pts[j]);
            }

            Vector2 centroid = Vector2.Zero;
            float signedArea = 0f;

            if (path.IsClosed && pts.Count >= 3)
            {
                for (int i = 0; i < pts.Count; i++)
                {
                    int j = (i + 1) % pts.Count;
                    float a = pts[i].X * pts[j].Y - pts[j].X * pts[i].Y;
                    signedArea += a;
                    centroid += (pts[i] + pts[j]) * a;
                }
                signedArea *= 0.5f;
                if (MathF.Abs(signedArea) > float.Epsilon)
                    centroid /= (6f * signedArea);
                else
                    centroid = pts.Aggregate(Vector2.Zero, (acc, p) => acc + p) / pts.Count;
            }
            else
            {
                centroid = pts.Aggregate(Vector2.Zero, (acc, p) => acc + p) / pts.Count;
            }

            return new ShapeMetrics(centroid, perimeter);
        }
    }
}
