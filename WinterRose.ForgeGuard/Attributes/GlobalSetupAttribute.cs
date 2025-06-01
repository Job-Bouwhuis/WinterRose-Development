using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeGuardChecks
{
    /// <summary>
    /// Applied to a guard class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GuardClassAttribute : Attribute
    {
        public MethodInfo? GuardSetup { get; set; }
        public MethodInfo? GuardTeardown { get; set; }

        public MethodInfo? BeforeEach { get; set; }
        public MethodInfo? AfterEach { get; set; }

        public List<(MethodInfo guard, Severity severity)> Guards { get; } = [];

        public GuardClassAttribute FromType(Type t, out MethodInfo? globalSetup, out MethodInfo? globalTeardown)
        {
            globalSetup = null;
            globalTeardown = null;

            var methods = t.GetMethods();

            foreach (var method in methods)
            {
                if (IsDefined(method, typeof(GlobalSetupAttribute), true))
                {
                    if (globalSetup != null)
                        throw new InvalidOperationException($"Multiple [GlobalSetup] methods in '{t.FullName}'. Only one per guard class allowed.");
                    globalSetup = method;
                }

                else if (IsDefined(method, typeof(GlobalTeardownAttribute), true))
                {
                    if (globalTeardown != null)
                        throw new InvalidOperationException($"Multiple [GlobalTeardown] methods in '{t.FullName}'. Only one per guard class allowed.");
                    globalTeardown = method;
                }

                else if (IsDefined(method, typeof(GuardSetupAttribute), true))
                {
                    if (GuardSetup != null)
                        throw new InvalidOperationException($"Multiple [GuardSetup] methods found in '{t.FullName}'. Only one per guard class allowed.");
                    GuardSetup = method;
                }

                else if (IsDefined(method, typeof(GuardTeardownAttribute), true))
                {
                    if (GuardTeardown != null)
                        throw new InvalidOperationException($"Multiple [GuardTeardown] methods found in '{t.FullName}'. Only one per guard class allowed.");
                    GuardTeardown = method;
                }

                else if (IsDefined(method, typeof(BeforeEachAttribute), true))
                {
                    if (BeforeEach != null)
                        throw new InvalidOperationException($"Multiple [BeforeEach] methods found in '{t.FullName}'. Only one per guard class allowed.");
                    BeforeEach = method;
                }

                else if (IsDefined(method, typeof(AfterEachAttribute), true))
                {
                    if (AfterEach != null)
                        throw new InvalidOperationException($"Multiple [AfterEach] methods found in '{t.FullName}'. Only one per guard class allowed.");
                    AfterEach = method;
                }

                else if (method.GetCustomAttribute<GuardAttribute>() is GuardAttribute c)
                {

                    Guards.Add((method, c.severity));
                }
            }

            if (Guards.Count == 0)
                throw new Exception("Guard class has no guards.");

            return this;
        }

        internal Severity Run(StreamWriter output) 
        {
            if (Guards.Count == 0)
                return Severity.Info;
            Severity successful = Severity.Info;

            GuardSetup?.Invoke(null, null);

            foreach(var (guard, severity) in Guards)
            {
                object guardClass = Activator.CreateInstance(guard.DeclaringType);
                BeforeEach.Invoke(guardClass, null);
                try
                {
                    guard.Invoke(guardClass, null);
                }
                catch (TargetInvocationException tie)
                {
                    Exception e = tie.InnerException;
                    output.Write(ForgeGuard.Format($"{e.GetType().Name}: {e.Message}", severity));
                    if (successful < severity)
                        successful = severity;
                }
                AfterEach.Invoke(guardClass, null);
            }

            GuardTeardown?.Invoke(null, null);
            return successful;
        }
    }

    /// <summary>
    /// This attribute should be applied to the global setup method and is ran before any guard class is executed
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GlobalSetupAttribute : Attribute;

    /// <summary>
    /// Apply to the method that does the global teardown and is executed after all guard classes have ran to completion. successful or not
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GlobalTeardownAttribute : Attribute;

    /// <summary>
    /// Applied to a static method in the guard class and runs before any test within thr guard class runs
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GuardSetupAttribute : Attribute;

    /// <summary>
    /// Applied to an instance method in the guard class and runs before each individual test within the guard class
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BeforeEachAttribute : Attribute;

    /// <summary>
    /// Applied to an instance method in the guard class and runs after each individual test within the guard class
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AfterEachAttribute : Attribute;

    /// <summary>
    /// Applied to a static method in the guard class that runs after all guard clauses have ran, whether they were successful or not
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GuardTeardownAttribute : Attribute;

    /// <summary>
    /// Defines a guard method in a guard class
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GuardAttribute : Attribute
    {
        internal Severity severity;
        public GuardAttribute() => severity = Severity.Catastrophic;
        public GuardAttribute(Severity severity) => this.severity = severity;
    }
}
