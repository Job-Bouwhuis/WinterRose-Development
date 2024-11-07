using System;
using WinterRose.Plugins;

namespace test;

public class TestStuff : IPlugin
{
    public void OnLoad()
    {
        Console.WriteLine("Hello World!");
    }

    public void OnUnload()
    {
        Console.WriteLine("Goodbye World!");
    }

    public void Run()
    {
        Console.WriteLine("Executing!");
    }
}
