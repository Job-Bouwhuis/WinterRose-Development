using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.Vectors;

namespace WinterRose;

/// <summary>
/// Provides methods for interacting with the Windows operating system.
/// </summary>
public static partial class Windows
{
    /// <summary>
    /// The handle of the current process.
    /// </summary> 
    public static WindowsHandle MyHandle
    {
        get
        {
            if(myHandle is not null)
                return myHandle;

            Process currentProcess = Process.GetCurrentProcess();
            if(currentProcess.MainWindowHandle != IntPtr.Zero)
                return myHandle = new WindowsHandle(currentProcess.ProcessName, currentProcess.MainWindowHandle);

            return myHandle = new WindowsHandle(currentProcess.ProcessName, GetConsoleWindow());
        }
    }
    private static WindowsHandle? myHandle;

    /// <summary>
    /// A list of all handles of all windows that are currently open on the system.
    /// </summary>
    public static List<WindowsHandle> Handles
    {
        get
        {
            List<string> allWindowHandleNames = GetAllWindowHandleNames();
            List<WindowsHandle> list = new List<WindowsHandle>();
            foreach (string item in allWindowHandleNames)
            {
                IntPtr windowHandle = GetWindowHandle(item);
                if (windowHandle != IntPtr.Zero)
                {
                    list.Add(new WindowsHandle(item, windowHandle));
                }
            }

            return list;
        }
    }

    /// <summary>
    /// Called when the application is about to exit. Once this event is called, the closing of the app can not be canceled
    /// </summary>
    public static event Action ApplicationExit = delegate { };

