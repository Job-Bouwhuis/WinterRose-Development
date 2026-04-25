using System.Drawing;
using System.Diagnostics;
namespace TargetedKeySender;

public class TargetWindow
{
    public IntPtr Handle;
    public string Title;
    public string ProcessName;
    public Icon Icon;

    private static Dictionary<string, Icon> ICON_CACHE = new();

    public static TargetWindow FromProcess(Process process)
    {
        return new TargetWindow
        {
            Handle = process.MainWindowHandle,
            Title = process.MainWindowTitle,
            ProcessName = process.ProcessName,
            Icon = GetProcessIcon(process)
        };
    }

    public void Activate()
    {
        if (NativeMethods.IsIconic(Handle))
        {
            NativeMethods.ShowWindow(Handle, NativeMethods.SW_RESTORE);
        }

        NativeMethods.SetForegroundWindow(Handle);
    }

    private static Icon GetProcessIcon(Process process)
    {
        try
        {
            string filePath = process.MainModule.FileName;

            if (string.IsNullOrEmpty(filePath))
                return SystemIcons.Application;

            if (ICON_CACHE.TryGetValue(filePath, out Icon cached))
                return cached;

            Icon icon = Icon.ExtractAssociatedIcon(filePath);

            ICON_CACHE[filePath] = icon;

            return icon;
        }
        catch
        {
            return SystemIcons.Application;
        }
    }
}
