using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using WinterRose.StaticValueModifiers;

namespace TopDownGame.Enemies.Movement;

public sealed class AIMovementController : ObjectBehavior
{
    public AIMovement Current { get; private set; } = new IdleMovement("Idle");
    public Dictionary<string, AIMovement> Movements { get; set; } = [];
    public StaticCombinedModifier<float> MovementSpeed { get; private set; }

    protected override void Update()
    {

    }
}
