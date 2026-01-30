namespace WinterRose.EventBusses;

public sealed class VoidInvocation : Invocation
{
    internal VoidInvocation(Action method) : base(method) { }

    public new VoidInvocation Before(Action before)
    {
        this.before = Invocation.Create(before);
        return this;
    }
    public new VoidInvocation After(Action after)
    {
        this.after = Invocation.Create(after);
        return this;
    }

    public new void Invoke()
    {
        base.Invoke();
    }

    public static implicit operator VoidInvocation(Action action) => Create(action);
}

// 1 arg
public sealed class VoidInvocation<TIn1> : Invocation
{
    internal VoidInvocation(Action<TIn1> method) : base(method) { }

    public VoidInvocation<TIn1> Before(Action<TIn1> before)
    {
        this.before = Create(before);
        return this;
    }
    public VoidInvocation<TIn1> After(Action<TIn1> after)
    {
        this.after = Create(after);
        return this;
    }

    public VoidInvocation<TIn1> Before(VoidInvocation<TIn1> before)
    {
        this.before = before;
        return this;
    }
    public VoidInvocation<TIn1> After(VoidInvocation<TIn1> after)
    {
        this.after = after;
        return this;
    }

    public new void Invoke(TIn1 arg1)
    {
        base.Invoke(arg1);
    }

    public static implicit operator VoidInvocation<TIn1>(Action<TIn1> action) => Create(action);
}

// 2 args
public sealed class VoidInvocation<TIn1, TIn2> : Invocation
{
    internal VoidInvocation(Action<TIn1, TIn2> method) : base(method) { }

    public VoidInvocation<TIn1, TIn2> Before(Action<TIn1, TIn2> before) => (VoidInvocation<TIn1, TIn2>)(this.before = Create(before));
    public VoidInvocation<TIn1, TIn2> After(Action<TIn1, TIn2> after) => (VoidInvocation<TIn1, TIn2>)(this.after = Create(after));

    public VoidInvocation<TIn1, TIn2> Before(VoidInvocation<TIn1, TIn2> before) { this.before = before; return this; }
    public VoidInvocation<TIn1, TIn2> After(VoidInvocation<TIn1, TIn2> after) { this.after = after; return this; }

    public new void Invoke(TIn1 arg1, TIn2 arg2) => base.Invoke(arg1, arg2);

    public static implicit operator VoidInvocation<TIn1, TIn2>(Action<TIn1, TIn2> action) => Create(action);
}

// 3 args
public sealed class VoidInvocation<TIn1, TIn2, TIn3> : Invocation
{
    internal VoidInvocation(Action<TIn1, TIn2, TIn3> method) : base(method) { }

    public VoidInvocation<TIn1, TIn2, TIn3> Before(Action<TIn1, TIn2, TIn3> before) => (VoidInvocation<TIn1, TIn2, TIn3>)(this.before = Create(before));
    public VoidInvocation<TIn1, TIn2, TIn3> After(Action<TIn1, TIn2, TIn3> after) => (VoidInvocation<TIn1, TIn2, TIn3>)(this.after = Create(after));

    public VoidInvocation<TIn1, TIn2, TIn3> Before(VoidInvocation<TIn1, TIn2, TIn3> before) { this.before = before; return this; }
    public VoidInvocation<TIn1, TIn2, TIn3> After(VoidInvocation<TIn1, TIn2, TIn3> after) { this.after = after; return this; }

    public new void Invoke(TIn1 arg1, TIn2 arg2, TIn3 arg3) => base.Invoke(arg1, arg2, arg3);

    public static implicit operator VoidInvocation<TIn1, TIn2, TIn3>(Action<TIn1, TIn2, TIn3> action) => Create(action);
}

// 4 args
public sealed class VoidInvocation<TIn1, TIn2, TIn3, TIn4> : Invocation
{
    internal VoidInvocation(Action<TIn1, TIn2, TIn3, TIn4> method) : base(method) { }

    public VoidInvocation<TIn1, TIn2, TIn3, TIn4> Before(Action<TIn1, TIn2, TIn3, TIn4> before) => (VoidInvocation<TIn1, TIn2, TIn3, TIn4>)(this.before = Create(before));
    public VoidInvocation<TIn1, TIn2, TIn3, TIn4> After(Action<TIn1, TIn2, TIn3, TIn4> after) => (VoidInvocation<TIn1, TIn2, TIn3, TIn4>)(this.after = Create(after));

    public VoidInvocation<TIn1, TIn2, TIn3, TIn4> Before(VoidInvocation<TIn1, TIn2, TIn3, TIn4> before) { this.before = before; return this; }
    public VoidInvocation<TIn1, TIn2, TIn3, TIn4> After(VoidInvocation<TIn1, TIn2, TIn3, TIn4> after) { this.after = after; return this; }

    public new void Invoke(TIn1 arg1, TIn2 arg2, TIn3 arg3, TIn4 arg4) => base.Invoke(arg1, arg2, arg3, arg4);

    public static implicit operator VoidInvocation<TIn1, TIn2, TIn3, TIn4>(Action<TIn1, TIn2, TIn3, TIn4> action) => Create(action);
}

// 5 args
public sealed class VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> : Invocation
{
    internal VoidInvocation(Action<TIn1, TIn2, TIn3, TIn4, TIn5> method) : base(method) { }

    public VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> Before(Action<TIn1, TIn2, TIn3, TIn4, TIn5> before)
        => (VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5>)(this.before = Create(before));
    public VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> After(Action<TIn1, TIn2, TIn3, TIn4, TIn5> after)
        => (VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5>)(this.after = Create(after));

    public VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> Before(VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> before) { this.before = before; return this; }
    public VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> After(VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> after) { this.after = after; return this; }

    public new void Invoke(TIn1 arg1, TIn2 arg2, TIn3 arg3, TIn4 arg4, TIn5 arg5)
        => base.Invoke(arg1, arg2, arg3, arg4, arg5);

    public static implicit operator VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5>(Action<TIn1, TIn2, TIn3, TIn4, TIn5> action)
        => Create(action);
}