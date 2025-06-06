using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;

namespace WinterRose.FrostWarden;

public abstract class Component : IComponent
{
    public Entity owner { get; internal set; }
    public Transform transform => owner.transform;
}
