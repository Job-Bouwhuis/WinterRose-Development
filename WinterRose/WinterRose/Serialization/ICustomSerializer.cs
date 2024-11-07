using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Serialization
{
    /// <summary>
    /// Implement this in your class to customize the serializing of a specific object
    /// </summary>
    public interface ICustomSerializer
    {
        Type SerializerType { get; }

        string Serialize(object obj, int depth);
        object Deserialize(string data, int depth);
    }
}
