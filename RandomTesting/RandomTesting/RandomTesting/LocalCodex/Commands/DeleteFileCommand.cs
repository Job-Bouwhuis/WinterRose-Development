// DeleteFileCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class DeleteFileCommand : IAgentCommand
{
    public string Name => "delete_file";
    public string Description => "Deletes an existing file.";

    public bool IsReadonly => false;

    public async Task<string> ExecuteAsync(
       AgentCommandContext context,
       IReadOnlyDictionary<string, string> arguments,
       string thought,
       CancellationToken cancellationToken)
    {
        var path = GetRequired(arguments, "path");
        var fullPath = context.ResolvePath(path);

        if (!File.Exists(fullPath))
        {
            return FormatOperationResult(false, $"File not found: {path}");
        }

        var exeDirectory = AppContext.BaseDirectory;
        var trashRoot = Path.Combine(exeDirectory, "trash");

        Directory.CreateDirectory(trashRoot);

        var fileName = Path.GetFileName(fullPath);
        var uniqueName = $"{Guid.NewGuid():N}_{fileName}";
        var trashPath = Path.Combine(trashRoot, uniqueName);

        File.Move(fullPath, trashPath);

        await Task.CompletedTask;

        return FormatOperationResult(true, $"Moved file to trash: {path}");
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
    @"Tool: delete_file (moves to trash)

Arguments:
- path: string (required, relative or absolute file path)

Notes:
- Instead of deleting, moves file into a trash folder next to the executable.
- Each deleted file gets a unique name to avoid collisions.
- Trash folder is auto-created if missing.

Failure points:
- Missing required argument: path
- File does not exist
- Invalid or inaccessible file path
- Permission denied while moving
- File is locked by another process";
    }
}