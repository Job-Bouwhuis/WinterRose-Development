using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WinterRose.Monogame.EditorMode;

namespace WinterRose.Monogame;

/// <summary>
/// Helper class for getting keyboard and mouse input from the user. place the <b>UpdateState()</b> method at the top of your update for this to be used.
/// </summary>
public static class Input
{
    private static KeyboardState current;
    private static KeyboardState previous;
    private static MouseState currentMouse;
    private static MouseState previousMouse;
    private static Vector2 inputVectorCurrent;
    public static float normalizedInputLerpingRisingSpeed = 0.12f;
    public static float normalizedInputLerpingFallingSpeed = 0.2f;
    private static float lastMouseScroll;
    private static float mouseScrollDelta;

    private static List<Keys> previousPressed = new List<Keys>();

    /// <summary>
    /// Whether the user is currently interacting with the UI. always returns false when <see cref="BlockInputIfUISelected"/> is false
    /// </summary>
    public static bool UISelected
    {
        get
        {
            if (!BlockInputIfUISelected)
                return false;
            return gui.IsWindowFocused(ImGuiFocusedFlags.AnyWindow);
        }
    }
    public static bool BlockInputIfUISelected { get; set; } = true;
    public static Vector2I MouseDelta => new(currentMouse.X - previousMouse.X, currentMouse.Y - previousMouse.Y);

    /// <summary>
    /// contains all the keys that are pressed. since the last time of an update call
    /// </summary>
    public static List<Keys> currentPressed = new List<Keys>();

    /// <summary>
    /// Updates the keyboard and mouse states so this class works as intended.
    /// </summary>
    public static void UpdateState()
    {
        try
        {
            previous = current;
            current = Keyboard.GetState();
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();
            currentPressed = current.GetPressedKeys().ToList();
            previousPressed = previous.GetPressedKeys().ToList();
            mouseScrollDelta = currentMouse.ScrollWheelValue - lastMouseScroll;
            lastMouseScroll = currentMouse.ScrollWheelValue;
        }
        catch (InvalidOperationException)
        {
            UpdateState(); // very very sometimes it fails for one try, this recusive retry  makes it
            // work after 1 or 2 retries. never had it happen fail more than twice in a row.
            // this has been extensively tested. so leave it. its save
        }
    }
    /// <summary>
    /// gets if the specified key is pressed on this frame
    /// </summary>
    /// <param name="key"></param>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns>returns if the specified key is being pressed this frame, and not the last. otherwise false</returns>
    public static bool GetKeyDown(Keys key, bool workInEditor = false)
    {
        if (UISelected && BlockInputIfUISelected)
            return false;
        if (!workInEditor && Editor.Opened)
            return false;

        return MonoUtils.IsActive && current.IsKeyDown(key) && previous.IsKeyUp(key);
    }

    /// <summary>
    /// gets whether the specified key is pressed
    /// </summary>
    /// <param name="key"></param>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns>returns true if the given key is being pressed</returns>
    public static bool GetKey(Keys key, bool workInEditor = false)
    {
        if (UISelected && BlockInputIfUISelected)
            return false;

        if (!workInEditor && Editor.Opened)
            return false;

        return MonoUtils.IsActive && current.IsKeyDown(key);
    }

    /// <summary>
    /// gets if the specified key is released on this frame
    /// </summary>
    /// <param name="key"></param>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns>returns true if the key is released on the current frame, otherwise false</returns>
    public static bool GetKeyUp(Keys key, bool workInEditor = false)
    {
        if (UISelected && UISelected)
            return false;

        if (!workInEditor && Editor.Opened)
            return false;

        return MonoUtils.IsActive && current.IsKeyUp(key) && previous.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if all the given keys are currently pressed
    /// </summary>
    /// <param name="keys"></param>
    /// <returns>true if all of the given keys are pressed. otherwise false</returns>
    public static bool GetKeys(params Keys[] keys)
    {
        if (!MonoUtils.IsActive || (UISelected && UISelected))
            return false;

        foreach (Keys key in keys)
            if (current.IsKeyDown(key))
                return false;
        return true;
    }
    /// <summary>
    /// Checks if all the given keys are pressed at the same time
    /// </summary>
    /// <param name="keys"></param>
    /// <returns>true if none of the given keys are pressed. otherwise false</returns>
    public static bool GetKeysUp(params Keys[] keys)
    {
        if (!MonoUtils.IsActive || (UISelected && UISelected))
            return false;
        foreach (Keys key in keys)
            if (current.IsKeyUp(key) && previous.IsKeyDown(key))
                return false;
        return true;
    }
    /// <summary>
    /// Gets if all the given keys are released at the same time
    /// </summary>
    /// <param name="keys"></param>
    /// <returns>true if all of the given keys are pressed in the current frame. otherwise false</returns>
    public static bool GetKeysDown(params Keys[] keys)
    {
        if (!MonoUtils.IsActive || UISelected)
            return false;

        foreach (Keys key in keys)
            if (current.IsKeyDown(key) && previous.IsKeyUp(key))
                return false;
        return true;
    }
    /// <summary>
    /// Gets all pressed keys and returns a tuple that contains all keys and a bool that specifies if any key is pressed
    /// </summary>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns></returns>
    public static (List<Keys> Pressedkeys, bool IsAnyKeyPressed) GetAnyKey(bool workInEditor = false)
    {
        if (!MonoUtils.IsActive || (UISelected && BlockInputIfUISelected) || (!workInEditor && Editor.Opened))
            return (new List<Keys>(), false);
        return (currentPressed, currentPressed.Count > 0);
    }
    /// <summary>
    /// Gets all pressed keys and returns a tuple that contains all keys that were pressed this frame and were not pressed last frame, and a bool that specifies if any key is pressed
    /// </summary>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns></returns>
    public static (List<Keys> Pressedkeys, bool IsAnyKeyPressed) GetAnyKeyDown(bool workInEditor = false)
    {
        if (!MonoUtils.IsActive || UISelected && BlockInputIfUISelected || (!workInEditor && Editor.Opened))
            return (new List<Keys>(), false);
        List<Keys> result = new List<Keys>();
        foreach (var key in currentPressed)
        {
            if (!previousPressed.Contains(key))
                result.Add(key);
        }
        return (result, result.Count > 0);
    }
    /// <summary>
    /// Gets if specified mouse button is being pressed this frame
    /// </summary>
    /// <param name="button"></param>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns></returns>
    public static bool GetMouseDown(MouseButton button, bool workInEditor = false)
    {
        if (!MonoUtils.IsActive || UISelected || (!workInEditor && Editor.Opened))
            return false;

        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) && BlockInputIfUISelected)
            return false;

