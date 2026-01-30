namespace WinterRose.EventBusses;

public sealed class Subscription : IDisposable
{
    public Subscription(string eventName)
    {
        EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
    }
    private Subscription() { } // for serialization

    public string EventName { get; private set; }
    internal bool IsUnsubscribed { get; private set; } = false;
    public void Dispose() => Unsubscribe();
    public void Unsubscribe() 
    {
        IsUnsubscribed = true;
    }
}
