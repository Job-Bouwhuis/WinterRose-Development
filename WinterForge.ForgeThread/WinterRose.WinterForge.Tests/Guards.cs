using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks;

namespace WinterRose.ForgeThread.Tests;

[GuardClass("ThreadLoom.Basic")]
public class ThreadLoomBasicGuards
{
    static ThreadLoom loom;
    static int mainThreadId;

    [GuardSetup]
    public static void Setup()
    {
        loom = new ThreadLoom();
        loom.RegisterMainThread(); // registers current thread as "Main"
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
        loom.RegisterWorkerThread("Worker1");
    }

    [Guard]
    public void WorkerRunsOnNamedThread()
    {
        string? workerThreadName = null;
        var task = loom.InvokeOn("Worker1", () =>
        {
            workerThreadName = Thread.CurrentThread.Name;
        });

        while(true)
        {
            if (task.IsCompleted)
                break;
        }
        
        //task.Wait(200000).TrueOrThrow("Worker job timed out");

        Forge.Expect(workerThreadName).Not.Null();
        Forge.Expect(workerThreadName!.StartsWith("WinterRose.ThreadLoom-Worker1")).True();
    }

    [Guard]
    public void WorkerCanReturnValue()
    {
        var task = loom.InvokeOn<int>("Worker1", () =>
        {
            return 42;
        });

        loom.ProcessPendingActions();

        var result = task.Result;
        Forge.Expect(result).EqualTo(42);
    }

    [Guard]
    public void WorkerPostsBackToMainQueue()
    {
        bool mainCallbackFired = false;

        var task = loom.InvokeOn("Worker1", () =>
        {
            // enqueue a callback to main. main will have to pump to run it.
            loom.InvokeOn("Main", () => mainCallbackFired = true);
        });

        // ensure worker job completed
        task.Wait(2000).TrueOrThrow("Worker job timed out");

        // nothing executed on main yet until we pump
        Forge.Expect(mainCallbackFired).False();

        // pump main to run posted actions
        loom.ProcessPendingActions();

        Forge.Expect(mainCallbackFired).True();
    }

    [GuardTeardown]
    public static void teardown()
    {
        loom.Dispose();
    }

}

[GuardClass("ThreadLoom.Coroutine")]
public class ThreadLoomCoroutineGuards
{
    static ThreadLoom loom;

    [GuardSetup]
    public static void Setup()
    {
        loom = new ThreadLoom();
        loom.RegisterMainThread();
    }

    static IEnumerator<object?> CounterCoroutine(int increments, Action<int> onTick)
    {
        for (int i = 0; i < increments; i++)
        {
            onTick(i + 1);
            yield return null; // resume next tick
        }
    }

    static IEnumerator<object?> TimedCoroutine(Action onComplete)
    {
        yield return TimeSpan.FromMilliseconds(80); // wait ~80ms
        onComplete();
    }

    [Guard]
    public void CoroutineAdvancesOverTicks()
    {
        int counter = 0;
        var handle = loom.InvokeOn("Main", CounterCoroutine(3, v => counter = v));

        // initially 0
        Forge.Expect(counter).EqualTo(0);

        // pump a few times (simulate ticks)
        for (int i = 0; i < 3; i++)
            loom.ProcessPendingActions();

        Forge.Expect(counter).EqualTo(3);

        handle.Dispose();
    }

    [Guard]
    public void CoroutineTimedResume()
    {
        bool completed = false;
        var handle = loom.InvokeOn("Main", TimedCoroutine(() => completed = true));
        
        // not yet complete until timer fires and we pump
        Forge.Expect(completed).False();

        // first iteration work
        loom.ProcessPendingActions();

        Thread.Sleep(120);

        // second iteration work
        loom.ProcessPendingActions();

        Forge.Expect(completed).True();

        handle.Dispose();
    }

    [GuardTeardown]
    public static void teardown()
    {
        loom.Dispose();
    }
}

