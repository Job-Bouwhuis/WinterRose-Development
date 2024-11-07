using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Gets the size of the specified screen.
        /// </summary>
        /// <param name="screenIndex">The index of the screen.</param>
        /// <returns>The size of the specified screen.</returns>
        [Experimental("WR_EXPERIMENTAL")]
        public static Vector2I GetScreenSize(int screenIndex)
        {
            List<Rect> monitors = new List<Rect>();
            IntPtr hdc = GetDC(IntPtr.Zero);

            MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData) =>
            {
                MonitorInfo mi = new MonitorInfo();
                mi.cbSize = Marshal.SizeOf(typeof(MonitorInfo));
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    monitors.Add(mi.rcMonitor);
                }
                return true; // Continue enumeration
            };

            EnumDisplayMonitors(hdc, IntPtr.Zero, callback, IntPtr.Zero);
            ReleaseDC(IntPtr.Zero, hdc);

            if (screenIndex >= 0 && screenIndex < monitors.Count)
            {
                Rect monitor = monitors[screenIndex];
                int width = Math.Max(Math.Abs(monitor.left), monitor.right);
                int height = monitor.bottom + Math.Abs(monitor.top);
                return new Vector2I(width, height);
            }

            throw new ArgumentOutOfRangeException(nameof(screenIndex), "Invalid screen index.");
        }

        /// <summary>
        /// Gets the screen size of the primary screen.
        /// </summary>
        /// <returns></returns>
        public static Vector2I GetScreenSize()
        {
            List<Rect> monitors = new List<Rect>();
            IntPtr hdc = GetDC(IntPtr.Zero);

            MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData) =>
            {
                MonitorInfo mi = new MonitorInfo();
                mi.cbSize = Marshal.SizeOf(typeof(MonitorInfo));
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    monitors.Add(mi.rcMonitor);
                }
                return true; // Continue enumeration
            };

            EnumDisplayMonitors(hdc, IntPtr.Zero, callback, IntPtr.Zero);
            ReleaseDC(IntPtr.Zero, hdc);


            Rect monitor = monitors.Where(monitors => monitors is { top:0, left:0 }).First();

            int width = Math.Max(Math.Abs(monitor.left), monitor.right);
            int height = monitor.bottom + Math.Abs(monitor.top);
            return new Vector2I(width, height);
        }

        /// <summary>
        /// Gets the number of screens connected to the PC.
        /// </summary>
        /// <returns>The number of screens connected to the PC.</returns>
        public static int GetNumberOfScreens()
        {
            return GetSystemMetrics(SM_CMONITORS);
        }
    }
}
