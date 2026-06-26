// ApplyPatchCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class ApplyPatchCommand : IAgentCommand
{
    public string Name => "apply_line_patch";
    public string Description => "Applies a unified diff patch to a file.";
    public bool IsReadonly => false;

    public async Task<string> ExecuteAsync(
        AgentCommandContext context,
        IReadOnlyDictionary<string, string> arguments,
        string thought,
        CancellationToken cancellationToken)
    {
        var path = GetRequired(arguments, "path");
        var patch = GetPatch(arguments);

        if(IsPatchNoOp(patch))
        {
            return FormatOperationResult(true, "Patch is a no-op, and no changes were applied. Please provide a valid diff with additions or removals.");
        }

        var fullPath = context.ResolvePath(path);

        if (!File.Exists(fullPath))
        {
            return FormatOperationResult(false, $"File not found: {path}");
        }

        var originalLines = await File.ReadAllLinesAsync(fullPath, cancellationToken);
        var patchedLines = ApplyUnifiedDiff(originalLines, patch);

        if (patchedLines is null)
        {
            return FormatOperationResult(false, "Patch failed to apply cleanly.");
        }

        await File.WriteAllTextAsync(
            fullPath,
            string.Join(Environment.NewLine, patchedLines),
            cancellationToken);

        return FormatOperationResult(true, $"{patchedLines.Count} lines patched in {path}");
    }

    private static List<string>? ApplyUnifiedDiff(string[] original, string patch)
    {
        var originalList = original.ToList();
        var patchLines = patch.Replace("\r\n", "\n").Split('\n');

        var result = new List<string>(originalList);

        int i = 0;

        while (i < patchLines.Length)
        {
            var line = patchLines[i];

            if (!line.StartsWith("@@"))
            {
                i++;
                continue;
            }

            var header = line;
            var (oldStart, oldCount, newStart, newCount) = ParseHeader(header);

            var targetIndex = oldStart - 1;

            i++;

            var chunk = new List<string>();

            while (i < patchLines.Length && !patchLines[i].StartsWith("@@"))
            {
                chunk.Add(patchLines[i]);
                i++;
            }

            var newBlock = new List<string>();
            var removeCount = 0;

            foreach (var c in chunk)
            {
                if (c.StartsWith("+"))
                {
                    newBlock.Add(c[1..]);
                }
                else if (c.StartsWith("-"))
                {
                    removeCount++;
                }
                else if (c.StartsWith(" "))
                {
                    newBlock.Add(c[1..]);
                }
            }

            var safeRemoveCount = Math.Min(removeCount, result.Count - targetIndex);

            if (safeRemoveCount > 0)
                result.RemoveRange(targetIndex, safeRemoveCount);

            result.InsertRange(targetIndex, newBlock);
            result.InsertRange(targetIndex, newBlock);
        }

        return result;
    }

    private static (int oldStart, int oldCount, int newStart, int newCount) ParseHeader(string header)
    {
        // @@ -10,7 +10,8 @@
        var parts = header.Split(' ');

        var oldPart = parts[1]; // -10,7
        var newPart = parts[2]; // +10,8

        var oldSplit = oldPart[1..].Split(',');
        var newSplit = newPart[1..].Split(',');

        return (
            int.Parse(oldSplit[0]),
            oldSplit.Length > 1 ? int.Parse(oldSplit[1]) : 1,
            int.Parse(newSplit[0]),
            newSplit.Length > 1 ? int.Parse(newSplit[1]) : 1
        );
    }

    private static string GetPatch(IReadOnlyDictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("patch", out var value))
        {
            throw new InvalidOperationException("Missing required argument: patch");
        }

        return value;
    }

    public bool IsPatchNoOp(string patch)
    {
        var lines = patch.Split('\n');
        int hunkCount = 0;
        int totalChanges = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("@@"))
                hunkCount++;

            if (line.StartsWith("+") && !line.StartsWith("+++"))
                totalChanges++;
            if (line.StartsWith("-") && !line.StartsWith("---"))
                totalChanges++;
        }

        // No hunks → no changes
        if (hunkCount == 0)
            return true;

        // No additions/removals (only metadata) → no-op
        if (totalChanges == 0)
            return true;

        return false;
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
    @"Tool: apply_line_patch

Arguments:
- path: string (relative or absolute path to the target file)
- patch: string (unified diff format patch)

Notes:
- Applies a unified diff patch to an existing file on disk.
- Patch must follow standard unified diff format with @@ headers.
- Line numbers in the patch are 1-based.
- The tool performs a direct in-place modification of the file.
- If multiple hunks exist, they are applied sequentially.

Failure points:
- Diff syntax given results in no operation
- File not found at resolved path
- Patch format is invalid or cannot be parsed
- Hunk target range does not match file content
- Remove/insert ranges exceed file bounds
- Patch does not apply cleanly due to mismatched context lines
- Empty or missing patch argument
- Permission issues when writing to file";
    }
}
