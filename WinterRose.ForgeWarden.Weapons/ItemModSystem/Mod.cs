using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.ItemModSystem;
/// <summary>
/// A mod for an item
/// </summary>
/// <typeparam name="T">The class type that represents the moddable item. eg: <see cref="DamageSystem.WeaponSystem.Weapon"/></typeparam>
public class Mod<T>
{
    /// <summary>
    /// The target this mod acts upon
    /// </summary>
    public T Target { get; set; }

    private List<ModEffect<T>> effects = [];
    public void AddEffect<TEffect>() where TEffect : ModEffect<T> => effects.Add(ActivatorExtra.CreateInstance<TEffect>()!);

    public void Apply()
    {
        foreach (var effect in effects)
            effect.Apply(Target);
    }

    public void Unapply()
    {
        foreach (var effect in effects)
            effect.Unapply(Target);
    }
}

/// <summary>
/// An interface that is used by <see cref="ModEffect{T}"/> to provide the tiers of values, eg linear, exponential, other
/// </summary>
public interface IModEffectValueProvider
{
    float[] CreateValues(int maxLevel, float maxValue);
}

/// <summary>
/// A specific modifier for <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ModEffect<T> : IModEffectValueProvider
{
    protected internal abstract void Apply(T target);
    protected internal abstract void Unapply(T target);

    /// <summary>
    /// The kind of automatic value scaling applied to the tiers. By default, linear
    /// </summary>
    protected virtual IModEffectValueProvider TierValueProvider { get; }

    public int Level
    {
        get => level;
        private set
        {
            if (value > MaxLevel)
                level = maxLevel;
            else if (value < 0)
                level = 0;
            else
                level = value;
        }
    }
    public int MaxLevel
    {
        get => maxLevel;
        set
        {
            maxLevel = value;
            if (MaxValue > 0)
                TierValues = TierValueProvider.CreateValues(MaxLevel, value);
        }
    }

    public float MaxValue
    {
        get => maxValue;
        set
        {
            maxValue = value;
            TierValues = TierValueProvider.CreateValues(MaxLevel, value);
        }
    }

    protected float[] TierValues;
    private float maxValue;
    private int maxLevel = 1;
    private int level = 1;

    protected ModEffect(int maxLevel, float maxValue)
    {
        TierValueProvider = this;
        MaxLevel = maxLevel;
        MaxValue = maxValue;
    }

    public float[] CreateValues(int maxLevel, float maxValue)
    {
        if (maxLevel == 1)
            return [maxValue];

        float[] values = new float[maxLevel];

        float step = maxValue / (maxLevel - 1);
        for (int i = 0; i < maxLevel; i++)
            values[i] = MathF.Round(i * step, 1);
        return values;
    }

    protected void OverrideTier(int tierIndex, float value)
    {
        if (tierIndex < 0 || tierIndex >= MaxLevel)
            throw new ArgumentOutOfRangeException(nameof(tierIndex));
        TierValues[tierIndex] = value;
    }

    protected float CurrentValue => TierValues[Level - 1];
}