using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{

    public readonly record struct CommandResult(int ExitCode, string Output, string Error)
    {
        public bool Success => ExitCode == 0;
    }

    /// <summary>
    /// Represents a shell command that can be run in the command prompt.
    /// </summary>
    public class ShellCommand
    {
        private readonly string command;

        public ShellCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentException("Command cannot be null or empty", nameof(command));

            this.command = command;
        }

        public static CommandResult Run(string command) => new ShellCommand(command).Execute();

        public CommandResult Execute()
        {
            using var process = new Process();
            process.StartInfo.FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/bash";
            process.StartInfo.Arguments = OperatingSystem.IsWindows() ? $"/c {command}" : $"-c \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return new CommandResult(process.ExitCode, output.Trim(), error.Trim());
        }

        public override string ToString() => command;

        public static implicit operator string(ShellCommand cmd) => cmd.ToString();
        public static implicit operator ShellCommand(string cmd) => new ShellCommand(cmd);
    }
}
