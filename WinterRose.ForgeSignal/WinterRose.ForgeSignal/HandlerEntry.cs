namespace WinterRose.ForgeSignal;

public sealed class HandlerEntry
{
    readonly Delegate? strong;
    readonly WeakReference? weak;
    public readonly bool Once;
    public readonly DispatchTarget Target;

    HandlerEntry(Delegate strong, bool once, DispatchTarget target)
    {
        this.strong = strong;
        Once = once;
        Target = target;
        weak = null;
    }

    HandlerEntry(WeakReference weak, bool once, DispatchTarget target)
    {
        this.weak = weak;
        Once = once;
        Target = target;
        strong = null;
    }

    public static HandlerEntry Create(Delegate del, SubscribeOptions? options)
    {
        var opt = options ?? new SubscribeOptions();
        if (opt.Weak)
            return new HandlerEntry(new WeakReference(del), opt.Once, opt.Target);
        return new HandlerEntry(del, opt.Once, opt.Target);
    }

    public bool IsAlive => strong != null || (weak?.IsAlive ?? false);

    public bool TryGet(out Delegate? del)
    {
        if (strong != null) { del = strong; return true; }
        del = weak?.Target as Delegate;
        return del != null;
    }
}

