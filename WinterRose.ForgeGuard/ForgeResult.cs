using System.Net.NetworkInformation;
using System.Text;

namespace WinterRose.ForgeGuardChecks;

public readonly record struct GuardClauseResult(Severity Severity, string? message, Exception? Exception = null, bool Fatal = false)
{
    public override string ToString()
    {
        if (Severity == Severity.Healthy)
            return $"{Severity}";

        if (!ForgeGuard.IncludeColorInMessageFormat)
            return $"({Severity}) {message} " + (Fatal ? "(Fatal)" : "");

        string colorCode = Severity switch
        {
            Severity.Healthy => "\u001b[32m",               // Green
            Severity.Info => "\u001b[38;5;11m",             // yellow
            Severity.Minor => "\u001b[36m",                 // blue
            Severity.Major => "\u001b[38;5;208m",           // orange
            Severity.Catastrophic => "\u001b[38;5;196m",    // red
            _ => "\u001b[37m",                              // Default white
        };

        string reset = "\u001b[0m";
        string fatalTag = Fatal ? "(Fatal)" : "";

        return $"{colorCode}({Severity}) {message} {fatalTag}{reset}";
    }
}

[System.Diagnostics.DebuggerDisplay("{guardClassType.Name}")]
public readonly struct GuardClassResult(Type guardClassType)
{
    private readonly Type guardClassType = guardClassType;

    public readonly Dictionary<string, GuardClauseResult> GuardResults { get; } = [];
    public Severity HighestSeverity
    {
        get
        {
            Severity severity = Severity.Healthy;
            foreach (var guard in GuardResults)
                if (guard.Value.Severity > severity)
                    severity = guard.Value.Severity;
            return severity;
        }
    }
    public GuardClauseResult? GetFatalResult
    {
        get
        {
            var fatalPair = GuardResults.FirstOrDefault(g => g.Value.Fatal);
            return fatalPair.Equals(default(KeyValuePair<string, GuardClauseResult>)) ? null : fatalPair.Value;
        }
    }

    public void AddGuardResult(string guardName, Severity result) => GuardResults.Add(guardName, new(result, null));
    public void AddGuardResult(string guardName, Severity result, string message) => GuardResults.Add(guardName, new(result, message));
    public void AddGuardresult(string guardName, GuardClauseResult result) => GuardResults.Add(guardName, result);

    public override string ToString()
    {
        return ToStringBuilder().ToString();
    }

    public StringBuilder ToStringBuilder()
    {
        StringBuilder sb = new();

        bool allHealthy = GuardResults.Values.All(r => r.Severity == Severity.Healthy);
        bool hasFailure = !allHealthy;

        if (ForgeGuard.IncludeColorInMessageFormat)
        {
            if (allHealthy)
                sb.Append("\u001b[32m"); // Green
            else
                sb.Append("\u001b[33m"); // Yellow
        }

        sb.Append($"\t[{guardClassType.Name}]");

        if (ForgeGuard.IncludeColorInMessageFormat)
            sb.Append("\u001b[0m"); // Reset

        sb.AppendLine(":");

        foreach (var guard in GuardResults)
        {
            string colorPrefix = "";
            string colorSuffix = "";

            if (ForgeGuard.IncludeColorInMessageFormat)
            {
                colorPrefix = guard.Value.Severity switch
                {
                    Severity.Healthy => "\u001b[32m",       // Green
                    Severity.Info => "\u001b[36m",          // Cyan
                    Severity.Minor => "\u001b[33m",         // Yellow
                    Severity.Major => "\u001b[35m",         // Magenta
                    Severity.Catastrophic => "\u001b[31m",  // Red
                    _ => ""
                };

                if (colorPrefix != "")
                    colorSuffix = "\u001b[0m";
            }

            if(guard.Value.Severity is Severity.Healthy)
                sb.AppendLine("\t\t" + colorPrefix + $"[{guard.Key}] - {guard.Value.ToString().TrimEnd()}" + colorSuffix);
            else
                sb.AppendLine("\t\t" + colorPrefix + guard.Value.ToString().TrimEnd() + colorSuffix);
        }

        return sb;
    }

}

public class GuardResult
{
    public Dictionary<string, GuardClassResult> GuardClassResults { get; }

    public Severity HighestSeverity
    {
        get
        {
            Severity severity = Severity.Healthy;
            foreach (var guard in GuardClassResults)
            {
                Severity highest = guard.Value.HighestSeverity;
                if (highest > severity)
                    severity = highest;
            }
            return severity;
        }
    }

    public GuardResult() => GuardClassResults = [];

    public void AddGuardResult(string guardName, GuardClassResult result) => GuardClassResults.Add(guardName, result);

    public override string ToString()
    {
        StringBuilder sb = new("[ForgeGuard diagnostics result]\n");

        foreach(var guard in GuardClassResults)
            sb.AppendLine($"{guard.Value.ToString()}");

        return sb.ToString();
    }
}
