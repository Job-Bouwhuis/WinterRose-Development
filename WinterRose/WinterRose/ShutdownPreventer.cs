using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using WinterRose.ForgeSignal;

namespace WinterRose;

public static class ShutdownPreventer
{
    // constants
    private const int WM_QUERYENDSESSION = 0x0011;
    private const int WM_ENDSESSION = 0x0016;
    private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

    // exit flags
    private const uint EWX_LOGOFF = 0x00000000;
    private const uint EWX_SHUTDOWN = 0x00000001;
    private const uint EWX_REBOOT = 0x00000002;
    private const uint EWX_FORCE = 0x00000004;
    private const uint EWX_POWEROFF = 0x00000008;

    // token privilege constants
    private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
    private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
    private const uint TOKEN_ADJUST_PRIVILEGES = 0x00000020;
    private const uint TOKEN_QUERY = 0x00000008;

    private const int WM_USER = 0x0400;
    private const int WM_CLEANUP = WM_USER + 1;
    private const int WM_QUIT = 0x0012;

    private const int WM_SET_REASON = WM_USER + 2;

    private const int SW_SHOWNOACTIVATE = 4;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const int OFFSCREEN_X = -32000;
    private const int OFFSCREEN_Y = -32000;
    private static readonly IntPtr HWND_TOP = IntPtr.Zero;

    private static Thread messageLoopThread = null;
    private static ManualResetEventSlim readyEvent = new ManualResetEventSlim(false);
    private static Exception windowCreateException = null;
    private const string MESSAGE_CLASS_NAME = "ShutdownPreventer_MessageWindow_Class";

    // internal state
    private static IntPtr windowHandle = IntPtr.Zero;
    private static bool isLocked = false;
    private static string lockReason = null;
    private static IntPtr classAtom = IntPtr.Zero;
    private static WndProcDelegate wndProcDelegate; // keep alive so GC doesn't collect


    // Public API
    public static bool IsLocked => isLocked;

    public static MulticastVoidInvocation OnSystemShutdown { get; private set; }

    public static void LockShutdown(string reason, IntPtr hwnd = default)
    {
        if (!OperatingSystem.IsWindows())
            return;

        lockReason = reason ?? "Application is preventing shutdown.";

        if (isLocked)
        {
            // update the reason if already locked
            if (hwnd != IntPtr.Zero || windowHandle != IntPtr.Zero)
                PostMessage(hwnd != IntPtr.Zero ? hwnd : windowHandle, WM_SET_REASON, IntPtr.Zero, IntPtr.Zero);
            return;
        }

        if (hwnd != IntPtr.Zero)
        {
            // use external window provided
            windowHandle = hwnd;
            TryRegisterShutdownReason(); // immediately register reason
        }
        else
        {
            // fallback: ensure internal message window exists
            EnsureMessageWindow();
            PostMessage(windowHandle, WM_SET_REASON, IntPtr.Zero, IntPtr.Zero);
        }

        isLocked = true;
    }

    public static void UnlockShutdown()
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (!isLocked) return;

        isLocked = false;

        if (windowHandle != IntPtr.Zero)
        {
            if (messageLoopThread != null && messageLoopThread.IsAlive)
            {
                // internal window: post cleanup
                PostMessage(windowHandle, WM_CLEANUP, IntPtr.Zero, IntPtr.Zero);
                if (!messageLoopThread.Join(2000))
                {
                    // leave running in background if join fails
                }
            }
            else
            {
                // external window: just unregister reason
                TryUnregisterShutdownReason();
            }
        }

