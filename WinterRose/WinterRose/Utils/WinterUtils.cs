#if !WINTERROSE
#define WINTERROSE
#endif

using System.Collections;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using WinterRose.FileManagement;

namespace WinterRose
{
    /// <summary>
    /// Suggestions for this class are welcome. please report them the the Author, <b>TheSnowOwl</b>
    /// </summary>
    public static partial class WinterUtils
    {
        public static long RandomLongID
        {
            get
            {
                return new Random(new Random(DateTime.Now.TimeOfDay.TotalSeconds.FloorToInt()).Next()).NextInt64();
            }
        }

        /// <summary>
        /// Gets whether the given <paramref name="num"/> is inside the <paramref name="range"/>
        /// </summary>
        /// <param name="range"></param>
        /// <param name="num"></param>
        /// <returns>True if <paramref name="num"/> is within the <paramref name="range"/>, otherwise false</returns>
        public static bool Contains(this Range range, int num)
        {
            if (num > range.Start.Value && num < range.End.Value)
                return true;
            return false;
        }

        /// <summary>
        /// Checks if the given <paramref name="str"/> ends with any of the given <paramref name="options"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool EndsWith(this string str, params string[] options)
        {
            foreach (var option in options)
                if (str.EndsWith(option))
                    return true;
            return false;
        }

        /// <summary>
        /// Checks if the given <paramref name="str"/> starts with any of the given <paramref name="options"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool StartsWith(this string str, params string[] options)
        {
            foreach (var option in options)
                if (str.StartsWith(option))
                    return true;
            return false;
        }

        /// <summary>
        /// Tries to open a directory with name <paramref name="name"/> inside <paramref name="parent"/>.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="child"></param>
        /// <returns>True if the child was found and opened, otherwise false </returns>
        public static bool OpenChildDirectory(this DirectoryInfo parent, string name, out DirectoryInfo child)
        {
            string childPath = Path.Combine(parent.FullName, name);
            child = new DirectoryInfo(childPath);
            return child.Exists;
        }

        /// <summary>
        /// Checks whether the given <paramref name="type"/> is an anonymous type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type type)
        {
            return type.Name.Contains("<>f__AnonymousType");
        }

        /// <summary>
        /// Gets a random float between the given <paramref name="min"/> and <paramref name="max"/> values
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float NextFloat(this Random random, float min, float max)
        {
            float f = (float)random.NextDouble() * (max - min) + min;
            return f;
        }

        /// <summary>
        /// Reads the file
        /// </summary>
        /// <param name="info"></param>
        /// <returns>The content of the file. if the file can not be read due to being readonly or in use by another process, returns "File In Use"</returns>
        public static string Read(this FileInfo info)
        {
            try
            {

                return FileManager.Read(info.FullName);
            }
            catch
            {
                return "File In Use";
            }
        }

        /// <summary>
        /// Checks whether all the given <paramref name="values"/> exist in the given <paramref name="collection"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool Contains<T>(this List<T> collection, params T[] values)
        {
            foreach (var val in values)
                if (!collection.Contains(val))
                    return false;
            return true;
        }


        /// <summary>
        /// Matches the given <paramref name="str1"/> with the given <paramref name="str2"/> with fuzzy matching
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <param name="maxErr">The maximum percentage errors allowed. Can help to find more or less results</param>
        /// <returns>The matches found. Can have up to </returns>
        public static int FuzzyMatchString(string str1, string str2)
        {
            int[,] distance = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
                distance[i, 0] = i;

            for (int j = 0; j <= str2.Length; j++)
                distance[0, j] = j;

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            int maxLength = Math.Max(str1.Length, str2.Length);
            return 100 - (distance[str1.Length, str2.Length] * 100) / maxLength;
        }

        /// <summary>
        /// Rounds the given float <paramref name="f"/>
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int Round(this float f, int digits = 0 , MidpointRounding roundingMode = MidpointRounding.ToZero) => 
            (int)float.Round(f, digits, roundingMode);
        /// <summary>
        /// Rounds the given double <paramref name="d"/>
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int Round(this double d) => (int)double.Round(d);

