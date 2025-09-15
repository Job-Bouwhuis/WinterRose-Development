using Raylib_cs;
using System.Diagnostics.CodeAnalysis;

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

    internal bool IsPressed(IInputProvider provider, [NotNullWhen(true)] out InputBinding? binding)
    {
        foreach (var b in Bindings)
            if (provider.IsPressed(b))
            {
                binding = b;
                return true;
            }
        binding = null;
        return false;
    }

    internal bool IsDown(IInputProvider provider, [NotNullWhen(true)] out InputBinding? binding)
    {
        foreach (var b in Bindings)
            if (provider.IsDown(b))
            {
                binding = b;
                return true;
            }
        binding = null;
        return false;
    }

    internal bool IsUp(IInputProvider provider, [NotNullWhen(true)] out InputBinding? binding)
    {
        foreach (var b in Bindings)
            if (provider.IsUp(b))
            {
                binding = b;
                return true;
            }
        binding = null;
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
