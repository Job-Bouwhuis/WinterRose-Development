using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Types
{
    public class CrystalNumber : CrystalType
    {
        private double value;

        public static CrystalNumber Zero => 0d;

        public override string Name => "Number";

        [IncludeWithSerialization]
        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public CrystalNumber() { }

        public static implicit operator CrystalNumber(double value)
        {
            return new CrystalNumber() { Value = value };
        }

        public override object GetValue()
        {
            return Value;
        }
        public override CrystalType SetValue(object value)
        {
            if (value is double)
            {
                Value = (double)value;
                return this;
            }
            else
                return FromObject(value);
        }

        public override CrystalType Add(CrystalType value)
        {
            if (value is CrystalNumber n)
            {
                return FromObject(Value + (double)n.GetValue());
            }
            throw new Exception("Operator \"+\" on type \"Number\" can only work when the second argument is also a number");
        }
        public override CrystalType Subtract(CrystalType value)
        {
            if (value is CrystalNumber n)
            {
                return FromObject(Value - (double)n.GetValue());
            }
            throw new Exception("Operator \"-\" on type \"Number\" can only work when the second argument is also a number");
        }
        public override CrystalType Multiply(CrystalType value)
        {
            if (value is CrystalNumber n)
            {
                return FromObject(Value + (double)n.GetValue());
            }
            throw new Exception("Operator \"*\" on type \"Number\" can only work when the second argument is also a number");
        }
        public override CrystalType Divide(CrystalType value)
        {
            if (value is CrystalNumber n)
            {
                return FromObject(Value + (double)n.GetValue());
            }
            throw new Exception("Operator \"/\" on type \"Number\" can only work when the second argument is also a number");
        }
        public override CrystalBoolean Equal(CrystalType value) => value is CrystalNumber n && Value == n.Value;
        public override CrystalBoolean NotEqual(CrystalType value) => !Equal(value);
        public override CrystalBoolean GreaterThan(CrystalType value) => value is CrystalNumber n && Value > n.Value;
        public override CrystalBoolean LessThan(CrystalType value) => value is CrystalNumber n && Value < n.Value;
        public override CrystalBoolean GreaterThanOrEqual(CrystalType value) => value is CrystalNumber n && Value >= n.Value;
        public override CrystalBoolean LessThanOrEqual(CrystalType value) => value is CrystalNumber n && Value <= n.Value;
    }
}
