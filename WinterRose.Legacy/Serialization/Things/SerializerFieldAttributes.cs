using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Legacy.Serialization.Things
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class IncludePrivateFieldsForFieldAttribute : Attribute { }
    /// <summary>
    /// Represents an attribute that can be used to include properties in class fields in the serialization process
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class IncludePropertiesForFieldAttribute : Attribute { }

    /// <summary>
    /// Makes sure that when serializing or deserializing this field will always be ignore
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class WFExcludeAttribute : Attribute { }

    /// <summary>
    /// Tells the serializer to include this property when handling its declaring class
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class WFIncludeAttribute : Attribute { }

    /// <summary>
    /// Tells the serializer to use the private fields within this class or struct even if the passed setting states not to include them
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class IncludePrivateFieldsAttribute : Attribute { }

    /// <summary>
    /// Tells the serializer to include all properties in the given class or struct even if they do not have the <b>WFIncludeAttribute</b> attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class IncludeAllPropertiesAttribute : Attribute { }
    
    /// <summary>
    /// Tells the serializer to serialize this class as the given type, even if the serializer would normally serialize it as a different type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializeAsAttribute<T> : SerializeAsAttributeINTERNAL
    {
        /// <summary>
        /// The type this class should be serialized as
        /// </summary>
        public override Type Type => typeof(T);
    }

    public abstract class SerializeAsAttributeINTERNAL : Attribute
    {
        public abstract Type Type { get; }
    }
}
