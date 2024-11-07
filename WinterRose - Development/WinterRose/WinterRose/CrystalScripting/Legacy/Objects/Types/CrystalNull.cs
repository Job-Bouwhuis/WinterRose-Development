using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Types
{
    public sealed class CrystalNull : CrystalType
    {
        public override string Name => "null";

        public static object Null { get; } = new CrystalNull();

        public override object GetValue() => null;
        public override CrystalType SetValue(object value) => FromObject(value);

        public CrystalNull() { }

        public override CrystalType Add(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"+\"");
        public override CrystalType Subtract(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"-\"");
        public override CrystalType Multiply(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"*\"");
        public override CrystalType Divide(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"/\"");
        //public override CrystalType Modulo(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"%\"");
        //public override CrystalType Power(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"^\"");
        public override CrystalBoolean And(CrystalBoolean value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"&&\"");
        public override CrystalBoolean Or(CrystalBoolean value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"||\"");
        public override CrystalBoolean Not() => throw new NotImplementedException($"Object {Name} has no definition for operator \"!\"");
        public override CrystalBoolean Xor(CrystalBoolean value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"^\"");
        public override CrystalBoolean Equal(CrystalType value) => value is CrystalNull;
        public override CrystalBoolean NotEqual(CrystalType value) => !Equal(value);
        public override CrystalBoolean GreaterThan(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \">\"");
        public override CrystalBoolean LessThan(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"<\"");
        public override CrystalBoolean GreaterThanOrEqual(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \">=\"");
        public override CrystalBoolean LessThanOrEqual(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"<=\"");
    }
}
