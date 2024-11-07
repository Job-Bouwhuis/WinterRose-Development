using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.DamageSystem;

public class FireDamage : DamageType
{
    public override void DealDamage(Vitality target)
    {
        target.DealDamage(Damage);
    }
}
