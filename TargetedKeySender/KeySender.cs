using System.Runtime.InteropServices;
namespace TargetedKeySender;


public static class KeySender
{
    public static void SendKey(ushort virtualKey)
    {
        INPUT[] inputs = new INPUT[2];

        inputs[0] = new INPUT
        {
            type = InputType.Keyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    dwFlags = KeyEventF.KeyDown
                }
            }
        };

        inputs[1] = new INPUT
        {
            type = InputType.Keyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    dwFlags = KeyEventF.KeyUp
                }
            }
        };

        NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
}