﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;

namespace WinterRose.Monogame.DamageSystem;

/// <summary>
/// Vitatly holds <see cref="DamageSystem.Health"/> and <see cref="DamageSystem.Armor"/><br></br>
/// Use <see cref="DealDamage(int)"/> to deal damage based on this instance its armor value
/// </summary>
public class Vitality : ObjectComponent, IHittable
{
    /// <summary>
    /// The Health values
    /// </summary>
    [IncludeInTemplateCreation]
    public Health Health { get; set; } = new();
    /// <summary>
    /// The Armor values
    /// </summary>
    [IncludeInTemplateCreation]
    public Armor Armor { get; set; } = new();

    [IgnoreInTemplateCreation]
    public event Action OnDeath { add => Health.OnDeath += value; remove => Health.OnDeath -= value; }

    /// <summary>
    /// Whether or not this can take damage or not.
    /// </summary>
    public bool IsInvunerable
    {
        get => Armor.BaseArmor == 0;
        set
        {
            if (value)
            {
                armorBeforeInvunrable = Armor.BaseArmor;
                Armor.SubtractiveArmorModifier.SetBaseValue(0);
                return;
            }

            Armor.SubtractiveArmorModifier.SetBaseValue(armorBeforeInvunrable);
        }
    }
    [IgnoreInTemplateCreation]
    private float armorBeforeInvunrable = 0;

    /// <summary>
    /// Calculates the correct damage depending on <see cref="Armor"/>, and deals it to <see cref="Health"/>
    /// </summary>
    /// <param name="damage"></param>
    public void DealDamage(int damage) => Health.DealDamage(Armor.CalculateReducedDamage(damage));

    /// <summary>
    /// Deals the damage and ignores the armor. the exact damage amount is dealt to this entity
    /// </summary>
    /// <param name="damage"></param>
    public void DealDamageIgnoreArmor(int damage) => Health.DealDamage(damage);

    /// <summary>
    /// Deals the damage based on the <see cref="DamageType"/> provided
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamageFrom(DamageType damage) => damage.DealDamage(this);

    /// <summary>
    /// Deals the damage based on the <see cref="DamageType"/> provided
    /// </summary>
    public void TakeDamageFrom<T>() where T : DamageType => ActivatorExtra.CreateInstance<T>().DealDamage(this);

    public void OnHit(Projectile bullet, DamageType damage)
    {
        TakeDamageFrom(damage);
    }
}
