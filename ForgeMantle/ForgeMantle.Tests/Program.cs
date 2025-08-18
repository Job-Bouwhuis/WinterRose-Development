using WinterRose.ForgeMantle;
using WinterRose.ForgeMantle.Values;
using WinterRose.ForgeMantle.Serialization;
using WinterRose.ForgeMantle.Models;

internal class Program
{
    static void Main()
    {
        var configManager = new ConfigManager();

        var memoryLayer = new ConfigLayer(new WindowsRegistryStorage("Config", new WinterForgeStringSerializer<ConfigSnapshot>()));
        configManager.AddLayer(memoryLayer);

        configManager.Update(snap =>
        {
            snap.State["player.health"] = new ConfigValue<int>(100);
            snap.State["player.name"] = new ConfigValue<string>("Snow");
        });

        configManager.ApplyChanges();

        Console.WriteLine(configManager.GetValue("player.health")); // 100
        Console.WriteLine(configManager.GetValue("player.name"));   // Snow

        configManager.Update(snap => snap.State["player.health"] = new ConfigValue<int>(50));
        configManager.ApplyChanges();

        Console.WriteLine(configManager.GetValue("player.health")); // 50

        configManager.Undo();

        Console.WriteLine(configManager.GetValue("player.health")); // 100 again

        configManager.Save();

        configManager.Restore();
        configManager.ApplyChanges();
    }
}