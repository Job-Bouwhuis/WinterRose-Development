using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    public static partial class Windows
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int MessageBox(HandleRef hWnd, string text, string caption, int type);
        private static DialogResult GetResult(int val)
        {
            return val switch
            {
                1 => DialogResult.OK,
                2 => DialogResult.Cancel,
                3 => DialogResult.Abort,
                4 => DialogResult.Retry,
                5 => DialogResult.Ignore,
                6 => DialogResult.Yes,
                7 => DialogResult.No,
                _ => DialogResult.No,
            };
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

        /// <summary>
        /// Creates, updates or deletes the taskbar icon.
        /// </summary>
        [DllImport("shell32.Dll", CharSet = CharSet.Unicode)]
        private static extern bool NotifyIcon(NotifyCommand cmd, [In] ref NotifyCommand data);
        /// <summary>
        /// Creates the helper window that receives messages from the taskar icon.
        /// </summary>
        [DllImport("User32.dll", EntryPoint = "CreateWindowExW", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(int dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, int dwStyle, int x, int y,
            int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance,
            IntPtr lpParam);
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWind, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWind);


        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hwind);

        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        private static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("user32")]
        private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AbortSystemShutdown(string lpMachineName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int CancelShutdown();

        [DllImport("user32")]
        private static extern void LockWorkStation();

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr findwindow(string lpclassname, string lpwindowname);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndinsertafter, int x, int y, int cx, int cy, uint uflags);

        [DllImport("user32.dll")]
        private static extern long GetWindowRect(int hWnd, ref Rectangle lpRect);

        [DllImport("user32.dll", EntryPoint = "MonitorFromWindow")]
        private static extern IntPtr MonitorFromWindow([In] IntPtr hwnd, uint dwFlags);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, ref PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorBrightness(IntPtr handle, ref uint minimumBrightness, ref uint currentBrightness, ref uint maxBrightness);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetMonitorBrightness(IntPtr handle, uint newBrightness);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(int hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(int hWnd, StringBuilder title, int size);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    }
}
