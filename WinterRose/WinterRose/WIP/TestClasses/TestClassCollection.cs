using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using WinterRose.Vectors;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.WIP.TestClasses
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class EVENTSBABYYYYYYYY
    {
        public event EventHandler? KissMeBaby;

        public void Invoke() => KissMeBaby?.Invoke(this, new());
    }

    public class eventSubscriber
    {
        public void kissMeBaby(object? sender, EventArgs e)
        {
            Console.WriteLine("kiss me baby");
        }
    }

    public enum GameMode
    {
        None,
        GameRoom,
        Singleplayer,
        LocalMultiplayer
    }

    public class LightBlock
    {
        public int R;
        public int G;
        public int B;
        public CTC ctc;
    }
    public class CTC
    {
        public bool HasTarget { get; set; }
        public LightBlock Block { get; set; }
    }
    public class Randomness
    {
        public static string RandomString(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static int[] RandomInts(int length) => Enumerable.Repeat(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, length).Select(n => n[new Random().Next(0, 9)]).ToArray();
        public static Dictionary<ListClassTest, ListClassTest> RandomDic(int count)
        {
            Dictionary<ListClassTest, ListClassTest> result = new();
            foreach (int _ in Enumerable.Range(0, count))
                result.Add(ListClassTest.Random(), ListClassTest.Random());
            return result;
        }
    }

    [SerializeAs<Yeeter>]
    public class Yeeter : IEnumerable<int>
    {

        public int[] ints = new int[10];
        public Yeeter()
        {
            foreach (int i in Enumerable.Range(0, 10))
                ints[i] = i;
        }
        public IEnumerator<int> GetEnumerator()
        {
            foreach (int i in ints)
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class ListClassTest
    {
        public int num1, num2;
        public ListClassTest(int num1, int num2)
        {
            this.num1 = num1;
            this.num2 = num2;

        }
        public ListClassTest()
        {
            num1 = 0;
            num2 = 0;
        }
        public static ListClassTest Random() => new(new Random().Next(0, 10), new Random().Next(11, 20));
        public override string ToString() => $"{num1} - {num2}";
    }
    public class ArrayClassTest
    {
        public int num;
        public bool state;
        public ArrayClassTest(int num, bool state)
        {
            this.num = num;
            this.state = state;
        }
        public ArrayClassTest()
        {
            num = 0;
            state = false;
        }
        public static ArrayClassTest Random() => new(new Random().Next(0, 100), new Random().Next(0, 1) != 0);

    }

    public class DatesAndTimeSpans
    {
        public List<DateTime> Times { get; set; } = new() { DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now };
        public TimeSpan[] TimeSpans { get; set; } = new TimeSpan[2] { new TimeSpan(0, 1, 2), new(1, 1, 1, 1, 1) };
    }

    public class TimeDataTypeTests
    {
        public DateTime dateTime = DateTime.Now;
        public TimeSpan span = new(1, 1, 1, 1, 1);
        public TimeOnly time = new(6, 2, 5, 100);
        public DateOnly date = DateOnly.FromDateTime(DateTime.Now);

        public static void RunTests()
        {
            TimeDataTypeTests data = new();

            string serializedData = WinterForge.SerializeToString(data);
            
            var i = WinterForge.DeserializeFromString<TimeDataTypeTests>(serializedData);

            bool same = data.IsSame(i);
        }

        public bool IsSame(TimeDataTypeTests other)
        {
            return dateTime == other.dateTime && span == other.span && time == other.time && date == other.date;
        }
    }

    public class ContainsListOfNums
    {
        public List<Klant> nums = [];
    }

    public class Everything
    {
        public bool a;
        public byte b;
        public sbyte c;
        public char d;
        public decimal e;
        public double f;
        public float g;
        public int h;
        public uint i;
        public long l;
        public ulong m;
        public short n;
        public ushort o;
        public string p;
        public Vector3 vec;
        public List<int> nums;
        private event EventHandler? Shank;
        public GameMode mode;
        public Everything()
        {
            a = true;
            b = 1;
            c = 2;
            d = '3';
            e = 4;
            f = 5;
            g = 6;
            h = 7;
            i = 8;
            l = 9;
            m = 10;
            n = 11;
            o = 12;
            p = "13";
            vec = new Vector3(14, 15, 16);
            nums = [17, 18, 19, 20];
            //names = Array.Empty<string>();
            mode = GameMode.None;
        }
        public static Everything Random()
        {
            Everything r = new();

            r.a = true;
            r.b = TypeWorker.CastPrimitive(new Random().Next(0, 255), new byte());
            r.c = TypeWorker.CastPrimitive(new Random().Next(-128, 127), new sbyte());
            r.d = Convert.ToChar(new Random().Next(0, 255));
            r.e = TypeWorker.CastPrimitive(new Random().Next(0, 128) + new Random().NextDouble(), new decimal());
            r.f = TypeWorker.CastPrimitive(new Random().Next(0, 128) + new Random().NextDouble(), new double());
            r.g = TypeWorker.CastPrimitive(new Random().Next(0, 128) + new Random().NextDouble(), new float());
            r.h = new Random().Next(0, 10000);
            r.i = TypeWorker.CastPrimitive(new Random().Next(0, 10000), new uint());

            r.l = new Random().NextInt64(0, 9223372036854775807);
            r.m = TypeWorker.CastPrimitive(new Random().NextInt64(922337202685477, 922337203685477) + new Random().NextInt64(922337202685477, 922337203685477), new ulong());
            r.n = TypeWorker.CastPrimitive(new Random().Next(0, short.MaxValue), new short());
            r.o = TypeWorker.CastPrimitive(new Random().NextInt64(625, 655) + new Random().NextInt64(625, 655), new ushort());

            r.p = Randomness.RandomString(5).FirstCapital();
            r.vec = Vector3.Random();
            r.nums.ConsecutiveNumbers(10);
            //r.names = new string[new Random().Next(1, 100)];
            //for (int i = 0; i < r.names.Length; i++)
            //r.names[i] = Randomness.RandomString(10);
            switch (new Random().Next(0, 2))
            {
                case 0:
                    r.mode = GameMode.GameRoom;
                    break;
                case 1:
                    r.mode = GameMode.Singleplayer;
                    break;
                case 2:
                    r.mode = GameMode.LocalMultiplayer;
                    break;
            }
            r.Shank += r.WriteSomethingEvent;
            return r;
        }
        public void WriteSomethingEvent(dynamic? o, EventArgs e)
        {
            Console.WriteLine("Something " + o.p);
        }

        public void Invoke()
        {
            Shank?.Invoke(this, EventArgs.Empty);
        }
    }

    public class EverythingExtra : Everything
    {
        public TimeDataTypeTests times;
        public Dictionary<ListClassTest, ListClassTest> dic;
        public ListClassTest[] list;




        public EverythingExtra()
        {
            list = new ListClassTest[0];
            dic = new Dictionary<ListClassTest, ListClassTest>();
            times = new TimeDataTypeTests();
        }
        public static new EverythingExtra Random()
        {
            EverythingExtra r;
            r = CreateNewFromEverything(Everything.Random());

            r.list = new ListClassTest[100];
            for (int i = 0; i < r.list.Length; i++)
                r.list[i] = ListClassTest.Random();
            r.dic = Randomness.RandomDic(100);
            r.times = new TimeDataTypeTests();
            return r;
        }
        public static EverythingExtra CreateNewFromEverything(Everything e)
        {
            EverythingExtra r = new();
            r.a = e.a;
            r.b = e.b;
            r.c = e.c;
            r.d = e.d;
            r.e = e.e;
            r.f = e.f;
            r.g = e.g;
            r.h = e.h;
            r.i = e.i;
            r.l = e.l;
            r.m = e.m;
            r.n = e.n;
            r.o = e.o;
            r.p = e.p;
            r.vec = e.vec;
            r.nums = e.nums;
            r.mode = e.mode;
            return r;
        }
    }

    public class Node
    {
        public Vector2 position;
        public int player;

        public ArrayClassTest[] floats = new ArrayClassTest[20];

        public Node()
        {
            position = new Vector2();
            player = 0;
            for (int i = 0; i < 20; i++)
                floats[i] = ArrayClassTest.Random();
        }
    }

    public class Weirdness
    {
        public Node[] nodes;

        public Weirdness()
        {
            nodes = new Node[20];
            for (int i = 0; i < 20; i++)
                nodes[i] = new();
        }
    }


    public class Klant
    {
        public int id;
        public string? name;
        public string? phoneNumber;
        public static Klant Random() => new(new Random().Next(0, 1000000)) { name = Randomness.RandomString(5), phoneNumber = Randomness.RandomString(5) };
        public Klant(int id) : this() => this.id = id;
        public Klant() 
        {
            orders.Add(new Order());
            orders.Add(new Order());

        }
        public List<Order> orders = [];
    }

    public class Order
    {
        public List<dinges> dingen = [];
        public Order()
        {
            dingen.Add(dinges.Random());
            dingen.Add(dinges.Random());
        }
    }

    public record SomeGuy(string Name, string LastName, int Age)

    {
        public SomeGuy() : this("", "", 0) { }
    }

    public class dinges
    {
        [WFInclude]
        public Vector2 vec { get; set; }
        public dinges()
        {
            vec = new();
        }
        public static dinges Random() => new() { vec = Vector2.Random() };
    }




    public enum GameModeSetting
    {
        None,
        Beginner,
        Easy,
        Normal,
        Hard,
        Expert,
        Impossible
    }
    
    public class Employee
    {
        public int MwIdnr { get; set; }
        public string? MwNaam { get; set; }
        public int? MwAfdnr { get; set; }
        public short? MwDflUrenPWk { get; set; }
        public DateTime? MwGebdatum { get; set; }
        public DateTime? MwIndienst { get; set; }
        public string? MwInfo { get; set; }
        public string? MwOptions { get; set; }
        public int? MwKgnr { get; set; }
        public string? MwDflIndprj { get; set; }
        public string? MwExtNummer { get; set; }
        public string? MwEmail { get; set; }
        public string? MwVrijeTekst1 { get; set; }
        public string? MwVrijeTekst2 { get; set; }
        public string? MwVrijeTekst3 { get; set; }
        public string? MwVrijeTekst4 { get; set; }
        public string? MwVrijeTekst5 { get; set; }
        public DateTime? MwUitdienst { get; set; }
        public string? MwDflMgnr { get; set; }
        public string? MwDflWpnr { get; set; }
        public DateTime? MwLastmodified { get; set; }
        public string? MwTariefcode { get; set; }
        public short? MwRegiMode { get; set; }
        public string? MwCode { get; set; }
        public DateTime? MwStarttijd { get; set; }
        public double? MwNrmCapMa { get; set; }
        public double? MwNrmCapDi { get; set; }
        public double? MwNrmCapWo { get; set; }
        public double? MwNrmCapDo { get; set; }
        public double? MwNrmCapVr { get; set; }
        public double? MwNrmCapZa { get; set; }
        public double? MwNrmCapZo { get; set; }
        public short? MwGrpnr { get; set; }
        public short? MwRegiPerm { get; set; }
        public short? MwSkipDetplYn { get; set; }
        public double? MwNrmOndrhdMa { get; set; }
        public double? MwNrmOndrhdDi { get; set; }
        public double? MwNrmOndrhdWo { get; set; }
        public double? MwNrmOndrhdDo { get; set; }
        public double? MwNrmOndrhdVr { get; set; }
        public double? MwNrmOndrhdZa { get; set; }
        public double? MwNrmOndrhdZo { get; set; }
        public double? MwNrmVoorzMa { get; set; }
        public double? MwNrmVoorzDi { get; set; }
        public double? MwNrmVoorzWo { get; set; }
        public double? MwNrmVoorzDo { get; set; }
        public double? MwNrmVoorzVr { get; set; }
        public double? MwNrmVoorzZa { get; set; }
        public double? MwNrmVoorzZo { get; set; }
        public short? MwWbsoYn { get; set; }
        public double? MwNrm2CapMa { get; set; }
        public double? MwNrm2CapDi { get; set; }
        public double? MwNrm2CapWo { get; set; }
        public double? MwNrm2CapDo { get; set; }
        public double? MwNrm2CapVr { get; set; }
        public double? MwNrm2CapZa { get; set; }
        public double? MwNrm2CapZo { get; set; }
        public double? MwNrm3CapMa { get; set; }
        public double? MwNrm3CapDi { get; set; }
        public double? MwNrm3CapWo { get; set; }
        public double? MwNrm3CapDo { get; set; }
        public double? MwNrm3CapVr { get; set; }
        public double? MwNrm3CapZa { get; set; }
        public double? MwNrm3CapZo { get; set; }
        public int? MwAfdnr2 { get; set; }
        public int? MwAfdnr3 { get; set; }
        public short? MwUrenInvoerType { get; set; }
        public short? MwRegiMmbYn { get; set; }
        public short? MwImportcontroleYn { get; set; }
        public DateTime? MwCreated { get; set; }
        public byte[]? MwPicture { get; set; }
        public short? MwGender { get; set; }
        public string? MwAdres { get; set; }
        public string? MwPc { get; set; }
        public string? MwPlaats { get; set; }
        public string? MwLand { get; set; }
        public string? MwTel { get; set; }
        public string? MwMobile { get; set; }
        public string? MwErpGuid { get; set; }
        public short? MwRegiMatYn { get; set; }
        public short? MwUitzendkrachtYn { get; set; }
        public short? MwPermanentYn { get; set; }
        public DateTime? MwStarttijdDi { get; set; }
        public DateTime? MwStarttijdWo { get; set; }
        public DateTime? MwStarttijdDo { get; set; }
        public DateTime? MwStarttijdVr { get; set; }
        public DateTime? MwStarttijdZa { get; set; }
        public DateTime? MwStarttijdZo { get; set; }
        public string? MwTagId { get; set; }
        public short? MwSfcOnbemandYn { get; set; }
        public short? MwSfcClusterYn { get; set; }
        public short? MwSfcPalletbonYn { get; set; }
        public short? MwSfcXtrahrsYn { get; set; }
        public short? MwSfcSupervisorYn { get; set; }
        public string? MwUitzendburo { get; set; }
        public short? MwOverurenYn { get; set; }
        public short? MwSfcLijnregiYn { get; set; }
        public string? MwVoornaam { get; set; }
        public DateTime? MwIfLastmodified { get; set; }
        public int? MwPauzeroosterMa { get; set; }
        public int? MwPauzeroosterDi { get; set; }
        public int? MwPauzeroosterWo { get; set; }
        public int? MwPauzeroosterDo { get; set; }
        public int? MwPauzeroosterVr { get; set; }
        public int? MwPauzeroosterZa { get; set; }
        public int? MwPauzeroosterZo { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

}