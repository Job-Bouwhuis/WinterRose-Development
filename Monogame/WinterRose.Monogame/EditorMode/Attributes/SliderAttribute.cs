using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame
{
    /// <summary>
    /// Tells the object inspector in the editor to render a slider with the provided min and max values for this field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal class SliderAttribute : Attribute
    {
        public float Min { get; }
        public float Max { get; }

        public SliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
