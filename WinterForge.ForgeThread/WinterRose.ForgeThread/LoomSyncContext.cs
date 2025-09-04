namespace WinterRose.Threading;

/// <summary>
/// A SynchronizationContext bound to a LoomScheduler.
/// Ensures that async/await continuations return to the owning loom.
/// </summary>
internal class LoomSyncContext : SynchronizationContext
{
    private readonly LoomScheduler _scheduler;

    public LoomSyncContext(LoomScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        _scheduler.Enqueue(() =>
        {
            d(state);
            return Task.CompletedTask;
        });
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        // Since this scheduler is single-threaded, we can safely run inline.
        d(state);
    }
}