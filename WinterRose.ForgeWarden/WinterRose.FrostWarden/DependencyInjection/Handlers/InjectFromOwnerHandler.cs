using WinterRose.ForgeGuardChecks;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.DependencyInjection.Handlers;

class InjectFromOwnerHandler : IInjectionHandler<InjectFromSelfAttribute>
{
    public void Inject(Component component, MemberData member, InjectFromSelfAttribute attr)
    {
        var target = component.GetComponent(member.Type);
        if (attr.ThrowWhenAbsent)
            Forge.Expect(target).Not.Null();
        member.SetValue(component, target);
    }
}
