// OverrideLinesCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class OverrideLinesCommand : IAgentCommand
{
    public string Name => "override_lines";
    public string Description => "Replaces a range of lines in a file.";

    public bool IsReadonly => false;
    public async Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken)
    {
        var path = GetRequired(arguments, "path");
        var start = GetRequiredInt(arguments, "start");
        var end = GetRequiredInt(arguments, "end");
        var content = GetOptional(arguments, "content");
        var fullPath = context.ResolvePath(path);

        if (!File.Exists(fullPath))
        {
            return FormatOperationResult(false, $"File not found: {path}");
        }

        var lines = await File.ReadAllLinesAsync(fullPath, cancellationToken);

        if (start < 1 || end < 1 || start > end || end > lines.Length)
        {
            return FormatOperationResult(false, $"Invalid range {start}-{end} for file with {lines.Length} lines.");
        }

        var newLines = SplitLines(content);
        var combinedLines = new List<string>();

        combinedLines.AddRange(lines.Take(start - 1));
        combinedLines.AddRange(newLines);
        combinedLines.AddRange(lines.Skip(end));

        await File.WriteAllTextAsync(fullPath, string.Join(Environment.NewLine, combinedLines), cancellationToken);

        return FormatOperationResult(true, $"Replaced lines {start}-{end} in {path}");
    }

    private static string[] SplitLines(string content)
    {
        return content.Replace("\r\n", "\n").Split('\n');
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required argument: {name}");
        }

        return value.Trim();
    }

    private static string GetOptional(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value))
        {
            return string.Empty;
        }

        return value;
    }

    private static int GetRequiredInt(IReadOnlyDictionary<string, string> arguments, string name)
    {
        var rawValue = GetRequired(arguments, name);
        if (!int.TryParse(rawValue, out var value))
        {
            throw new InvalidOperationException($"Invalid integer for {name}: {rawValue}");
        }

        return value;
    }

    private static string FormatOperationResult(bool success, string message)
    {
        return "OPERATION_RESULT:" + Environment.NewLine +
               $"success={(success ? "true" : "false")}" + Environment.NewLine +
               $"message={message}";
    }

    public string GetToolExample()
    {
        return
    @"Tool: override_lines

Arguments:
- path: string (required, file path to modify)
- start: int (required, 1-based start line index)
- end: int (required, 1-based end line index)
- content: string (optional, replacement text; split into multiple lines)

Notes:
- Replaces a continuous range of lines in an existing file.
- Line indexing is 1-based (first line = 1).
- The replacement content is split by newline characters and inserted as multiple lines.
- The operation preserves all lines outside the specified range.
- File is fully rewritten after applying changes.

Failure points:
- Missing required arguments (path, start, or end)
- File does not exist at resolved path
- Invalid line range (start < 1, end < start, or end > total lines)
- Non-integer start/end values
- File read/write permission issues
- Replacement content may be empty, resulting in deletion-like behavior
- File modified externally during execution leading to mismatched state";
    }
}
