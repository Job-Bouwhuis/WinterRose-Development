using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeGuardChecks.Exceptions
{
    public class ValueNotNullException(string messageStart)
        : Exception($"{messageStart} expected to be null, but it was not");

    public class ValueNullException(string messageStart)
        : Exception($"{messageStart} expected to not be null, but it was");

    public class ValueFalseException(string messageStart)
        : Exception($"{messageStart} expected to be true, but was false");

    public class ValueTrueException(string messageStart)
        : Exception($"{messageStart} expected to be false, but was true");

    public class ValueGreaterThanException(string messageStart, object? comparedTo)
        : Exception($"{messageStart} expected to be greater than {comparedTo}, but it was not");

    public class ValueLessThanException(string messageStart, object? comparedTo)
        : Exception($"{messageStart} expected to be less than {comparedTo}, but it was not");

    public class ValueEqualException(string messageStart, object? comparedTo)
        : Exception($"{messageStart} expected to equal {comparedTo}, but it did not");

    public class ValueNotEqualException(string messageStart, object? comparedTo)
        : Exception($"{messageStart} expected to not equal {comparedTo}, but it did");

    public class ValueEmptyException(string messageStart)
        : Exception($"{messageStart} was expected to not be empty, but it was");

    public class ValueNotEmptyException(string messageStart)
        : Exception($"{messageStart} was expected to be empty, but it was not");

    public class ValueDoesNotContainException(string messageStart, object? element)
        : Exception($"{messageStart} was expected to contain '{element}', but it did not");

    public class ValueContainsException(string messageStart, object? element)
        : Exception($"{messageStart} was expected to not contain '{element}', but it did");

    public class ValueNotOfTypeException(string messageStart, Type expectedType)
        : Exception($"{messageStart} was expected to be of type '{expectedType.Name}', but it was not");

    public class ValueNotAssignableToTypeException(string messageStart, Type targetType)
        : Exception($"{messageStart} was not assignable to '{targetType.Name}'");

    public class ValueNotInRangeException(string messageStart, object? min, object? max)
        : Exception($"{messageStart} was expected to be in range [{min}, {max}], but it was not");

    public class ValueDoesNotMatchException(string messageStart, string pattern)
        : Exception($"{messageStart} was expected to match pattern '{pattern}', but it did not");

    public class ValueUnexpectedException(string messageStart, object? expected, object? actual)
        : Exception($"{messageStart} was expected to be '{expected}', but was '{actual}'");
}
