using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeGuardChecks
{
    /// <summary>
    /// The main class for ForgeGuard operations.
    /// </summary>
    public static class ForgeGuard
    {
        public static bool IncludeColorInMessageFormat { get; set; } = false;
        public static bool ReBrowseAssembliesForGuards { get; set; } = false;
        private static List<GuardClassAttribute> guardClasses = [];
        private static List<MethodInfo> globalSetups = [];
        private static List<MethodInfo> globalTeardowns = [];

        /// <summary>
        /// Scans the loaded assemblies for guard classes
        /// </summary>
        public static void IndexGuards()
        {
            if (guardClasses.Count != 0 && !ReBrowseAssembliesForGuards)
                return;

            var butts = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly ass in butts)
            {
                Type[] cheeks = ass.GetTypes();
                foreach (Type freckle in cheeks)
                {
                    if (freckle == typeof(GuardClassAttribute))
                        continue;
                    if (!guardClasses.Any(x => x.GuardClassType == freckle))
                        if (freckle.GetCustomAttribute<GuardClassAttribute>() is GuardClassAttribute c)
                        {
                            guardClasses.Add(c.FromType(freckle, out var globalSetup, out var globalteardown));
                            if (globalSetup is not null)
                                globalSetups.Add(globalSetup);
                            if (globalteardown is not null)
                                globalTeardowns.Add(globalteardown);
                        }
                }
            }
        }

        public static GuardResult RunGuards(Stream output)
        {
            IndexGuards();
            StreamWriter writer = new(output);
            foreach (var gsetup in globalSetups)
                gsetup.Invoke(null, null);

            GuardResult result = new();

            foreach (var guard in guardClasses)
            {
                GuardClassResult guardResult = guard.Run(writer);
                writer.Flush();

                if (guardResult.GetFatalResult is not null)
                {
                    string stack = Environment.StackTrace;
                    Environment.Exit(500);
                }

                result.AddGuardResult(guard.GuardClassType.Name, guardResult);
            }

            foreach (var gteardown in globalTeardowns)
                gteardown.Invoke(null, null);

            return result;
        }

        internal static string Format(string message, Severity severity)
        {
            if (IncludeColorInMessageFormat)
            {
                string colorCode = severity switch
                {
                    Severity.Info => "\u001b[38;5;11m",             // yellow
                    Severity.Minor => "\u001b[36m",                 // blue
                    Severity.Major => "\u001b[38;5;208m",           // orange
                    Severity.Catastrophic => "\u001b[38;5;196m",    // red
                    _ => "\u001b[37m"                               // white
                };

                string resetCode = "\u001b[0m";
                return $"{colorCode}[{severity}] - {message}{resetCode}\n";
            }

            return $"[{severity}] - {message}\n";
        }

        internal static string Format(string message, Severity severity, bool includeColor)
        {
            if (includeColor)
            {
                string colorCode = severity switch
                {
                    Severity.Info => "\u001b[38;5;11m",             // yellow
                    Severity.Minor => "\u001b[36m",                 // blue
                    Severity.Major => "\u001b[38;5;208m",           // orange
                    Severity.Catastrophic => "\u001b[38;5;196m",    // red
                    _ => "\u001b[0m"
                };

                string resetCode = "\u001b[0m";
                return $"{colorCode}[{severity}] - {message}{resetCode}\n";
            }

            return $"[{severity}] - {message}\n";
        }
    }
}
