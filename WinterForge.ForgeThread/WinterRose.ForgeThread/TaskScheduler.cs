using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Threading;

/// <summary>
/// Tick-based task scheduler for a single thread.
/// Runs enqueued delegates cooperatively, one tick at a time.
/// </summary>
internal class LoomScheduler
{
    private readonly Queue<Func<Task>> _queue = new();
    private readonly object _lock = new();

    public SynchronizationContext SyncContext { get; }

    public LoomScheduler()
    {
        SyncContext = new LoomSyncContext(this);
    }

    public void Enqueue(Func<Task> func)
    {
        lock (_lock)
        {
            _queue.Enqueue(func);
        }
    }

    /// <summary>
    /// Execute all queued items for this tick.
    /// Should be called by the owning thread (main loop or worker).
    /// </summary>
    public void RunTick()
    {
        List<Func<Task>> toRun;
        lock (_lock)
        {
            toRun = new List<Func<Task>>(_queue);
            _queue.Clear();
        }

        foreach (var func in toRun)
        {
            try
            {
                var task = func();
                if (task == null)
                    continue;

                // ensure exceptions bubble properly
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.Error.WriteLine($"[LoomScheduler] Unhandled exception: {t.Exception}");
                    }
                }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[LoomScheduler] Exception during RunTick: {ex}");
            }
        }
    }
}
