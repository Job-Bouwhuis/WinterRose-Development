using System;
using System.Numerics;
namespace WinterRose;

public struct Int256 : IComparable<Int256>, IEquatable<Int256>
{
    private readonly ulong low;      // Lower 64 bits
    private readonly ulong midLow;   // Middle lower 64 bits
    private readonly ulong midHigh;  // Middle upper 64 bits
    private readonly ulong high;     // Upper 64 bits
    private readonly bool isNegative; // Sign

    public Int256(ulong low, ulong midLow, ulong midHigh, ulong high, bool isNegative = false)
    {
        this.low = low;
        this.midLow = midLow;
        this.midHigh = midHigh;
        this.high = high;
        this.isNegative = isNegative;
    }

    public Int256(int value)
    {
        low = (ulong)Math.Abs((long)value);
        midLow = 0;
        midHigh = 0;
        high = 0;
        isNegative = value < 0;
    }

    public Int256(long value)
    {
        low = (ulong)Math.Abs(value);
        midLow = 0;
        midHigh = 0;
        high = 0;
        isNegative = value < 0;
    }

    public Int256(ulong value)
    {
        low = value;
        midLow = 0;
        midHigh = 0;
        high = 0;
        isNegative = false;
    }

    // Equality check
    public bool Equals(Int256 other) => this == other;

    public override bool Equals(object obj) => obj is Int256 other && this == other;
    public override int GetHashCode() => HashCode.Combine(low, midLow, midHigh, high, isNegative);

    public static implicit operator Int256(int value) => new(value);
    public static implicit operator Int256(long value) => new(value);
    public static implicit operator Int256(ulong value) => new(value);

    // Addition
    public static Int256 operator +(Int256 a, Int256 b)
    {
        ulong carry = 0;

        ulong low = AddWithCarry(a.low, b.low, ref carry);
        ulong midLow = AddWithCarry(a.midLow, b.midLow, ref carry);
        ulong midHigh = AddWithCarry(a.midHigh, b.midHigh, ref carry);
        ulong high = AddWithCarry(a.high, b.high, ref carry);

        return new Int256(low, midLow, midHigh, high, a.isNegative); // Sign logic omitted
    }

    private static ulong AddWithCarry(ulong a, ulong b, ref ulong carry)
    {
        ulong result = a + b + carry;
        carry = (result < a || result < b) ? 1UL : 0UL;
        return result;
    }

    // Subtraction
    public static Int256 operator -(Int256 a, Int256 b)
    {
        ulong borrow = 0;

        ulong low = SubtractWithBorrow(a.low, b.low, ref borrow);
        ulong midLow = SubtractWithBorrow(a.midLow, b.midLow, ref borrow);
        ulong midHigh = SubtractWithBorrow(a.midHigh, b.midHigh, ref borrow);
        ulong high = SubtractWithBorrow(a.high, b.high, ref borrow);

        return new Int256(low, midLow, midHigh, high, a.isNegative); // Sign logic omitted
    }

    private static ulong SubtractWithBorrow(ulong a, ulong b, ref ulong borrow)
    {
        ulong result = a - b - borrow;
        borrow = (a < b + borrow) ? 1UL : 0UL;
        return result;
    }

    // Multiplication
    public static Int256 operator *(Int256 a, Int256 b)
    {
        BigInteger aBig = a.ToBigInteger();
        BigInteger bBig = b.ToBigInteger();

        BigInteger product = aBig * bBig;

        return FromBigInteger(product);
    }
    public static Int256 operator ++(Int256 value) => value + 1;
    public static Int256 operator --(Int256 value) => value - 1;

    // Division
    public static Int256 operator /(Int256 a, Int256 b)
    {
        BigInteger aBig = a.ToBigInteger();
        BigInteger bBig = b.ToBigInteger();

        BigInteger quotient = aBig / bBig;

        return FromBigInteger(quotient);
    }

    // Modulus
    public static Int256 operator %(Int256 a, Int256 b)
    {
        BigInteger aBig = a.ToBigInteger();
        BigInteger bBig = b.ToBigInteger();

        BigInteger remainder = aBig % bBig;

        return FromBigInteger(remainder);
    }

