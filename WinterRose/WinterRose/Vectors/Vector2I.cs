using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Net.Http.Headers;
using System.Numerics;

namespace WinterRose.Vectors
{
    /// <summary>
    /// A vector2 with integer values
    /// </summary>
    [DebuggerDisplay("X: {x} - Y: {y}")]
    public struct Vector2I : INumber<Vector2I>
    {
        /// <summary>
        /// Represents (0, 0)
        /// </summary>
        private static readonly Vector2I zero = new();
        /// <summary>
        /// Represents (0, 0)
        /// </summary>
        private static readonly Vector2I one = new(1, 1);
        /// <summary>
        /// The X component of the vector
        /// </summary>
        public int x;
        /// <summary>
        /// The Y component of the vector
        /// </summary>
        public int y;

        public static Vector2I One => one;
        public static int Radix => 10;
        public static Vector2I Zero => zero;
        public static Vector2I AdditiveIdentity => Zero;
        public static Vector2I MultiplicativeIdentity => One;

        /// <summary>
        /// The magnitude of the vector
        /// </summary>
        public int Magnitude
        {
            get
            {
                return (int)Math.Sqrt(x * x + y * y);
            }
        }

        public Vector2I() : this(0) { }
        public Vector2I(int xy) => x = y = xy;
        public Vector2I(int x, int y) : this(x)
        {
            this.y = y;
        }
        public Vector2I(Vector2 vec)
        {
            x = (int)vec.x;
            y = (int)vec.y;
        }

        public (int x, int y) Break() => (x, y);
        public void Deconstruct(out int x, out int y) => (x, y) = Break();
        public override string ToString()
        {
            return $"X: {x}, Y: {y}";
        }

        public int CompareTo(object? obj)
        {
            if (obj is Vector2I vec)
            {
                return CompareTo(vec);
            }
            return 0;
        }

        public int CompareTo(Vector2I other)
        {
            if (x == other.x && y == other.y)
                return 0;
            if (x > other.x && y > other.y)
                return 1;
            return -1;
        }

        public static Vector2I Abs(Vector2I value)
        {
            Vector2I result = new Vector2I(Math.Abs(value.x), Math.Abs(value.y));
            return result;
        }

        public static bool IsCanonical(Vector2I value)
        {
            if (value.x > 0 && value.y > 0)
                return true;
            return false;
        }

        public static bool IsComplexNumber(Vector2I value)
        {
            return false;
        }

        public static bool IsEvenInteger(Vector2I value)
        {
            if (value.x % 2 == 0 && value.y % 2 == 0)
                return true;
            return false;
        }

        public static bool IsFinite(Vector2I value)
        {
            if (value.x != int.MaxValue && value.y != int.MaxValue)
                return true;
            return false;
        }

        public static bool IsImaginaryNumber(Vector2I value)
        {
            return false;
        }

        public static bool IsInfinity(Vector2I value)
        {
            return false;
        }

        public static bool IsInteger(Vector2I value)
        {
            return true;
        }

        public static bool IsNaN(Vector2I value)
        {
            return false;
        }

        public static bool IsNegative(Vector2I value)
        {
            if (value.x < 0 && value.y < 0)
                return true;
            return false;
        }

        public static bool IsNegativeInfinity(Vector2I value)
        {
            return false;
        }

        public static bool IsNormal(Vector2I value)
        {
            return true;
        }

        public static bool IsOddInteger(Vector2I value)
        {
            return !IsEvenInteger(value);
        }

        public static bool IsPositive(Vector2I value)
        {
            if (value.x > 0 && value.y > 0)
                return true;
            return false;
        }

        public static bool IsPositiveInfinity(Vector2I value)
        {
            return false;
        }

        public static bool IsRealNumber(Vector2I value)
        {
            return true;
        }

        public static bool IsSubnormal(Vector2I value)
        {
            if (value.x < 0 && value.y < 0)
                return true;
            return false;
        }

        public static bool IsZero(Vector2I value)
        {
            return value == Zero;
        }

