using Microsoft.Xna.Framework;
using System;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame;

public class ModyfiablePlayerMovement : ObjectBehavior
{
    public float BaseSpeed
    {
        get => AdditiveSpeedModifier.BaseValue;
        set => AdditiveSpeedModifier.SetBaseValue(value);
    }
    public StaticAdditiveModifier<float> AdditiveSpeedModifier { get; } = new();

    [DefaultArguments(20)]
    public ModyfiablePlayerMovement(int speed)
    {
        BaseSpeed = speed;
    }

    private const float MAX_ZOOM_OUT = .7f; // Define the maximum zoom-out level
    private const float ZOOM_IN_SPEED = 600f; // Speed to zoom back in when no input is detected

    private void Update()
    {
        // Calculate the target position based on input and transform.up
        Vector2 inputDirection = Input.GetNormalizedWASDInput();

        // Move the player towards the target position
        transform.position += inputDirection * AdditiveSpeedModifier.Value;

        // Check if Camera.main exists and adjust zoom based on movement speed
        if (Camera.current != null)
        {
            float targetZoom;
            if (inputDirection.LengthSquared() > 0) // If input is detected
            {
                // Calculate the movement magnitude (speed)
                float movementMagnitude = inputDirection.Length() * BaseSpeed;
                targetZoom = 1.0f - (movementMagnitude * 5f); // Adjust zoom sensitivity as needed

                targetZoom = Math.Clamp(targetZoom, MAX_ZOOM_OUT, 1f);

                Camera.current.Zoom = MathS.Lerp(Camera.current.Zoom, targetZoom, 2 * Time.SinceLastFrame);
            }
            else // No input detected
            {
                // Quickly interpolate back to zoom level of 1
                Camera.current.Zoom = Math.Clamp(MathS.Lerp(Camera.current.Zoom, 1f, ZOOM_IN_SPEED * Time.SinceLastFrame), 0, 1);
            }
        }
    }
}
