using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Types
{
    public sealed class CrystalString : CrystalType
    {
        public override string Name => "string";
        [IncludeWithSerialization]
        public string Value { get; set; }

        public CrystalString() { }

        public override object GetValue()
        {
            return Value;
        }
        public override CrystalType SetValue(object value)
        {
            if (value is string)
            {
                Value = (string)value;
                return this;
            }
            else
                return FromObject(value);
        }
    }
}
