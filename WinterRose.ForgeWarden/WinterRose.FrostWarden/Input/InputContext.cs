using Raylib_cs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Input;

public class InputContext
{
    public int Priority { get; set; }

    public bool HasAnyFocus => HasKeyboardFocus || HasMouseFocus;

    public bool HasKeyboardFocus { get; internal set; }
    public bool HasMouseFocus { get; internal set; }

    public bool IsRequestingKeyboardFocus { get; set; }
    public bool IsRequestingMouseFocus { get; set; }
    public bool IsActive { get; set; } = true;

    public InputContext? HighestPriorityKeyboardAbove { get; internal set; }
    public InputContext? HighestPriorityMouseAbove { get; internal set; }

    /// <inheritdoc cref="IInputProvider.MousePosition"/>
    public Vector2 MousePosition
    {
        get
        {
            if (IsActive && HasMouseFocus)
                return Provider.MousePosition;
            return new Vector2(-1, -1);
        }
    }
    /// <inheritdoc cref="IInputProvider.MouseDelta"/>
    public Vector2 MouseDelta
    {
        get
        {
            if (IsActive && HasMouseFocus)
                return Provider.MouseDelta;
            return new Vector2(0, 0);
        }
    }

    private static ConcurrentDictionary<string, NamedControl> Controls { get; } = new();
    internal static void RegisterNamedControl(NamedControl namedControl)
    {
        if (Controls.TryGetValue(namedControl.Name, out _))
            throw new Exception($"Named Control with name {namedControl.Name} already exists!");

        Controls.AddOrUpdate(
            namedControl.Name, 
            k => namedControl, 
            (k, o) => o);
    }
    public IInputProvider Provider { get; }
    public float ScrollDelta => Provider.ScrollDelta;

    private readonly Dictionary<InputBinding, double> heldStartTimes = [];
    private readonly Dictionary<InputBinding, (int, double)> pressCounts = [];

    public InputContext(IInputProvider provider, int priority) : this(provider, priority, true) { }
    public InputContext(IInputProvider provider, int priority, bool autoRegister)
    {
        Priority = priority;
        Provider = provider;

        if(autoRegister)
            InputManager.RegisterContext(this);
    }


    /// <inheritdoc cref="IInputProvider.IsPressed(InputBinding)"/>
    public bool IsPressed(string controlName)
    {
        if (!IsActive)
            return false;

        if (!Controls.TryGetValue(controlName, out var control))
            throw new InvalidInputException(controlName);

        return control.IsPressed(Provider, out var binding) && HasRightFocus(binding); // Named controls assumed keyboard/gamepad
    }
    /// <inheritdoc cref="IInputProvider.IsDown(InputBinding)"/>
    public bool IsDown(string controlName)
    {
        if (!IsActive)
            return false;

        if (!Controls.TryGetValue(controlName, out var control))
            return false;

        return control.IsDown(Provider, out var binding) && HasRightFocus(binding);
    }
    /// <inheritdoc cref="IInputProvider.IsUp(InputBinding)"/>
    public bool IsUp(string controlName)
    {
        if (!IsActive)
            return false;

        if (!Controls.TryGetValue(controlName, out var control))
            return false;

        return control.IsUp(Provider, out var binding) && HasRightFocus(binding);
    }
    /// <inheritdoc cref="IInputProvider.IsPressed(InputBinding)"/>
    public bool IsPressed(KeyboardKey key)
        => IsPressed(new InputBinding(InputDeviceType.Keyboard, (int)key));
    /// <inheritdoc cref="IInputProvider.IsDown(InputBinding)"/>
    public bool IsDown(KeyboardKey key)
        => IsDown(new InputBinding(InputDeviceType.Keyboard, (int)key));
    /// <inheritdoc cref="IInputProvider.IsDown(InputBinding)"/>
    public bool IsUp(KeyboardKey key)
        => IsUp(new InputBinding(InputDeviceType.Keyboard, (int)key));
    /// <inheritdoc cref="IInputProvider.IsPressed(InputBinding)"/>
    public bool IsPressed(MouseButton button)
        => IsPressed(new InputBinding(InputDeviceType.Mouse, (int)button));
    /// <inheritdoc cref="IInputProvider.IsDown(InputBinding)"/>
    public bool IsDown(MouseButton button)
        => IsDown(new InputBinding(InputDeviceType.Mouse, (int)button));
    /// <inheritdoc cref="IInputProvider.IsUp(InputBinding)"/>
    public bool IsUp(MouseButton button)
        => IsUp(new InputBinding(InputDeviceType.Mouse, (int)button));

    /// <summary>
    /// Was this input repeated
    /// </summary>
    /// <param name="binding"></param>
    /// <returns></returns>
    public bool WasRepeated(InputBinding binding)
    {
        return WasRepeated(binding, 2);
    }
    /// <summary>
    /// Was this input repeated within the given timespan
    /// </summary>
    /// <param name="binding"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public bool WasRepeated(InputBinding binding, TimeSpan interval)
    {
        return WasRepeated(binding, 2, interval);
    }
    /// <summary>
    /// Was this input repeated the given amount of times
    /// </summary>
    /// <param name="binding"></param>
    /// <param name="times"></param>
    /// <returns></returns>
    public bool WasRepeated(InputBinding binding, int times)
    {
        return WasRepeated(binding, times, TimeSpan.FromSeconds(1)); // default window
    }
    /// <summary>
    /// Was this input repeated the given amount of times within the given timespan
    /// </summary>
    /// <param name="binding"></param>
    /// <param name="times"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public bool WasRepeated(InputBinding binding, int times, TimeSpan interval)
    {
        switch (binding.DeviceType)
        {
            case InputDeviceType.Keyboard:
                // Raylib already has a repeat for keys, but only single-step.
                // We'll simulate multi-repeat with timing.
                double now = Raylib.GetTime();

                if (IsPressed(binding))
                {
                    if (!pressCounts.TryGetValue(binding, out var state))
                        state = (0, 0);

                    int count = state.Item1 + 1;
                    double lastTime = state.Item2;

                    pressCounts[binding] = (count, now);

                    if (count >= times && now - lastTime <= interval.TotalSeconds)
                    {
                        // Reset counter after success
                        pressCounts[binding] = (0, now);
                        return true;
                    }
                }
                break;
        }

        return false;
    }
    /// <summary>
    /// Was this input held down for the given duration
    /// </summary>
    /// <param name="binding"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public bool HeldFor(InputBinding binding, TimeSpan duration)
    {
        double now = Raylib.GetTime();

        if (IsDown(binding))
        {
            if (!heldStartTimes.ContainsKey(binding))
                heldStartTimes[binding] = now;

            double heldTime = now - heldStartTimes[binding];
            return heldTime >= duration.TotalSeconds;
        }
        else
        {
            heldStartTimes.Remove(binding);
        }

        return false;
    }

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
        IsRequestingMouseFocus = ray.CheckCollisionPointRec(Provider.MousePosition, bounds);
    }
    internal bool IsMouseHovering(Rectangle bounds)
    {
        Console.WriteLine($"b:{bounds} --- m:{Provider.MousePosition} --- overshadowed:{HighestPriorityMouseAbove is not null} --- by:{HighestPriorityMouseAbove?.Priority}");
        if (HighestPriorityMouseAbove is not null)
            return false;
        
        if (ray.CheckCollisionPointRec(Provider.MousePosition, bounds))
            return true;
        return false;
    }
}


