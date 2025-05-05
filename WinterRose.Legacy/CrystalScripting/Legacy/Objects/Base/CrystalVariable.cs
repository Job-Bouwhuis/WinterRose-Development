using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy.Objects.Types;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Base
{
    [DebuggerDisplay("CrystalVariable: {Name} = {Type.GetValue()}"), IncludePrivateFields]
    public sealed class CrystalVariable
    {
        public static CrystalVariable Empty => new CrystalVariable("", null);
        public static CrystalVariable Null => new CrystalVariable("Null Literal", new CrystalNull());
        public static CrystalVariable True => new("True", CrystalBoolean.True);
        public static CrystalVariable False => new("False", CrystalBoolean.False);
        private string name;
        private CrystalType value;

        public string Name { get => name; set => name = value; }
        public CrystalType Type { get => value; set => this.value = value; }
        public bool IsValid => !string.IsNullOrWhiteSpace(name);

        public CrystalVariable(string name, CrystalType value)
        {
            this.name = name;
            this.value = value;
        }
        public CrystalVariable() { } // For serialization

        public static CrystalVariable operator +(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.Add(value));
        }
        public static CrystalVariable operator -(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.Subtract(value));
        }
        public static CrystalVariable operator *(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.Multiply(value));
        }
        public static CrystalVariable operator /(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.Divide(value));
        }
        public static CrystalVariable operator %(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.Modulus(value));
        }
        public static CrystalVariable operator ^(CrystalVariable v, CrystalType value)
        {
            if (v.Type is CrystalBoolean && value is CrystalBoolean e)
                return new("operatorResult", v.Type.Xor(e));
            throw new InvalidOperationException("Cannot perform bitwise XOR on non-boolean types.");
        }
        public static CrystalVariable operator &(CrystalVariable v, CrystalType value)
        {
            if (v.Type is CrystalBoolean && value is CrystalBoolean e)
                return new("operatorResult", v.Type.And(e.Value));
            throw new InvalidOperationException("Cannot preform biwise AND on non-boolean types.");
        }
        public static CrystalVariable operator |(CrystalVariable v, CrystalType value)
        {
            if (v.Type is CrystalBoolean && value is CrystalBoolean e)
                return new("operatorResult", v.Type.Or(e.Value));
            throw new InvalidOperationException("Cannot preform bitwise OR on non-boolean types.");
        }
        public static CrystalVariable operator ==(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.Equal(value));
        }
        public static CrystalVariable operator !=(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.NotEqual(value));
        }
        public static CrystalVariable operator >(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.GreaterThan(value));
        }
        public static CrystalVariable operator <(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.LessThan(value));
        }
        public static CrystalVariable operator >=(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.GreaterThanOrEqual(value));
        }
        public static CrystalVariable operator <=(CrystalVariable v, CrystalType value)
        {
            return new("operatorResult", v.Type.LessThanOrEqual(value));
        }

        public static CrystalVariable operator +(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.Add(value.Type));
        }
        public static CrystalVariable operator -(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.Subtract(value.Type));
        }
        public static CrystalVariable operator *(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.Multiply(value.Type));
        }
        public static CrystalVariable operator /(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.Divide(value.Type));
        }
        public static CrystalVariable operator %(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.Modulus(value.Type));
        }
        public static CrystalVariable operator ^(CrystalVariable v, CrystalVariable value)
        {
            if (v.Type is CrystalBoolean && value.Type is CrystalBoolean e)
                return new("operatorResult", v.Type.Xor(e));
            throw new InvalidOperationException("Cannot perform bitwise XOR on non-boolean types.");
        }
        public static CrystalVariable operator &(CrystalVariable v, CrystalVariable value)
        {
            if (v.Type is CrystalBoolean && value.Type is CrystalBoolean e)
                return new("operatorResult", v.Type.And(e.Value));
            throw new InvalidOperationException("Cannot preform biwise AND on non-boolean types.");
        }
        public static CrystalVariable operator |(CrystalVariable v, CrystalVariable value)
        {
            if (v.Type is CrystalBoolean && value.Type is CrystalBoolean e)
                return new("operatorResult", v.Type.Or(e.Value));
            throw new InvalidOperationException("Cannot preform bitwise OR on non-boolean types.");
        }
        public static CrystalVariable operator ==(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.Equal(value.Type));
        }
        public static CrystalVariable operator !=(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.NotEqual(value.Type));
        }
        public static CrystalVariable operator >(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.GreaterThan(value.Type));
        }
        public static CrystalVariable operator <(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.LessThan(value.Type));
        }
        public static CrystalVariable operator >=(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.GreaterThanOrEqual(value.Type));
        }
        public static CrystalVariable operator <=(CrystalVariable v, CrystalVariable value)
        {
            return new("operatorResult", v.Type.LessThanOrEqual(value.Type));
        }

    }
}


