using System.Numerics;
using WinterRose.ValueModifiers;

namespace WinterRose.StaticValueModifiers;

public class StaticAdditiveModifier<T> : StaticModifier<T> where T : INumber<T>
{
    protected override T Modify(T value)
    {
        var val = new AdditiveModifier<T>();
        val.SetModifiers(Modifiers);
        return val.Modify(value);
    }

    public static implicit operator T(StaticAdditiveModifier<T> val) => val.Value;
    public static implicit operator StaticAdditiveModifier<T>(T val) => new() { BaseValue = val };
}
