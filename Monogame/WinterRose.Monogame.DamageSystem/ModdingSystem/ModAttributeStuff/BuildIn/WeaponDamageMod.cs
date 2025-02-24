using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Monogame.Weapons;

namespace WinterRose.Monogame.ModdingSystem;
public class WeaponDamageMod : ModAttribute<Weapon>
{
    private bool boostChanged = false;

    /// <summary>
    /// A percentage value how much base damage this attribute adds to the weapon
    /// </summary>
    [Show]
    public float DamageBoost
    {
        get => damageBoost;
        set
        {
            damageBoost = value;
            boostChanged = true;
        }
    }
    private float damageBoost = 1;

    private string effectString;
    public override string EffectString
    {
        get
        {
            if(boostChanged)
            {
                effectString = $"+{damageBoost * 100}% Damage";
                boostChanged = false;
            }
            return effectString;
        }
    }

    public override void Apply(Weapon weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        var dmg = weapon.Magazine.Bullet.Damage;
        ModifierKey = dmg.DamageModifier.AddAdditive((dmg.BaseDamage * damageBoost).FloorToInt());
    }

    public override void Unapply(Weapon weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        var dmg = weapon.Magazine.Bullet.Damage;
        dmg.DamageModifier.RemoveAdditive(ModifierKey);
    }
}
