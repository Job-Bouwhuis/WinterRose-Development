using System;
using System.ComponentModel.DataAnnotations;
using WinterRose.ForgeSignal;
using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.HealthSystem;

/// <summary>
/// Health component. anything that has health should use this component.
/// </summary>
public class Health
{
    /// <summary>
    /// Getting returns the maximum health based on the <see cref="AddititiveHealthModifier"/><br></br><br></br>
    /// Setting this value overrides the <b>base max health</b> <br></br>Add a modifier to <see cref="AddititiveHealthModifier"/> to temporarily increase health if that is desired.
    /// </summary>
    public float MaxHealth
    {
        get => AddititiveHealthModifier.Value;
        set
        {
            if (value is < 1)
                throw new ArgumentException("Value must be bigger than 0");

            float percentageOfCurrentToMax = (float)currentHealth / MaxHealth;
            AddititiveHealthModifier.SetBaseValue(value);

            currentHealth = (int)(AddititiveHealthModifier.BaseValue * percentageOfCurrentToMax);
        }
    }

    public MulticastVoidInvocation OnDeath { get; } = new();

    /// <summary>
    /// The current health
    /// </summary>
    public float CurrentHealth => currentHealth;

    /// <summary>
    /// The modifier for base health
    /// </summary>
    public StaticAdditiveModifier<float> AddititiveHealthModifier { get; set; } = new();
    public MulticastVoidInvocation<Health, float, float> OnHealthChanged { get; set; } = new();

    private float currentHealth = 1;

    public Health() : this(100) { }

    public Health(float health)
    {
        currentHealth = MaxHealth = health;
    }

    /// <summary>
    /// Subtracts <paramref name="damage"/> from <see cref="CurrentHealth"/>. to a minimum of 0
    /// </summary>
    /// <param name="damage"></param>
    public void DealDamage(float damage)
    {
        float previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Math.Max(currentHealth, 0);

        if(currentHealth <= 0)
            OnDeath.Invoke();

        OnHealthChanged.Invoke(this, previousHealth, currentHealth);
    }

    /// <summary>
    /// Adds <paramref name="amount"/> to <see cref="CurrentHealth"/> to a maximum of <see cref="MaxHealth"/>
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount)
    {
        float previousHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Math.Max(currentHealth, MaxHealth);

        OnHealthChanged.Invoke(this, previousHealth, currentHealth);
    }
}
