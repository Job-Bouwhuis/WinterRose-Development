
using System.Diagnostics;
using System.Runtime.Serialization;
using WinterRose.ForgeGuardChecks.Exceptions;

namespace WinterRose.ForgeGuardChecks.Expectations;

public class MethodExpectation
{
    protected readonly Delegate method;
    protected readonly object[] args;
    protected readonly string messageStart;

    protected bool hasRun;
    protected object? returnValue;
    protected Exception? thrownException;
    protected TimeSpan executionTime;

    /// <summary>
    /// A negative version of this expectation.
    /// </summary>
    public virtual MethodExpectation Not => new NegatedMethodExpectation(method, args, messageStart);

    /// <summary>
    /// An <see cref="Expectation"/> for the return value of the method when it is called.<br></br>
    /// Calls <code>Not.ToThrowAny()</code> before returning the expectation, so it is safe to chain.<br></br>
    /// </summary>
    public Expectation ThatReturnValue
    {
        get
        {
            RunIfNeeded();
            Not.ToThrowAny();
            return new Expectation(returnValue, messageStart);
        }
    }

    public MethodExpectation(Delegate method, object[] args, string messageStart)
    {
        this.method = method;
        this.args = args;
        this.messageStart = messageStart;
    }

    protected void RunIfNeeded()
    {
        if (hasRun) return;

        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            returnValue = method.DynamicInvoke(args);
        }
        catch (Exception e)
        {
            thrownException = e.InnerException ?? e;
        }
        finally
        {
            sw.Stop();
            executionTime = sw.Elapsed;
            hasRun = true;
        }
    }

    /// <summary>
    /// Expects the method to have thrown an exception of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>the same reference</returns>
    /// <exception cref="WrongExceptionThrownException"></exception>
    /// <exception cref="NoExceptionThrownException"></exception>
    public virtual MethodExpectation ToThrow<T>() where T : Exception
    {
        RunIfNeeded();

        if (thrownException is T)
            return this;

        if (thrownException is not null)
            throw new WrongExceptionThrownException(messageStart, thrownException, typeof(T));

        throw new NoExceptionThrownException(messageStart, typeof(T));
    }

    /// <summary>
    /// Expects the method to have thrown any exception.
    /// </summary>
    /// <returns>the same reference</returns>
    /// <exception cref="NoExceptionThrownException"></exception>
    public virtual MethodExpectation ToThrowAny()
    {
        RunIfNeeded();

        if (thrownException is not null)
            return this;

        throw new NoExceptionThrownException(messageStart);
    }

    /// <summary>
    /// Expects the method to return the specified value.
    /// </summary>
    /// <param name="expectedReturnValue"></param>
    /// <returns>the same reference</returns>
    /// <exception cref="ExceptionThrownException"></exception>
    /// <exception cref="InvalidReturnValueException"></exception>
    public virtual MethodExpectation ToReturn(object? expectedReturnValue)
    {
        RunIfNeeded();

        if (thrownException != null)
            throw new ExceptionThrownException(messageStart, thrownException.GetType());

        if (expectedReturnValue == null && returnValue == null)
            return this;

        if (expectedReturnValue == null || !expectedReturnValue.Equals(returnValue))
            throw new InvalidReturnValueException(messageStart, expectedReturnValue, returnValue);

        return this;
    }

    /// <summary>
    /// Expects the method to complete within the given <paramref name="time"/>
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    /// <exception cref="MethodExecutionTimeExceededException"></exception>
    public virtual MethodExpectation ToCompleteWithin(TimeSpan time)
    {
        RunIfNeeded();

        if (executionTime > time)
            throw new MethodExecutionTimeExceededException(messageStart, time, executionTime);

        return this;
    }

    /// <summary>
    /// Expects the method to complete within the given number of milliseconds.
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public MethodExpectation ToCompleteWithin(long ms) => ToCompleteWithin(TimeSpan.FromMilliseconds(ms));

    /// <summary>
    /// Returns the same reference, allowing for more readable chaining of expectations. not required for chaining.
    /// </summary>
    /// <returns></returns>
    public MethodExpectation And() => this;
}
