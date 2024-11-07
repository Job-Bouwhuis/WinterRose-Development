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

    private void Update()
    {
        // Calculate the target position based on input and transform.up
        Vector2 inputDirection = Input.GetNormalizedWASDInput();

        // Move the player towards the target position
        transform.position += inputDirection * AdditiveSpeedModifier.Value;
    }
}