        public static Vector2I MaxMagnitude(Vector2I x, Vector2I y)
        {
            if (x.Magnitude > y.Magnitude)
                return x;
            return y;
        }

        public static Vector2I MaxMagnitudeNumber(Vector2I x, Vector2I y)
        {
            if (x.Magnitude > y.Magnitude)
                return x;
            return y;
        }

        public static Vector2I MinMagnitude(Vector2I x, Vector2I y)
        {
            if (x.Magnitude < y.Magnitude)
                return x;
            return y;
        }

        public static Vector2I MinMagnitudeNumber(Vector2I x, Vector2I y)
        {
            if (x.Magnitude < y.Magnitude)
                return x;
            return y;
        }

        public static Vector2I Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        {
            bool foundOpener = false;
            bool foundCloser = false;
            bool isXNegative = false;
            bool isYNegative = false;
            bool foundComma = false;
            int x = 0;
            int y = 0;
            string current = "";

            for (int i = 0; i < s.Length; i++)
            {
                if (i is 0 && s[i] is '{')
                {
                    foundOpener = true;
                    continue;
                }
                if (s[i] is '}')
                {
                    if(!foundOpener)
                        throw new FormatException("Invalid format for Vector2I");
                    if(i != s.Length - 1)
                        throw new FormatException("Invalid format for Vector2I");
                    foundCloser = true;
                    y = int.Parse(current);
                    continue;
                }
                if (!foundOpener)
                    throw new FormatException("Invalid format for Vector2I");

                char c = s[i];
                if (c is '-')
                {
                    if (foundComma)
                        isYNegative = true;
                    else
                        isXNegative = true;
                    continue;
                }
                if (c is ',')
                {
                    if (foundComma)
                        throw new FormatException("Invalid format for Vector2I");
                    foundComma = true;
                    x = int.Parse(current);
                    current = "";
                    continue;
                }
                if (char.IsDigit(c))
                {
                    current += c;
                    continue;
                }
                throw new FormatException("Invalid format for Vector2I");

            }

            if (!foundOpener || !foundCloser)
                throw new FormatException("Invalid format for Vector2I");

            if (isXNegative)
                x *= -1;
            if (isYNegative)
                y *= -1;

            return new Vector2I(x, y);
        }

        public static Vector2I Parse(string s, NumberStyles style, IFormatProvider? provider)
        {
            return Parse(s.AsSpan(), style, provider);
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector2I result)
        {
            try
            {
                result = Parse(s, style, provider);
                return true;
            }
            catch
            {
                result = Zero;
                return false;
            }
        }

        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector2I result)
        {
            try
            {
                if(s is null)
                {
                    result = Zero;
                    return false;
                }
                result = Parse(s, style, provider);
                return true;
            }
            catch
            {
                result = Zero;
                return false;
            }
        }

        public bool Equals(Vector2I other)
        {
            return this == other;
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            try
            {
                var span = ToString().AsSpan();
                span.TryCopyTo(destination);
                charsWritten = span.Length;
                return true;
            }
            catch
            {
                charsWritten = 0;
                return false;
            }
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return ToString();
        }

