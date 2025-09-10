using System.Diagnostics;
using System.Threading;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeThread;
using WinterRose.ForgeThread.Tests;

namespace WinterRose.ForgeThreads.Tests;

internal class Program
{
    private static int[] GenerateArray(int size)
    {
        var arr = new int[size];
        for (int i = 0; i < size; i++) arr[i] = i;
        Shuffle(arr);
        return arr;
    }

    private static void Shuffle(int[] arr)
    {
        var rand = new Random();
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }

    static async Task Main()
    {
        PooledThreadLoom pool = new PooledThreadLoom("sorter", Environment.ProcessorCount);
        int[] arr = GenerateArray(400_000_000, pool);
        Stopwatch stopwatch = Stopwatch.StartNew();
        ParallelQuickSort<int>.Sort(arr, pool);
        stopwatch.Stop();
        Console.WriteLine(stopwatch.ElapsedTicks);
        Console.WriteLine(stopwatch.ElapsedMilliseconds);


        return;
        Console.Clear();
        ForgeGuard.IncludeColorInMessageFormat = true;
        var guardResult = ForgeGuard.Run(new TextWriterStream(Console.Out));
        Console.WriteLine(guardResult.ToString());
    }

    private static int[] GenerateArray(int size, PooledThreadLoom loom)
    {
        var arr = new int[size];
        int threadCount = Environment.ProcessorCount;
        int chunkSize = (size + threadCount - 1) / threadCount;

        var done = new CountdownEvent(threadCount);

        // Fill array in parallel chunks
        for (int t = 0; t < threadCount; t++)
        {
            int start = t * chunkSize;
            int end = Math.Min(start + chunkSize, size);
            loom.Schedule(() =>
            {
                for (int i = start; i < end; i++) arr[i] = i;
                done.Signal();
            });
        }

        done.Wait();

        // Parallel shuffle: each thread shuffles its chunk, then we do a final pass to mix chunks
        ParallelShuffle(arr, loom);

        return arr;
    }

    private static void ParallelShuffle(int[] arr, PooledThreadLoom loom)
    {
        int threadCount = Environment.ProcessorCount;
        int chunkSize = (arr.Length + threadCount - 1) / threadCount;
        var done = new CountdownEvent(threadCount);

        // Each thread shuffles its own chunk
        for (int t = 0; t < threadCount; t++)
        {
            int start = t * chunkSize;
            int end = Math.Min(start + chunkSize, arr.Length);
            loom.Schedule(() =>
            {
                var rand = new Random(Guid.NewGuid().GetHashCode());
                for (int i = end - 1; i > start; i--)
                {
                    int j = rand.Next(start, i + 1);
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
                done.Signal();
            });
        }

        done.Wait();
    }


    public static void RunSortingBenchmarks(PooledThreadLoom loom)
    {
        int threadCount = Environment.ProcessorCount;
        int startSize = 25000;
        int maxSize = 1000000;
        int iterations = 10;

        Console.WriteLine($"Using {threadCount} threads for sorting benchmarks");
        Console.WriteLine("ArraySize\tAverageTime(ms)");

        for (int size = startSize; size <= maxSize; size *= 2)
        {
            var times = new List<double>();

            for (int i = 0; i < iterations; i++)
            {
                var array = GenerateRandomArray(size);
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                using (var iterator = new MultiThreadedSortedIterator<int>(loom, array, threadCount))
                {
                    var result = new List<int>();
                    while (iterator.HasNext())
                    {
                        result.Add(iterator.Next());
                    }
                }

                stopwatch.Stop();
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            // remove min and max
            times.Sort();
            if (times.Count > 2)
            {
                times.RemoveAt(0); // lowest
                times.RemoveAt(times.Count - 1); // highest
            }

            double avg = times.Average();
            Console.WriteLine($"{size}\t\t{avg:F2}");
        }
    }

    private static int[] GenerateRandomArray(int size)
    {
        var rnd = new Random();
        var array = new int[size];
        for (int i = 0; i < size; i++)
            array[i] = rnd.Next(0, 1000000);
        return array;
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
