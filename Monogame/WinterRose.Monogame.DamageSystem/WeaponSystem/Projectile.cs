using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using WinterRose;
using WinterRose.Monogame.DamageSystem;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame.Weapons;

[RequireComponent<Collider>(AutoAdd = false)]
public class Projectile : ObjectBehavior
{
    /// <summary>
    /// The speed of the bullet.
    /// </summary>
    [IncludeInTemplateCreation]
    public StaticCombinedModifier<float> Speed { get; set; } = 10.0f;

    /// <summary>
    /// The damage the bullet deals.
    /// </summary>
    [IncludeInTemplateCreation]
    public DamageType Damage { get; set; } = new NeutralDamage();

    /// <summary>
    /// The lifetime of the bullet in seconds.
    /// </summary>
    [IncludeInTemplateCreation]
    public StaticCombinedModifier<float> Lifetime { get; set; } = 1.0f;

    /// <summary>
    /// The splash radius of the bullet (may be 0 for no splash)
    /// </summary>
    [IncludeInTemplateCreation]
    public StaticCombinedModifier<float> SplashRadius { get; set; } = 0.0f;

    /// <summary>
    /// The damage falloff for the splash radius (if the splash radius is 0, this value is not used
    /// </summary>
    [IncludeInTemplateCreation]
    public StaticCombinedModifier<float> SplashDamageFalloff { get; set; } = 0.0f;

    /// <summary>
    /// The spread the bullet has (in a circle)
    /// </summary>
    [IncludeInTemplateCreation]
    public StaticCombinedModifier<float> Spread { get; set; } = 0.0f;

    /// <summary>
    /// The force the bullet applies to objects it hits
    /// </summary>
    [IncludeInTemplateCreation]
    public StaticCombinedModifier<float> Force { get; set; } = 0.0f;

    [Show]
    private float currentTime = 0;

    [Show, IgnoreInTemplateCreation]
    private float creationTime;

    [Show, IgnoreInTemplateCreation]
    private Vector2 direction;

    private PhysicsObject physics;

    [Show, IgnoreInTemplateCreation]
    private List<IProjectileHitAction> hitActions = new();

    public override void ResetClone(in ObjectComponent newComponent)
    {
        Projectile clone = (Projectile)newComponent;
        clone.Speed = (StaticCombinedModifier<float>)Speed.Clone();
        clone.Lifetime = (StaticCombinedModifier<float>)Lifetime.Clone();
        clone.Damage = (DamageType)Damage.Clone();
        clone.SplashRadius = (StaticCombinedModifier<float>)SplashRadius.Clone();
        clone.SplashDamageFalloff = (StaticCombinedModifier<float>)SplashDamageFalloff.Clone();
        clone.Spread = (StaticCombinedModifier<float>)Spread.Clone();
        clone.Force = (StaticCombinedModifier<float>)Force.Clone();
    }

    /// <summary>
    /// Initializes the bullet for use.
    /// </summary>
    /// <param name="direction"></param>
    public void Fire()
    {
        _ = owner;
        this.direction = transform.up;

        // randomly spread the bullet
        this.direction += new Vector2((float)Random.Shared.NextDouble(), (float)Random.Shared.NextDouble()) * Spread;
        direction.Normalize();

        creationTime = Time.SinceStartup;
    }

    // Start is called before the first frame update
    void Start()
    {
        _ = owner;
        physics = AttachOrFetchComponent<PhysicsObject>();

        if (TryFetchComponent<Collider>(out var col))
            col.OnCollisionEnter += OnCollisionEnter;

        var actions = FetchComponents<IProjectileHitAction>();
        actions.Foreach(hitActions.Add);
    }

    // Update is called once per frame
    void Update()
    {
        _ = this.owner;
        currentTime += Time.SinceLastFrame;
        if (currentTime > Lifetime)
        {
            Destroy(owner);
        }

        physics.Velocity = direction * Speed;
    }

    private void OnCollisionEnter(CollisionInfo collision)
    {
        if(collision.other.TryFetchComponent(out Projectile p))
        {
            // if the object is a projectile, don't do anything
            return;
        }

        foreach (var action in hitActions)
            action.OnHit(this, collision.other.owner, collision.CollisionSide);

        Destroy(owner);
    }
}