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
        private readonly ConcurrentDictionary<string, LoomThread> threads = new();
        private readonly ConcurrentDictionary<string, LoomScheduler> schedulers = new();
        private readonly ConcurrentDictionary<string, int> tickRates = new();
        private readonly ConcurrentDictionary<string, ManualResetEventSlim> wakeEvents = new();

        private bool disposed;

        public const string DEFAULT_MAIN_NAME = "Main";

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

        /// <summary>
        /// Invoke a function on the named thread and get a Task{T} with the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="threadName">Target thread.</param>
        /// <param name="func">Function to execute.</param>
        /// <param name="priority">Job priority.</param>
        /// <returns>Task with result.</returns>
        public Task<T> InvokeOn<T>(string threadName, Func<T> func, JobPriority priority = JobPriority.Normal)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
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

        // --- RUNON OVERLOADS (new) ---
        // runs synchronous actions in order (returns a Task that completes when action finished)
        public Task InvokeOn(string threadName, Action action, JobPriority priority = JobPriority.Normal, bool bypassTick = false)
        {
            return EnqueueItem(threadName, action, priority, CancellationToken.None, bypassTick);
        }

        // runs async functions (the function runs on the loom thread; its continuations capture the loom scheduler)
        public Task<T> InvokeOn<T>(string threadName, Func<Task<T>> func, JobPriority priority = JobPriority.Normal, bool bypassTick = false)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            void wrapper()
            {
                try
                {
                    var task = func();
                    if (task == null)
                    {
                        tcs.SetResult(default!);
                        return;
                    }

                    if (schedulers.TryGetValue(threadName, out var scheduler))
                    {
                        task.ContinueWith((t, o) =>
                        {
                            scheduler.SyncContext.Post(_ =>
                            {
                                if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                                else if (t.IsCanceled) tcs.SetCanceled();
                                else tcs.SetResult(t.Result);
                            }, null);
                        }, scheduler);
                    }
                    else
                    {
                        task.ContinueWith((t, o) =>
                        {
                            if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                            else if (t.IsCanceled) tcs.SetCanceled();
                            else tcs.SetResult(t.Result);
                        }, scheduler);
                    }
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
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            void wrapper()
            {
                try
                {
                    var task = func();
                    if (task == null)
                    {
                        tcs.SetResult();
                        return;
                    }

                    if (schedulers.TryGetValue(threadName, out var scheduler))
                    {
                        task.ContinueWith((t, o) =>
                        {
                            scheduler.SyncContext.Post(_ =>
                            {
                                if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                                else if (t.IsCanceled) tcs.SetCanceled();
                                else tcs.SetResult();
                            }, null);
                        }, scheduler);
                    }
                    else
                    {
                        task.ContinueWith((t, o) =>
                        {
                            if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                            else if (t.IsCanceled) tcs.SetCanceled();
                            else tcs.SetResult();
                        }, scheduler);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            EnqueueItem(threadName, wrapper, priority, CancellationToken.None, bypassTick);
            return tcs.Task;
        }

        // accepts an already-started Task (e.g. Task.Run(...)). The completion is observed and posted to the loom scheduler.
        public Task<T> InvokeOn<T>(string threadName, Task<T> externalTask, bool bypassTick = false)
        {
            if (externalTask == null) throw new ArgumentNullException(nameof(externalTask));
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            void wrapper()
            {
                if (schedulers.TryGetValue(threadName, out var scheduler))
                {
                    externalTask.ContinueWith((t, o) =>
                    {
                        scheduler.SyncContext.Post(_ =>
                        {
                            if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                            else if (t.IsCanceled) tcs.SetCanceled();
                            else tcs.SetResult(t.Result);
                        }, null);
                    }, scheduler);
                }
                else
                {
                    externalTask.ContinueWith((t, o) =>
                    {
                        if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                        else if (t.IsCanceled) tcs.SetCanceled();
                        else tcs.SetResult(t.Result);
                    }, scheduler);
                }
            }

            EnqueueItem(threadName, wrapper, JobPriority.Normal, CancellationToken.None, bypassTick);
            return tcs.Task;
        }

        // accepts an already-started Task (e.g. Task.Run(...)). The completion is observed and posted to the loom scheduler.
        public Task InvokeOn(string threadName, Task externalTask, bool bypassTick = false)
        {
            if (externalTask == null) throw new ArgumentNullException(nameof(externalTask));
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            void wrapper()
            {
                if (schedulers.TryGetValue(threadName, out var scheduler))
                {
                    externalTask.ContinueWith((t, o) =>
                    {
                        scheduler.SyncContext.Post(_ =>
                        {
                            if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                            else if (t.IsCanceled) tcs.SetCanceled();
                            else tcs.SetResult();
                        }, null);
                    }, scheduler);
                }
                else
                {
                    externalTask.ContinueWith((t, o) =>
                    {
                        if (t.IsFaulted) tcs.SetException(t.Exception ?? new Exception("Task faulted"));
                        else if (t.IsCanceled) tcs.SetCanceled();
                        else tcs.SetResult();
                    }, scheduler);
                }
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
        /// Invoke an asynchronous function on the named thread; the function itself will run on that thread and its returned Task will be awaited there.
        /// </summary>
        /// <param name="threadName">Target thread.</param>
        /// <param name="func">Async function to run.</param>
        /// <param name="priority">Job priority.</param>
        /// <returns>Task representing the completion.</returns>
        public Task InvokeOn(string threadName, Func<Task> func, JobPriority priority = JobPriority.Normal)
        {
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            async void wrapper()
            {
                try
                {
                    await func().ConfigureAwait(false);
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            EnqueueItem(threadName, wrapper, priority, CancellationToken.None);
            return tcs.Task;
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

            // begin tick for the loom
            loom.BeginNextTick();

            // run any scheduler work queued for this loom (continuations)
            if (schedulers.TryGetValue(name, out var mainScheduler))
            {
                mainScheduler.RunTick();
            }

            int executed = 0;
            while (executed < maxItems && loom.TryDequeue(out var item))
            {
                ExecuteScheduledItem(item, mainScheduler);
                // after executing the item, run any continuations that were posted during execution
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

                // update last-yield in the handle for consumers
                if (yielded is TimeSpan)
                {
                    // don't set LastYield for time-waits; keep previous value
                }
                else
                {
                    // store any non-timeSpan (including null) as last-yield
                    handle.UpdateLastYield(yielded);
                }

                if (yielded == null)
                {
                    // resume next tick on same loom
                    EnqueueItem(handle.Name, () => ResumeCoroutine(handle), JobPriority.Normal, CancellationToken.None);
                }
                else if (yielded is TimeSpan ts)
                {
                    // schedule resume after timespan
                    var timer = new Timer(_ => EnqueueItem(handle.Name, () => ResumeCoroutine(handle), JobPriority.Normal, CancellationToken.None), null, ts, Timeout.InfiniteTimeSpan);
                    handle.AttachTimer(timer);
                }
                else
                {
                    // yielded a value (not a timespan) — treat as single-tick resume but keep the yielded value available
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

            // remove scheduler and tick rate entries
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
                    if (!loom.ThreadInstance.Join(waitFor.Value))
                    {
                        // cannot forcefully abort; leave as background
                    }
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
            {
                ShutdownThread(name, waitFor);
            }
        }

        /// <summary>
        /// Get list of currently registered thread names.
        /// </summary>
        public IReadOnlyCollection<string> RegisteredThreadNames => threads.Keys.ToArray();

        private void WorkerLoop(LoomThread loom)
        {
            var schedulerExists = schedulers.TryGetValue(loom.Name, out var scheduler);
            var hasRate = tickRates.TryGetValue(loom.Name, out var ticksPerSecond);
            var hasWake = wakeEvents.TryGetValue(loom.Name, out var wakeEvent);
            double targetMs = hasRate && ticksPerSecond > 0 ? 1000.0 / ticksPerSecond : 0.0;
            var sw = new System.Diagnostics.Stopwatch();

            try
            {
                while (!loom.IsCancellationRequested)
                {
                    sw.Restart();

                    loom.BeginNextTick();

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
                            if (hasWake && wakeEvent != null)
                            {
                                // wait until either timeout elapses or someone sets the wakeEvent
                                wakeEvent.Wait((int)remaining);
                                wakeEvent.Reset();
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
                // swallow to keep thread alive
            }
            finally
            {
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
            loom.EnqueueNextTick(item);

            if (bypassTick && wakeEvents.TryGetValue(threadName, out var evt))
            {
                try { evt.Set(); } catch { }
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
