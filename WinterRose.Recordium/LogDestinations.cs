namespace WinterRose.Recordium;

public static class LogDestinations
{
    private static List<ILogDestination> logDestinations = [];

    public static void AddDestination(ILogDestination logDestination)
    {
        ArgumentNullException.ThrowIfNull(logDestination);
        var others = logDestinations.Where(x => x.GetType() == logDestination.GetType());
        foreach (var o in others)
            if (!o.AllowDuplicate(logDestination))
                return;
        
        logDestinations.Add(logDestination);
    }

    public static void RemoveDestination(ILogDestination logDestination)
    {
        logDestinations.Remove(logDestination);
    }

    public static IReadOnlyList<ILogDestination> GetAllDestinations()
    {
        return logDestinations;
    }
}