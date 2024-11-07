using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.StaticValueModifiers;

namespace WinterRose.StaticValueModifiers;

/// <summary>
/// Modifier to contain both <see cref="StaticAdditiveModifier{T}"/> and a <see cref="StaticMultiplicativeModifier{T}"/>. 
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("{BaseValue} >> {Value}")]
public class StaticCombinedModifier<T> : ICloneable where T : INumber<T>
{
    private StaticAdditiveModifier<T> AdditiveModifier { get; set; } = new();
    private StaticMultiplicativeModifier<T> MultiplicativeModifier { get; set; } = new();

    /// <summary>
    /// The base value for this combined modifier
    /// </summary>
    public T BaseValue
    {
        get => AdditiveModifier.BaseValue;
        set
        {
            AdditiveModifier.SetBaseValue(value);
            MultiplicativeModifier.SetBaseValue(AdditiveModifier.Value);
        }
    }

    /// <summary>
    /// The value based on the current base values and modifiers (first addition, then multiplication)
    /// </summary>
    public T Value => MultiplicativeModifier.Value;

    /// <summary>
    /// Adds a modifier to the Additive part
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int AddAdditive(T value)
    {
        int key = AdditiveModifier.Add(value);
        MultiplicativeModifier.SetBaseValue(AdditiveModifier.Value);
        return key;
    }
    /// <summary>
    /// Removes a modifier from the Additive part
    /// </summary>
    /// <param name="key"></param>
    public void RemoveAdditive(int key)
    {
        AdditiveModifier.Remove(key);
    }

    /// <summary>
    /// Adds a modifier to the multiplicative part
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int AddMultiplicative(T value) => MultiplicativeModifier.Add(value);
    /// <summary>
    /// Removes a modifier from the multiplicative part
    /// </summary>
    /// <param name="key"></param>
    public void RemoveMultiplicative(int key) => MultiplicativeModifier.Remove(key);

    public static implicit operator StaticCombinedModifier<T>(T value) => new() { BaseValue = value };
    public static implicit operator T(StaticCombinedModifier<T> value) => value.Value;

    public object Clone()
    {
        StaticCombinedModifier<T> clone = (StaticCombinedModifier<T>)MemberwiseClone();
        clone.AdditiveModifier = (StaticAdditiveModifier<T>)AdditiveModifier.Clone();
        clone.MultiplicativeModifier = (StaticMultiplicativeModifier<T>)MultiplicativeModifier.Clone();
        return clone;
    }
}
