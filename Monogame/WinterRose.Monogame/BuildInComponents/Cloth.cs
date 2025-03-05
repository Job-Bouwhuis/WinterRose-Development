using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace WinterRose.Monogame;

public class Cloth : ActiveRenderer
{
    public class Node
    {
        public Vector2 CurrentPosition { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public Vector2 Acceleration { get; set; }
        public bool IsLocked { get; set; }
        public Color? Color { get; set; }
        public Vector2 Damping { get; set; } = new(1);

        // Add per-node stiffness
        public float Stiffness { get; set; }

        public Node(Vector2 position, bool isLocked = false, float stiffness = 1.0f)
        {
            CurrentPosition = position;
            PreviousPosition = position;
            IsLocked = isLocked;
            Acceleration = Vector2.Zero;
            Stiffness = stiffness; // Default stiffness
        }

        public void Update()
        {
            if (IsLocked) return; // Don't move if the node is locked

            // Introduce damping factor
            Vector2 velocity = (CurrentPosition - PreviousPosition) * Damping;
            Vector2 newPosition = CurrentPosition + velocity + Acceleration * Time.deltaTime * Time.deltaTime;

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


    // Global stiffness property
    public float Stiffness
    {
        get => globalStiffness;
        set
        {
            globalStiffness = value;
            // Apply the global stiffness to all nodes
            foreach (var row in nodesGrid)
            {
                foreach (var node in row)
                {
                    node.Stiffness = globalStiffness;
                }
            }
        }
    }
    private float globalStiffness = 0.15f;

    [Show]
    private List<List<Node>> nodesGrid;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float NodeSpacing { get; private set; }
    public int ResolutionWidth { get; private set; }
    public int ResolutionHeight { get; private set; }
    public int Thickness { get; set; } = 1;
    public override TimeSpan DrawTime { get; protected set; }
    public float Damping
    {
        get => nodesGrid[0][0].Damping.X;
        set
        {
            foreach (var row in nodesGrid)
                foreach (var node in row)
                    node.Damping = new Vector2(value, value);
        }
    }

    public float LayerDepth { get; set; } = 0.5f;
    public Color Color { get; private set; } = Color.White;

    public Cloth(Vector2 initialPosition, int resolutionWidth, int resolutionHeight, float nodeSpacing)
    {
        ResolutionWidth = resolutionWidth;
        ResolutionHeight = resolutionHeight;
        NodeSpacing = nodeSpacing;
        nodesGrid = new List<List<Node>>();

        // Initialize nodes in a grid based on the initial position
        for (int y = 0; y < ResolutionHeight; y++)
        {
            var row = new List<Node>();
            for (int x = 0; x < ResolutionWidth; x++)
            {
                Vector2 position = initialPosition + new Vector2(x * NodeSpacing, y * NodeSpacing);
                bool isLocked = (y == 0 && (x == 0 || x == ResolutionWidth - 1)); // Lock top corners
                row.Add(new Node(position, isLocked));
            }
            nodesGrid.Add(row);
        }


        Stiffness = 0.15f;
    }


    public override RectangleF Bounds
    {
        get
        {
            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);

            foreach (var row in nodesGrid)
            {
                foreach (var node in row)
                {
                    var position = node.CurrentPosition;

                    if (position.X < min.X) min.X = position.X;
                    if (position.Y < min.Y) min.Y = position.Y;

                    if (position.X > max.X) max.X = position.X;
                    if (position.Y > max.Y) max.Y = position.Y;
                }
            }

            return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }
    }


    public void ApplyForceToAll(Vector2 force)
    {
        foreach (var row in nodesGrid)
            foreach (var node in row)
                node.ApplyForce(force);
    }

    protected override void Update()
    {
        foreach (var row in nodesGrid)
            foreach (var node in row)
                node.Update();

        SatisfyConstraints();
    }

    private void SatisfyConstraints()
    {
        for (int pass = 0; pass < 15; pass++)
        {
            for (int y = 0; y < nodesGrid.Count; y++)
            {
                for (int x = 0; x < nodesGrid[y].Count; x++)
                {
                    var node = nodesGrid[y][x];

                    // Apply vertical and horizontal constraints
                    if (y < nodesGrid.Count - 1)
                    {
                        var nodeBelow = nodesGrid[y + 1][x];
                        SatisfyNodeConstraint(node, nodeBelow);
                    }
                    if (x < nodesGrid[y].Count - 1)
                    {
                        var nodeRight = nodesGrid[y][x + 1];
                        SatisfyNodeConstraint(node, nodeRight);
                    }

                    // Apply diagonal constraints (optional but improves stability)
                    if (y < nodesGrid.Count - 1 && x < nodesGrid[y].Count - 1)
                    {
                        var nodeDiagonal = nodesGrid[y + 1][x + 1];
                        SatisfyNodeConstraint(node, nodeDiagonal);
                    }

                    if (y < nodesGrid.Count - 1 && x > 0)
                    {
                        var nodeDiagonalLeft = nodesGrid[y + 1][x - 1];
                        SatisfyNodeConstraint(node, nodeDiagonalLeft);
                    }
                }
            }
        }
    }

    private void SatisfyNodeConstraint(Node nodeA, Node nodeB)
    {
        Vector2 delta = nodeB.CurrentPosition - nodeA.CurrentPosition;
        float distance = delta.Length();

        // Calculate excess distance
        float excessDistance = distance - NodeSpacing;

        // Use the average of both node's stiffness for constraint calculation
        float avgStiffness = (nodeA.Stiffness + nodeB.Stiffness) / 2;

        // Adjustment based on stiffness
        Vector2 adjustment = -Vector2.Normalize(delta) * (excessDistance * avgStiffness * 0.5f);

        adjustment = adjustment with { X = (float)Math.Round(adjustment.X, 4), Y = (float)Math.Round(adjustment.Y, 4) };

        if (nodeA.IsLocked && !nodeB.IsLocked)
        {
            nodeB.CurrentPosition += adjustment;
            return;
        }

        if (nodeB.IsLocked && !nodeA.IsLocked)
        {
            nodeA.CurrentPosition -= adjustment;
            return;
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


    private float lastMaxExcessLength = 0f;
    public bool DebugMode { get; set; } = true;

    public override void Render(SpriteBatch batch)
    {
        Stopwatch sw = Stopwatch.StartNew();

        float maxExcessLength = 0f;

        if (DebugMode)
        {
            for (int y = 0; y < ResolutionHeight; y++)
            {
                for (int x = 0; x < ResolutionWidth - 1; x++)
                {
                    Vector2 start = nodesGrid[y][x].CurrentPosition;
                    Vector2 end = nodesGrid[y][x + 1].CurrentPosition;

                    float distance = Vector2.Distance(start, end);
                    float tooLong = distance - NodeSpacing;

                    if (tooLong > maxExcessLength)
                    {
                        maxExcessLength = tooLong;
                    }
                }
            }

            for (int x = 0; x < ResolutionWidth; x++)
            {
                for (int y = 0; y < ResolutionHeight - 1; y++)
                {
                    Vector2 start = nodesGrid[y][x].CurrentPosition;
                    Vector2 end = nodesGrid[y + 1][x].CurrentPosition;

                    float distance = Vector2.Distance(start, end);
                    float tooLong = distance - NodeSpacing;

                    if (tooLong > maxExcessLength)
                    {
                        maxExcessLength = tooLong;
                    }
                }
            }
        }

        float referenceMaxExcess = DebugMode && lastMaxExcessLength > 0 ? lastMaxExcessLength : maxExcessLength;

        for (int y = 0; y < ResolutionHeight; y++)
        {
            for (int x = 0; x < ResolutionWidth - 1; x++)
            {
                Vector2 start = nodesGrid[y][x].CurrentPosition;
                Vector2 end = nodesGrid[y][x + 1].CurrentPosition;

                float distance = Vector2.Distance(start, end);
                float tooLong = distance - NodeSpacing;

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
                    segmentColor = nodesGrid[y][x].Color ?? Color;
                }

                batch.DrawLine(start, end, segmentColor, Thickness, LayerDepth);
            }
        }

        // Render vertical lines
        for (int x = 0; x < ResolutionWidth; x++)
        {
            for (int y = 0; y < ResolutionHeight - 1; y++)
            {
                Vector2 start = nodesGrid[y][x].CurrentPosition;
                Vector2 end = nodesGrid[y + 1][x].CurrentPosition;

                float distance = Vector2.Distance(start, end);
                float tooLong = distance - NodeSpacing;

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
                    segmentColor = nodesGrid[y][x].Color ?? Color;
                }

                batch.DrawLine(start, end, segmentColor, Thickness, LayerDepth);
            }
        }

        if (DebugMode)
        {
            lastMaxExcessLength = maxExcessLength;
        }

        sw.Stop();

        DrawTime = sw.Elapsed;
    }

}
