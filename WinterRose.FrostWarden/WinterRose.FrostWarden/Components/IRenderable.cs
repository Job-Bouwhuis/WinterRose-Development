namespace WinterRose.FrostWarden.Components;

public interface IRenderable : IComponent
{
    void Draw(Matrix4x4 viewMatrix);
}
