using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// Provides enums for the Windows API. commonly used in the <see cref="Windows"/> class.
    /// </summary>
    public static partial class Windows
    {
        public enum WallpaperStyle : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public enum PowerMode
        {
            Balanced,
            HighPerformance,
            PowerSaver
        }

        public enum BatteryFlag : byte
        {
            High = 1,
            Low = 2,
            Critical = 4,
            Charging = 8,
            NoBattery = 128,
            Unknown = 255
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemPowerInfo
        {
            public ACLineStatus ACLineStatus;
            public BatteryFlag BatteryFlag;
            public byte BatteryLifePercent;
            public byte Reserved1;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;

            public void SetPowerStatus(PowerMode mode) => SetPowerMode(mode);
        }

        public enum ACLineStatus : byte
        {
            Offline = 0,
            Online = 1,
            Unknown = 255
        }

        public enum WindowStyles : uint
        {
            All = 0x17cf0000,

            WS_BORDER = 0x800000,
            WS_CAPTION = 0xc00000,
            WS_CHILD = 0x40000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_DISABLED = 0x8000000,
            WS_DLGFRAME = 0x400000,
            WS_GROUP = 0x20000,
            WS_HSCROLL = 0x100000,
            WS_ICONIC = 0x20000000,
            WS_MAXIMIZE = 0x1000000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x20000,
            WS_OVERLAPPED = 0x0,
            WS_OVERLAPPEDWINDOW = 0xcf0000,
            WS_POPUP = 0x80000000,
            WS_POPUPWINDOW = 0x80880000,
            WS_SIZEBOX = 0x40000,
            WS_SYSMENU = 0x80000,
            WS_TABSTOP = 0x10000,
            WS_THICKFRAME = 0x40000,
            WS_TILED = 0x0,
            WS_TILEDWINDOW = 0xcf0000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000
        }

        public enum ConsoleCloseCtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        /// <summary>
        /// Main operations performed on the
        /// <see cref="NotifyIcon"/> function.
        /// </summary>
        public enum NotifyCommand
        {
            /// <summary>
            /// The taskbar icon is being created.
            /// </summary>
            Add = 0x00,

            /// <summary>
            /// The settings of the taskbar icon are being updated.
            /// </summary>
            Modify = 0x01,

            /// <summary>
            /// The taskbar icon is deleted.
            /// </summary>
            Delete = 0x02,

            /// <summary>
            /// Focus is returned to the taskbar icon. Currently not in use.
            /// </summary>
            SetFocus = 0x03,

            /// <summary>
            /// Shell32.dll version 5.0 and later only. Instructs the taskbar
            /// to behave according to the version number specified in the 
            /// uVersion member of the structure pointed to by lpdata.
            /// This message allows you to specify whether you want the version
            /// 5.0 behavior found on Microsoft Windows 2000 systems, or the
            /// behavior found on earlier Shell versions. The default value for
            /// uVersion is zero, indicating that the original Windows 95 notify
            /// icon behavior should be used.
            /// </summary>
            SetVersion = 0x04
        }

        /// <summary>
        /// Flags that define the icon that is shown on a balloon
        /// tooltip.
        /// </summary>
        public enum BalloonFlags
        {
            /// <summary>
            /// No icon is displayed.
            /// </summary>
            None = 0x00,

            /// <summary>
            /// An information icon is displayed.
            /// </summary>
            Info = 0x01,

            /// <summary>
            /// A warning icon is displayed.
            /// </summary>
            Warning = 0x02,

            /// <summary>
            /// An error icon is displayed.
            /// </summary>
            Error = 0x03,

            /// <summary>
            /// Windows XP Service Pack 2 (SP2) and later.
            /// Use a custom icon as the title icon.
            /// </summary>
            User = 0x04,

            /// <summary>
            /// Windows XP (Shell32.dll version 6.0) and later.
            /// Do not play the associated sound. Applies only to balloon ToolTips.
            /// </summary>
            NoSound = 0x10,

            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later. The large version
            /// of the icon should be used as the balloon icon. This corresponds to the
            /// icon with dimensions SM_CXICON x SM_CYICON. If this flag is not set,
            /// the icon with dimensions XM_CXSMICON x SM_CYSMICON is used.<br/>
            /// - This flag can be used with all stock icons.<br/>
            /// - Applications that use older customized icons (NIIF_USER with hIcon) must
            ///   provide a new SM_CXICON x SM_CYICON version in the tray icon (hIcon). These
            ///   icons are scaled down when they are displayed in the System Tray or
            ///   System Control Area (SCA).<br/>
            /// - New customized icons (NIIF_USER with hBalloonIcon) must supply an
            ///   SM_CXICON x SM_CYICON version in the supplied icon (hBalloonIcon).
            /// </summary>
            LargeIcon = 0x20,

            /// <summary>
            /// Windows 7 and later.
            /// </summary>
            RespectQuietTime = 0x80
        }

        /// <summary>
        /// A result from a message box.
        /// </summary>
        public enum DialogResult
        {
            OK = 1,
            Cancel = 2,
            Abort = 3,
            Retry = 4,
            Ignore = 5,
            Yes = 6,
            No = 7
        }

        /// <summary>
        /// The buttons that are displayed on a message box.
        /// </summary>
        public enum MessageBoxButtons
        {
            OK = 0,
            OKCancel = 1,
            AbortRetryIgnore = 2,
            YesNoCancel = 3,
            YesNo = 4,
            RetryCancel = 5
        }

        /// <summary>
        /// An icon that is displayed on a message box.
        /// </summary>
        public enum MessageBoxIcon
        {
            None = 0,
            Error = 16,
            Question = 32,
            Exclamation = 48,
            Information = 64
        }
    }
}