        public static Vector2I Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            return Parse(s, NumberStyles.Integer, provider);
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector2I result)
        {
            return TryParse(s, NumberStyles.Integer, provider, out result);
        }

        public static Vector2I Parse(string s, IFormatProvider? provider)
        {
            return Parse(s.AsSpan(), NumberStyles.Integer, provider);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector2I result)
        {
            return TryParse(s, NumberStyles.Integer, provider, out result);
        }

        static bool INumberBase<Vector2I>.TryConvertFromChecked<TOther>(TOther value, out Vector2I result)
        {
            {
                if (value is string s)
                {
                    result = Parse(s, NumberStyles.Integer, null);
                    return true;
                }
            }
            {
                if (value is Vector2I v)
                {
                    result = v;
                    return true;
                }
            }
            {
                if (value is Vector2 vec)
                {
                    result = new Vector2I((int)vec.x, (int)vec.y);
                    return true;
                }
            }
            {
                if (value is Vector3 vec3)
                {
                    result = new Vector2I((int)vec3.x, (int)vec3.y);
                    return true;
                }
            }
            {
                if (value is System.Numerics.Vector2 vec2)
                {
                    result = new Vector2I((int)vec2.X, (int)vec2.Y);
                    return true;
                }
            }
            {
                if (value is System.Numerics.Vector3 vec3)
                {
                    result = new Vector2I((int)vec3.X, (int)vec3.Y);
                    return true;
                }
            }
            {
                if (value is System.Numerics.Vector4 vec4)
                {
                    result = new Vector2I((int)vec4.X, (int)vec4.Y);
                    return true;
                }
            }

            result = Zero;
            return false;
        }

        static bool INumberBase<Vector2I>.TryConvertFromSaturating<TOther>(TOther value, out Vector2I result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<Vector2I>.TryConvertFromTruncating<TOther>(TOther value, out Vector2I result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<Vector2I>.TryConvertToChecked<TOther>(Vector2I value, out TOther result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<Vector2I>.TryConvertToSaturating<TOther>(Vector2I value, out TOther result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<Vector2I>.TryConvertToTruncating<TOther>(Vector2I value, out TOther result)
        {
            throw new NotImplementedException();
        }

        public static Vector2I operator +(Vector2I a, Vector2I b) => new(a.x + b.x, a.y + b.y);
        public static Vector2I operator -(Vector2I a, Vector2I b) => new(a.x - b.x, a.y - b.y);
        public static Vector2I operator *(Vector2I a, Vector2I b) => new(a.x * b.x, a.y * b.y);
        public static Vector2I operator /(Vector2I a, Vector2I b) => new(a.x / b.x, a.y / b.y);
        public static Vector2I operator %(Vector2I a, Vector2I b) => new(a.x % b.x, a.y % b.y);
        public static Vector2I operator +(Vector2I a, int b) => new(a.x + b, a.y + b);
        public static Vector2I operator -(Vector2I a, int b) => new(a.x - b, a.y - b);
        public static Vector2I operator *(Vector2I a, int b) => new(a.x * b, a.y * b);
        public static Vector2I operator /(Vector2I a, int b) => new(a.x / b, a.y / b);
        public static Vector2I operator %(Vector2I a, int b) => new(a.x % b, a.y % b);

        public static explicit operator Vector2I(Vector2 vec) => new(vec);
        public static implicit operator Vector2(Vector2I vec) => new(vec.x, vec.y);
        public static implicit operator Point(Vector2I vec) => new(vec.x, vec.y);
        public static explicit operator Vector2I(Point p) => new(p.X, p.Y);
        public static implicit operator System.Numerics.Vector2(Vector2I vec) => new(vec.x, vec.y);
        public static explicit operator Vector2I(System.Numerics.Vector2 vec) => new((int)vec.X, (int)vec.Y);
        public static implicit operator Vector2I((int x, int y) vec) => new(vec.x, vec.y);

        public static bool operator >(Vector2I left, Vector2I right)
        {
            if(left.x > right.x)
            {
                return left.y > right.y;
            }
            return false;
        }

        public static bool operator >=(Vector2I left, Vector2I right)
        {
            return left.x >= right.x && left.y >= right.y;
        }

        public static bool operator <(Vector2I left, Vector2I right)
        {
            return left.x < right.x && left.y < right.y;
        }

        public static bool operator <=(Vector2I left, Vector2I right)
        {
            return left.x <= right.x && left.y <= right.y;
        }

        public static Vector2I operator --(Vector2I value)
        {
            value.x--;
            value.y--;
            return value;
        }

        public static bool operator ==(Vector2I left, Vector2I right)
        {
            return left.x == right.x && left.y == right.y;
        }

        public static bool operator !=(Vector2I left, Vector2I right)
        {
            return left.x != right.x || left.y != right.y;
        }

        public static Vector2I operator ++(Vector2I value)
        {
            value.x++;
            value.y++;
            return value;
        }

        public static Vector2I operator -(Vector2I value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

        public static Vector2I operator +(Vector2I value)
        {
            return value;
        }
    }
}