[GuardClass("ThreadLoom.Scheduler")]
public class ThreadLoomSchedulerGuards
{
    static ThreadLoom loom;

    [GuardSetup]
    public static void Setup()
    {
        loom = new ThreadLoom();
        loom.RegisterMainThread();
    }

    [Guard]
    public void InvokeAfterExecutes()
    {
        bool fired = false;
        using var handle = loom.InvokeAfter("Main", () => fired = true, TimeSpan.FromMilliseconds(60));

        // before sleep/pump it should be false
        Forge.Expect(fired).False();

        Thread.Sleep(90);
        loom.ProcessPendingActions();

        Forge.Expect(fired).True();
    }

    [Guard]
    public void ScheduleRepeatingProducesMultipleJobs()
    {
        int counter = 0;
        using var disp = loom.ScheduleRepeating("Main", () => Interlocked.Increment(ref counter), TimeSpan.FromMilliseconds(30));

        Thread.Sleep(140); // allow several timer ticks to enqueue work
        loom.ProcessPendingActions();

        Forge.Expect(counter).GreaterThanOrEqualTo(3);

        // cleanup
        disp.Dispose();
    }

    [GuardTeardown]
    public static void teardown()
    {
        loom.Dispose();
    }
}

[GuardClass("ThreadLoom.Pool")]
public class ThreadPoolLoomGuards
{
    static ThreadLoom loom;
    static ThreadPoolLoom pool;

    [GuardSetup]
    public static void Setup()
    {
        loom = new ThreadLoom();
        pool = new ThreadPoolLoom(loom, "pool", 3);
    }

    [Guard]
    public void PoolDispatchesToWorkers()
    {
        int completed = 0;
        const int workCount = 10;
        var tasks = new Task[workCount];

        for (int i = 0; i < workCount; i++)
        {
            tasks[i] = pool.Schedule(() =>
            {
                // simulate some work
                Thread.Sleep(10);
                Interlocked.Increment(ref completed);
            });
        }

        Forge.Expect(() => Task.WhenAll(tasks).Wait()).WhenCalled().ToCompleteWithin(5000);
        Forge.Expect(completed).EqualTo(workCount);
    }

    [GuardTeardown]
    public static void teardown()
    {
        pool.Dispose();
        loom.Dispose();
    }
}

[GuardClass("ThreadLoom.Priority")]
public class ThreadLoomPriorityGuards
{
    static ThreadLoom loom;

    [GuardSetup]
    public static void Setup()
    {
        loom = new ThreadLoom();
        loom.RegisterWorkerThread("PriWorker");
    }

    [Guard]
    public void HighPriorityRunsBeforeLowPriority()
    {
        var runOrder = new List<string>();
        loom.Pause("PriWorker");
        var tLow1 = loom.InvokeOn("PriWorker", () => runOrder.Add("low1"), JobPriority.Low);
        var tLow2 = loom.InvokeOn("PriWorker", () => runOrder.Add("low2"), JobPriority.Low);
        var tNormal = loom.InvokeOn("PriWorker", () => runOrder.Add("normal"), JobPriority.Normal);
        var tHigh = loom.InvokeOn("PriWorker", () => runOrder.Add("high"), JobPriority.High);
        loom.Resume("PriWorker");

        // wait for completion
        Task.WaitAll(new[] { tLow1, tLow2, tNormal, tHigh }, 2000).TrueOrThrow("Priority jobs timed out");

        // ensure high is among first executed items
        Forge.Expect(runOrder.First()).EqualTo("high");
    }

    [GuardTeardown]
    public static void teardown()
    {
        loom.Dispose();
    }
}

// small helper extension used by guards to make boolean waits fail fast with clear message
static class GuardHelpers
{
    public static void TrueOrThrow(this bool value, string message)
    {
        if (!value) throw new TimeoutException(message);
    }
}

