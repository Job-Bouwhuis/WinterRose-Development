using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.Threading;

namespace WinterRose.ForgeThread
{
    /// <summary>
    /// Central manager for named "looms" (threads) that accept scheduled jobs and invocations.
    /// </summary>
    public class ThreadLoom : IDisposable
    {
        private readonly ConcurrentDictionary<string, SharedWorkQueue> sharedQueues = new();
        private readonly ConcurrentDictionary<string, string> threadToSharedGroup = new();

        private readonly ConcurrentDictionary<string, LoomThread> threads = new();
        private readonly ConcurrentDictionary<string, LoomScheduler> schedulers = new();
        private readonly ConcurrentDictionary<string, int> tickRates = new();
        private readonly ConcurrentDictionary<string, ManualResetEventSlim> wakeEvents = new();

        private class SharedWorkQueue
        {
            public readonly ConcurrentQueue<ScheduledItem> Queue = new();
            public readonly ManualResetEventSlim Wake = new(false);
            public readonly ConcurrentDictionary<string, byte> Members = new();
        }

        private bool disposed;

        public const string DEFAULT_MAIN_NAME = "Main";

        /// <summary>
        /// Create a named shared work queue. Worker threads have to be seperately registered to this group
        /// </summary>
        public void CreateQueueGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName)) throw new ArgumentException("groupName required", nameof(groupName));
            if (!sharedQueues.TryAdd(groupName, new SharedWorkQueue()))
            {
                throw new InvalidOperationException($"Shared queue '{groupName}' already exists.");
            }
        }

        /// <summary>
        /// Make an already-registered thread join a shared queue. After this, enqueues targeted at the thread
        /// will go into the shared queue and any member thread may execute them.
        /// </summary>
        public void JoinSharedQueue(string threadName, string groupName)
        {
            if (string.IsNullOrWhiteSpace(threadName)) throw new ArgumentException("threadName required", nameof(threadName));
            if (!threads.ContainsKey(threadName)) throw new InvalidOperationException($"Thread '{threadName}' is not registered.");

            // create the group if it does not exist
            sharedQueues.GetOrAdd(groupName, _ => new SharedWorkQueue());

            if (!threadToSharedGroup.TryAdd(threadName, groupName))
            {
                throw new InvalidOperationException($"Thread '{threadName}' is already a member of a shared queue.");
            }

            sharedQueues[groupName].Members.TryAdd(threadName, 0);
        }

        /// <summary>
        /// Remove a thread from its shared queue (if any).
        /// </summary>
        public void LeaveSharedQueue(string threadName)
        {
            if (string.IsNullOrWhiteSpace(threadName)) throw new ArgumentException("threadName required", nameof(threadName));
            if (threadToSharedGroup.TryRemove(threadName, out var groupName))
            {
                if (sharedQueues.TryGetValue(groupName, out var q))
                {
                    q.Members.TryRemove(threadName, out _);
                    // optional: automatically destroy empty group
                    if (q.Members.IsEmpty)
                    {
                        // try to remove and dispose wake event
                        if (sharedQueues.TryRemove(groupName, out var removed))
                        {
                            try { removed.Wake.Set(); removed.Wake.Dispose(); } catch { }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Destroy a shared queue, returning any queued items to thread-local queues (best-effort).
        /// </summary>
        public void DestroySharedQueue(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName)) throw new ArgumentException("groupName required", nameof(groupName));
            if (!sharedQueues.TryRemove(groupName, out var q)) return;

            // best-effort: try to push remaining items to members' local queues
            var members = q.Members.Keys.ToArray();
            ScheduledItem item;
            int idx = 0;
            while (q.Queue.TryDequeue(out item))
            {
                if (members.Length == 0) break;
                // round-robin dispatch back into member looms
                var target = members[idx % members.Length];
                if (threads.TryGetValue(target, out var loom))
                {
                    try { loom.EnqueueNextTick(item); } catch { }
                }
                idx++;
            }

            try { q.Wake.Set(); q.Wake.Dispose(); } catch { }
        }

        /// <summary>
        /// Registers the current thread as a pumped "main" loom. You must call <see cref="ProcessPendingActions(string,int)"/>
        /// from the same thread to execute queued actions.
        /// </summary>
        /// <param name="name">Logical name of the thread.</param>
        public void RegisterMainThread(string name = DEFAULT_MAIN_NAME)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name required", nameof(name));
            if (threads.ContainsKey(name)) throw new InvalidOperationException($"Thread '{name}' already registered.");

            var loom = new LoomThread(name, isHostedMain: true);
            if (!threads.TryAdd(name, loom)) throw new InvalidOperationException($"Failed to register main thread '{name}'.");

            var scheduler = new LoomScheduler();
            if (!schedulers.TryAdd(name, scheduler))
            {
                threads.TryRemove(name, out _);
                throw new InvalidOperationException($"Failed to register scheduler for main thread '{name}'.");
            }

            wakeEvents.TryAdd(name, new ManualResetEventSlim(false));
        }

        /// <summary>
        /// Create and start a dedicated worker thread managed by the loom.
        /// </summary>
        /// <param name="name">Logical name of the worker.</param>
        /// <param name="priority">OS thread priority.</param>
        /// <param name="isBackground">Whether the created thread is a background thread.</param>
        public void RegisterWorkerThread(string name, ThreadPriority priority = ThreadPriority.Normal, bool isBackground = true, int ticksPerSecond = 60)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name required", nameof(name));
            if (threads.ContainsKey(name)) throw new InvalidOperationException($"Thread '{name}' already registered.");

            var loom = new LoomThread(name, isHostedMain: false);
            if (!threads.TryAdd(name, loom))
            {
                throw new InvalidOperationException($"Failed to register thread '{name}'.");
            }

            var scheduler = new LoomScheduler();
            if (!schedulers.TryAdd(name, scheduler))
            {
                threads.TryRemove(name, out _);
                throw new InvalidOperationException($"Failed to register scheduler for thread '{name}'.");
            }

            tickRates[name] = Math.Max(0, ticksPerSecond);
            wakeEvents.TryAdd(name, new ManualResetEventSlim(false));

            Thread thread = new Thread(() => WorkerLoop(loom))
            {
                IsBackground = isBackground,
                Name = $"WinterRose.ThreadLoom-{name}",
                Priority = priority
            };

            loom.AttachThread(thread);
            thread.Start();
        }

        private static TaskCompletionSource<T> CreateTcs<T>() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private void ObserveTaskCompletion<TResult>(Task<TResult> sourceTask, string threadName, TaskCompletionSource<TResult> tcs)
        {
            if (sourceTask == null)
            {
                tcs.SetResult(default!);
                return;
            }

            if (schedulers.TryGetValue(threadName, out var scheduler))
            {
                sourceTask.ContinueWith(t =>
                {
                    scheduler.SyncContext.Post(_ =>
                    {
                        if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                        else if (t.IsCanceled) tcs.SetCanceled();
                        else tcs.SetResult(t.Result);
                    }, null);
                }, TaskScheduler.Default);
            }
            else
            {
                sourceTask.ContinueWith(t =>
                {
                    if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                    else if (t.IsCanceled) tcs.SetCanceled();
                    else tcs.SetResult(t.Result);
                }, TaskScheduler.Default);
            }
        }

        private void ObserveTaskCompletion(Task sourceTask, string threadName, TaskCompletionSource<object?> tcs)
        {
            if (sourceTask == null)
            {
                tcs.SetResult(null);
                return;
            }

            if (schedulers.TryGetValue(threadName, out var scheduler))
            {
                sourceTask.ContinueWith(t =>
                {
                    scheduler.SyncContext.Post(_ =>
                    {
                        if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                        else if (t.IsCanceled) tcs.SetCanceled();
                        else tcs.SetResult(null);
                    }, null);
                }, TaskScheduler.Default);
            }
            else
            {
                sourceTask.ContinueWith(t =>
                {
                    if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                    else if (t.IsCanceled) tcs.SetCanceled();
                    else tcs.SetResult(null);
                }, TaskScheduler.Default);
            }
        }


        public Task<T> InvokeOn<T>(string threadName, Func<T> func, JobPriority priority = JobPriority.Normal)
        {
            var tcs = CreateTcs<T>();

            void wrapper()
            {
                try
                {
                    T res = func();
                    tcs.SetResult(res);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            EnqueueItem(threadName, wrapper, priority, CancellationToken.None);
            return tcs.Task;
        }

        public Task InvokeOn(string threadName, Action func, JobPriority priority = JobPriority.Normal)
        {
            var tcs = CreateTcs<object>();

            void wrapper()
            {
                try
                {
                    func();
                    tcs.SetResult(null!);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            EnqueueItem(threadName, wrapper, priority, CancellationToken.None);
            return tcs.Task;
        }

        public Task<T> InvokeOn<T>(string threadName, Func<Task<T>> func, JobPriority priority = JobPriority.Normal, bool bypassTick = false)
        {
            var tcs = CreateTcs<T>();

            void wrapper()
            {
                try
                {
                    var task = func();
                    ObserveTaskCompletion(task, threadName, tcs);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            EnqueueItem(threadName, wrapper, priority, CancellationToken.None, bypassTick);
            return tcs.Task;
        }

        public Task InvokeOn(string threadName, Func<Task> func, JobPriority priority = JobPriority.Normal, bool bypassTick = false)
        {
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            void wrapper()
            {
                try
                {
                    if (!schedulers.TryGetValue(threadName, out var scheduler))
                    {
                        tcs.SetException(new InvalidOperationException($"Scheduler for thread '{threadName}' not found"));
                        return;
                    }

                    var task = Task.Factory.StartNew(
                        async () => await func(),
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        scheduler
                    ).Unwrap();

                    ObserveTaskCompletion(task, threadName, tcs);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            EnqueueItem(threadName, wrapper, priority, CancellationToken.None, bypassTick);
            return tcs.Task;
        }

        public Task InvokeOn(string threadName, Func<Task> func, JobPriority priority = JobPriority.Normal)
        {
            return InvokeOn(threadName, func, priority, bypassTick: false);
        }

        public Task<T> InvokeOn<T>(string threadName, Task<T> externalTask, bool bypassTick = false)
        {
            if (externalTask == null) throw new ArgumentNullException(nameof(externalTask));
            var tcs = CreateTcs<T>();

            void wrapper()
            {
                ObserveTaskCompletion(externalTask, threadName, tcs);
            }

            EnqueueItem(threadName, wrapper, JobPriority.Normal, CancellationToken.None, bypassTick);
            return tcs.Task;
        }

        public Task InvokeOn(string threadName, Task externalTask, bool bypassTick = false)
        {
            if (externalTask == null) throw new ArgumentNullException(nameof(externalTask));
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            void wrapper()
            {
                ObserveTaskCompletion(externalTask, threadName, tcs);
            }

            EnqueueItem(threadName, wrapper, JobPriority.Normal, CancellationToken.None, bypassTick);
            return tcs.Task;
        }
        /// <summary>
        /// Start a coroutine (simple cooperative iterator) on a named loom. The coroutine yields null to indicate "resume next tick",
        /// or yields a TimeSpan to indicate a timed wait.
        /// </summary>
        /// <param name="threadName">Target loom.</param>
        /// <param name="routine">Enumerator representing the coroutine.</param>
        /// <returns>Handle that can be used to stop the coroutine.</returns>
        public CoroutineHandle InvokeOn(string threadName, IEnumerator<object?> routine, bool async = true, JobPriority priority = JobPriority.Normal, bool bypassTick = false)
        {
            if (routine == null) throw new ArgumentNullException(nameof(routine));
            var handle = StartCoroutine(threadName, routine);
            return handle;
        }

        /// <summary>
        /// Schedule an action to run on a specific loom after a delay.
        /// </summary>
        /// <param name="name">Target loom.</param>
        /// <param name="action">Action to execute.</param>
        /// <param name="delay">Delay before execution.</param>
        /// <returns>IDisposable handle that can be disposed to cancel the scheduled action before it runs.</returns>
        public IDisposable InvokeAfter(string name, Action action, TimeSpan delay)
        {
            if (!threads.ContainsKey(name)) throw new InvalidOperationException($"Thread '{name}' is not registered.");
            var timer = new Timer(_ =>
            {
                try
                {
                    EnqueueItem(name, action, JobPriority.Normal, CancellationToken.None);
                }
                catch
                {
                    // ignore
                }
            }, null, delay, Timeout.InfiniteTimeSpan);

            return timer;
        }

        /// <summary>
        /// Schedule a repeating action on a loom. Returns IDisposable to cancel the repetition.
        /// </summary>
        /// <param name="name">Target loom.</param>
        /// <param name="action">Action to execute each tick.</param>
        /// <param name="period">Time between executions.</param>
        /// <returns>Disposable to stop repeating.</returns>
        public IDisposable ScheduleRepeating(string name, Action action, TimeSpan period)
        {
            if (!threads.ContainsKey(name)) throw new InvalidOperationException($"Thread '{name}' is not registered.");
            var timer = new Timer(_ =>
            {
                try
                {
                    EnqueueItem(name, action, JobPriority.Normal, CancellationToken.None);
                }
                catch
                {
                    // ignore
                }
            }, null, period, period);

            return timer;
        }

        /// <summary>
        /// Process pending actions on a registered main/pumped thread. Call this from the thread that was registered.
        /// Executes at most maxItems actions (default all).
        /// </summary>
        /// <param name="name">Name of the registered main thread.</param>
        /// <param name="maxItems">Maximum number of items to process this call.</param>
        /// <returns>Number of executed actions.</returns>
        public int ProcessPendingActions(string name = DEFAULT_MAIN_NAME, int maxItems = int.MaxValue)
        {
            if (!threads.TryGetValue(name, out var loom)) throw new InvalidOperationException($"Thread '{name}' is not registered.");
            if (!loom.IsHostedMain) throw new InvalidOperationException($"Thread '{name}' is not a hosted main thread.");

            loom.BeginNextTick();

            if (schedulers.TryGetValue(name, out var mainScheduler))
            {
                mainScheduler.RunTick();
            }

            if (threadToSharedGroup.TryGetValue(name, out var groupName) && sharedQueues.TryGetValue(groupName, out var shared))
            {
                int taken = 0;
                while (taken < maxItems && shared.Queue.TryDequeue(out var sharedItem))
                {
                    ExecuteScheduledItem(sharedItem, mainScheduler);
                    mainScheduler?.RunTick();
                    taken++;
                }

                if (taken >= maxItems) return taken;
            }

            int executed = 0;
            while (executed < maxItems && loom.TryDequeue(out var item))
            {
                ExecuteScheduledItem(item, mainScheduler);
                mainScheduler?.RunTick();
                executed++;
            }

            return executed;
        }

        private CoroutineHandle StartCoroutine(string threadName, IEnumerator<object?> routine)
        {
            if (!threads.TryGetValue(threadName, out var loom)) throw new InvalidOperationException($"Thread '{threadName}' is not registered.");
            var handle = new CoroutineHandle(threadName, routine, this);
            void start()
            {
                ResumeCoroutine(handle);
            }

            EnqueueItem(threadName, start, JobPriority.Normal, CancellationToken.None);
            return handle;
        }

        internal void ResumeCoroutine(CoroutineHandle handle)
        {
            if (handle.IsStopped) return;

            try
            {
                bool moved = handle.Routine.MoveNext();
                if (!moved)
                {
                    handle.MarkCompleted();
                    return;
                }

                var yielded = handle.Routine.Current;

                if (yielded is TimeSpan)
                {
                    // dont set LastYield for time waits. theyre not a genuine value
                }
                else
                {
                    handle.UpdateLastYield(yielded);
                }

                if (yielded == null)
                {
                    EnqueueItem(handle.Name, () => ResumeCoroutine(handle), JobPriority.Normal, CancellationToken.None);
                }
                else if (yielded is TimeSpan ts)
                {
                    var timer = new Timer(_ => EnqueueItem(handle.Name, () => ResumeCoroutine(handle), JobPriority.Normal, CancellationToken.None), null, ts, Timeout.InfiniteTimeSpan);
                    handle.AttachTimer(timer);
                }
                else
                {
                    EnqueueItem(handle.Name, () => ResumeCoroutine(handle), JobPriority.Normal, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                handle.MarkFaulted(ex);
            }
        }

        /// <summary>
        /// Shutdown a registered thread. For worker threads this will stop the loop and join the thread.
        /// For hosted main threads this will mark it as shutdown so further posts are rejected.
        /// </summary>
        /// <param name="name">Name of the thread to shut down.</param>
        /// <param name="waitFor">Optional timeout to wait for the worker thread to exit.</param>
        public void ShutdownThread(string name, TimeSpan? waitFor = null)
        {
            if (!threads.TryRemove(name, out var loom)) return;

            LeaveSharedQueue(name);

            schedulers.TryRemove(name, out _);
            tickRates.TryRemove(name, out _);

            if (wakeEvents.TryRemove(name, out var evt))
            {
                try { evt.Set(); evt.Dispose(); } catch { }
            }

            loom.RequestShutdown();

            if (loom.ThreadInstance != null)
            {
                if (waitFor.HasValue)
                {
                    loom.ThreadInstance.Join(waitFor.Value);
                }
                else
                {
                    loom.ThreadInstance.Join();
                }
            }
        }

        /// <summary>
        /// Shutdown all managed threads.
        /// </summary>
        /// <param name="waitFor">Optional join timeout for each thread.</param>
        public void ShutdownAll(TimeSpan? waitFor = null)
        {
            var keys = new List<string>(threads.Keys);
            foreach (var name in keys)
                ShutdownThread(name, waitFor);
        }

        /// <summary>
        /// Get list of currently registered thread names.
        /// </summary>
        public IReadOnlyCollection<string> RegisteredThreadNames => threads.Keys.ToArray();

        private void WorkerLoop(LoomThread loom)
        {
            var schedulerExists = schedulers.TryGetValue(loom.Name, out var scheduler);
            var hasRate = tickRates.TryGetValue(loom.Name, out var ticksPerSecond);
            double targetMs = hasRate && ticksPerSecond > 0 ? 1000.0 / ticksPerSecond : 0.0;
            var sw = new System.Diagnostics.Stopwatch();

            try
            {
                while (!loom.IsCancellationRequested)
                {
                    sw.Restart();

                    loom.BeginNextTick();

                    SharedWorkQueue? sharedQueue = null;
                    var inGroup = threadToSharedGroup.TryGetValue(loom.Name, out var groupName)
                                  && sharedQueues.TryGetValue(groupName, out sharedQueue);

                    if (inGroup)
                    {
                        while (sharedQueue!.Queue.TryDequeue(out var sharedItem))
                        {
                            ExecuteScheduledItem(sharedItem, schedulerExists ? scheduler : null);
                        }
                    }

                    while (loom.TryTake(out var item))
                    {
                        ExecuteScheduledItem(item, schedulerExists ? scheduler : null);
                    }

                    if (schedulerExists) scheduler.RunTick();

                    if (targetMs > 0)
                    {
                        var elapsed = sw.Elapsed.TotalMilliseconds;
                        var remaining = targetMs - elapsed;
                        if (remaining > 0)
                        {
                            if (inGroup)
                            {
                                try
                                {
                                    sharedQueue!.Wake.Wait((int)remaining);
                                    sharedQueue.Wake.Reset();
                                }
                                catch { }
                            }
                            else if (wakeEvents.TryGetValue(loom.Name, out var wakeEvent) && wakeEvent != null)
                            {
                                try
                                {
                                    wakeEvent.Wait((int)remaining);
                                    wakeEvent.Reset();
                                }
                                catch { }
                            }
                            else
                            {
                                Thread.Sleep((int)remaining);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // expected on cancellation
            }
            catch (Exception)
            {
                // TODO: make genuine workerthread exception handling
            }
            finally
            {
                if (threadToSharedGroup.TryGetValue(loom.Name, out var finalGroup) &&
                    sharedQueues.TryGetValue(finalGroup, out var finalShared))
                {
                    while (finalShared.Queue.TryDequeue(out var remainingShared))
                    {
                        try { ExecuteScheduledItem(remainingShared, schedulerExists ? scheduler : null); } catch { }
                    }
                }

                while (loom.TryDequeue(out var remaining))
                {
                    try { ExecuteScheduledItem(remaining, schedulerExists ? scheduler : null); } catch { }
                }

                if (schedulerExists) scheduler.RunTick();
            }
        }

        private void ExecuteScheduledItem(ScheduledItem item, LoomScheduler? scheduler)
        {
            var previousContext = SynchronizationContext.Current;
            try
            {
                if (scheduler != null)
                {
                    SynchronizationContext.SetSynchronizationContext(scheduler.SyncContext);
                }

                item.Action();
                item.TrySetResult();
            }
            catch (Exception ex)
            {
                item.TrySetException(ex);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        private Task EnqueueItem(string threadName, Action action, JobPriority priority, CancellationToken cancellation, bool bypassTick = false)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ThreadLoom));
            if (!threads.TryGetValue(threadName, out var loom)) throw new InvalidOperationException($"Thread '{threadName}' is not registered.");
            if (loom.IsShutdown) throw new InvalidOperationException($"Thread '{threadName}' is shutting down.");

            var item = new ScheduledItem(action, priority);

            if (threadToSharedGroup.TryGetValue(threadName, out var groupName) &&
                sharedQueues.TryGetValue(groupName, out var shared))
            {
                shared.Queue.Enqueue(item);
                try 
                { 
                    shared.Wake.Set(); 
                }
                catch { }
            }
            else
            {
                loom.EnqueueNextTick(item);

                if (bypassTick && wakeEvents.TryGetValue(threadName, out var evt))
                {
                    try { evt.Set(); } catch { }
                }
            }

            return item.Task;
        }


        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            ShutdownAll();
        }

        public void Pause(string threadName)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ThreadLoom));
            if (!threads.TryGetValue(threadName, out var loom)) throw new InvalidOperationException($"Thread '{threadName}' is not registered.");
            if (loom.IsShutdown) throw new InvalidOperationException($"Thread '{threadName}' is shutting down.");

            loom.Paused = true;
        }
        public void Resume(string threadName)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ThreadLoom));
            if (!threads.TryGetValue(threadName, out var loom)) throw new InvalidOperationException($"Thread '{threadName}' is not registered.");
            if (loom.IsShutdown) throw new InvalidOperationException($"Thread '{threadName}' is shutting down.");

            loom.Paused = false;
        }
    }
}
