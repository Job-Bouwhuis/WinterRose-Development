namespace WinterRose.ForgeWarden;

/// <summary>
/// The system automatically assigns this field or property to the component instance of the field type, found on the owner of which the current component's instance is attached to
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InjectFromSelfAttribute : Attribute
{
    /// <summary>
    /// Optionally throw when the target component was not found. Defaults to <see langword="false"/>
    /// </summary>
    public bool ThrowWhenAbsent { get; set; }
}