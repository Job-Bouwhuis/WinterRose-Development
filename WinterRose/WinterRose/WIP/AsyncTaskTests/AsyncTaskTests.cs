using System;
using System.Threading.Tasks;

namespace WinterRose.WIP.AsyncTaskTests
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class AsyncTaskTests
    {

        public static async Task<string> LongRunningTaskTest(IProgress<AsyncTaskProgressResult> progress)
        {
            AsyncTaskProgressResult result = new();
            for (int i = 0; i < 10000; i++)
            {
                if (i % 1000 == 0)
                {
                    result.ProgressPersentage = i * 100 / 10000;
                    result.SomeString = $"Iteration: {i}";
                    progress.Report(result);
                }
            }
            return "Completed";
        }
    }


    public class AsyncTaskProgressResult

    {
        public double ProgressPersentage { get; set; }
        public string SomeString { get; set; } = "";
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member