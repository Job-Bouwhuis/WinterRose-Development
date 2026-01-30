namespace WinterRose.EventBusses;

public sealed class MulticastInvocation<TOut> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal Invocation<TOut> invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public MulticastInvocationResult<TOut> Invoke() => InvokeInternal();

    public Subscription Subscribe(Invocation<TOut> invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

    private MulticastInvocationResult<TOut> InvokeInternal()
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        var exceptions = new List<Exception>();

        MulticastInvocationResult<TOut> result = new();

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                TOut? res = sub.invocation.Invoke();
                result.AddResult(res);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            if (error is not null)
                error.Invoke(new AggregateException(exceptions));
            else
                throw new AggregateException(exceptions);

        return result;
    }
}

public sealed class MulticastInvocation<T1, TOut> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal Invocation<T1, TOut> invocation;
        internal bool isUnsubscribing = false;
        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastInvocation() => method = (object?[]? args) => InvokeInternal((T1)args![0]!);

    private readonly List<Subscription> invocations = new();

    public MulticastInvocationResult<TOut> Invoke(T1 arg1) => InvokeInternal(arg1);

    public Subscription Subscribe(Invocation<T1, TOut> invocation)
    {
        var sub = new Subscription
        {
            id = Guid.NewGuid(),
            invocation = invocation
        };
        lock (invocations)
        {
            invocations.Add(sub);
        }
        return sub;
    }

    private MulticastInvocationResult<TOut> InvokeInternal(T1 arg1)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        var exceptions = new List<Exception>();

        MulticastInvocationResult<TOut> result = new();

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                TOut? res = sub.invocation.Invoke(arg1);
                result.AddResult(res);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            if (error is not null)
                error.Invoke(new AggregateException(exceptions));
            else
                throw new AggregateException(exceptions);

        return result;
    }
}
public sealed class MulticastInvocation<T1, T2, TOut> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal Invocation<T1, T2, TOut> invocation;
        internal bool isUnsubscribing = false;
        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastInvocation() => method = (object?[]? args) => InvokeInternal((T1)args![0]!, (T2)args![1]!);

    private readonly List<Subscription> invocations = new();

    public MulticastInvocationResult<TOut> Invoke(T1 arg1, T2 arg2) => InvokeInternal(arg1, arg2);

    public Subscription Subscribe(Invocation<T1, T2, TOut> invocation)
    {
        var sub = new Subscription
        {
            id = Guid.NewGuid(),
            invocation = invocation
        };
        lock (invocations)
        {
            invocations.Add(sub);
        }
        return sub;
    }

    private MulticastInvocationResult<TOut> InvokeInternal(T1 arg1, T2 arg2)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        var exceptions = new List<Exception>();

        MulticastInvocationResult<TOut> result = new();

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                TOut? res = sub.invocation.Invoke(arg1, arg2);
                result.AddResult(res);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            if (error is not null)
                error.Invoke(new AggregateException(exceptions));
            else
                throw new AggregateException(exceptions);

        return result;
    }
}
public sealed class MulticastInvocation<T1, T2, T3, TOut> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal Invocation<T1, T2, T3, TOut> invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public MulticastInvocationResult<TOut> Invoke(T1 arg1, T2 arg2, T3 arg3) => InvokeInternal(arg1, arg2, arg3);

    public Subscription Subscribe(Invocation<T1, T2, T3, TOut> invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

    private MulticastInvocationResult<TOut> InvokeInternal(T1 arg1, T2 arg2, T3 arg3)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        var exceptions = new List<Exception>();

        MulticastInvocationResult<TOut> result = new();

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                TOut? res = sub.invocation.Invoke(arg1, arg2, arg3);
                result.AddResult(res);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            if (error is not null)
                error.Invoke(new AggregateException(exceptions));
            else
                throw new AggregateException(exceptions);

        return result;
    }
}
public sealed class MulticastInvocation<T1, T2, T3, T4, TOut> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal Invocation<T1, T2, T3, T4, TOut> invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public MulticastInvocationResult<TOut> Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => InvokeInternal(arg1, arg2, arg3, arg4);

    public Subscription Subscribe(Invocation<T1, T2, T3, T4, TOut> invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

    private MulticastInvocationResult<TOut> InvokeInternal(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        var exceptions = new List<Exception>();

        MulticastInvocationResult<TOut> result = new();

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                TOut? res = sub.invocation.Invoke(arg1, arg2, arg3, arg4);
                result.AddResult(res);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            if (error is not null)
                error.Invoke(new AggregateException(exceptions));
            else
                throw new AggregateException(exceptions);

        return result;
    }
}
public sealed class MulticastInvocation<T1, T2, T3, T4, T5, TOut> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal Invocation<T1, T2, T3, T4, T5, TOut> invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public MulticastInvocationResult<TOut> Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => InvokeInternal(arg1, arg2, arg3, arg4, arg5);

    public Subscription Subscribe(Invocation<T1, T2, T3, T4, T5, TOut> invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

    private MulticastInvocationResult<TOut> InvokeInternal(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        var exceptions = new List<Exception>();

        MulticastInvocationResult<TOut> result = new();

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                TOut? res = sub.invocation.Invoke(arg1, arg2, arg3, arg4, arg5);
                result.AddResult(res);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            if (error is not null)
                error.Invoke(new AggregateException(exceptions));
            else
                throw new AggregateException(exceptions);

        return result;
    }
}