namespace WinterRose.ForgeSignal;

public sealed class Subscription : IDisposable
{
    public Type PayloadType { get; }
    public HandlerEntry Entry { get; }
    readonly Signal bus;
    volatile bool disposed;

    internal Subscription(Type payloadType, HandlerEntry entry, Signal bus)
    {
        PayloadType = payloadType;
        Entry = entry;
        this.bus = bus;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        bus.Unsubscribe(this);
    }
}

