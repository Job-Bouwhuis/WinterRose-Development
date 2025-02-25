using System;
using WinterRose.SourceGeneration.Serialization;

namespace WinterRose
{
    /// <summary>
    /// An index using a float value.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{value} > {FromEnd}"), GenerateSerializer]
    public struct IndexF(float value, bool fromEnd)
    {
        public float value { get; set; } = MathF.Abs(value);
        public readonly bool FromEnd => fromEnd;

        public IndexF(float value) : this(value, value < 0) { }
        public IndexF() : this(0) { }

        public static implicit operator IndexF(float value) => new(value);
        public static implicit operator float(IndexF index) => index.value;
    }
}
