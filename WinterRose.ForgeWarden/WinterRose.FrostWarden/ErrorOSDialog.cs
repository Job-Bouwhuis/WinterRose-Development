using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinterRose.ForgeWarden;

public static class ErrorOSDialog
{
    public static void Show(string message, string title = "Error")
    {
        ConsoleColor backColor = Console.BackgroundColor;
        ConsoleColor consoleColor = Console.ForegroundColor;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"ERROR: [{title}] {message}");
        Console.ForegroundColor = consoleColor;
        Console.BackgroundColor = backColor;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Windows.MessageBox(message, title,
                Windows.MessageBoxButtons.OK,
                Windows.MessageBoxIcon.Error);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            RunProcess("osascript", $"-e 'display dialog \"{Escape(message)}\" with title \"{Escape(title)}\" buttons {{\"OK\"}} with icon stop'");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (CommandExists("kdialog"))
            {
                RunProcess("kdialog", $"--error \"{Escape(message)}\" --title \"{Escape(title)}\"");
            }
            else if (CommandExists("zenity"))
            {
                RunProcess("zenity", $"--error --text=\"{Escape(message)}\" --title=\"{Escape(title)}\"");
            }
            else
            {
                Console.Error.WriteLine($"[{title}] {message}");
            }
        }
        else
        {
            Console.Error.WriteLine($"[{title}] {message}");
        }
    }

    private static bool CommandExists(string name)
    {
        try
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "which",
                Arguments = name,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void RunProcess(string file, string args)
    {
        try
        {
            Process.Start(file, args);
        }
        catch
        {
            Console.Error.WriteLine($"Failed to run {file}: {args}");
        }
    }

    private static string Escape(string input) =>
        input.Replace("\"", "\\\"");
}
