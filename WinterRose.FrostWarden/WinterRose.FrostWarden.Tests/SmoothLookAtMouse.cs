using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.Tests
{
    class SmoothLookAtMouse : Component, IUpdatable
    {
        private const float RotationSpeedDegreesPerSecond = 180f;

        // Helper method for shortest lerp on angles in degrees
        private float LerpAngle(float from, float to, float t)
        {
            float delta = ((to - from + 180f) % 360f) - 180f;
            return from + delta * t;
        }

        public void Update()
        {
            // Get current rotation Z in degrees
            float currentAngle = transform.rotation.Z;

            // Get mouse position in world space
            Vector2 mouseScreenPos = Raylib.GetMousePosition();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorld(mouseScreenPos); // You’ll want your method to convert screen coords to world coords here

            // Vector from object to mouse
            Vector2 direction = new Vector2(mouseWorldPos.X - transform.position.X, mouseWorldPos.Y - transform.position.Y);

            if (direction.LengthSquared() < 0.0001f)
                return; // Avoid divide by zero or jitter

            // Calculate the target angle in degrees (0 degrees is along +X axis)
            float targetAngle = MathF.Atan2(direction.Y, direction.X) * (180f / MathF.PI);

            // Smoothly lerp current rotation to target angle based on rotation speed and frame time
            float maxDelta = RotationSpeedDegreesPerSecond * Raylib.GetFrameTime();
            float newAngle = LerpAngle(currentAngle, targetAngle, maxDelta / MathF.Abs(((targetAngle - currentAngle + 180f) % 360f) - 180f));

            // Update the transform rotation (in degrees)
            transform.rotationEulerDegrees = new Vector3(transform.rotation.X, transform.rotation.Y, newAngle);
        }
    }
}
