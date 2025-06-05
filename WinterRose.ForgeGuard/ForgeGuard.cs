using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeGuardChecks
{
    public class ForgeGuardTagNotFound(IEnumerable<string> tags)
    : Exception($"No guards found with the tags [{string.Join(", ", tags)}]. Make sure at least one guard with each tag is defined in the loaded assemblies. " +
                "If you are using custom tags, make sure they are registered correctly.");

    public class ForgeGuardNoGuardsFoundException()
        : Exception("No guards were found in the loaded assemblies. Make sure at least one guard class is defined and properly attributed.");

    /// <summary>
    /// The main class for ForgeGuard operations.
    /// </summary>
    public static class ForgeGuard
    {
        /// <summary>
        /// Whether the messages should include ANSI color codes for better readability for text renderers that support it. (such as terminals)
        /// </summary>
        public static bool IncludeColorInMessageFormat { get; set; } = false;
        /// <summary>
        /// Whether the system should reindex the assemblies for guard classes on each run of <see cref="RunGuards(Stream)"/>.
        /// </summary>
        public static bool ReBrowseAssembliesForGuards { get; set; } = false;
        private static List<GuardClassAttribute> guardClasses = [];
        private static List<MethodInfo> globalSetups = [];
        private static List<MethodInfo> globalTeardowns = [];

        /// <summary>
        /// Scans the loaded assemblies for guard classes
        /// </summary>
        private static void IndexGuards()
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

        public static GuardResult Run(Stream output, params string[] tags) => Run(output, true, tags);

        /// <summary>
        /// Runs all the guards found in all the currently loaded assemblies
        /// </summary>
        /// <param name="output">The stream where all guards that did not return <see cref="Severity.Healthy"/> have their error code sent to. For a full diagnostics, including healthy ones. use <see cref="GuardResult.ToString()"/></param>
        /// <returns>a summary result object of all the guards than ran. assuming no guards marked with <see cref="FatalAttribute"/> failed.</returns>
        public static GuardResult Run(Stream output, bool throwOnNotFoundTags, params string[] tags)
        {
            IndexGuards();
            StreamWriter writer = new(output);

            if(guardClasses.Count == 0)
                throw new ForgeGuardNoGuardsFoundException();

            foreach (var gsetup in globalSetups)
                gsetup.Invoke(null, null);

            GuardResult result = new();

            int foundGuards = 0;

            foreach (var guard in guardClasses)
            {
                if(tags.Length > 0 && !tags.Contains(guard.Tag))
                    continue;
                foundGuards++;
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

            if(foundGuards == 0 && throwOnNotFoundTags)
                throw new ForgeGuardTagNotFound(tags);

            return result;
        }

        /// <summary>
        /// relies on <see cref="IncludeColorInMessageFormat"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
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

        /// <summary>
        /// does not rely on <see cref="IncludeColorInMessageFormat"/> but instead takes a parameter to determine whether to include color codes or not.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <param name="includeColor"></param>
        /// <returns></returns>
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
