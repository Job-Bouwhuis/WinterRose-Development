using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks.Exceptions;

namespace WinterRose.ForgeGuardChecks.Expectations
{
    public class NegatedMethodExpectation : MethodExpectation
    {
        public override MethodExpectation Not => new MethodExpectation(method, args, messageStart);

        public NegatedMethodExpectation(Delegate method, object[] args, string messageStart)
            : base(method, args, messageStart)
        {
        }

        public override MethodExpectation ToThrow<T>()
        {
            RunIfNeeded();

            if (thrownException is T)
                throw new UnexpectedExceptionThrownException(messageStart, typeof(T));

            return this;
        }

        public override MethodExpectation ToThrowAny()
        {
            RunIfNeeded();

            if (thrownException is not null)
                throw new UnexpectedExceptionThrownException(messageStart, thrownException.GetType());

            return this;
        }

        public override MethodExpectation ToCompleteWithin(TimeSpan time)
        {
            RunIfNeeded();

            if (executionTime <= time)
                throw new MethodExecutionTimeTooFastException(messageStart, time, executionTime);

            return this;
        }

        public override MethodExpectation ToReturn(object? expectedReturnValue)
        {
            RunIfNeeded();

            // If method threw, it didn’t return a value, so negated expectation passes
            if (thrownException is not null)
                return this;

            if (expectedReturnValue == null && returnValue == null)
                throw new InvalidReturnValueException(messageStart, expectedReturnValue, returnValue);

            if (expectedReturnValue?.Equals(returnValue) == true)
                throw new InvalidReturnValueException(messageStart, expectedReturnValue, returnValue);

            return this;
        }

        public new MethodExpectation And() => this;
    }

}
