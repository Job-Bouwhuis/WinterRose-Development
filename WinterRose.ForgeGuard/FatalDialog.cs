using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinterRose.ForgeGuardChecks
{
    internal static class FatalDialog
    {
        public static void Show(string extraInfo = "")
        {
            string mainMessage = "Fatal healthcheck failed to succeed. " +
                "The application can not gracefully handle it. " +
                "therefor the app will now close abruptly";

            if (OperatingSystem.IsWindows())
            {
                ShowWindowsDialog(mainMessage, extraInfo);
            }
            else if (OperatingSystem.IsLinux())
            {
                ShowLinuxDialog(mainMessage, extraInfo);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(mainMessage);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(extraInfo);
                Console.ResetColor();
            }
        }

        private static void ShowWindowsDialog(string mainMessage, string extraInfo)
        {
            if(!string.IsNullOrWhiteSpace(extraInfo))
                mainMessage += "\n\nDetails:\n" + extraInfo;
            MessageBox(IntPtr.Zero, mainMessage, "Fatal Error", 0);
        }

        private static void ShowLinuxDialog(string mainMessage, string extraInfo)
        {
            if (!string.IsNullOrWhiteSpace(extraInfo))
                mainMessage += "\n\nDetails:\n" + extraInfo;

            if (File.Exists("/usr/bin/zenity"))
            {
                RunProcess("zenity", $"--error --text=\"{Escape(mainMessage)}\"");
            }
            else if (File.Exists("/usr/bin/kdialog"))
            {
                RunProcess("kdialog", $"--error \"{Escape(mainMessage)}\"");
            }
            else if (File.Exists("/usr/bin/xmessage"))
            {
                RunProcess("xmessage", $"-center \"{Escape(mainMessage)}\"");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(mainMessage);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(extraInfo);
                Console.ResetColor();
            }
        }

        private static void RunProcess(string fileName, string args)
        {
            var p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
        }

        private static string Escape(string text)
        {
            return text.Replace("\"", "\\\"");
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hwnd, string text, string caption, uint type);
    }
}
