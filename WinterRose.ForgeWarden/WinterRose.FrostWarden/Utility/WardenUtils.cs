using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    extension(Quaternion a)
    {
        public Vector3 EulerAngles => throw new NotImplementedException();

        public float Angle(Quaternion b)
        {
            float dot = MathF.Min(MathF.Abs(Quaternion.Dot(a, b)), 1f);
            return 2f * MathF.Acos(dot);
        }
    }

    extension(Math)
    {
        public static string ToStringFixedDecimals(double value, int decimals)
        {
            double rounded = Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            return rounded.ToString($"F{decimals}");
        }
    }

    extension(Vector2 vec)
    {
        public static bool operator <(Vector2 a, Vector2 b) => a.X < b.X && a.Y < b.Y;
        public static bool operator >(Vector2 a, Vector2 b) => a.X > b.X && a.Y > b.Y;
    }

    extension(Vector3 vec)
    {
        public Vector2 Vec2() => new(vec.X, vec.Y);

        public Vector3 Round(int decimals = 0)
        {
            return new Vector3(vec.X.Round(decimals), vec.Y.Round(decimals), vec.Z.Round(decimals));
        }
        public Vector3 RoundFixed(int decimals = 0)
        {
            float factor = (float)Math.Pow(10, decimals);
            return new Vector3(
                (float)Math.Round(vec.X * factor, MidpointRounding.AwayFromZero) / factor,
                (float)Math.Round(vec.Y * factor, MidpointRounding.AwayFromZero) / factor,
                (float)Math.Round(vec.Z * factor, MidpointRounding.AwayFromZero) / factor
            );
        }

        public string ToStringFixed(int decimals = 0)
        {
            return $"<{vec.X.ToString($"F{decimals}", CultureInfo.InvariantCulture)}, " +
                   $"{vec.Y.ToString($"F{decimals}", CultureInfo.InvariantCulture)}, " +
                   $"{vec.Z.ToString($"F{decimals}", CultureInfo.InvariantCulture)}>";
        }
    }


    

    extension (Color c)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="alpha">a float value 0-1</param>
        /// <returns></returns>
        public Color WithAlpha(float alpha)
            => new Color(c.R, c.G, c.B, (byte)Math.Clamp(c.A * alpha, 0f, 255f));

        public Color WithAlpha(byte alpha) => new(c.R, c.G, c.B, alpha);
        public Color WithAlpha(int alpha)
        {
            Forge.Expect(alpha).GreaterThan(0);
            Forge.Expect(alpha).LessThan(256);
            return WithAlpha(c, (byte)alpha);
        }

        public static bool operator ==(Color a, Color b)
        {
            return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        public static bool operator !=(Color a, Color b) => !(a == b);
    }
}
