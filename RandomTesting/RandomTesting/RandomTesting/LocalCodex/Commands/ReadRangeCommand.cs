// ReadRangeCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class ReadRangeCommand : IAgentCommand
{
    public string Name => "read_range";
    public string Description => "Reads a line range from a file.";

    public bool IsReadonly => true;
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

        var selectedLines = lines.Skip(start - 1).Take(end - start + 1);
        var content = string.Join(Environment.NewLine, selectedLines);

        return "RANGE_CONTENT:" + Environment.NewLine +
               $"path={path}" + Environment.NewLine +
               $"start={start}" + Environment.NewLine +
               $"end={end}" + Environment.NewLine +
               "content=" + Environment.NewLine +
               content;
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
    @"Tool: read_range

Arguments:
- path: string (required, file path to read from)
- start: int (required, 1-based start line index)
- end: int (required, 1-based end line index)

Notes:
- Reads and returns a specific range of lines from a file.
- Line indexing is 1-based (first line = 1).
- The range is inclusive (start and end lines are included in output).
- Output is returned as plain text under a content block.
- File content is not modified.

Failure points:
- Missing required arguments (path, start, or end)
- File does not exist at resolved path
- Invalid range (start < 1, end < start, or end > total lines)
- Non-integer start/end values
- File read permission issues
- File is empty or has fewer lines than requested range
- Large ranges may impact performance when reading large files";
    }
}
