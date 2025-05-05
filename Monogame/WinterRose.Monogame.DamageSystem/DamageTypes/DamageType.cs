using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.StatusSystem;
using WinterRose.StaticValueModifiers;

#nullable enable

namespace WinterRose.Monogame.DamageSystem;

/// <summary>
/// A type of damage that can be dealt
/// 
/// <br></br>Be sure to create a constructor with only '(int baseDamage)' and call base constructor '<see cref="DamageType(int)"/>'
/// if you want it to work with the world template file system
/// </summary>
public abstract class DamageType : ICloneable
{
    /// <summary>
    /// The base damage this damage type will inflict
    /// </summary>
    public int BaseDamage { get => DamageModifier.BaseValue; set => DamageModifier.BaseValue = value; }
    /// <summary>
    /// The modified damage based on <see cref="DamageModifier"/>
    /// </summary>
    public int Damage => DamageModifier.Value;

    /// <summary>
    /// The icon of this damage type. Can be null if not set.
    /// </summary>
    [IncludeWithSerialization]
    public Sprite? Icon { get; set; }

    /// <summary>
    /// The modifiers that dictate the actual damage
    /// </summary>
    [IncludeWithSerialization]
    public StaticCombinedModifier<int> DamageModifier { get; } = new();

    public DamageType() => BaseDamage = 5;
    public DamageType(int baseDamage) => BaseDamage = baseDamage;
    static DamageType()
    {
        Worlds.WorldTemplateObjectParsers.Add(typeof(DamageType), (instance, identifier) =>
        {
            string typeName = instance.GetType().Name;
            return $"{typeName}({((DamageType)instance).BaseDamage})";
        });
    }

    /// <summary>
    /// The status effect this damage type is linked to, if any. (null if none)
    /// </summary>
    [IncludeWithSerialization]
    public StatusEffect? ConnectedStatusEffect { get; set; }

    /// <summary>
    /// Deals the <see cref="Damage"/> to the <paramref name="target"/>
    /// </summary>
    /// <param name="target"></param>
    public abstract void DealDamage(Vitality target);

    public object Clone()
    {
        DamageType clone = (DamageType)MemberwiseClone();
        clone.BaseDamage = BaseDamage;
        CloneDamage(clone);
        return clone;
    }

    public virtual void CloneDamage(in DamageType clone) { }

    protected internal string ParseToString()
    {
        return BaseDamage.ToString();
    }

    /// <summary>
    /// Parses the string value
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected internal static object? ParseFromString(string data)
    {
        var baseDam = TypeWorker.CastPrimitive<int>(data);
        return new NeutralDamage() { BaseDamage = baseDam };
    }
}
