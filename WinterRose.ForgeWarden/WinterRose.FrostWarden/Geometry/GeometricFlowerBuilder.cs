using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace WinterRose.ForgeWarden.Geometry
{
    /// <summary>
    /// Color harmony scheme used when <see cref="GeometricFlowerBuilder.FlowerConfig.PetalColor"/>
    /// or <see cref="GeometricFlowerBuilder.FlowerConfig.CenterColor"/> are left <c>null</c>.
    /// </summary>
    public enum PaletteScheme
    {
        /// <summary>Pick a harmony scheme at random each time.</summary>
        Random,
        /// <summary>Petals and center sit at opposite hues (180° apart).</summary>
        Complementary,
        /// <summary>Petals and center share neighbouring hues (±18–30°).</summary>
        Analogous,
        /// <summary>Three hues spaced 120° apart; petals use the base, center uses the second.</summary>
        Triadic,
        /// <summary>Petals at base hue; center flanks the complement (±25–42°).</summary>
        SplitComplementary,
        /// <summary>Petals and center share the same hue, differ in lightness and saturation.</summary>
        Monochromatic,
        /// <summary>Reds, oranges, and yellows only.</summary>
        Warm,
        /// <summary>Blues, cyans, and purples only.</summary>
        Cool,
    }

    /// <summary>
    /// The output of <see cref="GeometricFlowerBuilder.Flower"/>.
    /// Contains the renderable collection <b>and</b> a fully-pinned config
    /// that can reproduce the exact same flower when passed back to <c>Flower()</c>.
    /// </summary>
    public sealed class FlowerResult
    {
        /// <summary>The rendered shape collection, ready to add to a scene.</summary>
        public ShapeCollection Shapes { get; init; }

        /// <summary>
        /// A fully-resolved copy of the config that was used to build this flower.
        /// Every field is pinned — <see cref="GeometricFlowerBuilder.FlowerConfig.Seed"/> is always
        /// set and <see cref="GeometricFlowerBuilder.FlowerConfig.PetalColor"/> /
        /// <see cref="GeometricFlowerBuilder.FlowerConfig.CenterColor"/> are never <c>null</c>.
        /// Store and replay to get an identical flower.
        /// </summary>
        public GeometricFlowerBuilder.FlowerConfig ResolvedConfig { get; init; }
    }

    public static class GeometricFlowerBuilder
    {
        public class FlowerConfig
        {
            /// <summary>
            /// Explicit petal color. Leave <c>null</c> to auto-generate from
            /// <see cref="ColorScheme"/> and <see cref="Seed"/>.
            /// </summary>
            public Color? PetalColor { get; init; } = null;

            /// <summary>
            /// Explicit flower-center color. Leave <c>null</c> to auto-generate from
            /// <see cref="ColorScheme"/> and <see cref="Seed"/>.
            /// </summary>
            public Color? CenterColor { get; init; } = null;

            /// <summary>
            /// Harmony scheme used when either color is <c>null</c>.
            /// Defaults to <see cref="PaletteScheme.Random"/>.
            /// </summary>
            public PaletteScheme ColorScheme { get; init; } = PaletteScheme.Random;

            public Vector2 Center { get; init; } = Vector2.Zero;
            public int MinPetals { get; init; } = 5;
            public int MaxPetals { get; init; } = 9;
            public int MinStems { get; init; } = 1;
            public int MaxStems { get; init; } = 70;
            public float InnerRadius { get; init; } = 6f;
            public float OuterRadius { get; init; } = 36f;
            public float AngularSpread { get; init; } = MathF.PI * 0.45f;
            public int ResolutionPerPetal { get; init; } = 20;
            public float Jitter { get; init; } = 0.06f;
            public float PetalDuration { get; init; } = 4;
            public float CenterDuration { get; init; } = 1.0f;

            /// <summary>
            /// RNG seed. When <c>null</c>, a seed is generated automatically and
            /// stored in <see cref="FlowerResult.ResolvedConfig"/> so the flower
            /// can still be reproduced later.
            /// </summary>
            public int? Seed { get; init; } = null;

            public int Layer { get; init; } = 0;
            public float StemLength { get; init; } = 120f;
            public float StemThickness { get; init; } = 4f;
            public float BranchChance { get; init; } = 0.14f;
            public float LeafChance { get; init; } = 0.5f;
            public float Scale { get; init; } = 1f;
        }

        // ─── Public entry point ──────────────────────────────────────────────────

        /// <summary>
        /// Build a flower from <paramref name="config"/> and return both the renderable
        /// collection and a fully-pinned config that reproduces the flower exactly.
        /// </summary>
        public static FlowerResult Flower(FlowerConfig config)
        {
            // Always pin the seed so the result is reproducible even when the
            // caller didn't specify one.
            int seed = config.Seed ?? new Random().Next();
            var rnd = new Random(seed);

            var collection = new ShapeCollection();

            // Resolve colors once from the seeded RNG; all flowers in this call
            // share one cohesive palette.
            var (petalColor, centerColor) = ResolveColors(config, rnd);

            // Build stems / branches
            int targetStems = rnd.Next(config.MinStems, config.MaxStems + 1);
            int remainingBranches = Math.Max(0, targetStems - 1);
            Vector2 initialDir = new Vector2(0f, -1f);
            var flowerPositions = new List<Vector2>();
            var occupiedPositions = new List<Vector2>();

            BuildStemRecursive(
                collection, config, rnd,
                config.Center, config.StemLength * config.Scale,
                config.Layer, initialDir,
                ref remainingBranches,
                flowerPositions, occupiedPositions,
                isInitialStem: true);

            // Place flowers at every branch tip
            foreach (var pos in flowerPositions)
                CreateFlowerAtPosition(collection, config, rnd, pos, petalColor, centerColor);

            // Build the fully-pinned resolved config for storage / replay
            var resolvedConfig = new FlowerConfig
            {
                PetalColor = petalColor,
                CenterColor = centerColor,
                ColorScheme = config.ColorScheme,
                Center = config.Center,
                MinPetals = config.MinPetals,
                MaxPetals = config.MaxPetals,
                MinStems = config.MinStems,
                MaxStems = config.MaxStems,
                InnerRadius = config.InnerRadius,
                OuterRadius = config.OuterRadius,
                AngularSpread = config.AngularSpread,
                ResolutionPerPetal = config.ResolutionPerPetal,
                Jitter = config.Jitter,
                PetalDuration = config.PetalDuration,
                CenterDuration = config.CenterDuration,
                Seed = seed,          // always pinned
                Layer = config.Layer,
                StemLength = config.StemLength,
                StemThickness = config.StemThickness,
                BranchChance = config.BranchChance,
                LeafChance = config.LeafChance,
                Scale = config.Scale,
            };

            return new FlowerResult { Shapes = collection, ResolvedConfig = resolvedConfig };
        }

        static Color VaryColor(Color baseColor, Random rnd, float amount = 0.12f)
        {
            float r = baseColor.R / 255f;
            float g = baseColor.G / 255f;
            float b = baseColor.B / 255f;

            (float hue, float sat, float val) = Raylib.ColorToHSV(baseColor);

            hue += ((float)rnd.NextDouble() - 0.5f) * amount * 60f;
            sat += ((float)rnd.NextDouble() - 0.5f) * amount;
            val += ((float)rnd.NextDouble() - 0.5f) * amount;

            if (sat < 0f) sat = 0f;
            if (sat > 1f) sat = 1f;

            if (val < 0f) val = 0f;
            if (val > 1f) val = 1f;

            return Raylib.ColorFromHSV(hue, sat, val);
        }

        // ─── Palette / color resolution ──────────────────────────────────────────

        static float SampleFlowerHue(Random rnd)
        {
            float t = (float)rnd.NextDouble();

            // Weighted regions: [0–0.25] reds/yellows, [0.45–0.6] magentas
            if (t < 0.6f)
            {
                // warm range: red → orange → yellow
                return 0.00f + (float)rnd.NextDouble() * 0.25f;
            }
            else
            {
                // pink → magenta → purple-ish (but avoid blue shift)
                return 0.80f + (float)rnd.NextDouble() * 0.20f;
            }
        }

        static float ClampFlowerHue(float hue)
        {
            hue %= 1f;
            if (hue < 0f) hue += 1f;

            // kill green zone
            if (hue > 0.25f && hue < 0.45f)
                hue = 0.25f + (hue - 0.25f) * 0.15f;

            // kill blue zone
            if (hue > 0.55f && hue < 0.75f)
                hue = 0.55f + (hue - 0.55f) * 0.10f;

            return hue;
        }

        static Color ApplyOccasionalSpike(Color baseColor, Random rnd, float chance = 0.08f)
        {
            if (rnd.NextDouble() > chance)
                return baseColor;

            (float h, float s, float v) = Raylib.ColorToHSV(baseColor);

            bool neon = rnd.NextDouble() < 0.5;

            if (neon)
            {
                s = MathF.Min(1f, s + 0.35f + (float)rnd.NextDouble() * 0.25f);
                v = MathF.Min(1f, v + 0.20f + (float)rnd.NextDouble() * 0.20f);
                h += ((float)rnd.NextDouble() - 0.5f) * 0.08f; // small hue wobble
            }
            else
            {
                s = MathF.Max(0f, s - 0.25f);
                v = MathF.Max(0f, v - 0.30f);
            }

            return HslToRayColor(h, s, v);
        }

        /// <summary>
        /// Resolve petal and center colors, respecting any explicit overrides in
        /// <paramref name="config"/> and using the seeded <paramref name="rnd"/> so
        /// results are reproducible.
        /// </summary>
        static (Color petal, Color center) ResolveColors(FlowerConfig config, Random rnd)
        {
            // Both colors explicitly provided — nothing to generate.
            if (config.PetalColor.HasValue && config.CenterColor.HasValue)
                return (config.PetalColor.Value, config.CenterColor.Value);

            // Pick a concrete scheme if Random was requested.
            PaletteScheme scheme = config.ColorScheme;
            if (scheme == PaletteScheme.Random)
            {
                var concreteSchemes = Enum.GetValues<PaletteScheme>()
                                         .Where(s => s != PaletteScheme.Random)
                                         .ToArray();
                scheme = concreteSchemes[rnd.Next(concreteSchemes.Length)];
            }

            float baseHue = SampleFlowerHue(rnd);

            bool allowSpikes = rnd.NextDouble() < 0.18;

            Color generatedPetal;
            Color generatedCenter;

            switch (scheme)
            {
                case PaletteScheme.Complementary:
                    {
                        float pSat = 0.65f + Rf(rnd) * 0.30f;
                        float pLit = 0.50f + Rf(rnd) * 0.20f;
                        float cHue = (baseHue + 0.50f) % 1f;
                        float cSat = 0.55f + Rf(rnd) * 0.30f;
                        float cLit = 0.25f + Rf(rnd) * 0.18f;

                        generatedPetal = ApplyOccasionalSpike(
                            HslToRayColor(baseHue, pSat, pLit),
                            rnd,
                            allowSpikes ? 0.12f : 0f
                        );

                        generatedCenter = ApplyOccasionalSpike(
                            HslToRayColor(cHue, cSat, cLit),
                            rnd,
                            allowSpikes ? 0.08f : 0f
                        );
                        break;
                    }

                case PaletteScheme.Analogous:
                    {
                        float offset = 0.05f + Rf(rnd) * 0.08f;
                        int sign = rnd.Next(2) == 0 ? 1 : -1;
                        float cHue = ((baseHue + sign * offset) % 1f + 1f) % 1f;

                        generatedPetal = ApplyOccasionalSpike(
                            HslToRayColor(baseHue, 0.70f + Rf(rnd) * 0.25f, 0.55f + Rf(rnd) * 0.15f),
                            rnd,
                            allowSpikes ? 0.10f : 0f
                        );

                        generatedCenter = ApplyOccasionalSpike(
                            HslToRayColor(cHue, 0.60f + Rf(rnd) * 0.25f, 0.28f + Rf(rnd) * 0.15f),
                            rnd,
                            allowSpikes ? 0.06f : 0f
                        );
                        break;
                    }

                case PaletteScheme.Triadic:
                    {
                        float cHue = (baseHue + 1f / 3f) % 1f;

                        generatedPetal = ApplyOccasionalSpike(
                            HslToRayColor(baseHue, 0.70f + Rf(rnd) * 0.25f, 0.55f + Rf(rnd) * 0.15f),
                            rnd,
                            allowSpikes ? 0.10f : 0f
                        );

                        generatedCenter = ApplyOccasionalSpike(
                            HslToRayColor(cHue, 0.65f + Rf(rnd) * 0.25f, 0.28f + Rf(rnd) * 0.20f),
                            rnd,
                            allowSpikes ? 0.06f : 0f
                        );
                        break;
                    }

                case PaletteScheme.SplitComplementary:
                    {
                        float flank = 0.07f + Rf(rnd) * 0.05f;
                        int sign = rnd.Next(2) == 0 ? 1 : -1;
                        float cHue = ((baseHue + 0.5f + sign * flank) % 1f + 1f) % 1f;

                        generatedPetal = ApplyOccasionalSpike(
                            HslToRayColor(baseHue, 0.70f + Rf(rnd) * 0.25f, 0.55f + Rf(rnd) * 0.15f),
                            rnd,
                            allowSpikes ? 0.10f : 0f
                        );

                        generatedCenter = ApplyOccasionalSpike(
                            HslToRayColor(cHue, 0.60f + Rf(rnd) * 0.30f, 0.28f + Rf(rnd) * 0.18f),
                            rnd,
                            allowSpikes ? 0.06f : 0f
                        );
                        break;
                    }

                case PaletteScheme.Monochromatic:
                    {
                        float pSat = 0.65f + Rf(rnd) * 0.25f;
                        float pLit = 0.55f + Rf(rnd) * 0.20f;
                        float cSat = Math.Min(1f, pSat + 0.12f);
                        float cLit = Math.Max(0.15f, pLit - 0.28f);

                        generatedPetal = ApplyOccasionalSpike(
                            HslToRayColor(baseHue, pSat, pLit),
                            rnd,
                            allowSpikes ? 0.08f : 0f
                        );

                        generatedCenter = ApplyOccasionalSpike(
                            HslToRayColor(baseHue, cSat, cLit),
                            rnd,
                            allowSpikes ? 0.05f : 0f
                        );
                        break;
                    }

                case PaletteScheme.Warm:
                    {
                        float pHue = Rf(rnd) * 0.167f;
                        float cHue = Rf(rnd) * 0.167f;

                        generatedPetal = ApplyOccasionalSpike(
                            HslToRayColor(pHue, 0.75f + Rf(rnd) * 0.20f, 0.52f + Rf(rnd) * 0.18f),
                            rnd,
                            allowSpikes ? 0.10f : 0f
                        );

                        generatedCenter = ApplyOccasionalSpike(
                            HslToRayColor(cHue, 0.65f + Rf(rnd) * 0.25f, 0.26f + Rf(rnd) * 0.15f),
                            rnd,
                            allowSpikes ? 0.06f : 0f
                        );
                        break;
                    }

                case PaletteScheme.Cool:
                    {
                        float pHue = 0.45f + Rf(rnd) * 0.30f;
                        float cHue = 0.45f + Rf(rnd) * 0.30f;

                        generatedPetal = ApplyOccasionalSpike(
                            HslToRayColor(pHue, 0.65f + Rf(rnd) * 0.25f, 0.55f + Rf(rnd) * 0.20f),
                            rnd,
                            allowSpikes ? 0.10f : 0f
                        );

                        generatedCenter = ApplyOccasionalSpike(
                            HslToRayColor(cHue, 0.55f + Rf(rnd) * 0.30f, 0.26f + Rf(rnd) * 0.18f),
                            rnd,
                            allowSpikes ? 0.06f : 0f
                        );
                        break;
                    }

                default:
                    {
                        generatedPetal = ApplyOccasionalSpike(
                            HslToRayColor(baseHue, 0.75f, 0.55f),
                            rnd,
                            allowSpikes ? 0.10f : 0f
                        );

                        generatedCenter = ApplyOccasionalSpike(
                            HslToRayColor((baseHue + 0.5f) % 1f, 0.65f, 0.28f),
                            rnd,
                            allowSpikes ? 0.06f : 0f
                        );
                        break;
                    }
            }

            return (
                config.PetalColor ?? generatedPetal,
                config.CenterColor ?? generatedCenter
            );
        }

        /// <summary>Convert HSL (all components 0–1) to a Raylib <see cref="Color"/>.</summary>
        static Color HslToRayColor(float h, float s, float l)
        {
            h = ((h % 1f) + 1f) % 1f;
            s = Math.Clamp(s, 0f, 1f);
            l = Math.Clamp(l, 0f, 1f);
            h = ClampFlowerHue(h);

            float c = (1f - MathF.Abs(2f * l - 1f)) * s;
            float x = c * (1f - MathF.Abs((h * 6f) % 2f - 1f));
            float m = l - c * 0.5f;

            float r, g, b;
            switch ((int)(h * 6f) % 6)
            {
                case 0: r = c; g = x; b = 0; break;
                case 1: r = x; g = c; b = 0; break;
                case 2: r = 0; g = c; b = x; break;
                case 3: r = 0; g = x; b = c; break;
                case 4: r = x; g = 0; b = c; break;
                default: r = c; g = 0; b = x; break;
            }

            return new Color(
                (int)((r + m) * 255f + 0.5f),
                (int)((g + m) * 255f + 0.5f),
                (int)((b + m) * 255f + 0.5f),
                255);
        }

        /// <summary>Shorthand for a [0, 1) float from <paramref name="rnd"/>.</summary>
        static float Rf(Random rnd) => (float)rnd.NextDouble();

        // ─── Flower geometry ─────────────────────────────────────────────────────

        // Colors are passed explicitly so they come from the already-resolved palette.
        static void CreateFlowerAtPosition(
            ShapeCollection collection, FlowerConfig config, Random rnd,
            Vector2 position, Color petalColor, Color centerColor)
        {
            // Center circle — morphs from a tiny dot to full size.
            int centerPoints = Math.Max(12, config.ResolutionPerPetal / 2);
            var centerSmall = CirclePoints(centerPoints, position, config.InnerRadius * 0.2f * config.Scale);
            var centerLarge = CirclePoints(centerPoints, position, config.InnerRadius * 0.9f * config.Scale);

            var centerSmallPath = new ShapePath(centerSmall, true,
                new Rendering.ShapeStyle().Filled().WithoutOutline().WithColor(centerColor), config.Layer);
            var centerLargePath = new ShapePath(centerLarge, true,
                new Rendering.ShapeStyle().Filled().WithoutOutline().WithColor(centerColor), config.Layer);

            var centerMorph = new Animation.ShapeMorph(centerSmallPath, centerLargePath).OptimizeCorrespondence();
            var centerAnim = centerMorph.Animate(config.CenterDuration).Ease(Animation.Easing.CubicInOut);
            collection.Add(centerAnim);
            centerAnim.Play();

            // Petals
            int petalCount = rnd.Next(config.MinPetals, config.MaxPetals + 1);

            for (int i = 0; i < petalCount; i++)
            {
                float baseAngle = i * MathF.Tau / petalCount;
                float jitterAngle = ((float)rnd.NextDouble() - 0.5f) * (MathF.Tau / petalCount) * 0.12f;
                float angle = baseAngle + jitterAngle;

                float scaledInner = config.InnerRadius * config.Scale;
                float scaledOuter = config.OuterRadius * config.Scale;

                Color variedPetalColor = VaryColor(petalColor, rnd, 0.18f);
                var petalLocal = BuildPetalLocal(angle, variedPetalColor, scaledInner, scaledOuter,
                                                    config.AngularSpread, config.ResolutionPerPetal,
                                                    config.Jitter, rnd);
                var petalAnchored = AnchorBaseCorner(petalLocal);
                var petalPath = TranslateBaseTo(petalAnchored, position);

                float budScale = MathF.Max(0.08f, 0.22f * config.Scale);
                var budLocal = BuildPetalLocal(angle, new Color(30, 160, 30, 255),
                                                  scaledInner * budScale, scaledOuter * budScale,
                                                  config.AngularSpread * 0.6f, config.ResolutionPerPetal,
                                                  config.Jitter * 0.7f, rnd);
                var budAnchored = AnchorBaseCorner(budLocal);
                var budPath = TranslateBaseTo(budAnchored, position);

                var morph = new Animation.ShapeMorph(budPath, petalPath).OptimizeCorrespondence();
                var anim = morph.Animate(config.PetalDuration).Ease(Animation.Easing.CubicInOut);
                collection.Add(anim);
                anim.Play();
            }
        }

        static void BuildStemRecursive(
            ShapeCollection collection, FlowerConfig config, Random rnd,
            Vector2 origin, float length, int layer, Vector2 parentDir,
            ref int remainingBranches,
            List<Vector2> flowerPositions, List<Vector2> occupiedPositions,
            int depth = 0, bool isInitialStem = false)
        {
            if (parentDir.LengthSquared() < 1e-6f) parentDir = new Vector2(0f, -1f);
            parentDir = Vector2.Normalize(parentDir);

            const int MAX_DEPTH = 4;
            const float MIN_DEG = 25f;
            const float MAX_DEG = 75f;
            const float DEG_TO_RAD = MathF.PI / 180f;

            if (depth >= MAX_DEPTH)
            {
                Vector2 adjustedPos = FindValidFlowerPosition(origin, occupiedPositions, config.OuterRadius * config.Scale * 2.2f, parentDir, rnd);
                if (Vector2.Distance(origin, adjustedPos) > 1f)
                    CreateConnectingStem(collection, config, layer, origin, adjustedPos, rnd);
                flowerPositions.Add(adjustedPos);
                occupiedPositions.Add(adjustedPos);
                return;
            }

            Vector2 pickDir()
            {
                if (isInitialStem)
                {
                    float a = ((float)rnd.NextDouble() - 0.5f) * 15f * DEG_TO_RAD;
                    float c = MathF.Cos(a), s = MathF.Sin(a);
                    return Vector2.Normalize(new Vector2(parentDir.X * c - parentDir.Y * s,
                                                        parentDir.X * s + parentDir.Y * c));
                }

                for (int attempt = 0; attempt < 12; attempt++)
                {
                    float angleDeg = (float)(MIN_DEG + rnd.NextDouble() * (MAX_DEG - MIN_DEG));
                    float signedAngle = (rnd.Next(2) == 0 ? 1 : -1) * angleDeg * DEG_TO_RAD;
                    float c = MathF.Cos(signedAngle), s = MathF.Sin(signedAngle);
                    var dir = new Vector2(parentDir.X * c - parentDir.Y * s,
                                         parentDir.X * s + parentDir.Y * c);

                    float perturb = ((float)rnd.NextDouble() - 0.5f) * 12f * DEG_TO_RAD;
                    c = MathF.Cos(perturb); s = MathF.Sin(perturb);
                    dir = Vector2.Normalize(new Vector2(dir.X * c - dir.Y * s, dir.X * s + dir.Y * c));

                    float dot = Math.Clamp(Vector2.Dot(dir, parentDir), -1f, 1f);
                    float angBetween = MathF.Acos(dot) * (180f / MathF.PI);

                    if (dir.Y < -0.01f && angBetween >= MIN_DEG - 0.001f && angBetween <= MAX_DEG + 0.001f)
                        return dir;
                }

                var fallback = new Vector2(parentDir.X * 0.5f, -MathF.Abs(parentDir.Y) - 0.3f);
                if (fallback.LengthSquared() < 1e-6f) fallback = new Vector2(0f, -1f);
                return Vector2.Normalize(fallback);
            }

            Vector2 dir = pickDir();
            var stemPoints = new List<Vector2> { origin };
            int segments = rnd.Next(3, 6);
            float segLen = MathF.Max(1f, length / segments);
            Vector2 current = origin;

            for (int i = 0; i < segments; i++)
            {
                var perp = new Vector2(-dir.Y, dir.X);
                float sideJitter = ((float)rnd.NextDouble() - 0.5f) * segLen * 0.35f;
                float forwardJitter = ((float)rnd.NextDouble() - 1.0f) * segLen * 0.12f;

                Vector2 step = dir * (segLen + forwardJitter) + perp * sideJitter;
                if (step.Y > 0f) step.Y *= -1f;
                current += step;
                stemPoints.Add(current);

                float smallRot = ((float)rnd.NextDouble() - 0.5f) * 6f * DEG_TO_RAD;
                float cc = MathF.Cos(smallRot), ss = MathF.Sin(smallRot);
                dir = Vector2.Normalize(new Vector2(dir.X * cc - dir.Y * ss, dir.X * ss + dir.Y * cc));
                if (dir.Y > -0.01f) dir = Vector2.Normalize(new Vector2(dir.X, -MathF.Abs(dir.Y) - 0.1f));
            }

            var stemColor = new Color(20, 120, 20, 255);
            float thickness = MathF.Max(2f, config.StemThickness * config.Scale);
            var stemStyle = new Rendering.ShapeStyle()
                .Filled().WithOutline(stemColor).WithColor(stemColor).WithThickness(thickness);

            var stemPath = new ShapePath(stemPoints.ToArray(), false, stemStyle, layer);
            var stub = new ShapePath(new[] { origin, origin }, false, stemStyle, layer);
            var morph = new Animation.ShapeMorph(stub, stemPath).OptimizeCorrespondence();
            var anim = morph.Animate(0.9f + (float)rnd.NextDouble() * 0.8f).Ease(Animation.Easing.CubicInOut);
            collection.Add(anim);
            anim.Play();

            if (rnd.NextDouble() < config.LeafChance && stemPoints.Count > 2)
            {
                int segIndex = Math.Max(1, rnd.Next(1, stemPoints.Count - 1));
                var leafAnchor = stemPoints[segIndex];
                var leaf = MakeLeaf(leafAnchor, rnd, config.Scale);
                var leafBud = BudCircle(leaf.Points.Count, leafAnchor, 2.8f * config.Scale, new Color(30, 160, 30, 255), layer);
                var leafMorph = new Animation.ShapeMorph(leafBud, leaf).OptimizeCorrespondence();
                var leafAnim = leafMorph.Animate(0.8f + (float)rnd.NextDouble() * 0.6f).Ease(Animation.Easing.CubicInOut);
                collection.Add(leafAnim);
                leafAnim.Play();
            }

            bool willBranch = remainingBranches > 0 && rnd.NextDouble() < config.BranchChance && depth < MAX_DEPTH;

            if (!willBranch)
            {
                Vector2 tipPos = stemPoints.Last();
                Vector2 adjustedPos = FindValidFlowerPosition(tipPos, occupiedPositions, config.OuterRadius * config.Scale * 2.2f, dir, rnd);
                if (Vector2.Distance(tipPos, adjustedPos) > 1f)
                    CreateConnectingStem(collection, config, layer, tipPos, adjustedPos, rnd);
                flowerPositions.Add(adjustedPos);
                occupiedPositions.Add(adjustedPos);
            }
            else
            {
                int branches = Math.Min(remainingBranches, rnd.Next(2, 4));
                remainingBranches -= branches;

                for (int b = 0; b < branches; b++)
                {
                    bool foundValid = false;

                    for (int attempt = 0; attempt < 30; attempt++)
                    {
                        Vector2 baseChildDir = pickDir();
                        Vector2 steeringDir = CalculateSteeringDirection(stemPoints.Last(), baseChildDir, occupiedPositions, config.OuterRadius * config.Scale * 2.2f);

                        float lengthVariation = 0.70f + (float)rnd.NextDouble() * 0.50f;
                        float childLen = MathF.Max(length * lengthVariation, config.StemLength * config.Scale * 0.25f);

                        Vector2 testEndpoint = EstimateBranchEndpoint(stemPoints.Last(), steeringDir, childLen, rnd, attempt);

                        if (!IsPositionTooClose(testEndpoint, occupiedPositions, config.OuterRadius * config.Scale * 2.2f))
                        {
                            BuildStemRecursive(collection, config, rnd, stemPoints.Last(), childLen, layer, steeringDir, ref remainingBranches, flowerPositions, occupiedPositions, depth + 1, false);
                            foundValid = true;
                            break;
                        }
                    }

                    if (!foundValid)
                    {
                        Vector2 bestDir = FindLeastCrowdedDirection(stemPoints.Last(), dir, occupiedPositions, rnd);
                        float extendedLen = length * 1.3f;
                        BuildStemRecursive(collection, config, rnd, stemPoints.Last(), extendedLen, layer, bestDir, ref remainingBranches, flowerPositions, occupiedPositions, depth + 1, false);
                    }
                }
            }
        }

        // ─── Spatial helpers ─────────────────────────────────────────────────────

        static Vector2 FindValidFlowerPosition(Vector2 startPos, List<Vector2> occupiedPositions, float minDistance, Vector2 currentDir, Random rnd)
        {
            if (!IsPositionTooClose(startPos, occupiedPositions, minDistance))
                return startPos;

            for (int i = 1; i <= 5; i++)
            {
                Vector2 testPos = startPos + currentDir * (minDistance * 0.5f * i);
                if (!IsPositionTooClose(testPos, occupiedPositions, minDistance))
                    return testPos;
            }

            for (int ring = 1; ring <= 3; ring++)
            {
                float radius = minDistance * 0.8f * ring;
                for (int angle = 0; angle < 12; angle++)
                {
                    float theta = angle * MathF.Tau / 12f;
                    var offset = new Vector2(MathF.Cos(theta), MathF.Sin(theta)) * radius;
                    if (offset.Y > 0) offset.Y *= -0.5f;
                    Vector2 testPos = startPos + offset;
                    if (!IsPositionTooClose(testPos, occupiedPositions, minDistance))
                        return testPos;
                }
            }

            return startPos + currentDir * minDistance * 2f;
        }

        static Vector2 CalculateSteeringDirection(Vector2 origin, Vector2 baseDir, List<Vector2> occupiedPositions, float avoidanceRadius)
        {
            Vector2 avoidanceForce = Vector2.Zero;

            foreach (var occupied in occupiedPositions)
            {
                Vector2 toOccupied = occupied - origin;
                float dist = toOccupied.Length();
                if (dist < avoidanceRadius * 2f && dist > 0.001f)
                {
                    float strength = 1f - (dist / (avoidanceRadius * 2f));
                    avoidanceForce -= Vector2.Normalize(toOccupied) * strength;
                }
            }

            Vector2 combinedDir = baseDir + avoidanceForce * 0.5f;
            if (combinedDir.Y > -0.01f)
                combinedDir = new Vector2(combinedDir.X, -MathF.Abs(combinedDir.Y) - 0.2f);

            return Vector2.Normalize(combinedDir);
        }

        static Vector2 FindLeastCrowdedDirection(Vector2 origin, Vector2 preferredDir, List<Vector2> occupiedPositions, Random rnd)
        {
            float bestScore = float.MinValue;
            Vector2 bestDir = preferredDir;

            for (int i = 0; i < 16; i++)
            {
                float angle = (i / 16f) * MathF.Tau;
                Vector2 testDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                if (testDir.Y >= 0) continue;

                float score = 0f;
                foreach (var occupied in occupiedPositions)
                    score += Vector2.Distance(origin + testDir * 100f, occupied);

                score += Vector2.Dot(testDir, preferredDir) * 50f;

                if (score > bestScore) { bestScore = score; bestDir = testDir; }
            }

            return bestDir;
        }

        static Vector2 EstimateBranchEndpoint(Vector2 origin, Vector2 baseDir, float length, Random rnd, int seed)
        {
            var testRnd = new Random(rnd.Next() + seed);
            int segments = testRnd.Next(3, 6);
            float segLen = length / segments;
            Vector2 current = origin;
            Vector2 dir = baseDir;

            for (int i = 0; i < segments; i++)
            {
                var perp = new Vector2(-dir.Y, dir.X);
                float sideJitter = ((float)testRnd.NextDouble() - 0.5f) * segLen * 0.35f;
                float forwardJitter = ((float)testRnd.NextDouble() - 1.0f) * segLen * 0.12f;

                Vector2 step = dir * (segLen + forwardJitter) + perp * sideJitter;
                if (step.Y > 0f) step.Y *= -1f;
                current += step;

                float smallRot = ((float)testRnd.NextDouble() - 0.5f) * 6f * (MathF.PI / 180f);
                float c = MathF.Cos(smallRot), s = MathF.Sin(smallRot);
                dir = Vector2.Normalize(new Vector2(dir.X * c - dir.Y * s, dir.X * s + dir.Y * c));
                if (dir.Y > -0.01f) dir = Vector2.Normalize(new Vector2(dir.X, -MathF.Abs(dir.Y) - 0.1f));
            }

            return current;
        }

        static bool IsPositionTooClose(Vector2 pos, List<Vector2> existingPositions, float minDistance)
        {
            float minDistSq = minDistance * minDistance;
            foreach (var existing in existingPositions)
                if (Vector2.DistanceSquared(pos, existing) < minDistSq) return true;
            return false;
        }

        static void CreateConnectingStem(ShapeCollection collection, FlowerConfig config, int layer, Vector2 from, Vector2 to, Random rnd)
        {
            var stemColor = new Color(20, 120, 20, 255);
            float thickness = MathF.Max(2f, config.StemThickness * config.Scale * 0.8f);
            var stemStyle = new Rendering.ShapeStyle()
                .Filled().WithOutline(stemColor).WithColor(stemColor).WithThickness(thickness);

            var stemPath = new ShapePath(new[] { from, to }, false, stemStyle, layer);
            var stub = new ShapePath(new[] { from, from }, false, stemStyle, layer);
            var morph = new Animation.ShapeMorph(stub, stemPath).OptimizeCorrespondence();
            var anim = morph.Animate(0.6f + (float)rnd.NextDouble() * 0.4f).Ease(Animation.Easing.CubicInOut);
            collection.Add(anim);
            anim.Play();
        }

        // ─── Shape helpers (unchanged) ───────────────────────────────────────────

        static ShapePath MakeLeaf(Vector2 anchor, Random rnd, float scale)
        {
            float length = (14f + (float)rnd.NextDouble() * 8f) * scale;
            float angle = (float)(rnd.NextDouble() * MathF.PI * 2.0);
            int resolution = 10;
            var pts = new List<Vector2>(resolution * 2);

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                float a = (t * 2f - 1f) * 0.6f;
                float r = length * (0.6f + 0.4f * (1f - MathF.Abs(a)));
                pts.Add(new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r));
            }
            for (int i = resolution - 1; i >= 0; i--)
            {
                float t = i / (float)(resolution - 1);
                float a = (t * 2f - 1f) * -0.6f;
                float r = length * (0.25f + 0.6f * (1f - MathF.Abs(a)));
                pts.Add(new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r));
            }

            var arr = pts.Select(p => Vector2.Transform(p, Matrix3x2.CreateRotation(angle))).ToArray();
            var style = new Rendering.ShapeStyle()
                .Filled()
                .WithOutline(Color.Black)
                .WithColor(new Color(30, 140, 40, 255))
                .WithThickness(1.2f * scale);

            var leafPath = new ShapePath(arr, true, style, 1);
            leafPath = AnchorBaseCorner(leafPath);
            leafPath = TranslateBaseTo(leafPath, anchor);
            return leafPath;
        }

        static ShapePath BuildPetalLocal(float angle, Color petalColor, float innerRadius, float outerRadius, float angularSpread, int resolution, float jitter, Random rnd)
        {
            float half = angularSpread * 0.5f;
            int outerCount = Math.Max(3, resolution);
            int innerCount = Math.Max(3, resolution / 2);
            var ptsList = new List<Vector2>(outerCount + innerCount);

            for (int i = 0; i < outerCount; i++)
            {
                float t = i / (float)(outerCount - 1);
                float theta = (t * 2f - 1f) * half;
                float sigma = MathF.Max(0.0001f, half * 0.5f);
                float bump = MathF.Exp(-(theta * theta) / (2f * sigma * sigma));
                float noise = 1f + ((float)rnd.NextDouble() - 0.5f) * jitter;
                float r = (innerRadius + (outerRadius - innerRadius) * bump) * noise;
                ptsList.Add(new Vector2(MathF.Cos(theta) * r, MathF.Sin(theta) * r));
            }

            for (int i = 0; i < innerCount; i++)
            {
                float t = i / (float)(innerCount - 1);
                float theta = (1f - t) * half * 2f - half;
                float noise = 1f + ((float)rnd.NextDouble() - 0.5f) * (jitter * 0.6f);
                float r = innerRadius * 0.45f * noise;
                ptsList.Add(new Vector2(MathF.Cos(theta) * r, MathF.Sin(theta) * r));
            }

            var points = ptsList.ToArray();
            var rot = Matrix3x2.CreateRotation(angle);
            for (int i = 0; i < points.Length; i++) points[i] = Vector2.Transform(points[i], rot);

            var style = new Rendering.ShapeStyle()
                .Filled()
                .WithOutline(Color.Black)
                .WithColor(petalColor)
                .WithThickness(2f);

            return new ShapePath(points, true, style, 1);
        }

        static ShapePath AnchorBaseCorner(ShapePath path)
        {
            if (path.Points.Count == 0) return path;

            int best = 0; float bestDist = float.MaxValue;
            for (int i = 0; i < path.Points.Count; i++)
            {
                var p = path.Points[i];
                float d = p.X * p.X + p.Y * p.Y;
                if (d < bestDist) { bestDist = d; best = i; }
            }

            var basePoint = path.Points[best];
            var translated = path.Points.Select(p => p - basePoint).ToArray();
            return new ShapePath(translated, path.IsClosed, path.Style, path.Layer);
        }

        static ShapePath TranslateBaseTo(ShapePath baseAnchoredPath, Vector2 target)
        {
            var pts = baseAnchoredPath.Points.Select(p => p + target).ToArray();
            return new ShapePath(pts, baseAnchoredPath.IsClosed, baseAnchoredPath.Style, baseAnchoredPath.Layer);
        }

        static ShapePath BudCircle(int pointCount, Vector2 center, float radius, Color color, int layer = 0)
        {
            var pts = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                float a = (i / (float)pointCount) * MathF.Tau;
                pts[i] = new Vector2(MathF.Cos(a) * radius, MathF.Sin(a) * radius);
            }

            var style = new Rendering.ShapeStyle().Filled().WithoutOutline().WithColor(color);
            return new ShapePath(pts, true, style, 2).WithCenter(center);
        }

        static Vector2[] CirclePoints(int pointCount, Vector2 center, float radius)
        {
            var pts = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                float a = (i / (float)pointCount) * MathF.Tau;
                pts[i] = center + new Vector2(MathF.Cos(a) * radius, MathF.Sin(a) * radius);
            }
            return pts;
        }
    }
}