using WinterRose.Monogame;

namespace TopDownGame.Enemies.Movement;

internal class IdleMovement(string name) : AIMovement(name)
{
    public override void Move(Transform transform, float speed, float deltaTime)
    {
        // does nothing. AI is idle / stationary
    }
}
