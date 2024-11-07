using Microsoft.CSharp.RuntimeBinder;
using System;
using WinterRose.Serialization;

namespace WinterRose.Serialization
{
    /// <summary>
    /// Holds the result of deserializing using the <see cref="SnowSerializer"/>
    /// </summary>
    public readonly struct DeserializationResult
    {
        private readonly dynamic? result;
        /// <summary>
        /// Gets whether the deserialization produced a value or not. if it did not produce a value, the deserialization failed
        /// </summary>
        public bool HasValue
        {
            get
            {
                return result is not null && !result.Equals(null);
            }
        }

        /// <summary>
        /// Gets the result for this deserialization operation. If the deserialization failed, this will be null
        /// </summary>
        public dynamic Result
        {
            get
            {
                try
                {
                    return result ?? default;
                }
                catch (RuntimeBinderException ex)
                {
                    throw new DeserializationFailedException("Something failed. This is most likely due to the class/struct you try to Deserialize being private or internal." +
                        " See inner exception for details", ex);
                }
                catch (Exception ex)
                {
                    throw new DeserializationFailedException("Something failed. See inner exception for details", ex);
                }
            }
        }

        /// <summary>
        /// Useless to use on your own.
        /// </summary>
        public DeserializationResult()
        {
            result = default;
        }

        /// <summary>
        /// Creates a new <see cref="DeserializationResult"/> with the given result
        /// </summary>
        /// <param name="result"></param>
        public DeserializationResult(dynamic result)
        {
            this.result = result;
        }
    }
}


