namespace WinterRose
{
    /// <summary>
    /// A point in a <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="fraction"></param>
    /// <param name="value"></param>
    [System.Diagnostics.DebuggerDisplay("{Fraction} > {Value}")]
    public readonly struct ValueRangePoint(float fraction, float value)
    {
        /// <summary>
        /// The fraction where the range will be exactly this value
        /// </summary>
        public float Fraction => fraction;
        /// <summary>
        /// The value
        /// </summary>
        public float Value => value;

        /// <summary>
        /// Implicitly converts a (float, float) tuple to a <see cref="ValueRangePoint"/>
        /// </summary>
        /// <param name="data"></param>
        public static implicit operator ValueRangePoint((float fraction, float value) data) => new(data.fraction, data.value);
        /// <summary>
        /// Deconstructs the <see cref="ValueRangePoint"/> into its values
        /// </summary>
        /// <param name="fraction"></param>
        /// <param name="value"></param>
        public void Deconstruct(out float fraction, out float value)
        {
            value = Value;
            fraction = Fraction;
        }
    }
}
