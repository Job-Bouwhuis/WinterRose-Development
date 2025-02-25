using Microsoft.Xna.Framework;

namespace WinterRose.Monogame;

/// <summary>
/// Top down player movement that does not use physics.
/// </summary>
public class PlayerMovement : ObjectBehavior
{
    /// <summary>
    /// The speed in pixels per second.
    /// </summary>
    public float Speed { get; set; } = 410f;

    public PlayerMovement(int speed) : this() => this.Speed = speed;
    public PlayerMovement() { }

    protected override void Update()
    {
        // Calculate the target position based on input and transform.up
        Vector2 inputDirection = Input.GetNormalizedWASDInput();
        Vector2 localUp = transform.up;
        var target = transform.position + (inputDirection.X * localUp + inputDirection.Y * new Vector2(-localUp.Y, localUp.X));

        // Move the player towards the target position
        transform.position += Vector2.Lerp(transform.position, target, Speed * Time.SinceLastFrame);
    }
}
