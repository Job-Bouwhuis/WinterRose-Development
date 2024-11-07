using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using WinterRose.SourceGeneration.Serialization;

namespace WinterRose.Monogame
{
    /// <summary>
    /// A struct that represents a 2D Vector with integer values.
    /// </summary>
    [DebuggerDisplay("X: {X} - Y: {Y}"), GenerateSerializer]
    public struct Vector2I
    {
        /// <summary>
        /// returns (0, 0)
        /// </summary>
        public static readonly Vector2I Zero = new();
        /// <summary>
        /// the X component
        /// </summary>
        public int X;
        /// <summary>
        /// the Y component
        /// </summary>
        public int Y;

        /// <summary>
        /// Creates a new Vector2I where the X and Y components are 0
        /// </summary>
        public Vector2I() : this(0) { }
        /// <summary>
        /// Creates a new Vector2I where the X and Y components are the same value
        /// </summary>
        /// <param name="xy"></param>
        public Vector2I(int xy) => X = Y = xy;
        /// <summary>
        /// Creates a new Vector2I where the X and Y components are set to the given values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2I(int x, int y) : this(x)
        {
            this.Y = y;
        }
        /// <summary>
        /// Creates a new Vector2I from a <see cref="Vector2"/>
        /// </summary>
        /// <param name="vec"></param>
        public Vector2I(Vector2 vec)
        {
            X = (int)vec.X;
            Y = (int)vec.Y;
        }

        /// <summary>
        /// Deconstructs the Vector2I into its X and Y components
        /// </summary>
        /// <returns></returns>
        public (int x, int y) Deconstruct() => (X, Y);
        /// <summary>
        /// Gets a string representation of this Vector2I
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }

        /// <summary>
        /// Check whether the values of this Vector2I lie within the bounds of the RectangleF
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public bool IsWithin(RectangleF bounds)
        {
            return bounds.Contains(this);
        }

        public static Vector2I operator +(Vector2I a, Vector2I b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2I operator -(Vector2I a, Vector2I b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2I operator *(Vector2I a, Vector2I b) => new(a.X * b.X, a.Y * b.Y);
        public static Vector2I operator /(Vector2I a, Vector2I b) => new(a.X / b.X, a.Y / b.Y);
        public static Vector2I operator %(Vector2I a, Vector2I b) => new(a.X % b.X, a.Y % b.Y);
        public static Vector2I operator +(Vector2I a, int b) => new(a.X + b, a.Y + b);
        public static Vector2I operator -(Vector2I a, int b) => new(a.X - b, a.Y - b);
        public static Vector2I operator *(Vector2I a, int b) => new(a.X * b, a.Y * b);
        public static Vector2I operator /(Vector2I a, int b) => new(a.X / b, a.Y / b);
        public static Vector2I operator %(Vector2I a, int b) => new(a.X % b, a.Y % b);
        public static bool operator ==(Vector2I a, Vector2I b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vector2I a, Vector2I b) => a.X != b.X || a.Y != b.Y;
        public static bool operator ==(Vector2I a, int b) => a.X == b && a.Y == b;
        public static bool operator !=(Vector2I a, int b) => a.X != b || a.Y != b;

        public static explicit operator Vector2I(Vector2 vec) => new(vec);
        public static explicit operator Vector2I(Point p) => new(p.X, p.Y);

        public static implicit operator Vector2(Vector2I vec) => new(vec.X, vec.Y);
        public static implicit operator Point(Vector2I vec) => new(vec.X, vec.Y);
        public static implicit operator WinterRose.Vectors.Vector2I(Vector2I vector2I) => new(vector2I.X, vector2I.Y);
    }
}
