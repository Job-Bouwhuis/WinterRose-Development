using System.Diagnostics;
namespace TargetedKeySender;

public static class WindowScanner
{
    public static List<TargetWindow> GetWindows()
    {
        return Process.GetProcesses()
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle))
            .Select(TargetWindow.FromProcess)
            .ToList();
    }
}