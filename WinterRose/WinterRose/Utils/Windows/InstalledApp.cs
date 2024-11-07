using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    public static partial class Windows
    {
        /// <summary>
        /// A class that represents an application that is installed on the system.
        /// </summary>
        public class InstalledApp
        {
            public string? Name { get; private set; }
            public string? Publisher { get; private set; }
            public string? Version { get; private set; }
            public string? InstallDate { get; private set; }
            public string? InstallLocation { get; private set; }
            public string? UninstallString { get; private set; }
            public ShellCommand? QuietUninstallString { get; private set; }
            public string? ModifyPath { get; private set; }
            public string? DisplayIcon { get; private set; }
            public string? DisplayVersion { get; private set; }
            public string? EstimatedSize { get; private set; }
            public URL? HelpLink { get; private set; }
            public string? HelpTelephone { get; private set; }
            public URL? URLInfoAbout { get; private set; }
            public URL? URLUpdateInfo { get; private set; }
            public string? Comments { get; private set; }
            public string? Contact { get; private set; }
            public string? Readme { get; private set; }
            public string? ParentDisplayName { get; private set; }
            public string? ParentDisplayVersion { private get; set; }
            public string? ParentPublisher { get; private set; }
            public string? ParentURLInfoAbout { get; private set; }
            public string? ParentURLUpdateInfo { get; private set; }
            public ShellCommand? UninstallCommand => UninstallString;
            public string? ExePath { get; private set; }
            public string? ExeName { get; private set; }


            public static List<InstalledApp> installedApps;
            static InstalledApp()
            {
                installedApps = GetInstalledApps();
            }

            private static List<InstalledApp> GetInstalledApps()
            {
                List<InstalledApp> installedApps = new List<InstalledApp>();

                //using RegKey key = RegKey.LocalMachine()["SOFTWARE"]["Microsoft"]["Windows"]["CurrentVersion"]["Uninstall"];
                using RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                try
                {
                    if (key != null)
                    {
                        var subkeys = key.GetSubKeyNames();
                        foreach (string subKeyName in subkeys)
                        {
                            using RegistryKey subKey = key.OpenSubKey(subKeyName);
                            object displayName = subKey.GetValue("DisplayName");

                            var keyFields = subKey.GetValueNames();

                            if (displayName != null)
                            {
                                InstalledApp app = new InstalledApp
                                {
                                    Name = displayName.ToString(),
                                    Publisher = subKey.GetValue("Publisher")?.ToString(),
                                    Version = subKey.GetValue("DisplayVersion")?.ToString(),
                                    InstallDate = subKey.GetValue("InstallDate")?.ToString(),
                                    InstallLocation = subKey.GetValue("InstallLocation")?.ToString(),
                                    UninstallString = subKey.GetValue("UninstallString")?.ToString(),
                                    QuietUninstallString = subKey.GetValue("QuietUninstallString")?.ToString(),
                                    ModifyPath = subKey.GetValue("ModifyPath")?.ToString(),
                                    DisplayIcon = subKey.GetValue("DisplayIcon")?.ToString(),
                                    DisplayVersion = subKey.GetValue("DisplayVersion")?.ToString(),
                                    EstimatedSize = subKey.GetValue("EstimatedSize")?.ToString(),
                                    HelpLink = subKey.GetValue("HelpLink")?.ToString(),
                                    HelpTelephone = subKey.GetValue("HelpTelephone")?.ToString(),
                                    URLInfoAbout = subKey.GetValue("URLInfoAbout")?.ToString(),
                                    URLUpdateInfo = subKey.GetValue("URLUpdateInfo")?.ToString(),
                                    Comments = subKey.GetValue("Comments")?.ToString(),
                                    Readme = subKey.GetValue("Readme")?.ToString(),
                                    ParentDisplayName = subKey.GetValue("ParentDisplayName")?.ToString(),
                                    ParentDisplayVersion = subKey.GetValue("ParentDisplayVersion")?.ToString(),
                                    ParentPublisher = subKey.GetValue("ParentPublisher")?.ToString(),
                                    ParentURLInfoAbout = subKey.GetValue("ParentURLInfoAbout")?.ToString(),
                                    ParentURLUpdateInfo = subKey.GetValue("ParentURLUpdateInfo")?.ToString(),
                                };

                                app.InstallLocation ??= subKey.GetValue("InstallPath")?.ToString();
                                app.InstallLocation ??= subKey.GetValue("InstallSource")?.ToString();

                                if (app.DisplayIcon is not null && app.DisplayIcon.EndsWith(".exe,0"))
                                {
                                    string exePath = app.DisplayIcon.Split(',')[0];
                                    app.ExeName = Path.GetFileName(exePath);
                                    app.ExePath = exePath;
                                    app.InstallLocation = Path.GetDirectoryName(exePath);
                                    installedApps.Add(app);
                                    continue;
                                }

                                app.ExeName = subKey.GetValue("ExeName")?.ToString();
                                if (app.InstallLocation != null && app.ExeName is not null)
                                    app.ExePath = Path.Combine(app.InstallLocation, app.ExeName);

                                installedApps.Add(app);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Type typeofE = e.GetType();
                }

                return installedApps;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
