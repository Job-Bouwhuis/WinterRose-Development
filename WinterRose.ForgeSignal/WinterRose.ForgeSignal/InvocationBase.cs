namespace WinterRose.ForgeSignal;

public class Invocation
{
    public static Invocation<TOut> Create<TOut>(Func<TOut> func) => new Invocation<TOut>(func);
    public static VoidInvocation Create(Action action) => new VoidInvocation(action);

    public static Invocation<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> func) => new Invocation<TIn, TOut>(func);
    public static VoidInvocation<TIn> Create<TIn>(Action<TIn> action) => new VoidInvocation<TIn>(action);

    public static Invocation<TIn1, TIn2, TOut> Create<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> func)
        => new Invocation<TIn1, TIn2, TOut>(func);
    public static VoidInvocation<TIn1, TIn2> Create<TIn1, TIn2>(Action<TIn1, TIn2> action)
        => new VoidInvocation<TIn1, TIn2>(action);

    public static Invocation<TIn1, TIn2, TIn3, TOut> Create<TIn1, TIn2, TIn3, TOut>(Func<TIn1, TIn2, TIn3, TOut> func)
        => new Invocation<TIn1, TIn2, TIn3, TOut>(func);
    public static VoidInvocation<TIn1, TIn2, TIn3> Create<TIn1, TIn2, TIn3>(Action<TIn1, TIn2, TIn3> action)
        => new VoidInvocation<TIn1, TIn2, TIn3>(action);

    public static Invocation<TIn1, TIn2, TIn3, TIn4, TOut> Create<TIn1, TIn2, TIn3, TIn4, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TOut> func)
        => new Invocation<TIn1, TIn2, TIn3, TIn4, TOut>(func);
    public static VoidInvocation<TIn1, TIn2, TIn3, TIn4> Create<TIn1, TIn2, TIn3, TIn4>(Action<TIn1, TIn2, TIn3, TIn4> action)
        => new VoidInvocation<TIn1, TIn2, TIn3, TIn4>(action);

    public static Invocation<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Create<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> func)
        => new Invocation<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(func);
    public static VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> Create<TIn1, TIn2, TIn3, TIn4, TIn5>(Action<TIn1, TIn2, TIn3, TIn4, TIn5> action)
        => new VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5>(action);

    public static MulticastVoidInvocation CreateVoidMulticast() => new();
    public static MulticastVoidInvocation<T1> CreateVoidMulticast<T1>() => new();
    public static MulticastVoidInvocation<T1, T2> CreateVoidMulticast<T1, T2>() => new();
    public static MulticastVoidInvocation<T1, T2, T3> CreateVoidMulticast<T1, T2, T3>() => new();
    public static MulticastVoidInvocation<T1, T2, T3, T4> CreateVoidMulticast<T1, T2, T3, T4>() => new();
    public static MulticastVoidInvocation<T1, T2, T3, T4, T5> CreateVoidMulticast<T1, T2, T3, T4, T5>() => new();

    public static MulticastInvocation<TOut> CreateMulticast<TOut>() => new();
    public static MulticastInvocation<T1, TOut> CreateMulticast<T1, TOut>() => new();
    public static MulticastInvocation<T1, T2, TOut> CreateMulticast<T1, T2, TOut>() => new();
    public static MulticastInvocation<T1, T2, T3, TOut> CreateMulticast<T1, T2, T3, TOut>() => new();
    public static MulticastInvocation<T1, T2, T3, T4, TOut> CreateMulticast<T1, T2, T3, T4, TOut>() => new();
    public static MulticastInvocation<T1, T2, T3, T4, T5, TOut> CreateMulticast<T1, T2, T3, T4, T5, TOut>() => new();

    protected Delegate method;
    protected Invocation? before;
    protected Invocation? after;
    protected Invocation? FinallyInvoc;
    protected VoidInvocation<Exception> error;

    protected Invocation(Delegate method) => this.method = method;

    // exists for Multicast Invocation
    protected Invocation() { }

    public virtual Invocation Before(Invocation method)
    {
        before = method;
        return this;
    }

    public virtual Invocation After(Invocation method)
    {
        after = method;
        return this;
    }

    public virtual Invocation Error(VoidInvocation<Exception> method)
    {
        error = method;
        return this;
    }
    public virtual Invocation Error(Action<Exception> method)
    {
        error = method;
        return this;
    }

    public virtual Invocation Finally(Invocation method)
    {
        FinallyInvoc = method;
        return this;
    }

    public virtual object? Invoke(params object?[]? args)
    {
        object? res = null;
        try
        {
            before?.Invoke(args);
            res = InvocationArgs.ValidateAndInvoke(method, args);
            after?.Invoke(args);
        }
        catch (Exception e)
        {
            if (error is null)
                throw;
            error.Invoke(e);
        }
        finally
        {
            FinallyInvoc?.Invoke(args);
        }
        return res;
    }
}