    static Windows()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => ApplicationExit();
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.IsTerminating)
                ApplicationExit();
        };
        AppDomain.CurrentDomain.DomainUnload += (sender, e) => ApplicationExit();
    }

    public static void AbortPCShutdown() => AbortSystemShutdown(Environment.MachineName);
    public static void CancelPCShutdown() => CancelShutdown();

    /// <summary>
    /// Generates a mouse click at the current position of the mouse.
    /// </summary>
    /// <param name="ms">The amount of miliseconds to hold the mouse down. (this will stop execution of the calling thread for this long aswell)</param>
    public static void MouseClick(int ms = 10)
    {
        mouse_event(2u, 0u, 0u, 0u, 0);
        Thread.Sleep(ms);
        mouse_event(4u, 0u, 0u, 0u, 0);
    }

    public static SystemPowerInfo GetSystemPowerInfo() => PowerStatus.GetPowerStatus();

    /// <summary>
    /// Gets the size of the screen at the given index. This index is the same as the index of the screen in the windows display settings.
    /// </summary>
    /// <returns></returns>
    public static Vector2I GetScreenSize() => ScreenSize.GetScreenSize();

    /// <summary>
    /// Gets the number of screens connected to the system.
    /// </summary>
    /// <returns></returns>
    public static int GetNumberOfScreens() => ScreenSize.GetNumberOfScreens();

    /// <summary>
    ///  Checks if the window of the given handle is minimized.
    /// </summary>
    /// <param name="hWind"></param>
    /// <returns>True if the window is minimized, otherwise false</returns>
    public static bool IsWindowMinimized(IntPtr hWind) => IsIconic(hWind);
    /// <summary>
    /// Raises the window of the given handle to the foreground. and gives it focus.
    /// </summary>
    /// <param name="hWind"></param>
    public static void RaiseWindowToForeground(IntPtr hWind) => SetForegroundWindow(hWind);
    /// <summary>
    /// If the window of the given handle is minimized or maximized it will be restored
    /// to its normal state.
    /// </summary>
    /// <param name="hWind"></param>
    public static void ShowWindow(IntPtr hWind) => ShowWindowAsync(hWind, 1);
    /// <summary>
    /// Minimizes the window of the given handle.
    /// </summary>
    /// <param name="hWind"></param>
    public static void MinimizeWindow(IntPtr hWind) => ShowWindowAsync(hWind, 2);
    /// <summary>
    ///  Maximizes the window of the given handle.
    /// </summary>
    /// <param name="hWind"></param>
    public static void MaximizeWindow(IntPtr hWind) => ShowWindowAsync(hWind, 3);

    /// <summary>
    /// Sets the brightness of the monitor the process is running on.<br></br><br></br>
    /// 
    /// This only works when the monitor supports brightness control through the windows API (such as laptops).
    /// </summary>
    /// <param name="newValue">The new brighness value. clamped: 0 - 100</param>
    [Experimental("WR_EXPERIMENTAL")]
    public static void SetBrightness(int newValue)
    {
        newValue = Math.Clamp(newValue, 0, 100);
        new BrightnessController(MyHandle.Handle).SetBrightness(newValue);
    }
    
    /// <summary>
    /// Sets the wallpaper of the desktop to the image at the given path.
    /// <br></br><br></br>
    /// <b>WARNING:</b> this action is irreversible. Make sure you have a backup of the current wallpaper.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="style"></param>
    public static void SetWallpaper(string path, WallpaperStyle style)
    {
        Wallpaper.Set(new(path), style);
    }

    /// <summary>
    /// Sets the power mode of the system.
    /// </summary>
    /// <param name="state"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void SetPowerMode(PowerMode state)
    {
        /*
        Balanced: 381b4222-f694-41f0-9685-ff5bb260df2e
        High performance: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
        Power saver: a1841308-3541-4fab-bc81-f71556f20b4a
         */

        string GUID = state switch

        {
            PowerMode.Balanced => "381b4222-f694-41f0-9685-ff5bb260df2e",
            PowerMode.HighPerformance => "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
            PowerMode.PowerSaver => "a1841308-3541-4fab-bc81-f71556f20b4a",
            _ => throw new ArgumentException("Invalid PowerMode", nameof(state)),
        };

        Process.Start("powercfg", $"-setactive  {GUID}");
    }

    /// <summary>
    /// Open a single file
    /// </summary>
    /// <param name="file">Path to the selected file, or null if the return value is false</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="filter">File name filter. Example: "txt files (*.txt)\0*.txt\0All files (*.*)\0*.*\0"</param>
    /// <param name="initialDirectory">Example : "c:\\"</param>
    /// <param name="showHidden">Forces the showing of system and hidden files</param>
    /// <returns>True of a file was selected, false if the dialog was cancelled or closed</returns>
    public static bool OpenFile(out string file, string title = "Open File", string filter = "All Files (*.*)\0*.*\0", string initialDirectory = "C:\\", bool showHidden = false)
    {
        return OpenFileDialog.OpenFile(out file, title, filter, initialDirectory, showHidden);
    }

    /// <summary>
    /// Save a single file
    /// </summary>
    /// <param name="selectedPath"></param>
    /// <param name="title"></param>
    /// <param name="filter">File name filter. Example: "txt files (*.txt)\0*.txt\0All files (*.*)\0*.*\0"</param>
    /// <param name="initialDirectory">Example : "c:\\"</param>
    /// <param name="defaultExtension">The default file extension the saved file will be given</param>
    /// <param name="showHidden">Forces the showing of system and hidden files</param>
    /// <returns>True if the user clicked "save", false if the user clicked "cancel" or closed the dialog using the X close button</returns>
    public static bool SaveFile(out string selectedPath, string title = "Save File", string filter = "All Files (*.*)\0*.*\0", string initialDirectory = "C:\\", string defaultExtension = null, bool showHidden = false)
    {
        return SaveFileDialog.SaveFile(out selectedPath, title, filter, initialDirectory, defaultExtension, showHidden);
    }

    /// <summary>
    /// Open multiple files
    /// </summary>
    /// <param name="files">Paths to the selected files, or null if the return value is false</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="filter">File name filter. Example : "txt files (*.txt)|*.txt|All files (*.*)|*.*"</param>
    /// <param name="initialDirectory">Example : "c:\\"</param>
    /// <param name="showHidden">Forces the showing of system and hidden files</param>
    /// <returns>True of one or more files were selected, false if the dialog was cancelled or closed</returns>
    public static bool OpenFiles(out string[] files, string title = null, string filter = null, string initialDirectory = null, bool showHidden = false)
    {
        return OpenFileDialog.OpenFiles(out files, title, filter, initialDirectory, showHidden);
    }

    /// <summary>
    /// Opens a folder dialog that returns the selected folder.
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static bool OpenFolder(out string folder, string title = "Open Folder")
    {
        var browser = OSFolderBrowser.Open();
        browser.Title = title;
        bool result = browser.Open();
        folder = browser.SelectedPath;
        return result;
    }

    // Helper method to convert string to IntPtr
    private static IntPtr StringToIntPtrUni(string str)
    {
        return Marshal.StringToHGlobalUni(str);
    }

    // Helper method to free IntPtr allocated by StringToIntPtrUni
    private static void FreeIntPtr(IntPtr ptr)
    {
        Marshal.FreeHGlobal(ptr);
    }

    /// <summary>
    /// Creates a new console window. if your app makes a managed window, then call this before you make the window.
    /// </summary>
    public static void OpenConsole(bool WriteInitializeLine = true)
    {
        AllocConsole();

        // Redirect standard input/output/error to custom streams
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });

        if (WriteInitializeLine)
            Console.WriteLine("Console Initialized");
    }
    /// <summary>
    /// Releases the console created by WinterRose.Windows.CreateConsole. <br></br><br></br>
    /// </summary>
    public static void CloseConsole()
    {
        // close the console
        FreeConsole();
    }
    /// <summary>
    ///  Shuts down the PC
    /// </summary>
    public static void PCShutdown()
    {
        Process.Start("shutdown", "/s /t 0");
    }
    /// <summary>
    ///  Locks the pc
    /// </summary>
    public static void PCLock()
    {
        LockWorkStation();
    }
    /// <summary>
    /// Restarts the pc
    /// </summary>
    public static void PCRestart()
    {
        Process.Start("shutdown", "/r /t 0");
    }
    /// <summary>
    /// Puts the PC into hibernation
    /// </summary>
    public static void PCHibernate()
    {
        SetSuspendState(hiberate: true, forceCritical: true, disableWakeEvent: true);
    }
    /// <summary>
    /// Puts the PC to sleep
    /// </summary>
    public static void PCSleep()
    {
        SetSuspendState(hiberate: false, forceCritical: true, disableWakeEvent: true);
    }
    /// <summary>
    /// Sets the position of the window of the given handle to the given position.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="position"></param>
    public static void SetWindowPosition(IntPtr handle, Vector2I position)
    {
        SetWindowPos(handle, IntPtr.Zero, position.X, position.Y, 0, 0, 5u);
    }
    /// <summary>
    /// Gets the position of the window of the given handle.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns> a Vector2 with the windows position coordinates</returns>
    public static Vector2 GetWindowPosition(IntPtr handle)
    {
        Rectangle lpRect = default(Rectangle);
        GetWindowRect((int)handle, ref lpRect);
        return new Vector2(lpRect.X, lpRect.Y);
    }
    /// <summary>
    /// Sets the position of the cursor to the given position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void SetMousePosition(int x, int y)
    {
        SetCursorPos(x, y);
    }
    /// <summary>
    /// Shows a message box with the given parameters
    /// </summary>
    /// <param name="text"></param>
    /// <param name="title"></param>
    /// <param name="buttons"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    public static DialogResult MessageBox(string text, string title = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
        return GetResult(MessageBox(new HandleRef(null, GetActiveWindow()), text, title, (int)buttons | (int)icon));
    }
    /// <summary>
    /// Creates a new window with the given parameters.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="styles"></param>
    /// <returns></returns>
    [Experimental("WR_EXPERIMENTAL")]
    public static WindowsHandle CreateWindow(string title, int width, int height, int x = 100, int y = 100, WindowStyles styles = WindowStyles.WS_OVERLAPPEDWINDOW)
    {
        nint handle = CreateWindowEx(0, "STATIC", title, (int)styles, x, y, width, height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        WindowsHandle windowsHandle = new WindowsHandle(Process.GetCurrentProcess().ProcessName, handle);
        return windowsHandle;
    }

    private static List<string> GetAllWindowHandleNames()
    {
        List<string> list = new List<string>();
        Process[] processes = Process.GetProcesses();
        foreach (Process process in processes)
        {
            process.Refresh();
            if (process.MainWindowHandle != IntPtr.Zero && !string.IsNullOrEmpty(process.MainWindowTitle))
            {
                list.Add(process.ProcessName);
            }
        }

        return list;
    }

    private static IntPtr GetWindowHandle(string name)
    {
        Process process = Process.GetProcessesByName(name).FirstOrDefault();
        if (process != null && process.MainWindowHandle != IntPtr.Zero)
        {
            return process.MainWindowHandle;
        }

        return IntPtr.Zero;
    }
}

