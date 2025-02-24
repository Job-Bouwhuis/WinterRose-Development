using Microsoft.Xna.Framework;
using System;
using static Microsoft.Xna.Framework.Input.Keys;

namespace WinterRose.Monogame.Tests.Scripts;

internal class SideScrollingController : ObjectBehavior
{
    [IncludeInTemplateCreation]
    public float Speed { get; set; } = 250f;
    [IncludeInTemplateCreation]
    public float SprintSpeed { get; set; } = 350f;
    [IncludeInTemplateCreation]
    public float JumpHeight { get; set; } = 700f;

    public float raycastDistance = 55;

    public SideScrollingController() { }
    public SideScrollingController(float speed) => Speed = speed;
     
    [Show]
    private bool grounded = false;

    Gravity grav;
    PhysicsObject physics;
    Raycaster raycaster;

    protected override void Awake()
    {
        grav = FetchOrAttachComponent<Gravity>();
        physics = FetchOrAttachComponent<PhysicsObject>();
        raycaster = FetchOrAttachComponent<Raycaster>();
        raycaster.IsVisible = true;
    }

    protected override void Update()
    {
        if (Input.GetKey(Space) && grounded)
        {
            physics.ApplyForce(new Vector2(0, -JumpHeight));
            grounded = false;
        }

        if (Input.GetKey(LeftShift))
            physics.Velocity = new(SprintSpeed, physics.Velocity.Y);
        else
            physics.Velocity = new(Speed, physics.Velocity.Y);

        if (raycaster.Raycast(transform.position, -transform.up, raycastDistance, .2f, out var hit))
        {
            grounded = true;
            grav.ApplyGravity = false;

            // if we are touching the ground, and the distance to the maximum distance is more than .1f. we are inside the ground. we need to move up in the y axis in steps of .1f until we are not inside the ground.
            if (hit.distanceRemaining > .4f)
                transform.position = new(transform.position.X, transform.position.Y - .4f);
        }
        else
        {
            grav.ApplyGravity = true;
            grounded = false;
        }

        // calculate acceleration for the physics object based on the input and max speed
        if (Input.GetKey(A))
            physics.Velocity = new(-Speed, physics.Velocity.Y);
        else if (Input.GetKey(D))
            physics.Velocity = new(Speed, physics.Velocity.Y);
        else
            physics.Velocity = new(0, physics.Velocity.Y);
    }
}
