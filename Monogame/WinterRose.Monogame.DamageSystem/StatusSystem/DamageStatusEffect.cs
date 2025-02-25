using WinterRose.Monogame.DamageSystem;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame.StatusSystem;

/// <summary>
/// A status effect that deals damage
/// </summary>
[method: DefaultArguments("new damage status effect")]
public abstract class DamageStatusEffect : StatusEffect
{
    /// <summary>
    /// Multiplies the base damage by the stacks before modifiying it with the additive and multiplicative modifiers
    /// </summary>
    public abstract bool MultiplyByStacks { get; }
    /// <summary>
    /// The percentage of damage a status effect tick deals to the target taken from the damage instance that inflicted the status effect.
    /// <br></br> eg. having 100 damage, and a 0.1f percentage, results in 10 damage per status tick. this can be further enhanced by <see cref="MultiplyByStacks"/>
    /// </summary>
    public StaticCombinedModifier<float> damagePercentage { get; } = new() { BaseValue = 0.08f }; 
    public override StatusEffectUpdateType UpdateType => StatusEffectUpdateType.StackRemoval;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public abstract DamageType DamageType { get; }

    protected internal abstract override void Update(StatusEffector effector);
}
