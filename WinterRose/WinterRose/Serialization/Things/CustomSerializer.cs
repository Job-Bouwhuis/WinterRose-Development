using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization.Things;

namespace WinterRose.Serialization
{
    /// <summary>
    /// Implement this in your class to customize the serializing of a specific object
    /// </summary>
    public abstract class CustomSerializer<T> : CustomSerializer
    {
        /// <summary>
        /// The type this serializer works for
        /// </summary>
        internal override Type SerializerType => typeof(T);
    }
}

namespace WinterRose.Serialization.Things
{
    public abstract class CustomSerializer
    {
        /// <summary>
        /// The type this serializer works for
        /// </summary>
        internal abstract Type SerializerType { get; }

        public abstract string Serialize(object obj, int depth);
        public abstract object Deserialize(string data, int depth);
    }
}
