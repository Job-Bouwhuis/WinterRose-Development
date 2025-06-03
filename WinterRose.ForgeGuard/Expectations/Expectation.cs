using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks.Exceptions;

namespace WinterRose.ForgeGuardChecks.Expectations
{
    public class Expectation
    {
        protected readonly object? item;
        protected readonly string messageStart;

        public Expectation(object? item, string messageStart)
        {
            this.item = item;
            this.messageStart = messageStart;
        }

        public virtual Expectation Not => new NegatedExpectation(item, messageStart);

        public virtual MethodExpectation WhenCalled(params object[] args)
        {
            if (item is not Delegate d)
                throw new InvalidOperationException($"{messageStart} expected to be a delegate, but was {item?.GetType()}");
            return new MethodExpectation(d, args, messageStart);
        }

        public virtual void Null()
        {
            if (item is not null)
                throw new ValueNotNullException(messageStart);
        }

        public virtual void True()
        {
            if (item is bool b && !b)
                throw new ValueFalseException(messageStart);
        }

        public virtual void False()
        {
            if (item is bool b && b)
                throw new ValueTrueException(messageStart);
        }

        public virtual void EqualTo(object? value)
        {
            if (!Equals(item, value))
                throw new ValueEqualException(messageStart, value);
        }

        public virtual void GreaterThan(IComparable value)
        {
            if (item is not IComparable self || self.CompareTo(value) <= 0)
                throw new ValueGreaterThanException(messageStart, value);
        }

        public virtual void LessThan(IComparable value)
        {
            if (item is not IComparable self || self.CompareTo(value) >= 0)
                throw new ValueLessThanException(messageStart, value);
        }

        public virtual void InRange(IComparable min, IComparable max)
        {
            if (item is not IComparable self || self.CompareTo(min) < 0 || self.CompareTo(max) > 0)
                throw new ValueNotInRangeException(messageStart, min, max);
        }

        public virtual void Empty()
        {
            if (item is string s && s.Length != 0)
                throw new ValueNotEmptyException(messageStart);

            if (item is System.Collections.ICollection c && c.Count != 0)
                throw new ValueNotEmptyException(messageStart);
        }

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

        public virtual void OfType(Type expectedType)
        {
            if (item is null || !expectedType.IsInstanceOfType(item))
                throw new ValueNotOfTypeException(messageStart, expectedType);
        }

        public virtual void AssignableTo(Type expectedBaseType)
        {
            if (item is null || !expectedBaseType.IsAssignableFrom(item.GetType()))
                throw new ValueNotAssignableToTypeException(messageStart, expectedBaseType);
        }

        public virtual void Matches(string pattern)
        {
            if (item is not string s || !System.Text.RegularExpressions.Regex.IsMatch(s, pattern))
                throw new ValueDoesNotMatchException(messageStart, pattern);
        }

        public virtual void Is(object? expected)
        {
            if (!Equals(item, expected))
                throw new ValueUnexpectedException(messageStart, expected, item);
        }
    }
}
