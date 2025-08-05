using System;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.StaticValueModifiers;

namespace TopDownGame.Enemies.Movement;

public abstract class AIMovement(string name)
{
    [WFInclude]
    public AIMovementController Controller { get; set; }

    [WFInclude]
    public string Name { get; private set; } = name;

    public abstract void Move();

    public abstract void TransitionOut(AIMovement next);
    public abstract void TransitionIn(AIMovement current);
}
