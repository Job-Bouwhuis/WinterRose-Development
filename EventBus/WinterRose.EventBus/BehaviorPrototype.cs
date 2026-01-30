using WinterRose.Reflection;

namespace WinterRose.EventBusses;

public sealed class BehaviorPrototype
{
    private Behavior prototypeInstance;
    private ReflectionHelper reflection;

    public BehaviorPrototype WithParam(string name, object? value) 
    {
        if(reflection.TryGetMember(name, out var member))
        {
            member.SetValue(prototypeInstance, value);
            return this;
        }

        prototypeInstance.OtherParams[name] = value; 
        return this;
    }
    public BehaviorPrototype WithPriority(int p) 
    { 
        prototypeInstance.Priority = p; 
        return this; 
    }

    private static Dictionary<string, Type> behaviorCache = [];

    static BehaviorPrototype()
    {
        Type[] types = TypeWorker.FindTypesWithBase<Behavior>();
        foreach (Type type in types)
        {
            string behaviorName = SanitizeBehaviorName(type.Name);
            behaviorCache[behaviorName] = type;
        }
    }

    public BehaviorPrototype(string name)
    {
        if (!behaviorCache.TryGetValue(name, out Type? type))
            throw new ArgumentException($"No behavior found with name '{name}'", nameof(name));
        prototypeInstance = (Behavior)Activator.CreateInstance(type)!;
        reflection = new ReflectionHelper(prototypeInstance);
    }
    public static implicit operator Behavior(BehaviorPrototype prototype) => prototype.prototypeInstance;

    internal static string SanitizeBehaviorName(string name)
    {
        if (name.EndsWith("Behavior"))
            name = name[..^"Behavior".Length];
        return name;
    }
}
