using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WinterRose.Diff;
using WinterRose.ForgeThread;

namespace DiffTrackerTest;

internal class Program
{
    static void Main(string[] args)
    {
        File.Copy("every-wound-becomes-a-star.wav", "every-wound-becomes-a-star DIFFED.wav", true);
        test().GetAwaiter().GetResult();



        ThreadLoom loom = new ThreadLoom();
        loom.RegisterWorkerThread("worker1");
        loom.RegisterMainThread(); // takes current running thread and registers it as main

        int result = loom.ComputeOn("worker1", async () =>
        {
            // at this point were on worker1
            await Task.Delay(1000);
            // after an await, with default async await stuff, youre most often on another thread
            // not with ForgeThread, it resumes on the exact same thread, but all async await
            // functionality works exactly as normal
            return 10;
        });

        // as stated in the ComputeOn above,
        // an async await does not leave the thread its assigned to
        // however what if you want more workhorses for the same tasks
        // and dont care if a task leaves a specific thread or not
        // of course you can do that
        loom.CreatePool("pool", 10);
        int result2 = loom.ComputeOn("pool", async () =>
        {
            // at this point were on any of the workers in the pool
            await Task.Delay(1000);
            // here we may be on another thread, but also may be on the same, honestly not predictable
            return 10;
        });

        loom.InvokeOn("worker1", () =>
        {
            // doesnt return but will run on worker1
        });

        CoroutineHandle coroutine = loom.InvokeOn("pool", MyCoroutine());
        var finished = coroutine.IsComplete;
        var awaitableTask = coroutine.Task;
        var lastYielded = coroutine.LastYield; // a non threadsafe access to whatever it last yielded
        coroutine.Dispose(); // stops the coroutine even if its not finished. see it as a CancellationToken

        while(true)
        {
            loom.TickMainThread(); // since this is the main thread, we have to manually tick it
            // doing this automatically would not have it be the main thread anymore
        }


        // all threads within a loom can have a task scheduled from any thread, and itll be invoked on the thread its scheduled on, or pool
    }

    static IEnumerator MyCoroutine()
    {
        yield return 1;
        yield return TimeSpan.FromSeconds(5); // will continue after 5s
        yield return 2;
        yield return 3;
        yield return 4;
    }

    static async Task test()
    {
        //using var orig = File.Open("every-wound-becomes-a-star.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //using var mod = File.Open("every-wound-becomes-a-star EDIT.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);

        //using var orig2 = File.Open("every-wound-becomes-a-star DIFFED.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);

        var rops = DirectoryDiff.Load("testOps.wfbin");

        ThreadLoom loom = new();
        loom.CreatePool("DiffPool", 10);

        DirectoryDiff ops = loom.ComputeOn("DiffPool", new DiffEngine().DirectoryDiffAsync(
            @"D:\GitRepositories\Personal\WinterRose-Development\DiffTrackerTest\bin\Debug\net10.0 - old",
            @"D:\GitRepositories\Personal\WinterRose-Development\DiffTrackerTest\bin\Debug\net10.0"));
        ops.Save("testOps.wfbin");
    }
}
