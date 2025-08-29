using Raylib_cs;
using System.Runtime.InteropServices;

namespace WinterRose.ForgeWarden.Windowing
{
    public class Window
    {
        public int Width => ray.GetScreenWidth();
        public int Height => ray.GetScreenHeight();

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
            Raylib.InitWindow(width, height, Title);

            if(ConfigFlags.HasFlag(Raylib_cs.ConfigFlags.TransparentWindow))
            {
                MakeNonActivating(Windows.MyHandle.Handle);
                UnfocusCurrentWindow(Windows.MyHandle.Handle);
                HideFromTaskbarAndAltTab(Windows.MyHandle.Handle);
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
    }
}