        public static List<T> AddMany<T>(this List<T> values, params T[] toAdd)
        {
            foreach (var val in toAdd)
                values.Add(val);
            return values;
        }

        /// <summary>
        /// Converts the given <see cref="byte"/> array to a <see cref="Bitmap"/>. To retrieve the data stored use <see cref="ConvertToBytes(Bitmap)"/> This method is only available on Windows 7 or higher
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static Bitmap ConvertToBitmap(this byte[] inputBytes, ProgressReporter? progress = null)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                throw new OperationCanceledException("This method requires Windows 7 or higher");

            // Calculate the number of pixels needed based on the input byte array length
            int pixelCount = (int)Math.Ceiling((double)inputBytes.Length / 3);

            // Calculate the width and height of the bitmap to create a square texture
            int textureSize = (int)Math.Ceiling(Math.Sqrt(pixelCount));
            int textureWidth = textureSize;
            int textureHeight = (int)Math.Ceiling((double)pixelCount / textureSize);

            // Create a new bitmap with dimensions equal to the texture size
            Bitmap bitmap = new Bitmap(textureWidth, textureHeight);

            // Copy the input bytes into the bitmap
            int inputIndex = 0;
            for (int y = 0; y < textureHeight; y++)
            {
                Console.WriteLine($"HorizontalLine: {y}/{textureHeight}");
                for (int x = 0; x < textureWidth; x++)
                {
                    // Check if we have reached the end of the input array
                    if (inputIndex >= inputBytes.Length)
                    {
                        // Set the color of the current pixel to black
                        bitmap.SetPixel(x, y, Color.Black);
                    }
                    else
                    {

                        byte b1 = inputBytes[inputIndex];
                        byte b2 = inputBytes.Length > inputIndex + 1 ? inputBytes[inputIndex + 1] : (byte)0;
                        byte b3 = inputBytes.Length > inputIndex + 2 ? inputBytes[inputIndex + 2] : (byte)0;

                        // Create a new color with the pixel bytes
                        Color pixelColor = Color.FromArgb(255, b1, b2, b3);

                        // Check if any of the pixel bytes are 0
                        if (pixelColor.ToArgb() != 0)
                        {
                            // Set the color of the current pixel
                            bitmap.SetPixel(x, y, pixelColor);
                        }
                        else
                        {
                            // Decrement the input index by 3 to retry with the previous bytes
                            inputIndex -= 3;

                            // Set the color of the current pixel to black
                            bitmap.SetPixel(x, y, Color.Black);
                        }

                        // Increment the input index by 3 to move to the next set of bytes
                        inputIndex += 3;
                    }
                }
            }

            return bitmap;
        }
        /// <summary>
        /// Converts the <see cref="Bitmap"/> to a <see cref="byte"/> array. this method makes usable data when the bitmap is created using <see cref="ConvertToBitmap(byte[], ProgressReporter)"/>. This method is only available on windows 7 or higher
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] ConvertToBytes(this Bitmap bitmap)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                throw new OperationCanceledException("This method requires Windows 7 or higher");
            int width = bitmap.Width;
            int height = bitmap.Height;

            Color emptyColor = Color.FromArgb(0, 0, 0, 0);

            //byte[] outputArray = new byte[width * height * 4];
            List<byte> bytes = new();

            int pixelX = 0;
            int pixelY = 0;

            for (int i = 0; i < height; i++)
            {
                Console.WriteLine(i);
                for (int j = 0; j < width; j++)
                {
                    Color color = bitmap.GetPixel(pixelX, pixelY);

                    //bytes.Add(color.A);
                    bytes.Add(color.R);
                    bytes.Add(color.G);
                    bytes.Add(color.B);

                    pixelX++;
                }
                pixelX = 0;
                pixelY++;
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Converts a 1D <see cref="byte"/> array to a 2D <see cref="byte"/> array
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns></returns>
        public static T[,] ConvertTo2D<T>(this T[] inputArray)
        {
            int length = inputArray.Length;
            int sideLength = (int)Math.Ceiling(Math.Sqrt(length)); // Calculate the length of each side of the square matrix
            T[,] outputArray = new T[sideLength, sideLength];

            for (int i = 0; i < length; i++)
            {
                outputArray[i / sideLength, i % sideLength] = inputArray[i];
            }

            return outputArray;
        }
        /// <summary>
        /// Converts a 2D <see cref="byte"/> array to a 1D <see cref="byte"/> array
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns></returns>
        static T[] ConvertTo1D<T>(this T[,] inputArray)
        {
            int rowCount = inputArray.GetLength(0);
            int colCount = inputArray.GetLength(1);
            T[] outputArray = new T[rowCount * colCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    int index = i * colCount + j;
                    outputArray[index] = inputArray[i, j];
                }
            }

            return outputArray;
        }

        /// <summary>
        /// Checks if <paramref name="type"/> inherits from <paramref name="baseType"/> anywhere down the line of inheritance
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns>True if <paramref name="type"/> does inherits from <paramref name="baseType"/>. otherwise false</returns>
        public static bool DerivesFrom(this Type type, Type baseType)
        {
            if (type.BaseType == null) return false;
            if (type.BaseType == baseType) return true;
            return type.BaseType.DerivesFrom(baseType);
        }
