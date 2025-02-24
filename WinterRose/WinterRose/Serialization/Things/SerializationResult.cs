using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Serialization
{
    /// <summary>
    /// The result of serializing using the <see cref="SnowSerializer"/>
    /// </summary>
    public readonly struct SerializationResult
    {
        /// <summary>
        /// Represents a empty serialization result with no value
        /// </summary>
        public static readonly SerializationResult Empty = new(null);

        /// <summary>
        /// Indicates whether the serialization was successful or not
        /// </summary>
        public readonly bool HasValue => result is not null && result.Length > 0;
        private readonly StringBuilder? result;

        /// <summary>
        /// Useless to use on your own.
        /// </summary>
        public SerializationResult() => throw new InvalidOperationException("This struct should not be constructed by a end user.");
        internal SerializationResult(StringBuilder? result) => this.result = result;
        /// <summary>
        /// Gets the result of the serialization as a string
        /// </summary>
        public string Result => result?.ToString() ?? "";
        /// <summary>
        /// Gets the raw result as a <see cref="StringBuilder"/>
        /// </summary>
        public StringBuilder? ResultRaw => result;
        /// <summary>
        /// Gets the result of the serialization as a string
        /// </summary>
        /// <returns><see cref="Result"/></returns>
        public override string ToString() => Result;
    }
}
