using WinterRose.EventBusses;
using WinterRose.WinterForgeSerializing;

internal class Program
{
    private static void Main(string[] args)
    {
        EventBus bus = new();

        using var sub1 = bus.Subscribe("TestEvent", Behavior.Of("TakeDamage")
                                                      .WithParam("Target", "Player1"));

        bus.Invoke("TestEvent", ("DamageAmount", 10));

        string s = WinterForge.SerializeToString(bus, TargetFormat.FormattedHumanReadable);
        Console.WriteLine(s);
    }
}

public sealed class TakeDamageBehavior : Behavior
{
    public string Target { get; set; }
    public override void Execute(EventContext context)
    {
        Console.WriteLine($"Taking {context["DamageAmount"]} damage to {Target} from event {context.EventName}");
    }
}