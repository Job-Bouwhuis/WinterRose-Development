using WinterRose.Vectors;
using WinterRose.FileManagement;
using System.Diagnostics;
using WinterRose;
using WinterRose.Encryption;
using WinterRose.WIP.ReflectionTests;
using System.Reflection;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.Factory;
using SnowLibraryTesting;
using WinterRose.Music;
using WinterRose.WinterForgeSerializing;
using WinterRose.Reflection;
using WinterRose.AnonymousTypes;

#pragma warning disable aaa
new Programm().Start();
#pragma warning restore aaa

class typeincdludetests
{
    public List<ListPropertySerializeTests> s = [];
    public typeincdludetests()
    {
        10.Repeat(i => s.Add(new()));
    }
}

internal unsafe class TestClass
{
    private Vector2 pos;

    public Vector2* Position
    {
        get
        {
            fixed (Vector2* e = &pos)
                return e;
        }
    }

    public TestClass()
    {
        pos = new Vector2(1, 1);
    }
}

internal class Programm
{
    public void Start()
    {
        object v = new Person();
        ReflectionHelper rh = new(ref v);
        var x = rh.GetMember("a");
        x.SetValue(ref v, 1);
        object age = x.GetValue(ref v);
        return;
        WinterForgeSerializationTests();
    }

    private unsafe void EncryptionTests()
    {
        string m = """"
                       Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
                       Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in 
                       reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in 
                       culpa qui officia deserunt mollit anim id est laborum.
                       """";

        //string m = "this is a very secret message lmao";
        string p = "diegowasafuncartoon";
        string k = "Kryptonie";
        string decryptkey = "Kryptonye";

        string encrypted = Encryptor.Encrypt(m, k, p, 49, Console.WriteLine);
        Console.WriteLine("\n----\n");
        string decrypted = Encryptor.Decrypt(encrypted, k, p, 49, Console.WriteLine);

        FileManager.Write("encrypted.txt", encrypted, true);
    }

    private unsafe void SerializationTests()
    {
        while (true)
        {
            //SerializingTest();

            6.Repeat(i => GC.Collect());

            Console.WriteLine($"\n\nMemory in use after collecting...\n" +
                $"{GC.GetTotalMemory(true)}");

            Console.ReadKey();
            Console.Clear();
        }
    }

    private void WinterForgeSerializationTests()
    {
        while (true)
        {
            WinterForgeSerializingTest(i => Person.Random());

            6.Repeat(i => GC.Collect());

            Console.WriteLine($"\n\nMemory in use after collecting...\n" +
                              $"{GC.GetTotalMemory(true)}");

            Console.ReadKey();
            Console.Clear();
        }
    }

