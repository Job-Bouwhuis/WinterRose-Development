using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.DependencyInjection;

interface IInjectionHandler<TAttribute> : IInjectionHandler where TAttribute : Attribute
{
    void Inject(Component component, MemberData member, TAttribute attribute);

    void IInjectionHandler.Inject(Component component, MemberData member, Attribute attribute)
        => Inject(component, member, (TAttribute)attribute);
}

interface IInjectionHandler
{
    void Inject(Component component, MemberData member, Attribute attribute);
}

