using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.StaticValueModifiers.BuildInValueProviders
{
    class DateOnlyValueProvider : CustomValueProvider<DateOnly>
    {
        public override DateOnly CreateObject(string value, InstructionExecutor executor)
        {
            return DateOnly.Parse(value);
        }

        public override string CreateString(DateOnly obj, ObjectSerializer serializer)
        {
            return obj.ToString();
        }
    }
}
