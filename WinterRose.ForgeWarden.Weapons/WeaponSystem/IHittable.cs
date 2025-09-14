using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.DamageSystem;
using WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

namespace WinterRose.ForgeWarden.WeaponSystem;
internal interface IHittable
{
    /// <summary>
    /// Called when a projectile hits this entity.
    /// </summary>
    /// <param name="hitInfo">Information about the hit, such as damage, crits, status effects, etc.</param>
    void OnHit(HitInfo hitInfo);
}

public struct HitInfo
{
    /// <summary>
    /// The projectile that caused the hit to occur
    /// </summary>
    public Projectile Projectile { get; }
    /// <summary>
    /// The amount of damage that was applied to the target
    /// </summary>
    public float Damage { get; }
    /// <summary>
    /// The type of damage that hit the target
    /// </summary>
    public DamageType DamageType { get; }
    /// <summary>
    /// The level of crit the hit caused
    /// </summary>
    public int CritLevel { get; }
    /// <summary>
    /// The amount of status effects the hit applied to the target
    /// </summary>
    public int StatusProcs { get; }

    

    public HitInfo(Projectile projectile, float damage, DamageType damageType, int critLevel, int statusProcs)
    {
        Projectile = projectile;
        Damage = damage;
        DamageType = damageType;
        CritLevel = critLevel;
        StatusProcs = statusProcs;
    }
}
