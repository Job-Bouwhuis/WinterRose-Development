using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.Tests
{
    public class SmoothLookAtMouse : Component, IUpdatable
    {
        public StaticCombinedModifier<float> RotationSpeed { get; set; } = 180f;

        public void Update()
        {
            Vector2 mouseScreenPos = Raylib.GetMousePosition();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorld(mouseScreenPos);

            Vector2 direction = new(mouseWorldPos.X - transform.position.X, mouseWorldPos.Y - transform.position.Y);

            if (direction.LengthSquared() < 0.0001f)
                return;

            Vector2 forward = Vector2.Normalize(direction);

            float targetAngle = MathF.Atan2(forward.Y, forward.X); 
            Quaternion targetRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, targetAngle);

            float t = RotationSpeed * Raylib.GetFrameTime() * (MathF.PI / 180f);
            float fraction = Math.Clamp(t / transform.rotation.Angle(targetRotation), 0f, 1f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, fraction);
        }
    }
}
