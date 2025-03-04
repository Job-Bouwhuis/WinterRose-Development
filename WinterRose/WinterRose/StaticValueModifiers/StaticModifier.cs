using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.Serialization;

namespace WinterRose.StaticValueModifiers;

/// <summary>
/// A static multiplier updates the value every time the modifiers change, instead recalculating it every time.
/// <br></br> Useful for values that stay mostly the same, for example Armor or Health.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("{BaseValue} >> {Value}")]
public abstract class StaticModifier<T> : ICloneable
{
    protected StaticModifier() { }

    /// <summary>
    /// The value as of the current <see cref="BaseValue"/> and each modifier.
    /// </summary>
    public T Value => currentValue;
    T currentValue;

    /// <summary>
    /// The base value of the modifier
    /// </summary>
    [IncludeWithSerialization]
    public T BaseValue { get => baseValue; set => SetBaseValue(value); }
    T baseValue;

    protected StaticModifier(T baseValue) => SetBaseValue(baseValue);

    protected Dictionary<int, Func<T, T>> Modifiers { get; set; } = [];

    protected abstract T Modify(T value);

    public int Add(T value)
    {
        int key = Modifiers.NextAvalible();
        Modifiers.Add(key, val => value);
        currentValue = Modify(baseValue);
        return key;
    }

    public void Remove(int key)
    {
        Modifiers.Remove(key);
        currentValue = Modify(baseValue);
    }

    /// <summary>
    /// Updates the base value of this static modifier.
    /// </summary>
    /// <param name="value"></param>
    public void SetBaseValue(T value)
    {
        baseValue = value;
        currentValue = Modify(baseValue);
    }

    public object Clone()
    {
        StaticModifier<T> result = (StaticModifier<T>)MemberwiseClone();
        result.Modifiers = [];
        return result;
    }
}
