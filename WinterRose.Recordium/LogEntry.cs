using System.Diagnostics;

namespace WinterRose.Recordium;

public enum LogVerbosity
{
    Minimal,
    Normal,
    Detailed,
    Full
}

public enum LogFragmentType
{
    Timestamp,
    Severity,
    Category,
    Message,
    FileLocation,
    Thread,
    Exception,
    Custom
}

public class CategoryFragment : ILogFragment
{
    public LogFragmentType Type => LogFragmentType.Severity;
    public string Category { get; set; }
    public string FormatValue() => Category;
}

public class SeverityFragment : ILogFragment
{
    public LogFragmentType Type => LogFragmentType.Severity;
    public LogSeverity Severity { get; set; }
    public string FormatValue() => Severity.ToString();
}

public class TimestampFragment : ILogFragment
{
    public LogFragmentType Type => LogFragmentType.Timestamp;
    public DateTime Value;
    public string TimestampFormat { get; set; } = "dd-MMM-yy HH:mm:ss";

    public string FormatValue() => Value.ToString(TimestampFormat);
}

public class MessageFragment : ILogFragment
{
    public LogFragmentType Type => LogFragmentType.Message;
    public string Value;
    public string FormatValue() => Value;
}

public class ExceptionFragment : ILogFragment
{
    public LogFragmentType Type => LogFragmentType.Exception;
    public Exception Exception;

    public string FormatValue() => FormatException(Exception);

    private static string FormatException(Exception ex, int indentLevel = 0)
    {
        string indent = new string('\t', indentLevel);
        string result = $"{Environment.NewLine}{indent}Exception: {ex.GetType().FullName}: {ex.Message}";
        result += $"{Environment.NewLine}{indent}{ex.StackTrace}";

        if (ex.InnerException != null)
            result += $"{Environment.NewLine}{indent}Inner Exception →" +
                      FormatException(ex.InnerException, indentLevel + 1);

        if(indentLevel == 0)
            result += $"{Environment.NewLine}{Environment.NewLine}";
        return result;
    }
}

public class FileLocationFragment : ILogFragment
{
    public LogFragmentType Type => LogFragmentType.FileLocation;
    public string FileName;
    public int LineNumber;

    public string FormatValue() => $"{FileName}:{LineNumber}";
}

public interface ILogFragment
{
    LogFragmentType Type { get; }
    string FormatValue();
}

public record PrintableLogFragment(string Fragment, LogFragmentType Type)
{
    public override string ToString() => Fragment;
}

