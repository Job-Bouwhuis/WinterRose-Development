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
    }

    static async Task test()
    {
        using var orig2 = File.Open("every-wound-becomes-a-star DIFFED.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);


        using var orig = File.Open("every-wound-becomes-a-star.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var mod = File.Open("every-wound-becomes-a-star EDIT.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);

        ThreadLoom loom = new();
        loom.CreatePool("DiffPool", 10);
        FileDiff ops = loom.ComputeOn("DiffPool", new DiffEngine().FileDiffAsync(orig, mod));
        ops.Save("testOps.wfbin");
    }
}