[GuardClass("ThreadLoom.CoroutineAdvanced")]
public class ThreadLoomCoroutineAdvancedGuards
{
    static ThreadLoom loom;

    [GuardSetup]
    public static void Setup()
    {
        loom = new ThreadLoom();
        loom.RegisterMainThread();
    }

    static IEnumerator<object?> CounterCoroutine(int increments, Action<int> onTick)
    {
        for (int i = 0; i < increments; i++)
        {
            onTick(i + 1);
            yield return null; // resume next tick
        }
    }

    [Guard]
    public void MultipleIndependentCoroutines()
    {
        int counterA = 0;
        int counterB = 0;

        var handle1 = loom.InvokeOn("Main", CounterCoroutine(3, v => counterA = v));
        var handle2 = loom.InvokeOn("Main", CounterCoroutine(5, v => counterB = v));

        // tick 1: process main queue
        loom.ProcessPendingActions();
        Forge.Expect(counterA).EqualTo(1);
        Forge.Expect(counterB).EqualTo(1);

        // tick 2
        Thread.Sleep(50); // timers may fire, but they enqueue work
        loom.ProcessPendingActions();
        Forge.Expect(counterA).EqualTo(2);
        Forge.Expect(counterB).EqualTo(2);

        // tick 3
        Thread.Sleep(50);
        loom.ProcessPendingActions();
        Forge.Expect(counterA).EqualTo(3); // completed
        Forge.Expect(counterB).EqualTo(3);

        // tick 4
        Thread.Sleep(50);
        loom.ProcessPendingActions();

        // tick 5
        Thread.Sleep(50);
        loom.ProcessPendingActions();

        // cleanup
        handle1.Dispose();
        handle2.Dispose();
    }

    [GuardTeardown]
    public static void teardown()
    {
        loom.Dispose();
    }
}


[GuardClass("Timed Coroutines")]
public class TimedCoroutineGuards
{
    static ThreadLoom loom;

    [GuardSetup]
    public static void setup()
    {
        loom = new();
        loom.RegisterWorkerThread("TimedWorker");
    }

    [GuardTeardown]
    public static void teardown()
    {
        loom.Dispose();
    }

    [Guard]
    public void OneTimerCoroutine()
    {
        return;
        bool completed = false;
        var watch = Stopwatch.StartNew();
        CoroutineHandle handle = loom.InvokeOn("TimedWorker", TimedCoroutine(() => completed = true));
        handle.Wait();
        watch.Stop();
    }

    [Guard]
    public void PlentyTimerCoroutines()
    {
        return;
        int completed = 0;
        var watch = Stopwatch.StartNew();

        List<CoroutineHandle> routines =
        [
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
            loom.InvokeOn("TimedWorker", TimedCoroutine(() => Interlocked.Increment(ref completed))),
        ];

        routines.WhenAll();

        watch.Stop();
        Forge.Expect(completed).EqualTo(19);
    }

    [Guard]
    public void PlentyOfCoroutinesOnAPool()
    {
        int completed = 0;
        ThreadPoolLoom pool = new(loom, "pool", 2);
        List<Task> tasks =
        [
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
            pool.Schedule(() => Interlocked.Increment(ref completed)),
        ];

        Task.WhenAll(tasks).Wait();

        Forge.Expect(completed).EqualTo(10);
    }

    static IEnumerator<object?> TimedCoroutine(Action onComplete)
    {
        int delay = 1000;

        Console.WriteLine("Zero");
        yield return TimeSpan.FromMilliseconds(delay);
        Console.WriteLine("first");
        yield return TimeSpan.FromMilliseconds(delay);
        Console.WriteLine("second");
        yield return TimeSpan.FromMilliseconds(delay);
        Console.WriteLine("third");
        yield return TimeSpan.FromMilliseconds(delay);
        Console.WriteLine("fourth");
        onComplete();
    }
}

