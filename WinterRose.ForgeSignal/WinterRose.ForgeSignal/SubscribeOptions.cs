namespace WinterRose.ForgeSignal;

public sealed class SubscribeOptions
{
    public bool Weak { get; set; }
    public bool Once { get; set; }
    public DispatchTarget Target { get; set; } = DispatchTarget.Current;
}

