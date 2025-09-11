namespace WinterRose.ForgeSignal;

public class Invocation
{
    // 0 arg
    public static Invocation<TOut> Create<TOut>(Func<TOut> func) => new Invocation<TOut>(func);
    public static VoidInvocation Create(Action action) => new VoidInvocation(action);

    // 1 arg
    public static Invocation<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> func) => new Invocation<TIn, TOut>(func);
    public static VoidInvocation<TIn> Create<TIn>(Action<TIn> action) => new VoidInvocation<TIn>(action);

    // 2 args
    public static Invocation<TIn1, TIn2, TOut> Create<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> func)
        => new Invocation<TIn1, TIn2, TOut>(func);
    public static VoidInvocation<TIn1, TIn2> Create<TIn1, TIn2>(Action<TIn1, TIn2> action)
        => new VoidInvocation<TIn1, TIn2>(action);

    // 3 args
    public static Invocation<TIn1, TIn2, TIn3, TOut> Create<TIn1, TIn2, TIn3, TOut>(Func<TIn1, TIn2, TIn3, TOut> func)
        => new Invocation<TIn1, TIn2, TIn3, TOut>(func);
    public static VoidInvocation<TIn1, TIn2, TIn3> Create<TIn1, TIn2, TIn3>(Action<TIn1, TIn2, TIn3> action)
        => new VoidInvocation<TIn1, TIn2, TIn3>(action);

    // 4 args
    public static Invocation<TIn1, TIn2, TIn3, TIn4, TOut> Create<TIn1, TIn2, TIn3, TIn4, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TOut> func)
        => new Invocation<TIn1, TIn2, TIn3, TIn4, TOut>(func);
    public static VoidInvocation<TIn1, TIn2, TIn3, TIn4> Create<TIn1, TIn2, TIn3, TIn4>(Action<TIn1, TIn2, TIn3, TIn4> action)
        => new VoidInvocation<TIn1, TIn2, TIn3, TIn4>(action);

    // 5 args
    public static Invocation<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> Create<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> func)
        => new Invocation<TIn1, TIn2, TIn3, TIn4, TIn5, TOut>(func);
    public static VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5> Create<TIn1, TIn2, TIn3, TIn4, TIn5>(Action<TIn1, TIn2, TIn3, TIn4, TIn5> action)
        => new VoidInvocation<TIn1, TIn2, TIn3, TIn4, TIn5>(action);

    //public static Invocation Create(Delegate del) => new(del);
    public static MulticastInvocation CreateMulticast() => new();

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
