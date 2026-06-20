// RenameFileCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class RenameFileCommand : IAgentCommand
{
    public string Name => "rename_file";
    public string Description => "Renames or moves a file.";

    public bool IsReadonly => false;
    public async Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken)
    {
        var oldPath = GetRequired(arguments, "old_path");
        var newPath = GetRequired(arguments, "new_path");
        var fullOldPath = context.ResolvePath(oldPath);
        var fullNewPath = context.ResolvePath(newPath);

        if (!File.Exists(fullOldPath))
        {
            return FormatOperationResult(false, $"File not found: {oldPath}");
        }

        if (File.Exists(fullNewPath))
        {
            return FormatOperationResult(false, $"Destination file already exists: {newPath}");
        }

        var directory = Path.GetDirectoryName(fullNewPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Move(fullOldPath, fullNewPath);

        await Task.CompletedTask;
        return FormatOperationResult(true, $"Renamed {oldPath} to {newPath}");
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required argument: {name}");
        }

        return value.Trim();
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
    @"Tool: rename_file

Arguments:
- old_path: string (required, existing file path)
- new_path: string (required, target file path or new name)

Notes:
- Renames a file or moves it to a new location.
- Automatically creates target directories if they do not exist.
- Operation fails if the source file does not exist or the destination already exists.
- This is a direct filesystem move operation.

Failure points:
- Missing required arguments (old_path or new_path)
- Source file does not exist
- Destination file already exists
- Invalid or inaccessible path format
- Permission denied for source or destination directory
- Target directory creation failure
- File locked by another process";
    }
}
