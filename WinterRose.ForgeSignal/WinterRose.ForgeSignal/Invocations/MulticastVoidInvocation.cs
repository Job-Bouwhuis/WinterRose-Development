namespace WinterRose.EventBusses;

public sealed class MulticastVoidInvocation : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal VoidInvocation invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastVoidInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public void Invoke() => InvokeInternal();

    public Subscription Subscribe(VoidInvocation invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

    public Subscription Subscribe(Action invocation) => Subscribe(Create(invocation));

    private void InvokeInternal()
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        List<Exception> exceptions = [];

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                sub.invocation.Invoke();
            }
            catch (Exception ex) // is only thrown when the invocation doesnt specify a error invocation
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            if(error is not null)
                error?.Invoke(new AggregateException(exceptions));
            else
                throw new AggregateException(exceptions);
        }
    }
}
public sealed class MulticastVoidInvocation<T1> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal VoidInvocation<T1> invocation;
        internal bool isUnsubscribing = false;
        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastVoidInvocation() => method = (object?[]? args) => InvokeInternal((T1)args![0]!);

    private readonly List<Subscription> invocations = new();

    public void Invoke(T1 arg1) => InvokeInternal(arg1);

    public Subscription Subscribe(VoidInvocation<T1> invocation)
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

    public Subscription Subscribe(Action<T1> action) => Subscribe(Invocation.Create(action));

    private void InvokeInternal(T1 arg1)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        var exceptions = new List<Exception>();

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                sub.invocation.Invoke(arg1);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            if (error is null)
                throw new AggregateException(exceptions);
            error?.Invoke(new AggregateException(exceptions));
        }
    }
}
public sealed class MulticastVoidInvocation<T1, T2> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal VoidInvocation<T1, T2> invocation;
        internal bool isUnsubscribing = false;
        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastVoidInvocation() => method = (object?[]? args) => InvokeInternal((T1)args![0]!, (T2)args![1]!);

    private readonly List<Subscription> invocations = new();

    public void Invoke(T1 arg1, T2 arg2) => InvokeInternal(arg1, arg2);

    public static MulticastVoidInvocation<T1, T2> operator +(MulticastVoidInvocation<T1, T2> a, Action<T1, T2> b)
    {
        a.Subscribe(b);
        return a;
    }

    public Subscription Subscribe(VoidInvocation<T1, T2> invocation)
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

    public Subscription Subscribe(Action<T1, T2> invocation) => Subscribe(Create(invocation));

    private void InvokeInternal(T1 arg1, T2 arg2)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        var exceptions = new List<Exception>();

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                sub.invocation.Invoke(arg1, arg2);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            if(error is null)
                throw new AggregateException(exceptions);
            error?.Invoke(new AggregateException(exceptions));
            
        }
    }
}
public sealed class MulticastVoidInvocation<T1, T2, T3> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal VoidInvocation<T1, T2, T3> invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastVoidInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public void Invoke(T1 arg1, T2 arg2, T3 arg3) => InvokeInternal(arg1, arg2, arg3);

    public Subscription Subscribe(VoidInvocation<T1, T2, T3> invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

    private void InvokeInternal(T1 arg1, T2 arg2, T3 arg3)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        List<Exception> exceptions = [];

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                sub.invocation.Invoke(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            error?.Invoke(new AggregateException(exceptions));
    }
}
public sealed class MulticastVoidInvocation<T1, T2, T3, T4> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal VoidInvocation<T1, T2, T3, T4> invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastVoidInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => InvokeInternal(arg1, arg2, arg3, arg4);

    public Subscription Subscribe(VoidInvocation<T1, T2, T3, T4> invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

    private void InvokeInternal(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        List<Exception> exceptions = [];

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                sub.invocation.Invoke(arg1, arg2, arg3, arg4);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            error?.Invoke(new AggregateException(exceptions));
    }
}
public sealed class MulticastVoidInvocation<T1, T2, T3, T4, T5> : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal VoidInvocation<T1, T2, T3, T4, T5> invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastVoidInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => InvokeInternal(arg1, arg2, arg3, arg4, arg5);

    public Subscription Subscribe(VoidInvocation<T1, T2, T3, T4, T5> invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

    private void InvokeInternal(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        Subscription[] snapshot;
        lock (invocations)
        {
            invocations.RemoveAll(s => s.isUnsubscribing);
            snapshot = invocations.ToArray();
        }

        List<Exception> exceptions = [];

        foreach (var sub in snapshot)
        {
            if (sub.isUnsubscribing)
                continue;

            try
            {
                sub.invocation.Invoke(arg1, arg2, arg3, arg4, arg5);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            error?.Invoke(new AggregateException(exceptions));
    }
}
