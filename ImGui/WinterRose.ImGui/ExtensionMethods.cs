using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ImGuiApps;

namespace WinterRose.ImGuiApps;

public static class ExtensionMethods
{
    /// <summary>
    /// Converts a <see cref="System.Drawing.Color"/> to a <see cref="Color"/>
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static Color ToWRColor(this System.Drawing.Color color) => new(color.R, color.G, color.B, color.A);
    /// <summary>
    /// The <see cref="System.Numerics.Vector4"/> color is expected to be in the range of 0-1 when converting to a <see cref="Color"/>. Everything above 1 will be clamped.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static Color ToWRColor(this System.Numerics.Vector4 color)
    {
        byte R = (byte)(color.X * 255);
        byte G = (byte)(color.Y * 255);
        byte B = (byte)(color.Z * 255);
        byte A = (byte)(color.W * 255);
        return new Color(R, G, B, A);
    }

    /// <summary>
    /// Converts a <see cref="Color"/> to a <see cref="System.Drawing.Color"/>
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static System.Drawing.Color ToSystemColor(this Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    /// <summary>
    /// Converts a <see cref="Color"/> to a <see cref="System.Numerics.Vector4"/> with range 0-1
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static System.Numerics.Vector4 ToVector4(this Color color) => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

}
