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
        // Get the Type object for the Assets class
        Type assetsType = typeof(Assets);

        // Look for the public static generic method "Load" with no parameters
        var loadMethodInfo = assetsType.GetMethod(
            nameof(Assets.Load),
            1,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
            [typeof(string)]
        );

        if (loadMethodInfo == null)
            return null; // Method not found

        // Construct the generic method using the target type
        var genericLoadMethod = loadMethodInfo.MakeGenericMethod(targetType);

        // Invoke the static method (no instance, no parameters)
        return genericLoadMethod.Invoke(null, [name]);
    }

}
