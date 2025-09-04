using System.Collections.Concurrent;
using System.Reflection;

namespace WinterRose.ForgeSignal;

public sealed class Signal : IDisposable
{
    private readonly struct Unit { }

    // --- UPDATED: static API (added parameterless overloads) ---
    public static Subscription On(Action handler, SubscribeOptions? options = null) =>
        Global.Subscribe<Unit>(_ => handler(), options);

    public static Subscription OnAsync(Func<Task> handler, SubscribeOptions? options = null) =>
        Global.SubscribeAsync<Unit>(_ => handler(), options);

    public static int Fire() => Global.Publish(default(Unit));

    public static Task<int> FireAsync() => Global.PublishAsync(default(Unit));

    public static int ClearGlobal<T>() => Global.Clear<T>();

    public static int ClearGlobal() => Global.Clear<Unit>();

    public static void ClearAllGlobal() => Global.ClearAll();

    // --- UPDATED: parameterless subscribe helpers (non-generic wrappers) ---
    public Subscription Subscribe(Action handler, SubscribeOptions? options = null)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        return Subscribe<Unit>(_ => handler(), options);
    }

    public Subscription SubscribeAsync(Func<Task> handler, SubscribeOptions? options = null)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        return SubscribeAsync<Unit>(_ => handler(), options);
    }

    // --- UPDATED: Publish / PublishAsync usages (no target/scheduler logic) ---
    // Note: replaced Dispatch(h.Target, ...) with Dispatch(() => ...) and
    // DispatchAsync(() => ...) which now simply invoke the delegate.

    public int Publish<T>(T payload)
    {
        if (disposed) throw new ObjectDisposedException(nameof(Signal));
        if (!handlers.TryGetValue(typeof(T), out var list)) return 0;

        List<HandlerEntry> snapshot;
        lock (list) snapshot = list.ToList();

        int invoked = 0;
        foreach (var h in snapshot)
        {
            if (!h.IsAlive)
            {
                Remove(typeof(T), h);
                continue;
            }

            if (h.TryGet(out var del))
            {
                invoked++;
                if (del is Action<T> a)
                    Dispatch(() => a(payload));
                else if (del is Func<T, Task> f)
                    _ = DispatchAsync(() => f(payload)); // fire-and-forget async handlers
            }

            if (h.Once) Remove(typeof(T), h);
        }

        return invoked;
    }

    public Task<int> PublishAsync<T>(T payload)
    {
        if (disposed) throw new ObjectDisposedException(nameof(Signal));
        if (!handlers.TryGetValue(typeof(T), out var list)) return Task.FromResult(0);

        List<HandlerEntry> snapshot;
        lock (list) snapshot = list.ToList();

        var tasks = new List<Task>(snapshot.Count);
        int invoked = 0;

        foreach (var h in snapshot)
        {
            if (!h.IsAlive)
            {
                Remove(typeof(T), h);
                continue;
            }

            if (h.TryGet(out var del))
            {
                invoked++;
                if (del is Action<T> a)
                    tasks.Add(DispatchAsync(() => { a(payload); return Task.CompletedTask; }));
                else if (del is Func<T, Task> f)
                    tasks.Add(DispatchAsync(() => f(payload)));
            }

            if (h.Once) Remove(typeof(T), h);
        }

        return tasks.Count == 0
            ? Task.FromResult(invoked)
            : WhenAllWithCount(tasks, invoked);
    }

    // --- UPDATED: Dispatch methods — now simple synchronous/async invocations ---
    // removed any scheduler / synccontext / AllowMultiThread checks
    static void Dispatch(Action action)
    {
        action();
    }

    static Task DispatchAsync(Func<Task> action)
    {
        return action();
    }

    readonly ConcurrentDictionary<Type, List<HandlerEntry>> handlers = new();
    volatile bool disposed;

    public static Signal Global { get; } = new Signal();

    public Subscription Subscribe<T>(
        Action<T> handler,
        SubscribeOptions? options = null)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        var entry = HandlerEntry.Create(handler, options);
        var list = handlers.GetOrAdd(typeof(T), _ => new List<HandlerEntry>());
        lock (list) list.Add(entry);
        return new Subscription(typeof(T), entry, this);
    }

    public Subscription SubscribeAsync<T>(
        Func<T, Task> handler,
        SubscribeOptions? options = null)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        var entry = HandlerEntry.Create(handler, options);
        var list = handlers.GetOrAdd(typeof(T), _ => new List<HandlerEntry>());
        lock (list) list.Add(entry);
        return new Subscription(typeof(T), entry, this);
    }

    public int Clear<T>()
    {
        if (!handlers.TryRemove(typeof(T), out var list)) return 0;
        lock (list) list.Clear();
        return 1;
    }

    public void ClearAll()
    {
        foreach (var kv in handlers.Keys.ToArray())
        {
            if (handlers.TryRemove(kv, out var list))
                lock (list) list.Clear();
        }
    }

    public void Unsubscribe(Subscription subscription)
    {
        if (subscription == null) return;
        Remove(subscription.PayloadType, subscription.Entry);
    }

    void Remove(Type type, HandlerEntry entry)
    {
        if (!handlers.TryGetValue(type, out var list)) return;
        lock (list) list.Remove(entry);
    }

    static async Task<int> WhenAllWithCount(List<Task> tasks, int count)
    {
        await Task.WhenAll(tasks).ConfigureAwait(false);
        return count;
    }

    public void Dispose()
    {
        disposed = true;
        ClearAll();
    }
}