        // clear local state
        messageLoopThread = null;
        windowCreateException = null;
        readyEvent.Reset();
        lockReason = null;
        windowHandle = IntPtr.Zero; // clear handle if using external one, safe because unregister done
    }

    public static void Shutdown(bool force = false)
    {
        EnableShutdownPrivilege();
        uint flags = EWX_SHUTDOWN | (force ? EWX_FORCE : 0);
        if (!ExitWindowsEx(flags, 0))
            ThrowLastWin32();
    }

    public static void Reboot(bool force = false)
    {
        EnableShutdownPrivilege();
        uint flags = EWX_REBOOT | (force ? EWX_FORCE : 0);
        if (!ExitWindowsEx(flags, 0))
            ThrowLastWin32();
    }

    public static void Logoff(bool force = false)
    {
        uint flags = EWX_LOGOFF | (force ? EWX_FORCE : 0);
        if (!ExitWindowsEx(flags, 0))
            ThrowLastWin32();
    }

    public static void Hibernate()
    {
        // Note: SetSuspendState may require system capabilities and proper power configuration.
        if (!SetSuspendState(true, false, false))
            ThrowLastWin32();
    }

    public static void Sleep()
    {
        if (!SetSuspendState(false, false, false))
            ThrowLastWin32();
    }

    public static void LockPC()
    {
        if (!LockWorkStation())
            ThrowLastWin32();
    }

    // --- Internal helpers ---

    private static void MessageLoopThreadProc()
    {
        try
        {
            // root delegate on this thread
            wndProcDelegate = WndProc;

            WNDCLASSEX wcex = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<WNDCLASSEX>(),
                style = 0,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate),
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = GetModuleHandle(IntPtr.Zero),
                hIcon = IntPtr.Zero,
                hCursor = IntPtr.Zero,
                hbrBackground = IntPtr.Zero,
                lpszMenuName = null,
                lpszClassName = MESSAGE_CLASS_NAME,
                hIconSm = IntPtr.Zero
            };

            classAtom = RegisterClassEx(ref wcex);

            // Create a normal top-level window (tiny). We'll move it offscreen and show without activating.
            windowHandle = CreateWindowEx(
                0,
                MESSAGE_CLASS_NAME,
                "ShutdownPreventerWindow",
                0,            // no visible style flags here; we'll ShowWindow after positioning
                0, 0, 1, 1,
                IntPtr.Zero,  // top-level window (not HWND_MESSAGE)
                IntPtr.Zero,
                wcex.hInstance,
                IntPtr.Zero);

            if (windowHandle == IntPtr.Zero)
            {
                int err = Marshal.GetLastWin32Error();
                windowCreateException = new Win32Exception(err);
                readyEvent.Set();
                return;
            }

            // move the window offscreen and show it without activating (so it's considered interactive but doesn't steal focus)
            SetWindowPos(windowHandle, HWND_TOP, OFFSCREEN_X, OFFSCREEN_Y, 1, 1, SWP_NOZORDER | SWP_NOACTIVATE);
            ShowWindow(windowHandle, SW_SHOWNOACTIVATE);

            // signal ready to caller
            readyEvent.Set();

            // normal message loop
            MSG msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
        catch (Exception ex)
        {
            windowCreateException = ex;
            readyEvent.Set();
        }
        finally
        {
            // cleanup on this thread (safe: same thread that created window/class)
            try { TryUnregisterShutdownReason(); } catch { }
            if (windowHandle != IntPtr.Zero)
            {
                DestroyWindow(windowHandle);
                windowHandle = IntPtr.Zero;
            }

            try { UnregisterClass(MESSAGE_CLASS_NAME, GetModuleHandle(IntPtr.Zero)); } catch { }

            classAtom = IntPtr.Zero;
            wndProcDelegate = null;
        }
    }

    private static void EnsureMessageWindow()
    {
        if (windowHandle != IntPtr.Zero) return;

        // start message loop thread if not running
        if (messageLoopThread == null || !messageLoopThread.IsAlive)
        {
            readyEvent.Reset();
            windowCreateException = null;
            messageLoopThread = new Thread(MessageLoopThreadProc)
            {
                IsBackground = true,
                Name = "ShutdownPreventer_MessageLoop"
            };
            messageLoopThread.Start();

            // wait for the window creation (or failure) on the message thread
            readyEvent.Wait();

            // propagate any create-time exception to caller
            if (windowHandle == IntPtr.Zero && windowCreateException != null)
                throw windowCreateException;
        }

        wndProcDelegate = WndProc; // keep delegate rooted
        string className = "ShutdownPreventer_MessageWindow_Class";

        WNDCLASSEX wcex = new WNDCLASSEX
        {
            cbSize = Marshal.SizeOf<WNDCLASSEX>(),
            style = 0,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = GetModuleHandle(IntPtr.Zero),
            hIcon = IntPtr.Zero,
            hCursor = IntPtr.Zero,
            hbrBackground = IntPtr.Zero,
            lpszMenuName = null,
            lpszClassName = className,
            hIconSm = IntPtr.Zero
        };

        classAtom = RegisterClassEx(ref wcex);
        if (classAtom == IntPtr.Zero)
        {
            int err = Marshal.GetLastWin32Error();
            // if class already registered by same name, RegisterClassEx may fail; try CreateWindowEx anyway
        }

        windowHandle = CreateWindowEx(
            0,
            className,
            string.Empty,
            0,
            0, 0, 0, 0,
            HWND_MESSAGE,
            IntPtr.Zero,
            wcex.hInstance,
            IntPtr.Zero);

        if (windowHandle == IntPtr.Zero)
            ThrowLastWin32();
    }

    private static void DestroyMessageWindow()
    {
        if (windowHandle != IntPtr.Zero)
        {
            DestroyWindow(windowHandle);
            windowHandle = IntPtr.Zero;
        }

        if (classAtom != IntPtr.Zero)
        {
            // Unregister class if it was registered by us
            UnregisterClass(Marshal.PtrToStringUni(classAtom), GetModuleHandle(IntPtr.Zero));
            classAtom = IntPtr.Zero;
        }

        // allow delegate to be collected
        wndProcDelegate = null;
    }

    private static void TryRegisterShutdownReason()
    {
        if (windowHandle == IntPtr.Zero || string.IsNullOrEmpty(lockReason)) return;

        if (!ShutdownBlockReasonCreate(windowHandle, lockReason))
            ThrowLastWin32();
    }

    private static void TryUnregisterShutdownReason()
    {
        if (windowHandle == IntPtr.Zero) return;
        // best-effort destroy; ignore failure
        ShutdownBlockReasonDestroy(windowHandle);
    }

    // Window procedure
    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_QUERYENDSESSION)
        {
            // Notify subscribers, but do NOT auto-unlock or veto
            OnSystemShutdown.Invoke();

            // Respect existing lock
            if (isLocked)
                return IntPtr.Zero; // veto shutdown if locked
            else
                return new IntPtr(1); // allow
        }

        if (msg == WM_ENDSESSION)
        {
            // Also notify here just in case the session is ending
            if (wParam != IntPtr.Zero)
                OnSystemShutdown.Invoke();
        }

        if (msg == WM_CLEANUP)
        {
            TryUnregisterShutdownReason();
            PostQuitMessage(0);
            return IntPtr.Zero;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    // Privilege enabling to allow ExitWindowsEx to work in some contexts
    private static void EnableShutdownPrivilege()
    {
        if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr tokenHandle))
            ThrowLastWin32();

        try
        {
            if (!LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, out LUID luid))
                ThrowLastWin32();

            TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Luid = luid,
                Attributes = SE_PRIVILEGE_ENABLED
            };

            if (!AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
                ThrowLastWin32();
        }
        finally
        {
            CloseHandle(tokenHandle);
        }
    }

    private static void ThrowLastWin32()
    {
        int err = Marshal.GetLastWin32Error();
        throw new Win32Exception(err);
    }

    // --- Native interop ---

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public int cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr RegisterClassEx([In] ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool ShutdownBlockReasonCreate(IntPtr hWnd, string pwszReason);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShutdownBlockReasonDestroy(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool LockWorkStation();

    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
        ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TOKEN_PRIVILEGES
    {
        public int PrivilegeCount;
        public LUID Luid;
        public uint Attributes;
    }
}
