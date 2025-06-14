using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Components;

namespace WinterRose.FrostWarden.Tests
{
    class Mover : Component, IUpdatable
    {
        public float speed = 1000f;

        private float fadeAmount = 0f;
        private const float fadeSpeed = 5.1f;

        public void Update()
        {
            Vector2 input = Vector2.Zero;

            if (ray.IsKeyPressed(KeyboardKey.F))
                owner.GetComponent<SpriteRenderer>()!.Shader.TrySetValue("fade", true);

            if (ray.IsKeyDown(KeyboardKey.E))
                transform.rotation -= new Quaternion(0, 0, 1, 0);

            if (ray.IsKeyDown(KeyboardKey.Q))
                transform.rotation += new Quaternion(0, 0, 1, 0);

            if (Raylib.IsKeyDown(KeyboardKey.W)) input.Y -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.S)) input.Y += 1;
            if (Raylib.IsKeyDown(KeyboardKey.A)) input.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.D)) input.X += 1;

            if (input != Vector2.Zero)
            {
                input = Vector2.Normalize(input);
                Vector3 move = new(input.X, input.Y, 0);
                owner.transform.position += move * speed * Raylib.GetFrameTime();
            }
        }
    }
}
