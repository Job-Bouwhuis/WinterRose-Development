using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.StatusSystem;

namespace WinterRose.Monogame.DamageSystem;

public class FireDamage : DamageType
{
    public override void DealDamage(Vitality target)
    {
        target.DealDamage(Damage);
    }

    private FireDamage() { } // for serialization

    public FireDamage(int BaseDamage) : base(BaseDamage) 
    {
        ConnectedStatusEffect = new FireStatusEffect(this);
    }
}
