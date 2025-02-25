using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame.DamageSystem;

/// <summary>
/// Holds armor value for whatever can be damaged.... or anything else really.... lmao
/// </summary>
public class Armor
{
    /// <summary>
    /// The base armor. a value between 0 and 1. Where 0 is full invulnerability <br></br><br></br>
    /// 
    /// default value if not overridden: .99
    /// </summary>
    public float BaseArmor
    {
        get => SubtractiveArmorModifier.BaseValue;
        set
        {
            if (value is < 0 or > 1)
                throw new ArgumentException("Value must be between 0 and 1");
            SubtractiveArmorModifier.SetBaseValue(value);
        }
    }

    public Armor() : this(1) { }
    public Armor(float baseArmor) => BaseArmor = baseArmor;

    static Armor()
    {
        Worlds.WorldTemplateObjectParsers.Add(typeof(Armor), (instance, identifier) =>
        {
            return $"{nameof(Armor)}({((Armor)instance).BaseArmor.ToString().Replace(',', '.')}f)";
        });
    }

    /// <summary>
    /// The armor based on current modifiers
    /// </summary>
    public float CurrentArmor => SubtractiveArmorModifier.Value;

    /// <summary>
    /// Values that are added onto the armor<br></br><br></br>
    /// 
    /// When armor is 0 damage is 100% reduced, when armor is 1 damage is 0% reduced.<br></br>
    /// add a modifier of .1 to result in a armor of .9 for 10% armor and thus 10% damage reduction 
    /// </summary>
    public StaticSubtractiveModifier<float> SubtractiveArmorModifier { get; set; } = new();

    /// <summary>
    /// Gets the damage reduced by the armor value.
    /// </summary>
    /// <param name="value">The raw damage value that is received.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public int CalculateReducedArmor(int rawDamage) => (int)Math.Round(rawDamage * CurrentArmor, 0, MidpointRounding.AwayFromZero);
}
