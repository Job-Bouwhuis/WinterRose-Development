using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.Input;


namespace VerdantRequiem.Configs.Controls;

internal class ControlBindings
{
    public static void BindInitial()
    {
        Reg("reload", KeyboardKey.R);
        Reg("fire", MouseButton.Left);
    }

    private static void Reg(NamedControl control) => InputContext.RegisterNamedControl(control);
    private static void Reg(string name, params Enum[] values) => Reg(Create(name, values));

    private static NamedControl Create(string name, params Enum[] values)
    {
        NamedControl control = new(name);

        foreach (Enum value in values)
            control.AddBinding(new InputBinding(ResolveDevice(value), Convert.ToInt32(value)));

        return control;
    }

    private static InputDeviceType ResolveDevice(Enum value)
    {
        Type enumType = value.GetType();

        if (enumType == typeof(KeyboardKey))
            return InputDeviceType.Keyboard;

        if (enumType == typeof(MouseButton))
            return InputDeviceType.Mouse;

        throw new InvalidOperationException($"Unsupported input enum type: {enumType.Name}");
    }
}
