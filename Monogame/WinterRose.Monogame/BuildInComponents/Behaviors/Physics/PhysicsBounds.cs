using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

public class PhysicsBounds : ObjectComponent
{
    PhysicsObject physics;

    public float GrabityDampening { get; set; } = .60f;

    /// <summary>
    /// The bounds of where this object can move. If the object is outside of these bounds it will be moved back inside.
    /// </summary>
    public RectangleF Bounds { get; set; } = new(MonoUtils.WindowResolution.X, MonoUtils.WindowResolution.Y, 0, 0);

    protected override void Awake()
    {
        if (!TryFetchComponent(out physics))
            physics = owner.AttachComponent<PhysicsObject>();

        physics.PhysicsUpdate += UpdatePhysics;
    }

    private void UpdatePhysics(float deltaTime)
    {
        Debug.DrawRectangle(Bounds, Color.Magenta);

        // Calculate the bounds with the object's size
        RectangleF adjustedBounds = Bounds;

        // Check for collision with world bounds
        if (transform.position.X - physics.ObjectSize.X / 2 < adjustedBounds.Left)
        {
            transform.position = new(adjustedBounds.Left + physics.ObjectSize.X / 2, transform.position.Y);
            physics.Velocity = new(physics.Velocity.X * -GrabityDampening, physics.Velocity.Y);
        }
        else if (transform.position.X + physics.ObjectSize.X / 2 > adjustedBounds.Right)
        {
            transform.position = new(adjustedBounds.Right - physics.ObjectSize.X / 2, transform.position.Y);
            physics.Velocity = new(physics.Velocity.X * -GrabityDampening, physics.Velocity.Y);
        }

        if (transform.position.Y - physics.ObjectSize.Y / 2 < adjustedBounds.Top)
        {
            transform.position = new(transform.position.X, adjustedBounds.Top + physics.ObjectSize.Y / 2);
            physics.Velocity = new(physics.Velocity.X, physics.Velocity.Y * -GrabityDampening);
        }
        else if (physics.transform.position.Y + physics.ObjectSize.Y / 2 > adjustedBounds.Bottom)
        {
            transform.position = new(transform.position.X, adjustedBounds.Bottom - physics.ObjectSize.Y / 2);
            physics.Velocity = new(physics.Velocity.X, physics.Velocity.Y * -GrabityDampening);
        }
    }
}
