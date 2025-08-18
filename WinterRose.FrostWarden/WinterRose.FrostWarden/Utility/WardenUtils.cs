using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden;

/// <summary>
/// Provides several utility and extension methods for use in the ForgeWarden engine
/// </summary>
public static class WardenUtils
{
    public static float Angle(this Quaternion a, Quaternion b)
    {
        float dot = MathF.Min(MathF.Abs(Quaternion.Dot(a, b)), 1f);
        return 2f * MathF.Acos(dot);
    }
}
