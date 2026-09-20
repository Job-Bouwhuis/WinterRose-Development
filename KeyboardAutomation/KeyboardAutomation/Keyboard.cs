using System.Runtime.InteropServices;

namespace KeyboardAutomation;

public static class Keyboard
{
    private const int INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    
    private const ushort VK_SHIFT = 0x10;
    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_MENU = 0x12;
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(
        uint cInputs,
        INPUT[] pInputs,
        int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public INPUTUNION U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern short VkKeyScan(char ch);

    public static void PressKey(char c)
    {
        short result = VkKeyScan(c);

        if (result == -1)
            throw new ArgumentException($"Cannot type '{c}' using current keyboard layout.");

        ushort virtualKey = (ushort)(result & 0xFF);
        byte modifiers = (byte)(result >> 8);

        PressKey(
            virtualKey,
            (modifiers & 1) != 0,
            (modifiers & 2) != 0,
            (modifiers & 4) != 0);
    }
    
    public static void PressKey(
        ushort virtualKey,
        bool shift = false,
        bool control = false,
        bool alt = false)
    {
        List<INPUT> inputs = [];

        void AddInput(ushort key, uint flags)
        {
            inputs.Add(new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = key,
                        dwFlags = flags
                    }
                }
            });
        }

        if (control)
            AddInput(VK_CONTROL, 0);

        if (alt)
            AddInput(VK_MENU, 0);

        if (shift)
            AddInput(VK_SHIFT, 0);

        AddInput(virtualKey, 0);
        AddInput(virtualKey, KEYEVENTF_KEYUP);

        if (shift)
            AddInput(VK_SHIFT, KEYEVENTF_KEYUP);

        if (alt)
            AddInput(VK_MENU, KEYEVENTF_KEYUP);

        if (control)
            AddInput(VK_CONTROL, KEYEVENTF_KEYUP);

        SendInput(
            (uint)inputs.Count,
            inputs.ToArray(),
            Marshal.SizeOf<INPUT>());
    }
}