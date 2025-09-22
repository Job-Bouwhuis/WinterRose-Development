namespace WinterRose.StateKeeper;

public class ValueTrigger<TItem, TValue> : Trigger
{
    private readonly Func<TItem, TValue> valueGetter;
    private readonly TValue expectedValue;
    private readonly TItem item;

    public ValueTrigger(TItem item, Func<TItem, TValue> valueGetter, TValue expectedValue, State targetState)
        : base(targetState)
    {
        this.item = item;
        this.valueGetter = valueGetter;
        this.expectedValue = expectedValue;
    }

    public override bool IsTriggered()
    {
        return EqualityComparer<TValue>.Default.Equals(valueGetter(item), expectedValue);
    }
}
