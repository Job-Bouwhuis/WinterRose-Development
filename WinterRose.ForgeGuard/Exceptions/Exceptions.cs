using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeGuardChecks.Exceptions
{
    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be null, but it was not. 
    /// </summary>
    /// <param name="messageStart"></param>
    public class ValueNotNullException(string messageStart)
        : Exception($"{messageStart} expected to be null, but it was not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to not be null, but it was.
    /// </summary>
    /// <param name="messageStart"></param>
    public class ValueNullException(string messageStart)
        : Exception($"{messageStart} expected to not be null, but it was");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be true, but it was false.
    /// </summary>
    /// <param name="messageStart"></param>
    public class ValueFalseException(string messageStart)
        : Exception($"{messageStart} expected to be true, but was false");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be false, but it was true.
    /// </summary>
    /// <param name="messageStart"></param>
    public class ValueTrueException(string messageStart)
        : Exception($"{messageStart} expected to be false, but was true");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be greater than a specified value, but it was not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="comparedTo"></param>
    public class ValueGreaterThanException(string messageStart, object? comparedTo)
        : Exception($"{messageStart} expected to be greater than {comparedTo}, but it was not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be less than a specified value, but it was not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="comparedTo"></param>
    public class ValueLessThanException(string messageStart, object? comparedTo)
        : Exception($"{messageStart} expected to be less than {comparedTo}, but it was not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to equal a specified value, but it did not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="comparedTo"></param>
    public class ValueEqualException(string messageStart, object? comparedTo)
        : Exception($"{messageStart} expected to equal {comparedTo}, but it did not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to not equal a specified value, but it did.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="comparedTo"></param>
    public class ValueNotEqualException(string messageStart, object? comparedTo)
        : Exception($"{messageStart} expected to not equal {comparedTo}, but it did");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be empty (e.g., an empty string or collection), but it was not.
    /// </summary>
    /// <param name="messageStart"></param>
    public class ValueEmptyException(string messageStart)
        : Exception($"{messageStart} was expected to not be empty, but it was");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected the have contents (e.g., a string or collection), but it was empty.
    /// </summary>
    /// <param name="messageStart"></param>
    public class ValueNotEmptyException(string messageStart)
        : Exception($"{messageStart} was expected to be empty, but it was not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to contain a specific element, but it did not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="element"></param>
    public class ValueDoesNotContainException(string messageStart, object? element)
        : Exception($"{messageStart} was expected to contain '{element}', but it did not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to not contain a specific element, but it did.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="element"></param>
    public class ValueContainsException(string messageStart, object? element)
        : Exception($"{messageStart} was expected to not contain '{element}', but it did");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be of a specific type, but it was not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="expectedType"></param>
    public class ValueNotOfTypeException(string messageStart, Type expectedType)
        : Exception($"{messageStart} was expected to be of type '{expectedType.Name}', but it was not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to not be of a specific type, but it was.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="expectedType"></param>
    public class ValueOfTypeException(string messageStart, Type expectedType)
    : Exception($"{messageStart} was expected to not be of type '{expectedType.Name}', but it was");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be assignable to a specific type, but it was not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="targetType"></param>
    public class ValueNotAssignableToTypeException(string messageStart, Type targetType)
        : Exception($"{messageStart} was not assignable to '{targetType.Name}'");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to not be assignable to a specific type, but it was.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="targetType"></param>
    public class ValueAssignableToTypeException(string messageStart, Type targetType)
    : Exception($"{messageStart} was assignable to '{targetType.Name}'");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be in a specific range, but it was not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public class ValueNotInRangeException(string messageStart, object? min, object? max)
        : Exception($"{messageStart} was expected to be in range [{min}, {max}], but it was not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to not be in a specific range, but it was.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public class ValueInRangeException(string messageStart, object? min, object? max)
    : Exception($"{messageStart} was expected to be in range [{min}, {max}], but it was not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to match a specific pattern (e.g., regex), but it did not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="pattern"></param>
    public class ValueDoesNotMatchException(string messageStart, string pattern)
        : Exception($"{messageStart} was expected to match pattern '{pattern}', but it did not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to not match a specific pattern (e.g., regex), but it did.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="pattern"></param>
    public class ValueDoesMatchException(string messageStart, string pattern)
    : Exception($"{messageStart} was expected to match pattern '{pattern}', but it did not");

    /// <summary>
    /// Thrown by <see cref="Forge.Expect{T}(T, string, string, int)"/> chains when a value is expected to be a specific value, but it was not.
    /// </summary>
    /// <param name="messageStart"></param>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    public class ValueUnexpectedException(string messageStart, object? expected, object? actual)
        : Exception($"{messageStart} was expected to be '{expected}', but was '{actual}'");
}
