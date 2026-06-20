// ReadFileCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class ReadFileCommand : IAgentCommand
{
    public string Name => "readfile";
    public string Description => "Reads the full contents of a file.";

    public bool IsReadonly => true;
    private static readonly HashSet<string> IGNORED_EXTENSIONS =
    [
        ".exe",
        ".dll",
        ".pdb",
        ".bin",
        ".obj",
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".bmp",
        ".tga",
        ".ico",
        ".pdf",
        ".zip",
        ".rar",
        ".7z",
        ".tar",
        ".gz",
        ".mp4",
        ".mp3",
        ".wav",
        ".avi",
        ".mov",
        ".ttf",
        ".otf",
        ".woff",
        ".woff2"
    ];

    public async Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken)
    {
        var path = GetRequired(arguments, "path");
        var fullPath = context.ResolvePath(path);

        if (!File.Exists(fullPath))
        {
            return FormatOperationResult(false, $"File not found: {path}");
        }

        if (IGNORED_EXTENSIONS.Contains(Path.GetExtension(fullPath)))
        {
            return FormatOperationResult(false, $"Reading file type not allowed: {path}");
        }

        var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
        return "FILE_CONTENT:" + Environment.NewLine +
               $"path={path}" + Environment.NewLine +
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

    private static string FormatOperationResult(bool success, string message)
    {
        return "OPERATION_RESULT:" + Environment.NewLine +
               $"success={(success ? "true" : "false")}" + Environment.NewLine +
               $"message={message}";
    }

    public string GetToolExample()
    {
        return
    @"Tool: readfile

Arguments:
- path: string (required, file path to read)

Notes:
- Reads the full contents of a file as plain text.
- Returns both metadata (path) and raw file content.
- Does not modify the file in any way.
- Useful for inspection before applying edits or patches.

Failure points:
- Missing required argument: path
- File does not exist at resolved path
- Permission denied when reading file
- File is locked by another process
- Very large files may cause performance or memory issues
- Binary files may produce unreadable output";
    }
}
