using Raylib_cs;
using WinterRose.ForgeWarden.Geometry.Animation;
using WinterRose.ForgeWarden.Geometry.Rendering;

namespace WinterRose.ForgeWarden.Geometry
{
    public static class Shape
    {
        public static ShapePath Line(Vector2 start, Vector2 end)
        {
            return new ShapePath(new[] { start, end }, false, ShapeStyle.Default);
        }

        public static ShapePath Circle(float radius, int resolution = 32)
        {
            if (resolution < 3) resolution = 3;
            var pts = new Vector2[resolution];
            float inv = 1f / resolution;
            for (int i = 0; i < resolution; i++)
            {
                float a = i * MathF.PI * 2f * inv;
                pts[i] = new Vector2(MathF.Cos(a) * radius, MathF.Sin(a) * radius);
            }
            return new ShapePath(pts, true, ShapeStyle.Default);
        }

        public static ShapePath Polygon(int sides, float radius)
        {
            if (sides < 3) sides = 3;
            var pts = new Vector2[sides];
            float inv = 1f / sides;
            for (int i = 0; i < sides; i++)
            {
                float a = i * MathF.PI * 2f * inv;
                pts[i] = new Vector2(MathF.Cos(a) * radius, MathF.Sin(a) * radius);
            }
            return new ShapePath(pts, true, ShapeStyle.Default);
        }

        public static ShapePath Star(int points, float innerRadius, float outerRadius)
        {
            if (points < 2) points = 2;
            int verts = points * 2;
            var pts = new Vector2[verts];
            float inv = 1f / verts;
            for (int i = 0; i < verts; i++)
            {
                float a = i * MathF.PI * 2f * inv;
                float r = (i % 2 == 0) ? outerRadius : innerRadius;
                pts[i] = new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r);
            }
            return new ShapePath(pts, true, ShapeStyle.Default);
        }

        public static ShapePath Rectangle(float width, float height)
        {
            float hw = width * 0.5f;
            float hh = height * 0.5f;

            return new ShapePath(new[]
            {
                new Vector2(-hw, -hh),
                new Vector2(hw, -hh),
                new Vector2(hw, hh),
                new Vector2(-hw, hh)
            }, true, ShapeStyle.Default);
        }

        public static ShapePath Square(float size)
        {
            return Rectangle(size, size);
        }

        // returns a single closed petal oriented at `angle` (radians) with a petal-style applied
        public static ShapePath Petal(
            float angle,
            Color petalColor,
            float innerRadius = 6f,
            float outerRadius = 36f,
            float angularSpread = MathF.PI * 0.45f, // total angular width of petal
            int resolution = 20,                     // points along the outer arc
            float jitter = 0.06f,                    // radial jitter strength
            int? seed = null,
            int layer = 0)
        {
            var rnd = seed.HasValue ? new Random(seed.Value) : new Random();

            // outer arc samples from -halfSpread to +halfSpread
            float half = angularSpread * 0.5f;
            int outerCount = Math.Max(3, resolution);
            int innerCount = Math.Max(3, resolution / 2);

            var ptsList = new List<Vector2>(outerCount + innerCount + 2);

            // sample outer arc (tip side)
            for (int i = 0; i < outerCount; i++)
            {
                float t = i / (float)(outerCount - 1); // 0..1
                float theta = (t * 2f - 1f) * half;    // -half .. +half

                // radial profile: gaussian-like centered on 0
                float sigma = half * 0.5f;
                float bump = MathF.Exp(-(theta * theta) / (2f * sigma * sigma));

                float noise = 1f + ((float)rnd.NextDouble() - 0.5f) * jitter;
                float r = innerRadius + (outerRadius - innerRadius) * bump;
                r *= noise;

                float x = MathF.Cos(theta) * r;
                float y = MathF.Sin(theta) * r;
                ptsList.Add(new Vector2(x, y));
            }

            // inner arc (returning back toward base). we sample from +half to -half so winding is consistent
            for (int i = 0; i < innerCount; i++)
            {
                float t = i / (float)(innerCount - 1); // 0..1
                float theta = (1f - t) * half * 2f - half; // maps 0->+half, 1->-half

                // inner radius is near center, slightly raised to give thickness
                float innerNoise = 1f + ((float)rnd.NextDouble() - 0.5f) * (jitter * 0.6f);
                float r = innerRadius * 0.45f * innerNoise;

                float x = MathF.Cos(theta) * r;
                float y = MathF.Sin(theta) * r;
                ptsList.Add(new Vector2(x, y));
            }

            // finalize closed path (ensure closed *sequence* if your renderer expects repeat or not — your triangulation uses first point pivot)
            var pts = ptsList.ToArray();

            // rotate whole petal by requested angle
            var rot = Matrix3x2.CreateRotation(angle);
            for (int i = 0; i < pts.Length; i++)
                pts[i] = Vector2.Transform(pts[i], rot);

            // build style
            var style = new Rendering.ShapeStyle()
                .Filled()
                .WithOutline(Color.Black)
                .WithColor(petalColor)
                .WithThickness(2f);

            return new ShapePath(pts, true, style, layer).WithBaseAtOrigin();
        }

