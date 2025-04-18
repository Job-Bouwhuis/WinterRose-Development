using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using WinterRose.Serialization;

namespace WinterRose.Monogame.Tests;

internal class BallSpawner : ObjectBehavior
{
    [IncludeWithSerialization]
    public Sprite Sprite { get; set; }

    public BallSpawner(Sprite sprite) => Sprite = sprite;

    [ExcludeFromSerialization]
    public List<WorldObject> spawned = [];

    int balls = 0;

    public int ballsPerSpawn = 3;

    protected override void Update()
    {
        if (Input.Space)
        {
            foreach (int i in ballsPerSpawn)
            {
                SpawnBall();
            }
        }
    }

    private void SpawnBall()
    {
        balls++;
        var ball = world.CreateObject("ball" + balls);
        ball.transform.position = transform.position with { Y = transform.position.Y + 30 };
        ball.AttachComponent<SpriteRenderer>(Sprite);
        var phyisics = ball.AttachComponent<PhysicsObject>();
        ball.AttachComponent<CircleCollider>().Radius = 6;
        ball.AttachComponent<PhysicsBounds>();
        ball.AttachComponent<Gravity>();
        ball.FetchComponent<PhysicsObject>().Velocity = FetchComponent<PhysicsObject>().Velocity;

        spawned.Add(ball);

        // apply random force to the ball
        var random = new Random();
        var force = new Vector2(random.Next(-1000, 1000), random.Next(-1000, 1000));
        phyisics.ApplyForce(force);
    }
}