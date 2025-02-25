using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace WinterRose.Monogame;

/// <summary>
/// Represents a 2d rope
/// </summary>
public class Rope : ActiveRenderer
{
    public class Node
    {
        public Vector2 CurrentPosition { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public Vector2 Acceleration { get; set; }
        public bool IsLocked { get; set; }
        public Color? Color { get; set; }
        public Vector2 Damping { get; set; } = new(1);

        public Node(Vector2 position, bool isLocked = false)
        {
            CurrentPosition = position;
            PreviousPosition = position;
            IsLocked = isLocked;
            Acceleration = Vector2.Zero;
        }

        public void Update()
        {
            if (IsLocked) return; // Don't move if the node is locked

            // Introduce damping factor
            Vector2 velocity = (CurrentPosition - PreviousPosition) * Damping;
            Vector2 newPosition = CurrentPosition + velocity + Acceleration * Time.SinceLastFrame * Time.SinceLastFrame;

            // Ensure CurrentPosition is valid
            if (CurrentPosition.X is float.NaN)
            {
                CurrentPosition = PreviousPosition;
                Acceleration = Vector2.Zero;
                return;
            }

            PreviousPosition = CurrentPosition;
            CurrentPosition = newPosition;

            Acceleration = Vector2.Zero; // Reset acceleration after each update
        }

        public void ApplyForce(Vector2 force)
        {
            if (IsLocked) return;
            Acceleration += force;
        }
    }

    private List<Node> nodes;
    public float SegmentLength { get; private set; }
    public int Resolution { get; private set; }
    public int Tickness { get; set; } = 1;
    public float Dampening
    {
        get => nodes[0].Damping.X;
        set
        {
            foreach (var node in nodes)
            {
                node.Damping = new(value, value);
            }
        }
    }

   /// <summary>
   /// The layerdepth used when rendering the rope.
   /// </summary>
    public float LayerDepth { get; set; }

    public Color Color { get; private set; } = Color.White;

    public Rope(Vector2 start, Vector2 end, int resolution, float elasticity)
    {
        // Don't interact with game engine here, only initialize component
        Resolution = resolution;
        SegmentLength = elasticity;
        nodes = [];

        // Generate nodes between start and end based on resolution
        for (int i = 0; i <= Resolution; i++)
        {
            Vector2 position = Vector2.Lerp(start, end, (float)i / Resolution);
            bool isLocked = (i == 0); // Lock the first node by default
            nodes.Add(new Node(position, isLocked));
        }

        Dampening = 0.995f;

    }

    public override RectangleF Bounds
    {
        get
        {
            Vector2 min = new(float.MaxValue, float.MaxValue); // Start with max values for min
            Vector2 max = new(float.MinValue, float.MinValue); // Start with min values for max

            // Iterate through all nodes to determine the bounds
            foreach (var node in nodes)
            {
                var position = node.CurrentPosition;

                // Update min and max based on the node's position
                if (position.X < min.X) min.X = position.X;
                if (position.Y < min.Y) min.Y = position.Y;

                if (position.X > max.X) max.X = position.X;
                if (position.Y > max.Y) max.Y = position.Y;
            }

            return new RectangleF(max.X - min.X, max.Y / 2, min.X, min.Y);
        }
    }

    public override TimeSpan DrawTime { get; protected set; }

    public Node[] Nodes => [.. nodes];

    protected override void Awake()
    {
        transform.position = nodes[0].CurrentPosition;
    }

    protected override void Update()
    {
        foreach (var node in nodes)
        {
            node.Update(); // Update node positions based on time step
        }

        SatisfyConstraints(SegmentLength);
    }

    private void SatisfyConstraints(float maxDistance)
    {
        for (int pass = 0; pass < 80; pass++) 
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var nodeA = nodes[i];
                var nodeB = nodes[i + 1];

                Vector2 delta = nodeB.CurrentPosition - nodeA.CurrentPosition;
                float distance = delta.Length();

                if (distance > maxDistance)
                {
                    float excessDistance = distance - maxDistance;

                    Vector2 adjustment = -Vector2.Normalize(delta) * (excessDistance * 0.5f);

                    adjustment = adjustment with { X = (float)Math.Round(adjustment.X, 4), Y = (float)Math.Round(adjustment.Y, 4) };


                    if (nodeA.IsLocked && !nodeB.IsLocked)
                    {
                        nodeB.CurrentPosition += adjustment;
                        continue;
                    }

                    if (nodeB.IsLocked && !nodeA.IsLocked)
                    {
                        nodeA.CurrentPosition -= adjustment;
                        continue;
                    }

                    if (!nodeA.IsLocked)
                    {
                        nodeA.CurrentPosition -= adjustment;
                    }

                    if (!nodeB.IsLocked)
                    {
                        nodeB.CurrentPosition += adjustment;
                    }
                }
            }
        }
    }


    private void SetNodeColor(Node node, float distance, float minDistance, float maxDistance)
    {
        float t = MathHelper.Clamp((distance - minDistance) / (maxDistance - minDistance), 0, 1);

        Color nodeColor = Color.Lerp(Color.Green, Color.Red, t);

        node.Color = nodeColor;
    }

    private float lastMaxExcessLength = 0f;
    public bool DebugMode { get; set; } = true; 

    public override void Render(SpriteBatch batch)
    {
        float maxExcessLength = 0f;

        if (DebugMode)
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Vector2 start = nodes[i].CurrentPosition;
                Vector2 end = nodes[i + 1].CurrentPosition;

                float distance = Vector2.Distance(start, end);
                float tooLong = distance - SegmentLength;

                if (tooLong > maxExcessLength)
                {
                    maxExcessLength = tooLong;
                }
            }
        }

        float referenceMaxExcess = DebugMode && lastMaxExcessLength > 0 ? lastMaxExcessLength : maxExcessLength;

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector2 start = nodes[i].CurrentPosition;
            Vector2 end = nodes[i + 1].CurrentPosition;

            float distance = Vector2.Distance(start, end);
            float tooLong = distance - SegmentLength;

            Color segmentColor;
            if (DebugMode)
            {
                if (tooLong <= 0)
                {
                    segmentColor = Color.Green;
                }
                else
                {
                    float stretchFactor = referenceMaxExcess > 0 ? tooLong / referenceMaxExcess : 0f;
                    segmentColor = Color.Lerp(Color.Green, Color.Red, stretchFactor);
                }
            }
            else
            {
                segmentColor = nodes[i].Color ?? Color;
            }

            batch.DrawLine(start, end, segmentColor, Tickness, LayerDepth);
        }

        if (DebugMode)
        {
            lastMaxExcessLength = maxExcessLength;
        }
    }

}

