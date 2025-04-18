using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.WinterForge
{
    /// <summary>
    /// Used to create a custom way to define the way a type is stored using the <see cref="WinterForge"/> serialization system.
    /// </summary>
    public abstract class CustomValueProvider<T> : CustomValueProviderINTERNAL
    {
        internal override Type Type => typeof(T);

        internal override string _CreateString(object? obj)
        {
            return CreateString((T)obj);
        }

        internal override object? _CreateObject(string value)
        {
            return CreateObject(value);
        }

        public abstract string CreateString(T obj);
        public abstract T? CreateObject(string value);
    }

    /// <summary>
    /// Used internally to browse types to find custom value providers
    /// </summary>
    public abstract class CustomValueProviderINTERNAL
    {
        internal abstract Type Type { get; }
        internal abstract string _CreateString(object? obj);
        internal abstract object? _CreateObject(string value);
    }
}
