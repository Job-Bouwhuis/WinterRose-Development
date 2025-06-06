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
        /// <summary>
        /// A non-negated version of this expectation.
        /// </summary>
        public override MethodExpectation Not => new MethodExpectation(method, args, messageStart);

        public NegatedMethodExpectation(Delegate method, object[] args, string messageStart)
            : base(method, args, messageStart)
        {
        }

        /// <summary>
        /// Expects the method to not throw an exception of type <typeparamref name="T"/>. allows other exceptions to be thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>the same reference</returns>
        /// <exception cref="UnexpectedExceptionThrownException"></exception>
        public override MethodExpectation ToThrow<T>()
        {
            RunIfNeeded();

            if (thrownException is T)
                throw new UnexpectedExceptionThrownException(messageStart, typeof(T));

            return this;
        }

        /// <summary>
        /// Expects the method to not throw any exception.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnexpectedExceptionThrownException"></exception>
        public override MethodExpectation ToThrowAny()
        {
            RunIfNeeded();

            if (thrownException is not null)
                throw new UnexpectedExceptionThrownException(messageStart, thrownException.GetType());

            return this;
        }

        /// <summary>
        /// Expects the method to take longer than <paramref name="time"/> to execute.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <exception cref="MethodExecutionTimeTooFastException"></exception>
        public override MethodExpectation ToCompleteWithin(TimeSpan time)
        {
            RunIfNeeded();

            if (executionTime <= time)
                throw new MethodExecutionTimeTooFastException(messageStart, time, executionTime);

            return this;
        }

        /// <summary>
        /// Expects the return value of the method to be not equal to <paramref name="returnValue"/>
        /// </summary>
        /// <param name="expectedReturnValue"></param>
        /// <returns></returns>
        /// <exception cref="InvalidReturnValueException"></exception>
        public override MethodExpectation ToReturn(object? returnValue)
        {
            RunIfNeeded();

            // If method threw, it didn’t return a value, so negated expectation passes
            if (thrownException is not null)
                return this;

            if (returnValue == null && base.returnValue == null)
                throw new InvalidReturnValueException(messageStart, returnValue, base.returnValue);

            if (returnValue?.Equals(base.returnValue) == true)
                throw new InvalidReturnValueException(messageStart, returnValue, base.returnValue);

            return this;
        }
    }

}
