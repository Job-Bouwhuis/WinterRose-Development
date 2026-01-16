using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace WinterRose.ForgeWarden.Geometry
{
    public static class GeometricFlowerBuilder
    {
        public class FlowerConfig
        {
            public Color PetalColor { get; init; } = Color.Yellow;
            public Color CenterColor { get; init; } = new Color(139, 69, 19, 255); // Brown center
            public Vector2 Center { get; init; } = Vector2.Zero;
            public int MinPetals { get; init; } = 5;
            public int MaxPetals { get; init; } = 9;
            public int MinStems { get; init; } = 8;  // Increased minimum
            public int MaxStems { get; init; } = 70;
            public float InnerRadius { get; init; } = 6f;
            public float OuterRadius { get; init; } = 36f;
            public float AngularSpread { get; init; } = MathF.PI * 0.45f;
            public int ResolutionPerPetal { get; init; } = 20;
            public float Jitter { get; init; } = 0.06f;
            public float PetalDuration { get; init; } = 4;
            public float CenterDuration { get; init; } = 1.0f;
            public int? Seed { get; init; } = null;
            public int Layer { get; init; } = 0;
            public float StemLength { get; init; } = 120f;
            public float StemThickness { get; init; } = 4f;
            public float BranchChance { get; init; } = 0.85f;  // High chance to branch
            public float LeafChance { get; init; } = 0.5f;
            public float Scale { get; init; } = 1f;
        }

        public static ShapeCollection Flower(FlowerConfig config)
        {
            var rnd = config.Seed.HasValue ? new Random(config.Seed.Value) : new Random();
            var collection = new ShapeCollection();

            // stems: create a single main stem and allow branching
            int targetStems = rnd.Next(config.MinStems, config.MaxStems + 1);
            int remainingBranches = Math.Max(0, targetStems - 1);
            Vector2 initialDir = new Vector2(0f, -1f); // Point upward
            List<Vector2> flowerPositions = new List<Vector2>();
            List<Vector2> occupiedPositions = new List<Vector2>(); // Track flower positions to avoid overlap
            BuildStemRecursive(collection, config, rnd, config.Center, config.StemLength * config.Scale, config.Layer, initialDir, ref remainingBranches, flowerPositions, occupiedPositions, isInitialStem: true);

            // Now create flowers at each branch tip
            foreach (var flowerPos in flowerPositions)
            {
                CreateFlowerAtPosition(collection, config, rnd, flowerPos);
            }

            return collection;
        }

        // FIXED: Separated flower creation so it can be placed at branch tips
        static void CreateFlowerAtPosition(ShapeCollection collection, FlowerConfig config, Random rnd, Vector2 position)
        {
            // center circle (will animate from small to full)
            int centerPoints = Math.Max(12, config.ResolutionPerPetal / 2);
            var centerSmall = CirclePoints(centerPoints, position, config.InnerRadius * 0.2f * config.Scale);
            var centerLarge = CirclePoints(centerPoints, position, config.InnerRadius * 0.9f * config.Scale);

            var centerSmallPath = new ShapePath(centerSmall, true,
                new Rendering.ShapeStyle().Filled()
                .WithoutOutline().WithColor(config.CenterColor), config.Layer); // FIXED: Use CenterColor

            var centerLargePath = new ShapePath(centerLarge, true,
                new Rendering.ShapeStyle().Filled()
                .WithoutOutline().WithColor(config.CenterColor), config.Layer); // FIXED: Use CenterColor

            var centerMorph = new Animation.ShapeMorph(centerSmallPath, centerLargePath).OptimizeCorrespondence();
            var centerAnim = centerMorph.Animate(config.CenterDuration).Ease(Animation.Easing.CubicInOut);
            collection.Add(centerAnim);
            centerAnim.Play();

            // petals
            int petalCount = rnd.Next(config.MinPetals, config.MaxPetals + 1);

            for (int i = 0; i < petalCount; i++)
            {
                float baseAngle = i * MathF.Tau / petalCount;
                float jitterAngle = ((float)rnd.NextDouble() - 0.5f) * (MathF.Tau / petalCount) * 0.12f;
                float angle = baseAngle + jitterAngle;

                float scaledInner = config.InnerRadius * config.Scale;
                float scaledOuter = config.OuterRadius * config.Scale;

                // final petal local (base near origin)
                var petalLocal = BuildPetalLocal(
                    angle: angle,
                    petalColor: config.PetalColor,
                    innerRadius: scaledInner,
                    outerRadius: scaledOuter,
                    angularSpread: config.AngularSpread,
                    resolution: config.ResolutionPerPetal,
                    jitter: config.Jitter,
                    rnd: rnd);

                // anchor the base corner at origin
                var petalAnchored = AnchorBaseCorner(petalLocal);

                // translate anchored base to flower position
                var petalPath = TranslateBaseTo(petalAnchored, position);

                // bud = scaled copy of same petal geometry but green and smaller
                float budScale = MathF.Max(0.08f, 0.22f * config.Scale);
                var budLocal = BuildPetalLocal(
                    angle: angle,
                    petalColor: new Color(30, 160, 30, 255),
                    innerRadius: scaledInner * budScale,
                    outerRadius: scaledOuter * budScale,
                    angularSpread: config.AngularSpread * 0.6f,
                    resolution: config.ResolutionPerPetal,
                    jitter: config.Jitter * 0.7f,
                    rnd: rnd);

                var budAnchored = AnchorBaseCorner(budLocal);
                var budPath = TranslateBaseTo(budAnchored, position);

                // morph
                var morph = new Animation.ShapeMorph(budPath, petalPath).OptimizeCorrespondence();
                var anim = morph.Animate(config.PetalDuration).Ease(Animation.Easing.CubicInOut);

                collection.Add(anim);
                anim.Play();
            }
        }

        static void BuildStemRecursive(ShapeCollection collection, FlowerConfig config, Random rnd, Vector2 origin, float length, int layer, Vector2 parentDir, ref int remainingBranches, List<Vector2> flowerPositions, List<Vector2> occupiedPositions, int depth = 0, bool isInitialStem = false)
        {
            // default parent direction = up
            if (parentDir.LengthSquared() < 1e-6f) parentDir = new Vector2(0f, -1f);
            parentDir = Vector2.Normalize(parentDir);

            // Limit recursion depth to prevent tiny branches
            const int MAX_DEPTH = 4;
            if (depth >= MAX_DEPTH)
            {
                // Terminal branch - must find a valid position for the flower
                Vector2 adjustedPos = FindValidFlowerPosition(origin, occupiedPositions, config.OuterRadius * config.Scale * 2.2f, parentDir, rnd);

                // If position was adjusted, create a connecting stem
                if (Vector2.Distance(origin, adjustedPos) > 1f)
                {
                    CreateConnectingStem(collection, config, layer, origin, adjustedPos, rnd);
                }

                flowerPositions.Add(adjustedPos);
                occupiedPositions.Add(adjustedPos);
                return;
            }

            const float MIN_DEG = 25f;
            const float MAX_DEG = 75f;
            const float DEG_TO_RAD = MathF.PI / 180f;

            Vector2 pickDir()
            {
                // For initial stem, constrain to near-vertical
                if (isInitialStem)
                {
                    float smallAngle = ((float)rnd.NextDouble() - 0.5f) * 15f * DEG_TO_RAD;
                    float c = MathF.Cos(smallAngle);
                    float s = MathF.Sin(smallAngle);
                    var dir = new Vector2(parentDir.X * c - parentDir.Y * s, parentDir.X * s + parentDir.Y * c);
                    return Vector2.Normalize(dir);
                }

                for (int attempt = 0; attempt < 12; attempt++)
                {
                    float angleDeg = (float)(MIN_DEG + rnd.NextDouble() * (MAX_DEG - MIN_DEG));
                    float angleRad = angleDeg * DEG_TO_RAD;

                    int sign = rnd.Next(0, 2) == 0 ? 1 : -1;
                    float signedAngle = sign * angleRad;

                    float c = MathF.Cos(signedAngle);
                    float s = MathF.Sin(signedAngle);
                    var dir = new Vector2(parentDir.X * c - parentDir.Y * s, parentDir.X * s + parentDir.Y * c);

                    float perturb = ((float)rnd.NextDouble() - 0.5f) * 12f * DEG_TO_RAD;
                    c = MathF.Cos(perturb); s = MathF.Sin(perturb);
                    dir = new Vector2(dir.X * c - dir.Y * s, dir.X * s + dir.Y * c);

                    dir = Vector2.Normalize(dir);

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
            float segLen = MathF.Max(1f, length / (float)segments);
            Vector2 current = origin;
            for (int i = 0; i < segments; i++)
            {
                var perp = new Vector2(-dir.Y, dir.X);
                float along = segLen;
                float sideJitter = (float)(rnd.NextDouble() - 0.5) * segLen * 0.35f;
                float forwardJitter = (float)(rnd.NextDouble() - 1.0) * segLen * 0.12f;

                Vector2 step = dir * (along + forwardJitter) + perp * sideJitter;
                if (step.Y > 0f) step.Y *= -1f;
                current += step;
                stemPoints.Add(current);

                float smallRot = ((float)rnd.NextDouble() - 0.5f) * 6f * (MathF.PI / 180f);
                float cc = MathF.Cos(smallRot), ss = MathF.Sin(smallRot);
                dir = new Vector2(dir.X * cc - dir.Y * ss, dir.X * ss + dir.Y * cc);
                dir = Vector2.Normalize(dir);
                if (dir.Y > -0.01f) dir = Vector2.Normalize(new Vector2(dir.X, -MathF.Abs(dir.Y) - 0.1f));
            }

            // create stem path
            var stemColor = new Color(20, 120, 20, 255);
            float thickness = MathF.Max(2f, config.StemThickness * config.Scale);
            var stemStyle = new Rendering.ShapeStyle().Filled().WithOutline(stemColor).WithColor(stemColor).WithThickness(thickness);
            var stemPath = new ShapePath(stemPoints.ToArray(), false, stemStyle, layer);

            var stub = new ShapePath(new[] { origin, origin }, false, stemStyle, layer);
            var morph = new Animation.ShapeMorph(stub, stemPath).OptimizeCorrespondence();
            var anim = morph.Animate(0.9f + (float)rnd.NextDouble() * 0.8f).Ease(Animation.Easing.CubicInOut);
            collection.Add(anim);
            anim.Play();

            // leaf chance on a random interior segment
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

            // Determine if this branch should have a flower
            bool willBranch = remainingBranches > 0 && rnd.NextDouble() < config.BranchChance && depth < MAX_DEPTH;

            if (!willBranch)
            {
                // Terminal branch - find valid position for flower
                Vector2 tipPos = stemPoints.Last();
                Vector2 adjustedPos = FindValidFlowerPosition(tipPos, occupiedPositions, config.OuterRadius * config.Scale * 2.2f, dir, rnd);

                // If position was adjusted, create a connecting stem
                if (Vector2.Distance(tipPos, adjustedPos) > 1f)
                {
                    CreateConnectingStem(collection, config, layer, tipPos, adjustedPos, rnd);
                }

                flowerPositions.Add(adjustedPos);
                occupiedPositions.Add(adjustedPos);
            }
            else
            {
                // This branch will branch further
                int branches = Math.Min(remainingBranches, rnd.Next(2, 4));
                remainingBranches -= branches;

                for (int b = 0; b < branches; b++)
                {
                    // Try to find a valid branch configuration that steers away from occupied positions
                    bool foundValidBranch = false;

                    for (int attempt = 0; attempt < 30; attempt++)
                    {
                        // Calculate direction that steers away from occupied positions
                        Vector2 baseChildDir = pickDir();
                        Vector2 steeringDir = CalculateSteeringDirection(stemPoints.Last(), baseChildDir, occupiedPositions, config.OuterRadius * config.Scale * 2.2f);

                        // Try different lengths - allow going longer to find space
                        float lengthVariation = 0.70f + (float)rnd.NextDouble() * 0.50f; // 70-120% of parent
                        float childLen = length * lengthVariation;
                        // Be more permissive with minimum length
                        childLen = MathF.Max(childLen, config.StemLength * config.Scale * 0.25f);

                        // Calculate approximate endpoint
                        Vector2 testEndpoint = EstimateBranchEndpoint(stemPoints.Last(), steeringDir, childLen, rnd, attempt);

                        // Check if this endpoint is valid
                        if (!IsPositionTooClose(testEndpoint, occupiedPositions, config.OuterRadius * config.Scale * 2.2f))
                        {
                            // Valid position found! Create the branch
                            BuildStemRecursive(collection, config, rnd, stemPoints.Last(), childLen, layer, steeringDir, ref remainingBranches, flowerPositions, occupiedPositions, depth + 1, false);
                            foundValidBranch = true;
                            break;
                        }
                    }

                    // If we still couldn't find a valid position, force one by extending far away
                    if (!foundValidBranch)
                    {
                        // Last resort: create a longer branch that goes in the least crowded direction
                        Vector2 bestDir = FindLeastCrowdedDirection(stemPoints.Last(), dir, occupiedPositions, rnd);
                        float extendedLen = length * 1.3f; // 30% longer
                        BuildStemRecursive(collection, config, rnd, stemPoints.Last(), extendedLen, layer, bestDir, ref remainingBranches, flowerPositions, occupiedPositions, depth + 1, false);
                    }
                }
            }
        }

        // Find a valid position for a flower by adjusting away from occupied positions
        static Vector2 FindValidFlowerPosition(Vector2 startPos, List<Vector2> occupiedPositions, float minDistance, Vector2 currentDir, Random rnd)
        {
            // If position is already valid, use it
            if (!IsPositionTooClose(startPos, occupiedPositions, minDistance))
                return startPos;

            // Try to move along current direction
            for (int i = 1; i <= 5; i++)
            {
                float extension = minDistance * 0.5f * i;
                Vector2 testPos = startPos + currentDir * extension;
                if (!IsPositionTooClose(testPos, occupiedPositions, minDistance))
                    return testPos;
            }

            // Try radial search around the position
            for (int ring = 1; ring <= 3; ring++)
            {
                float radius = minDistance * 0.8f * ring;
                for (int angle = 0; angle < 12; angle++)
                {
                    float theta = angle * MathF.Tau / 12f;
                    Vector2 offset = new Vector2(MathF.Cos(theta), MathF.Sin(theta)) * radius;
                    // Prefer upward directions
                    if (offset.Y > 0) offset.Y *= -0.5f;
                    Vector2 testPos = startPos + offset;
                    if (!IsPositionTooClose(testPos, occupiedPositions, minDistance))
                        return testPos;
                }
            }

            // Last resort: move far away in current direction
            return startPos + currentDir * minDistance * 2f;
        }

        // Calculate a steering direction that moves away from occupied positions
        static Vector2 CalculateSteeringDirection(Vector2 origin, Vector2 baseDir, List<Vector2> occupiedPositions, float avoidanceRadius)
        {
            Vector2 avoidanceForce = Vector2.Zero;

            foreach (var occupied in occupiedPositions)
            {
                Vector2 toOccupied = occupied - origin;
                float dist = toOccupied.Length();

                if (dist < avoidanceRadius * 2f && dist > 0.001f)
                {
                    // Push away from occupied positions
                    float strength = 1f - (dist / (avoidanceRadius * 2f));
                    avoidanceForce -= Vector2.Normalize(toOccupied) * strength;
                }
            }

            // Combine base direction with avoidance
            Vector2 combinedDir = baseDir + avoidanceForce * 0.5f;

            // Ensure it stays generally upward
            if (combinedDir.Y > -0.01f)
                combinedDir = new Vector2(combinedDir.X, -MathF.Abs(combinedDir.Y) - 0.2f);

            return Vector2.Normalize(combinedDir);
        }

        // Find the direction with the least crowding
        static Vector2 FindLeastCrowdedDirection(Vector2 origin, Vector2 preferredDir, List<Vector2> occupiedPositions, Random rnd)
        {
            float bestScore = float.MinValue;
            Vector2 bestDir = preferredDir;

            // Test 16 directions
            for (int i = 0; i < 16; i++)
            {
                float angle = (i / 16f) * MathF.Tau;
                Vector2 testDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

                // Only consider upward directions
                if (testDir.Y >= 0) continue;

                // Score based on distance to occupied positions
                float score = 0f;
                foreach (var occupied in occupiedPositions)
                {
                    Vector2 futurePos = origin + testDir * 100f; // Project forward
                    float dist = Vector2.Distance(futurePos, occupied);
                    score += dist; // Higher score = further from obstacles
                }

                // Bonus for directions close to preferred
                float alignment = Vector2.Dot(testDir, preferredDir);
                score += alignment * 50f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = testDir;
                }
            }

            return bestDir;
        }

        // Estimate where a branch would end up (approximate calculation for collision detection)
        static Vector2 EstimateBranchEndpoint(Vector2 origin, Vector2 baseDir, float length, Random rnd, int seed)
        {
            // Use seed to make this deterministic for a given attempt
            var testRnd = new Random(rnd.Next() + seed);

            int segments = testRnd.Next(3, 6);
            float segLen = length / segments;
            Vector2 current = origin;
            Vector2 dir = baseDir;

            for (int i = 0; i < segments; i++)
            {
                var perp = new Vector2(-dir.Y, dir.X);
                float along = segLen;
                float sideJitter = ((float)testRnd.NextDouble() - 0.5f) * segLen * 0.35f;
                float forwardJitter = ((float)testRnd.NextDouble() - 1.0f) * segLen * 0.12f;

                Vector2 step = dir * (along + forwardJitter) + perp * sideJitter;
                if (step.Y > 0f) step.Y *= -1f;
                current += step;

                float smallRot = ((float)testRnd.NextDouble() - 0.5f) * 6f * (MathF.PI / 180f);
                float c = MathF.Cos(smallRot), s = MathF.Sin(smallRot);
                dir = new Vector2(dir.X * c - dir.Y * s, dir.X * s + dir.Y * c);
                dir = Vector2.Normalize(dir);
                if (dir.Y > -0.01f) dir = Vector2.Normalize(new Vector2(dir.X, -MathF.Abs(dir.Y) - 0.1f));
            }

            return current;
        }

        // Helper method to check if a position is too close to existing flowers
        static bool IsPositionTooClose(Vector2 pos, List<Vector2> existingPositions, float minDistance)
        {
            float minDistSq = minDistance * minDistance;
            foreach (var existing in existingPositions)
            {
                float distSq = Vector2.DistanceSquared(pos, existing);
                if (distSq < minDistSq)
                    return true;
            }
            return false;
        }

        // Create a connecting stem between two points (for adjusted flower positions)
        static void CreateConnectingStem(ShapeCollection collection, FlowerConfig config, int layer, Vector2 from, Vector2 to, Random rnd)
        {
            var stemColor = new Color(20, 120, 20, 255);
            float thickness = MathF.Max(2f, config.StemThickness * config.Scale * 0.8f); // Slightly thinner
            var stemStyle = new Rendering.ShapeStyle().Filled().WithOutline(stemColor).WithColor(stemColor).WithThickness(thickness);

            // Create a simple 2-point stem
            var stemPoints = new Vector2[] { from, to };
            var stemPath = new ShapePath(stemPoints, false, stemStyle, layer);

            var stub = new ShapePath(new[] { from, from }, false, stemStyle, layer);
            var morph = new Animation.ShapeMorph(stub, stemPath).OptimizeCorrespondence();
            var anim = morph.Animate(0.6f + (float)rnd.NextDouble() * 0.4f).Ease(Animation.Easing.CubicInOut);
            collection.Add(anim);
            anim.Play();
        }

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
                var p = new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r);
                pts.Add(p);
            }
            for (int i = resolution - 1; i >= 0; i--)
            {
                float t = i / (float)(resolution - 1);
                float a = (t * 2f - 1f) * -0.6f;
                float r = length * (0.25f + 0.6f * (1f - MathF.Abs(a)));
                var p = new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r);
                pts.Add(p);
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
                float r = innerRadius + (outerRadius - innerRadius) * bump;
                r *= noise;
                float x = MathF.Cos(theta) * r;
                float y = MathF.Sin(theta) * r;
                ptsList.Add(new Vector2(x, y));
            }

            for (int i = 0; i < innerCount; i++)
            {
                float t = i / (float)(innerCount - 1);
                float theta = (1f - t) * half * 2f - half;
                float innerNoise = 1f + ((float)rnd.NextDouble() - 0.5f) * (jitter * 0.6f);
                float r = innerRadius * 0.45f * innerNoise;
                float x = MathF.Cos(theta) * r;
                float y = MathF.Sin(theta) * r;
                ptsList.Add(new Vector2(x, y));
            }

            var pts = ptsList.ToArray();

            var rot = Matrix3x2.CreateRotation(angle);
            for (int i = 0; i < pts.Length; i++) pts[i] = Vector2.Transform(pts[i], rot);

            var style = new Rendering.ShapeStyle()
                .Filled()
                .WithOutline(Color.Black)
                .WithColor(petalColor)
                .WithThickness(2f);

            return new ShapePath(pts, true, style, 1);
        }

        static ShapePath AnchorBaseCorner(ShapePath path)
        {
            if (path.Points.Count == 0) return path;

            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < path.Points.Count; i++)
            {
                var p = path.Points[i];
                float d = p.X * p.X + p.Y * p.Y;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            var pts = path.Points.ToArray();
            var translated = new Vector2[pts.Length];
            var basePoint = pts[best];
            for (int i = 0; i < pts.Length; i++)
                translated[i] = pts[i] - basePoint;

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
                float t = i / (float)pointCount;
                float a = t * MathF.Tau;
                pts[i] = new Vector2(MathF.Cos(a) * radius, MathF.Sin(a) * radius);
            }

            var style = new Rendering.ShapeStyle()
                .Filled()
                .WithoutOutline()
                .WithColor(color);

            return new ShapePath(pts, true, style, 2).WithCenter(center);
        }

        static Vector2[] CirclePoints(int pointCount, Vector2 center, float radius)
        {
            var pts = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / (float)pointCount;
                float a = t * MathF.Tau;
                pts[i] = center + new Vector2(MathF.Cos(a) * radius, MathF.Sin(a) * radius);
            }
            return pts;
        }
    }
}