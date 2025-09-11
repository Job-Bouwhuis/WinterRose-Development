namespace WinterRose.ForgeSignal;

public sealed class MulticastInvocation : Invocation
{
    public class Subscription : IDisposable
    {
        internal Guid id;
        internal Invocation invocation;
        internal bool isUnsubscribing = false;

        public void Unsubscribe() => Dispose();
        public void Dispose() => isUnsubscribing = true;
    }

    public MulticastInvocation() => method = InvokeInternal;

    private List<Subscription> invocations = [];

    public void Invoke() => InvokeInternal();

    public Subscription Subscribe(Invocation invocation)
    {
        Subscription sub = new()
        {
            id = Guid.CreateVersion7(),
            invocation = invocation
        };
        invocations.Add(sub);
        return sub;
    }

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
            error?.Invoke(new AggregateException(exceptions));
    }
}
