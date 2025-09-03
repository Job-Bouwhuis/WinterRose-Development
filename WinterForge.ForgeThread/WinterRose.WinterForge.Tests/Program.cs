using System.Threading;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeThread;

namespace WinterRose.ForgeThreads.Tests;

internal class Program
{
    static async Task Main()
    {
        Console.Clear();
        ForgeGuard.IncludeColorInMessageFormat = true;
        var guardResult = ForgeGuard.Run(new TextWriterStream(Console.Out));
        Console.WriteLine(guardResult.ToString());
    }

    /*
     # ForgeThread Refactor To-Do
        - [ ] Add **custom TaskScheduler** for ForgeThread threads → keep all async/await on the same thread
        - [ ] Support **immediate vs tick-bound execution** queues → immediate skips tick rate
        - [ ] Allow **actions returning Task** to run cooperatively (concurrent) without crossing threads
        - [ ] Keep **void actions sequential** within their queue + priority order
        - [ ] Integrate **ticks per second** for thread timing → tick-bound tasks run on schedule
        - [ ] Make **coroutines awaitable** so `await` works with ForgeThread coroutines
     */
}
