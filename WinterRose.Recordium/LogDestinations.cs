namespace WinterRose.Recordium;

public static class LogDestinations
{
    private static List<ILogDestination> logDestinations = [];

    public static void AddDestination(ILogDestination logDestination)
    {
        logDestinations.Add(logDestination);
    }

    public static void RemoveDestination(ILogDestination logDestination)
    {
        logDestinations.Remove(logDestination);
    }

    public static List<ILogDestination> GetAllDestinations(params List<ILogDestination> extenders)
    {
        var r = new List<ILogDestination>();
        r.AddRange(logDestinations);
        r.AddRange(extenders);
        return r;
    }
}