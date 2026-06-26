// AgentCommandContext.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class AgentCommandContext
{
    public string WorkspaceRoot { get; internal set; }

    public AgentCommandContext(string workspaceRoot)
    {
        WorkspaceRoot = Path.GetFullPath(workspaceRoot);
    }

    public string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new InvalidOperationException("Path is required.");
        }

        var combinedPath = Path.GetFullPath(Path.Combine(WorkspaceRoot, relativePath));
        var rootPath = Path.GetFullPath(WorkspaceRoot);

        if (!IsSamePath(combinedPath, rootPath) && !IsInsideRoot(combinedPath, rootPath))
        {
            throw new InvalidOperationException("Path escapes the workspace root.");
        }

        return combinedPath;
    }

    private static bool IsInsideRoot(string path, string rootPath)
    {
        var comparison = GetComparison();

        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(rootPath);

        return fullPath.StartsWith(fullRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, comparison)
               || string.Equals(fullPath.TrimEnd(Path.DirectorySeparatorChar),
                                fullRoot.TrimEnd(Path.DirectorySeparatorChar),
                                comparison);
    }

    private static bool IsSamePath(string left, string right)
    {
        return string.Equals(left, right, GetComparison());
    }

    private static StringComparison GetComparison()
    {
        return OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    }
}
