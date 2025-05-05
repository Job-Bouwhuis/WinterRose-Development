using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Types
{
    public class CrystalBoolean : CrystalType
    {
        public override string Name => "boolean";
        [IncludeWithSerialization]
        public bool Value { get; set; }

        public static CrystalBoolean False => new() { Value = false };
        public static CrystalBoolean True => new() { Value = true };

        public CrystalBoolean() { }

        public override object GetValue() => Value;

        public override CrystalType SetValue(object value)
        {
            if (value is bool)
            {
                Value = (bool)value;
                return this;
            }
            else
                return FromObject(value);
        }

        public override CrystalBoolean And(CrystalBoolean value) => Value && value.Value;
        public override CrystalBoolean Or(CrystalBoolean value) => Value || value.Value;
        public override CrystalBoolean Not() => !Value;
        public override CrystalBoolean Xor(CrystalBoolean value) => Value ^ value.Value;
        public override CrystalBoolean Equal(CrystalType value) => value is CrystalBoolean b && Value == b.Value;
        public override CrystalBoolean NotEqual(CrystalType value) => !Equal(value);
        public override CrystalBoolean GreaterThan(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \">\"");
        public override CrystalBoolean LessThan(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"<\"");
        public override CrystalBoolean GreaterThanOrEqual(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \">=\"");
        public override CrystalBoolean LessThanOrEqual(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"<=\"");


        public static implicit operator CrystalBoolean(bool value) => new CrystalBoolean() { Value = value };
        public static implicit operator bool(CrystalBoolean value) => value.Value;
    }
}
