using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.HealthSystem;
using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.StatusSystem;
public class BurnEffect : StatusEffect
{
    public override string Name => "Burn";

    // Damage per stack is now moddable
    public StaticCombinedModifier<float> DamagePerStack { get; set; } = 5f;

    public BurnEffect()
    {
        MaxStacks = 10;
        SecondsPerStack = 1f; // one second per stack
        RemoveAllStacksOnTimeout = false; // remove stacks one by one
    }

    /// <summary>
    /// Checks if this effect can be applied to the target effector.
    /// Example: only apply if the entity has a Vitality component.
    /// </summary>
    public override bool Validate(StatusEffector effector)
    {
        return effector.owner.TryFetchComponent<Vitality>(out _);
    }

    protected override void OnApply(StatusEffector effector)
    {
        // Could trigger visual/audio hooks here
    }

    protected override void OnUpdate(StatusEffector effector)
    {
        // Apply damage per stack
        if (effector.owner.TryFetchComponent<Vitality>(out var vitals))
        {
            float damage = DamagePerStack * Stacks;
            vitals.ApplyDamage(damage);
        }
    }

    public override StatusEffect Clone()
    {
        return new BurnEffect
        {
            DamagePerStack = this.DamagePerStack,
            MaxStacks = this.MaxStacks,
            SecondsPerStack = this.SecondsPerStack,
            RemoveAllStacksOnTimeout = this.RemoveAllStacksOnTimeout
        };
    }
}