/// <summary>
/// A log entry in the Recordium logging system
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public class LogEntry
{
    private string DebuggerDisplay => ToString(LogVerbosity.Minimal);

    /// <summary>
    /// The severity of the log
    /// </summary>
    public LogSeverity Severity { get; set; }
    /// <summary>
    /// The log message
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// When the log was submitted
    /// </summary>
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// The category for the log. eg "Renderer" or "IO"
    /// </summary>
    public string Category { get; set; }
    /// <summary>
    /// The exception that was thrown (may be null if the log does not concern an exception)
    /// </summary>
    public Exception? Exception { get; set; }
    /// <summary>
    /// The stack trace of the exception thrown (null if <see cref="LogEntry.Exception"/> is also null)
    /// </summary>
    public StackTrace? StackTrace { get; set; }
    /// <summary>
    /// The file name where the log originates from
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// The line number within the file where the log originates from
    /// </summary>
    public int LineNumber { get; set; }
    /// <summary>
    /// The threadID of the thread that submitted the log
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// Creates a log entry
    /// </summary>
    public LogEntry(LogSeverity severity, string category, string message, string? fileName, int lineNumber, int threadId)
    {
        Severity = severity;
        Message = message;
        Category = category;
        FileName = fileName ?? "";
        LineNumber = lineNumber;
        ThreadId = threadId;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a log entry with an exception
    /// </summary>
    public LogEntry(
        LogSeverity severity,
        Exception? exception,
        string category,
        string? message = null,
        string? fileName = null,
        int lineNumber = 0,
        int threadId = 0)
    {
        Severity = severity;
        Exception = exception;
        Message = message ?? "";
        Category = category;
        FileName = fileName ?? "";
        LineNumber = lineNumber;
        ThreadId = threadId;
        Timestamp = DateTime.UtcNow;
        StackTrace = exception != null ? new StackTrace(exception, true) : null;
    }
    
    public override string ToString() => ToString(LogVerbosity.Detailed);

    public IReadOnlyList<PrintableLogFragment> GetFragments(LogVerbosity verbosity)
    {
        string template = Log.GetTemplate(verbosity);

        var fragmentMap = new Dictionary<string, ILogFragment?>(StringComparer.OrdinalIgnoreCase)
        {
            ["time"] = new TimestampFragment { Value = Timestamp },
            ["severity"] = new SeverityFragment { Severity = Severity },
            ["category"] = new CategoryFragment { Category = Category },
            ["message"] = new MessageFragment { Value = Message },

            ["file"] = string.IsNullOrWhiteSpace(FileName)
                ? null
                : new FileLocationFragment
                {
                    FileName = Path.GetFileName(FileName),
                    LineNumber = LineNumber
                },

            ["thread"] = ThreadId == 0
                ? null
                : new MessageFragment { Value = ThreadId.ToString() },

            ["exception"] = Exception == null
                ? null
                : new ExceptionFragment { Exception = Exception }
        };

        var result = new List<PrintableLogFragment>();

        ResolveText(template, result, fragmentMap);

        return result;
    }
    string ResolveToken(string key, Dictionary<string, ILogFragment?> fragmentMap)
    {
        if (!fragmentMap.TryGetValue(key, out ILogFragment? fragment) || fragment == null)
            return "";

        string value = fragment.FormatValue();
        return string.IsNullOrWhiteSpace(value) ? "" : value;
    }

    void ResolveText(
string input,
List<PrintableLogFragment> output,
Dictionary<string, ILogFragment?> fragmentMap)
    {
        int i = 0;

        while (i < input.Length)
        {
            int open = input.IndexOf('{', i);

            if (open == -1)
            {
                EmitLiteral(input[i..], output);
                break;
            }

            EmitLiteral(input[i..open], output);

            int close = FindMatchingBrace(input, open);

            if (close == -1)
            {
                EmitLiteral(input[open..], output);
                break;
            }

            string token = input.Substring(open + 1, close - open - 1);

            // CONDITIONAL
            if (token.StartsWith("?"))
            {
                int colon = token.IndexOf(':');

                if (colon == -1)
                {
                    i = close + 1;
                    continue;
                }

                string conditionKey = token[1..colon];
                string rest = token[(colon + 1)..];

                int pipe = rest.IndexOf('|');

                string trueBranch;
                string falseBranch = "";

                if (pipe == -1)
                {
                    trueBranch = rest;
                }
                else
                {
                    trueBranch = rest[..pipe];
                    falseBranch = rest[(pipe + 1)..];
                }

                bool isValid =
                    fragmentMap.TryGetValue(conditionKey, out ILogFragment? f) &&
                    f != null &&
                    !string.IsNullOrWhiteSpace(f.FormatValue());

                string chosen = isValid ? trueBranch : falseBranch;

                if (!string.IsNullOrWhiteSpace(chosen))
                {
                    ResolveText(chosen, output, fragmentMap);
                }

                i = close + 1;
                continue;
            }

            // NORMAL TOKEN
            string resolved = ResolveToken(token, fragmentMap);

            if (!string.IsNullOrWhiteSpace(resolved))
            {
                output.Add(new PrintableLogFragment(
                    resolved,
                    fragmentMap.TryGetValue(token, out var f2) && f2 != null
                        ? f2.Type
                        : LogFragmentType.Custom));
            }

            i = close + 1;
        }
    }

    void EmitLiteral(string text, List<PrintableLogFragment> output)
    {
        if (!string.IsNullOrEmpty(text))
        {
            output.Add(new PrintableLogFragment(
                text,
                LogFragmentType.Custom));
        }
    }

    int FindMatchingBrace(string text, int openIndex)
    {
        int depth = 0;

        for (int i = openIndex; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                depth++;
            }
            else if (text[i] == '}')
            {
                depth--;

                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    public string ToString(LogVerbosity verbosity)
    {
        var fragments = GetFragments(verbosity);

        var output = new System.Text.StringBuilder();

        for (int i = 0; i < fragments.Count; i++)
        {
            PrintableLogFragment? fragment = fragments[i];
            output.Append(fragment);
        }

        return output.ToString().TrimEnd();
    }
}
