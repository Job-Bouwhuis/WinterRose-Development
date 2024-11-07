using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// A utility class to add an If condition to a function call chain.
    /// </summary>
    [Obsolete("Will be removed in a later verison", DiagnosticId = "WR_Obsolete")]
    public class If
    {
        private bool condition;

        private If(bool condition)
        {
            this.condition = condition;
        }

        /// <summary>
        /// Creates an instance of the If class with a boolean condition.
        /// </summary>
        /// <param name="condition">The boolean condition to check.</param>
        /// <returns>A new instance of the If class.</returns>
        public static If Condition(bool condition)
        {
            return new If(condition);
        }

        /// <summary>
        /// Creates an instance of the If class with a boolean condition returned by a function.
        /// </summary>
        /// <param name="condition">A Func that returns the boolean condition to check.</param>
        /// <returns>A new instance of the If class.</returns>
        public static If Condition(Func<bool> condition)
        {
            return new If(condition());
        }

        /// <summary>
        /// Adds an action to execute if the condition is true.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public If Then(Action action)
        {
            if (condition)
                action();
            return this;
        }

        /// <summary>
        /// Adds an action to execute if the condition is false.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void Else(Action action)
        {
            if (!condition)
                action();
        }
    }
}
