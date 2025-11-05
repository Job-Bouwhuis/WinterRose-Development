using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WinterRose
{
    /// <summary>
    /// When the app creates a window, you can use this class to hook onto the windows message loop.
    /// </summary>
    public static class WindowHooks
    {
        /// <summary>
        /// A class full of some common messages
        /// </summary>
        public static class Messages
        {
            // ─── Lifecycle ─────────────────────────────────────────────

            /// <summary>WM_CREATE: Sent when a window is being created.</summary>
            public const int Create = 0x0001;

            /// <summary>WM_DESTROY: Sent when a window is being destroyed.</summary>
            public const int Destroy = 0x0002;

            /// <summary>WM_MOVE: Sent after a window has been moved.</summary>
            public const int Move = 0x0003;

            /// <summary>WM_SIZE: Sent when a window is resized.</summary>
            public const int Size = 0x0005;

            /// <summary>WM_CLOSE: Sent when the user tries to close the window.</summary>
            public const int Close = 0x0010;

            /// <summary>WM_QUIT: Sent to a thread to request termination.</summary>
            public const int Quit = 0x0012;

            /// <summary>WM_SHOWWINDOW: Sent when window is shown or hidden.</summary>
            public const int ShowWindow = 0x0018;

            // ─── System Events ──────────────────────────────────────────

            /// <summary>WM_QUERYENDSESSION: Sent when the user attempts to log off or shutdown.</summary>
            public const int QueryEndSession = 0x0011;

            /// <summary>WM_ENDSESSION: Sent when the session is ending.</summary>
            public const int EndSession = 0x0016;

            /// <summary>WM_POWERBROADCAST: Sent to notify of power management events.</summary>
            public const int PowerBroadcast = 0x0218;

            // ─── Input: Mouse ───────────────────────────────────────────

            /// <summary>WM_MOUSEMOVE: Sent when the mouse is moved.</summary>
            public const int MouseMove = 0x0200;

            /// <summary>WM_LBUTTONDOWN: Left mouse button pressed.</summary>
            public const int LeftButtonDown = 0x0201;

            /// <summary>WM_LBUTTONUP: Left mouse button released.</summary>
            public const int LeftButtonUp = 0x0202;

            /// <summary>WM_RBUTTONDOWN: Right mouse button pressed.</summary>
            public const int RightButtonDown = 0x0204;

            /// <summary>WM_RBUTTONUP: Right mouse button released.</summary>
            public const int RightButtonUp = 0x0205;

            /// <summary>WM_MBUTTONDOWN: Middle mouse button pressed.</summary>
            public const int MiddleButtonDown = 0x0207;

            /// <summary>WM_MBUTTONUP: Middle mouse button released.</summary>
            public const int MiddleButtonUp = 0x0208;

            /// <summary>WM_MOUSEWHEEL: Mouse wheel moved.</summary>
            public const int MouseWheel = 0x020A;

            /// <summary>WM_MOUSELEAVE: Mouse has left the window.</summary>
            public const int MouseLeave = 0x02A3;

            // ─── Input: Keyboard ────────────────────────────────────────

            /// <summary>WM_KEYDOWN: A key is pressed.</summary>
            public const int KeyDown = 0x0100;

            /// <summary>WM_KEYUP: A key is released.</summary>
            public const int KeyUp = 0x0101;

            /// <summary>WM_CHAR: A character is input (includes keyboard layout and modifiers).</summary>
            public const int Char = 0x0102;

            /// <summary>WM_SYSKEYDOWN: A system key (ALT or F10) is pressed.</summary>
            public const int SysKeyDown = 0x0104;

            /// <summary>WM_SYSKEYUP: A system key is released.</summary>
            public const int SysKeyUp = 0x0105;

            // ─── Focus ──────────────────────────────────────────────────

            /// <summary>WM_SETFOCUS: Sent when window gains keyboard focus.</summary>
            public const int SetFocus = 0x0007;

            /// <summary>WM_KILLFOCUS: Sent when window loses keyboard focus.</summary>
            public const int KillFocus = 0x0008;

            /// <summary>WM_ACTIVATE: Sent when the window is activated or deactivated.</summary>
            public const int Activate = 0x0006;

            /// <summary>WM_ACTIVATEAPP: Sent when the app is activated or deactivated.</summary>
            public const int ActivateApp = 0x001C;

            // ─── Paint / Drawing ────────────────────────────────────────

            /// <summary>WM_PAINT: Sent when a portion of the window needs to be redrawn.</summary>
            public const int Paint = 0x000F;

            /// <summary>WM_ERASEBKGND: Sent when the background must be erased (before WM_PAINT).</summary>
            public const int EraseBackground = 0x0014;

            // ─── System Commands ────────────────────────────────────────

            /// <summary>WM_SYSCOMMAND: Sent when a system command is executed (minimize, close, etc).</summary>
            public const int SystemCommand = 0x0112;

            /// <summary>SC_CLOSE: Close command from system menu.</summary>
            public const int SystemClose = 0xF060;

            /// <summary>SC_MINIMIZE: Minimize command from system menu.</summary>
            public const int SystemMinimize = 0xF020;

            /// <summary>SC_MAXIMIZE: Maximize command from system menu.</summary>
            public const int SystemMaximize = 0xF030;

            /// <summary>SC_RESTORE: Restore command from system menu.</summary>
            public const int SystemRestore = 0xF120;

            // ─── DPI / Scaling ──────────────────────────────────────────

            /// <summary>WM_DPICHANGED: Notifies that the DPI setting changed.</summary>
            public const int DpiChanged = 0x02E0;

            // ─── Misc ───────────────────────────────────────────────────

            /// <summary>WM_SETCURSOR: Sent to set the cursor image.</summary>
            public const int SetCursor = 0x0020;

            /// <summary>WM_GETMINMAXINFO: Sent to retrieve min/max window size limits.</summary>
            public const int GetMinMaxInfo = 0x0024;

            /// <summary>WM_WINDOWPOSCHANGING: Sent before a window's size, position, or Z order is changed.</summary>
            public const int WindowPosChanging = 0x0046;

            /// <summary>WM_WINDOWPOSCHANGED: Sent after a window's size, position, or Z order is changed.</summary>
            public const int WindowPosChanged = 0x0047;

            // ─── Raw Input ──────────────────────────────────────────────

            /// <summary>WM_INPUT: Sent to retrieve raw input data.</summary>
            public const int RawInput = 0x00FF;

            // ─── IME ────────────────────────────────────────────────────

            /// <summary>WM_IME_SETCONTEXT: IME context is activated or deactivated.</summary>
            public const int ImeSetContext = 0x0281;

            /// <summary>WM_IME_STARTCOMPOSITION: Composition starts for IME input.</summary>
            public const int ImeStartComposition = 0x010D;

            /// <summary>WM_IME_ENDCOMPOSITION: Composition ends for IME input.</summary>
            public const int ImeEndComposition = 0x010E;

            /// <summary>WM_IME_COMPOSITION: Input composition ongoing.</summary>
            public const int ImeComposition = 0x010F;

            // ─── Custom ─────────────────────────────────────────────────

            /// <summary>WM_APP: Base value for application-defined messages.</summary>
            public const int App = 0x8000;

            /// <summary>WM_USER: Base value for user-defined messages (reserved by the system).</summary>
            public const int User = 0x0400;
        }

        private const int GWL_WNDPROC = -4;
        private const int WM_CLOSE = 0x0010;
        private const int WM_QUERYENDSESSION = 0x0011;

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private static WndProc? _customWndProc;
        private static IntPtr _oldWndProc = IntPtr.Zero;

        private static readonly Dictionary<uint, Action<WindowMessage>> messageHandlers = new();
        private static readonly List<Action<CancelableWindowMessageEventArgs>> cancelableCloseHandlers = new();

        public static event Action<WindowMessage>? OnAnyMessage;

        public static event EventHandler<CancelableWindowMessageEventArgs>? OnWindowCloseAttempt;

        static WindowHooks()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                throw new Exception("Failed to get window handle.");

            _customWndProc = CustomWndProc;
            _oldWndProc = GetWindowLongPtr(hWnd, GWL_WNDPROC);
            SetWindowLongPtr(hWnd, GWL_WNDPROC, _customWndProc);
        }

        public static void RegisterHandler(uint msg, Action<WindowMessage> handler)
        {
            if (messageHandlers.ContainsKey(msg))
                messageHandlers[msg] += handler;
            else
                messageHandlers[msg] = handler;
        }

        public static void RegisterCancelableCloseHandler(Action<CancelableWindowMessageEventArgs> handler)
        {
            cancelableCloseHandlers.Add(handler);
        }

        private static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            var message = new WindowMessage
            {
                HWnd = hWnd,
                Msg = (int)msg,
                WParam = wParam,
                LParam = lParam
            };

            OnAnyMessage?.Invoke(message);

            switch (msg)
            {
                case WM_CLOSE:
                case WM_QUERYENDSESSION:
                    {
                        var args = new CancelableWindowMessageEventArgs(msg, wParam, lParam);
                        OnWindowCloseAttempt?.Invoke(null, args);

                        foreach (var handler in cancelableCloseHandlers)
                            handler.Invoke(args);

                        if (args.Cancel)
                            return IntPtr.Zero;
                        break;
                    }
                default:
                    if (messageHandlers.TryGetValue(msg, out var handlerForMsg))
                    {
                        handlerForMsg.Invoke(message);
                    }
                    break;
            }

            

            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }

        // DllImports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WndProc newProc);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }

    public class WindowMessage
    {
        public nint HWnd { get; set; }
        public int Msg { get; set; }
        public nint WParam { get; set; }
        public nint LParam { get; set; }
    }

    public class CancelableWindowMessageEventArgs : EventArgs
    {
        public uint MessageId { get; }
        public IntPtr WParam { get; }
        public IntPtr LParam { get; }
        public bool Cancel { get; set; }

        public CancelableWindowMessageEventArgs(uint messageId, IntPtr wParam, IntPtr lParam)
        {
            MessageId = messageId;
            WParam = wParam;
            LParam = lParam;
            Cancel = false;
        }
    }

}
