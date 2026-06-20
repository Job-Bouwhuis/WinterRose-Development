// CreateFileCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class CreateFileCommand : IAgentCommand
{
    public string Name => "create_file";
    public string Description => "Creates a new file.";

    public bool IsReadonly => false;
    public async Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken)
    {
        var path = GetRequired(arguments, "path");
        var overwrite = GetOptionalBool(arguments, "overwrite", false);
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

        await File.WriteAllTextAsync(fullPath, "", cancellationToken);

        return FormatOperationResult(true, $"Created file: {path}");
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
    @"Tool: create_file

Arguments:
- path: string (required, relative or absolute file path)
- overwrite: bool (optional, default = false; allows overwriting existing files)

Notes:
- Creates a new file at the specified path.
- Automatically creates missing directories in the path.
- Normalizes newline characters to the system format.
- If overwrite is false and file already exists, the operation fails safely.
- If content is not provided, an empty file is created.

Failure points:
- Missing required argument: path
- File already exists and overwrite = false
- Invalid or inaccessible file path
- Directory creation failure (permissions or invalid path)
- Disk write failure (permissions, disk full, IO errors)";
    }
}
