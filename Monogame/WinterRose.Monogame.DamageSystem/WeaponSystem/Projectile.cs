using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using WinterRose;
using WinterRose.Monogame.DamageSystem;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame.Weapons;

/// <summary>
/// it go brrrrrr
/// </summary>
[RequireComponent<Collider>(AutoAdd = false)]
[ParallelBehavior]
public class Projectile : ObjectBehavior
{
    /// <summary>
    /// The speed of the bullet.
    /// </summary>
    [WFInclude]
    public StaticCombinedModifier<float> Speed { get; set; } = 10.0f;

    /// <summary>
    /// The damage the bullet deals.
    /// </summary>
    [WFInclude]
    public DamageType Damage { get; set; } = new NeutralDamage();

    /// <summary>
    /// The lifetime of the bullet in seconds.
    /// </summary>
    [WFInclude]
    public StaticCombinedModifier<float> Lifetime { get; set; } = 1.0f;

    /// <summary>
    /// The splash radius of the bullet (may be 0 for no splash)
    /// </summary>
    [WFInclude]
    public StaticCombinedModifier<float> SplashRadius { get; set; } = 0.0f;

    /// <summary>
    /// The damage falloff for the splash radius (if the splash radius is 0, this value is not used
    /// </summary>
    [WFInclude]
    public StaticCombinedModifier<float> SplashDamageFalloff { get; set; } = 0.0f;

    /// <summary>
    /// The spread the bullet has (in a circle)
    /// </summary>
    [WFInclude]
    public StaticCombinedModifier<float> Spread { get; set; } = 0.0f;

    /// <summary>
    /// The force the bullet applies to objects it hits
    /// </summary>
    [WFInclude]
    public StaticCombinedModifier<float> Force { get; set; } = 0.0f;
    /// <summary>
    /// The chance this bullet has to apply a status effect. 
    /// if greater than 100, guaranteed 1 with a chance to apply 2, 
    /// greater than 200 guaranteed 2, with chance of 3
    /// </summary>
    [WFInclude]
    public int StatusChance { get; set; }

    [Show]
    private float currentTime = 0;

    [Show, IgnoreInTemplateCreation]
    private float creationTime;

    [Show, IgnoreInTemplateCreation]
    private Vector2 direction;

    private PhysicsObject physics;

    [Show, IgnoreInTemplateCreation]
    private List<IProjectileHitAction> hitActions = new();

    protected override void ResetClone(in ObjectComponent newComponent)
    {
        base.ResetClone(newComponent);
        Projectile clone = (Projectile)newComponent;
        clone.Speed = (StaticCombinedModifier<float>)Speed.Clone();
        clone.Lifetime = (StaticCombinedModifier<float>)Lifetime.Clone();
        clone.Damage = (DamageType)Damage.Clone();
        clone.SplashRadius = (StaticCombinedModifier<float>)SplashRadius.Clone();
        clone.SplashDamageFalloff = (StaticCombinedModifier<float>)SplashDamageFalloff.Clone();
        clone.Spread = (StaticCombinedModifier<float>)Spread.Clone();
        clone.Force = (StaticCombinedModifier<float>)Force.Clone();
        clone.hitActions = [];
        clone.physics = null;
        clone.creationTime = 0;
        clone.currentTime = 0;
        clone.direction = new();
    }

    public void Fire()
    {
        _ = owner;
        this.direction = transform.up;

        // randomly spread the bullet
        this.direction += new Vector2((float)Random.Shared.NextDouble(), (float)Random.Shared.NextDouble()) * Spread;
        direction.Normalize();

        creationTime = Time.SinceStartup;
    }

    protected override void Start()
    {
        physics = AttachOrFetchComponent<PhysicsObject>();

        if (TryFetchComponent<Collider>(out var col))
            col.OnCollisionEnter += OnCollisionEnter;

        var actions = FetchComponents<IProjectileHitAction>();
        actions.Foreach(hitActions.Add);
    }

    protected override void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > Lifetime)
        {
            Destroy(owner);
        }

        physics.Velocity = direction * Speed;
    }

    private void OnCollisionEnter(CollisionInfo collision)
    {
        if(collision.other.HasComponent<Projectile>())
            return; // if the object is a projectile, don't do anything

        foreach (var action in hitActions)
            action.OnHit(this, collision.other.owner, collision.CollisionSide);

        Destroy(owner);
    }
}