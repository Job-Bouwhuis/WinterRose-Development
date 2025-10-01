using Microsoft.CodeAnalysis;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.FileManagement;
using WinterRose.Monogame;
using WinterRose.Reflection;
using WinterRose.WinterForgeSerializing.Workers;
using WinterRose.WIP.TestClasses;
using windows = WinterRose.Windows;


// Get all DLL files in the directory
var assemblyFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");

foreach (var assemblyFile in assemblyFiles)
{
    try
    {
        // Load the assembly from the file
        Assembly.LoadFrom(assemblyFile);
        Console.WriteLine($"Successfully loaded assembly: {assemblyFile}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load assembly {assemblyFile}: {ex.Message}");
    }
}

//windows.OpenConsole(false);

//DelegateSerializeTest();

//Type t = TypeWorker.FindType("Program+<>c__DisplayClass0_0");

//ContainsListOfNums obj = new ContainsListOfNums();
//30.Repeat(i => obj.nums.Add(Klant.Random()));
//windows.OpenConsole();

//ObjectSerializer serializer = new();
//serializer.SerializeToFile(obj, "humanReadable.txt");

//List<Instruction> instructions;
//using (Stream reader = File.OpenRead("humanReadable.txt"))
//using (Stream opcodes = new FileStream("opcodes.txt", FileMode.Create, FileAccess.ReadWrite))
//{
//    new HumanReadableParser().Parse(reader, opcodes);

//    reader.Seek(0, SeekOrigin.Begin);
//    using (Stream formatted = new FileStream("humanFormatted.txt", FileMode.Create, FileAccess.ReadWrite))
//        new HumanReadableIndenter().Process(reader, formatted);
//}

//using (Stream opcodes = new FileStream("opcodes.txt", FileMode.Open, FileAccess.ReadWrite))
//{
//    instructions = InstructionParser.ParseInstructions(opcodes);
//}

//WinterForgeVM executor = new();

//int i = 0;
//System.Diagnostics.Stopwatch execForge = new System.Diagnostics.Stopwatch();
//while (i++ < 1000)
//{
//    execForge.Restart();

//    object result = executor.Execute(instructions);
//    execForge.Stop();
//    Console.WriteLine($"\n\nEverything took:\n" +
//        $"ms: {execForge.Elapsed.TotalMilliseconds} -- " +
//        $"nano: {execForge.Elapsed.TotalNanoseconds} " +
//        $"({execForge.Elapsed.TotalSeconds}s)");
//}


//Console.WriteLine("end");
//Console.ReadLine();
//return;

static void DelegateSerializeTest()
{
    int x = 10;
    string message = "Hello, WinterThorn!";
    Func<string> lambda = () => $"{message} x {x}";

    object closure = lambda.Target;

    if (closure == null)
    {
        Console.WriteLine("No closure object found — lambda probably doesn't capture anything.");
        return;
    }

    Console.WriteLine($"Closure type: {closure.GetType().FullName}");

    foreach (var field in closure.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
    {
        object value = field.GetValue(closure);
        Console.WriteLine($"Field: {field.Name}, Value: {value}");
    }
    ObjectSerializer serializer = new(null);
    string serialized = serializer.SerializeToString(closure);
    Console.WriteLine("\n--- Serialized Closure ---");
    Console.WriteLine(serialized);
}

using var game = new TestApp();
game.Run();
