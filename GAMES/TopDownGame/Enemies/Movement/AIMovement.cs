using System;
using WinterRose.Monogame;
using WinterRose.Serialization;
using WinterRose.StaticValueModifiers;

namespace TopDownGame.Enemies.Movement;

public abstract class AIMovement(string name)
{
    [IncludeWithSerialization]
    public AIMovementController Controller { get; set; }

    [IncludeWithSerialization]
    public string Name { get; private set; } = name;

    public abstract void Move();

    public abstract void TransitionOut(AIMovement next);
    public abstract void TransitionIn(AIMovement current);
}
