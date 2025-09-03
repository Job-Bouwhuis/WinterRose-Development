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
}  
