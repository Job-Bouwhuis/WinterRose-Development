using Microsoft.Xna.Framework;

namespace WinterRose.Monogame.Tests;

internal class MouseFollow : ObjectBehavior
{
    [IncludeInTemplateCreation]
    public float Speed { get; set; } = 10f;
    [IncludeInTemplateCreation]
    public bool Lerped { get; set; } = true;
    protected override void Update()
    {
        Vector2 targetPos = Transform.ScreenToWorldPos(Input.MousePosition, Camera.current);

        if (Lerped)
            // lerp the position to the target position according to the speed
            transform.position = Vector2.Lerp(transform.position, targetPos, Speed * (float)Time.SinceLastFrame);
        else
            transform.position = targetPos;
    }
}