using ForgeMantle;
using ForgeMantle.Values;
using ForgeMantle.Serialization;

internal class Program
{
    static void Main()
    {
        var configManager = new ConfigManager();

        var memoryLayer = new ConfigLayer(new FileStorage("Config.wfbin", new WinterForgeSerializer()));
        configManager.AddLayer(memoryLayer);

        configManager.Update(snap =>
        {
            snap.State["player.health"] = new BoxedConfigValue<int>(100);
            snap.State["player.name"] = new BoxedConfigValue<string>("Snow");
        });

        configManager.ApplyChanges();

        Console.WriteLine(configManager.GetValue("player.health")); // 100
        Console.WriteLine(configManager.GetValue("player.name"));   // Snow

        configManager.Update(snap => snap.State["player.health"] = new BoxedConfigValue<int>(50));
        configManager.ApplyChanges();

        Console.WriteLine(configManager.GetValue("player.health")); // 50

        configManager.Undo();

        Console.WriteLine(configManager.GetValue("player.health")); // 100 again

        configManager.Save();

        configManager.Restore();
        configManager.ApplyChanges();
    }
}