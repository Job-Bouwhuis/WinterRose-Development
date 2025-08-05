using Microsoft.Xna.Framework;
using System;
using WinterRose.Monogame;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame;

public class ModifiablePlayerMovement : ObjectBehavior
{
    [IncludeInTemplateCreation, WFInclude]
    public StaticCombinedModifier<float> SpeedModifier { get; private set; } = 200;

    public ModifiablePlayerMovement(float speed) => SpeedModifier = speed;

    public ModifiablePlayerMovement() { }

    protected override void Update()
    {
        // Calculate the target position based on input and transform.up
        Vector2 inputDirection = Input.GetNormalizedWASDInput();

        // Move the player towards the target position
        transform.position += inputDirection * SpeedModifier.Value * Time.deltaTime;
    }
}
