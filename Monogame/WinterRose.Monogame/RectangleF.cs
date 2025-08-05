using Microsoft.Xna.Framework;
using System;

namespace WinterRose.Monogame
{
    /// <summary>
    /// A rectangle with float values
    /// </summary>
    public struct RectangleF
    {
        /// <summary>
        /// A <see cref="RectangleF"/> with all values set to 0
        /// </summary>
        public static readonly RectangleF Zero = new(0, 0, 0, 0);

        /// <summary>
        /// The width of this rectangle
        /// </summary>
        [WFInclude]
        public float Width { get; set; }
        /// <summary>
        /// The height of this rectangle
        /// </summary>
        [WFInclude]
        public float Height { get; set; }
        /// <summary>
        /// The X position of this rectangle
        /// </summary>
        [WFInclude]
        public float X { get; set; }
        /// <summary>
        /// The Y position of this rectangle
        /// </summary>
        [WFInclude]
        public float Y { get; set; }

        /// <summary>
        /// The left side of this rectangle
        /// </summary>
        public readonly float Left => X;
        /// <summary>
        /// The right side of this rectangle
        /// </summary>
        public readonly float Right => X + Width;
        /// <summary>
        /// The top side of this rectangle
        /// </summary>
        public readonly float Top => Y;
        /// <summary>
        /// The bottom side of this rectangle
        /// </summary>
        public readonly float Bottom => Y + Height;
        /// <summary>
        /// Whether this rectangle is <see cref="Zero"/> or not
        /// </summary>
        public readonly bool IsEmpty => Width == 0 && Height == 0 && X == 0 && Y == 0;

        /// <summary>
        /// The position of this rectangle
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        /// <summary>
        /// The size of this rectangle
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return new Vector2(Width, Height);
            }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        /// <summary>
        /// The center of this rectangle
        /// </summary>
        public readonly Vector2 Center => new Vector2(X + Width / 2, Y + Height / 2);

        /// <summary>
        /// Creates a new <see cref="RectangleF"/> with the given width and height
        /// </summary>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <param name="X">The X position</param>
        /// <param name="Y">The Y position</param>
        public RectangleF(float width, float height, float X, float Y)
        {
            Width = width;
            Height = height;
            this.X = X;
            this.Y = Y;
        }
        /// <summary>
        /// Creates a new empty <see cref="RectangleF"/>
        /// </summary>
        public RectangleF()
        {
            Width = 0;
            Height = 0;
            X = 0;
            Y = 0;
        }

        public RectangleF(Vector2 location, Vector2 size) : this()
        {
            X = location.X;
            Y = location.Y;
            Width = size.X;
            Height = size.Y;
        }

        /// <summary>
        /// Implicitly converts a <see cref="Rectangle"/> to a <see cref="RectangleF"/>
        /// </summary>
        /// <param name="r"></param>
        public static implicit operator RectangleF(Rectangle r)
        {
            return new(r.Width, r.Height, r.X, r.Y);
        }
        /// <summary>
        /// Implicitly converts a <see cref="RectangleF"/> to a <see cref="Rectangle"/>
        /// </summary>
        /// <param name="r"></param>
        public static implicit operator Rectangle(RectangleF r)
        {
            return new((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        }

        /// <summary>
        /// Does this rectangle contain the given point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public readonly bool Contains(int x, int y)
        {
            if (X <= x && x < X + Width && Y <= y)
            {
                return y < Y + Height;
            }

            return false;
        }
        /// <summary>
        /// Does this rectangle contain the given point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public readonly bool Contains(float x, float y)
        {
            if ((float)X <= x && x < (float)(X + Width) && (float)Y <= y)
            {
                return y < (float)(Y + Height);
            }

            return false;
        }
        /// <summary>
        /// Does this rectangle contain the given point
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public readonly bool Contains(Vector2I value)
        {
            if (X <= value.X && value.X < X + Width && Y <= value.Y)
            {
                return value.Y < Y + Height;
            }

            return false;
        }
        /// <summary>
        /// Does this rectangle contain the given point
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public readonly bool Contains(Vector2 value)
        {
            if ((float)X <= value.X && value.X < (float)(X + Width) && (float)Y <= value.Y)
            {
                return value.Y < (float)(Y + Height);
            }

            return false;
        }
        /// <summary>
        /// Does this rectangle contain the given rectangle
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public readonly bool Contains(RectangleF value)
        {
            if (X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y)
            {
                return value.Y + value.Height <= Y + Height;
            }

            return false;
        }
        /// <summary>
        /// inflates this rectangle by the given amount
        /// </summary>
        /// <param name="horizontalAmount"></param>
        /// <param name="verticalAmount"></param>
        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            X -= (int)horizontalAmount;
            Y -= (int)verticalAmount;
            Width += (int)horizontalAmount * 2;
            Height += (int)verticalAmount * 2;
        }
        /// <summary>
        /// Does this rectangle intersect with the given rectangle
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public readonly bool Intersects(RectangleF value)
        {
            if (value.Left < Right && Left < value.Right && value.Top < Bottom)
            {
                return Top < value.Bottom;
            }

            return false;
        }
        /// <summary>
        /// Whether rectangles <paramref name="a"/> and <paramref name="b"/> intersect
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static RectangleF Intersect(RectangleF a, RectangleF b)
        {
            Intersect(ref a, ref b, out var result);
            return result;
        }
        private static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
        {
            if (value1.Intersects(value2))
            {
                float num = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                float num2 = Math.Max(value1.X, value2.X);
                float num3 = Math.Max(value1.Y, value2.Y);
                float num4 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                result = new RectangleF(num2, num3, num - num2, num4 - num3);
            }
            else
            {
                result = new RectangleF(0, 0, 0, 0);
            }
        }
        /// <summary>
        /// Offsets this rectangles position by the given amount
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void Offset(float offsetX, float offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }
        /// <summary>
        /// Offsets this rectangles position by the given amount
        /// </summary>
        /// <param name="amount"></param>
        public void Offset(Vector2 amount)
        {
            X += amount.X;
            Y += amount.Y;
        }
        /// <summary>
        /// Gets a string representation of this rectangle
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";
        }
        /// <summary>
        /// Creates a new rectangle that is the union of <paramref name="value1"/> and <paramref name="value2"/>
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static RectangleF Union(RectangleF value1, RectangleF value2)
        {
            float num = Math.Min(value1.X, value2.X);
            float num2 = Math.Min(value1.Y, value2.Y);
            return new RectangleF(num, num2, Math.Max(value1.Right, value2.Right) - num, Math.Max(value1.Bottom, value2.Bottom) - num2);
        }
        /// <summary>
        /// Deconstructs this rectangle into its X, Y, Width and Height components
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Deconstruct(out float x, out float y, out float width, out float height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }
    }
}