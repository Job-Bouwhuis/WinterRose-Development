using WinterRose.Recordium;


namespace WinterRose.Recordium.Tests;

class Program
{
    static void Main(string[] args)
    {
        LogDestinations.AddDestination(new ConsoleDestination());
        LogDestinations.AddDestination(new FileDestination("Logs"));

        Log logger = new Log("Tests");

        logger.Debug("debug test");
        logger.Info("info test");
        logger.Warning("error test");
        logger.Error("error test");
        logger.Critical("critical test");
        logger.Catastrophic("catastrophic test");

        try
        {
            throw new Exception("test");
        }
        catch (Exception ex)
        {
            logger.Catastrophic(ex, "Unhandled exception during execution of app");
        }
        throw new Exception("uncaught");
        Console.ReadLine();
    }
}