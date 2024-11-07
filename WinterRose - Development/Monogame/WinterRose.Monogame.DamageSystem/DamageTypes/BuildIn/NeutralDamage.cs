using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.DamageSystem.DamageTypes.BuildIn
{
    internal class NeutralDamage : DamageType
    {
        public override void DealDamage(Vitality target)
        {
            target.DealDamage(Damage);
        }
    }
}
