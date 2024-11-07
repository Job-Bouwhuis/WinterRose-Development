using System;
using System.Collections.Generic;
using System.Linq;

namespace WinterRose
{
    /// <summary>
    /// A range of values that can be used to lerp between.
    /// </summary>
    /// <param name="points"></param>
    public struct ValueRange(List<ValueRangePoint> points)
    {
        /// <summary>
        /// All the points
        /// </summary>
        public readonly List<ValueRangePoint> Points => points;

        /// <summary>
        /// Adds the given point to the range
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(ValueRangePoint point)
        {
            List<ValueRangePoint> pnts = Points.ToList();
            pnts.Add(point);
            pnts.Sort((x, y) => x.Fraction.CompareTo(y.Fraction));
            points = [.. pnts];
        }
        /// <summary>
        /// Adds the given point to the range
        /// </summary>
        /// <param name="fraction"></param>
        /// <param name="value"></param>
        public void AddPoint(float fraction, float value)
        {
            points.Add((fraction, value));
        }
        /// <summary>
        /// Removes the point that exists on the given fraction. if no point exists at this fraction it does nothing
        /// </summary>
        /// <param name="fraction"></param>
        public void RemovePoint(float fraction)
        {
            int index = points.FindIndex(x => x.Fraction == fraction);
            if (index is -1)
                return;
            points.RemoveAt(index);
        }

        public float GetValue(float fraction)
        {
            if (points.Count == 0)
                throw new InvalidOperationException("No points available in the range.");

            if (fraction <= 0f)
                return points.First().Value;
            else if (fraction >= 1f)
                return points.Last().Value;
            else
            {
                // Find the two points between which the fraction lies
                ValueRangePoint lowerBound = points.Last(p => p.Fraction <= fraction);
                ValueRangePoint upperBound = points.First(p => p.Fraction >= fraction);

                // Perform linear interpolation between the two points
                float t = (fraction - lowerBound.Fraction) / (upperBound.Fraction - lowerBound.Fraction);
                return MathS.Lerp(lowerBound.Value, upperBound.Value, t);
            }
        }
    }
}
