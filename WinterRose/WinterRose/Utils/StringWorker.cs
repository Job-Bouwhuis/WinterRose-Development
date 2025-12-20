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
    }
}