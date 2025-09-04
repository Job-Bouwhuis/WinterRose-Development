namespace WinterRose.Threading;

/// <summary>
/// A SynchronizationContext bound to a LoomScheduler.
/// Ensures that async/await continuations return to the owning loom.
/// </summary>
internal class LoomSyncContext : SynchronizationContext
{
    private readonly LoomScheduler scheduler;

    public LoomSyncContext(LoomScheduler scheduler)
    {
        this.scheduler = scheduler;
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        scheduler.Queue(Task.Factory.StartNew(
            () => d(state),
            CancellationToken.None,
            TaskCreationOptions.None,
            scheduler
        ));
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        d(state);
    }
}