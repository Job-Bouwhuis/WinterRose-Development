using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.DependencyInjection.Handlers;

class InjectFromParentHandler : IInjectionHandler<InjectFromParentAttribute>
{
    public void Inject(Component component, MemberData member, InjectFromParentAttribute attr)
    {
        if (attr.ThrowWhenAbsent)
            if (component.transform.parent is null)
                throw new NullReferenceException($"Entity {component.owner.Name} has no parent to take components from");

        var target = component.transform.parent?.GetComponent(member.Type);
        if (attr.ThrowWhenAbsent)
            throw new NullReferenceException($"Parent of entity {component.owner.Name} does not have a component of type {member.Type.FullName} at the time of adding component {component.GetType().FullName}");

        member.SetValue(component, target);
    }
}