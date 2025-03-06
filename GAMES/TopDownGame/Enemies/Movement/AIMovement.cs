using WinterRose.Monogame;
using WinterRose.StaticValueModifiers;

namespace TopDownGame.Enemies.Movement;

public abstract class AIMovement(string name)
{
    public string Name { get; private set; } = name;

    public abstract void Move(Transform transform, float speed, float deltaTime);
}
