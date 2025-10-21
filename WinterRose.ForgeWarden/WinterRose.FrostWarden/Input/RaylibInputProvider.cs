using Raylib_cs;
using System.Runtime.InteropServices;

namespace WinterRose.ForgeWarden.Input;

/// <summary>
/// The default input provider based on RayLib. 
/// </summary>
public class RaylibInputProvider : IInputProvider
{
    private static class OSMouseInput
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct X11Point
        {
            public int X;
            public int Y;
            public int screen;
        }

        [DllImport("libX11.so.6")]
        static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        static extern IntPtr XDefaultRootWindow(IntPtr display);

        [DllImport("libX11.so.6")]
        static extern int XQueryPointer(IntPtr display, IntPtr window, out IntPtr rootReturn,
                                        out IntPtr childReturn, out int rootX, out int rootY,
                                        out int winX, out int winY, out uint maskReturn);

        [DllImport("libX11.so.6")]
        static extern int XCloseDisplay(IntPtr display);

        private static float lastFrame = -1;
        private static Vector2 cachedMouse = new Vector2(-1, -1);

        public static Vector2 GetMouseWithinWindow()
        {
            float currentFrame = ray.GetFrameTime();
            if (lastFrame == currentFrame)
                return cachedMouse;

            Vector2 mouse = new Vector2(-1, -1);

            if (OperatingSystem.IsWindows())
            {
                if (GetCursorPos(out POINT p))
                {
                    int winX = Application.Current.Window.Position.X;
                    int winY = Application.Current.Window.Position.Y;
                    int winW = Application.Current.Window.Width;
                    int winH = Application.Current.Window.Height;

                    if (p.X >= winX && p.X <= winX + winW && p.Y >= winY && p.Y <= winY + winH)
                        mouse = new Vector2(p.X - winX, p.Y - winY);
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                IntPtr display = XOpenDisplay(IntPtr.Zero);
                if (display != IntPtr.Zero)
                {
                    IntPtr root = XDefaultRootWindow(display);

                    if (root != IntPtr.Zero)
                    {
                        XQueryPointer(display, root, out _, out _, out int rootX, out int rootY,
                            out _, out _, out _);

                        int winPosX = Application.Current.Window.Position.X;
                        int winPosY = Application.Current.Window.Position.Y;
                        int winW = Application.Current.Window.Width;
                        int winH = Application.Current.Window.Height;

                        if (rootX >= winPosX && rootX <= winPosX + winW &&
                            rootY >= winPosY && rootY <= winPosY + winH)
                        {
                            mouse = new Vector2(rootX - winPosX, rootY - winPosY);
                        }
                    }

                    XCloseDisplay(display);
                }
            }

            lastFrame = currentFrame;
            cachedMouse = mouse;
            return cachedMouse;
        }
    }

    private readonly int gamepadIndex;

    /// <inheritdoc cref="IInputProvider.MousePosition"/>
    public Vector2 MousePosition => currentMousePosition;
    /// <inheritdoc cref="IInputProvider.MouseDelta"/>
    public Vector2 MouseDelta => currentMousePosition - lastMousePosition;

    public RaylibInputProvider(int gamepadIndex = 0)
    {
        this.gamepadIndex = gamepadIndex;
    }

    private Vector2 lastMousePosition;
    private Vector2 currentMousePosition;

    public void Update()
    {
        lastMousePosition = currentMousePosition;
        currentMousePosition = OSMouseInput.GetMouseWithinWindow();

        int winWidth = Application.Current.Window.Width;
        int winHeight = Application.Current.Window.Height;

        bool mouseOutside = currentMousePosition.X < 0 || currentMousePosition.Y < 0 ||
                            currentMousePosition.X > winWidth || currentMousePosition.Y > winHeight;

        if (mouseOutside /*|| !Raylib.IsWindowFocused()*/)
            currentMousePosition = new(-1, -1);
    }

