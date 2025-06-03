using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks.Exceptions;
using WinterRose.ForgeGuardChecks.Expectations;

namespace WinterRose.ForgeGuardChecks
{
    /// <summary>
    /// A class used in guard methods to set rules for data.
    /// </summary>
    public static class Forge
    {
        /// <summary>
        /// Construct an expectation model for the provided <paramref name="value"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value that is expected to be something</param>
        /// <param name="callerName">auto filled by compiler</param>
        /// <param name="expr">auto filled by compiler</param>
        /// <param name="line">auto filled by compiler</param>
        /// <returns>an <see cref="Expectation{T}"/> of <paramref name="value"/></returns>
        public static Expectation Expect<T>(T value, 
            [CallerMemberName] string callerName = "", 
            [CallerArgumentExpression("value")] string expr = "",
            [CallerLineNumber] int line = 0)
        {
            return new(value, FormatErrorLine(callerName, expr, line));
        }

        internal static string FormatErrorLine(string cname, string expr, int line)
        {
            return $"in guard <{cname}> on (line {line}), '{expr}'";
        }
    }
}
