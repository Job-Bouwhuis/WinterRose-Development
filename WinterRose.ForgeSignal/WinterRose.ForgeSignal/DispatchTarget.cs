namespace WinterRose.ForgeSignal;

public readonly struct DispatchTarget
{
    public readonly SynchronizationContext? SyncContext;
    public readonly TaskScheduler? Scheduler;

    public DispatchTarget(SynchronizationContext? syncContext, TaskScheduler? scheduler)
    {
        SyncContext = syncContext;
        Scheduler = scheduler;
    }

    public static DispatchTarget Current =>
        new DispatchTarget(SynchronizationContext.Current, TaskScheduler.Current);
}

