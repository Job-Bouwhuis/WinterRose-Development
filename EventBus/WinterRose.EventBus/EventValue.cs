namespace WinterRose.EventBusses;

public record struct EventValue(string Name, object? Value)
{
    public static implicit operator EventValue((string Name, object? Value) tuple) => new EventValue(tuple.Name, tuple.Value);
}
