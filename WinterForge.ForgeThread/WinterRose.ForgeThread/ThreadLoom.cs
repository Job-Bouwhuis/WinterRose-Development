using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WinterRose.ForgeThread
{
    /// <summary>
    /// Central manager for named "looms" (threads) that accept scheduled jobs and invocations.
    /// </summary>
    public class ThreadLoom : IDisposable
    {
        private readonly ConcurrentDictionary<string, LoomThread> threads = new();
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
        }

        /// <summary>
        /// Create and start a dedicated worker thread managed by the loom.
        /// </summary>
        /// <param name="name">Logical name of the worker.</param>
        /// <param name="priority">OS thread priority.</param>
        /// <param name="isBackground">Whether the created thread is a background thread.</param>
        public void RegisterWorkerThread(string name, ThreadPriority priority = ThreadPriority.Normal, bool isBackground = true)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name required", nameof(name));
            if (threads.ContainsKey(name)) throw new InvalidOperationException($"Thread '{name}' already registered.");

            var loom = new LoomThread(name, isHostedMain: false);
            if (!threads.TryAdd(name, loom))
            {
                throw new InvalidOperationException($"Failed to register thread '{name}'.");
            }

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
        /// Invoke an action on the named thread and get a Task that completes when the action finished.
        /// </summary>
        /// <param name="name">Target thread name.</param>
        /// <param name="action">Action to execute.</param>
        /// <param name="priority">Job priority.</param>
        /// <returns>A task representing completion.</returns>
        public Task InvokeOn(string name, Action action, JobPriority priority = JobPriority.Normal)
        {
            return EnqueueItem(name, action, priority, CancellationToken.None);
        }

        /// <summary>
        /// Invoke a function on the named thread and get a Task{T} with the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Target thread.</param>
        /// <param name="func">Function to execute.</param>
        /// <param name="priority">Job priority.</param>
        /// <returns>Task with result.</returns>
        public Task<T> InvokeOn<T>(string name, Func<T> func, JobPriority priority = JobPriority.Normal)
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

            EnqueueItem(name, wrapper, priority, CancellationToken.None);
            return tcs.Task;
        }

        /// <summary>
        /// Invoke an asynchronous function on the named thread; the function itself will run on that thread and its returned Task will be awaited there.
        /// </summary>
        /// <param name="name">Target thread.</param>
        /// <param name="func">Async function to run.</param>
        /// <param name="priority">Job priority.</param>
        /// <returns>Task representing the completion.</returns>
        public Task InvokeOnAsync(string name, Func<Task> func, JobPriority priority = JobPriority.Normal)
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

            EnqueueItem(name, wrapper, priority, CancellationToken.None);
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

            loom.BeginNextTick();

            int executed = 0;
            while (executed < maxItems && loom.TryDequeue(out var item))
            {
                ExecuteScheduledItem(item);
                executed++;
            }

            return executed;
        }

        /// <summary>
        /// Start a coroutine (simple cooperative iterator) on a named loom. The coroutine yields null to indicate "resume next tick",
        /// or yields a TimeSpan to indicate a timed wait.
        /// </summary>
        /// <param name="threadName">Target loom.</param>
        /// <param name="routine">Enumerator representing the coroutine.</param>
        /// <returns>Handle that can be used to stop the coroutine.</returns>
        public CoroutineHandle StartCoroutine(string threadName, IEnumerator<object?> routine)
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
                    // unsupported yield types: treat as single-tick resume
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
            try
            {
                while (!loom.IsCancellationRequested)
                {
                    loom.BeginNextTick();
                    if (loom.TryTake(out var item))
                    {
                        //Console.WriteLine("Tick: " + loom.Name);
                        ExecuteScheduledItem(item);
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
                    try { ExecuteScheduledItem(remaining); } catch { }
                }
            }
        }

        private void ExecuteScheduledItem(ScheduledItem item)
        {
            try
            {
                item.Action();
                item.TrySetResult();
            }
            catch (Exception ex)
            {
                item.TrySetException(ex);
            }
        }

        private Task EnqueueItem(string threadName, Action action, JobPriority priority, CancellationToken cancellation)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ThreadLoom));
            if (!threads.TryGetValue(threadName, out var loom)) throw new InvalidOperationException($"Thread '{threadName}' is not registered.");
            if (loom.IsShutdown) throw new InvalidOperationException($"Thread '{threadName}' is shutting down.");

            var item = new ScheduledItem(action, priority);
            loom.EnqueueNextTick(item);

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
