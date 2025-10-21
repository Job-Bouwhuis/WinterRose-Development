using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.DependencyInjection.Handlers;
internal class InjectAssetHandler : IInjectionHandler<InjectAssetAttribute>
{
    public void Inject(Component component, MemberData member, InjectAssetAttribute attribute)
    {
        object? asset = CreateAsset(attribute.AssetName, member.Type);
        if (attribute.ThrowWhenAbsent)
            Forge.Expect(asset).Not.Null();
        member.SetValue(component, asset);
    }

    public object? CreateAsset(string name, Type targetType)
    {
        Type assetsType = typeof(Assets);
        var loadMethodInfo = assetsType.GetMethod(
            nameof(Assets.Load),
            1,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
            [typeof(string)]
        );
        var genericLoadMethod = loadMethodInfo!.MakeGenericMethod(targetType);
        return genericLoadMethod.Invoke(null, [name]);
    }

}
