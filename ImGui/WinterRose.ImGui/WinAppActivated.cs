using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ImGuiApps
{
    public class WindowFocusDetector
    {
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        private static WinEventDelegate winEventDelegate;
        private static IntPtr hWinEventHook;

        public static event EventHandler<EventArgs> WindowFocusChanged;

        public static void Start()
        {
            winEventDelegate = new WinEventDelegate(WinEventProc);
            hWinEventHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, winEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        public static void Stop()
        {
            UnhookWinEvent(hWinEventHook);
        }

        private static void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            WindowFocusChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
