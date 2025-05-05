using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Legacy.Serialization
{
    /// <summary>
    /// Place this attribute on a class or struct to generate serialization code for it. 
    /// <br></br>the generated code will serialize faster than the default <see cref="SnowSerializer"/> but still accessed through the same methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateSerializerAttribute : Attribute
    {
        public bool IncludePrivateFields { get; set; } = false;
        public bool IncludePrivateProperties { get; set; } = false;
        public bool MultiTheaded { get; set; } = true;
    }
}
