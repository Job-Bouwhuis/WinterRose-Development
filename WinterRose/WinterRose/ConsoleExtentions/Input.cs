using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ConsoleExtentions
{
    /// <summary>
    /// Allows for easy key input gathering from console applications
    /// </summary>
    public static class Input
    { 
        /// <summary>
        /// gets the specified key. using intercept to determain if the pressed key should appear in the console.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="intercept"></param>
        /// <param name="wait"></param>
        /// <returns>returns true when the key is pressed. otherwise, if wait is set to false and the key is not being pressed, it returns false</returns>
        public static bool GetKey(ConsoleKey key, bool intercept = true, bool wait = true) => (wait) ? Console.ReadKey(intercept).Key == key : Console.KeyAvailable && Console.ReadKey(intercept).Key == key;
    }
    /// <summary>
    /// Extra stuff for console applications
    /// </summary>
    public static class ConsoleS
    {
        /// <summary>
        /// Writes a red text line with slight indent to the console
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Message"></param>
        public static void WriteErrorLine<T>(T Message)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\t{Message}");
            Console.ForegroundColor = color;
        }
        /// <summary>
        /// Writes the red text to the console with a tab before it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Message"></param>
        public static void WriteError<T>(T Message)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"\t{Message}");
            Console.ForegroundColor = color;
        }
        /// <summary>
        /// Writes a yellow text line with slight indent to the console
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Message"></param>
        public static void WriteWarningLine<T>(T Message)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\t{Message}");
            Console.ForegroundColor = color;
        }
        /// <summary>
        /// Writes the yellow text to the console with a tab before it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Message"></param>
        public static void WriteWarning<T>(T Message)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\t{Message}");
            Console.ForegroundColor = color;
        }
    }
    /// <summary>
    /// Allows for flashing of Console Windows
    /// </summary>
    public static class WindowFlasher
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure, in bytes
            /// </summary>
            public UInt32 cbSize;

            /// <summary>
            /// A handle to the window to be flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;

            /// <summary>
            /// The flash status. This parameter can be one or more of the following values.
            /// </summary>
            public UInt32 dwFlags;

            /// <summary>
            /// The number of times to flash the window.
            /// </summary>
            public UInt32 uCount;

            /// <summary>
            /// The rate at which the window is to be flashed, in milliseconds. If dwTimeout is zero, the function uses the default cursor blink rate.
            /// </summary>
            public UInt32 dwTimeout;
        }

        /// <summary>
        /// Flash both the window caption and taskbar button. This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        private const UInt32 FLASHW_ALL = 0x00000003;
        /// <summary>
        /// Stop flashing. The system restores the window to its original state.
        /// </summary>
        private const UInt32 FLASHW_STOP = 0x00000004;

        /// <summary>
        /// Create an instance of the FLASHWINFO structure
        /// </summary>
        /// <param name="flashwConstant">One of the provided FLASHW contant values</param>
        /// <param name="uCount">uCount to initialize the struct</param>
        /// <param name="dwTimeout">dwTimeout to initalize the struct</param>
        /// <returns>A fully instantiated FLASHWINFO struct</returns>
        private static FLASHWINFO GetFLASHWINFO(UInt32 flashwConstant, UInt32 uCount = UInt32.MaxValue, UInt32 dwTimeout = 0)
        {
            FLASHWINFO fInfo = new FLASHWINFO()
            {
                hwnd = GetConsoleWindow(),
                dwFlags = flashwConstant,
                uCount = uCount,
                dwTimeout = dwTimeout
            };
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            return fInfo;
        }

        /// <summary>
        /// Flashes the console window (continues indefinitely)
        /// </summary>
        public static void Flash(uint count = uint.MaxValue)
        {
            FLASHWINFO fInfo = GetFLASHWINFO(FLASHW_ALL, count);
            FlashWindowEx(ref fInfo);
        }

        /// <summary>
        /// Stops the flashing of the console window
        /// </summary>
        public static void StopFlash()
        {
            FLASHWINFO fInfo = GetFLASHWINFO(FLASHW_STOP);
            FlashWindowEx(ref fInfo);
        }
    }
}
