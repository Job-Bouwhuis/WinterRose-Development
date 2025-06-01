using WinterRose.ForgeGuardChecks;

namespace ForgeGuardTests;

internal class Program
{
    static void Main(string[] args)
    {
        ForgeGuard.IncludeColorInMessageFormat = true;
        ForgeGuard.IndexGuards();
        Stream s = Console.OpenStandardOutput();
        ForgeGuard.RunGuards(s);
    }
}

[GuardClass]
public class ExampleGuardClass
{
    // Global setup method — runs before any test classes
    [GlobalSetup]
    public static void InitEverything()
    {
    }

    // Global teardown — cleans up after all tests are done
    [GlobalTeardown]
    public static void CleanUpEverything()
    {
    }

    // Class-level setup — runs before any guard in this class
    [GuardSetup]
    public static void SetupGuardClass()
    {
    }

    // Class-level teardown — runs after all guards in this class
    [GuardTeardown]
    public static void TeardownGuardClass()
    {
    }

    // Instance-level setup — runs before each individual guard method
    [BeforeEach]
    public void SetupBeforeEach()
    {
    }

    // Instance-level teardown — runs after each individual guard method
    [AfterEach]
    public void CleanupAfterEach()
    {
    }

    [Guard(Severity.Info)]
    public void SampleGuardTest3()
    {
        throw new Exception("this is a test 1");
    }

    [Guard(Severity.Minor)]
    public void SampleGuardTest4()
    {
        throw new Exception("this is a test 2");
    }

    // A test method (a "guard" in ForgeGuard terms)
    [Guard(Severity.Major)]
    public void SampleGuardTest()
    {
        throw new Exception("this is a test 3");
    }

    [Guard(Severity.Catastrophic)]
    public void SampleGuardTest2()
    {
        throw new Exception("this is a test 4");
    }
}
