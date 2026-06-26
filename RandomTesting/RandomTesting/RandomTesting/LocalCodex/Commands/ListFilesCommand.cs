// ListFilesCommand.cs
using System.Text;

namespace RandomTesting.LocalCodex.Commands;

public sealed class ListFilesCommand : IAgentCommand
{
    public string Name => "list_files";
    public string Description => "Lists files and directories in a path with optional filtering.";

    public bool IsReadonly => true;
    public Task<string> ExecuteAsync(
        AgentCommandContext context,
        IReadOnlyDictionary<string, string> arguments,
        string thought,
        CancellationToken cancellationToken)
    {
        var path = GetRequired(arguments, "path");
        var pattern = GetOptional(arguments, "pattern");

        var fullPath = context.ResolvePath(path);

        if (!Directory.Exists(fullPath))
        {
            return Task.FromResult(FormatOperationResult(false, $"Directory not found: {path}"));
        }

        var files = Directory.GetFiles(fullPath, "*");
        var dirs = Directory.GetDirectories(fullPath, "*");

        var filteredFiles = ApplyPattern(files, pattern);
        var filteredDirs = ApplyPattern(dirs, pattern);

        var result = new StringBuilder();
        result.AppendLine("LIST_FILES:");
        result.AppendLine($"path={path}");
        result.AppendLine("items=");

        foreach (var dir in filteredDirs)
        {
            result.AppendLine(ToRelative(fullPath, dir) + "/");
        }

        foreach (var file in filteredFiles)
        {
            result.AppendLine(ToRelative(fullPath, file));
        }

        return Task.FromResult(result.ToString());
    }

    private static IEnumerable<string> ApplyPattern(IEnumerable<string> items, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return items;
        }

        pattern = pattern.Trim();

        return items.Where(item =>
        {
            var fileName = Path.GetFileName(item);

            return MatchesGlob(fileName, pattern);
        });
    }

    private static bool MatchesGlob(string text, string pattern)
    {
        var parts = pattern.Split('*', StringSplitOptions.None);

        if (parts.Length == 1)
        {
            return text.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }

        int index = 0;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];

            if (string.IsNullOrEmpty(part))
            {
                continue;
            }

            var foundIndex = text.IndexOf(part, index, StringComparison.OrdinalIgnoreCase);

            if (foundIndex == -1)
            {
                return false;
            }

            if (i == 0 && !pattern.StartsWith("*") && foundIndex != 0)
            {
                return false;
            }

            index = foundIndex + part.Length;
        }

        if (!pattern.EndsWith("*"))
        {
            var lastPart = parts[^1];
            if (!text.EndsWith(lastPart, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static string ToRelative(string root, string fullPath)
    {
        return Path.GetRelativePath(root, fullPath);
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
    @"Tool: list_files

Arguments:
- path: string (required, directory path to list)
- pattern: string (optional, simple filter applied to file/folder names)

Notes:
- Lists both files and directories under the specified path.
- Directories are marked with a trailing '/'.
- Output paths are relative to the provided root path.
- Pattern filtering is case-insensitive and matches substrings after removing '*' wildcards.
- When recursive is enabled, all nested files and folders are included.

Failure points:
- Missing required argument: path
- Directory does not exist at resolved path
- Permission issues when accessing filesystem
- Invalid or inaccessible path format
- Large directory trees may cause performance delays when recursive = true";
    }
}
