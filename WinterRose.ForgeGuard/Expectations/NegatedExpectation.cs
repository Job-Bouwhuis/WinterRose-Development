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
        public override Expectation Not => new Expectation(item, messageStart);

        public NegatedExpectation(object? item, string messageStart) : base(item, messageStart)
        {
        }

        public override void Null()
        {
            if (item is null)
                throw new ValueNullException(messageStart);
        }

        public override void True()
        {
            if (item is bool b && b)
                throw new ValueTrueException(messageStart);
        }

        public override void False()
        {
            if (item is bool b && !b)
                throw new ValueFalseException(messageStart);
        }

        public override void EqualTo(object? value)
        {
            if (Equals(item, value))
                throw new ValueNotEqualException(messageStart, value);
        }

        public override void GreaterThan(IComparable value)
        {
            if (item is not IComparable self || self.CompareTo(value) > 0)
                throw new ValueLessThanException(messageStart, value);
        }

        public override void LessThan(IComparable value)
        {
            if (item is not IComparable self || self.CompareTo(value) < 0)
                throw new ValueGreaterThanException(messageStart, value);
        }

        public override void InRange(IComparable min, IComparable max)
        {
            if (item is not IComparable self || (self.CompareTo(min) >= 0 && self.CompareTo(max) <= 0))
                throw new ValueNotInRangeException(messageStart, min, max);
        }

        public override void Empty()
        {
            if (item is string s && s.Length == 0)
                throw new ValueEmptyException(messageStart);

            if (item is System.Collections.ICollection c && c.Count == 0)
                throw new ValueEmptyException(messageStart);
        }

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

        public override void OfType(Type type)
        {
            if (item != null && type.IsInstanceOfType(item))
                throw new ValueNotOfTypeException(messageStart, type);
        }

        public override void AssignableTo(Type type)
        {
            if (item is not null && type.IsAssignableFrom(item.GetType()))
                throw new ValueNotAssignableToTypeException(messageStart, type);
        }

        public override void Matches(string pattern)
        {
            if (item is string s && System.Text.RegularExpressions.Regex.IsMatch(s, pattern))
                throw new ValueDoesNotMatchException(messageStart, pattern);
        }

        public override void Is(object? expected)
        {
            if (Equals(item, expected))
                throw new ValueUnexpectedException(messageStart, expected, item);
        }
    }


}
