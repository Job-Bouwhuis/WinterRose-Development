using System.Numerics;
using WinterRose.ValueModifiers;

namespace WinterRose.StaticValueModifiers
{
    public class StaticMultiplicativeModifier<T> : StaticModifier<T> where T : INumber<T>
    {
        protected override T Modify(T value)
        {
            var val = new MultiplicativeModifier<T>();
            val.SetModifiers(Modifiers);
            return val.Modify(value);
        }
    }
}