        return button switch
        {
            MouseButton.Left => currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released,
            MouseButton.Right => currentMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton == ButtonState.Released,
            MouseButton.Middle => currentMouse.MiddleButton == ButtonState.Pressed && previousMouse.MiddleButton == ButtonState.Released,
            _ => false
        };
    }

    /// <summary>
    /// Gets if the specified mouse button is released this frame
    /// </summary>
    /// <param name="button"></param>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns>true if the specified key is released this frame, and pressed last frame, otherwise false</returns>
    public static bool GetMouseUp(MouseButton button, bool workInEditor = false)
    {
        if (!MonoUtils.IsActive || UISelected || (!workInEditor && Editor.Opened))
            return false;

        return button switch
        {
            MouseButton.Left => currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed,
            MouseButton.Right => currentMouse.RightButton == ButtonState.Released && previousMouse.RightButton == ButtonState.Pressed,
            MouseButton.Middle => currentMouse.MiddleButton == ButtonState.Released && previousMouse.MiddleButton == ButtonState.Pressed,
            _ => false
        };
    }
    /// <summary>
    /// Gets if the specified mouse button is pressed
    /// </summary>
    /// <param name="button"></param>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns></returns>
    public static bool GetMouse(MouseButton button, bool workInEditor = false)
    {
        if (!MonoUtils.IsActive)
            return false;

        if (UISelected || (workInEditor && Editor.Opened))
            return false;

        // check if any ImGui window is hovered
        if (BlockInputIfUISelected && ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow))
            return false;

        return button switch
        {
            MouseButton.Left => currentMouse.LeftButton == ButtonState.Pressed,
            MouseButton.Right => currentMouse.RightButton == ButtonState.Pressed,
            MouseButton.Middle => currentMouse.MiddleButton == ButtonState.Pressed,
            _ => false
        };
    }
    /// <summary>
    /// Gets or sets the current mouse position on the screen
    /// </summary>
    public static Vector2I MousePosition
    {
        get => new(currentMouse.X, currentMouse.Y);
        set => WinterRose.Windows.SetMousePosition(value.X, value.Y);
    }
    /// <summary>
    /// gets the current mouse scroll value since the game started
    /// </summary>
    public static float MouseScroll => currentMouse.ScrollWheelValue;

    /// <summary>
    /// Gets the change in the mouse scroll value since the last frame
    /// </summary>
    public static float MouseScrollDelta => mouseScrollDelta / 120;

    /// <summary>
    /// Gets a normalized input vector using <see cref="GetKey(Keys, bool)"/>
    /// </summary>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns>a normalized vector of the pressed input keys</returns>
    public static Vector2 NormalizedDirection(Keys up, Keys down, Keys left, Keys right, bool workInEditor = false)
    {
        Vector2 result = new(0, 0);
        if (GetKey(up, workInEditor))
            result.Y--;
        if (GetKey(down, workInEditor))
            result.Y++;
        if (GetKey(left, workInEditor))
            result.X--;
        if (GetKey(right, workInEditor))
            result.X++;

        if (result.X != 0 || result.Y != 0)
            result.Normalize();

        result = Vector2.Lerp(inputVectorCurrent, result, (result == Vector2.Zero ? normalizedInputLerpingFallingSpeed : normalizedInputLerpingRisingSpeed));
        inputVectorCurrent = result;

        return result;
    }

    /// <summary>
    /// Calls <see cref="NormalizedDirection(Keys, Keys, Keys, Keys, bool)"/> with the keys W, S, A, D
    /// </summary>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns></returns>
    public static Vector2 GetNormalizedWASDInput(bool workInEditor = false) => NormalizedDirection(Keys.W, Keys.S, Keys.A, Keys.D, workInEditor);

    /// <summary>
    /// Gets a packed enum of all the keys that are currently pressed
    /// </summary>
    /// <param name="workInEditor">Whether or not to check if the key is pressed when the editor is open</param>
    /// <returns></returns>
    public static Keys GetPressedKeys(bool workInEditor = false)
    {
        if (!MonoUtils.IsActive || UISelected || (!workInEditor && Editor.Opened))
            return Keys.None;

        Keys res = Keys.None;

        var keys = current.GetPressedKeys();
        foreach (var key in keys)
        {
            res |= key;
        }
        return res;
    }

    #region Key properties
    /// <summary>
    /// Whether the left mouse button is currently pressed
    /// </summary>
    public static bool MouseLeft => GetMouse(MouseButton.Left);
    /// <summary>
    /// Whether the right mouse button is currently pressed
    /// </summary>
    public static bool MouseRight => GetMouse(MouseButton.Right);
    /// <summary>
    /// Whether the middle mouse button is currently pressed
    /// </summary>
    public static bool MouseMiddle => GetMouse(MouseButton.Middle);
    /// <summary>
    /// Whether the left mouse button was pressed this frame
    /// </summary>
    public static bool MouseLeftPressed => GetMouseDown(MouseButton.Left);
    /// <summary>
    /// Whether the right mouse button was pressed this frame
    /// </summary>
    public static bool MouseRightPressed => GetMouseDown(MouseButton.Right);
    /// <summary>
    /// Whether the middle mouse button was pressed this frame
    /// </summary>
    public static bool MouseMiddlePressed => GetMouseDown(MouseButton.Middle);
    /// <summary>
    /// Whether the left mouse button was released this frame
    /// </summary>
    public static bool MouseLeftReleased => GetMouseUp(MouseButton.Left);
    /// <summary>
    /// Whether the right mouse button was released this frame
    /// </summary>
    public static bool MouseRightReleased => GetMouseUp(MouseButton.Right);
    /// <summary>
    /// Whether the middle mouse button was released this frame
    /// </summary>
    public static bool MouseMiddleReleased => GetMouseUp(MouseButton.Middle);
    /// <summary>
    /// Whether the back button on the mouse is currently pressed
    /// </summary>
    public static bool MouseBack => GetKey(Keys.BrowserBack);
    /// <summary>
    /// Whether the forward button on the mouse is currently pressed
    /// </summary>
    public static bool MouseForward => GetKey(Keys.BrowserForward);
    /// <summary>
    /// Whether the back button on the mouse was pressed this frame
    /// </summary>
    public static bool MouseBackPressed => GetKeyDown(Keys.BrowserBack);
    /// <summary>
    /// Whether the forward button on the mouse was pressed this frame
    /// </summary>
    public static bool MouseForwardPressed => GetKeyDown(Keys.BrowserForward);
    /// <summary>
    /// Whether the back button on the mouse was released this frame
    /// </summary>
    public static bool MouseBackReleased => GetKeyUp(Keys.BrowserBack);
    /// <summary>
    /// Whether the forward button on the mouse was released this frame
    /// </summary>
    public static bool MouseForwardReleased => GetKeyUp(Keys.BrowserForward);
    /// <summary>
    /// Whether the A key is currently pressed
    /// </summary>
    public static bool A => GetKey(Keys.A);
    /// <summary>
    /// Whether the B key is currently pressed
    /// </summary>
    public static bool B => GetKey(Keys.B);
    /// <summary>
    /// Whether the C key is currently pressed
    /// </summary>
    public static bool C => GetKey(Keys.C);
    /// <summary>
    /// Whether the D key is currently pressed
    /// </summary>
    public static bool D => GetKey(Keys.D);
    /// <summary>
    /// Whether the E key is currently pressed
    /// </summary>
    public static bool E => GetKey(Keys.E);
    /// <summary>
    /// Whether the F key is currently pressed
    /// </summary>
    public static bool F => GetKey(Keys.F);
    /// <summary>
    /// Whether the G key is currently pressed
    /// </summary>
    public static bool G => GetKey(Keys.G);
    /// <summary>
    /// Whether the H key is currently pressed
    /// </summary>
    public static bool H => GetKey(Keys.H);
    /// <summary>
    /// Whether the I key is currently pressed
    /// </summary>
    public static bool I => GetKey(Keys.I);
    /// <summary>
    /// Whether the J key is currently pressed
    /// </summary>
    public static bool J => GetKey(Keys.J);
    /// <summary>
    /// Whether the K key is currently pressed
    /// </summary>
    public static bool K => GetKey(Keys.K);
    /// <summary>
    /// Whether the L key is currently pressed
    /// </summary>
    public static bool L => GetKey(Keys.L);
    /// <summary>
    /// Whether the M key is currently pressed
    /// </summary>
    public static bool M => GetKey(Keys.M);
    /// <summary>
    /// Whether the N key is currently pressed
    /// </summary>
    public static bool N => GetKey(Keys.N);
    /// <summary>
    /// Whether the O key is currently pressed
    /// </summary>
    public static bool O => GetKey(Keys.O);
    /// <summary>
    /// Whether the P key is currently pressed
    /// </summary>
    public static bool P => GetKey(Keys.P);
    /// <summary>
    /// Whether the Q key is currently pressed
    /// </summary>
    public static bool Q => GetKey(Keys.Q);
    /// <summary>
    /// Whether the R key is currently pressed
    /// </summary>
    public static bool R => GetKey(Keys.R);
    /// <summary>
    /// Whether the S key is currently pressed
    /// </summary>
    public static bool S => GetKey(Keys.S);
    /// <summary>
    /// Whether the T key is currently pressed
    /// </summary>
    public static bool T => GetKey(Keys.T);
    /// <summary>
    /// Whether the U key is currently pressed
    /// </summary>
    public static bool U => GetKey(Keys.U);
    /// <summary>
    /// Whether the V key is currently pressed
    /// </summary>
    public static bool V => GetKey(Keys.V);
    /// <summary>
    /// Whether the W key is currently pressed
    /// </summary>
    public static bool W => GetKey(Keys.W);
    /// <summary>
    /// Whether the X key is currently pressed
    /// </summary>
    public static bool X => GetKey(Keys.X);
    /// <summary>
    /// Whether the Y key is currently pressed
    /// </summary>
    public static bool Y => GetKey(Keys.Y);
    /// <summary>
    /// Whether the Z key is currently pressed
    /// </summary>
    public static bool Z => GetKey(Keys.Z);
    /// <summary>
    /// Whether the 0 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad0 => GetKey(Keys.NumPad0);
    /// <summary>
    /// Whether the 1 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad1 => GetKey(Keys.NumPad1);
    /// <summary>
    /// Whether the 2 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad2 => GetKey(Keys.NumPad2);
    /// <summary>
    /// Whether the 3 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad3 => GetKey(Keys.NumPad3);
    /// <summary>
    /// Whether the 4 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad4 => GetKey(Keys.NumPad4);
    /// <summary>
    /// Whether the 5 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad5 => GetKey(Keys.NumPad5);
    /// <summary>
    /// Whether the 6 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad6 => GetKey(Keys.NumPad6);
    /// <summary>
    /// Whether the 7 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad7 => GetKey(Keys.NumPad7);
    /// <summary>
    /// Whether the 8 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad8 => GetKey(Keys.NumPad8);
    /// <summary>
    /// Whether the 9 key on the numpad is currently pressed
    /// </summary>
    public static bool NumPad9 => GetKey(Keys.NumPad9);
    /// <summary>
    /// Whether the F1 key is currently pressed
    /// </summary>
    public static bool F1 => GetKey(Keys.F1);
    /// <summary>
    /// Whether the F2 key is currently pressed
    /// </summary>
    public static bool F2 => GetKey(Keys.F2);
    /// <summary>
    /// Whether the F3 key is currently pressed.
    /// </summary>
    public static bool F3 => GetKey(Keys.F3);
    /// <summary>
    /// Whether the F4 key is currently pressed.
    /// </summary>
    public static bool F4 => GetKey(Keys.F4);

    /// <summary>
    /// Whether the F5 key is currently pressed.
    /// </summary>
    public static bool F5 => GetKey(Keys.F5);

    /// <summary>
    /// Whether the F6 key is currently pressed.
    /// </summary>
    public static bool F6 => GetKey(Keys.F6);

    /// <summary>
    /// Whether the F7 key is currently pressed.
    /// </summary>
    public static bool F7 => GetKey(Keys.F7);

    /// <summary>
    /// Whether the F8 key is currently pressed.
    /// </summary>
    public static bool F8 => GetKey(Keys.F8);

    /// <summary>
    /// Whether the F9 key is currently pressed.
    /// </summary>
    public static bool F9 => GetKey(Keys.F9);

    /// <summary>
    /// Whether the F10 key is currently pressed.
    /// </summary>
    public static bool F10 => GetKey(Keys.F10);

    /// <summary>
    /// Whether the F11 key is currently pressed.
    /// </summary>
    public static bool F11 => GetKey(Keys.F11);

    /// <summary>
    /// Whether the F12 key is currently pressed.
    /// </summary>
    public static bool F12 => GetKey(Keys.F12);

    /// <summary>
    /// Whether the F13 key is currently pressed.
    /// </summary>
    public static bool F13 => GetKey(Keys.F13);

    /// <summary>
    /// Whether the F14 key is currently pressed.
    /// </summary>
    public static bool F14 => GetKey(Keys.F14);

    /// <summary>
    /// Whether the F15 key is currently pressed.
    /// </summary>
    public static bool F15 => GetKey(Keys.F15);

    /// <summary>
    /// Whether the F16 key is currently pressed.
    /// </summary>
    public static bool F16 => GetKey(Keys.F16);

    /// <summary>
    /// Whether the F17 key is currently pressed.
    /// </summary>
    public static bool F17 => GetKey(Keys.F17);

    /// <summary>
    /// Whether the F18 key is currently pressed.
    /// </summary>
    public static bool F18 => GetKey(Keys.F18);

    /// <summary>
    /// Whether the F19 key is currently pressed.
    /// </summary>
    public static bool F19 => GetKey(Keys.F19);

    /// <summary>
    /// Whether the F20 key is currently pressed.
    /// </summary>
    public static bool F20 => GetKey(Keys.F20);

    /// <summary>
    /// Whether the F21 key is currently pressed.
    /// </summary>
    public static bool F21 => GetKey(Keys.F21);

    /// <summary>
    /// Whether the F22 key is currently pressed.
    /// </summary>
    public static bool F22 => GetKey(Keys.F22);

    /// <summary>
    /// Whether the F23 key is currently pressed.
    /// </summary>
    public static bool F23 => GetKey(Keys.F23);

    /// <summary>
    /// Whether the F24 key is currently pressed.
    /// </summary>
    public static bool F24 => GetKey(Keys.F24);

    /// <summary>
    /// Whether the OemTilde key is currently pressed.
    /// </summary>
    public static bool OemTilde => GetKey(Keys.OemTilde);

    /// <summary>
    /// Whether the OemSemicolon key is currently pressed.
    /// </summary>
    public static bool OemSemicolon => GetKey(Keys.OemSemicolon);

    /// <summary>
    /// Whether the OemQuotes key is currently pressed.
    /// </summary>
    public static bool OemQuotes => GetKey(Keys.OemQuotes);

    /// <summary>
    /// Whether the OemQuestion key is currently pressed.
    /// </summary>
    public static bool OemQuestion => GetKey(Keys.OemQuestion);

    /// <summary>
    /// Whether the OemPlus key is currently pressed.
    /// </summary>
    public static bool OemPlus => GetKey(Keys.OemPlus);

    /// <summary>
    /// Whether the OemPipe key is currently pressed.
    /// </summary>
    public static bool OemPipe => GetKey(Keys.OemPipe);

    /// <summary>
    /// Whether the OemPeriod key is currently pressed.
    /// </summary>
    public static bool OemPeriod => GetKey(Keys.OemPeriod);

    /// <summary>
    /// Whether the OemOpenBrackets key is currently pressed.
    /// </summary>
    public static bool OemOpenBrackets => GetKey(Keys.OemOpenBrackets);

    /// <summary>
    /// Whether the OemMinus key is currently pressed.
    /// </summary>
    public static bool OemMinus => GetKey(Keys.OemMinus);

    /// <summary>
    /// Whether the OemCloseBrackets key is currently pressed.
    /// </summary>
    public static bool OemCloseBrackets => GetKey(Keys.OemCloseBrackets);

    /// <summary>
    /// Whether the OemComma key is currently pressed.
    /// </summary>
    public static bool OemComma => GetKey(Keys.OemComma);

    /// <summary>
    /// Whether the OemBackslash key is currently pressed.
    /// </summary>
    public static bool OemBackslash => GetKey(Keys.OemBackslash);

    /// <summary>
    /// Whether the OemClear key is currently pressed.
    /// </summary>
    public static bool OemClear => GetKey(Keys.OemClear);

    /// <summary>
    /// Whether the OemCopy key is currently pressed.
    /// </summary>
    public static bool OemCopy => GetKey(Keys.OemCopy);

    /// <summary>
    /// Whether the OemEnlW key is currently pressed.
    /// </summary>
    public static bool OemEnlW => GetKey(Keys.OemEnlW);
    /// <summary>
    /// Whether the space key is currently pressed
    /// </summary>
    public static bool Space => GetKey(Keys.Space);
    /// <summary>
    /// Whether the left shift key is currently pressed
    /// </summary>
    public static bool LeftShift => GetKey(Keys.LeftShift);
    /// <summary>
    /// Whether the right shift key is currently pressed
    /// </summary>
    public static bool RightShift => GetKey(Keys.RightShift);
    /// <summary>
    /// Whether the left control key is currently pressed
    /// </summary>
    public static bool LeftControl => GetKey(Keys.LeftControl);
    /// <summary>
    /// Whether the right control key is currently pressed
    /// </summary>
    public static bool RightControl => GetKey(Keys.RightControl);
    /// <summary>
    /// Whether the left alt key is currently pressed
    /// </summary>
    public static bool LeftAlt => GetKey(Keys.LeftAlt);
    /// <summary>
    /// Whether the right alt key is currently pressed
    /// </summary>
    public static bool RightAlt => GetKey(Keys.RightAlt);
    /// <summary>
    /// Whether the left windows key is currently pressed
    /// </summary>
    public static bool LeftWindows => GetKey(Keys.LeftWindows);
    /// <summary>
    /// Whether the right windows key is currently pressed
    /// </summary>
    public static bool RightWindows => GetKey(Keys.RightWindows);
    /// <summary>
    /// Whether the enter key is currently pressed
    /// </summary>
    public static bool Enter => GetKey(Keys.Enter);
    /// <summary>
    /// Whether the escape key is currently pressed
    /// </summary>
    public static bool Escape => GetKey(Keys.Escape);
    /// <summary>
    /// Whether the tab key is currently pressed
    /// </summary>
    public static bool Tab => GetKey(Keys.Tab);
    /// <summary>
    /// Whether the back key is currently pressed
    /// </summary>
    public static bool Back => GetKey(Keys.Back);
    /// <summary>
    /// Whether the caps lock key is currently pressed
    /// </summary>
    public static bool CapsLock => GetKey(Keys.CapsLock);
    /// <summary>
    /// Whether the page up key is currently pressed
    /// </summary>
    public static bool PageUp => GetKey(Keys.PageUp);
    /// <summary>
    /// Whether the page down key is currently pressed
    /// </summary>
    public static bool PageDown => GetKey(Keys.PageDown);
    /// <summary>
    /// Whether the end key is currently pressed
    /// </summary>
    public static bool End => GetKey(Keys.End);
    /// <summary>
    /// Whether the home key is currently pressed
    /// </summary>
    public static bool Home => GetKey(Keys.Home);
    /// <summary>
    /// Whether the insert key is currently pressed
    /// </summary>
    public static bool Insert => GetKey(Keys.Insert);
    /// <summary>
    /// Whether the delete key is currently pressed
    /// </summary>
    public static bool Delete => GetKey(Keys.Delete);
    /// <summary>
    /// Whether the add key is currently pressed
    /// </summary>
    public static bool Add => GetKey(Keys.Add);
    /// <summary>
    /// Whether the subtract key is currently pressed
    /// </summary>
    public static bool Subtract => GetKey(Keys.Subtract);
    /// <summary>
    /// Whether the multiply key is currently pressed
    /// </summary>
    public static bool Multiply => GetKey(Keys.Multiply);
    /// <summary>
    /// Whether the divide key is currently pressed
    /// </summary>
    public static bool Divide => GetKey(Keys.Divide);
    /// <summary>
    /// Whether the left key is currently pressed
    /// </summary>
    public static bool Left => GetKey(Keys.Left);
    /// <summary>
    /// Whether the right key is currently pressed
    /// </summary>
    public static bool Right => GetKey(Keys.Right);
    /// <summary>
    /// Whether the up key is currently pressed
    /// </summary>
    public static bool Up => GetKey(Keys.Up);
    /// <summary>
    /// Whether the down key is currently pressed
    /// </summary>
    public static bool Down => GetKey(Keys.Down);
    /// <summary>
    /// Whether the num lock key is currently pressed
    /// </summary>
    public static bool NumLock => GetKey(Keys.NumLock);
    /// <summary>
    /// Whether the scroll key is currently pressed
    /// </summary>
    public static bool Scroll => GetKey(Keys.Scroll);
    /// <summary>
    /// Whether the left alt key was pressed this frame.
    /// </summary>
    public static bool LeftAltPressed => GetKeyDown(Keys.LeftAlt);
    /// <summary>
    /// Whether the right alt key was pressed this frame.
    /// </summary>
    public static bool RightAltPressed => GetKeyDown(Keys.RightAlt);
    /// <summary>
    /// Whether the left shift key was pressed this frame.
    /// </summary>
    public static bool LeftShiftPressed => GetKeyDown(Keys.LeftShift);
    /// <summary>
    /// Whether the right shift key was pressed this frame.
    /// </summary>
    public static bool RightShiftPressed => GetKeyDown(Keys.RightShift);
    /// <summary>
    /// Whether the left control key was pressed this frame.
    /// </summary>
    public static bool LeftControlPressed => GetKeyDown(Keys.LeftControl);
    /// <summary>
    /// Whether the right control key was pressed this frame.
    /// </summary>
    public static bool RightControlPressed => GetKeyDown(Keys.RightControl);
    /// <summary>
    /// Whether the left windows key was pressed this frame.
    /// </summary>
    public static bool LeftWindowsPressed => GetKeyDown(Keys.LeftWindows);
    /// <summary>
    /// Whether the right windows key was pressed this frame.
    /// </summary>
    public static bool RightWindowsPressed => GetKeyDown(Keys.RightWindows);
    /// <summary>
    /// Whether the enter key was pressed this frame.
    /// </summary>
    public static bool EnterPressed => GetKeyDown(Keys.Enter);
    /// <summary>
    /// Whether the escape key was pressed this frame.
    /// </summary>
    public static bool EscapePressed => GetKeyDown(Keys.Escape);
    /// <summary>
    /// Whether the tab key was pressed this frame.
    /// </summary>
    public static bool TabPressed => GetKeyDown(Keys.Tab);
    /// <summary>
    /// Whether the back key was pressed this frame.
    /// </summary>
    public static bool BackPressed => GetKeyDown(Keys.Back);
    /// <summary>
    /// Whether the caps lock key was pressed this frame.
    /// </summary>
    public static bool CapsLockPressed => GetKeyDown(Keys.CapsLock);
    /// <summary>
    /// Whether the page up key was pressed this frame.
    /// </summary>
    public static bool PageUpPressed => GetKeyDown(Keys.PageUp);
    /// <summary>
    /// Whether the page down key was pressed this frame.
    /// </summary>
    public static bool PageDownPressed => GetKeyDown(Keys.PageDown);
    /// <summary>
    /// Whether the end key was pressed this frame.
    /// </summary>
    public static bool EndPressed => GetKeyDown(Keys.End);
    /// <summary>
    /// Whether the home key was pressed this frame.
    /// </summary>
    public static bool HomePressed => GetKeyDown(Keys.Home);
    /// <summary>
    /// Whether the insert key was pressed this frame.
    /// </summary>
    public static bool InsertPressed => GetKeyDown(Keys.Insert);
    /// <summary>
    /// Whether the delete key was pressed this frame.
    /// </summary>
    public static bool DeletePressed => GetKeyDown(Keys.Delete);
    /// <summary>
    /// Whether the add key was pressed this frame.
    /// </summary>
    public static bool AddPressed => GetKeyDown(Keys.Add);
    /// <summary>
    /// Whether the subtract key was pressed this frame.
    /// </summary>
    public static bool SubtractPressed => GetKeyDown(Keys.Subtract);
    /// <summary>
    /// Whether the multiply key was pressed this frame.
    /// </summary>
    public static bool MultiplyPressed => GetKeyDown(Keys.Multiply);
    /// <summary>
    /// Whether the divide key was pressed this frame.
    /// </summary>
    public static bool DividePressed => GetKeyDown(Keys.Divide);
    /// <summary>
    /// Whether the left arrow key was pressed this frame.
    /// </summary>
    public static bool LeftPressed => GetKeyDown(Keys.Left);
    /// <summary>
    /// Whether the right arrow key was pressed this frame.
    /// </summary>
    public static bool RightPressed => GetKeyDown(Keys.Right);
    /// <summary>
    /// Whether the up arrow key was pressed this frame.
    /// </summary>
    public static bool UpPressed => GetKeyDown(Keys.Up);
    /// <summary>
    /// Whether the down arrow key was pressed this frame.
    /// </summary>
    public static bool DownPressed => GetKeyDown(Keys.Down);
    /// <summary>
    /// Whether the num lock key was pressed this frame.
    /// </summary>
    public static bool NumLockPressed => GetKeyDown(Keys.NumLock);
    /// <summary>
    /// Whether the scroll key was pressed this frame.
    /// </summary>
    public static bool ScrollPressed => GetKeyDown(Keys.Scroll);
    /// <summary>
    /// Whether the left alt key was released this frame.
    /// </summary>
    public static bool LeftAltReleased => GetKeyUp(Keys.LeftAlt);
    /// <summary>
    /// Whether the right alt key was released this frame.
    /// </summary>
    public static bool RightAltReleased => GetKeyUp(Keys.RightAlt);
    /// <summary>
    /// Whether the left shift key was released this frame.
    /// </summary>
    public static bool LeftShiftReleased => GetKeyUp(Keys.LeftShift);
    /// <summary>
    /// Whether the right shift key was released this frame.
    /// </summary>
    public static bool RightShiftReleased => GetKeyUp(Keys.RightShift);
    /// <summary>
    /// Whether the left control key was released this frame.
    /// </summary>
    public static bool LeftControlReleased => GetKeyUp(Keys.LeftControl);
    /// <summary>
    /// Whether the right control key was released this frame.
    /// </summary>
    public static bool RightControlReleased => GetKeyUp(Keys.RightControl);
    /// <summary>
    /// Whether the left windows key was released this frame.
    /// </summary>
    public static bool LeftWindowsReleased => GetKeyUp(Keys.LeftWindows);
    /// <summary>
    /// Whether the right windows key was released this frame.
    /// </summary>
    public static bool RightWindowsReleased => GetKeyUp(Keys.RightWindows);
    /// <summary>
    /// Whether the enter key was released this frame.
    /// </summary>
    public static bool EnterReleased => GetKeyUp(Keys.Enter);
    /// <summary>
    /// Whether the escape key was released this frame.
    /// </summary>
    public static bool EscapeReleased => GetKeyUp(Keys.Escape);
    /// <summary>
    /// Whether the tab key was released this frame.
    /// </summary>
    public static bool TabReleased => GetKeyUp(Keys.Tab);
    /// <summary>
    /// Whether the back key was released this frame.
    /// </summary>
    public static bool BackReleased => GetKeyUp(Keys.Back);
    /// <summary>
    /// Whether the caps lock key was released this frame.
    /// </summary>
    public static bool CapsLockReleased => GetKeyUp(Keys.CapsLock);
    /// <summary>
    /// Whether the page up key was released this frame.
    /// </summary>
    public static bool PageUpReleased => GetKeyUp(Keys.PageUp);
    /// <summary>
    /// Whether the page down key was released this frame.
    /// </summary>
    public static bool PageDownReleased => GetKeyUp(Keys.PageDown);
    /// <summary>
    /// Whether the end key was released this frame.
    /// </summary>
    public static bool EndReleased => GetKeyUp(Keys.End);
    /// <summary>
    /// Whether the home key was released this frame.
    /// </summary>
    public static bool HomeReleased => GetKeyUp(Keys.Home);
    /// <summary>
    /// Whether the insert key was released this frame.
    /// </summary>
    public static bool InsertReleased => GetKeyUp(Keys.Insert);
    /// <summary>
    /// Whether the delete key was released this frame.
    /// </summary>
    public static bool DeleteReleased => GetKeyUp(Keys.Delete);
    /// <summary>
    /// Whether the add key was released this frame.
    /// </summary>
    public static bool AddReleased => GetKeyUp(Keys.Add);
    /// <summary>
    /// Whether the subtract key was released this frame.
    /// </summary>
    public static bool SubtractReleased => GetKeyUp(Keys.Subtract);
    /// <summary>
    /// Whether the multiply key was released this frame.
    /// </summary>
    public static bool MultiplyReleased => GetKeyUp(Keys.Multiply);
    /// <summary>
    /// Whether the divide key was released this frame.
    /// </summary>
    public static bool DivideReleased => GetKeyUp(Keys.Divide);
    /// <summary>
    /// Whether the left arrow key was released this frame.
    /// </summary>
    public static bool LeftReleased => GetKeyUp(Keys.Left);
    /// <summary>
    /// Whether the right arrow key was released this frame.
    /// </summary>
    public static bool RightReleased => GetKeyUp(Keys.Right);
    /// <summary>
    /// Whether the up arrow key was released this frame.
    /// </summary>
    public static bool UpReleased => GetKeyUp(Keys.Up);
    /// <summary>
    /// Whether the down arrow key was released this frame.
    /// </summary>
    public static bool DownReleased => GetKeyUp(Keys.Down);
    /// <summary>
    /// Whether the num lock key was released this frame.
    /// </summary>
    public static bool NumLockReleased => GetKeyUp(Keys.NumLock);
    /// <summary>
    /// Whether the scroll key was released this frame.
    /// </summary>
    public static bool ScrollReleased => GetKeyUp(Keys.Scroll);
    /// <summary>
    /// Whether the space key was pressed this frame.
    /// </summary>
    public static bool SpaceDown => GetKeyDown(Keys.Space);
    /// <summary>
    /// Whether the space key was released this frame.
    /// </summary>
    public static bool SpaceUp => GetKeyUp(Keys.Space);

    /// <summary>
    /// Whether either alt key is currently pressed
    /// </summary>
    public static bool Alt => GetKey(Keys.LeftAlt) || GetKey(Keys.RightAlt);
    /// <summary>
    /// Whether either shift key is currently pressed
    /// </summary>
    public static bool Shift => GetKey(Keys.LeftShift) || GetKey(Keys.RightShift);
    /// <summary>
    /// Whether either control key is currently pressed
    /// </summary>
    public static bool Control => GetKey(Keys.LeftControl) || GetKey(Keys.RightControl);
    /// <summary>
    /// Whether either windows key is currently pressed
    /// </summary>
    public static bool Windows => GetKey(Keys.LeftWindows) || GetKey(Keys.RightWindows);
    /// <summary>
    /// Whether either alt key is pressed this frame
    /// </summary>
    public static bool AltPressed => GetKeyDown(Keys.LeftAlt) || GetKeyDown(Keys.RightAlt);
    /// <summary>
    /// Whether either shift key is pressed this frame
    /// </summary>
    public static bool ShiftPressed => GetKeyDown(Keys.LeftShift) || GetKeyDown(Keys.RightShift);
    /// <summary>
    /// Whether either control key is pressed this frame
    /// </summary>
    public static bool ControlPressed => GetKeyDown(Keys.LeftControl) || GetKeyDown(Keys.RightControl);
    /// <summary>
    /// Whether either windows key is pressed this frame
    /// </summary>
    public static bool WindowsPressed => GetKeyDown(Keys.LeftWindows) || GetKeyDown(Keys.RightWindows);
    /// <summary>
    /// Whether either alt key is released this frame
    /// </summary>
    public static bool AltReleased => GetKeyUp(Keys.LeftAlt) || GetKeyUp(Keys.RightAlt);
    /// <summary>
    /// Whether either shift key is released this frame
    /// </summary>
    public static bool ShiftReleased => GetKeyUp(Keys.LeftShift) || GetKeyUp(Keys.RightShift);
    /// <summary>
    /// Whether either control key is released this frame
    /// </summary>
    public static bool ControlReleased => GetKeyUp(Keys.LeftControl) || GetKeyUp(Keys.RightControl);
    /// <summary>
    /// Whether either windows key is released this frame
    /// </summary>
    public static bool WindowsReleased => GetKeyUp(Keys.LeftWindows) || GetKeyUp(Keys.RightWindows);

    /// <summary>
    /// Whether the A key was pressed this frame.
    /// </summary>
    public static bool ADown => GetKeyDown(Keys.A);

    /// <summary>
    /// Whether the B key was pressed this frame.
    /// </summary>
    public static bool BDown => GetKeyDown(Keys.B);

    /// <summary>
    /// Whether the C key was pressed this frame.
    /// </summary>
    public static bool CDown => GetKeyDown(Keys.C);

    /// <summary>
    /// Whether the D key was pressed this frame.
    /// </summary>
    public static bool DDown => GetKeyDown(Keys.D);

    /// <summary>
    /// Whether the E key was pressed this frame.
    /// </summary>
    public static bool EDown => GetKeyDown(Keys.E);

    /// <summary>
    /// Whether the F key was pressed this frame.
    /// </summary>
    public static bool FDown => GetKeyDown(Keys.F);

    /// <summary>
    /// Whether the G key was pressed this frame.
    /// </summary>
    public static bool GDown => GetKeyDown(Keys.G);

    /// <summary>
    /// Whether the H key was pressed this frame.
    /// </summary>
    public static bool HDown => GetKeyDown(Keys.H);

    /// <summary>
    /// Whether the I key was pressed this frame.
    /// </summary>
    public static bool IDown => GetKeyDown(Keys.I);

    /// <summary>
    /// Whether the J key was pressed this frame.
    /// </summary>
    public static bool JDown => GetKeyDown(Keys.J);

    /// <summary>
    /// Whether the K key was pressed this frame.
    /// </summary>
    public static bool KDown => GetKeyDown(Keys.K);

    /// <summary>
    /// Whether the L key was pressed this frame.
    /// </summary>
    public static bool LDown => GetKeyDown(Keys.L);

    /// <summary>
    /// Whether the M key was pressed this frame.
    /// </summary>
    public static bool MDown => GetKeyDown(Keys.M);

    /// <summary>
    /// Whether the N key was pressed this frame.
    /// </summary>
    public static bool NDown => GetKeyDown(Keys.N);

    /// <summary>
    /// Whether the O key was pressed this frame.
    /// </summary>
    public static bool ODown => GetKeyDown(Keys.O);

    /// <summary>
    /// Whether the P key was pressed this frame.
    /// </summary>
    public static bool PDown => GetKeyDown(Keys.P);

    /// <summary>
    /// Whether the Q key was pressed this frame.
    /// </summary>
    public static bool QDown => GetKeyDown(Keys.Q);

    /// <summary>
    /// Whether the R key was pressed this frame.
    /// </summary>
    public static bool RDown => GetKeyDown(Keys.R);

    /// <summary>
    /// Whether the S key was pressed this frame.
    /// </summary>
    public static bool SDown => GetKeyDown(Keys.S);

    /// <summary>
    /// Whether the T key was pressed this frame.
    /// </summary>
    public static bool TDown => GetKeyDown(Keys.T);

    /// <summary>
    /// Whether the U key was pressed this frame.
    /// </summary>
    public static bool UDown => GetKeyDown(Keys.U);

    /// <summary>
    /// Whether the V key was pressed this frame.
    /// </summary>
    public static bool VDown => GetKeyDown(Keys.V);

    /// <summary>
    /// Whether the W key was pressed this frame.
    /// </summary>
    public static bool WDown => GetKeyDown(Keys.W);

    /// <summary>
    /// Whether the X key was pressed this frame.
    /// </summary>
    public static bool XDown => GetKeyDown(Keys.X);

    /// <summary>
    /// Whether the Y key was pressed this frame.
    /// </summary>
    public static bool YDown => GetKeyDown(Keys.Y);

    /// <summary>
    /// Whether the Z key was pressed this frame.
    /// </summary>
    public static bool ZDown => GetKeyDown(Keys.Z);

    /// <summary>
    /// Whether the NumPad0 key was pressed this frame.
    /// </summary>
    public static bool NumPad0Down => GetKeyDown(Keys.NumPad0);

    /// <summary>
    /// Whether the NumPad1 key was pressed this frame.
    /// </summary>
    public static bool NumPad1Down => GetKeyDown(Keys.NumPad1);

    /// <summary>
    /// Whether the NumPad2 key was pressed this frame.
    /// </summary>
    public static bool NumPad2Down => GetKeyDown(Keys.NumPad2);

    /// <summary>
    /// Whether the NumPad3 key was pressed this frame.
    /// </summary>
    public static bool NumPad3Down => GetKeyDown(Keys.NumPad3);

    /// <summary>
    /// Whether the NumPad4 key was pressed this frame.
    /// </summary>
    public static bool NumPad4Down => GetKeyDown(Keys.NumPad4);

    /// <summary>
    /// Whether the NumPad5 key was pressed this frame.
    /// </summary>
    public static bool NumPad5Down => GetKeyDown(Keys.NumPad5);

    /// <summary>
    /// Whether the NumPad6 key was pressed this frame.
    /// </summary>
    public static bool NumPad6Down => GetKeyDown(Keys.NumPad6);

    /// <summary>
    /// Whether the NumPad7 key was pressed this frame.
    /// </summary>
    public static bool NumPad7Down => GetKeyDown(Keys.NumPad7);

    /// <summary>
    /// Whether the NumPad8 key was pressed this frame.
    /// </summary>
    public static bool NumPad8Down => GetKeyDown(Keys.NumPad8);

    /// <summary>
    /// Whether the NumPad9 key was pressed this frame.
    /// </summary>
    public static bool NumPad9Down => GetKeyDown(Keys.NumPad9);

    /// <summary>
    /// Whether the F1 key was pressed this frame.
    /// </summary>
    public static bool F1Down => GetKeyDown(Keys.F1);

    /// <summary>
    /// Whether the F2 key was pressed this frame.
    /// </summary>
    public static bool F2Down => GetKeyDown(Keys.F2);

    /// <summary>
    /// Whether the F3 key was pressed this frame.
    /// </summary>
    public static bool F3Down => GetKeyDown(Keys.F3);

    /// <summary>
    /// Whether the F4 key was pressed this frame.
    /// </summary>
    public static bool F4Down => GetKeyDown(Keys.F4);

    /// <summary>
    /// Whether the F5 key was pressed this frame.
    /// </summary>
    public static bool F5Down => GetKeyDown(Keys.F5);

    /// <summary>
    /// Whether the F6 key was pressed this frame.
    /// </summary>
    public static bool F6Down => GetKeyDown(Keys.F6);

    /// <summary>
    /// Whether the F7 key was pressed this frame.
    /// </summary>
    public static bool F7Down => GetKeyDown(Keys.F7);

    /// <summary>
    /// Whether the F8 key was pressed this frame.
    /// </summary>
    public static bool F8Down => GetKeyDown(Keys.F8);

    /// <summary>
    /// Whether the F9 key was pressed this frame.
    /// </summary>
    public static bool F9Down => GetKeyDown(Keys.F9);

    /// <summary>
    /// Whether the F10 key was pressed this frame.
    /// </summary>
    public static bool F10Down => GetKeyDown(Keys.F10);

    /// <summary>
    /// Whether the F11 key was pressed this frame.
    /// </summary>
    public static bool F11Down => GetKeyDown(Keys.F11);

    /// <summary>
    /// Whether the F12 key was pressed this frame.
    /// </summary>
    public static bool F12Down => GetKeyDown(Keys.F12);

    /// <summary>
    /// Whether the F13 key was pressed this frame.
    /// </summary>
    public static bool F13Down => GetKeyDown(Keys.F13);

    /// <summary>
    /// Whether the F14 key was pressed this frame.
    /// </summary>
    public static bool F14Down => GetKeyDown(Keys.F14);

    /// <summary>
    /// Whether the F15 key was pressed this frame.
    /// </summary>
    public static bool F15Down => GetKeyDown(Keys.F15);

    /// <summary>
    /// Whether the F16 key was pressed this frame.
    /// </summary>
    public static bool F16Down => GetKeyDown(Keys.F16);

    /// <summary>
    /// Whether the F17 key was pressed this frame.
    /// </summary>
    public static bool F17Down => GetKeyDown(Keys.F17);

    /// <summary>
    /// Whether the F18 key was pressed this frame.
    /// </summary>
    public static bool F18Down => GetKeyDown(Keys.F18);

    /// <summary>
    /// Whether the F19 key was pressed this frame.
    /// </summary>
    public static bool F19Down => GetKeyDown(Keys.F19);

    /// <summary>
    /// Whether the F20 key was pressed this frame.
    /// </summary>
    public static bool F20Down => GetKeyDown(Keys.F20);

    /// <summary>
    /// Whether the F21 key was pressed this frame.
    /// </summary>
    public static bool F21Down => GetKeyDown(Keys.F21);

    /// <summary>
    /// Whether the F22 key was pressed this frame.
    /// </summary>
    public static bool F22Down => GetKeyDown(Keys.F22);

    /// <summary>
    /// Whether the F23 key was pressed this frame.
    /// </summary>
    public static bool F23Down => GetKeyDown(Keys.F23);

    /// <summary>
    /// Whether the F24 key was pressed this frame.
    /// </summary>
    public static bool F24Down => GetKeyDown(Keys.F24);

    /// <summary>
    /// Whether the OemTilde key was pressed this frame.
    /// </summary>
    public static bool OemTildeDown => GetKeyDown(Keys.OemTilde);

    /// <summary>
    /// Whether the OemSemicolon key was pressed this frame.
    /// </summary>
    public static bool OemSemicolonDown => GetKeyDown(Keys.OemSemicolon);

    /// <summary>
    /// Whether the OemQuotes key was pressed this frame.
    /// </summary>
    public static bool OemQuotesDown => GetKeyDown(Keys.OemQuotes);

    /// <summary>
    /// Whether the OemQuestion key was pressed this frame.
    /// </summary>
    public static bool OemQuestionDown => GetKeyDown(Keys.OemQuestion);

    /// <summary>
    /// Whether the OemPlus key was pressed this frame.
    /// </summary>
    public static bool OemPlusDown => GetKeyDown(Keys.OemPlus);

    /// <summary>
    /// Whether the OemPipe key was pressed this frame.
    /// </summary>
    public static bool OemPipeDown => GetKeyDown(Keys.OemPipe);

    /// <summary>
    /// Whether the OemPeriod key was pressed this frame.
    /// </summary>
    public static bool OemPeriodDown => GetKeyDown(Keys.OemPeriod);

    /// <summary>
    /// Whether the OemOpenBrackets key was pressed this frame.
    /// </summary>
    public static bool OemOpenBracketsDown => GetKeyDown(Keys.OemOpenBrackets);

    /// <summary>
    /// Whether the OemMinus key was pressed this frame.
    /// </summary>
    public static bool OemMinusDown => GetKeyDown(Keys.OemMinus);

    /// <summary>
    /// Whether the OemCloseBrackets key was pressed this frame.
    /// </summary>
    public static bool OemCloseBracketsDown => GetKeyDown(Keys.OemCloseBrackets);

    /// <summary>
    /// Whether the OemComma key was pressed this frame.
    /// </summary>
    public static bool OemCommaDown => GetKeyDown(Keys.OemComma);

    /// <summary>
    /// Whether the OemBackslash key was pressed this frame.
    /// </summary>
    public static bool OemBackslashDown => GetKeyDown(Keys.OemBackslash);

    /// <summary>
    /// Whether the OemClear key was pressed this frame.
    /// </summary>
    public static bool OemClearDown => GetKeyDown(Keys.OemClear);

    /// <summary>
    /// Whether the OemCopy key was pressed this frame.
    /// </summary>
    public static bool OemCopyDown => GetKeyDown(Keys.OemCopy);

    /// <summary>
    /// Whether the OemEnlW key was pressed this frame.
    /// </summary>
    public static bool OemEnlWDown => GetKeyDown(Keys.OemEnlW);

    /// <summary>
    /// Whether the A key was released this frame.
    /// </summary>
    public static bool AUp => GetKeyUp(Keys.A);

    /// <summary>
    /// Whether the B key was released this frame.
    /// </summary>
    public static bool BUp => GetKeyUp(Keys.B);

    /// <summary>
    /// Whether the C key was released this frame.
    /// </summary>
    public static bool CUp => GetKeyUp(Keys.C);

    /// <summary>
    /// Whether the D key was released this frame.
    /// </summary>
    public static bool DUp => GetKeyUp(Keys.D);

    /// <summary>
    /// Whether the E key was released this frame.
    /// </summary>
    public static bool EUp => GetKeyUp(Keys.E);

    /// <summary>
    /// Whether the F key was released this frame.
    /// </summary>
    public static bool FUp => GetKeyUp(Keys.F);

    /// <summary>
    /// Whether the G key was released this frame.
    /// </summary>
    public static bool GUp => GetKeyUp(Keys.G);

    /// <summary>
    /// Whether the H key was released this frame.
    /// </summary>
    public static bool HUp => GetKeyUp(Keys.H);

    /// <summary>
    /// Whether the I key was released this frame.
    /// </summary>
    public static bool IUp => GetKeyUp(Keys.I);

    /// <summary>
    /// Whether the J key was released this frame.
    /// </summary>
    public static bool JUp => GetKeyUp(Keys.J);

    /// <summary>
    /// Whether the K key was released this frame.
    /// </summary>
    public static bool KUp => GetKeyUp(Keys.K);

    /// <summary>
    /// Whether the L key was released this frame.
    /// </summary>
    public static bool LUp => GetKeyUp(Keys.L);

    /// <summary>
    /// Whether the M key was released this frame.
    /// </summary>
    public static bool MUp => GetKeyUp(Keys.M);

    /// <summary>
    /// Whether the N key was released this frame.
    /// </summary>
    public static bool NUp => GetKeyUp(Keys.N);

    /// <summary>
    /// Whether the O key was released this frame.
    /// </summary>
    public static bool OUp => GetKeyUp(Keys.O);

    /// <summary>
    /// Whether the P key was released this frame.
    /// </summary>
    public static bool PUp => GetKeyUp(Keys.P);

    /// <summary>
    /// Whether the Q key was released this frame.
    /// </summary>
    public static bool QUp => GetKeyUp(Keys.Q);

    /// <summary>
    /// Whether the R key was released this frame.
    /// </summary>
    public static bool RUp => GetKeyUp(Keys.R);

    /// <summary>
    /// Whether the S key was released this frame.
    /// </summary>
    public static bool SUp => GetKeyUp(Keys.S);

    /// <summary>
    /// Whether the T key was released this frame.
    /// </summary>
    public static bool TUp => GetKeyUp(Keys.T);

    /// <summary>
    /// Whether the U key was released this frame.
    /// </summary>
    public static bool UUp => GetKeyUp(Keys.U);

    /// <summary>
    /// Whether the V key was released this frame.
    /// </summary>
    public static bool VUp => GetKeyUp(Keys.V);

    /// <summary>
    /// Whether the W key was released this frame.
    /// </summary>
    public static bool WUp => GetKeyUp(Keys.W);

    /// <summary>
    /// Whether the X key was released this frame.
    /// </summary>
    public static bool XUp => GetKeyUp(Keys.X);

    /// <summary>
    /// Whether the Y key was released this frame.
    /// </summary>
    public static bool YUp => GetKeyUp(Keys.Y);

    /// <summary>
    /// Whether the Z key was released this frame.
    /// </summary>
    public static bool ZUp => GetKeyUp(Keys.Z);

    /// <summary>
    /// Whether the NumPad0 key was released this frame.
    /// </summary>
    public static bool NumPad0Up => GetKeyUp(Keys.NumPad0);

    /// <summary>
    /// Whether the NumPad1 key was released this frame.
    /// </summary>
    public static bool NumPad1Up => GetKeyUp(Keys.NumPad1);

    /// <summary>
    /// Whether the NumPad2 key was released this frame.
    /// </summary>
    public static bool NumPad2Up => GetKeyUp(Keys.NumPad2);

    /// <summary>
    /// Whether the NumPad3 key was released this frame.
    /// </summary>
    public static bool NumPad3Up => GetKeyUp(Keys.NumPad3);

    /// <summary>
    /// Whether the NumPad4 key was released this frame.
    /// </summary>
    public static bool NumPad4Up => GetKeyUp(Keys.NumPad4);

    /// <summary>
    /// Whether the NumPad5 key was released this frame.
    /// </summary>
    public static bool NumPad5Up => GetKeyUp(Keys.NumPad5);

    /// <summary>
    /// Whether the NumPad6 key was released this frame.
    /// </summary>
    public static bool NumPad6Up => GetKeyUp(Keys.NumPad6);

    /// <summary>
    /// Whether the NumPad7 key was released this frame.
    /// </summary>
    public static bool NumPad7Up => GetKeyUp(Keys.NumPad7);

    /// <summary>
    /// Whether the NumPad8 key was released this frame.
    /// </summary>
    public static bool NumPad8Up => GetKeyUp(Keys.NumPad8);

    /// <summary>
    /// Whether the NumPad9 key was released this frame.
    /// </summary>
    public static bool NumPad9Up => GetKeyUp(Keys.NumPad9);

    /// <summary>
    /// Whether the F1 key was released this frame.
    /// </summary>
    public static bool F1Up => GetKeyUp(Keys.F1);

    /// <summary>
    /// Whether the F2 key was released this frame.
    /// </summary>
    public static bool F2Up => GetKeyUp(Keys.F2);

    /// <summary>
    /// Whether the F3 key was released this frame.
    /// </summary>
    public static bool F3Up => GetKeyUp(Keys.F3);

    /// <summary>
    /// Whether the F4 key was released this frame.
    /// </summary>
    public static bool F4Up => GetKeyUp(Keys.F4);

    /// <summary>
    /// Whether the F5 key was released this frame.
    /// </summary>
    public static bool F5Up => GetKeyUp(Keys.F5);

    /// <summary>
    /// Whether the F6 key was released this frame.
    /// </summary>
    public static bool F6Up => GetKeyUp(Keys.F6);

    /// <summary>
    /// Whether the F7 key was released this frame.
    /// </summary>
    public static bool F7Up => GetKeyUp(Keys.F7);

    /// <summary>
    /// Whether the F8 key was released this frame.
    /// </summary>
    public static bool F8Up => GetKeyUp(Keys.F8);

    /// <summary>
    /// Whether the F9 key was released this frame.
    /// </summary>
    public static bool F9Up => GetKeyUp(Keys.F9);

    /// <summary>
    /// Whether the F10 key was released this frame.
    /// </summary>
    public static bool F10Up => GetKeyUp(Keys.F10);

    /// <summary>
    /// Whether the F11 key was released this frame.
    /// </summary>
    public static bool F11Up => GetKeyUp(Keys.F11);

    /// <summary>
    /// Whether the F12 key was released this frame.
    /// </summary>
    public static bool F12Up => GetKeyUp(Keys.F12);

    /// <summary>
    /// Whether the F13 key was released this frame.
    /// </summary>
    public static bool F13Up => GetKeyUp(Keys.F13);

    /// <summary>
    /// Whether the F14 key was released this frame.
    /// </summary>
    public static bool F14Up => GetKeyUp(Keys.F14);

    /// <summary>
    /// Whether the F15 key was released this frame.
    /// </summary>
    public static bool F15Up => GetKeyUp(Keys.F15);

    /// <summary>
    /// Whether the F16 key was released this frame.
    /// </summary>
    public static bool F16Up => GetKeyUp(Keys.F16);

    /// <summary>
    /// Whether the F17 key was released this frame.
    /// </summary>
    public static bool F17Up => GetKeyUp(Keys.F17);

    /// <summary>
    /// Whether the F18 key was released this frame.
    /// </summary>
    public static bool F18Up => GetKeyUp(Keys.F18);

    /// <summary>
    /// Whether the F19 key was released this frame.
    /// </summary>
    public static bool F19Up => GetKeyUp(Keys.F19);

    /// <summary>
    /// Whether the F20 key was released this frame.
    /// </summary>
    public static bool F20Up => GetKeyUp(Keys.F20);

    /// <summary>
    /// Whether the F21 key was released this frame.
    /// </summary>
    public static bool F21Up => GetKeyUp(Keys.F21);

    /// <summary>
    /// Whether the F22 key was released this frame.
    /// </summary>
    public static bool F22Up => GetKeyUp(Keys.F22);

    /// <summary>
    /// Whether the F23 key was released this frame.
    /// </summary>
    public static bool F23Up => GetKeyUp(Keys.F23);

    /// <summary>
    /// Whether the F24 key was released this frame.
    /// </summary>
    public static bool F24Up => GetKeyUp(Keys.F24);

    /// <summary>
    /// Whether the OemTilde key was released this frame.
    /// </summary>
    public static bool OemTildeUp => GetKeyUp(Keys.OemTilde);

    /// <summary>
    /// Whether the OemSemicolon key was released this frame.
    /// </summary>
    public static bool OemSemicolonUp => GetKeyUp(Keys.OemSemicolon);

    /// <summary>
    /// Whether the OemQuotes key was released this frame.
    /// </summary>
    public static bool OemQuotesUp => GetKeyUp(Keys.OemQuotes);

    /// <summary>
    /// Whether the OemQuestion key was released this frame.
    /// </summary>
    public static bool OemQuestionUp => GetKeyUp(Keys.OemQuestion);

    /// <summary>
    /// Whether the OemPlus key was released this frame.
    /// </summary>
    public static bool OemPlusUp => GetKeyUp(Keys.OemPlus);

    /// <summary>
    /// Whether the OemPipe key was released this frame.
    /// </summary>
    public static bool OemPipeUp => GetKeyUp(Keys.OemPipe);

    /// <summary>
    /// Whether the OemPeriod key was released this frame.
    /// </summary>
    public static bool OemPeriodUp => GetKeyUp(Keys.OemPeriod);

    /// <summary>
    /// Whether the OemOpenBrackets key was released this frame.
    /// </summary>
    public static bool OemOpenBracketsUp => GetKeyUp(Keys.OemOpenBrackets);

    /// <summary>
    /// Whether the OemMinus key was released this frame.
    /// </summary>
    public static bool OemMinusUp => GetKeyUp(Keys.OemMinus);

    /// <summary>
    /// Whether the OemCloseBrackets key was released this frame.
    /// </summary>
    public static bool OemCloseBracketsUp => GetKeyUp(Keys.OemCloseBrackets);

    /// <summary>
    /// Whether the OemComma key was released this frame.
    /// </summary>
    public static bool OemCommaUp => GetKeyUp(Keys.OemComma);

    /// <summary>
    /// Whether the OemBackslash key was released this frame.
    /// </summary>
    public static bool OemBackslashUp => GetKeyUp(Keys.OemBackslash);

    /// <summary>
    /// Whether the OemClear key was released this frame.
    /// </summary>
    public static bool OemClearUp => GetKeyUp(Keys.OemClear);

    /// <summary>
    /// Whether the OemCopy key was released this frame.
    /// </summary>
    public static bool OemCopyUp => GetKeyUp(Keys.OemCopy);

    /// <summary>
    /// Whether the OemEnlW key was released this frame.
    /// </summary>
    public static bool OemEnlWUp => GetKeyUp(Keys.OemEnlW);

    #endregion

}
/// <summary>
/// Selector for mouse buttons
/// </summary>
public enum MouseButton
{
    /// <summary>
    /// The left mouse button
    /// </summary>
    Left,
    /// <summary>
    /// The right mouse button
    /// </summary>
    Right,
    /// <summary>
    /// The middle mouse button
    /// </summary>
    Middle
}

