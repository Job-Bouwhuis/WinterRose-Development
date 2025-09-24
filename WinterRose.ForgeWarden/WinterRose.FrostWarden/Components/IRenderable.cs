using WinterRose.ForgeWarden.Components;

namespace WinterRose.ForgeWarden;

public interface IRenderable : IComponent
{
    void Draw(Matrix4x4 viewMatrix);
}
