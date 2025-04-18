using System;
using System.ComponentModel.DataAnnotations;
using WinterRose.Monogame.Weapons;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame.DamageSystem;

/// <summary>
/// Health component. anything that has health should use this component.
/// </summary>
public class Health
{
    /// <summary>
    /// Getting returns the maximum health based on the <see cref="AddititiveHealthModifier"/><br></br><br></br>
    /// Setting this value overrides the <b>base max health</b> <br></br>Add a modifier to <see cref="AddititiveHealthModifier"/> to temporarily increase health if that is desired.
    /// </summary>
    public int MaxHealth
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

    public event Action OnDeath = delegate { };

    /// <summary>
    /// The current health
    /// </summary>
    public int CurrentHealth => currentHealth;

    /// <summary>
    /// The modifier for base health
    /// </summary>
    public StaticAdditiveModifier<int> AddititiveHealthModifier { get; set; } = new();
    public Action<Health, int, int> OnHealthChanged { get; set; } = delegate { };

    private int currentHealth = 1;

    public Health() : this(100) { }

    public Health(int health)
    {
        currentHealth = MaxHealth = health;
    }

    static Health()
    {
        Worlds.WorldTemplateObjectParsers.Add(typeof(Health), (instance, identifier) =>
        {
            return $"{nameof(Health)}({((Health)instance).MaxHealth})";
        });
    }

    /// <summary>
    /// Subtracts <paramref name="damage"/> from <see cref="CurrentHealth"/>. to a minimum of 0
    /// </summary>
    /// <param name="damage"></param>
    public void DealDamage(int damage)
    {
        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Math.Max(currentHealth, 0);

        if(currentHealth <= 0)
            OnDeath();

        OnHealthChanged(this, previousHealth, currentHealth);
    }

    /// <summary>
    /// Adds <paramref name="amount"/> to <see cref="CurrentHealth"/> to a maximum of <see cref="MaxHealth"/>
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount)
    {
        int previousHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Math.Max(currentHealth, MaxHealth);

        OnHealthChanged(this, previousHealth, currentHealth);
    }
}
