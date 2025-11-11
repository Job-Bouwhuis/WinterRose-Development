using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks;

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

    public static Vector2 Vec2(this Vector3 vec) => new(vec.X, vec.Y);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <param name="alpha">a float value 0-1</param>
    /// <returns></returns>
    public static Color WithAlpha(this Color c, float alpha) 
        => new Color(c.R, c.G, c.B, (byte)Math.Clamp(c.A * alpha, 0f, 255f));

    public static Color WithAlpha(this Color c, byte alpha) => new(c.R, c.G, c.B, alpha);
    public static Color WithAlpha(this Color c, int alpha)
    {
        Forge.Expect(alpha).GreaterThan(0);
        Forge.Expect(alpha).LessThan(256);
        return WithAlpha(c, (byte)alpha);
    }
}
