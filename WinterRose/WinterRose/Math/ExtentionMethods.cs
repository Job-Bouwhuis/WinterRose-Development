using System;
using System.Collections.Generic;

namespace WinterRose
{
    /// <summary>
    /// Math Extention Methods for all Snow needs
    /// </summary>
    public static partial class MathS
    {
        /// <summary>
        /// Finds the smallest float in the list of floats
        /// </summary>
        /// <param name="floats"></param>
        /// <returns>the smallest float in the list</returns>
        public static float Min(this List<float> floats)
        {
            float result = float.MaxValue;
            floats.Foreach(x => result = x < result ? x : result);
            return result;
        }
        /// <summary>
        /// Finds the largest float in the list of floats
        /// </summary>
        /// <param name="floats"></param>
        /// <returns>the largest float in the list</returns>
        public static float Max(this List<float> floats)
        {
            float result = float.MinValue;
            floats.Foreach(x => result = x > result ? x : result);
            return result;
        }
        /// <summary>
        /// floors the given double to the nearest integer
        /// </summary>
        /// <param name="num">This number will be converted to an integer</param>
        /// <returns>an integer floored from to the nearest possible whole number</returns>
        public static int FloorToInt(this double num) => (int)Math.Floor(num);
        /// <summary>
        /// floors the given float to the nearest integer
        /// </summary>
        /// <param name="num">This number will be converted to an integer</param>
        /// <returns>an integer floored from to the nearest possible whole number</returns>
        public static int FloorToInt(this float num) => (int)Math.Floor(num);

        /// <summary>
        /// Raises the given double to the nearest integer
        /// </summary>
        /// <param name="num">This number will be converted to an integer</param>
        /// <returns>an integer raised from to the nearest possible whole number</returns>
        public static int CeilingToInt(this double num) => (int)Math.Floor(num);
        /// <summary>
        /// Raises the given float to the nearest integer
        /// </summary>
        /// <param name="num">This number will be convertd to an integer</param>
        /// <returns>an integer raised from to the nearest possible whole number</returns>
        public static int CeilingToInt(this float num) => (int)Math.Ceiling(num);
        /// <summary>
        /// floors the given decumal to the nearest round integer
        /// </summary>
        /// <param name="num"></param>
        /// <returns>the floored number</returns>
        public static int FloorToInt(this decimal num) => (int)Math.Ceiling(num);
        /// <summary>
        /// Raises the given decimal to the nearest round integer
        /// </summary>
        /// <param name="num"></param>
        /// <returns>the raised number</returns>
        public static int CeilingToInt(this decimal num) => (int)Math.Ceiling(num);

        /// <summary>
        /// Clears the list and fills it with numbers starting at 0 up to count - 1
        /// </summary>
        /// <param name="nums"></param>
        /// <param name="count"></param>
        /// <returns><paramref name="nums"/></returns>
        public static List<int> ConsecutiveNumbers(this List<int> nums, int count)
        {
            nums.Clear();
            foreach(int i in count)
                nums.Add(i);                                                                                                            
            return nums;
        }
        /// <summary>
        /// Clears the list and fills it with numbers spanning between that of <paramref name="range"/>
        /// </summary>
        /// <param name="nums"></param>
        /// <param name="range"></param>
        /// <returns><paramref name="nums"/></returns>
        public static List<int> ConsecutiveNumbers(this List<int> nums, Range range)
        {
            nums.Clear();
            foreach(int i in range)
                nums.Add(i);
            return nums;
        }
    }
}
