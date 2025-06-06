using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeGuardChecks.Exceptions
{
    public class WrongExceptionThrownException(string messageStart, Exception actual, Type expectedExceptionType) 
        : Exception($"{messageStart} threw an exception of type {actual.GetType().Name} while {expectedExceptionType.Name} was expected");

    public class ExceptionThrownException(string messageStart, Type exceptionType)
        : Exception($"{messageStart} threw an exception of type {exceptionType.Name}. while a successful return was expected");

    public class InvalidReturnValueException(string messageStart, object? expected, object? actual)
     : Exception($"{messageStart} returned {Format(actual)} while {Format(expected)} was expected")
    {
        private static string Format(object? value)
        {
            if (value == null)
                return "'null'";

            Type type = value.GetType();
            if (type.IsPrimitive || value is string)
                return $"'{value}'";

            return $"'{type.Name}'";
        }
    }

    public class NoExceptionThrownException : Exception
    {
        public NoExceptionThrownException(string messageStart) 
            : base($"{messageStart} expected to throw any exception, but didnt")
        {
        }

        public NoExceptionThrownException(string messageStart, Type expectedExceptionType)
            : base($"{messageStart} expected to throw {expectedExceptionType.Name}, but didnt throw any") 
        { 
        }
    }

    public class MethodExecutionTimeExceededException(string messageStart, TimeSpan time, TimeSpan actual) 
        : Exception($"{messageStart} took more than {time:mm:ss.fff}ms to execute. ({actual:mm:ss.fff})");

    public class UnexpectedExceptionThrownException(string messageStart, Type exceptionType)
    : Exception($"{messageStart} was expected to run successfully, but threw {exceptionType.Name}");

    public class MethodExecutionTimeTooFastException(string messageStart, TimeSpan minimum, TimeSpan actual)
    : Exception($"{messageStart} completed faster than expected. Took {actual:mm\\:ss\\.fff}, expected at least {minimum:mm\\:ss\\.fff}");

}
