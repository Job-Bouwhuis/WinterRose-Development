using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden;
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InjectAssetAttribute(string assetName) : Attribute
{
    public string AssetName => assetName;

    /// <summary>
    /// Optionally throw when the target asset was not found. Defaults to <see langword="false"/>
    /// </summary>
    public bool ThrowWhenAbsent { get; set; } = false;
}
