using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeGuardChecks
{
    /// <summary>
    /// The severity of a guard
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// No issues found for this guard.
        /// </summary>
        Healthy,
        /// <summary>
        /// There are minor issues with this guard, minor enough to be trivial. 
        /// the application should be able to fix them on its own.
        /// </summary>
        Info,
        /// <summary>
        /// There are issues with this guard, but they are not critical. 
        /// the application may still have issues fixing them on its own, but wont cause a crash.
        /// </summary>
        Minor,
        /// <summary>
        /// There are critical issues with this guard, the application may not be able to fix them on its own.
        /// </summary>
        Major,
        /// <summary>
        /// There are catastrophic issues with this guard, the application is likely to crash or become unusable.
        /// </summary>
        Catastrophic
    }
}
