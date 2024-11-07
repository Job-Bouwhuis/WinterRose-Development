using Microsoft.Xna.Framework;
using System;

namespace WinterRose.Monogame;

public sealed class PhysicsObject : ObjectComponent
{
    public Vector2 Velocity { get; set; }
    public Vector2 Acceleration { get; set; }
    public Vector2 Drag { get; set; } = new Vector2(0.5f, 0.5f);
    private float mass = 1;

    public RectangleF ObjectSize { get; set; } = new RectangleF(1, 1, 0, 0);

    public float Mass
    {
        get => mass;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Mass must be greater than 0");
            }
            mass = value;
        }
    }

    public Action<float> PhysicsUpdate { get; internal set; } = delegate { };

    public void ApplyForce(Vector2 force)
    {
        Acceleration += force / Mass;
    }

    public void ApplyForce(float x, float y)
    {
        ApplyForce(new Vector2(x, y));
    }

    public void ApplyForce(float force)
    {
        ApplyForce(new Vector2(force));
    }

    public void ApplyForceX(float force)
    {
        ApplyForce(new Vector2(force, 0));
    }

    public void ApplyForceY(float force)
    {
        ApplyForce(new Vector2(0, force));
    }

    internal void UpdatePhysicsSubSteps(float deltaTime, int subSteps = 1)
    {
        for (int i = 0; i < subSteps; i++)
        {
            PhysicsUpdate(deltaTime / subSteps);
        }

        // Apply the acceleration to the velocity
        Velocity += Acceleration * deltaTime;

        // Apply drag to the acceleration
        Acceleration *= Drag;

        if (Acceleration.LengthSquared() < 0.001f)
        {
            Acceleration = Vector2.Zero;
        }

        if (Velocity.LengthSquared() < 0.001f)
        {
            Velocity = Vector2.Zero;
        }
        if(Acceleration == Vector2.Zero)
        {
            Velocity *= Drag;
        }

        // Apply gravity but only let it go as fast as its terminal velocity


        // Apply the velocity to the position
        transform.position += Velocity * deltaTime;
    }
}
