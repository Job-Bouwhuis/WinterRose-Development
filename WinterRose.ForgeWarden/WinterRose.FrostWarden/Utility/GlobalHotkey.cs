using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WinterRose.ForgeWarden;

public static class GlobalHotkey
{
    private static readonly List<HotkeyBinding> bindings = new();
    private static readonly IKeyStateBackend backend = CreateBackend();

    public static void RegisterHotkey(string name, bool triggerOnce, params int[] keys)
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

    public static void Update() // call once per frame
    {
        for (int i = 0; i < bindings.Count; i++)
            bindings[i].Update();
    }

    // =========================
    // Internal helper types
    // =========================

    private class HotkeyBinding
    {
        public string Name { get; }
        private readonly bool triggerOnce;
        private readonly int[] keys;
        private bool wasPressedLastFrame;
        private bool triggeredThisFrame;

        public HotkeyBinding(string name, bool triggerOnce, int[] keys)
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
        bool IsKeyDown(int keyCode);
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

        public bool IsKeyDown(int keyCode)
        {
            return (GetAsyncKeyState(keyCode) & 0x8000) != 0;
        }
    }

    private class LinuxKeyBackend : IKeyStateBackend
    {
        [DllImport("libX11.so.6")]
        private static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        private static extern int XQueryKeymap(IntPtr display, byte[] keys);

        private readonly IntPtr display;

        public LinuxKeyBackend()
        {
            display = XOpenDisplay(IntPtr.Zero);
            if (display == IntPtr.Zero)
                throw new InvalidOperationException("Cannot open X display");
        }

        public bool IsKeyDown(int keyCode)
        {
            byte[] keys = new byte[32];
            XQueryKeymap(display, keys);

            int byteIndex = keyCode / 8;
            int bitIndex = keyCode % 8;

            return (keys[byteIndex] & (1 << bitIndex)) != 0;
        }
    }
}