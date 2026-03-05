using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeWarden.Components.Buildin;

/// <summary>
/// Makes the object rotated towards the mouse at all times.
/// </summary>
public class LookAtMouse : Component, IUpdatable
{
    /// <summary>
    /// Represented in full rotations per second. eg 1 would take 1 second to do a full 360 rotation and 0.5s to do a 180
    /// </summary>
    public float RotationSpeed { get; set; } = 1;

    public void Update()
    {
        if (Camera.main is null)
        {
            log.Error("LookAtMouse requires a camera");
            return;
        }

        // Mouse position in world space
        Vector2 mouseWorldPos = Camera.main.ScreenToWorld(Input.MousePosition).Vec2();

        // 2D direction vector from object to mouse
        Vector2 direction = mouseWorldPos - new Vector2(transform.position.X, transform.position.Y);
        if (direction.LengthSquared() < 0.0001f)
            return; // too close, skip rotation

        // Angle in radians to point at the mouse
        float targetAngle = MathF.Atan2(-direction.Y, direction.X);

        // Correct for sprite front being up (+Y)
        targetAngle -= MathF.PI / 2f;

        // Invert rotation to match clockwise movement (optional, depends on coordinate system)
        targetAngle = -targetAngle;

        // Create quaternion rotation around Z axis
        Quaternion targetRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, targetAngle);

        // Smoothly rotate toward target
        float maxRadians = RotationSpeed * MathF.PI * 2f * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, MathF.Min(1f, maxRadians));
    }
}
