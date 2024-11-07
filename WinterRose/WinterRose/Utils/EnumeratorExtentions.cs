using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// Provides extention features for Foreach loops
    /// </summary>
    public static class EnumeratorExtentions
    {
        /// <summary>
        /// Allows for a foreach loop to be used on <see cref="Range"/>
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static CustomIntEnumerator GetEnumerator(this Range range)
        {
            return new CustomIntEnumerator(range);
        }

        /// <summary>
        /// Allows for a foreach loop to be used on <see cref="int"/>
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static CustomIntEnumerator GetEnumerator(this int number)
        {
            return new CustomIntEnumerator(0, number);
        }

        /// <summary>
        /// Allows for enumerations on all types that implement <see cref="INumber{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="number"></param>
        /// <returns></returns>
        public static CustomINumberEnumerator<T> GetEnumerator<T>(this T number) where T : INumber<T>
        {
            return new CustomINumberEnumerator<T>(number);
        }
    }

    public class CustomINumberEnumerator<T> : IEnumerator<T> where T : INumber<T>
    {
        T current;
        T end;
        public CustomINumberEnumerator(T start)
        {
            current = T.Zero - T.One;
            end = start;
        }

        public T Current => current;

        object IEnumerator.Current => current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (current < end)
            {
                current++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            current = T.Zero;
        }
    }

    /// <summary>
    /// Provides enumerations on a <see cref="int"/> or <see cref="Range"/>
    /// </summary>
    public class CustomIntEnumerator
    {
        private int current;
        private readonly int end;
        private readonly bool fromEnd;

        /// <summary>
        /// Creates a new object of this struct
        /// </summary>
        /// <param name="range"></param>
        /// <exception cref="NotSupportedException"></exception>
        public CustomIntEnumerator(Range range) : this(range.Start.Value, range.End.Value) { }

        /// <summary>
        /// Creates a new object of this struct
        /// </summary>
        /// <param name="start">the start of the enumeration</param>
        /// <param name="end">The end of the enumeration. if <paramref name="end"/> is less than <paramref name="start"/> then the enumeration will go from end to start</param>
        public CustomIntEnumerator(int start, int end)
        {
            current = start - 1;

            if (fromEnd = end < start)
            {
                current = Math.Abs(end);
                this.end = start;
            }
                this.end = end - 1;
        }

        /// <summary>
        /// Moves the current value to the next position
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (fromEnd)
            {
                if (current > end)
                {
                    current--;
                    return true;
                }
            }
            else
            {
                if (current < end)
                {
                    current++;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// The current value
        /// </summary>
        public int Current { get => current; }
    }
}