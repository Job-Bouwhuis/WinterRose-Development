using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WinterRose.Monogame.Worlds;
using WinterRose.Monogame;
using System;
using System.Collections;
using System.Net;

namespace WinterRose.Monogame;
#nullable enable

/// <summary>
/// Simple top down player controller 
/// </summary>
public class TopDownPlayerController : ObjectBehavior
{
    [IncludeWithSerialization]
    public float Speed { get; set; } = 410f;

    [ExcludeFromSerialization]
    PhysicsObject physics;

    public TopDownPlayerController(int speed) : this() => this.Speed = speed;
    public TopDownPlayerController() { }

    protected override void Awake()
    {
        physics = FetchOrAttachComponent<PhysicsObject>();
    }

    protected override void Update()
    {
        // Calculate the target position based on input and transform.up
        Vector2 inputDirection = Input.GetNormalizedWASDInput();
 
        // apply acceleration to the physics object based on the input direction to achieve the target velocity. gradually accelerate to the target velocity
        // and gradually decelerate to 0 velocity when there is no input
        // and gradually lower the acceleration when the target velocity is reached


        // calculate the target velocity based on the input direction and the speed
        var targetVelocity = inputDirection * Speed;

        // calculate the acceleration needed to achieve the target velocity
        var acceleration = (targetVelocity - physics.Velocity) / 0.1f;

        // apply the acceleration to the physics object
        physics.ApplyForce(acceleration);
    }
}
