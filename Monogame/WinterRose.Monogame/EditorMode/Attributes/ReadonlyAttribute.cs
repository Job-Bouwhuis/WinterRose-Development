using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame
{
    /// <summary>
    /// Tells the hirarchy to display this field as readonly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    class ReadonlyAttribute : Attribute
    {
    }
}
