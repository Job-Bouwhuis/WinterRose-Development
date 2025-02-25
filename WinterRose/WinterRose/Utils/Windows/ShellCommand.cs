using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// Represents a shell command that can be run in the command prompt.
    /// </summary>
    public class ShellCommand
    {
        private string? command;

        public void Execute()
        {
            if (command is null)
                throw new InvalidOperationException("The command is not set.");
            
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
        }

        public override string ToString()
        {
            return command;
        }

        public static implicit operator string(ShellCommand shellCommand)
        {
            return shellCommand.ToString();
        }

        public static implicit operator ShellCommand(string command)
        {
            ShellCommand shellCommand = new ShellCommand();
            shellCommand.command = command;
            return shellCommand;
        }
    }
}
