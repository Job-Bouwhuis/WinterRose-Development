namespace WinterRose.ImGuiUI
{
    using ImGuiNET;
    using System;
    using Win32;

    internal class ImGuiInputHandler
    {
        readonly IntPtr hwnd;
        ImGuiMouseCursor lastCursor;

        public ImGuiInputHandler(IntPtr hwnd)
        {
            this.hwnd = hwnd;
        }

        public bool Update()
        {
            var io = ImGui.GetIO();
            UpdateMousePosition(io, hwnd);
            var mouseCursor = io.MouseDrawCursor ? ImGuiMouseCursor.None : ImGui.GetMouseCursor();
            if (mouseCursor != lastCursor)
            {
                lastCursor = mouseCursor;

                // only required if mouse icon changes
                // while mouse isn't moved otherwise redundent.
                // so practically it's redundent.
                UpdateMouseCursor(io, mouseCursor);
            }

            if (!io.WantCaptureMouse && ImGui.IsAnyMouseDown())
            {
                // workaround: where overlay gets stuck in a non-clickable mode forever.
                for (var i = 0; i < 5; i++)
                {
                    io.AddMouseButtonEvent(i, false);
                }
            }

            return io.WantCaptureMouse;
        }

        public bool ProcessMessage(WindowMessage msg, UIntPtr wParam, IntPtr lParam)
        {
            if (ImGui.GetCurrentContext() == IntPtr.Zero)
                return false;

            var io = ImGui.GetIO();
            switch (msg)
            {
                case WindowMessage.SetFocus:
                case WindowMessage.KillFocus:
                    io.AddFocusEvent(msg == WindowMessage.SetFocus);
                    break;
                case WindowMessage.LButtonDown:
                case WindowMessage.LButtonDoubleClick:
                case WindowMessage.LButtonUp:
                    io.AddMouseButtonEvent(0, msg != WindowMessage.LButtonUp);
                    break;
                case WindowMessage.RButtonDown:
                case WindowMessage.RButtonDoubleClick:
                case WindowMessage.RButtonUp:
                    io.AddMouseButtonEvent(1, msg != WindowMessage.RButtonUp);
                    break;
                case WindowMessage.MButtonDown:
                case WindowMessage.MButtonDoubleClick:
                case WindowMessage.MButtonUp:
                    io.AddMouseButtonEvent(2, msg != WindowMessage.MButtonUp);
                    break;
                case WindowMessage.XButtonDown:
                case WindowMessage.XButtonDoubleClick:
                case WindowMessage.XButtonUp:
                    io.AddMouseButtonEvent(
                        GET_XBUTTON_WPARAM(wParam) == 1 ? 3 : 4,
                        msg != WindowMessage.XButtonUp);
                    break;
                case WindowMessage.MouseWheel:
                    io.AddMouseWheelEvent(0.0f, GET_WHEEL_DELTA_WPARAM(wParam) / WHEEL_DELTA);
                    break;
                case WindowMessage.MouseHWheel:
                    io.AddMouseWheelEvent(-GET_WHEEL_DELTA_WPARAM(wParam) / WHEEL_DELTA, 0.0f);
                    break;
                case WindowMessage.KeyDown:
                case WindowMessage.SysKeyDown:
                case WindowMessage.KeyUp:
                case WindowMessage.SysKeyUp:
                    bool is_key_down = msg == WindowMessage.SysKeyDown || msg == WindowMessage.KeyDown;
                    if ((ulong)wParam < 256 && TryMapKey((WinKey)wParam, out ImGuiKey imguikey))
                    {
                        if (imguikey == ImGuiKey.PrintScreen && !is_key_down)
                        {
                            io.AddKeyEvent(imguikey, true);
                        }

                        io.AddKeyEvent(imguikey, is_key_down);
                    }

                    break;
                case WindowMessage.Char:
                    io.AddInputCharacterUTF16((ushort)wParam);
                    break;
                case WindowMessage.SetCursor:
                    if (Utils.Loword((int)(long)lParam) == 1)
                    {
                        var mouseCursor = io.MouseDrawCursor ? ImGuiMouseCursor.None : ImGui.GetMouseCursor();
                        lastCursor = mouseCursor;
                        if (UpdateMouseCursor(io, mouseCursor))
                        {
                            return true;
                        }
                    }

                    break;
            }

            return false;
        }

        private static void UpdateMousePosition(ImGuiIOPtr io, IntPtr handleWindow)
        {
            if (User32.GetCursorPos(out POINT pos) && User32.ScreenToClient(handleWindow, ref pos))
            {
                io.AddMousePosEvent(pos.X, pos.Y);
            }
        }

        private static bool UpdateMouseCursor(ImGuiIOPtr io, ImGuiMouseCursor requestedcursor)
        {
            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
                return false;

            if (requestedcursor == ImGuiMouseCursor.None)
            {
                User32.SetCursor(IntPtr.Zero);
            }
            else
            {
                var cursor = SystemCursor.IDC_ARROW;
                switch (requestedcursor)
                {
                    case ImGuiMouseCursor.Arrow: cursor = SystemCursor.IDC_ARROW; break;
                    case ImGuiMouseCursor.TextInput: cursor = SystemCursor.IDC_IBEAM; break;
                    case ImGuiMouseCursor.ResizeAll: cursor = SystemCursor.IDC_SIZEALL; break;
                    case ImGuiMouseCursor.ResizeEW: cursor = SystemCursor.IDC_SIZEWE; break;
                    case ImGuiMouseCursor.ResizeNS: cursor = SystemCursor.IDC_SIZENS; break;
                    case ImGuiMouseCursor.ResizeNESW: cursor = SystemCursor.IDC_SIZENESW; break;
                    case ImGuiMouseCursor.ResizeNWSE: cursor = SystemCursor.IDC_SIZENWSE; break;
                    case ImGuiMouseCursor.Hand: cursor = SystemCursor.IDC_HAND; break;
                    case ImGuiMouseCursor.NotAllowed: cursor = SystemCursor.IDC_NO; break;
                }

                User32.SetCursor(User32.LoadCursor(IntPtr.Zero, cursor));
            }

            return true;
        }

        private static bool TryMapKey(WinKey key, out ImGuiKey result)
        {
            static ImGuiKey KeyToImGuiKeyShortcut(WinKey keyToConvert, WinKey startKey1, ImGuiKey startKey2)
            {
                var changeFromStart1 = (int)keyToConvert - (int)startKey1;
                return startKey2 + changeFromStart1;
            }

            result = key switch
            {
                >= WinKey.F1 and <= WinKey.F24 => KeyToImGuiKeyShortcut(key, WinKey.F1, ImGuiKey.F1),
                >= WinKey.NUMPAD0 and <= WinKey.NUMPAD9 => KeyToImGuiKeyShortcut(key, WinKey.NUMPAD0, ImGuiKey.Keypad0),
                >= WinKey.KEY_A and <= WinKey.KEY_Z => KeyToImGuiKeyShortcut(key, WinKey.KEY_A, ImGuiKey.A),
                >= WinKey.KEY_0 and <= WinKey.KEY_9 => KeyToImGuiKeyShortcut(key, WinKey.KEY_0, ImGuiKey._0),
                WinKey.TAB => ImGuiKey.Tab,
                WinKey.LEFT => ImGuiKey.LeftArrow,
                WinKey.RIGHT => ImGuiKey.RightArrow,
                WinKey.UP => ImGuiKey.UpArrow,
                WinKey.DOWN => ImGuiKey.DownArrow,
                WinKey.PRIOR => ImGuiKey.PageUp,
                WinKey.NEXT => ImGuiKey.PageDown,
                WinKey.HOME => ImGuiKey.Home,
                WinKey.END => ImGuiKey.End,
                WinKey.INSERT => ImGuiKey.Insert,
                WinKey.DELETE => ImGuiKey.Delete,
                WinKey.BACK => ImGuiKey.Backspace,
                WinKey.SPACE => ImGuiKey.Space,
                WinKey.RETURN => ImGuiKey.Enter,
                WinKey.ESCAPE => ImGuiKey.Escape,
                WinKey.OEM_7 => ImGuiKey.Apostrophe,
                WinKey.OEM_COMMA => ImGuiKey.Comma,
                WinKey.OEM_MINUS => ImGuiKey.Minus,
                WinKey.OEM_PERIOD => ImGuiKey.Period,
                WinKey.OEM_2 => ImGuiKey.Slash,
                WinKey.OEM_1 => ImGuiKey.Semicolon,
                WinKey.OEM_PLUS => ImGuiKey.Equal,
                WinKey.OEM_4 => ImGuiKey.LeftBracket,
                WinKey.OEM_5 => ImGuiKey.Backslash,
                WinKey.OEM_6 => ImGuiKey.RightBracket,
                WinKey.OEM_3 => ImGuiKey.GraveAccent,
                WinKey.CAPITAL => ImGuiKey.CapsLock,
                WinKey.SCROLL => ImGuiKey.ScrollLock,
                WinKey.NUMLOCK => ImGuiKey.NumLock,
                WinKey.SNAPSHOT => ImGuiKey.PrintScreen,
                WinKey.PAUSE => ImGuiKey.Pause,
                WinKey.DECIMAL => ImGuiKey.KeypadDecimal,
                WinKey.DIVIDE => ImGuiKey.KeypadDivide,
                WinKey.MULTIPLY => ImGuiKey.KeypadMultiply,
                WinKey.SUBTRACT => ImGuiKey.KeypadSubtract,
                WinKey.ADD => ImGuiKey.KeypadAdd,
                WinKey.SHIFT => ImGuiKey.ModShift,
                WinKey.CONTROL => ImGuiKey.ModCtrl,
                WinKey.MENU => ImGuiKey.ModAlt,
                WinKey.LSHIFT => ImGuiKey.LeftShift,
                WinKey.LCONTROL => ImGuiKey.LeftCtrl,
                WinKey.LMENU => ImGuiKey.LeftAlt,
                WinKey.LWIN => ImGuiKey.LeftSuper,
                WinKey.RSHIFT => ImGuiKey.RightShift,
                WinKey.RCONTROL => ImGuiKey.RightCtrl,
                WinKey.RMENU => ImGuiKey.RightAlt,
                WinKey.RWIN => ImGuiKey.RightSuper,
                WinKey.APPS => ImGuiKey.Menu,
                WinKey.BROWSER_BACK => ImGuiKey.AppBack,
                WinKey.BROWSER_FORWARD => ImGuiKey.AppForward,
                _ => ImGuiKey.None
            };

            return result != ImGuiKey.None;
        }

        private static readonly float WHEEL_DELTA = 120;

        private static int GET_WHEEL_DELTA_WPARAM(UIntPtr wParam) => Utils.Hiword((int)wParam);

        private static int GET_XBUTTON_WPARAM(UIntPtr wParam) => Utils.Hiword((int)wParam);
    }
}
