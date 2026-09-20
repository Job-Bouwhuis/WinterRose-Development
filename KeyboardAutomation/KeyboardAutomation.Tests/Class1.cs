namespace KeyboardAutomation.Tests;

public class Class1
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("wait...");
        await Task.Delay(1000);
        Keyboard.PressKey('[');
        Console.WriteLine("done.");
    }
}