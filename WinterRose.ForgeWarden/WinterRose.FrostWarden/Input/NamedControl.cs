using Raylib_cs;

namespace WinterRose.ForgeWarden.Input;

public class NamedControl
{
    public string Name { get; set; } // e.g. "Forward", "Confirm", "Cancel"
    public List<InputBinding> Bindings { get; set; } = new();

    // --- Pressed / Down / Up ---
    public bool IsPressed(IInputProvider provider)
    {
        foreach (var binding in Bindings)
            if (provider.IsPressed(binding))
                return true;
        return false;
    }

    public bool IsDown(IInputProvider provider)
    {
        foreach (var binding in Bindings)
            if (provider.IsDown(binding))
                return true;
        return false;
    }

    public bool IsUp(IInputProvider provider)
    {
        foreach (var binding in Bindings)
            if (provider.IsUp(binding))
                return true;
        return false;
    }

    // --- Value ---
    public float GetValue(IInputProvider provider)
    {
        float value = 0;
        foreach (var binding in Bindings)
        {
            if(binding.Relation == InputAxisRelation.Positive)
                value = Math.Max(value, provider.GetValue(binding));
            else
                value = Math.Min(value, provider.GetValue(binding));
        }
            
        return value;
    }

    public void AddBinding(InputBinding binding)
    {
        Bindings.Add(binding);
    }

    public void AddBinding(KeyboardKey key)
    {
        Bindings.Add(new InputBinding(InputDeviceType.Keyboard, (int)key));
    }

    public void AddBinding(MouseButton button)
    {
        Bindings.Add(new InputBinding(InputDeviceType.Mouse, (int)button));
    }
}
