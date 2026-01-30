namespace WinterRose.EventBusses;

public sealed class Invocation<TIn1, TOut> : Invocation
{
    Invocation? continuation;

    public Invocation(Func<TIn1, TOut> method) : base(method) { }

    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Invocation<TOut, TNewOut> next)
    {
        continuation = next;
        return next;
    }
    public VoidInvocation<TOut> ContinueWith(VoidInvocation<TOut> next)
    {
        continuation = next;
        return next;
    }
    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Func<TOut, TNewOut> next)
    {
        var n = (Invocation<TOut, TNewOut>)next;

        ContinueWith(n);
        return n;
    }
    public VoidInvocation<TOut> ContinueWith(Action<TOut> next)
    {
        var n = (VoidInvocation<TOut>)next;
        ContinueWith(n);
        return n;
    }

    public override object? Invoke(params object?[]? args)
    {
        return Invoke((TIn1)args.FirstOrDefault());
    }

    public TOut Invoke(TIn1 arg)
    {
        TOut res = (TOut)base.Invoke(arg)!;
        continuation?.Invoke(res);
        return res;
    }

    public static implicit operator Invocation<TIn1, TOut>(Func<TIn1, TOut> method) => Create(method);
}

public sealed class Invocation<TIn1, TIn2, TOut> : Invocation
{
    Invocation? continuation;

    public Invocation(Func<TIn1, TIn2, TOut> method) : base(method) { }

    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Invocation<TOut, TNewOut> next)
    {
        continuation = next;
        return next;
    }
    public VoidInvocation<TOut> ContinueWith(VoidInvocation<TOut> next)
    {
        continuation = next;
        return next;
    }
    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Func<TOut, TNewOut> next)
    {
        var n = (Invocation<TOut, TNewOut>)next;

        ContinueWith(n);
        return n;
    }
    public VoidInvocation<TOut> ContinueWith(Action<TOut> next)
    {
        var n = (VoidInvocation<TOut>)next;
        ContinueWith(n);
        return n;
    }

    public TOut Invoke(TIn1 arg1, TIn2 arg2)
    {
        TOut res = (TOut)base.Invoke(arg1, arg2)!;
        continuation?.Invoke(res);
        return res;
    }

    public static implicit operator Invocation<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> method) => Create(method);
}

public sealed class Invocation<TIn1, TIn2, TIn3, TOut> : Invocation
{
    Invocation? continuation;

    public Invocation(Func<TIn1, TIn2, TIn3, TOut> method) : base(method) { }

    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Invocation<TOut, TNewOut> next)
    {
        continuation = next;
        return next;
    }
    public VoidInvocation<TOut> ContinueWith(VoidInvocation<TOut> next)
    {
        continuation = next;
        return next;
    }
    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Func<TOut, TNewOut> next)
        => ContinueWith(Invocation.Create(next));
    public VoidInvocation<TOut> ContinueWith(Action<TOut> next)
        => ContinueWith(Invocation.Create(next));

    public override object? Invoke(params object?[]? args)
        => base.Invoke(args?.Take(3).ToArray());

    public TOut Invoke(TIn1 arg1, TIn2 arg2, TIn3 arg3)
    {
        TOut res = (TOut)base.Invoke(arg1, arg2, arg3)!;
        continuation?.Invoke(res);
        return res;
    }

    public static implicit operator Invocation<TIn1, TIn2, TIn3, TOut>(Func<TIn1, TIn2, TIn3, TOut> method) => Create(method);
}

// 4 args
public sealed class Invocation<TIn1, TIn2, TIn3, TIn4, TOut> : Invocation
{
    Invocation? continuation;

    public Invocation(Func<TIn1, TIn2, TIn3, TIn4, TOut> method) : base(method) { }

    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Invocation<TOut, TNewOut> next)
    {
        continuation = next;
        return next;
    }
    public VoidInvocation<TOut> ContinueWith(VoidInvocation<TOut> next)
    {
        continuation = next;
        return next;
    }
    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Func<TOut, TNewOut> next)
        => ContinueWith(Invocation.Create(next));
    public VoidInvocation<TOut> ContinueWith(Action<TOut> next)
        => ContinueWith(Invocation.Create(next));

    public override object? Invoke(params object?[]? args)
        => base.Invoke(args?.Take(4).ToArray());

    public TOut Invoke(TIn1 arg1, TIn2 arg2, TIn3 arg3, TIn4 arg4)
    {
        TOut res = (TOut)base.Invoke(arg1, arg2, arg3, arg4)!;
        continuation?.Invoke(res);
        return res;
    }

    public static implicit operator Invocation<TIn1, TIn2, TIn3, TIn4, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TOut> method) => Create(method);
}

// 5 args
public sealed class Invocation<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> : Invocation
{
    Invocation? continuation;

    public Invocation(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> method) : base(method) { }

    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Invocation<TOut, TNewOut> next)
    {
        continuation = next;
        return next;
    }
    public VoidInvocation<TOut> ContinueWith(VoidInvocation<TOut> next)
    {
        continuation = next;
        return next;
    }
    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Func<TOut, TNewOut> next)
        => ContinueWith(Invocation.Create(next));
    public VoidInvocation<TOut> ContinueWith(Action<TOut> next)
        => ContinueWith(Invocation.Create(next));

    public override object? Invoke(params object?[]? args)
        => base.Invoke(args?.Take(5).ToArray());

    public TOut Invoke(TIn1 arg1, TIn2 arg2, TIn3 arg3, TIn4 arg4, TIn5 arg5)
    {
        TOut res = (TOut)base.Invoke(arg1, arg2, arg3, arg4, arg5)!;
        continuation?.Invoke(res);
        return res;
    }

    public static implicit operator Invocation<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> method) => Create(method);
}

public sealed class Invocation<TOut> : Invocation
{
    Invocation? continuation;

    public Invocation(Func<TOut> method) : base(method) { }

    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Invocation<TOut, TNewOut> next)
    {
        continuation = next;
        return next;
    }
    public VoidInvocation<TOut> ContinueWith(VoidInvocation<TOut> next)
    {
        continuation = next;
        return next;
    }

    public Invocation<TOut, TNewOut> ContinueWith<TNewOut>(Func<TOut, TNewOut> next)
    {
        var n = (Invocation<TOut, TNewOut>)next;

        ContinueWith(n);
        return n;
    }
    public VoidInvocation<TOut> ContinueWith(Action<TOut> next)
    {
        var n = (VoidInvocation<TOut>)next;
        ContinueWith(n);
        return n;
    }

    public override object? Invoke(params object?[]? args)
    {
        return Invoke();
    }

    public TOut Invoke()
    {
        TOut res = (TOut)base.Invoke()!;
        continuation?.Invoke(res);
        return res;
    }

    public static implicit operator Invocation<TOut>(Func<TOut> method) => Create(method);
}
