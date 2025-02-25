using Microsoft.Xna.Framework.Input;
using WinterRose.Monogame;
using WinterRose.Monogame.StatusSystem;
using WinterRose.StaticValueModifiers;

namespace TopDownGame.Player;

[RequireComponent<StatusEffector>]
internal class Dash : ObjectBehavior
{
    public StaticCombinedModifier<float> DashDistance { get; } = new() { BaseValue = 450f };
    public StaticCombinedModifier<float> DashCooldown { get; } = new() { BaseValue = 5f };
    public Keys DashKey { get; set; } = Keys.Space;

    private StatusEffector effector;
    private bool mayDash = true;

    protected override void Awake()
    {
        effector = FetchComponent<StatusEffector>();
        effector.OnEffectRemoved += (effector, effect) =>
        {
            if (effect is not DashCooldownEffect)
                return;
            mayDash = true;
        };
    }

    protected override void Update()
    {
        if (Input.GetKeyDown(DashKey) && mayDash)
        {
            var dir = Input.GetNormalizedWASDInput();

            transform.Translate(dir * DashDistance);
            mayDash = false;
            effector.Apply<DashCooldownEffect>(1, DashCooldown.Value);
            effector.Apply<FireStatusEffect>(1, 0.5f);
        }
    }

    private class DashCooldownEffect : StatusEffect
    {
        public override string Description => "A timeout during which you cant dash";
        public override StatusEffectUpdateType UpdateType => StatusEffectUpdateType.Static;
        public override StatusEffectType EffectType => StatusEffectType.Cooldown;

        public DashCooldownEffect() => MaxStacks = 1;
    }
}
