using WinterRose.ForgeWarden.Physics;
using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

[RequireComponent<RigidBodyComponent>]
[RequireComponent<Collider>]
public class Projectile : Component, IUpdatable
{
    public class ProjectileStats
    {
        public DamageType DamageType { get; set; }
        public StaticCombinedModifier<float> Damage { get; set; } = 10;
        public StaticCombinedModifier<float> CritChance { get; set; } = 0.1f;
        public StaticCombinedModifier<float> CritMultiplier { get; set; } = 2.0f;
        public StaticCombinedModifier<float> StatusChance { get; set; } = 0.2f;
        public StaticCombinedModifier<float> Speed { get; set; } = 5;
        public StaticCombinedModifier<float> Lifetime { get; set; } = 5;
        public StaticCombinedModifier<float> PunchThrough { get; set; } = 0;
    }

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

    public void Update()
    {
        // fly forward, check for hits, etc
    }
}
