using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.DamageSystem;

namespace WinterRose.Monogame.StatusSystem
{
    public class FireStatusEffect : DamageStatusEffect
    {
        public override string Description => "Damages the target every second. the amount of stacks equal the amount of times damage is dealt";
        public override bool MultiplyByStacks => false;

        public override DamageType DamageType { get; } = new FireDamage();

        public FireStatusEffect()
        {
            DamageType.BaseDamage = 10;
        }
        
        protected internal override void Update(StatusEffector effector)
        {
            if(!effector.TryFetchComponent<Vitality>(out var vitals))
            {
                Stacks = 0;
                return;
            }

            DamageType.DealDamage(vitals);
        }
    }
}
