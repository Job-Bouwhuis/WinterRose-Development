using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace WinterRoseUtilityApp.MailReader;

public class MailWatcher
{
    private readonly List<IMailMonitor> monitors = new();
    private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(5);
    private DateTime lastCheck = DateTime.MinValue;

    public void AddMonitor(IMailMonitor monitor)
    {
        monitors.Add(monitor);
    }

    public async Task UpdateAsync()
    {
        if (DateTime.UtcNow - lastCheck < pollInterval)
            return;

        lastCheck = DateTime.UtcNow;

        foreach (IMailMonitor monitor in monitors)
        {
            await monitor.FetchNewAsync(CancellationToken.None);
        }
    }
}
