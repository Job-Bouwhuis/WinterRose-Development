using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.DamageSystem;

namespace WinterRose.Monogame.StatusSystem;

public class FireStatusEffect : DamageStatusEffect
{
    public override string Description => "Damages the target every second. the amount of stacks equal the amount of times damage is dealt";
    public override bool MultiplyByStacks => false;

    public override DamageType DamageType { get; }

    public FireStatusEffect(FireDamage damageType)
    {
        DamageType = damageType;
    }

    public FireStatusEffect()
    {
        DamageType = new FireDamage(10);
    }

    protected internal override void StacksUpdated(StatusEffector effector, int lastStacks, int currentStacks)
    {
        if (lastStacks < currentStacks)
            return;

        if (!effector.TryFetchComponent<Vitality>(out var vitals))
        {
            Stacks = 0;
            return;
        }

        DamageType.DealDamage(vitals);
    }

    protected internal override void Update(StatusEffector effector) { }
}