[GuardClass("ThreadLoom.Async")]
public class ThreadLoomAsyncGuards
{
    static ThreadLoom loom;

    [GuardSetup]
    public static void Setup()
    {
        loom = new ThreadLoom();
        loom.RegisterMainThread();   // pumpable main
        loom.RegisterWorkerThread("WorkerA"); // worker with default tickrate
    }

    [GuardTeardown]
    public static void teardown()
    {
        loom.Dispose();
    }

    static void WaitForTask(Task task, int timeoutMs = 2000)
    {
        var sw = Stopwatch.StartNew();
        while (!task.IsCompleted)
        {
            //if (sw.ElapsedMilliseconds > timeoutMs) throw new TimeoutException("Task timed out");
            Thread.Sleep(5);
        }
    }

    [Guard]
    public void AsyncContinuationRunsOnWorkerThread()
    {
        string? continuationThreadName = null;

        var t = loom.InvokeOn("WorkerA", async () =>
        {
            continuationThreadName = Thread.CurrentThread.Name;
            await Task.Delay(20).ConfigureAwait(false);
            continuationThreadName = Thread.CurrentThread.Name;
        }, bypassTick: true);

        // wait for completion (worker wakes because bypassTick was used)
        WaitForTask(t);

        Forge.Expect(continuationThreadName).Not.Null();
        Forge.Expect(continuationThreadName!.StartsWith("WinterRose.ThreadLoom-WorkerA")).True();
    }

    [Guard]
    public void AsyncFunctionReturnsTypedValue()
    {
        var t = loom.InvokeOn<int>("WorkerA", async () =>
        {
            await Task.Delay(10).ConfigureAwait(false);
            return 1234;
        }, bypassTick: true);

        WaitForTask(t);
        Forge.Expect(t.Result).EqualTo(1234);
    }

    [Guard]
    public void ExternalTaskCompletesAndPostedToLoom()
    {
        // an externally started task (e.g. ThreadPool)
        var external = Task.Run(() =>
        {
            Thread.Sleep(30);
            return 7;
        });

        var t = loom.InvokeOn<int>("WorkerA", external, bypassTick: true);

        WaitForTask(t);
        Forge.Expect(t.Result).EqualTo(7);
    }

    [Guard]
    public void WorkerPostsBackToMain_AfterAwaitContinuation()
    {
        bool mainCallbackFired = false;

        var t = loom.InvokeOn("WorkerA", async () =>
        {
            // cause an await
            await Task.Delay(10).ConfigureAwait(false);
            // schedule back to main thread
            loom.InvokeOn("Main", () => mainCallbackFired = true);
        }, bypassTick: true);

        WaitForTask(t);

        // not yet pumped on main
        Forge.Expect(mainCallbackFired).False();

        // pump main to run posted action
        loom.ProcessPendingActions();

        Forge.Expect(mainCallbackFired).True();
    }

    [Guard]
    public void Coroutine_YieldsValues_And_TaskReturnsLastYield()
    {
        // coroutine that yields integer values
        static IEnumerator<object?> Sequence()
        {
            yield return 1;
            yield return 2;
            // finish
        }

        var handle = loom.InvokeOn("Main", Sequence());

        // nothing yet until pumped
        Forge.Expect(handle.LastYield).Null();

        // tick 1
        loom.ProcessPendingActions();
        Forge.Expect(handle.LastYield).EqualTo(1);

        // tick 2
        loom.ProcessPendingActions();
        // coroutine should complete on tick 2 (last yield = 2)
        // wait for its task to complete by pumping main until done
        var sw = Stopwatch.StartNew();
        while (!handle.Task.IsCompleted)
        {
            if (sw.ElapsedMilliseconds > 1000) throw new TimeoutException("Coroutine timed out");
            loom.ProcessPendingActions();
            Thread.Sleep(1);
        }

        Forge.Expect(handle.Task.Result).EqualTo((object?)2);
    }
}