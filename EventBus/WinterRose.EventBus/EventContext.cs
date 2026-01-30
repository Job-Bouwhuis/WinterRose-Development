using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WinterRose.AnonymousTypes;

namespace WinterRose.EventBusses;

public sealed class EventContext
{
    public string EventName { get; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Anonymous Args
    {
        get => field ??= new Anonymous();
        set => field = value;
    }

    public object? this[string key]
    {
        get => Args[key];
        set => Args[key] = value;
    }

    public T? Get<T>(string key) => Args.Get<T>(key);

    public bool IsConsumed { get; private set; }
    public void Consume() => IsConsumed = true;

    public EventContext(string eventName)
    {
        EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
    }
}
