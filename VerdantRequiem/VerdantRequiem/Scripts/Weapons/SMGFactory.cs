using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.DamageSystem;
using WinterRose.ForgeWarden.DamageSystem.WeaponSystem;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Worlds;

namespace VerdantRequiem.Scripts.Weapons;

internal class SMGFactory
{
    public static Weapon CreateSMG(World world, bool player = true)
    {
        var smg = world.CreateEntity("SMG");

        Magazine mag = smg.AddComponent<Magazine>();
        Weapon weapon = smg.AddComponent<Weapon>();
        weapon.AvailableTriggers = [new StandardTrigger() {
            FireRate = 8
        }];

        mag.Projectile = SMGProjectile();
        mag.ReloadBehavior = new FullReloadBehavior()
        {
            ReloadTime = 1.5f
        };

        return weapon;
    }

    private static ProjectileStats SMGProjectile() => new()
    {
        DamageType = new PhysicalDamage(),
        Damage = 10,
        CritChance = 0.1f,
        CritMultiplier = 1.5f,
        StatusChance = 0.05f,
        Speed = 50,
        PunchThrough = 0
    };
}
