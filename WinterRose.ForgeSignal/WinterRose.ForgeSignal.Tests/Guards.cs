using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeThread;

namespace WinterRose.ForgeSignal.Tests;
[GuardClass("ForgeSignal.Basic")]
public class ForgeSignalBasicGuards
{
    [Guard]
    public void FiresAndReceives()
    {
        int got = 0;
        using var _ = Signal.On<int>(i => got = i);
        var count = Signal.Fire(123);
        Forge.Expect(count).EqualTo(1);
        Forge.Expect(got).EqualTo(123);
    }

    [Guard]
    public void OnceOnlyIsRemovedAfterFirstFire()
    {
        int calls = 0;
        var opts = new SubscribeOptions { Once = true };
        using var _ = Signal.On<int>(_ => calls++, opts);
        
        Signal.Fire(1);
        Signal.Fire(2);

        Forge.Expect(calls).EqualTo(1);
    }

    [Guard]
    public void WeakSubscriberIsCollected()
    {
        int calls = 0;

        void AddWeak()
        {
            var target = new Listener(() => calls++);
            var opts = new SubscribeOptions { Weak = true };
            Signal.On<int>(target.Handler, opts);
            // target falls out of scope -> eligible for GC
        }

        AddWeak();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Signal.Fire(1); // weak should be gone
        Forge.Expect(calls).EqualTo(0);
    }

    class Listener
    {
        readonly Action onCall;
        public Listener(Action onCall) { this.onCall = onCall; }
        public void Handler(int _) => onCall();
    }

    [Guard]
    public void ClearAllRemovesStaticListeners()
    {
        int calls = 0;
        var sub = Signal.On<int>(_ => calls++);
        Signal.ClearAllGlobal();
        Signal.Fire(7);
        Forge.Expect(calls).EqualTo(0);
        sub.Dispose(); // no-op safe
    }
}

[GuardClass("ForgeSignal.Async")]
public class ForgeSignalAsyncGuards
{
    [Guard]
    public void AsyncHandlerRunsAndAggregates()
    {
        int calls = 0;
        using var _ = Signal.OnAsync<int>(async i =>
        {
            await Task.Delay(5);
            Interlocked.Add(ref calls, i);
        });

        var t = Signal.FireAsync(3);
        t.Wait();
        Forge.Expect(calls).EqualTo(3);
    }
}

//[GuardClass("ForgeSignal.LoomDispatch")]
//public class ForgeSignalLoomGuards
//{
//    static ThreadLoom loom;

//    [GuardSetup]
//    public static void Setup()
//    {
//        loom = new ThreadLoom();
//        loom.RegisterWorkerThread("WorkerA");
//    }

//    [GuardTeardown]
//    public static void Teardown()
//    {
//        loom.Dispose();
//        Signal.ClearAll();
//    }

//    [Guard]
//    public void DispatchTargetsLoomThread()
//    {
//        string? threadName = null;

//        var opts = new SubscribeOptions
//        {
//            Target = loom.OnLoom("WorkerA")
//        };

//        using var _ = Signal.On<string>(s => threadName = Thread.CurrentThread.Name, opts);

//        Signal.Fire("go");

//        // give WorkerA a moment to pump
//        var sw = System.Diagnostics.Stopwatch.StartNew();
//        while (threadName == null && sw.ElapsedMilliseconds < 2000) Thread.Sleep(5);

//        Forge.Expect(threadName).Not.Null();
//        Forge.Expect(threadName!.StartsWith("WinterRose.ThreadLoom-WorkerA")).True();
//    }
//}
