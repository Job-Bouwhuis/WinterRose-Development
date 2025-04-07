using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.StatusSystem;

namespace WinterRose.Monogame.Tests.StatusEffects;

internal class SpeedyGonzales : StatusEffect
{
    public override string Description => "makes you more speedy";

    public override StatusEffectUpdateType UpdateType => StatusEffectUpdateType.Static;

    public float SpeedBuff { get; set; } = 200f;

    public override StatusEffectType EffectType => StatusEffectType.Neutral;

    private float speedBeforeBuff = 0f;

    int key = 0;

    protected override void StacksUpdated(StatusEffector effector, int last, int current)
    {
        if (!effector.TryFetchComponent<TopDownPlayerController>(out var controller))
        {
            Stacks = 0;
            return;
        }

        if (current == 1)
        {
            speedBeforeBuff = controller.Speed;
            key = 1;
            controller.Speed += SpeedBuff;
        }
        else
        {
            controller.Speed = speedBeforeBuff;
        }
    }
}
