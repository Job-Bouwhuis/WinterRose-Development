using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.StaticValueModifiers;

namespace TopDownGame.Enemies.Movement;

[ParallelBehavior]
public sealed class AIMovementController : ObjectBehavior
{
    [IncludeWithSerialization]
    public AIMovement Current { get; private set; }
    [IncludeWithSerialization]
    public Dictionary<string, AIMovement> Movements { get; set; } = [];
    [IncludeWithSerialization]
    public StaticCombinedModifier<float> MovementSpeed { get; private set; } = 170;

    public Transform Target { get; set; }

    [IncludeWithSerialization]
    public float VisionRange { get; set; } = 1500;

    [IncludeWithSerialization]
    public float EvadeDistance { get; set; } = 200;

    protected override void Update()
    {
        if (Target == null)
        {
            Target = world.FindObjectWithFlag("Player")?.transform!;
            if(Target == null)
                throw new InvalidOperationException("No target!");
        }
        Current.Controller = this;
        Current.Move();
    }

    public T AddMovement<T>() where T : AIMovement
    {
        T t = ActivatorExtra.CreateInstance<T>()!;

        if (Movements.Any(x => x.Value.Name == t.Name))
            throw new InvalidOperationException($"Behavior with name {t.Name} already exists!");

        Movements.Add(t.Name, t);
        t.Controller = this;
        Current ??= t; // set Current to the first behavior added.
        return t;
    }

    public void SetMovement(string name)
    {
        AIMovement next = Movements[name]
            ?? throw new NullReferenceException("No AIBehavior exists with name " + name);

        Current.TransitionOut(next);
        next.TransitionIn(Current);
        Current = next;
    }
}
