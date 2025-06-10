using BulletSharp;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
using WinterRose.FrostWarden.Physics;

namespace WinterRose.FrostWarden.Tests
{
    public class BallSpawner : Component, IUpdatable
    {
        public void Update()
        {
            if (Raylib.IsKeyDown(KeyboardKey.Space))
            {
                Entity ball = new Entity();

                var sr = new SpriteRenderer(Sprite.CreateCircle(25, new Color(255, 0, 255)));
                ball.Add(sr);

                ball.transform.position = transform.position;

                var col = new Collider(new SphereShape(25 / 2));
                var rb = new RigidBodyComponent(col, 250);

                ball.Add(rb);

                owner.world.AddEntity(ball);
            }
        }
    }
}
