using Raylib_cs;

namespace WinterRose.ForgeWarden.Input;

public class NamedControl
{
    /// <summary>
    /// e.g. "Forward", "Confirm", "Cancel"
    /// </summary>
    [field: WFInclude]
    public string Name { get; }
    internal List<InputBinding> Bindings { get; } = [];

    public NamedControl(string name)
    {
        Name = name;
    }

    private NamedControl() { } // for serialization

    internal bool IsPressed(IInputProvider provider)
    {
        foreach (var binding in Bindings)
            if (provider.IsPressed(binding))
                return true;
        return false;
    }

    internal bool IsDown(IInputProvider provider)
    {
        foreach (var binding in Bindings)
            if (provider.IsDown(binding))
                return true;
        return false;
    }

    internal bool IsUp(IInputProvider provider)
    {
        foreach (var binding in Bindings)
            if (provider.IsUp(binding))
                return true;
        return false;
    }

    // --- Value ---
    internal float GetValue(IInputProvider provider)
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

    public NamedControl AddBinding(InputBinding binding)
    {
        Bindings.Add(binding);
        return this;
    }

    public NamedControl AddBinding(KeyboardKey key)
    {
        Bindings.Add(new InputBinding(InputDeviceType.Keyboard, (int)key));
        return this;
    }

    public NamedControl AddBinding(MouseButton button)
    {
        Bindings.Add(new InputBinding(InputDeviceType.Mouse, (int)button));
        return this;
    }

    public void Register()
    {
        InputContext.RegisterNamedControl(this);
    }
}
