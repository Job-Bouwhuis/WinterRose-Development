using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame.DamageSystem;

public abstract class DamageType
{
    public int BaseDamage { get => DamageModifier.BaseValue; set => DamageModifier.BaseValue = value; }
    public int Damage => DamageModifier.Value;




    /// <summary>
    /// The icon of this damage type. Can be null if not set.
    /// </summary>
    public Sprite Icon { get; set; }

    public StaticCombinedModifier<int> DamageModifier { get; } = new();

    public DamageType() => BaseDamage = 5;

    public abstract void DealDamage(Vitality target);
}
