namespace RandomTesting.LocalCodex.Commands;

public sealed class WriteAllTextCommand : IAgentCommand
{
    public string Name => "write_all_text";
    public string Description => "Writes full text content to a file, overwriting it completely.";

    public bool IsReadonly => false;

    public async Task<string> ExecuteAsync(
        AgentCommandContext context,
        IReadOnlyDictionary<string, string> arguments,
        string thought,
        CancellationToken cancellationToken)
    {
        var path = GetRequired(arguments, "path");
        var content = GetRequired(arguments, "content");
        var overwrite = GetOptionalBool(arguments, "overwrite", true);

        var fullPath = context.ResolvePath(path);

        if (File.Exists(fullPath) && !overwrite)
        {
            return FormatOperationResult(false, $"File already exists: {path}");
        }

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(
            fullPath,
            NormalizeNewlines(content),
            cancellationToken);

        return FormatOperationResult(true, $"Wrote full text to file: {path}");
    }

    private static string NormalizeNewlines(string value)
    {
        return value.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required argument: {name}");
        }

        return value.Trim();
    }

    private static bool GetOptionalBool(IReadOnlyDictionary<string, string> arguments, string name, bool defaultValue)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value.Trim(), out var parsed))
        {
            return parsed;
        }

        return defaultValue;
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
@"Tool: write_all_text

Arguments:
- path: string (required, file path to write to)
- content: string (required, full file content to write)
- overwrite: bool (optional, default = true; allows overwriting existing files)

Notes:
- Overwrites the entire file with the provided content.
- Creates missing directories automatically.
- This is a destructive operation (replaces full file contents).
- Unlike create_file, this command is intended for writing actual content immediately.

Failure points:
- Missing required arguments: path or content
- File exists and overwrite = false
- Invalid or inaccessible file path
- Directory creation failure (permissions or invalid path)
- Disk write failure (disk full, permissions, IO errors)";
    }
}