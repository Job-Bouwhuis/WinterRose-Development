using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace WinterRose.Monogame;

public class Gravity : ObjectComponent
{
    PhysicsObject physics;

    PhysicsBounds? Bounds;

    public const float GRAVITY = 9.81f;

    public float GravityForce { get; set; } = MathF.Pow(GRAVITY, 2);

    public bool ApplyGravity { get; set; } = true;


    protected override void Awake()
    {
        if (!TryFetchComponent(out physics))
            physics = owner.AttachComponent<PhysicsObject>();

        if (TryFetchComponent(out Bounds)) ; // if no bounds are found, we keep applying gravity.

        physics.PhysicsUpdate += UpdatePhysics;
    }

    private void UpdatePhysics(float deltaTime)
    {
        if (!ApplyGravity)
            return;
        // apply gravity if we are not touching or in very close proximity to the ground
        if (Bounds == null || physics.transform.position.Y + physics.ObjectSize.Height / 2 < Bounds.Bounds.Bottom - 0.1f)
        {
            physics.ApplyForce(new Vector2(0, GravityForce));
            return;
        }
    }
}
