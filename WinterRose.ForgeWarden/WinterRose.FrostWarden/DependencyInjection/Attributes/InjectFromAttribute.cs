using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden;

/// <summary>
/// The system automatically assigns this field or property to the component instance of the field, found anywhere in the <see cref="World"/> under any <see cref="Entity"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InjectFromAttribute : Attribute
{
    /// <summary>
    /// Optionally specify a name for an entity to search the target component from
    /// </summary>
    public string? EntityName { get; set; }
    /// <summary>
    /// Optionally specify one or more tags the entity must have in order for the component instance to be selected
    /// </summary>
    public string[]? Tags { get; set; }
    /// <summary>
    /// Optionally throw when the target component was not found. Defaults to <see langword="false"/>
    /// </summary>
    public bool ThrowWhenAbsent { get; set; }
}
