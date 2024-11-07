using WinterRose.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;
using WinterRose.SourceGeneration.Serialization;

namespace WinterRose.Vectors
{
    /// <summary>
    /// An object that represents a point in 2D worldspace
    /// </summary>
    [GenerateSerializer]
    public struct Vector2 : ICloneable, IEquatable<Vector2>, IComparable<Vector2>
    {
        /// <summary>
        /// Vector2 coördinate
        /// </summary>
        public float x, y;

        /// <summary>
        /// Defines a Vector2 with X and Y values at 0
        /// </summary>
        [ExcludeFromSerialization]
        public static readonly Vector2 Zero = new(0f, 0f);

        /// <summary>
        /// poppulated constructor
        /// </summary>
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        /// <summary>
        /// Creates a new instance of this Vector2 casting the doubles to floats
        /// </summary>
        public Vector2(double x, double y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }

        /// <summary>
        /// empty constructor
        /// </summary>
        public Vector2()
        {
            x = 0;
            y = 0;
        }

        /// <summary>
        /// Gets the magnitude of this vector
        /// </summary>
        public float Magnitude
        {
            get
            {
                return GetLength();
            }
        }

        /// <summary>
        /// Gets the normalized vector
        /// </summary>
        public Vector2 Normalized
        {
            get
            {
                Vector2 vec = (Vector2)Clone();
                vec.Normalize();
                return vec;
            }
        }

        /// <summary>
        /// Gets the length of the vector
        /// </summary>
        public float Length { get => MathF.Sqrt(x * x + y * y); }

        /// <summary>
        /// gets a string representation of this Vector2 object
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"X:{x}. Y:{y}";
        /// <summary>
        /// generates a vector2 with random values
        /// </summary>
        /// <returns>new vector2 where the values xy are randomized</returns>
        public static Vector2 Random(int min = 0, int max = int.MaxValue) => new Vector2(new Random().Next(min, max), new Random().Next(min, max));
        /// <summary>
        /// gets the distance between 2 vector2 points
        /// </summary>
        /// <returns>a float with the distance between the 2 points in a straight line</returns>
        public static float Distance(Vector2 v1, Vector2 v2) => MathF.Sqrt(MathF.Pow(v1.x - v2.x, 2) + MathF.Pow(v1.y - v2.y, 2));

        /// <summary>
        /// Normalizes the vector
        /// </summary>
        /// <returns>The nomalized vector</returns>
        public void Normalize()
        {
            float num = 1f / MathF.Sqrt(x * x + y * y);
            x *= num;
            y *= num;
        }
        /// <summary>
        /// Normalizes the given vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Normalize(Vector2 v)
        {
            v.Normalize();
            return v;
        }

        /// <summary>
        /// Gets the length of this vector
        /// </summary>
        /// <returns></returns>
        public float GetLength()
        {
            return MathF.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Gets the length of this vector squared
        /// </summary>
        /// <returns></returns>
        public float GetLengthSquared()
        {
            return x * x + y * y;
        }

        /// <summary>
        /// Creates a dot product with the given vectors
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static float Dot(Vector2 value1, Vector2 value2)
        {
            return value1.x * value2.x + value1.y * value2.y;
        }

        /// <summary>
        /// Lerps the given vectors to the given amount of time
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <returns>The lerped Vector2</returns>
        public static Vector2 Lerp(Vector2 value1, Vector2 value2, float amount)
        {
            return new Vector2(MathS.Lerp(value1.x, value2.x, amount), MathS.Lerp(value1.y, value2.y, amount));
        }
        public static Vector2 SmoothStep(Vector2 value1, Vector2 value2, float amount)
        {
            return new Vector2(MathS.SmoothStep(value1.x, value2.x, amount), MathS.SmoothStep(value1.y, value2.y, amount));
        }

        /// <summary>
        /// Clones this vector2
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Vector2(x, y);
        }

        /// <summary>
        /// Checks if the given vector2 is equals to this one
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if it is, otherwise false</returns>
        public bool Equals(Vector2 other) => Equals(this, other) || this == other;

