using WinterRose.ForgeWarden.Physics;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

[RequireComponent<RigidBodyComponent>]
[RequireComponent<Collider>]
public class Projectile : Component, IUpdatable
{
    public ProjectileStats Stats { get; set; }

    [InjectFromSelf]
    RigidBodyComponent rb;

    [InjectFromSelf]
    Collider collider;

    public float FinalDamage
    {
        get
        {
            int guaranteedCrits = (int)(Stats.CritChance / 1.0f); // each full 100% is a guaranteed crit
            float remainingChance = Stats.CritChance % 1.0f;      // leftover chance for one more crit

            int totalCrits = guaranteedCrits;
            if (Random.Shared.NextDouble() < remainingChance)
                totalCrits++;

            float finalDamage = Stats.Damage * MathF.Pow(Stats.CritMultiplier, totalCrits);
            return finalDamage;
        }
    }

    IProjectileHitAction[] hitActions;

    [Show]
    private float timeAlive = 0;

    public void Update()
    {
        rb.RigidBody.LinearVelocity = new BulletSharp.Math.Vector3(0, Stats.Speed, 0);
        timeAlive += Time.deltaTime;
        if (timeAlive >= Stats.Lifetime)
            Destroy();
    }
}
