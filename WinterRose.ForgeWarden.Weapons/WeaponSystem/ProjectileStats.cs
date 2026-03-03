using Raylib_cs;
using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

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

    public int CollisionLayer { get; set; }
    public int CollisionMask { get; set; }

    public Sprite ProjectileTexture
    {
        get
        {
            field ??= Sprite.CreateRectangle(15, 25, Color.Red);
            return field;
        }
        set;
    }
}
