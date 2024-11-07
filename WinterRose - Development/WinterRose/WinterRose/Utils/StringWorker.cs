using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// Provides methods to manipulate strings 
    /// </summary>
    public static class StringWorker
    {
        /// <summary>
        /// reverses the given string
        /// </summary>
        /// <param name="input"></param>
        /// <returns>a new string with its character order exactly reversed</returns>
        public static string ReverseOrder(this string input) => new string(input.ToCharArray().Reverse().ToArray());
        /// <summary>
        /// makes the first letter capital while making the rest lowercase
        /// </summary>
        /// <param name="source">string to be acted on</param>
        /// <returns>the manipulated string where all but the first letter are lower case</returns>
        public static string FirstCapital(this string source) => $"{source[0].ToString().ToUpper()}{source.TrimStart(source[0]).ToLower()}";
        /// <summary>
        /// Makes all first letters of sections seperated by a space a capital letter
        /// </summary>
        /// <param name="source"></param>
        /// <returns>the manipulated string where every word has its first letter turned into a captial letter</returns>
        public static string FirstCapitalOnAllWords(this string source)
        {
            StringBuilder result = new StringBuilder();
            source.Split(' ', StringSplitOptions.RemoveEmptyEntries).Foreach(x => result.Append($"{x.FirstCapital()} "));
            return result.ToString().Trim();

        }
        /// <summary>
        /// converts the given string to a base64 format
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns>the base64 result from the given string</returns>
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// converts the given base64 format into UTF8 characters
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns>the UTF8 string result from the given base64 format input</returns>
        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        /// <summary>
        /// allows easy animation of a string to be shown to the user. <br></br>combine with <see cref="WinterRose.WinterUtils.Foreach{T}(IEnumerable{T}, Action{T})"/>, <see cref="WinterUtils.ForeachAsync{T}(IEnumerable{T}, Action{T})"/>, or any other foreach loop to attchieve the desired result
        /// </summary>
        /// <param name="content"></param>
        /// <param name="delay"></param>
        /// <returns>yield returns a string with one extra character than the last until the given <paramref name="content"/> is returned in its fullest</returns>
        public static IEnumerable<string> StringAnimation(this string content, int delay)
        {
            string returnable = "";
            foreach (char c in content)
            {
                returnable += c;
                yield return returnable;
                Task.Delay(delay).Wait();
            }
        }
        /// <summary>
        /// allows easy animation of a string to be shown to the user. <br></br>combine with <see cref="WinterRose.WinterUtils.Foreach{T}(IEnumerable{T}, Action{T})"/>, <see cref="WinterUtils.ForeachAsync{T}(IEnumerable{T}, Action{T})"/>, or any other foreach loop to attchieve the desired result
        /// </summary>
        /// <param name="content"></param>
        /// <param name="delay"></param>
        /// <returns>yield returns a char taken from the given <paramref name="content"/> until the last char has been returned</returns>
        public static IEnumerable<char> StringAnimationChar(this string content, int delay)
        {
            foreach (char c in content)
            {
                yield return c;
                Task.Delay(delay).Wait();
            }
        }
        public static string GetInterestingString(string source)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            StringBuilder sb = new StringBuilder();
            bytes.Foreach(x => sb.Append(x));
            return sb.ToString();
        }
        public static string FromInterestingString(string source)
        {
            ReadOnlySpan<char> stuff = source;
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < source.Length - 3; i += 3)
            {
                string s = stuff.Slice(i, i + 3).ToString();
                bytes.Add(TypeWorker.CastPrimitive<byte>(s));
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}