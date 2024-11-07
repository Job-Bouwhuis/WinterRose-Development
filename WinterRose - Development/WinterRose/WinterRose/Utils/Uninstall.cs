using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;

namespace WinterRose
{
    /// <summary>
    /// Provides methods to uninstall applications.
    /// </summary>
    public class Uninstall
    {
        /// <summary>
        /// Uninstalls the current application by deleting the folder that contains the executable, and all the files and subfolders within it.<br></br>
        /// The application will close immediately after calling this method.
        /// <br></br><br></br>
        /// <b>WARNING:</b> A call to this method is irreversible, there is no way to recover the files once they are deleted, so use with caution.
        /// </summary>
        /// <param name="delay">The amount of delay before deleting this application. Must be at least 0.1</param>
        public static void UninstallThisApp(int delay)
        {
            // Validate the delay
            if (delay < 1)
                throw new ArgumentException("The delay must be at least 1 second.", nameof(delay));

            string batchFilePath = Path.Combine(Path.GetTempPath(), "delete_self.bat");
            string exeFilePath = FileManager.PathOneUp(Assembly.GetExecutingAssembly().Location);
            string batchFileContent = $"""
                                        @echo off
                                        echo "Deleting application in {delay} seconds..."
                                        timeout /t {delay} /nobreak > NUL
                                        rmdir /s /q {exeFilePath}
                                        del "%~f0"
                                        """;

            // Write the batch file
            File.WriteAllText(batchFilePath, batchFileContent);

            // Create a new process to run the batch file
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = batchFilePath,
                WindowStyle = ProcessWindowStyle.Maximized,
                UseShellExecute = true,
                CreateNoWindow = true
            };

            try
            {
                // clear the RegPrefs tied to this application
                RegPrefs.Flush();
            }
            catch (InvalidOperationException)
            {
                // entry assembly is not defined, so we can't clear the RegPrefs
            }


            // Start the batch file
            Process.Start(psi);

            Environment.Exit(0);
        }
    }
}