    private static unsafe void WinterThornTests()
    {
        WinterThorn scriptt = new("Generated", "some generated script", "WinterRose", new(0, 0, 1));

        Block block = ThornFactory.Block();
        block.ParseStatement("x;");
        block.ParseStatement("x = 5;");
        block.ParseStatement("x = 5 + 5;");
        block.ParseStatement("return x;");

        Function funcc = ThornFactory.Function("test", block);

        Class cc = ThornFactory.Class("test", [funcc]);
        Namespace nss = ThornFactory.Namespace("testt", [cc]);
        scriptt.DefineNamespace(nss);

        while (true)
        {
            WinterThorn script = new("TestScript", "An amazing test script", "Me", new(0, 0, 0), []);

            string source = FileManager.Read("TestThorn.thn");
            Stopwatch compileTime = Stopwatch.StartNew();
            Namespace ns = ThornFactory.ParseNamespace(source, script);
            compileTime.Stop();
            Console.WriteLine($"Compiled in {compileTime.ElapsedTicks} ticks ({compileTime.ElapsedMilliseconds}ms)");
            script.DefineNamespace(ns);

            Function func = script.Namespaces[1].Classes[0].Block.Functions[0];
            Stopwatch execTime = Stopwatch.StartNew();
            Variable var = func.Invoke();
            execTime.Stop();
            Console.WriteLine($"Executed in {execTime.ElapsedTicks} ticks ({execTime.ElapsedMilliseconds}ms)");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private static unsafe void AnonymousSerializaitonTest()
    {
        object obj = new
        {
            test = 5,
            test2 = "test",
            test3 = 2.5f,
            test4 = new Vector3(1, 2, 3),
            test5 = new
            {
                john = "wick",
                age = 25
            }
        };

        AnonymousObjectReader reader = new();
        reader.Read(obj);
        string serialized = reader.Serialize();
        AnonymousObjectReader other = new();
        object deser = other.Deserialize(serialized);
    }

    private unsafe void Megalovaina()
    {
        Note[] song =
        [
            Note.Octave4.D(180),
                Note.Rest(0),
                Note.Octave4.D(180),
                Note.Rest(12),
                Note.Octave5.D(180),
                Note.Octave4.A(180),
                Note.Rest(180),
                Note.Octave4.GSharp(180),
                Note.Rest(30),
                Note.Octave4.G(180),
                Note.Rest(30),
                Note.Octave4.F(180),
                Note.Rest(30),
                Note.Octave4.D(180),
                Note.Rest(1),
                Note.Octave4.F(180),
                Note.Rest(0),
                Note.Octave4.G(180),
                Note.Rest(0),
                Note.Octave4.C(180),
                Note.Rest(0),
                Note.Octave4.C(180),
                Note.Rest(0),
                Note.Octave5.D(180),
                Note.Rest(12),
                Note.Octave4.A(180),
                Note.Rest(180),
                Note.Octave4.GSharp(180),
                Note.Rest(30),
                Note.Octave4.G(180),
                Note.Rest(30),
                Note.Octave4.F(180),
                Note.Rest(30),
                Note.Octave4.D(180),
                Note.Rest(0),
                Note.Octave4.F(180),
                Note.Rest(0),
                Note.Octave4.G(180),
            ];

        Note[] bassLine =
        [
            Note.Rest(25),
                new Note(55, 180),
                Note.Rest(0),
                new Note(55, 180),
                Note.Rest(12),
                new Note(60, 180),
                new Note(57, 180),
                Note.Rest(180),
                new Note(54, 180),
                Note.Rest(30),
                new Note(51, 180),
                Note.Rest(30),
                new Note(48, 180),
                Note.Rest(30),
                new Note(45, 180),
                Note.Rest(1),
                new Note(48, 180),
                Note.Rest(0),
                new Note(51, 180),
                Note.Rest(0),
                new Note(54, 180),
                Note.Rest(0),
                new Note(57, 180),
                Note.Rest(0),
                new Note(60, 180),
                Note.Rest(12),
                new Note(60, 180),
                Note.Rest(180),
                new Note(60, 180),
                Note.Rest(30),
                new Note(60, 180),
                Note.Rest(30),
                new Note(60, 180),
                Note.Rest(30),
                new Note(60, 180),
                Note.Rest(0),
                new Note(60, 180),
                Note.Rest(0),
                new Note(60, 180),
            ];

        var melody = MusicComposer.ComposeFlat(song);
        var bass = MusicComposer.ComposeFlat(bassLine);

        AudioPlayer.Play(melody, volume: 0.1f, waitForSoundToBePlayed: false);
        AudioPlayer.Play(bass, volume: 0.1f);
    }

    //void SerializingTest()
    //{
    //    SerializerSettings settings = new()
    //    {
    //        ProgressReporter = AsyncProgressReport,
    //        ReportEvery = 5000,
    //        IncludeType = true,
    //        CircleReferencesEnabled = false,
    //        TheadsToUse = 12,
    //    };

    //    _ = "Input amount of objects to be serialized".StringAnimation(10).ForeachAsync(x => Console.Title = x);
    //    Console.WriteLine("Input Number:");
    //    string? input = Console.ReadLine();
    //    List<Vector3> list = new();
    //    if (input == null)
    //        return;

    //    try
    //    {
    //        int count = TypeWorker.CastPrimitive<int>(input);
    //        "Creating object list...".StringAnimation(10).Foreach(x => Console.Title = x);

    //        foreach (int i in count)
    //        {
    //            list.Add(new Vector3(i, i, i));
    //            if (i % 10000 == 0)
    //                Console.Title = $"{i} objects left";
    //        }
    //    }
    //    catch { Console.Clear(); return; }

    //    Console.Title = $"Working on {list.Count} objects...";

    //    Console.WriteLine("Serializing...");
    //    Stopwatch sw1 = Stopwatch.StartNew();
    //    string serializedResult = SnowSerializer.Serialize(list, settings).Result;
    //    sw1.Stop();

    //    Console.WriteLine("ites naar de console"); 

    //    Console.WriteLine("\n\nWriting to file...");
    //    FileManager.Write("SerializedData.txt", serializedResult, true);

    //    string readData = FileManager.Read("SerializedData.txt");

    //    Console.WriteLine("\n\nDeserializing...");
    //    Stopwatch sw2 = Stopwatch.StartNew();
    //    List<Vector3> deserializedResult = SnowSerializer.Deserialize<List<Vector3>>(readData, settings).Result;
    //    sw2.Stop();

    //    Console.WriteLine($"Serializing took {sw1.ElapsedTicks} ticks ({sw1.ElapsedMilliseconds}ms)");
    //    Console.WriteLine($"Deserializing took {sw2.ElapsedTicks} ticks ({sw2.ElapsedMilliseconds}ms)");

    //    Console.WriteLine($"Original list count: {list.Count}");
    //    Console.WriteLine($"Deserialized list count: {deserializedResult.Count}");
    //    Console.WriteLine(IsSame(list, deserializedResult) ? "Lists are the same" : "Lists are not the same");
    //    Console.Title = "Done...";

    //    bool IsSame(List<Vector3> l1, List<Vector3> l2)
    //    {
    //        if (l1.Count != l2.Count)
    //            return false;

    //        for (int i = 0; i < l1.Count; i++)
    //        {
    //            Vector3 item1 = l1[i];
    //            Vector3 item2 = l2[i];

    //            if (item1.x != item2.x || item1.y != item2.y || item1.z != item2.z)
    //                return false;
    //        }
    //        return true;
    //    }
    //}

     void WinterForgeSerializingTest<T>(Func<int, T> provider)
    {
        _ = "Input amount of objects to be serialized".StringAnimation(10).ForeachAsync(x => Console.Title = x);
        Console.WriteLine("Input Number:");
        string? input = Console.ReadLine();
        List<T> list = new();
        if (input == null)
            return;

        try
        {
            int count = TypeWorker.CastPrimitive<int>(input);
            "Creating object list...".StringAnimation(10).Foreach(x => Console.Title = x);

            foreach (int i in count)
            {
                list.Add(provider(i));
                if (i % 10000 == 0)
                    Console.Title = $"{i} objects left";
            }
        }
        catch { Console.Clear(); return; }

        Console.Title = $"Working on {list.Count} objects...";

        Console.WriteLine("Serializing...");
        Stopwatch sw1 = Stopwatch.StartNew();
        WinterForge.SerializeToFile(list, "Forged.txt");
        sw1.Stop();

        Console.WriteLine("\n\nDeserializing...");
        Stopwatch sw2 = Stopwatch.StartNew();
        List<T> deserializedResult = WinterForge.DeserializeFromFile<List<T>>("Forged.txt");
        sw2.Stop();

        Console.WriteLine($"Serializing took {sw1.ElapsedTicks} ticks ({sw1.ElapsedMilliseconds}ms)");
        Console.WriteLine($"Deserializing took {sw2.ElapsedTicks} ticks ({sw2.ElapsedMilliseconds}ms)");

        Console.WriteLine($"Original list count: {list.Count}");
        Console.WriteLine($"Deserialized list count: {deserializedResult.Count}");

        if(typeof(T) == typeof(Vector3))
        {
            Console.WriteLine(IsSame(list.Cast<Vector3>().ToList(), deserializedResult.Cast<Vector3>().ToList()) ? "Lists are the same" : "Lists are not the same");
        }
        Console.Title = "Done...";

        bool IsSame(List<Vector3> l1, List<Vector3> l2)
        {
            if (l1.Count != l2.Count)
                return false;

            for (int i = 0; i < l1.Count; i++)
            {
                Vector3 item1 = l1[i];
                Vector3 item2 = l2[i];

                if (item1.x != item2.x || item1.y != item2.y || item1.z != item2.z)
                    return false;
            }
            return true;
        }
    }

    void Copydir(DirectoryInfo dir, DirectoryInfo to, ref int fileshandled)
    {
        foreach (var file in dir.GetFiles())
        {
            file.CopyTo(Path.Combine(to.FullName, file.Name));
            fileshandled++;

            Console.WriteLine($"Copied {fileshandled} files");
        }

        foreach (var folder in dir.GetDirectories())
        {
            var newFolder = to.CreateSubdirectory(folder.Name);
            Copydir(folder, newFolder, ref fileshandled);
        }
    }

    private void Windows_OnAppTermination()
    {
    }

    void ReflectionsTest()
    {
        var r = new ReflectionTests();
        Type t = r.GetCustomAssembly();

        MethodInfo info = t.GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(int) });
        object? o1 = ActivatorExtra.CreateInstance(t, 0);
    }
}