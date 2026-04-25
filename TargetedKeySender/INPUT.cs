
using System.Runtime.InteropServices;

namespace TargetedKeySender;


[StructLayout(LayoutKind.Sequential)]
public struct INPUT
{
    public int type;
    public InputUnion U;
}
