using WinterRose.ForgeGuardChecks;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.Clear();
        ForgeGuard.IncludeColorInMessageFormat = true;
        var guardResult = ForgeGuard.Run(new TextWriterStream(Console.Out));
        Console.WriteLine(guardResult.ToString());
    }
}