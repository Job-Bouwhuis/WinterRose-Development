using System;

namespace WinterRose.Monogame;

/// <summary>
/// Use this if you wish to override the names of methods that the framework calls inside  your components, methods such as "Update", "Awake", "Start", "Close", ect
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class OverrideDefaultMethodNamesAttribute : Attribute
{
    internal string awake = "Awake";
    internal string start = "Start";
    internal string close = "Close";
    internal string update = "Update";

    public OverrideDefaultMethodNamesAttribute() { }

    /// <summary>
    /// The method that is run at the very start
    /// </summary>
    public string Awake
    {
        get { return awake; }
        set { awake = value; }
    }
    /// <summary>
    /// The method that runs when the object is set to enabled, and right after <see cref="Awake"/> was called
    /// </summary>
    public string Start
    {
        get { return start; }
        set { start = value; }
    }
    /// <summary>
    /// The method that runs when the object is being destroyed
    /// </summary>
    public string Close
    {
        get { return close; }
        set { close = value; }
    }
    /// <summary>
    /// The method that runs once every frame, only applies to <see cref="ObjectBehavior"/>, and not <see cref="ObjectComponent"/>
    /// </summary>
    public string Update
    {
        get { return update; }
        set { update = value; }
    }
}
