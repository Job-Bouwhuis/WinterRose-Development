using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Vectors;

namespace WinterRose;

public static partial class Windows
{
    public static partial class ScreenSize
    {
        // Windows DllImports (keep existing code)
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CMONITORS = 80;

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfo
        {
            public int cbSize;
            public Rect rcMonitor;
            public Rect rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public static Vector2I GetScreenSize(int screenIndex)
        {
            if (OperatingSystem.IsWindows())
                return GetWindowsScreenSize(screenIndex);
            else if (OperatingSystem.IsLinux())
                return GetLinuxScreenSize(screenIndex);
            else
                throw new PlatformNotSupportedException();
        }

        public static Vector2I GetScreenSize() => GetScreenSize(0);

        public static int GetNumberOfScreens()
        {
            if (OperatingSystem.IsWindows())
                return GetSystemMetrics(SM_CMONITORS);
            else if (OperatingSystem.IsLinux())
                return GetLinuxMonitors().Count;
            else
                throw new PlatformNotSupportedException();
        }

        #region Windows Implementation
        private static Vector2I GetWindowsScreenSize(int screenIndex)
        {
            List<Rect> monitors = new List<Rect>();
            IntPtr hdc = GetDC(IntPtr.Zero);

            MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData) =>
            {
                MonitorInfo mi = new MonitorInfo();
                mi.cbSize = Marshal.SizeOf(typeof(MonitorInfo));
                if (GetMonitorInfo(hMonitor, ref mi))
                    monitors.Add(mi.rcMonitor);
                return true;
            };

            EnumDisplayMonitors(hdc, IntPtr.Zero, callback, IntPtr.Zero);
            ReleaseDC(IntPtr.Zero, hdc);

            if (screenIndex < 0 || screenIndex >= monitors.Count)
                throw new ArgumentOutOfRangeException(nameof(screenIndex));

            Rect monitor = monitors[screenIndex];
            int width = Math.Max(Math.Abs(monitor.left), monitor.right);
            int height = monitor.bottom + Math.Abs(monitor.top);
            return new Vector2I(width, height);
        }
        #endregion

        #region Linux Implementation
        private static Vector2I GetLinuxScreenSize(int screenIndex)
        {
            var monitors = GetLinuxMonitors();
            if (screenIndex < 0 || screenIndex >= monitors.Count)
                throw new ArgumentOutOfRangeException(nameof(screenIndex));
            return monitors[screenIndex];
        }

        private static List<Vector2I> GetLinuxMonitors()
        {
            List<Vector2I> screens = new List<Vector2I>();

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = "-c \"xrandr --listmonitors\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Skip(1)) // skip header
                {
                    var parts = line.Trim().Split(' ');
                    // parts example: 0: +*HDMI-1 1920/527x1080/296+0+0
                    if (parts.Length >= 3)
                    {
                        var resPart = parts[2].Split('+')[0]; // take the resolution before '+'
                        var res = resPart.Split('x');
                        if (res.Length == 2 &&
                            int.TryParse(res[0], out int width) &&
                            int.TryParse(res[1], out int height))
                        {
                            screens.Add(new Vector2I(width, height));
                        }
                    }
                }
            }
            catch
            {
                // fallback: assume 1 monitor 800x600
                screens.Add(new Vector2I(800, 600));
            }

            return screens;
        }
        #endregion
    }

}