        /// <summary>
        /// Gets the hashcode for this object
        /// </summary>
        /// <returns>This objects hashcode</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Checks if the given vector2 is equals to this one
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if it is, otherwise false</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null) return false;
            if (obj is not Vector2) return false;
            return Equals((Vector2)obj);
        }

        public int CompareTo(Vector2 other)
        {
            // Calculate the magnitude (length) of both vectors
            float thisMagnitude = Length;
            float otherMagnitude = other.Length;

            // Compare the magnitudes and return accordingly
            if (thisMagnitude < otherMagnitude)
                return -1; // This vector is "smaller"
            else if (thisMagnitude > otherMagnitude)
                return 1;  // This vector is "larger"
            else
                return 0;  // Both vectors have the same magnitude
        }


        //all operator calculations    
        #region Operators
        /// <summary>
        /// Adds 2 Vector2 together
        /// </summary>
        /// <returns>a new vector2 where the 2 values are added together</returns>
        public static Vector2 operator +(Vector2 v1, Vector2 v2) => new Vector2(v1.x + v2.x, v1.y + v2.y);
        /// <summary>
        /// adds a float value to a vector2
        /// </summary>
        /// <returns>a new vector2 where the float has been added to the original values</returns>
        public static Vector2 operator +(Vector2 v, float f) => new Vector2(v.x + f, v.y + f);
        /// <summary>
        /// subtracts 2 Vector2 from eachother
        /// </summary>
        /// <returns>a new vector2 where the 2 values are subtracted</returns>
        public static Vector2 operator -(Vector2 v1, Vector2 v2) => new Vector2(v1.x - v2.x, v1.y - v2.y);
        /// <summary>
        /// subtracts a float value from a vector2
        /// </summary>
        /// <returns>a new vector2 where the float has been subtracted from the original values</returns>
        public static Vector2 operator -(Vector2 v, float f) => new Vector2(v.x - f, v.y - f);
        /// <summary>
        /// devides the values of 2 vector2
        /// </summary>
        /// <returns>a new vector2 where the values have been diveded with eachother</returns>
        public static Vector2 operator /(Vector2 v1, Vector2 v2) => new Vector2(v1.x / v2.x, v1.y / v2.y);
        /// <summary>
        /// devides the values of a vector2 with a float
        /// </summary>
        /// <returns>a new vector2 where the values are devided by the float</returns>
        public static Vector2 operator /(Vector2 v, float f) => new Vector2(v.x / f, v.y / f);
        /// <summary>
        /// multiplies the values together
        /// </summary> 
        /// <returns>a new vector2 where the values are multiplied by eachother</returns>
        public static Vector2 operator *(Vector2 v1, Vector2 v2) => new Vector2(v1.x * v2.x, v1.y * v2.y);
        /// <summary>
        /// multiplies the values by a float
        /// </summary>
        /// <returns>a new vector2 where the values are multiplied by the float</returns>
        public static Vector2 operator *(Vector2 v, float f) => new Vector2(v.x * f, v.y * f);
        /// <summary>
        /// makes a new vector3 with the remainders of a devision
        /// </summary>
        /// <returns>a new vector2 with the remainders of a devision as its values</returns>
        public static Vector2 operator %(Vector2 v1, Vector2 v2) => new Vector2(v1.x % v2.x, v1.y % v2.y);
        /// <summary></summary>
        ///<returns>a new vector2 where the values are the remainers of a division</returns>
        public static Vector2 operator %(Vector2 v, float f) => new Vector2(v.x % f, v.y % f);
        /// <summary></summary>
        /// <returns>true if left value is greater than the right value</returns>
        public static bool operator >(Vector2 v1, Vector2 v2) => v1.x > v2.x && v1.y > v2.y;
        /// <summary></summary>
        /// <returns>true if right value is greater than the left value</returns>
        public static bool operator <(Vector2 v1, Vector2 v2) => v1.x < v2.x && v1.y < v2.y;
        /// <summary></summary>
        /// <returns>true if left value is greater than the right value</returns>
        public static bool operator >(Vector2 v, float f) => v.x > f && v.y > f;
        /// <summary></summary>
        /// <returns>true if right value is greater than the left value</returns>
        public static bool operator <(Vector2 v, float f) => v.x < f && v.y < f;
        /// <summary></summary>
        /// <returns>true if left value is greater than, or equal the right value</returns>
        public static bool operator >=(Vector2 v1, Vector2 v2) => v1 > v2 || v1 == v2;
        /// <summary></summary>
        /// <returns>true if right value is greater than, or equal the left value</returns>
        public static bool operator <=(Vector2 v1, Vector2 v2) => v1 < v2 || v1 == v2;
        /// <summary></summary>
        /// <returns>Returns true if <paramref name="v1"/> its values are identical to <paramref name="v2"/></returns>
        public static bool operator ==(Vector2 v1, Vector2 v2) => v1.x == v2.x && v1.y == v2.y;
        /// <summary></summary>
        /// <returns>Returns true if <paramref name="v1"/> its values are identical to <paramref name="v2"/></returns>
        public static bool operator !=(Vector2 v1, Vector2 v2) => !(v1 == v2);

        public static Vector2 operator +(Vector2 me, Vector3 other) => new Vector2(me.x + other.x, me.y + other.y);
        public static Vector2 operator -(Vector2 me, Vector3 other) => new Vector2(me.x - other.x, me.y - other.y);
        public static Vector2 operator *(Vector2 me, Vector3 other) => new Vector2(me.x * other.x, me.y * other.y);
        public static Vector2 operator /(Vector2 me, Vector3 other) => new Vector2(me.x / other.x, me.y / other.y);
        public static Vector2 operator %(Vector2 me, Vector3 other) => new Vector2(me.x % other.x, me.y % other.y);


        #endregion
    }
}
