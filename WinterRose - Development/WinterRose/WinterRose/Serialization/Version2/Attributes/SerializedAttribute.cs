using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// Indicates that a field or property should be serialized by <see cref="Serialization.Version2.SnowSerializer"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SerializedAttribute  : Attribute
    {
        public SerializedAttribute() { }

        /// <summary>
        /// Indicates that a field or property may only be serialized by the provided serializers.
        /// </summary>
        public List<Type> AllowedSerializers { get; set; } = [];
    }
    
    /// <summary>
    /// Indicates that a field or property should be serialized by <see cref="Serialization.Version2.SnowSerializer"/> only if the provided serializer is used.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SerializedAttribute<T> : SerializedAttribute
    {
        /// <summary>
        /// Adds the provided serializer to the list of allowed serializers.
        /// </summary>
        public SerializedAttribute()
        {
            AllowedSerializers = [ typeof(T) ];
        }
    }
}
