using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.CrystalScripting.Legacy.Objects.Types
{
    [DebuggerDisplay("{Name} Value: {GetValue()}")]
    public abstract class CrystalType
    {
        public abstract string Name { get; }
        public abstract object GetValue();
        public abstract CrystalType SetValue(object value);

        public virtual CrystalType Add(CrystalType value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"+\""); }
        public virtual CrystalType Subtract(CrystalType value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"-\""); }
        public virtual CrystalType Multiply(CrystalType value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"*\""); }
        public virtual CrystalType Divide(CrystalType value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"/\""); }
        public virtual CrystalType Modulus(CrystalType value) => throw new NotImplementedException($"Object {Name} has no definition for operator \"%\"");
        public virtual CrystalBoolean And(CrystalBoolean value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"&\""); }
        public virtual CrystalBoolean Or(CrystalBoolean value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"|\""); }
        public virtual CrystalBoolean Xor(CrystalBoolean value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"^\""); }
        public virtual CrystalBoolean Not() { throw new NotImplementedException($"Object {Name} has no definition for operator \"!\""); }
        public virtual CrystalBoolean Equal(CrystalType value) => GetValue().Equals(value.GetValue());
        public virtual CrystalBoolean NotEqual(CrystalType value) { return !Equal(value); }
        public virtual CrystalBoolean GreaterThan(CrystalType value) { throw new NotImplementedException($"Object {Name} has no definition for operator \">\""); }
        public virtual CrystalBoolean LessThan(CrystalType value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"<\""); }
        public virtual CrystalBoolean GreaterThanOrEqual(CrystalType value) { throw new NotImplementedException($"Object {Name} has no definition for operator \">=\""); }
        public virtual CrystalBoolean LessThanOrEqual(CrystalType value) { throw new NotImplementedException($"Object {Name} has no definition for operator \"<=\""); }

        public static CrystalType FromObject(object? obj)
        {
            if (obj is null)
                return new CrystalNull();
            if (obj is string)
                return new CrystalString() { Value = (string)obj };
            else if (obj is double)
                return new CrystalNumber() { Value = (double)obj };
            else if (obj is bool)
                return new CrystalBoolean() { Value = (bool)obj };
            else if (obj is CrystalType)
                return (CrystalType)obj;
            else
                return new CrystalNull();
        }
        public static CrystalType ValueFromString(string variable)
        {
            if (TypeWorker.TryCastPrimitive(variable, out double doubleValue))
            {
                return new CrystalNumber() { Value = (double)doubleValue };
            }
            if (TypeWorker.TryCastPrimitive(variable, out bool boolValue))
            {
                return new CrystalBoolean() { Value = boolValue };
            }
            if (TypeWorker.TryCastPrimitive(variable, out string stringValue))
            {
                return new CrystalString() { Value = stringValue };
            }

            return new CrystalNull();
        }
        public static CrystalType TypeFromString(string type)
        {
            if (type is "number")
                return new CrystalNumber();
            if (type is "string")
                return new CrystalString();
            if (type is "boolean")
                return new CrystalBoolean();
            if (type is "null")
                return new CrystalNull();
            return new CrystalNull();
        }
    }
}