    /// <inheritdoc cref="IInputProvider.IsPressed(InputBinding)(InputBinding)"/>
    public bool IsPressed(InputBinding binding)
    {
        switch (binding.DeviceType)
        {
            case InputDeviceType.Keyboard:
                return Raylib.IsKeyPressed((KeyboardKey)binding.Code);

            case InputDeviceType.Mouse:
                return Raylib.IsMouseButtonPressed((MouseButton)binding.Code);

            case InputDeviceType.MouseWheel:
                float wheel = Raylib.GetMouseWheelMove();
                return binding.Relation == InputAxisRelation.Positive
                    ? wheel > 0.01f
                    : wheel < -0.01f;

            case InputDeviceType.GamepadButton:
                return Raylib.IsGamepadButtonPressed(gamepadIndex, (GamepadButton)binding.Code);

            case InputDeviceType.GamepadAxis:
                float axisPress = Raylib.GetGamepadAxisMovement(gamepadIndex, (GamepadAxis)binding.Code);
                return binding.Relation == InputAxisRelation.Positive
                    ? axisPress > binding.Threshold
                    : axisPress < -binding.Threshold;

            default:
                return false;
        }
    }

    /// <inheritdoc cref="IInputProvider.IsDown(InputBinding)"/>
    public bool IsDown(InputBinding binding)
    {
        bool result;

        switch (binding.DeviceType)
        {
            case InputDeviceType.Keyboard:
                result = Raylib.IsKeyDown((KeyboardKey)binding.Code);
                break;

            case InputDeviceType.Mouse:
                result = Raylib.IsMouseButtonDown((MouseButton)binding.Code);
                break;

            case InputDeviceType.GamepadButton:
                result = Raylib.IsGamepadButtonDown(gamepadIndex, (GamepadButton)binding.Code);
                break;

            case InputDeviceType.GamepadAxis:
                float axis = Raylib.GetGamepadAxisMovement(gamepadIndex, (GamepadAxis)binding.Code);
                result = binding.Relation == InputAxisRelation.Positive
                    ? axis > binding.Threshold
                    : axis < -binding.Threshold;
                break;

            default:
                result = false;
                break;
        }

        return result;
    }

    /// <inheritdoc cref="IInputProvider.IsUp(InputBinding)(InputBinding)"/>
    public bool IsUp(InputBinding binding)
        => binding.DeviceType switch
        {
            InputDeviceType.Keyboard => (bool)Raylib.IsKeyReleased((KeyboardKey)binding.Code),
            InputDeviceType.Mouse => (bool)Raylib.IsMouseButtonReleased((MouseButton)binding.Code),
            InputDeviceType.GamepadButton => (bool)Raylib.IsGamepadButtonReleased(gamepadIndex, (GamepadButton)binding.Code),
            _ => false,
        };

    /// <inheritdoc cref="IInputProvider.GetValue(InputBinding)(InputBinding)"/>
    public float GetValue(InputBinding binding)
    {
        switch (binding.DeviceType)
        {
            case InputDeviceType.Keyboard:
                return Raylib.IsKeyDown((KeyboardKey)binding.Code) ? 1f : 0f;

            case InputDeviceType.Mouse:
                return Raylib.IsMouseButtonDown((MouseButton)binding.Code) ? 1f : 0f;

            case InputDeviceType.MouseWheel:
                float wheel = Raylib.GetMouseWheelMove();
                return binding.Relation == InputAxisRelation.Positive
                    ? Math.Max(0, wheel)
                    : Math.Max(0, -wheel);

            case InputDeviceType.GamepadButton:
                return Raylib.IsGamepadButtonDown(gamepadIndex, (GamepadButton)binding.Code) ? 1f : 0f;

            case InputDeviceType.GamepadAxis:
                float axis = Raylib.GetGamepadAxisMovement(gamepadIndex, (GamepadAxis)binding.Code);
                return binding.Relation == InputAxisRelation.Positive
                    ? Math.Max(0, axis)
                    : Math.Max(0, -axis);

            default:
                return 0f;
        }
    }
}

