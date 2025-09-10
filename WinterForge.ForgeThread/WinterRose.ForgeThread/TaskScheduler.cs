using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Threading;

/// <summary>
/// Tick-based task scheduler for a single thread.
/// Runs enqueued tasks cooperatively, one tick at a time.
/// </summary>
internal class LoomScheduler : TaskScheduler
{
    private readonly ConcurrentQueue<Task> taskQueue = new();
    private readonly object lockObj = new();

    public SynchronizationContext SyncContext { get; }

    public LoomScheduler()
    {
        SyncContext = new LoomSyncContext(this);
    }

    /// <summary>
    /// Execute all queued items for this tick.
    /// Should be called by the owning thread (main loop or worker).
    /// </summary>
    public void RunTick()
    {
        List<Task> toRun;
        lock (lockObj)
        {
            toRun = new(taskQueue);
            taskQueue.Clear();
        }

        foreach (var task in toRun)
        {
            try
            {
                if (task is null)
                    continue;
                TryExecuteTask(task);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[LoomScheduler] Exception during RunTick: {ex}");
            }
        }
    }

    // ---- TaskScheduler core integration ----
    protected override IEnumerable<Task>? GetScheduledTasks()
    {
        lock (lockObj)
            return taskQueue.ToArray();
    }

    protected override void QueueTask(Task task)
    {
        lock (lockObj)
        {
            taskQueue.Enqueue(task);
        }
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        // No multithreaded inlining allowed: only the "owning thread" calls RunTick
        return false;
    }

    internal void Queue(Task task)
    {
        taskQueue.Enqueue(task);
    }
}
