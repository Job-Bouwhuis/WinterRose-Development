using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.DamageSystem;
using WinterRose.ValueModifiers;

namespace WinterRose.Monogame.StatusSystem
{
    /// <summary>
    /// A status effect that deals damage
    /// </summary>
    [method: DefaultArguments("new damage status effect")]
    public abstract class DamageStatusEffect : StatusEffect
    {
        public abstract override string Description { get; }
        /// <summary>
        /// Multiplies the base damage by the stacks before modifiying it with the additive and multiplicative modifiers
        /// </summary>
        public abstract bool MultiplyByStacks { get; }
        public override StatusEffectUpdateType UpdateType => StatusEffectUpdateType.StackRemoval;

        public abstract DamageType DamageType { get; }

        protected internal abstract override void Update(StatusEffector effector);
    }
}
