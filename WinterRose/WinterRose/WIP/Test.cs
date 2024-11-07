using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose;

namespace ChatThroughWinterRoseBot
{
    public class WindowOptions
    {
        public bool HasCloseButton { get; set; } = true;
        public bool HasMinimizeButton { get; set; } = true;
        public bool HasMaximizeButton { get; set; } = false; // This will prevent the window from being fullscreen

        // Other options can be added as needed
    }

    public interface IComponent
    {
        void Render(Graphics graphics);
    }

    public class NativeWindow
    {
        // Win32 API function declarations
        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        private static extern IntPtr RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
            int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        private struct MSG
        {
            public IntPtr hWnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point pt;
        }

        // Window properties
        private const int WS_OVERLAPPEDWINDOW = 0x00000000;
        private const int CS_HREDRAW = 0x0002;
        private const int CS_VREDRAW = 0x0001;
        private const int SW_SHOWNORMAL = 1;

        private IntPtr hWnd;

        // Delegate for the window procedure
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // Instance of the window procedure delegate
        private readonly WndProcDelegate wndProcDelegate;

        private List<IComponent> components = new List<IComponent>();

        public void AddComponent(IComponent component)
        {
            components.Add(component);
        }

        // Constructor
        public NativeWindow(int width, int height, string title, WindowOptions options)
        {
            Module mod = Assembly.GetEntryAssembly().DefinedTypes.First().Module;

            // Register window class
            WNDCLASSEX wc = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
                style = CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate = new WndProcDelegate(WndProc)),
                hInstance = Marshal.GetHINSTANCE(mod),
                hCursor = LoadCursor(IntPtr.Zero, 32512), // IDC_ARROW
                hbrBackground = IntPtr.Zero,
                lpszClassName = "NativeWindowClass"
            };

            RegisterClassEx(ref wc);

            // Create window
            uint dwStyle = WS_OVERLAPPEDWINDOW;
            //if (!options.HasCloseButton)
            //    dwStyle &= ~(0x80000U); // WS_SYSMENU
            //if (!options.HasMinimizeButton)
            //    dwStyle &= ~(0x20000U); // WS_MINIMIZEBOX
            //if (!options.HasMaximizeButton)
            //    dwStyle &= ~(0x10000U); // WS_MAXIMIZEBOX

            hWnd = CreateWindowEx(
                0,
                wc.lpszClassName,
                title,
                dwStyle,
                0, 0,
                width, height,
                IntPtr.Zero,
                IntPtr.Zero,
                wc.hInstance,
                IntPtr.Zero
            );

            // Show and update window
            ShowWindow(hWnd, SW_SHOWNORMAL);
            UpdateWindow(hWnd);
        }

        public void Run()
        {
            Windows.OpenConsole();
            MSG msg;
            bool running = true;
            while (running)
            {

                // Process messages
                while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 1 /* PM_REMOVE */))
                {
                    if (msg.message == 0x0012) // WM_QUIT
                    {
                        running = false; // Exit the loop if WM_QUIT message is received
                    }
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);

                    Console.WriteLine($"Received message: {msg.message}, wParam: {msg.wParam}, lParam: {msg.lParam}");
                }

                // Render all components
                using (Graphics graphics = Graphics.FromHwnd(hWnd))
                {
                    foreach (var component in components)
                    {
                        component.Render(graphics);
                    }
                }

                // Optionally add a delay or handle events
                // Thread.Sleep(16); // 60 FPS
            }
            Windows.CloseConsole();
        }

        // Main window procedure
        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case 0x0002: // WM_DESTROY
                    DestroyWindow(hWnd);
                    Environment.Exit(0);
                    return IntPtr.Zero;
            }
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    public class SquareComponent : IComponent
    {
        private int x, y, size;

        public SquareComponent(int x, int y, int size)
        {
            this.x = x;
            this.y = y;
            this.size = size;
        }

        public void Render(Graphics graphics)
        {
            using (Brush brush = new SolidBrush(Color.Black))
            {
                graphics.FillRectangle(brush, x, y, size, size);
            }
        }
    }

}
