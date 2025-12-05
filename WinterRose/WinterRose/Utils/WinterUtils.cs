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
using System.Reflection;
using System.Security.Cryptography;
using WinterRose.AnonymousTypes;

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

        extension(Range range)
        {
            /// <summary>
            /// Gets whether the given <paramref name="num"/> is inside the <paramref name="range"/>
            /// </summary>
            /// <param name="num"></param>
            /// <returns>True if <paramref name="num"/> is within the <paramref name="range"/>, otherwise false</returns>
            public bool Contains(int num)
            {
                if (num > range.Start.Value && num < range.End.Value)
                    return true;
                return false;
            }

            /// <summary>
            /// Converts the range to a list of integers
            /// </summary>
            /// <param name="range"></param>
            /// <returns>the created list</returns>
            public List<int> ToList()
            {
                List<int> nums = new();
                foreach (int i in range)
                    nums.Add(i);
                return nums;
            }
        }

        extension<K, V>(KeyValuePair<K, V> kvp)
        {
            /// <summary>
            /// Deconstructs the key value pair into a tuple of (Key, Value)
            /// </summary>
            /// <returns></returns>
            public (K Key, V Value) Deconstruct()
            {
                return (kvp.Key, kvp.Value);
            }
        }

        extension(string str)
        {
            /// <summary>
            /// Checks if the given string ends with any of the given <paramref name="options"/>
            /// </summary>
            /// <param name="options"></param>
            /// <returns></returns>
            public bool EndsWith(params string[] options)
            {
                foreach (var option in options)
                    if (str.EndsWith(option))
                        return true;
                return false;
            }

            /// <summary>
            /// Checks if the given starts with any of the given <paramref name="options"/>
            /// </summary>
            /// <param name="str"></param>
            /// <param name="options"></param>
            /// <returns></returns>
            public bool StartsWith(params string[] options)
            {
                foreach (var option in options)
                    if (str.StartsWith(option))
                        return true;
                return false;
            }
        }

        extension(DirectoryInfo dir)
        {
            /// <summary>
            /// Tries to open a directory with name <paramref name="name"/> inside the directory
            /// </summary>
            /// <param name="name"></param>
            /// <param name="child"></param>
            /// <returns>True if the child was found and opened, otherwise false </returns>
            public bool OpenChildDirectory(string name, out DirectoryInfo child)
            {
                string childPath = Path.Combine(dir.FullName, name);
                child = new DirectoryInfo(childPath);
                return child.Exists;
            }
        }

        extension(Type type)
        {
            /// <summary>
            /// Checks whether the given type is an anonymous type
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public bool IsAnonymousType()
            {
                return type.Name.Contains("<>f__AnonymousType") || type.IsAssignableTo(typeof(Anonymous));
            }

            /// <summary>
            /// Checks if <paramref name="type"/> inherits from <paramref name="baseType"/> anywhere down the line of inheritance
            /// </summary>
            /// <param name="type"></param>
            /// <param name="baseType"></param>
            /// <returns>True if <paramref name="type"/> does inherits from <paramref name="baseType"/>. otherwise false</returns>
            public bool DerivesFrom(Type baseType)
            {
                if (type.BaseType == null) return false;
                if (type.BaseType == baseType) return true;
                return type.BaseType.DerivesFrom(baseType);
            }

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
        }

        extension(Random rnd)
        {
            /// <summary>
            /// Gets a random float between the given <paramref name="min"/> and <paramref name="max"/> values
            /// </summary>
            /// <param name="random"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <returns></returns>
            public float NextFloat(float min, float max)
            {
                float f = (float)rnd.NextDouble() * (max - min) + min;
                return f;
            }
        }

        extension<T>(List<T> list)
        {
            /// <summary>
            /// Checks whether all the given <paramref name="values"/> exist in the given list
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="collection"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public bool Contains(params T[] values)
            {
                foreach (var val in values)
                    if (!list.Contains(val))
                        return false;
                return true;
            }

            /// <summary>
            /// Adds all the values to the list
            /// </summary>
            /// <param name="toAdd"></param>
            /// <returns>the same list reference</returns>
            public List<T> AddMany(params T[] toAdd)
            {
                foreach (var val in toAdd)
                    list.Add(val);
                return list;
            }

            /// <summary>
            /// Creates a new list and populates it with the reversed order of the operated list
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="values"></param>
            /// <returns>A new list with the items of the operated list reversed</returns>
            public List<T> ReverseOrder()
            {
                List<T> result = new();
                foreach (int i in list.Count * -1)
                    result.Add(list[i]);
                return result;
            }
        }

        extension (List<int> list)
        {
            /// <summary>
            /// finds the first unused <see cref="int"/> from a list
            /// </summary>
            /// <returns>the next avalible <see cref="int"/> from a list of type  <see cref="int"/></returns>
            public int NextAvalible()
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
        }

        extension(float f)
        {
            /// <summary>
            /// Rounds the given float
            /// </summary>
            /// <param name="f"></param>
            /// <returns></returns>
            public float Round(int digits = 0, MidpointRounding roundingMode = MidpointRounding.ToZero) =>
                float.Round(f, digits, roundingMode);
        }

        extension(double d)
        {
            /// <summary>
            /// Rounds the given double <paramref name="d"/>
            /// </summary>
            /// <param name="d"></param>
            /// <returns></returns>
            public int Round(int digits = 0, MidpointRounding roundingMode = MidpointRounding.ToZero)
                => (int)double.Round(d, digits, roundingMode);
        }

        extension(byte[] bytes)
        {
            /// <summary>
            /// Converts the given <see cref="byte"/> array to a <see cref="Bitmap"/>. To retrieve the data stored use <see cref="ConvertToBytes(Bitmap)"/> This method is only available on Windows 7 or higher
            /// </summary>
            /// <returns></returns>
            /// <exception cref="OperationCanceledException"></exception>
            public Bitmap ConvertToBitmap()
            {
                if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                    throw new OperationCanceledException("This method requires Windows 7 or higher");

                // Calculate the number of pixels needed based on the input byte array length
                int pixelCount = (int)Math.Ceiling((double)bytes.Length / 3);

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
                        if (inputIndex >= bytes.Length)
                        {
                            // Set the color of the current pixel to black
                            bitmap.SetPixel(x, y, Color.Black);
                        }
                        else
                        {

                            byte b1 = bytes[inputIndex];
                            byte b2 = bytes.Length > inputIndex + 1 ? bytes[inputIndex + 1] : (byte)0;
                            byte b3 = bytes.Length > inputIndex + 2 ? bytes[inputIndex + 2] : (byte)0;

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
        }

        extension(Bitmap bitmap)
        {
            /// <summary>
            /// Converts the <see cref="Bitmap"/> to a <see cref="byte"/> array. this method makes usable data when the bitmap is created using <see cref="ConvertToBitmap(byte[], ProgressReporter)"/>. This method is only available on windows 7 or higher
            /// </summary>
            /// <returns></returns>
            public byte[] ConvertToBytes()
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
        }

        extension<T>(T[] inputArray)
        {
            /// <summary>
            /// Converts a 1D <see cref="byte"/> array to a 2D <see cref="byte"/> array
            /// </summary>
            /// <returns></returns>
            public T[,] ConvertTo2D()
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
            /// executes the given action on every entry in the array
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="array"></param>
            /// <param name="action"></param>
            public void Foreach(Action<T> action)
            {
                foreach (var item in inputArray)
                    if (item != null)
                        action(item);
            }
            /// <summary>
            /// executes the given action on every entry in the array. passes the iteration int as the second argument.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="array"></param>
            /// <param name="action"></param>
            public void Foreach(Action<T, int> action)
            {
                for (int i = 0; i < inputArray.Length; i++)
                {
                    T item = inputArray[i];
                    action(item, i);
                }
            }
        }

        extension<T>(T[,] inputArray)
        {
            /// <summary>
            /// Converts a 2D <see cref="byte"/> array to a 1D <see cref="byte"/> array
            /// </summary>
            /// <returns></returns>
            public T[] ConvertTo1D()
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
        }

        extension (char c)
        {
            /// <summary>
            /// Indicates whether the specified character is catagorized as an uppercase letter
            /// </summary>
            /// <param name="c"></param>
            /// <returns>true if the given character is a uppercase letter, otherwise false</returns>
            public bool IsUpper()
            {
                return char.IsUpper(c);
            }
            /// <summary>
            /// Indicates whether the specified character is catagorized as a lowercase letter
            /// </summary>
            /// <param name="c"></param>
            /// <returns>true if the given character is a lower case letter, otherwise false</returns>
            public bool IsLower()
            {
                return char.IsLower(c);
            }
            /// <summary>
            /// Indicates whether the specified character is catagorized as a number
            /// </summary>
            /// <param name="c"></param>
            /// <returns>true if the given character is a number, otherwise false</returns>
            public bool IsNumber() => char.IsNumber(c);
            /// <summary>
            /// Conerts the given letter to its uppercase variant
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public char ToUpper() => char.ToUpper(c);
            /// <summary>
            /// Converts the given letter to its lowercase variant
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public char ToLower() => char.ToLower(c);
            /// <summary>
            /// Indicates whether the given char is a alphabetical letter or not.
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public bool IsLetter() => char.IsLetter(c);
        }

        extension<K, V> (Dictionary<K, V> dict) where K : notnull
        {
            /// <summary>
            /// Adds the given Pair to the Dictionary
            /// </summary>
            /// <param name="pair"></param>
            public void Add(KeyValuePair<K, V> pair) => dict.Add(pair.Key, pair.Value);
        }

        extension<T> (IEnumerable<T> values)
        {
            /// <summary>
            /// Splits the given IEnumerable into the given amount of parts. does not keep the order of the enumerable. 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="list"></param>
            /// <param name="parts"></param>
            /// <returns>a list of all parts</returns>
            public List<List<T>> Split(int parts)
            {
                int i = 0;
                var splits = from item in values
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
            public List<T>[] Partition(int totalPartitions)
            {
                if (values == null)
                    throw new ArgumentNullException("list");
                List<T> list = values.ToList();
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

            public void Foreach(Action<T, int> action)
            {
                int size = values.Count();
                for (int i = 0; i < size; i++)
                {
                    T item = values.ElementAt(i);
                    action(item, i);
                }
            }

            /// <summary>
            /// executes the given action on every entry in the Enumerable
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="action"></param>
            public void Foreach(Action<T> action)
            {
                foreach (var item in values)
                    action(item);
            }
        }
        extension (IEnumerable<int> values)
        {
            /// <summary>
            /// finds the first unused <see cref="int"/> from a list
            /// </summary>
            /// <returns>the next avalible <see cref="int"/> from a list of type  <see cref="int"/></returns>
            public int NextAvalible()
            {
                ArgumentNullException.ThrowIfNull(values);

                var sorted = values.OrderBy(x => x).ToList();

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
        }

        extension (DateTime time)
        {
            /// <summary>
            /// Calculates the relative time between the given times
            /// </summary>
            /// <param name="time"></param>
            /// <param name="target"></param>
            /// <returns>Timespan containing the relative time between the two and whether this time is in the past or not</returns>
            public (TimeSpan time, bool inThePast) GetRelativeTime(DateTime target)
            {
                var t = time - target;
                int c = DateTime.Compare(time, target);
                return (t, c == 1);
            }
        }

        extension<V> (Dictionary<int, V> dict)
        {
            /// <summary>
            /// finds the first unused <see cref="int"/> from a Dictionary which has a Key value of type <see cref="int"/>
            /// </summary>
            /// <returns>the next avalible <see cref="int"/> from the Dictionary of Keys of type <see cref="int"/></returns>
            public int NextAvalible()
                => dict.Keys.ToList().NextAvalible();
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
            if (list.Count == 0)
                return [];

            List<T> result = [];
            foreach (var item in list)
            {
                if (item is T t)
                    result.Add(t);
                else
                    throw new InvalidCastException($"Could not cast {item.GetType()} to {typeof(T)}");
            }
            return result;
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
    }
}
