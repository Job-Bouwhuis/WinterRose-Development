using Microsoft.Xna.Framework;
using System;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame;

public class ModyfiablePlayerMovement : ObjectBehavior
{
    [IncludeInTemplateCreation]
    public int BaseSpeed
    {
        get => AdditiveSpeedModifier.BaseValue;
        set => AdditiveSpeedModifier.SetBaseValue(value);
    }
    public StaticAdditiveModifier<int> AdditiveSpeedModifier { get; private set; } = 20;

    public ModyfiablePlayerMovement(int speed)
    {
        BaseSpeed = speed;
    }

    public ModyfiablePlayerMovement() { }

    private void Update()
    {
        // Calculate the target position based on input and transform.up
        Vector2 inputDirection = Input.GetNormalizedWASDInput();

        // Move the player towards the target position
        transform.position += inputDirection * AdditiveSpeedModifier.Value;
    }
}
