using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.DialogBoxes;
using WinterRose.ForgeWarden.ToastNotifications;

namespace WinterRose.ForgeWarden.Tests
{
    class Mover : Component, IUpdatable
    {
        public float speed = 1000f;

        private float fadeAmount = 0f;
        private const float fadeSpeed = 5.1f;

        Dialog d = new DefaultDialog("Horizontal Big",
                    "refer to \\L[https://github.com/Job-Bouwhuis/WinterRose.WinterForge|WinterForge github page] for info",
                    DialogPlacement.HorizontalBig, buttons: ["Ok"], priority: DialogPriority.High)
        {
            Style = new()
            {
                DialogBackground = Color.Red
            }
        };

        public void Update()
        {
            
            Vector2 input = Vector2.Zero;

            if (ray.IsKeyPressed(KeyboardKey.F))
                Toasts.ShowToast(new Toast("test", ToastType.Info, Random.Shared.Next(70, 220)));

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
