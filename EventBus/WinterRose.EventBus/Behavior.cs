namespace WinterRose.EventBusses;

public abstract class Behavior
{
    /// <summary>
    /// The priority of this behavior, when multiple behaviors are subscribed to the same event, higher priority behaviors are executed first.
    /// </summary>
    public int Priority { get; set; } = 0;
    /// <summary>
    /// This dictionary holds any additional parameters that arent defined as class members
    /// </summary>
    public Dictionary<string, object?> OtherParams { get; } = new();
    /// <summary>
    /// The name of this behavior, derived from the class name. "Behavior" suffix is removed.
    /// </summary>
    public string BehaviorName { get; }

    protected Behavior()
    {
        Type type = GetType();
        BehaviorName = BehaviorPrototype.SanitizeBehaviorName(type.Name);
    }

    /// <summary>
    /// Called when the event this behavior is subscribed to is invoked.
    /// </summary>
    /// <param name="context"></param>
    public abstract void Execute(EventContext context);

    /// <summary>
    /// Build a new Behavior factory for the given behavior name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static BehaviorPrototype Of(string name) => new BehaviorPrototype(name);
}
