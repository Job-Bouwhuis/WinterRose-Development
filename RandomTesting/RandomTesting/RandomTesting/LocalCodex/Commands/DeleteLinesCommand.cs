// DeleteLinesCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class DeleteLinesCommand : IAgentCommand
{
    public string Name => "delete_lines";
    public string Description => "Deletes a range of lines from a file.";

    public bool IsReadonly => false;
    public async Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken)
    {
        var path = GetRequired(arguments, "path");
        var start = GetRequiredInt(arguments, "start");
        var end = GetRequiredInt(arguments, "end");
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

        var combinedLines = new List<string>();
        combinedLines.AddRange(lines.Take(start - 1));
        combinedLines.AddRange(lines.Skip(end));

        await File.WriteAllTextAsync(fullPath, string.Join(Environment.NewLine, combinedLines), cancellationToken);

        return FormatOperationResult(true, $"Deleted lines {start}-{end} in {path}");
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required argument: {name}");
        }

        return value.Trim();
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
    @"Tool: delete_lines

Arguments:
- path: string (required, file path to modify)
- start: int (required, 1-based start line index)
- end: int (required, 1-based end line index)

Notes:
- Deletes a continuous range of lines from a file.
- Line indexing is 1-based (first line = 1).
- The range is inclusive (start and end lines are removed).
- File is rewritten after modification.
- Remaining lines are preserved in order.

Failure points:
- Missing required arguments (path, start, or end)
- File does not exist at resolved path
- Invalid line range (start < 1, end < start, or end > total lines)
- Non-integer values for start/end
- File read/write permission issues
- File changes during execution causing mismatched state";
    }
}
