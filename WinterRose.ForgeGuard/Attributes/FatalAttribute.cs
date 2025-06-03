using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeGuardChecks
{
    /// <summary>
    /// When applied to a guard method. if it fails with anything other than <see cref="Severity.Info"/> causes a full crash of the application
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FatalAttribute : Attribute;
}
