using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;
using WinterRose.WinterForge;

namespace WinterRose.StaticValueModifiers.BuildInValueProviders
{
    class DateOnlyValueProvider : CustomValueProvider<DateOnly>
    {
        public override DateOnly CreateObject(string value)
        {
            return DateOnly.Parse(value);
        }

        public override string CreateString(DateOnly obj)
        {
            return obj.ToString();
        }
    }
}
