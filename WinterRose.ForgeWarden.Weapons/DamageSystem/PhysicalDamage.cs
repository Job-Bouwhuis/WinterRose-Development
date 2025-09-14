using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.HealthSystem;

namespace WinterRose.ForgeWarden.DamageSystem;
public class PhysicalDamage : DamageType
{
    public override string Name => "Physical";

    public override void DealDamage(Vitality target, float amount)
    {
        target.ApplyDamage(amount); // Basic raw damage
    }
}
