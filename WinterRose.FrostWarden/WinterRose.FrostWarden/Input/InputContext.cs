using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Input;

public class InputContext
{
    public int Priority { get; }

    public bool HasAnyFocus => HasKeyboardFocus || HasMouseFocus;

    public bool HasKeyboardFocus { get; internal set; }
    public bool HasMouseFocus { get; internal set; }

    public bool IsRequestingKeyboardFocus { get; set; }
    public bool IsRequestingMouseFocus { get; set; }
    public bool IsActive { get; set; } = true;

    public Vector2 MousePosition
    {
        get
        {
            if (IsActive && HasMouseFocus)
                return Provider.MousePosition;
            return new Vector2(-1, -1);
        }
    }

    public Vector2 MouseDelta
    {
        get
        {
            if (IsActive && HasMouseFocus)
                return Provider.MouseDelta;
            return new Vector2(0, 0);
        }
    }

    public Dictionary<string, NamedControl> Controls { get; } = new();
    public IInputProvider Provider { get; }
    public InputContext? HighestPriorityKeyboardAbove { get; internal set; }
    public InputContext? HighestPriorityMouseAbove { get; internal set; }

    public InputContext(IInputProvider provider, int priority) : this(provider, priority, true) { }

    public InputContext(IInputProvider provider, int priority, bool autoRegister)
    {
        Priority = priority;
        Provider = provider;

        if(autoRegister)
            InputManager.RegisterContext(this);
    }

    // --- NamedControl based ---
    public bool IsPressed(string controlName)
    {
        if (!IsActive)
            return false;

        if (!Controls.TryGetValue(controlName, out var control))
            return false;

        return control.IsPressed(Provider) && HasKeyboardFocus; // Named controls assumed keyboard/gamepad
    }

    public bool IsDown(string controlName)
    {
        if (!IsActive)
            return false;

        if (!Controls.TryGetValue(controlName, out var control))
            return false;

        return control.IsDown(Provider) && HasKeyboardFocus;
    }

    public bool IsUp(string controlName)
    {
        if (!IsActive)
            return false;

        if (!Controls.TryGetValue(controlName, out var control))
            return false;

        return control.IsUp(Provider) && HasKeyboardFocus;
    }

    // --- Keyboard overloads ---
    public bool IsPressed(KeyboardKey key)
        => IsPressed(new InputBinding(InputDeviceType.Keyboard, (int)key));

    public bool IsDown(KeyboardKey key)
        => IsDown(new InputBinding(InputDeviceType.Keyboard, (int)key));

    public bool IsUp(KeyboardKey key)
        => IsUp(new InputBinding(InputDeviceType.Keyboard, (int)key));

    // --- Mouse overloads ---
    public bool IsPressed(MouseButton button)
        => IsPressed(new InputBinding(InputDeviceType.Mouse, (int)button));

    public bool IsDown(MouseButton button)
        => IsDown(new InputBinding(InputDeviceType.Mouse, (int)button));

    public bool IsUp(MouseButton button)
        => IsUp(new InputBinding(InputDeviceType.Mouse, (int)button));

    // --- Direct binding pass-through ---
    private bool IsPressed(InputBinding binding)
    {
        if (!IsActive)
            return false;

        if (!HasRightFocus(binding))
            return false;

        return Provider.IsPressed(binding);
    }

    private bool IsDown(InputBinding binding)
    {
        if (!IsActive)
            return false;

        if (!HasRightFocus(binding))
            return false;

        return Provider.IsDown(binding);
    }

    private bool IsUp(InputBinding binding)
    {
        if (!IsActive)
            return false;

        if (!HasRightFocus(binding))
            return false;

        return Provider.IsUp(binding);
    }

    private bool HasRightFocus(InputBinding binding) 
        => binding.DeviceType switch
        {
            InputDeviceType.Keyboard => HasKeyboardFocus,
            InputDeviceType.Mouse => HasMouseFocus,
            InputDeviceType.MouseWheel => HasMouseFocus,
            InputDeviceType.GamepadButton => HasKeyboardFocus,
            InputDeviceType.GamepadAxis => HasKeyboardFocus,
            _ => false
        };

    internal void Update()
    {
        Provider.Update();
    }

    internal void RequestMouseFocusIfHovered(Rectangle bounds)
    {
        if (HighestPriorityMouseAbove is not null)
        {   
            IsRequestingMouseFocus = false;
            return;
        }
        IsRequestingMouseFocus = ray.CheckCollisionPointRec(ray.GetMousePosition(), bounds);
    }

    internal bool IsMouseHovering(Rectangle bounds)
    {
        if (HighestPriorityMouseAbove is not null)
            return false;
        return ray.CheckCollisionPointRec(ray.GetMousePosition(), bounds);
    }
}


