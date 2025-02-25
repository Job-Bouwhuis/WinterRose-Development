using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WinterRose.Monogame
{
    public class Raycaster : Renderer
    {
        public struct RaycastHit
        {
            public Vector2 point;
            public Vector2 normal;
            public float distance;
            public float distanceRemaining;
        }
        public float MaxDistance { get; set; } = 50;
        public float MinRadius { get; set; } = 1f;
        public float MaxRadius { get; set; } = 5f;
        public float StepSize { get; set; } = 0.1f;

        private Vector2 lastDir;

        public override RectangleF Bounds { get; } = new();
        public override TimeSpan DrawTime { get; protected set; }

        private List<WorldObject> CurrentWorldObjects => world.Objects;

        private Vector2 hitPoint = new(float.NaN);

        protected override void Awake()
        {
            IsVisible = false;
        }

        public override void Render(SpriteBatch batch)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            if (hitPoint is not { X: float.NaN } and not { Y: float.NaN })
                batch.DrawCircle(hitPoint, 5, Color.Green); // Draw hit point (if any)

            batch.DrawLine(transform.position, transform.position + lastDir * MaxDistance, Color.Magenta);

            sw.Stop();
            DrawTime = sw.Elapsed;
        }

        public bool Raycast(Vector2 origin, Vector2 direction, float maxDistance, float stepSize, out RaycastHit hit)
        {
            lastDir = direction;
            MaxDistance = maxDistance;
            StepSize = stepSize;

            float totalDistance = 0;

            while (totalDistance < maxDistance)
            {
                Vector2 currentPosition = origin + direction * totalDistance;

                // Check collision
                if (CheckCollision(currentPosition))
                {
                    // calculate distance remaining to the max distance
                    float remainingDistance = maxDistance - totalDistance;
                    // calculate the distance from the start to the hit point
                    float distance = totalDistance - stepSize;
                    // calculate the normal of the hit point
                    Vector2 normal = Vector2.Normalize(currentPosition - origin);

                    hit = new RaycastHit
                    {
                        point = currentPosition,
                        normal = normal,
                        distance = distance,
                        distanceRemaining = remainingDistance
                    };

                    hitPoint = currentPosition;
                    return true;
                }

                totalDistance += stepSize;
            }
            hit = new();
            return false; // No collision within maxDistance
        }

        private bool CheckCollision(Vector2 position)
        {
            foreach (var obj in CurrentWorldObjects)
            {
                if (!obj.IsActive)
                    continue;

                if (obj == owner)
                    continue; // skip self

                foreach (Transform child in transform)
                    if (child.parent == obj.transform)
                        goto Continue;

                if (obj.TryFetchComponent(out SpriteRenderer sr))
                {
                    if (sr.IsVisible && sr.Enabled && sr.Bounds.Contains(position))
                        return true;
                }

Continue:;
            }

            return false;
        }
    }
}
