using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

            public void AddIcon()
            {
                Shell_NotifyIcon(NIM_ADD, ref notifyIconData);
            }

            public void RemoveIcon()
            {
                Shell_NotifyIcon(NIM_DELETE, ref notifyIconData);
            }
        }
    }
}