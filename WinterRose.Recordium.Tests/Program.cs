using WinterRose.Recordium;


namespace WinterRose.Recordium.Tests;

class Program
{
    static void Main(string[] args)
    {
        LogDestinations.AddDestination(new ConsoleLogDestination());
        LogDestinations.AddDestination(new FileLogDestination("Logs"));

        Log logger = new Log("Tests");

        logger.Debug("debug test");
        logger.Info("info test");
        logger.Warning("error test");
        logger.Error("error test");
        logger.Critical("critical test");
        logger.Fatal("catastrophic test");

        try
        {
            throw new Exception("test");
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "a manually logged exception caught using try catch.");
        }
        throw new Exception("an uncaught exception (this is the message of the exception, not a seperate message)");
    }
}