
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

    public virtual MethodExpectation Not => new NegatedMethodExpectation(method, args, messageStart);

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

    public virtual MethodExpectation ToThrow<T>() where T : Exception
    {
        RunIfNeeded();

        if (thrownException is T)
            return this;

        if (thrownException is not null)
            throw new WrongExceptionThrownException(messageStart, thrownException, typeof(T));

        throw new NoExceptionThrownException(messageStart, typeof(T));
    }

    public virtual MethodExpectation ToThrowAny()
    {
        RunIfNeeded();

        if (thrownException is not null)
            return this;

        throw new NoExceptionThrownException(messageStart);
    }

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

    public virtual MethodExpectation ToCompleteWithin(TimeSpan time)
    {
        RunIfNeeded();

        if (executionTime > time)
            throw new MethodExecutionTimeExceededException(messageStart, time, executionTime);

        return this;
    }

    public MethodExpectation ToCompleteWithin(long ms) => ToCompleteWithin(TimeSpan.FromMilliseconds(ms));

    public MethodExpectation And() => this;
}