#if NET6_0_OR_GEATER
        /// <summary>
        /// Casts the <see cref="MutableString"/> array to an array of <see cref="string"/>
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string[] ToStringArray(this MutableString[] array)
        {
            string[] result = new string[array.Length];
            foreach (int i in array.Length)
                result[i] = array[i];
            return result;
        }
        /// <summary>
        /// Casts the <see cref="string"/> array to an array of <see cref="MutableString"/>
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static MutableString[] ToMutableStringArray(this string[] array)
        {
            MutableString[] result = new MutableString[array.Length];
            foreach (int i in array.Length)
                result[i] = array[i];
            return result;
        }
#endif
        /// <summary>
        /// Creates a new list of the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns>A dynamic representation of the newly created list. cast this explicitly to the correct list variable if needed</returns>
        public static IList CreateList(Type t)
        {
            var list = typeof(List<>);
            var constructedListType = list.MakeGenericType(t);

            return (IList)ActivatorExtra.CreateInstance(constructedListType);
        }
        /// <summary>
        /// gets the last part of the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>the last part of the given path</returns>
        public static string GetDirectoryName(string path)
        {
            return path.Split(new char[] { '\\', '/' }).TakeLast(1).First();
        }
        /// <summary>
        /// Repeats the given action the given amount of times.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="times"></param>
        public static void Repeat(Action action, int times)
        {
            for (int i = 0; i < times; i++)
                action();
        }
        /// <summary>
        /// Repeats the given action the given amount of times. and invokes the callback method after the action has been repeated the given amount of times.
        /// </summary>
        public static void Repeat(Action action, int times, Action<float> callback, int interval = 1000)
        {
            for (int i = 0; i < times; i++)
            {
                if (i is not 0 && i % interval == 0)
                    callback((float)MathS.GetPercentage(i, times, 2));
                action();
            }

        }
        /// <summary>
        /// Repeats the given action the given amount of times. gives the current iteration number as argument for the action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="times"></param>
        public static void Repeat(Action<ulong> action, ulong times)
        {
            for (ulong i = 0; i < times; i++)
                action(i);
        }
        /// <summary>
        /// Repeats the given action the given amount of times. gives the current iteration number as argument for the action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="times"></param>
        public static void Repeat(Action<int> action, int times)
        {
            for (int i = 0; i < times; i++)
                action(i);
        }
        /// <summary>
        /// Repeats the given action until the condition is met. or if 'UntilConditionIsTrue' is set to true, it repeats the action while the condition is met
        /// </summary>
        public static void Repeat(Action action, Func<bool> until, bool UntilConditionIsTrue = true)
        {
            bool running;
            do
            {
                action();
                running = UntilConditionIsTrue ? until() : !until();
            }
            while (running);
        }

        // async variants
        /// <summary>
        /// Repeats the given action the given amount of times.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="times"></param>
        public static async Task RepeatAsync(Action action, int times)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < times; i++)
                    action();
            });
        }
        /// <summary>
        /// Repeats the given action until the condition is met. or if 'UntilConditionIsTrue' is set to true, it repeats the action while the condition is met
        /// </summary>
        public static async Task RepeatAsync(Action action, Func<bool> until, bool UntilConditionIsTrue = true)
        {
            await Task.Run(() =>
            {
                bool running;
                do
                {
                    action();
                    running = UntilConditionIsTrue ? until() : !until();
                }
                while (running);
            });
        }
        /// <summary>
        /// creates a list of consecutive numbers
        /// </summary>
        /// <param name="count"></param>
        /// <returns>a new list that counts from 0 to the given count - 1</returns>
        public static List<int> ConsecutiveNumbers(int count)
        {
            List<int> nums = new List<int>();
            Repeat(i => nums.Add(i), count);
            return nums;
        }
        /// <summary>
        /// Creates a new list and populates it with the reversed order of the operated list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns>A new list with the items of the operated list reversed</returns>
        public static List<T> ReverseOrder<T>(this List<T> values)
        {
            List<T> result = new();
            foreach (int i in values.Count * -1)
                result.Add(values[i]);
            return result;
        }
        /// <summary>
        /// Counts the amount of items in the given list
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int Count(this IEnumerable values)
        {
            int count = 0;
            foreach (var item in values)
                count++;
            return count;
        }
        /// <summary>
        /// Converts the range to a list of integers
        /// </summary>
        /// <param name="range"></param>
        /// <returns>the created list</returns>
        public static List<int> ToList(this Range range)
        {
            List<int> nums = new();
            foreach (int i in range)
            {
                nums.Add(i);
            }
            return nums;
        }
        public static FIFOStack ToFIFOStack<T>(this List<T> values)
        {
            return new(values);
        }

        /// <summary>
        /// Indicates whether the specified character is catagorized as an uppercase letter
        /// </summary>
        /// <param name="c"></param>
        /// <returns>true if the given character is a uppercase letter, otherwise false</returns>
        public static bool IsUpper(this char c)
        {
            return char.IsUpper(c);
        }
        /// <summary>
        /// Indicates whether the specified character is catagorized as a lowercase letter
        /// </summary>
        /// <param name="c"></param>
        /// <returns>true if the given character is a lower case letter, otherwise false</returns>
        public static bool IsLower(this char c)
        {
            return char.IsLower(c);
        }
        /// <summary>
        /// Indicates whether the specified character is catagorized as a number
        /// </summary>
        /// <param name="c"></param>
        /// <returns>true if the given character is a number, otherwise false</returns>
        public static bool IsNumber(this char c) => char.IsNumber(c);
        /// <summary>
        /// Conerts the given letter to its uppercase variant
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static char ToUpper(this char c) => char.ToUpper(c);
        /// <summary>
        /// Converts the given letter to its lowercase variant
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static char ToLower(this char c) => char.ToLower(c);
        /// <summary>
        /// Indicates whether the given char is a alphabetical letter or not.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsLetter(this char c) => char.IsLetter(c);
        /// <summary>
        /// Adds the given Pair to the Dictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="pair"></param>
        public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> pair) where TKey : notnull => dict.Add(pair.Key, pair.Value);

        /// <summary>
        /// Sets all the letters in the char array into one single string
        /// </summary>
        /// <param name="chars"></param>
        public static string MakeString(this char[] chars) => new(chars);
        public static List<char> ToCharList(this string s)
        {
            return s.ToCharArray().ToList();
        }
        /// <summary>
        /// Calculates the relative time between the given times
        /// </summary>
        /// <param name="time"></param>
        /// <param name="target"></param>
        /// <returns>Timespan containing the relative time between the two and whether this time is in the past or not</returns>
        public static (TimeSpan time, bool inThePast) GetRelativeTime(this DateTime time, DateTime target)
        {
            var t = time - target;
            int c = DateTime.Compare(time, target);
            return (t, c == 1);
        }

        /// <summary>
        /// Splits the given IEnumerable into the given amount of parts. does not keep the order of the enumerable. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="parts"></param>
        /// <returns>a list of all parts</returns>
        public static List<List<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            List<List<T>> result = new List<List<T>>();
            splits.Foreach(x => result.Add(x.ToList()));
            return result;
        }

        /// <summary>
        /// determains the most efficient way to create smaller groups of the given IEnumberable and handles upon that conclution but never goes above the max alowed partitions. if put back together into one list it retains the same order (should you handle the items from the first split list to the last)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>an array of lists that has elements of the IEnumberable operated on split between them</returns>
        public static List<T>[] Partition<T>(this IEnumerable<T> enumerable, int totalPartitions)
        {
            if (enumerable == null)
                throw new ArgumentNullException("list");
            List<T> list = enumerable.ToList();
            if (totalPartitions < 1)
                throw new ArgumentOutOfRangeException("totalPartitions");

            List<T>[] partitions = new List<T>[totalPartitions];

            int maxSize = (int)Math.Ceiling(list.Count / (double)totalPartitions);
            int k = 0;

            for (int i = 0; i < partitions.Length; i++)
            {
                partitions[i] = new List<T>();
                for (int j = k; j < k + maxSize; j++)
                {
                    if (j >= list.Count)
                        break;
                    partitions[i].Add(list[j]);
                }
                k += maxSize;
            }

            return partitions;
        }
        /// <summary>
        /// finds the first unused <see cref="int"/> from a list
        /// </summary>
        /// <param name="list"></param>
        /// <returns>the next avalible <see cref="int"/> from a list of type  <see cref="int"/></returns>
        public static int NextAvalible(this List<int>? list)
        {
            if (list is null)
                return -1;
            if (list.Count == 0)
                return 0;

            var sorted = list.OrderBy(x => x).ToList();

            int last = 0;
            foreach (int i in sorted.Count)
            {
                if (sorted[i] != i)
                {
                    return last + 1;
                }
                last = i;
            }
            return last + 1;
        }
        /// <summary>
        /// finds the first unused <see cref="int"/> from a Dictionary which has a Key value of type <see cref="int"/>
        /// </summary>
        /// <returns>the next avalible <see cref="int"/> from the Dictionary of Keys of type <see cref="int"/></returns>
        public static int NextAvalible<TValue>(this Dictionary<int, TValue> dict) => dict.Keys.ToList().NextAvalible();
        #region Repeat, For, and Foreach extention methods

        /// <summary>
        /// Repeats the given action the amount of times the source int is set to.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void Repeat(this int source, Action<int> action)
        {
            for (int i = 0; i < source; i++)
                action(i);
        }

        /// <summary>
        /// Repeats the given action the given amount of times.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <param name="times"></param>
        public static void Repeat<T>(this T source, Action<T> action, int times)
        {
            for (int i = 0; i < times; i++)
                action(source);
        }

        /// <summary>
        /// Repeats the given action until the condition is met. or if 'UntilConditionIsTrue' is set to true, it repeats the action while the condition is met
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void Repeat<T>(this T source, Action<T> action, Func<bool> until, bool UntilConditionIsTrue = true)
        {
            bool running;
            do
            {
                action(source);
                running = UntilConditionIsTrue ? until() : !until();
            }
            while (running);
        }
        /// <summary>
        /// executes the given action on every entry in the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="action"></param>
        public static void Foreach<T>(this T[] array, Action<T> action)
        {
            foreach (var item in array)
                if (item != null)
                    action(item);
        }
        /// <summary>
        /// executes the given action on every entry in the array. passes the iteration int as the second argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="action"></param>
        public static void Foreach<T>(this T[] array, Action<T, int> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                T item = array[i];
                if (item != null)
                    action(item, i);
            }
        }
        /// <summary>
        /// executes the given action on every entry in the Enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public static void Foreach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            List<T> a = enumerable.ToList();
            foreach (var item in enumerable)
                action(item);
        }
        // async variants
        /// <summary>
        /// executes the given action on every entry in the Enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns>the list where the given action is preformed on all entries</returns>
        public static async Task ForeachAsync<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            await Task.Run(() =>
            {
                foreach (var item in enumerable)
                    action(item);
                return enumerable;
            });
        }
        /// <summary>
        /// Repeats the given action the given amount of times.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <param name="times"></param>
        public static async Task RepeatAsync<T>(this T source, Action<T> action, int times)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < times; i++)
                    action(source);
            });

        }
        /// <summary>
        /// Repeats the given action until the stop condition is met.
        /// </summary>
        public static async Task RepeatAsync<T>(this T source, Action<T> action, Func<bool> until, bool UntilConditionIsTrue = true)
        {
            await Task.Run(() =>
            {
                bool running;
                do
                {
                    action(source);
                    running = UntilConditionIsTrue ? until() : !until();
                }
                while (running);
            });
        }
        /// <summary>
        /// executes the given action on every entry in the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="action"></param>
        /// <returns>the array where the action has executed on all entries</returns>
        public static async Task ForeachAsync<T>(this T[] array, Action<T> action)
        {
            await Task.Run(() =>
            {
                foreach (var item in array)
                    action(item);
                return array;
            });
        }
        #region FuncTypes
        /// <summary>
        /// executes the given action on every entry in the Enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns>the list where the given action is preformed on all entries</returns>
        public static IEnumerable<T> Foreach<T>(this IEnumerable<T> enumerable, Func<T, T> action)
        {
            List<T> vals = enumerable.ToList();
            for (int i = 0; i < vals.Count; i++)
            {
                T item = vals[i];
                vals[i] = action(item);
            }
            return vals;
        }
        /// <summary>
        /// executes the given action on every entry in the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="action"></param>
        /// <returns>the array where the action has executed on all entries</returns>
        public static T[] Foreach<T>(this T[] array, Func<T, T> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                T item = array[i];
                array[i] = action(item);
            }

            return array;
        }
        /// <summary>
        /// executes an action on every index of the array. index can be null, the default of the type will be used for this iteration then
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="action"></param>
        /// <returns>the array where the action has executed for all indexes</returns>
        public static T[] For<T>(this T[] array, Func<T, T> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                T iteration = array[i];
                array[i] = action((iteration == null) ? default : iteration);
            }
            return array;
        }

        // async variants
        /// <summary>
        /// executes the given action on every entry in the Enumerable
        /// </summary>
        /// <returns>the list where the given action is preformed on all entries</returns>
        public static async Task<IEnumerable<T>> ForeachAsync<T>(this IEnumerable<T> enumerable, Func<T, T> action)
        {
            await Task.Run(() =>
            {
                List<T> vals = enumerable.ToList();
                for (int i = 0; i < vals.Count; i++)
                {
                    T item = vals[i];
                    vals[i] = action(item);
                }
                return vals;
            });
            return default;
        }
        /// <summary>
        /// executes the given action on every entry in the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="action"></param>
        /// <returns>the array where the action has executed on all entries</returns>
        public static async Task<T[]> ForeachAsync<T>(this T[] array, Func<T, T> action)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < array.Length; i++)
                {
                    T item = array[i];
                    array[i] = action(item);
                }
                return array;
            });
            return Array.Empty<T>();
        }
        /// <summary>
        /// executes an action on every index of the array. index can be null, the default of the type will be used for this iteration then
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="action"></param>
        /// <returns>the array where the action has executed for all indexes</returns>
        public static async Task<T[]> ForAsync<T>(this T[] array, Func<T, T> action)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < array.Length; i++)
                {
                    T iteration = array[i];
                    array[i] = action((iteration == null) ? default : iteration);
                }
                return array;
            });
            return [];
        }

        /// <summary>
        /// Creates a new list of the type copying the values from the source list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public static List<T> CreateList<T>(IList list)
        {
            Type tt = typeof(T);
            

            ArgumentNullException.ThrowIfNull(list, nameof(list));
            if(list.Count == 0)
                return [];

            List<T> result = [];
            foreach (var item in list)
            {
                if(item is T t)
                    result.Add(t);
                else
                    throw new InvalidCastException($"Could not cast {item.GetType()} to {typeof(T)}");
            }
            return result;
        }

        #endregion
        #endregion

    }
}
