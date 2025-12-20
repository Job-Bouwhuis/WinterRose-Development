using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;

namespace WinterRose
{
    public static partial class Windows
    {
        public class SystemTrayIcon
        {
            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

            private const uint NIM_ADD = 0x00000000;
            private const uint NIM_DELETE = 0x00000002;
            private const uint NIF_MESSAGE = 0x00000001;
            private const uint NIF_ICON = 0x00000002;
            private const uint NIF_TIP = 0x00000004;
            private const uint WM_USER = 0x0400;
            private const uint WM_TRAYICON = WM_USER + 1;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private struct NOTIFYICONDATA
            {
                public uint cbSize;
                public IntPtr hWnd;
                public uint uID;
                public uint uFlags;
                public uint uCallbackMessage;
                public IntPtr hIcon;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string szTip;
            }

            private NOTIFYICONDATA notifyIconData;

            public SystemTrayIcon(IntPtr windowHandle, uint iconId, string tooltip, string iconFilePath)
            {
                notifyIconData = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                    hWnd = windowHandle,
                    uID = iconId,
                    uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                    uCallbackMessage = WM_TRAYICON,
                    hIcon = LoadIconFromFile(iconFilePath),
                    szTip = tooltip
                };
            }

            private IntPtr LoadIconFromFile(string filePath)
            {
                if (File.Exists(filePath))
                {
                    Icon icon = new Icon(filePath);
                    return icon.Handle;
                }
                return IntPtr.Zero;
            }

            // ---------- new constants ----------
            private const uint WM_LBUTTONUP = 0x0202;
            private const uint WM_LBUTTONDBLCLK = 0x0203;
            private const uint WM_RBUTTONUP = 0x0205;
            private const int GWLP_WNDPROC = -4;

            // ---------- events ----------
            public MulticastVoidInvocation Click = new();
            public MulticastVoidInvocation RightClick = new();
            public MulticastVoidInvocation DoubleClick = new();

            /// <summary>
            /// for KDE
            /// </summary>
            private Process trayProcess;

            // ---------- fields for window proc hook ----------
            private IntPtr originalWndProc = IntPtr.Zero;
            private WindowProcDelegate? windowProcDelegate; // keep delegate alive so it doesn't get GC'd

            // ---------- delegate type ----------
            private delegate IntPtr WindowProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            // ---------- P/Invoke for hooking (32/64-bit safe) ----------
            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
            private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr newProc);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
            private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr newProc);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            // ---------- register/unregister helpers ----------
            private void RegisterWindowProcHook()
            {
                if (notifyIconData.hWnd == IntPtr.Zero)
                    return;

                // prevent double register
                if (originalWndProc != IntPtr.Zero)
                    return;

                windowProcDelegate = new WindowProcDelegate(WndProc);
                IntPtr newProcPtr = Marshal.GetFunctionPointerForDelegate(windowProcDelegate);

                if (IntPtr.Size == 8)
                {
                    originalWndProc = SetWindowLongPtr64(notifyIconData.hWnd, GWLP_WNDPROC, newProcPtr);
                }
                else
                {
                    originalWndProc = SetWindowLong32(notifyIconData.hWnd, GWLP_WNDPROC, newProcPtr);
                }
            }

            private void UnregisterWindowProcHook()
            {
                if (notifyIconData.hWnd == IntPtr.Zero || originalWndProc == IntPtr.Zero)
                    return;

                if (IntPtr.Size == 8)
                {
                    SetWindowLongPtr64(notifyIconData.hWnd, GWLP_WNDPROC, originalWndProc);
                }
                else
                {
                    SetWindowLong32(notifyIconData.hWnd, GWLP_WNDPROC, originalWndProc);
                }

                originalWndProc = IntPtr.Zero;
                windowProcDelegate = null;
            }

            // ---------- WndProc ----------
            private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                try
                {
                    if(msg == WM_TRAYICON)
                        if (notifyIconData.uID == (uint)wParam.ToInt32())
                        {
                            uint mouseMsg = (uint)lParam.ToInt32();
                            if (mouseMsg == WM_LBUTTONUP) Click.Invoke();
                            else if (mouseMsg == WM_RBUTTONUP) RightClick.Invoke();
                            else if (mouseMsg == WM_LBUTTONDBLCLK) DoubleClick.Invoke();
                        }
                }
                catch
                {
                    // swallow exceptions from handlers to avoid breaking window proc
                }

                // call original proc (or default if none)
                if (originalWndProc != IntPtr.Zero)
                    return CallWindowProc(originalWndProc, hWnd, msg, wParam, lParam);

                return DefWindowProc(hWnd, msg, wParam, lParam);
            }

            // ---------- small ShowInTray / DeleteIcon updates ----------
            // replace your existing ShowInTray and DeleteIcon bodies with these (or add Register/Unregister calls)
            public void ShowInTray()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    RegisterWindowProcHook();
                    Shell_NotifyIcon(NIM_ADD, ref notifyIconData);
                    return;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    ShowTrayIconKde();
                }
            }

            public void DeleteIcon()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Shell_NotifyIcon(NIM_DELETE, ref notifyIconData);
                    UnregisterWindowProcHook();
                    return;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    KillTrayProcessKde();
                }
            }

            private void ShowTrayIconKde()
            {
                if (trayProcess != null && !trayProcess.HasExited)
                    return;

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "yad",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                psi.ArgumentList.Add("--notification");
                psi.ArgumentList.Add($"--image={GetIconPath()}");
                psi.ArgumentList.Add($"--text={notifyIconData.szTip}");
                psi.ArgumentList.Add("--command=echo CLICK");

                trayProcess = Process.Start(psi);

                Thread listener = new Thread(ListenTrayEventsKde);
                listener.IsBackground = true;
                listener.Start();
            }

            private void ListenTrayEventsKde()
            {
                try
                {
                    while (!trayProcess.StandardOutput.EndOfStream)
                    {
                        string line = trayProcess.StandardOutput.ReadLine();
                        if (line == "CLICK")
                            Click.Invoke();
                    }
                }
                catch
                {
                }
            }

            private void KillTrayProcessKde()
            {
                try
                {
                    trayProcess?.Kill();
                    trayProcess = null;
                }
                catch
                {
                }
            }

            private string GetIconPath()
            {
                return notifyIconData.hIcon != IntPtr.Zero
                    ? "/tmp/trayicon.png"
                    : "";
            }

        }
    }
}