using System.Numerics;
using WinterRose.ValueModifiers;

namespace WinterRose.StaticValueModifiers
{
    public class StaticSubtractiveModifier<T> : StaticModifier<T> where T : INumber<T>
    {
        protected override T Modify(T value)
        {
            var val = new SubtractiveModifier<T>();
            val.SetModifiers(Modifiers);
            return val.Modify(value);
        }
    }
}