        public static Animation.AnimatedShape CreatePetalBloomAnimation(
            float angle,
                Color petalColor,
            Vector2 flowerCenter,
            float innerRadius = 6f,
            float outerRadius = 36f,
            float angularSpread = MathF.PI * 0.45f,
            int resolution = 20,
            float jitter = 0.06f,
            int? seed = null,
            float duration = 1.2f,
            int layer = 0)
        {
            var rndSeed = seed ?? Environment.TickCount;

            // final petal shape
            var petal = Petal(angle, petalColor, innerRadius, outerRadius, angularSpread, resolution, jitter, rndSeed, layer)
                .WithCenter(flowerCenter);

            // bud: small green circle at flower center
            var budRadius = innerRadius * 0.35f; // tiny ball
            var bud = BudCircle(petal.Points.Count, flowerCenter, budRadius, new Color(0, 180, 0), layer);

            // morph
            var morph = new Animation.ShapeMorph(bud, petal).OptimizeCorrespondence();

            // animate growth
            var anim = morph.Animate(duration).Ease(Animation.Easing.CubicInOut);

            return anim;
        }

        private static ShapePath BudCircle(int pointCount, Vector2 center, float radius, Color color, int layer = 0)
        {
            var pts = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / (float)pointCount;
                float a = t * MathF.Tau;
                pts[i] = center + new Vector2(MathF.Cos(a) * radius, MathF.Sin(a) * radius);
            }

            var style = new Rendering.ShapeStyle()
                .Filled()
                .WithoutOutline()
                .WithColor(color);

            return new ShapePath(pts, true, style, layer);
        }

        // returns an AnimatedShape that morphs a small bud to a full petal aligned to `angle`
        public static Animation.AnimatedShape CreatePetalAnimation(
            float angle,
            Color petalColor,
            float innerRadius = 6f,
            float outerRadius = 36f,
            float angularSpread = MathF.PI * 0.45f,
            int resolution = 20,
            float jitter = 0.06f,
            int? seed = null,
            float duration = 1.2f,
            int layer = 0)
        {
            // final petal
            var petal = Petal(angle, petalColor, innerRadius, outerRadius, angularSpread, resolution, jitter, seed, layer);

            // bud: small petal-like shape oriented same angle, same point count
            int pointCount = Math.Max(3, petal.Points.Count);

            // bud style slightly darker and smaller
            var budColor = new Color(
                MathF.Max(0f, petalColor.R - 50),
                MathF.Max(0f, petalColor.G - 50),
                MathF.Max(0f, petalColor.B - 50),
                petalColor.A
            );

            var budStyle = new Rendering.ShapeStyle()
                .Filled()
                .WithoutOutline()
                .WithColor(budColor)
                .WithThickness(1f);

            // make a tiny rounded bud circle oriented on same angle with pointCount resolution
            var budPts = new Vector2[pointCount];
            float budRadius = innerRadius * 0.25f;
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / (float)pointCount;
                float a = t * MathF.Tau;
                budPts[i] = new Vector2(MathF.Cos(a) * budRadius, MathF.Sin(a) * budRadius);
            }

            var bud = new ShapePath(budPts, true, budStyle, layer);

            // ensure same point count (petal already has that count, but be explicit)
            bud = bud.Resample(pointCount);
            var final = petal.Resample(pointCount);

            var morph = new Animation.ShapeMorph(bud, final).OptimizeCorrespondence();
            var anim = morph.Animate(duration).Ease(Easing.CubicInOut);

            return anim;
        }


        public static ShapePath Arc(
            float radius,
            float startAngle,
            float endAngle,
            int resolution = 16)
        {
            if (resolution < 2) resolution = 2;

            var pts = new Vector2[resolution];
            float step = (endAngle - startAngle) / (resolution - 1);

            for (int i = 0; i < resolution; i++)
            {
                float a = startAngle + step * i;
                pts[i] = new Vector2(
                    MathF.Cos(a) * radius,
                    MathF.Sin(a) * radius
                );
            }

            return new ShapePath(pts, false, ShapeStyle.Default);
        }

        public static ShapePath Ring(
            float innerRadius,
            float outerRadius,
            int resolution = 32)
        {
            if (resolution < 3) resolution = 3;

            var pts = new Vector2[resolution * 2];
            float inv = 1f / resolution;

            for (int i = 0; i < resolution; i++)
            {
                float a = i * MathF.PI * 2f * inv;

                pts[i] = new Vector2(
                    MathF.Cos(a) * outerRadius,
                    MathF.Sin(a) * outerRadius
                );

                pts[pts.Length - 1 - i] = new Vector2(
                    MathF.Cos(a) * innerRadius,
                    MathF.Sin(a) * innerRadius
                );
            }

            return new ShapePath(pts, true, ShapeStyle.Default);
        }


        public static ShapePath Graph(IReadOnlyList<float> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Count == 0) return new ShapePath(Array.Empty<Vector2>(), false, ShapeStyle.Default);

            var pts = new Vector2[values.Count];
            for (int i = 0; i < values.Count; i++)
                pts[i] = new Vector2(i, values[i]);

            return new ShapePath(pts, false, ShapeStyle.Default);
        }

        public static ShapeMorph Morph(ShapePath from, ShapePath to)
        {
            return new ShapeMorph(from, to);
        }

        public static ShapeSequence Sequence()
        {
            return new Animation.ShapeSequence();
        }

        static float AngleDiff(float a, float b)
        {
            float diff = Math.Abs(a - b) % (MathF.PI * 2f);
            if (diff > MathF.PI) diff = MathF.PI * 2f - diff;
            return diff;
        }
    }
}
