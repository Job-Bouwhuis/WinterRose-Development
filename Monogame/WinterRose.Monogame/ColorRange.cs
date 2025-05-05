using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame
{
    /// <summary>
    /// A range of colors.
    /// </summary>
    public sealed class ColorRange
    {
        [IncludeWithSerialization]
        private List<ColorRangePoint> points;

        /// <summary>
        /// Creates a new empty color range.
        /// </summary>
        public ColorRange()
        {
            points = new();
        }

        /// <summary>
        /// Creates a new color range with the given points.
        /// </summary>
        /// <param name="points"></param>
        public ColorRange(ColorRangePoint[] points)
        {
            this.points = points.ToList();
        }

        /// <summary>
        /// Creates a new color range with the given points. If sort is true, the points will be sorted by fraction.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="sort"></param>
        public ColorRange(ColorRangePoint[] points, bool sort)
        {
            this.points = points.ToList();
            if (sort) this.points.Sort((x, y) => x.Fraction.CompareTo(y.Fraction));
        }

        /// <summary>
        /// The points in this color range.
        /// </summary>
        public List<ColorRangePoint> Points { get => points; set => points = value; }

        /// <summary>
        /// Gets the color lerped between the closest points to the given fraction.
        /// </summary>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public Color GetColor(float fraction)
        {
            if (points.Count == 0) return Color.White;
            if (points.Count == 1) return points[0].Color;

            int index = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Fraction > fraction)
                {
                    index = i;
                    break;
                }
            }

            if (index == 0) return points[0].Color;
            if (index == points.Count) return points[^1].Color;

            ColorRangePoint p1 = points[index - 1];
            ColorRangePoint p2 = points[index];

            float fractionBetween = (fraction - p1.Fraction) / (p2.Fraction - p1.Fraction);

            return Color.Lerp(p1.Color, p2.Color, fractionBetween);
        }

        /// <summary>
        /// Adds the given point to this color range.
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(ColorRangePoint point)
        {
            points.Add(point);
            points.Sort((x, y) => x.Fraction.CompareTo(y.Fraction));
        }

        /// <summary>
        /// Adds the given point to this color range.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="fraction"></param>
        public void AddPoint(Color color, float fraction)
        {
            AddPoint(new(color, fraction));
        }

        /// <summary>
        /// Removes the given point from this color range.
        /// </summary>
        /// <param name="point"></param>
        public void RemovePoint(ColorRangePoint point)
        {
            points.Remove(point);
        }

        /// <summary>
        /// Removes the given point from this color range.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="fraction"></param>
        public void RemovePoint(Color color, float fraction)
        {
            RemovePoint(new(color, fraction));
        }

        /// <summary>
        /// Clears all points from this color range.
        /// </summary>
        public void Clear()
        {
            points.Clear();
        }

        public static implicit operator ColorRange(ColorRangePoint[] points) => new(points);
    }

    /// <summary>
    /// A point in a <see cref="ColorRange"/>.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="fraction"></param>
    public class ColorRangePoint(Color color, float fraction)
    {
        /// <summary>
        /// The color at this fraction.
        /// </summary>
        [IncludeWithSerialization]
        public Color Color 
        {
            get => color;
            private set => color = value;
        }
        /// <summary>
        /// The fraction
        /// </summary>
        [IncludeWithSerialization]
        public float Fraction
        {
            get => fraction;
            private set => fraction = value;
        }

        public static implicit operator ColorRangePoint((Color color, float fraction) point) => new(point.color, point.fraction);
        private ColorRangePoint() : this(Color.Transparent, 0) { } // for serialization
    }
}
