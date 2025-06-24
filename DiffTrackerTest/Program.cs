using System.Runtime.CompilerServices;
using WinterRose.Diff;

namespace DiffTrackerTest;

internal class Program
{
    static void Main(string[] args)
    {
        File.Copy("every-wound-becomes-a-star.wav", "every-wound-becomes-a-star DIFFED.wav", true);

        using var orig = File.Open("every-wound-becomes-a-star.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var mod = File.Open("every-wound-becomes-a-star EDIT.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);

        using var orig2 = File.Open("every-wound-becomes-a-star DIFFED.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        orig2.Position = 0;

        var diffs = DiffTracker.GetSmartByteDiff(orig, mod);
        orig2.Position = 0;
        mod.Position = 0;
        DiffApplier.ApplyDiffs(orig2, diffs);
        orig2.Position = 0;
        mod.Position = 0;
    }

    public static void DumpStreamBytes(Stream stream)
    {
        long startPos = stream.Position;

        int b;
        while ((b = stream.ReadByte()) != -1)
        {
            Console.Write($"{b:X2} ");
        }

        Console.WriteLine();

        stream.Position = startPos; // reset position if needed
    }

}
