using System.Numerics;
using WinterRose.ForgeGuardChecks;
using WinterRose.WinterForgeSerializing;

namespace ForgeGuardTests;

internal class Program
{
    static void Main(string[] args)
    {
        ForgeGuard.IncludeColorInMessageFormat = true;
        Stream s = Console.OpenStandardOutput();
        GuardResult result = ForgeGuard.Run(s);

        if(result.HighestSeverity > Severity.Healthy)
            Console.WriteLine("a guard failed, that wasnt marked as fatal");
        else
            Console.WriteLine("nice and healthy");

        Console.WriteLine("\n\n\n");

        Console.WriteLine(result.ToString());

        string resultString = WinterForge.SerializeToString(result, TargetFormat.FormattedHumanReadable);
        Console.WriteLine("\n\n\n");
        Console.WriteLine(resultString);
    }
}

class Person
{
    public string Name { get; set; }
    public int Age { get; set; }

    public bool CanDrink()
    {
        return Age >= 18;
    }
}

[GuardClass("test")]
public class ExampleGuardClass
{
    Person p;
    Vector2 v;
    int a;

    [BeforeEach]
    public void Setup()
    {
        p = new()
        {
            Age = 14,
        };
        a = 15;
    }

    [Guard]
    public void GuardTest4()
    {
        Forge.Expect(v).Not.Null();
    }

    [Guard]
    public void MethodExpectation()
    {
        Forge.Expect(p.CanDrink).WhenCalled().ThatReturnValue.True();
    }

    [Guard, Fatal]
    public void FatalTest()
    {
        throw new Exception("testtt");
        // always succeeds in this demo, just there to show the way to mark a guard as absolute fatal. if it fails,
        // ForgeGuard will close the app *immediately* after this guard has ran. with a platform dependant way to notify the user
        // of this hard crash.
    }

    [Guard]
    public void intTest()
    {
        Forge.Expect(a).EqualTo(15);
    }
}
