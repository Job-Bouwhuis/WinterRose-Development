using Raylib_cs;

namespace WinterRose.ForgeWarden.Input;

public class RaylibInputProvider : IInputProvider
{
    private readonly int gamepadIndex;

    private readonly Dictionary<InputBinding, double> heldStartTimes = [];
    private readonly Dictionary<InputBinding, (int, double)> pressCounts = [];

    public Vector2 MousePosition => currentMousePosition;
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
        currentMousePosition = Raylib.GetMousePosition();
    }

    // --- PRESSED (this frame) ---
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

    // --- DOWN (held state) ---
    public bool IsDown(InputBinding binding)
    {
        switch (binding.DeviceType)
        {
            case InputDeviceType.Keyboard:
                return Raylib.IsKeyDown((KeyboardKey)binding.Code);

            case InputDeviceType.Mouse:
                return Raylib.IsMouseButtonDown((MouseButton)binding.Code);

            case InputDeviceType.GamepadButton:
                return Raylib.IsGamepadButtonDown(gamepadIndex, (GamepadButton)binding.Code);

            case InputDeviceType.GamepadAxis:
                float axis = Raylib.GetGamepadAxisMovement(gamepadIndex, (GamepadAxis)binding.Code);
                return binding.Relation == InputAxisRelation.Positive
                    ? axis > binding.Threshold
                    : axis < -binding.Threshold;

            default:
                return false;
        }
    }

    // --- UP (released this frame) ---
    public bool IsUp(InputBinding binding)
        => binding.DeviceType switch
        {
            InputDeviceType.Keyboard => (bool)Raylib.IsKeyReleased((KeyboardKey)binding.Code),
            InputDeviceType.Mouse => (bool)Raylib.IsMouseButtonReleased((MouseButton)binding.Code),
            InputDeviceType.GamepadButton => (bool)Raylib.IsGamepadButtonReleased(gamepadIndex, (GamepadButton)binding.Code),
            _ => false,
        };

    // --- VALUE (float axis or binary) ---
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

    public bool WasRepeated(InputBinding binding)
    {
        return WasRepeated(binding, 2);
    }

    public bool WasRepeated(InputBinding binding, TimeSpan interval)
    {
        return WasRepeated(binding, 2, interval);
    }

    public bool WasRepeated(InputBinding binding, int times)
    {
        return WasRepeated(binding, times, TimeSpan.FromSeconds(1)); // default window
    }

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


    // --- HELD FOR ---
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
}

