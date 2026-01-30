using System;
using WinterRose;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.DamageSystem;
using WinterRose.ForgeWarden.DamageSystem.WeaponSystem;
using WinterRose.ForgeWarden.WeaponSystem;

namespace WinterRose.ForgeWarden.HealthSystem;

/// <summary>
/// Vitatly holds <see cref="HealthSystem.Health"/> and <see cref="HealthSystem.Armor"/><br></br>
/// Use <see cref="DealDamage(int)"/> to deal damage based on this instance its armor value
/// </summary>
public class Vitality : Component, IHittable
{
    /// <summary>
    /// The Health values
    /// </summary>
    public Health Health { get; set; } = new();
    /// <summary>
    /// The Armor values
    /// </summary>
    public Armor Armor { get; set; } = new();

    public MulticastVoidInvocation OnDeath { get; } = new();

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
    private float armorBeforeInvunrable = 0;

    /// <summary>
    /// Calculates the correct damage depending on <see cref="Armor"/>, and deals it to <see cref="Health"/>
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(float damage) => Health.DealDamage(Armor.CalculateReducedDamage(damage));

    /// <summary>
    /// Deals the damage and ignores the armor. the exact damage amount is dealt to this entity
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamageIgnoreArmor(float damage) => Health.DealDamage(damage);

    /// <summary>
    /// Deals the damage based on the <see cref="DamageType"/> provided
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamageFrom(DamageType damage, float amount) => damage.DealDamage(this, amount);

    /// <summary>
    /// Deals the damage based on the <see cref="DamageType"/> provided
    /// </summary>
    public void TakeDamageFrom<T>(float amount) where T : DamageType => ActivatorExtra.CreateInstance<T>().DealDamage(this, amount);

    public void OnHit(HitInfo info)
    {
        TakeDamageFrom(info.DamageType, info.Damage);
    }
}
