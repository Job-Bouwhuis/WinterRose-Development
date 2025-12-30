using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Raylib_cs;
using WinterRose.ForgeWarden.Input;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden;

public static class GlobalHotkey
{
    private static readonly List<HotkeyBinding> bindings = new();
    private static readonly IKeyStateBackend backend = CreateBackend();

    public static void RegisterHotkey(string name, bool triggerOnce, params HotkeyScancode[] keys)
    {
        bindings.Add(new HotkeyBinding(name, triggerOnce, keys));
    }

    public static bool IsTriggered(string name)
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            var b = bindings[i];
            if (b.Name == name && b.CheckTriggered())
                return true;
        }
        return false;
    }

    public static void Update() 
    {
        for (int i = 0; i < bindings.Count; i++)
            bindings[i].Update();
    }
    

    private class HotkeyBinding
    {
        public string Name { get; }
        private readonly bool triggerOnce;
        private readonly HotkeyScancode[] keys;
        private bool wasPressedLastFrame;
        private bool triggeredThisFrame;

        public HotkeyBinding(string name, bool triggerOnce, HotkeyScancode[] keys)
        {
            Name = name;
            this.triggerOnce = triggerOnce;
            this.keys = keys;
        }

        public void Update()
        {
            bool allPressed = true;

            for (int i = 0; i < keys.Length; i++)
            {
                if (!backend.IsKeyDown(keys[i]))
                {
                    allPressed = false;
                    break;
                }
            }

            if (triggerOnce)
            {
                triggeredThisFrame = allPressed && !wasPressedLastFrame;
                wasPressedLastFrame = allPressed;
            }
            else
            {
                triggeredThisFrame = allPressed;
            }
        }

        public bool CheckTriggered() => triggeredThisFrame;
    }

    private interface IKeyStateBackend
    {
        bool IsKeyDown(HotkeyScancode keyCode);
    }

    private static IKeyStateBackend CreateBackend()
    {
        if (OperatingSystem.IsWindows())
            return new WindowsKeyBackend();
        if (OperatingSystem.IsLinux())
            return new LinuxKeyBackend();

        throw new PlatformNotSupportedException("Unsupported platform for GlobalHotkey");
    }

    private class WindowsKeyBackend : IKeyStateBackend
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public bool IsKeyDown(HotkeyScancode keyCode)
        {
            return (GetAsyncKeyState((int)keyCode) & 0x8000) != 0;
        }
    }

    private class LinuxKeyBackend : IKeyStateBackend
    {
        public bool IsKeyDown(HotkeyScancode keyCode)
        {
            if (keyCode is HotkeyScancode.MouseLeft
                or HotkeyScancode.MouseMiddle
                or HotkeyScancode.MouseRight
                or HotkeyScancode.Mouse4
                or HotkeyScancode.Mouse5)
            {
                return ForgeWardenEngine.Current.GlobalInput.Provider.IsDown(new InputBinding(InputDeviceType.Keyboard, (int)(keyCode switch {
                    HotkeyScancode.MouseLeft => MouseButton.Left,
                    HotkeyScancode.MouseRight => MouseButton.Right,
                    HotkeyScancode.MouseMiddle => MouseButton.Middle,
                    HotkeyScancode.Mouse4 => MouseButton.Back,
                    HotkeyScancode.Mouse5 => MouseButton.Forward
                    })));
            }
            
            var key = KeyboardMapper.Map(keyCode);
            bool r = ForgeWardenEngine.Current.GlobalInput.Provider.IsDown(new InputBinding(InputDeviceType.Keyboard, (int)key));
            return r;
        }

        private static Log l = new("linux input tests");
    }
}

