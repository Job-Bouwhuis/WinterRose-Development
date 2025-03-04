using Microsoft.Xna.Framework.Input;

using WinterRose.Monogame;
using WinterRose.StaticValueModifiers;

namespace TopDownGame.Players;

/// <summary>
/// Boostst the movement of the <see cref="ModifiablePlayerMovement"/> when holding a key
/// </summary>
[RequireComponent<ModifiablePlayerMovement>]
internal class PlayerSprint : ObjectBehavior
{
    ModifiablePlayerMovement movement;
    int modKey = -1;

    public StaticCombinedModifier<float> SprintSpeed { get; } = new() { BaseValue = 2f };

    public Keys SprintKey { get; set; } = Keys.LeftShift;

    protected override void Awake()
    {
        movement = FetchComponent<ModifiablePlayerMovement>();
    }

    protected override void Update()
    {
        if (Input.GetKey(SprintKey))
        {
            if (modKey != -1)
                movement.SpeedModifier.RemoveMultiplicative(modKey);

            modKey = movement.SpeedModifier.AddMultiplicative(SprintSpeed);
        }
        else
        {
            if (modKey != -1)
            {
                movement.SpeedModifier.RemoveMultiplicative(modKey);
                modKey = -1;
            }
        }
    }
}
