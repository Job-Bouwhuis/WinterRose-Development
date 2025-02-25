using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Asserting;

namespace WinterRose
{
    /// <summary>
    /// Assert Functions
    /// </summary>
    public class Assert
    {
        /// <summary>
        /// Asserts that a condition is true
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void True(bool condition, string? message = null)
        {
            if (!condition)
                throw new WinterAssertException(message ?? "Condition was false");
        }
        /// <summary>
        /// Asserts that a condition is false
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void False(bool condition, string? message = null)
        {
            if (condition)
                throw new WinterAssertException(message ?? "Condition was true");
        }
        /// <summary>
        /// Asserts that a condition is true and executes an action if it is not
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="action"></param>
        public static void True(bool condition, Action action)
        {
            if (!condition)
                action();
        }
        /// <summary>
        /// Asserts that a condition is false and executes an action if it is not
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="action"></param>
        public static void False(bool condition, Action action)
        {
            if (condition)
                action();
        }
        /// <summary>
        /// Asserts that an object is null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void Null(object? obj, string? message = null)
        {
            if (obj != null)
                throw new WinterAssertException(message ?? "Object was not null");
        }
        /// <summary>
        /// Asserts that an object is not null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void NotNull(object? obj, string? message = null)
        {
            if (obj == null)
                throw new WinterAssertException(message ?? "Object was null");
        }
        /// <summary>
        /// Asserts that two objects are equal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void Equal<T>(T expected, T actual, string? message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new WinterAssertException(message ?? $"Expected {expected} but got {actual}");
        }
        /// <summary>
        /// Asserts that two objects are not equal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void NotEqual<T>(T expected, T actual, string? message = null)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
                throw new WinterAssertException(message ?? $"Expected {expected} but got {actual}");
        }
        /// <summary>
        /// Asserts that two objects are the same object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void AreSame<T>(T expected, T actual, string? message = null)
        {
            if (!ReferenceEquals(expected, actual))
                throw new WinterAssertException(message ?? $"Expected {expected} but got {actual}");
        }
        /// <summary>
        /// Asserts that two objects are not the same object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void AreNotSame<T>(T expected, T actual, string? message = null)
        {
            if (ReferenceEquals(expected, actual))
                throw new WinterAssertException(message ?? $"Expected {expected} but got {actual}");
        }
        /// <summary>
        /// Asserts that an object is an instance of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void InstanceOf<T>(object obj, string? message = null)
        {
            if (obj is not T)
                throw new WinterAssertException(message ?? $"Expected {obj} to be an instance of {typeof(T)}");
        }
        /// <summary>
        /// Asserts that an object is not an instance of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void NotInstanceOf<T>(object obj, string? message = null)
        {
            if (obj is T)
                throw new WinterAssertException(message ?? $"Expected {obj} to not be an instance of {typeof(T)}");
        }
        /// <summary>
        /// Asserts that an object is assignable from a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void AssignableFrom<T>(object obj, string? message = null)
        {
            if (!typeof(T).IsAssignableFrom(obj.GetType()))
                throw new WinterAssertException(message ?? $"Expected {obj} to be assignable from {typeof(T)}");
        }
        /// <summary>
        /// Asserts that the collection contains no items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void Empty<T>(IEnumerable<T> enumerable, string? message = null)
        {
            if (enumerable.Any())
                throw new WinterAssertException(message ?? $"Expected {enumerable} to be empty");
        }
        /// <summary>
        /// Asserts that the collection contains at least one item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void NotEmpty<T>(IEnumerable<T> enumerable, string? message = null)
        {
            if (!enumerable.Any())
                throw new WinterAssertException(message ?? $"Expected {enumerable} to not be empty");
        }
        /// <summary>
        /// Asserts that a string is empty
        /// </summary>
        /// <param name="str"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void Empty(string str, string? message = null)
        {
            if (!string.IsNullOrEmpty(str))
                throw new WinterAssertException(message ?? $"Expected {str} to be empty");
        }
        /// <summary>
        /// Asserts that a string is not empty
        /// </summary>
        /// <param name="str"></param>
        /// <param name="message"></param>
        /// <exception cref="WinterAssertException"></exception>
        public static void NotEmpty(string str, string? message = null)
        {
            if (string.IsNullOrEmpty(str))
                throw new WinterAssertException(message ?? $"Expected {str} to not be empty");
        }
    }
}
