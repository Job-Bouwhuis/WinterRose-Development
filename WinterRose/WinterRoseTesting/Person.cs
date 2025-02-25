using WinterRose;
using WinterRose.SourceGeneration.Serialization;

namespace SnowLibraryTesting;

[GenerateSerializer]
public class Person
{
    public string name;
    public int age;
    public List<Person> kids;

    public Person(string name, int age, List<Person> kids)
    {
        this.name = name;
        this.age = age;
        this.kids = kids;
    }

    [DefaultArguments("papa", 45, 2)]
    public Person(string name, int age, int kids)
    {
        this.name = name;
        this.age = age;
        this.kids = new();
        foreach (var i in Enumerable.Range(0, kids))
        {
            this.kids.Add(new($"nameKid {i}", new Random().Next(0, 70)));
        }
    }
    public Person(string name, int age)
    {
        this.name = name;
        this.age = age;
        kids = new();
    }

    public Person() { }

    public static Person Random()
    {
        int age = new Random().Next(0, 105);
        int kids = (age > 30) ? new Random().Next(0, 4) : 0;
        return new(WinterRose.WIP.TestClasses.Randomness.RandomString(6), age, kids);
    }
    public override string ToString()
    {
        string result = "";
        result += "{\n";
        result += $"  Name: {name} \n  Age: {age} \n  Kids: {kids.Count}";
        if (kids.Count > 0)
        {
            foreach (var k in kids)
            {
                result += "\n  {\n";
                result += $"    Name: {k.name} \n    Age: {k.age} \n    Kids: {k.kids.Count}\n";
                result += "  }";
            }
        }
        result += "\n}";
        return result;
    }
    public void EventSubscriber(object? o, EventArgs e)
    {
        Console.WriteLine("Event raised");
    }
}