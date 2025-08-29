namespace WinterRose.ForgeWarden;

/// <summary>
/// When applied to a field where the type is assignable to <see cref="Component"/>, the system automatically seeks the component type of the field its applied to within the children of the object this component is attached to
/// <br></br><br></br>
/// eg:<br></br>
/// player - seeks Vitality component<br></br>
/// health bar - has Vitality component attached<br></br><br></br>
///     
/// before Awake is called on the component, the values are set provided they are found
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InjectFromChildrenAttribute : InjectFromAttribute;
