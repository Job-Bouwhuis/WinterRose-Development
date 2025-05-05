using WinterRose.WIP.TestClasses;
using System;

namespace WinterRose.Vectors
{
    /// <summary>
    /// An object that represents a position in 3D worldspace
    /// </summary>
    public struct Vector3
    {
        /// <summary>
        /// Vector3 coördinate
        /// </summary>
        public float x, y, z;
        /// <summary>
        /// (0,0,0)
        /// </summary>
        public static Vector3 Zero { get; } = new Vector3(0, 0, 0);
        /// <summary>
        /// (1,1,1)
        /// </summary>
        public static Vector3 One { get; } = new Vector3(1, 1, 1);
        /// <summary>
        /// (0,1,0)
        /// </summary>
        public static Vector3 UnitY { get; } = new Vector3(0, 1, 0);
        /// <summary>
        /// (1,0,0)
        /// </summary>
        public static Vector3 UnitX { get; } = new Vector3(1, 0, 0);
        /// <summary>
        /// (0,0,1)
        /// </summary>
        public static Vector3 UnitZ { get; } = new Vector3(0, 0, 1);

        /// <summary>
        /// poppulation constructor
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        /// <summary>
        /// empty constructor
        /// </summary>
        public Vector3()
        {
            x = 0;
            y = 0;
            z = 0;
        }
        /// <summary>
        /// Returns a string representation of the Vector3
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"X:{x} - Y:{y} - Z:{z}";
        }

        /// <summary>
        /// generates a vector3 with random values
        /// </summary>
        /// <returns>new vector3 where the values xyz are randomized</returns>
        public static Vector3 Random(int min = 0, int max = int.MaxValue) => new Vector3(new Random().Next(min, max), new Random().Next(min, max), new Random().Next(min, max));
        /// <summary>
        /// gets the distance between 2 vector3 points
        /// </summary>
        /// <returns>a float with the distance between the 2 points in a straight line</returns>
        public static float Distance(Vector3 v1, Vector3 v2) => MathF.Sqrt(MathF.Pow(v1.x - v2.x, 2) + MathF.Pow(v1.y - v2.y, 2) + MathF.Pow(v1.z - v2.z, 2));

        public static Vector3 Normalize(Vector3 vec)
        {
            // Get the length
            float length = MathF.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);

            // Divide each component by the length
            return new Vector3(vec.x / length, vec.y / length, vec.z / length);
        }

        public static Vector3 Cross(Vector3 front, Vector3 up)
        {
            // Calculate the cross product
            return new Vector3(
                               front.y * up.z - front.z * up.y,
                               front.z * up.x - front.x * up.z,
                               front.x * up.y - front.y * up.x);
        }

        #region Operators
        public static implicit operator System.Numerics.Vector3(Vector3 v) => new System.Numerics.Vector3(v.x, v.y, v.z);
        public static implicit operator Vector3(System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);
        /// <summary>
        /// Adds 2 Vector3 together
        /// </summary>
        /// <returns>a new vector3 where the 2 values are added together</returns>
        public static Vector3 operator +(Vector3 v1, Vector3 v2) => new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        /// <summary>
        /// adds a float value to a vector3
        /// </summary>
        /// <returns>a new vector3 where the float has been added to the original values</returns>
        public static Vector3 operator +(Vector3 v, float f) => new Vector3(v.x + f, v.y + f, v.z + f);
        /// <summary>
        /// subtracts 2 Vector3 from eachother
        /// </summary>
        /// <returns>a new vector3 where the 2 values are subtracted</returns>
        public static Vector3 operator -(Vector3 v1, Vector3 v2) => new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        /// <summary>
        /// subtracts a float value from a vector3
        /// </summary>
        /// <returns>a new vector3 where the float has been subtracted from the original values</returns>
        public static Vector3 operator -(Vector3 v, float f) => new Vector3(v.x - f, v.y - f, v.z - f);
        /// <summary>
        /// devides the values of 2 vector3
        /// </summary>
        /// <returns>a new vector3 where the values have been diveded with eachother</returns>
        public static Vector3 operator /(Vector3 v1, Vector3 v2) => new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        /// <summary>
        /// devides the values of a vector3 with a float
        /// </summary>
        /// <returns>a new vector3 where the values are devided by the float</returns>
        public static Vector3 operator /(Vector3 v, float f) => new Vector3(v.x / f, v.y / f, v.z / f);
        /// <summary>
        /// multiplies the values together
        /// </summary>
        /// <returns>a new vector3 where the values are multiplied by eachother</returns>
        public static Vector3 operator *(Vector3 v1, Vector3 v2) => new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        /// <summary>
        /// multiplies the values by a float
        /// </summary>
        /// <returns>a new vector3 where the values are multiplied by the float</returns>
        public static Vector3 operator *(Vector3 v, float f) => new Vector3(v.x * f, v.y * f, v.z * f);
        /// <summary>
        /// makes a new vector3 with the remainders of a devision
        /// </summary>
        /// <returns>a new vector3 where the values are the remainers of a division</returns>
        public static Vector3 operator %(Vector3 v1, Vector3 v2) => new Vector3(v1.x % v2.x, v1.y % v2.y, v1.z % v2.z);
        /// <summary>
        /// makes a new vector3 with the remainders of a devision
        /// </summary>
        ///<returns>a new vector3 where the values are the remainers of a division</returns>
        public static Vector3 operator %(Vector3 v, float f) => new Vector3(v.x % f, v.y % f, v.z % f);
        /// <summary></summary>
        /// <returns>true if left value is greater than the right value</returns>
        public static bool operator >(Vector3 v1, Vector3 v2) => v1.x > v2.x && v1.y > v2.y && v1.z > v2.z;
        /// <summary></summary>
        /// <returns>true if right value is greater than the left value</returns>
        public static bool operator <(Vector3 v1, Vector3 v2) => v1.x < v2.x && v1.y < v2.y && v1.z < v2.z;
        /// <summary></summary>
        /// <returns>true if left value is greater than the right value</returns>
        public static bool operator >(Vector3 v, float f) => v.x > f && v.y > f && v.z > f;
        /// <summary></summary>
        /// <returns>true if right value is greater than the left value</returns>
        public static bool operator <(Vector3 v, float f) => v.x < f && v.y < f && v.z < f;
        
        public static Vector3 operator +(Vector3 me, Vector2 other) => new Vector3(me.x + other.x, me.y + other.y, me.z);
        public static Vector3 operator -(Vector3 me, Vector2 other) => new Vector3(me.x - other.x, me.y - other.y, me.z);
        public static Vector3 operator *(Vector3 me, Vector2 other) => new Vector3(me.x * other.x, me.y * other.y, me.z);
        public static Vector3 operator /(Vector3 me, Vector2 other) => new Vector3(me.x / other.x, me.y / other.y, me.z);
        public static Vector3 operator %(Vector3 me, Vector2 other) => new Vector3(me.x % other.x, me.y % other.y, me.z);

        #endregion
    }
}
