// SearchTextCommand.cs
using System.Text;

namespace RandomTesting.LocalCodex.Commands;

public sealed class SearchTextCommand : IAgentCommand
{
    public string Name => "search_text";
    public string Description => "Searches for text inside files in a directory.";

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

    public async Task<string> ExecuteAsync(
        AgentCommandContext context,
        IReadOnlyDictionary<string, string> arguments,
        string thought,
        CancellationToken cancellationToken)
    {
        var query = GetRequired(arguments, "query");
        var path = GetOptional(arguments, "path");
        var filePattern = GetOptional(arguments, "file_pattern");
        var ignoredExtensionsArg = GetOptional(arguments, "ignored_extensions");
        var ignoredExtensions = ParseIgnoredExtensions(ignoredExtensionsArg).Union(IGNORED_EXTENSIONS).ToHashSet();

        var fullPath = string.IsNullOrWhiteSpace(path)
            ? context.WorkspaceRoot
            : context.ResolvePath(path);

        if (!Directory.Exists(fullPath))
        {
            return FormatOperationResult(false, $"Directory not found: {path}");
        }

        var files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);

        files = files
            .Where(f => !ignoredExtensions.Contains(Path.GetExtension(f)))
            .ToArray();

        var result = new StringBuilder();
        result.AppendLine("SEARCH_RESULTS:");
        result.AppendLine($"query={query}");
        result.AppendLine("matches=");

        foreach (var file in files)
        {
            var lines = await File.ReadAllLinesAsync(file, cancellationToken);

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    var relative = Path.GetRelativePath(fullPath, file);
                    result.AppendLine($"{relative}:{i + 1}: {lines[i].Trim()}");
                }
            }
        }

        return result.ToString();
    }

    private static HashSet<string> ParseIgnoredExtensions(string input)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(input))
        {
            return result;
        }

        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var ext = part.StartsWith(".") ? part : "." + part;
            result.Add(ext);
        }

        return result;
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
        return arguments.TryGetValue(name, out var value) ? value : string.Empty;
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
    @"Tool: search_text

Arguments:
- query: string (required, text to search for inside files)
- path: string (optional, directory to search in; defaults to workspace root)
- file_pattern: string (optional, simple filename filter)
- ignored_extensions: list<string> (optional, file extensions excluded from search)

Default ignored extensions (if not provided):
- .exe
- .dll
- .bin
- .obj
- .png
- .jpg
- .jpeg
- .gif
- .bmp
- .tga
- .ico
- .pdf
- .zip
- .rar
- .7z
- .tar
- .gz
- .mp4
- .mp3
- .wav
- .avi
- .mov
- .ttf
- .otf
- .woff
- .woff2

Notes:
- Recursively searches through all files in a directory.
- Matches are case-insensitive substring searches.
- Binary and media files are excluded by default via ignored_extensions.
- Returns results in format: relative_path:line_number: line_content
- If no path is provided, the workspace root is used.
- File pattern filtering applies before searching content.
- Large directory trees may impact performance.

Failure points:
- Missing required argument: query
- Directory does not exist or is inaccessible
- Permission denied when reading files
- Very large directory trees may cause performance issues
- Large files may cause high memory usage
- File encoding issues may produce incorrect or unreadable matches
- File access conflicts if files are locked during read
- Incorrect extension filtering may skip relevant files";
    }
}
