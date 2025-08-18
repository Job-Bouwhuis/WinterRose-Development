using WinterRose;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeWarden.Entities;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.DependencyInjection.Handlers;

class InjectFromWorldHandler : IInjectionHandler<InjectFromAttribute>
{
    public void Inject(Component component, MemberData member, InjectFromAttribute attr)
    {
        Entity? targetEntity = component.owner.world._Entities
            .FirstOrDefault(e =>
                (string.IsNullOrEmpty(attr.EntityName) || e.Name == attr.EntityName) &&
                (attr.Tags == null || attr.Tags.Length == 0 || attr.Tags.All(tag => e.Tags.Contains(tag)))
            );

        if (attr.ThrowWhenAbsent)
            Forge.Expect(targetEntity).Not.Null();

        if (targetEntity is not null)
        {
            var target = targetEntity.GetComponent(member.Type);
            if (attr.ThrowWhenAbsent)
                Forge.Expect(target).Not.Null();
            member.SetValue(component, target);
        }
    }
}

