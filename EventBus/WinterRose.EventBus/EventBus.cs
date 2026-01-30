using WinterRose.WinterForgeSerializing;

namespace WinterRose.EventBusses;

public sealed class EventBus
{
    [WFInclude]
    private Dictionary<Subscription, Behavior> Subscriptions = new();

    public Subscription Subscribe(string eventName, Behavior prototype)
    {
        Subscription sub = new(eventName);
        Subscriptions[sub] = prototype;
        return sub;
    }

    public void Invoke(string eventName, params EventValue[] args)
    {
        EventContext context = new(eventName);
        foreach (var arg in args)
            context.Args[arg.Name] = arg.Value;

        List<Subscription> toRemove = new();
        List<Subscription> toExecute = new();
        foreach (var kvp in Subscriptions)
        {
            Subscription subscription = kvp.Key;
            Behavior behavior = kvp.Value;
            if (subscription.IsUnsubscribed)
                toRemove.Add(subscription);
            else if (subscription.EventName == eventName)
                toExecute.Add(subscription);
        }

        foreach (var sub in toRemove)
            Subscriptions.Remove(sub);

        toExecute.Sort((a, b) => Subscriptions[b].Priority.CompareTo(Subscriptions[a].Priority));

        foreach (var sub in toExecute)
        {
            Behavior behavior = Subscriptions[sub];
            behavior.Execute(context);
            if (context.IsConsumed)
                break;
        }
    }

    [BeforeSerialize]
    internal void Cleanup()
    {
        List<Subscription> toRemove = new();
        foreach (var kvp in Subscriptions)
        {
            Subscription subscription = kvp.Key;
            if (subscription.IsUnsubscribed)
                toRemove.Add(subscription);
        }
        foreach (var sub in toRemove)
            Subscriptions.Remove(sub);
    }
}
