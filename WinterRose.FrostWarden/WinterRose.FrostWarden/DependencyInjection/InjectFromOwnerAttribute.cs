using WinterRose.FrostWarden.Entities;

namespace WinterRose.FrostWarden;

/// <summary>
/// The system automatically assigns this field or property to the component instance of the field type, found on the owner of which the current component's instance is attached to
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InjectFromOwnerAttribute : Attribute;

/// <summary>
/// The system automatically assigns this field or property to the component instance of the field, found anywhere in the <see cref="World"/> under any <see cref="Entity"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InjectFromAttribute : Attribute
{
    public string? EntityName { get; set; }
    public string[]? Tags { get; set; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InjectFromChildrenAttribute : InjectFromAttribute;
