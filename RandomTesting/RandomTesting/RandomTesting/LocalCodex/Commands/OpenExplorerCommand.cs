// OpenExplorerCommand.csusing System.Diagnostics;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RandomTesting.LocalCodex.Commands;

public sealed class OpenExplorerCommand : IAgentCommand
{
    public string Name => "open_explorer";
    public string Description => "Opens the file explorer for a specified path (Windows: Explorer, Linux: Dolphin)";

    public bool IsReadonly => true; // This command doesn't modify files

    public async Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken)
    {
        try
        {
            var path = GetRequired(arguments, "path");
            var fullPath = context.ResolvePath(path);

            // Validate that the path exists
            if (!Directory.Exists(fullPath) && !File.Exists(fullPath))
            {
                return FormatOperationResult(false, $"Path not found: {fullPath}");
            }

            // Determine OS at runtime and open appropriate explorer
            string? explorerCommand = null;
            string[]? explorerArgs = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: use explorer.exe
                explorerCommand = "explorer.exe";
                explorerArgs = new[] { $"\"{fullPath}\"" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux: try dolphin first, then fallback to other file managers
                var fileManagers = new[] { "dolphin", "nautilus", "thunar", "pcmanfm" };
                foreach (var manager in fileManagers)
                {
                    if (IsCommandAvailable(manager))
                    {
                        explorerCommand = manager;
                        explorerArgs = new[] { $"\"{fullPath}\"" };
                        break;
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS: use open command
                explorerCommand = "open";
                explorerArgs = new[] { "\"{fullPath}\"" };
            }

            if (explorerCommand != null && explorerArgs != null)
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = explorerCommand,
                    Arguments = string.Join(" ", explorerArgs),
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
                return FormatOperationResult(true, $"Opened explorer for: {fullPath}");
            }
            else
            {
                return FormatOperationResult(false, "No suitable file manager found for this operating system");
            }
        }
        catch (Exception ex)
        {
            return FormatOperationResult(false, $"Error opening explorer: {ex.Message}");
        }
    }

    private static bool IsCommandAvailable(string command)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(processStartInfo);
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
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
@"Tool: open_explorer

Arguments:
- path: string (required, directory or file path to open in explorer)

Notes:
- Opens the file explorer for the specified path for the user.
- This is a tool the user may request to use to get easy manual access to a folder in the workspace

Failure points:
- Missing required argument: path
- Path does not exist
- File explorer application not available on system";
    }
}