using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks.Exceptions;

namespace WinterRose.ForgeGuardChecks.Expectations
{
    /// <summary>
    /// An expectation for a value
    /// </summary>
    public class Expectation
    {
        protected readonly object? item;
        protected readonly string messageStart;

        public Expectation(object? item, string messageStart)
        {
            this.item = item;
            this.messageStart = messageStart;
        }

        public virtual NegatedExpectation Not => new NegatedExpectation(item, messageStart);

        /// <summary>
        /// Assumes the expected item is a Delegate reference and returns a <see cref="MethodExpectation"/> for it.<br></br>
        /// Throws <see cref="InvalidOperationException"/> if the item is not a delegate.
        /// </summary>
        /// <param name="args">The arguments for the method to call</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual MethodExpectation WhenCalled(params object[] args)
        {
            if (item is not Delegate d)
                throw new InvalidOperationException($"{messageStart} expected to be a delegate, but was {item?.GetType()}");
            return new MethodExpectation(d, args, messageStart);
        }

        /// <summary>
        /// Expects the value to be null
        /// </summary>
        /// <exception cref="ValueNotNullException"></exception>
        public virtual void Null()
        {
            if (item is not null)
                throw new ValueNotNullException(messageStart);
        }

        /// <summary>
        /// Expects the value to be a <see cref="bool"/> and <see langword="true"/>
        /// </summary>
        /// <exception cref="ValueFalseException"></exception>
        public virtual void True()
        {
            OfType<bool>();
            if (item is bool b && !b)
                throw new ValueFalseException(messageStart);
        }
        /// <summary>
        /// Expects the value to be a <see cref="bool"/> and <see langword="false"/>
        /// </summary>
        /// <exception cref="ValueTrueException"></exception>
        public virtual void False()
        {
            OfType<bool>();
            if (item is bool b && b)
                throw new ValueTrueException(messageStart);
        }

        /// <summary>
        /// Expects the value to be equal to <paramref name="value"/>.<br></br>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ValueEqualException"></exception>
        public virtual void EqualTo(object? value)
        {
            if (!Equals(item, value))
                throw new ValueEqualException(messageStart, value);
        }

        /// <summary>
        /// Expects the value to be greater than <paramref name="value"/>.<br></br>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ValueGreaterThanException"></exception>
        public virtual void GreaterThan(IComparable value)
        {
            if (item is not IComparable self || self.CompareTo(value) <= 0)
                throw new ValueGreaterThanException(messageStart, value);
        }

        public void GreaterThanOrEqualTo(int value)
        {
            if (item is not IComparable self || self.CompareTo(value) < 0)
                throw new ValueGreaterOrEqualThanException(messageStart, value);
        }

        public void LessThanOrEqualTo(int value)
        {
            if (item is not IComparable self || self.CompareTo(value) > 0)
                throw new ValueLessOrEqualThanException(messageStart, value);
        }

        /// <summary>
        /// Expects the value to be less than <paramref name="value"/>.<br></br>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ValueLessThanException"></exception>
        public virtual void LessThan(IComparable value)
        {
            if (item is not IComparable self || self.CompareTo(value) >= 0)
                throw new ValueLessThanException(messageStart, value);
        }

        /// <summary>
        /// Expects the value to be in the range of <paramref name="min"/> and <paramref name="max"/>.<br></br>
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <exception cref="ValueNotInRangeException"></exception>
        public virtual void InRange(IComparable min, IComparable max)
        {
            if (item is not IComparable self || self.CompareTo(min) < 0 || self.CompareTo(max) > 0)
                throw new ValueNotInRangeException(messageStart, min, max);
        }

        /// <summary>
        /// Expects the value to be empty.<br></br>
        /// </summary>
        /// <exception cref="ValueNotEmptyException"></exception>
        public virtual void Empty()
        {
            if (item is string s && s.Length != 0)
                throw new ValueNotEmptyException(messageStart);

            if (item is System.Collections.ICollection c && c.Count != 0)
                throw new ValueNotEmptyException(messageStart);
        }

        /// <summary>
        /// Expects the value to contain the specified <paramref name="element"/>.<br></br>
        /// </summary>
        /// <param name="element"></param>
        /// <exception cref="ValueDoesNotContainException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual void Contains(object? element)
        {
            if (item is System.Collections.IEnumerable e)
            {
                foreach (var i in e)
                {
                    if (Equals(i, element))
                        return;
                }
                throw new ValueDoesNotContainException(messageStart, element);
            }

            throw new InvalidOperationException($"{messageStart} is not a collection and cannot be checked for containment.");
        }

        /// <summary>
        /// Expects the value to be of the specified <paramref name="expectedType"/>.<br></br>
        /// </summary>
        /// <param name="expectedType"></param>
        /// <exception cref="ValueNotOfTypeException"></exception>
        public virtual void OfType(Type expectedType)
        {
            if (item is null || !expectedType.IsInstanceOfType(item))
                throw new ValueNotOfTypeException(messageStart, expectedType);
        }

        /// <summary>
        /// Expects the value to be of the specified <typeparamref name="T"/>.<br></br>
        /// </summary>
        /// <param name="expectedType"></param>
        /// <exception cref="ValueNotOfTypeException"></exception>
        public virtual void OfType<T>()
        {
            if (item is null || !typeof(T).IsInstanceOfType(item))
                throw new ValueNotOfTypeException(messageStart, typeof(T));
        }

        /// <summary>
        /// Expects the value to be assignable to the specified <paramref name="expectedBaseType"/>.<br></br>
        /// </summary>
        /// <param name="expectedBaseType"></param>
        /// <exception cref="ValueNotAssignableToTypeException"></exception>
        public virtual void AssignableTo(Type expectedBaseType)
        {
            if (item is null || !expectedBaseType.IsAssignableFrom(item.GetType()))
                throw new ValueNotAssignableToTypeException(messageStart, expectedBaseType);
        }

        /// <summary>
        /// Expects the value to match the specified Regex <paramref name="pattern"/>.<br></br>
        /// </summary>
        /// <param name="pattern"></param>
        /// <exception cref="ValueDoesNotMatchException"></exception>
        public virtual void Matches(string pattern)
        {
            if (item is not string s || !System.Text.RegularExpressions.Regex.IsMatch(s, pattern))
                throw new ValueDoesNotMatchException(messageStart, pattern);
        }
    }
}