public static class KeyboardMapper
{
    private static readonly Dictionary<HotkeyScancode, KeyboardKey> scancodeToKey = new()
    {
        // Letters
        { HotkeyScancode.A, KeyboardKey.A },
        { HotkeyScancode.B, KeyboardKey.B },
        { HotkeyScancode.C, KeyboardKey.C },
        { HotkeyScancode.D, KeyboardKey.D },
        { HotkeyScancode.E, KeyboardKey.E },
        { HotkeyScancode.F, KeyboardKey.F },
        { HotkeyScancode.G, KeyboardKey.G },
        { HotkeyScancode.H, KeyboardKey.H },
        { HotkeyScancode.I, KeyboardKey.I },
        { HotkeyScancode.J, KeyboardKey.J },
        { HotkeyScancode.K, KeyboardKey.K },
        { HotkeyScancode.L, KeyboardKey.L },
        { HotkeyScancode.M, KeyboardKey.M },
        { HotkeyScancode.N, KeyboardKey.N },
        { HotkeyScancode.O, KeyboardKey.O },
        { HotkeyScancode.P, KeyboardKey.P },
        { HotkeyScancode.Q, KeyboardKey.Q },
        { HotkeyScancode.R, KeyboardKey.R },
        { HotkeyScancode.S, KeyboardKey.S },
        { HotkeyScancode.T, KeyboardKey.T },
        { HotkeyScancode.U, KeyboardKey.U },
        { HotkeyScancode.V, KeyboardKey.V },
        { HotkeyScancode.W, KeyboardKey.W },
        { HotkeyScancode.X, KeyboardKey.X },
        { HotkeyScancode.Y, KeyboardKey.Y },
        { HotkeyScancode.Z, KeyboardKey.Z },

        // Numbers (top row)
        { HotkeyScancode.D1, KeyboardKey.One },
        { HotkeyScancode.D2, KeyboardKey.Two },
        { HotkeyScancode.D3, KeyboardKey.Three },
        { HotkeyScancode.D4, KeyboardKey.Four },
        { HotkeyScancode.D5, KeyboardKey.Five },
        { HotkeyScancode.D6, KeyboardKey.Six },
        { HotkeyScancode.D7, KeyboardKey.Seven },
        { HotkeyScancode.D8, KeyboardKey.Eight },
        { HotkeyScancode.D9, KeyboardKey.Nine },
        { HotkeyScancode.D0, KeyboardKey.Zero },

        // Modifiers
        { HotkeyScancode.LeftShift, KeyboardKey.LeftShift },
        { HotkeyScancode.RightShift, KeyboardKey.RightShift },
        { HotkeyScancode.LeftCtrl, KeyboardKey.LeftControl },
        { HotkeyScancode.RightCtrl, KeyboardKey.RightControl },
        { HotkeyScancode.LeftAlt, KeyboardKey.LeftAlt },
        { HotkeyScancode.RightAlt, KeyboardKey.RightAlt },

        // Enter / Backspace / Space
        { HotkeyScancode.Enter, KeyboardKey.Enter },
        { HotkeyScancode.Backspace, KeyboardKey.Backspace },
        { HotkeyScancode.Space, KeyboardKey.Space },
        { HotkeyScancode.Tab, KeyboardKey.Tab },
        { HotkeyScancode.Escape, KeyboardKey.Escape },

        // Brackets / punctuation
        { HotkeyScancode.OemMinus, KeyboardKey.Minus },
        { HotkeyScancode.OemPlus, KeyboardKey.Equal },
        { HotkeyScancode.OemOpenBrackets, KeyboardKey.LeftBracket },
        { HotkeyScancode.OemCloseBrackets, KeyboardKey.RightBracket },
        { HotkeyScancode.OemBackslash, KeyboardKey.Backslash },
        { HotkeyScancode.OemSemicolon, KeyboardKey.Semicolon },
        { HotkeyScancode.OemQuotes, KeyboardKey.Apostrophe },
        { HotkeyScancode.OemComma, KeyboardKey.Comma },
        { HotkeyScancode.OemPeriod, KeyboardKey.Period },
        { HotkeyScancode.OemSlash, KeyboardKey.Slash },
        { HotkeyScancode.OemTilde, KeyboardKey.Grave },

        // Function keys
        { HotkeyScancode.F1, KeyboardKey.F1 },
        { HotkeyScancode.F2, KeyboardKey.F2 },
        { HotkeyScancode.F3, KeyboardKey.F3 },
        { HotkeyScancode.F4, KeyboardKey.F4 },
        { HotkeyScancode.F5, KeyboardKey.F5 },
        { HotkeyScancode.F6, KeyboardKey.F6 },
        { HotkeyScancode.F7, KeyboardKey.F7 },
        { HotkeyScancode.F8, KeyboardKey.F8 },
        { HotkeyScancode.F9, KeyboardKey.F9 },
        { HotkeyScancode.F10, KeyboardKey.F10 },
        { HotkeyScancode.F11, KeyboardKey.F11 },
        { HotkeyScancode.F12, KeyboardKey.F12 }
        // Add more as needed (arrows, numpad, etc.)
    };

    public static KeyboardKey Map(HotkeyScancode scancode)
    {
        return scancodeToKey.TryGetValue(scancode, out var key) ? key : KeyboardKey.Null;
    }
}