    // Bitwise AND
    public static Int256 operator &(Int256 a, Int256 b)
    {
        return new Int256(
            a.low & b.low,
            a.midLow & b.midLow,
            a.midHigh & b.midHigh,
            a.high & b.high,
            a.isNegative & b.isNegative
        );
    }

    // Bitwise OR
    public static Int256 operator |(Int256 a, Int256 b)
    {
        return new Int256(
            a.low | b.low,
            a.midLow | b.midLow,
            a.midHigh | b.midHigh,
            a.high | b.high,
            a.isNegative | b.isNegative
        );
    }

    // Bitwise XOR
    public static Int256 operator ^(Int256 a, Int256 b)
    {
        return new Int256(
            a.low ^ b.low,
            a.midLow ^ b.midLow,
            a.midHigh ^ b.midHigh,
            a.high ^ b.high,
            a.isNegative ^ b.isNegative
        );
    }

    // Bitwise NOT
    public static Int256 operator ~(Int256 a)
    {
        return new Int256(
            ~a.low,
            ~a.midLow,
            ~a.midHigh,
            ~a.high,
            !a.isNegative
        );
    }

    // Left Shift
    public static Int256 operator <<(Int256 a, int shift)
    {
        if (shift >= 256) return new Int256(0, 0, 0, 0);
        if (shift == 0) return a;

        int chunkShift = shift / 64;
        int bitShift = shift % 64;

        ulong[] chunks = { a.low, a.midLow, a.midHigh, a.high };
        ulong[] result = new ulong[4];

        for (int i = 3; i >= chunkShift; i--)
        {
            result[i] = chunks[i - chunkShift] << bitShift;

            if (bitShift > 0 && i - chunkShift - 1 >= 0)
                result[i] |= chunks[i - chunkShift - 1] >> (64 - bitShift);
        }

        return new Int256(result[0], result[1], result[2], result[3], a.isNegative);
    }

    // Equality Operators
    public static bool operator ==(Int256 a, Int256 b)
    {
        return a.high == b.high && a.midHigh == b.midHigh &&
               a.midLow == b.midLow && a.low == b.low &&
               a.isNegative == b.isNegative;
    }

    public static bool operator !=(Int256 a, Int256 b) => !(a == b);

    // Comparison Operators
    public static bool operator <(Int256 a, Int256 b)
    {
        if (a.isNegative != b.isNegative)
            return a.isNegative;

        if (a.high != b.high)
            return a.high < b.high;
        if (a.midHigh != b.midHigh)
            return a.midHigh < b.midHigh;
        if (a.midLow != b.midLow)
            return a.midLow < b.midLow;
        return a.low < b.low;
    }

    public static bool operator >(Int256 a, Int256 b) => b < a;
    public static bool operator <=(Int256 a, Int256 b) => !(a > b);
    public static bool operator >=(Int256 a, Int256 b) => !(a < b);

    // Helper: Convert to BigInteger
    private BigInteger ToBigInteger()
    {
        BigInteger result = high;
        result = (result << 64) + midHigh;
        result = (result << 64) + midLow;
        result = (result << 64) + low;

        if (isNegative)
            result = -result;

        return result;
    }

    // Helper: Convert from BigInteger
    private static Int256 FromBigInteger(BigInteger value)
    {
        bool isNegative = value < 0;
        value = BigInteger.Abs(value);

        ulong low = (ulong)(value & ulong.MaxValue);
        value >>= 64;
        ulong midLow = (ulong)(value & ulong.MaxValue);
        value >>= 64;
        ulong midHigh = (ulong)(value & ulong.MaxValue);
        value >>= 64;
        ulong high = (ulong)value;

        return new Int256(low, midLow, midHigh, high, isNegative);
    }

    // ToString
    public override string ToString()
    {
        return $"{(isNegative ? "-" : "")}{high:X16}{midHigh:X16}{midLow:X16}{low:X16}";
    }

    // IComparable and IEquatable
    public int CompareTo(Int256 other)
    {
        if (this < other) return -1;
        if (this > other) return 1;
        return 0;
    }

    public ulong ToUInt64() => low;
}
