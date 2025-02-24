using WinterRose.Serialization;
using WinterRose.Vectors;
using WinterRose.FileManagement;
using System.Diagnostics;
using WinterRose.WIP.TestClasses;
using WinterRose;
using WinterRose.Encryption;
using WinterRose.WIP.ReflectionTests;
using System.Reflection;
using WinterRose.Networking.TCP;
using System.Diagnostics.CodeAnalysis;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.Factory;
using System.IO.Compression;
using WinterRose.Plugins;
using SnowLibraryTesting;
using WinterRose.Networking;
using WinterRose.Music;
using System.Net;
using System.Xml;
using System;
using WinterRose.Serialization.Things;

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
        SerializeReferenceCache cache = new();
        object o = new
        {
            A = 4
        };
        Person p = Person.Random();
        int key = cache[o];
        int key2 = cache[p];
        int key3 = cache[p];

        SerializationTests();

        WinterThorn script = new(
            """
            namespace Test.Movement
            {
            	class Move
            	{
                    function lmao
                    {
                        Console.WriteLine("test");
                    }
                }
            }
            """, "Test Script", "", "Me", new(0, 0, 0));

        script.GenerateDocumentation(new DirectoryInfo("ThornDocs"));

        Function main = script.GetInstancedClass("Test.Movement", "Move")!.Block.Functions[0];
        main.Invoke();
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

    TCPServer server;
    TCPUser user;

    public void NetworkingTest()
    {
HOJ:

        var ip = Network.GetLocalIPAddress();
        Console.WriteLine("This PCs IP: " + ip.ToString());

        Console.WriteLine("Host or join?");

        string input = Console.ReadLine();
        if (input is not "host" and not "join")
        {
            Console.WriteLine("Invalid input.");
            goto HOJ;
        }

        if (input == "host")
        {
            Host();
        }
        else
        {
            Join();
        }

        goto HOJ;


        void Host()
        {
            server = new();
            server.OnMessageReceived += OnMessageReceivedSERVER;
            server.ResponseCommandReceived += OnResponseMessageSERVER;
            server.Start(Network.GetLocalIPAddress(), 8000);

            while (true)
            {
                string message = Console.ReadLine();
                if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    server.Dispose();
                    break;
                }
                if (message.Equals("resp", StringComparison.OrdinalIgnoreCase))
                {
                    var sw = Stopwatch.StartNew();
                    Packet response = server.SendAndResponse("yeeto", server.ConnectedClients[0].Client);
                    sw.Stop();
                    Console.WriteLine(response.Payload);
                    Console.WriteLine("Gotten response in " + sw.ElapsedTicks + "ticks, or " + sw.ElapsedMilliseconds + "ms");
                    continue;
                }
                if (message.Equals("ping", StringComparison.OrdinalIgnoreCase))
                {
                    List<long> ms = [];
                    Stopwatch sw = new();

                    Stopwatch totalsw = Stopwatch.StartNew();

                    foreach (int i in 10000)
                    {
                        sw.Restart();
                        Packet response = server.SendAndResponse("ping", server.ConnectedClients[0].Client);
                        sw.Stop();
                        ms.Add(sw.ElapsedTicks);
                    }

                    totalsw.Stop();

                    var avarage = ms.Average();
                    var min = ms.Min();
                    var max = ms.Max();
                    Console.WriteLine($"10000 pings took an avarage of {avarage / 10000.0}ms. max of {max / 10000.0}ms, and min of {min / 10000.0}ms\n" +
                        $"all pings took {totalsw.ElapsedTicks / 10000.0}ms");
                }
                server.Send(message);
            }
        }

        void OnResponseMessageSERVER(string message, TCPClientInfo sender, Guid requestID)
        {
            if (message == "ping")
            {
                Console.WriteLine("command received: " + message);
                server.SendResponse("pong", sender.Client, requestID);
            }
        }

        void Join()
        {
            user = new();
            user.OnMessageReceived += OnMessageReceivedCLIENT;
            user.ResponseMessageReceived += OnResponseMessageCLIENT;

            Console.WriteLine("Enter IP to join");
            string ip = Console.ReadLine() ?? "self";

            if (ip == "self")
                ip = Network.GetLocalIPAddress().ToString();

            user.Connect(ip, 8000);

            if (!user.IsConnected)
            {
                Console.WriteLine("Failed to connect to server.");
                return;
            }

            string response = user.SendAndResponse("ping").Payload!;
            Console.WriteLine(response);

            while (true)
            {
                if (user.IsDisposed || !user.IsConnected)
                    break;

                string message = Console.ReadLine();

                if (user.IsDisposed || !user.IsConnected)
                    break;
                if (message.Equals("disconnect", StringComparison.OrdinalIgnoreCase))
                {
                    user.Disconnect();
                    break;
                }
                if (message.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("---------------");
                    foreach (string guid in user.GetAllConnectionsFromServer())
                    {
                        Console.WriteLine(guid);
                    }
                    Console.WriteLine("---------------");
                }
                if (message.StartsWith("info ", StringComparison.OrdinalIgnoreCase))
                {
                    string r = user.GetConnectionInfo(Guid.Parse(message.Split(' ')[1]));
                    Console.WriteLine(r);
                }
                user.Send(message);
            }
        }
    }

    void OnMessageReceivedCLIENT(string message, TCPUser self, TCPClientInfo? sender)
    {
        if (sender != null)
        {
            Console.WriteLine($"From {sender.Username}: {message}");
        }
        Console.WriteLine("From server: " + message);
    }
    void OnResponseMessageCLIENT(string message, Guid requestID)
    {
        Console.WriteLine("Request received: " + message);
        user.SendResponse("i love it", requestID);
    }

    void OnMessageReceivedSERVER(string message, TCPClientInfo client, TCPClientInfo? routing)
    {
        Console.WriteLine($"From client: {message}");

    }


    private unsafe void SerializationTests()
    {
        while (true)
        {
            SerializingTest();

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

    private static unsafe void PluginTests()
    {
        Plugin plugin = new("test1");
        var assembly = plugin.LoadPlugin("Scripts\\TestStuff.cs");

        var types = assembly.GetTypes();
        IPlugin pl = (IPlugin)Activator.CreateInstance(types[0]);

        pl.OnLoad();
        pl.Run();
        pl.OnUnload();

        Console.WriteLine("Make changes to the plugin file, and press enter when done to reload the plugin...");
        Console.ReadLine();

        assembly = plugin.LoadPlugin("Scripts\\TestStuff.cs");

        types = assembly.GetTypes();
        pl = (IPlugin)Activator.CreateInstance(types[0]);

        pl.OnLoad();
        pl.Run();
        pl.OnUnload();
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

        string serialized = SnowSerializer.Serialize(obj).Result;
        object deser = SnowSerializer.Deserialize<object>(serialized).Result;
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

    private void ThornScripTests()
    {
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

    private byte[] Compress(byte[] data)
    {
        using (var compressedStream = new MemoryStream())
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            zipStream.Write(data, 0, data.Length);
            return compressedStream.ToArray();
        }
    }

    private byte[] Decompress(byte[] data)
    {
        using (var compressedStream = new MemoryStream(data))
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }


    void SerializingTest()
    {
        SerializerSettings settings = new()
        {
            ProgressReporter = AsyncProgressReport,
            ReportEvery = 5000,
            IncludeType = false,
            TheadsToUse = 12
        };

        _ = "Input amount of objects to be serialized".StringAnimation(10).ForeachAsync(x => Console.Title = x);
        Console.WriteLine("Input Number:");
        string? input = Console.ReadLine();
        List<Vector3> list = new();
        if (input == null)
            return;

        try
        {
            int count = TypeWorker.CastPrimitive<int>(input);
            "Creating object list...".StringAnimation(10).Foreach(x => Console.Title = x);

            foreach (int i in count)
            {
                list.Add(new Vector3(i, i, i));
                if (i % 10000 == 0)
                    Console.Title = $"{i} objects left";
            }
        }
        catch { Console.Clear(); return; }

        Console.Title = $"Working on {list.Count} objects...";

        Console.WriteLine("Serializing...");
        Stopwatch sw1 = Stopwatch.StartNew();
        string serializedResult = SnowSerializer.Serialize(list, settings).Result;
        sw1.Stop();

        Console.WriteLine("\n\nWriting to file...");
        FileManager.Write("SerializedData.txt", serializedResult, true);

        string readData = FileManager.Read("SerializedData.txt");

        Console.WriteLine("\n\nDeserializing...");
        Stopwatch sw2 = Stopwatch.StartNew();
        List<Vector3> deserializedResult = SnowSerializer.Deserialize<List<Vector3>>(readData, settings).Result;
        sw2.Stop();

        Console.WriteLine($"Serializing took {sw1.ElapsedTicks} ticks ({sw1.ElapsedMilliseconds}ms)");
        Console.WriteLine($"Deserializing took {sw2.ElapsedTicks} ticks ({sw2.ElapsedMilliseconds}ms)");

        Console.WriteLine($"Original list count: {list.Count}");
        Console.WriteLine($"Deserialized list count: {deserializedResult.Count}");
        Console.WriteLine(IsSame(list, deserializedResult) ? "Lists are the same" : "Lists are not the same");
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

    void Method()
    {
        Task.Delay(1).Wait();
    }

    void ScramblerTests()
    {
        StringScrambler encrypter = new(new ScramblerSettings(ScrambleConfiguration.I, 0, 0), new ScramblerSettings(ScrambleConfiguration.III, 0, 0), new ScramblerSettings(ScrambleConfiguration.IV, 0, 0));

        List<Everything> list = new();
        WinterUtils.Repeat(() => list.Add(Everything.Random()), 100000, i => Console.WriteLine(i));
        string serialized = SnowSerializer.Serialize(list).ToString();

        string encrypted = encrypter.Encrypt(serialized);

        string decrypted = encrypter.Decrypt(encrypted);

        FileManager.Write("ScramblerTests.txt", encrypted, true);

        Console.WriteLine($"\n\n\t{decrypted == serialized}");
    }

    string TestReturn(int count, bool yeet) => $"{Randomness.RandomString(count)} {yeet}";

    void Something(string s) => Console.WriteLine(s);

    void FileIOTests()
    {
        FileManager.WriteLine("test.txt", "something11", true);
        FileManager.WriteLine("test.txt", "something22");
        FileManager.WriteLine("test.txt", "something33");
        FileManager.WriteLine("test.txt", "something44");
        FileManager.WriteLine("test.txt", "something55");

        string? one = FileManager.ReadLine("test.txt", 2);
        string? two = FileManager.Read("test.txt").RemoveNewlineCharacters();
        string[]? three = FileManager.ReadAllLines("test.txt").RemoveReadAnomalies().ToStringArray();
        Console.WriteLine($"{one}\n{two}\n");
        three.Foreach(x => Console.WriteLine(x));
        Console.ReadKey();
    }

    void AsyncProgressReport(ProgressReporter e)
    {
        Console.WriteLine();
        Console.WriteLine($"{e.Progress}% -- {e.Message}");
    }

    private static List<MutableString> GetCombinations(int lastNumber)
    {
        var combinations = new List<MutableString> { string.Empty };
        List<int> items = (List<int>)WinterUtils.CreateList(typeof(int));
        items.ConsecutiveNumbers(lastNumber);
        foreach (var item in items)
        {
            var newCombinations = new List<MutableString>();

            foreach (ReadOnlySpan<char> combination in combinations)
            {
                for (var i = 0; i <= combination.Length; i++)
                {
                    MutableString combi = new();
                    combi += combination[..i];
                    combi += item;
                    combi += combination[i..];
                    newCombinations.Add(combi);
                }
            }

            combinations.AddRange(newCombinations);
        }

        return combinations;
    }

    bool IsEqual(List<Vector3> list1, List<Vector3> list2)
    {
        if (list1.Count != list2.Count)
            return false;
        for (int i = 0; i < list1.Count; i++)
        {
            var item1 = list1[i];
            var item2 = list2[i];
            if (item1.x != item2.x || item1.y != item2.y || item1.z != item2.z)
                return false;
        }
        return true;
    }

    void ChristmasTree()
    {
        Console.WriteLine("Merry Christmas!");
        Console.WriteLine("I am a program that is going to build you a Christmas Tree.");
        Console.WriteLine("Please tell me how high you want you tree to be...");
        int height = int.Parse(Console.ReadLine());
        Console.WriteLine();
        Console.WriteLine();
        while (true)
        {
            for (int i = 0; i < height; i++)
            {
                //linkerkant van de boom
                WinterUtils.Repeat(() => Console.Write(" "), height - i);
                WinterUtils.Repeat(() => Console.Write("*"), i + 1);

                //rechter kant van de boom
                WinterUtils.Repeat(() => Console.Write("*"), i + 1);
                Console.WriteLine("\n");
            }
            //stammetje op de juiste plek zetten
            WinterUtils.Repeat(() => Console.WriteLine(" "), height);
            Console.WriteLine("*");
            Console.WriteLine();

            //ask the user if he wants another tree and repeat the cycle

            Console.WriteLine("Please tell me how high you want you tree to be if you want anther tree...");
            height = int.Parse(Console.ReadLine());
            Console.WriteLine("\n");

        }
    }
}