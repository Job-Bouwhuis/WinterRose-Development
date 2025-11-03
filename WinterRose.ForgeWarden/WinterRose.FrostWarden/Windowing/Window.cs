using Raylib_cs;
using System.Runtime.InteropServices;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden.Windowing
{
    public class Window
    {
        static Log log = new Log("Window");

        public int Width => ray.GetScreenWidth();
        public int Height => ray.GetScreenHeight();

        public unsafe nint Handle => (nint)ray.GetWindowHandle();

        public Vectors.Vector2I Position => (Vectors.Vector2I)ray.GetWindowPosition();

        public string Title
        {
            get
            {
                return title;
            }

            private set
            {
                title = value;
                ray.SetWindowTitle(Title);
            }
        }

        public bool IsReady => ray.WindowShouldClose() == false;
        public bool IsFullscreen => ray.IsWindowFullscreen();
        public Vector2 Size => new(Width, Height);

        public ConfigFlags ConfigFlags { get; private set; }

        private string title;

        public Window(string title, ConfigFlags configFlags = 0)
        {
            Title = title;
            this.ConfigFlags = configFlags;
        }

        public void Create(int width, int height)
        {
            Raylib.SetConfigFlags(ConfigFlags);
            if (ConfigFlags.HasFlag(Raylib_cs.ConfigFlags.TransparentWindow))
            {
                width++;
                height++;
            }
            Raylib.InitWindow(width, height, Title);

            if (ConfigFlags.HasFlag(Raylib_cs.ConfigFlags.TransparentWindow))
                ray.SetWindowPosition(-1, -1);


            if (ConfigFlags.HasFlag(Raylib_cs.ConfigFlags.TransparentWindow))
            {
                MakeNonActivating(Handle);
                UnfocusCurrentWindow(Handle);
                HideFromTaskbarAndAltTab(Handle);

                // enable layered style so DWM/compositor knows window can have per-pixel alpha
                int style = GetWindowLong(Handle, GWL_EXSTYLE);
                style |= WS_EX_LAYERED;
                SetWindowLong(Handle, GWL_EXSTYLE, style);

                // don't force global alpha (LWA_ALPHA) — that will override per-pixel alpha.
                // Instead, attempt DWM fallbacks and inspect backbuffer alpha bits.
                TryDwmTransparencyFallbacks(Handle);

                // Inspect the pixel format alpha bits so we know what the driver provided:
                int alphaBits = GetBackbufferAlphaBits(Handle);
                log.Debug($"Alpha bits in PFD: {alphaBits}");
            }
        }

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_APPWINDOW = 0x00040000;
        const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        const int WS_EX_LAYERED = 0x00080000;
        const int LWA_COLORKEY = 0x00000001;
        const int LWA_ALPHA = 0x00000002;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private const uint GW_HWNDNEXT = 2;

        private static void MakeNonActivating(IntPtr hwnd)
        {
            int style = GetWindowLong(hwnd, GWL_EXSTYLE);
            style |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            SetWindowLong(hwnd, GWL_EXSTYLE, style);
        }

        public static void UnfocusCurrentWindow(IntPtr currentHandle)
        {
            IntPtr next = GetWindow(currentHandle, GW_HWNDNEXT);
            if (next != IntPtr.Zero)
            {
                SetForegroundWindow(next);
            }
        }

        public static void HideFromTaskbarAndAltTab(IntPtr hWnd)
        {
            int style = GetWindowLong(hWnd, GWL_EXSTYLE);

            // Remove WS_EX_APPWINDOW if present, add WS_EX_TOOLWINDOW
            style &= ~WS_EX_APPWINDOW;
            style |= WS_EX_TOOLWINDOW;

            SetWindowLong(hWnd, GWL_EXSTYLE, style);
        }


        public void Close()
        {
            Raylib.CloseWindow();
        }

        public void ToggleFullscreen()
        {
            ray.ToggleFullscreen();
        }

        public void SetSize(int newWidth, int newHeight)
        {
            ray.SetWindowSize(Width, Height);
        }

        public void Center()
        {
            var monitor = ray.GetCurrentMonitor();
            var monitorWidth = ray.GetMonitorWidth(monitor);
            var monitorHeight = ray.GetMonitorHeight(monitor);
            ray.SetWindowPosition((monitorWidth - Width) / 2, (monitorHeight - Height) / 2);
        }

        public void RequestRecreate(ConfigFlags newFlags)
        {
            var pos = ray.GetWindowPosition();

            int width = Width, height = Height;

            Close();
            ConfigFlags = newFlags;
            Create(width, height);

            // Optionally reposition and restore any GL state
            ray.SetWindowPosition(pos.X.FloorToInt(), pos.Y.FloorToInt());
        }

        internal bool ShouldClose()
        {
            if (ray.WindowShouldClose())
            {
                return true;
            }
            return false;
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMargins);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND pBlurBehind);

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public uint dwFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fEnable;
            public IntPtr hRgnBlur;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fTransitionOnMaximized;
        }

        private const uint DWM_BB_ENABLE = 0x00000001;

        // GDI / pixel format helpers
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern int GetPixelFormat(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int DescribePixelFormat(IntPtr hdc, int iPixelFormat, uint nBytes, ref PIXELFORMATDESCRIPTOR ppfd);

        [StructLayout(LayoutKind.Sequential)]
        private struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize;
            public ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits;
            public byte cRedShift;
            public byte cGreenBits;
            public byte cGreenShift;
            public byte cBlueBits;
            public byte cBlueShift;
            public byte cAlphaBits;
            public byte cAlphaShift;
            public byte cAccumBits;
            public byte cAccumRedBits;
            public byte cAccumGreenBits;
            public byte cAccumBlueBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public sbyte iLayerType;
            public byte bReserved;
            public uint dwLayerMask;
            public uint dwVisibleMask;
            public uint dwDamageMask;
        }

        // --- New helper: inspect alpha bits in the pixel format ---
        private static int GetBackbufferAlphaBits(IntPtr hwnd)
        {
            IntPtr hdc = GetDC(hwnd);
            if (hdc == IntPtr.Zero)
                return -1;

            int pf = GetPixelFormat(hdc);
            PIXELFORMATDESCRIPTOR pfd = new PIXELFORMATDESCRIPTOR();
            pfd.nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>();
            pfd.nVersion = 1;

            int res = DescribePixelFormat(hdc, pf, (uint)pfd.nSize, ref pfd);
            ReleaseDC(hwnd, hdc);

            return pfd.cAlphaBits;
        }

        // --- New helper: try DWM fallbacks (extend frame, optionally blur) ---
        private static void TryDwmTransparencyFallbacks(IntPtr hwnd)
        {
            // Extend frame into client area (-1 for full client glass)
            try
            {
                var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
            catch { /* ignore - best-effort */ }

            // Try toggling blur-behind off then on (attempt to nudge DWM composition)
            try
            {
                var bbOff = new DWM_BLURBEHIND { dwFlags = DWM_BB_ENABLE, fEnable = true, hRgnBlur = IntPtr.Zero, fTransitionOnMaximized = false };
                DwmEnableBlurBehindWindow(hwnd, ref bbOff);

                // If needed, enable blur (set fEnable = true). Leave disabled by default.
                // var bbOn = new DWM_BLURBEHIND { dwFlags = DWM_BB_ENABLE, fEnable = true, hRgnBlur = IntPtr.Zero, fTransitionOnMaximized = false };
                // DwmEnableBlurBehindWindow(hwnd, ref bbOn);
            }
            catch { /* ignore */ }
        }
    }
}
