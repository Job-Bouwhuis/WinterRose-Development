using System.Runtime.InteropServices;

namespace TargetedKeySender;

[StructLayout(LayoutKind.Explicit)]
public struct InputUnion
{
    [FieldOffset(0)]
    public KEYBDINPUT ki;
}