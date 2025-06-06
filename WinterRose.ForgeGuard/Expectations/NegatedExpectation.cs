using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks.Exceptions;

namespace WinterRose.ForgeGuardChecks.Expectations
{
    public class NegatedExpectation : Expectation
    {
        public override NegatedExpectation Not => throw new NotSupportedException("Can not negate a negated expectation");

        public NegatedExpectation(object? item, string messageStart) : base(item, messageStart)
        {
        }

        /// <summary>
        /// Expects the value to be not null
        /// </summary>
        /// <exception cref="ValueNullException"></exception>
        public override void Null()
        {
            if (item is null)
                throw new ValueNullException(messageStart);
        }

        /// <summary>
        /// Expects the value to be a <see cref="bool"/> and <see langword="false"/>
        /// </summary>
        /// <exception cref="ValueTrueException"></exception>
        public override void True()
        {
            if (item is bool b && b)
                throw new ValueTrueException(messageStart);
        }

        /// <summary>
        /// Expects the value to be a <see cref="bool"/> and <see langword="true"/>
        /// </summary>
        /// <exception cref="ValueFalseException"></exception>
        public override void False()
        {
            if (item is bool b && !b)
                throw new ValueFalseException(messageStart);
        }

        /// <summary>
        /// Expects the value to be not equal to <paramref name="value"/>.<br></br>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ValueNotEqualException"></exception>
        public override void EqualTo(object? value)
        {
            if (Equals(item, value))
                throw new ValueNotEqualException(messageStart, value);
        }

        /// <summary>
        /// Expects the value to not be greater than <paramref name="value"/>.<br></br>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ValueLessThanException"></exception>
        public override void GreaterThan(IComparable value)
        {
            if (item is not IComparable self || self.CompareTo(value) > 0)
                throw new ValueLessThanException(messageStart, value);
        }

        /// <summary>
        /// Expects the value to not be less than <paramref name="value"/>.<br></br>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ValueGreaterThanException"></exception>
        public override void LessThan(IComparable value)
        {
            if (item is not IComparable self || self.CompareTo(value) < 0)
                throw new ValueGreaterThanException(messageStart, value);
        }

        /// <summary>
        /// Expects the value to not be in range of <paramref name="min"/> and <paramref name="max"/>.<br></br>
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <exception cref="ValueNotInRangeException"></exception>
        public override void InRange(IComparable min, IComparable max)
        {
            if (item is not IComparable self || (self.CompareTo(min) >= 0 && self.CompareTo(max) <= 0))
                throw new ValueNotInRangeException(messageStart, min, max);
        }

        /// <summary>
        /// Expects the value to not be empty.
        /// </summary>
        /// <exception cref="ValueEmptyException"></exception>
        public override void Empty()
        {
            if (item is string s && s.Length == 0)
                throw new ValueEmptyException(messageStart);

            if (item is System.Collections.ICollection c && c.Count == 0)
                throw new ValueEmptyException(messageStart);
        }

        /// <summary>
        /// Expects the value to not contain <paramref name="element"/>.<br></br>
        /// </summary>
        /// <param name="element"></param>
        /// <exception cref="ValueContainsException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override void Contains(object? element)
        {
            if (item is System.Collections.IEnumerable e)
            {
                foreach (var i in e)
                {
                    if (Equals(i, element))
                        throw new ValueContainsException(messageStart, element);
                }
            }
            else
            {
                throw new InvalidOperationException($"{messageStart} is not a collection and cannot be checked for containment.");
            }
        }

        /// <summary>
        /// Expects the value to not be of the specified <paramref name="type"/>.<br></br>
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ValueOfTypeException"></exception>
        public override void OfType(Type type)
        {
            if (item != null && type.IsInstanceOfType(item))
                throw new ValueOfTypeException(messageStart, type);
        }

        /// <summary>
        /// Expects the value to not be assignable to the specified <paramref name="type"/>.<br></br>
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ValueAssignableToTypeException"></exception>
        public override void AssignableTo(Type type)
        {
            if (item is not null && type.IsAssignableFrom(item.GetType()))
                throw new ValueAssignableToTypeException(messageStart, type);
        }

        /// <summary>
        /// Expects the value to not match the specified Regex <paramref name="pattern"/>.<br></br>
        /// </summary>
        /// <param name="pattern"></param>
        /// <exception cref="ValueDoesNotMatchException"></exception>
        public override void Matches(string pattern)
        {
            if (item is string s && System.Text.RegularExpressions.Regex.IsMatch(s, pattern))
                throw new ValueDoesNotMatchException(messageStart, pattern);
        }
    }
}
