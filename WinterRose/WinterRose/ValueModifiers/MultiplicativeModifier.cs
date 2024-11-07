using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ValueModifiers;

public class MultiplicativeModifier<T> : ValueModifier<T> where T : INumber<T>
{
    public override T Modify(T value)
    {
        T result = T.Zero;
        foreach (var modifier in Modifiers.Values)
        {
            value *= modifier(value);
        }
        return value;
    }
}
