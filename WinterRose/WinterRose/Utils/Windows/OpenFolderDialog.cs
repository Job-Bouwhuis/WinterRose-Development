using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinterRose
{
    public static partial class Windows
    {
        public static class OSFolderBrowser
        {
            public static IFolderBrowser Open()
            {
                if (OperatingSystem.IsWindows()) return new FolderBrowserDialog();
                if (OperatingSystem.IsLinux()) return new LinuxFolderBrowser();
                throw new PlatformNotSupportedException();
            }

            public interface IFolderBrowser
            {
                string Title { get; set; }
                string InitialDirectory { get; set; }
                bool ShowNewFolderButton { get; set; }
                string SelectedPath { get; }
                bool Open();
            }
            private class LinuxFolderBrowser : IFolderBrowser
            {
                public string Title { get; set; } = "Select a folder...";
                public string InitialDirectory { get; set; } = null;
                public bool ShowNewFolderButton { get; set; } = true;
                public string SelectedPath { get; private set; }

                public bool Open()
                {
                    // Use kdialog if available
                    string initial = InitialDirectory ?? Environment.GetEnvironmentVariable("HOME");
                    string command = $"kdialog --getexistingdirectory \"{initial}\" --title \"{Title}\"";

                    try
                    {
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "/bin/bash",
                                Arguments = $"-c \"{command}\"",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };

                        process.Start();
                        SelectedPath = process.StandardOutput.ReadLine();
                        process.WaitForExit();

                        return !string.IsNullOrEmpty(SelectedPath);
                    }
                    catch
                    {
                        SelectedPath = null;
                        return false;
                    }
                }
            }
            private class FolderBrowserDialog : IFolderBrowser
            {
                [DllImport("shell32.dll", CharSet = CharSet.Auto)]
                private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

                [DllImport("shell32.dll", CharSet = CharSet.Auto)]
                private static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

                [DllImport("user32.dll", CharSet = CharSet.Auto)]
                private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

                private delegate IntPtr BrowseCallbackProc(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData);

                [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
                private struct BROWSEINFO
                {
                    public IntPtr hwndOwner;
                    public IntPtr pidlRoot;
                    public IntPtr pszDisplayName;
                    public string lpszTitle;
                    public int ulFlags;
                    public BrowseCallbackProc lpfn;
                    public IntPtr lParam;
                    public int iImage;
                }

                private const int BFFM_INITIALIZED = 1;
                private const int BFFM_SELCHANGED = 2;
                private const int WM_USER = 0x400;
                private const int BFFM_SETSELECTIONW = (WM_USER + 103);
                private const int MAX_PATH = 260;

                public string Title { get; set; } = "Select a folder...";
                public string InitialDirectory { get; set; } = null;
                public bool ShowNewFolderButton { get; set; } = true;
                public string SelectedPath { get; private set; }

                public static bool SelectFolder(out string folder, string title = "Select a folder", string initialDirectory = null, bool showNewFolderButton = true)
                {
                    FolderBrowserDialog dialog = new FolderBrowserDialog
                    {
                        Title = title,
                        InitialDirectory = initialDirectory,
                        ShowNewFolderButton = showNewFolderButton
                    };

                    dialog.OpenDialog();
                    if (!string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        folder = dialog.SelectedPath;
                        return true;
                    }

                    folder = null;
                    return false;
                }

                private void OpenDialog()
                {
                    // Running on STA thread to ensure the dialog is shown
                    var t = new Thread(ShowFolderBrowserDialog);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();
                }

                private void ShowFolderBrowserDialog()
                {
                    BROWSEINFO bi = new BROWSEINFO
                    {
                        hwndOwner = IntPtr.Zero,
                        pidlRoot = IntPtr.Zero,
                        pszDisplayName = Marshal.AllocHGlobal(MAX_PATH),
                        lpszTitle = Title,
                        ulFlags = ShowNewFolderButton ? 0 : 0x200,
                        lpfn = new BrowseCallbackProc(BrowseCallback)
                    };

                    try
                    {
                        IntPtr pidl = SHBrowseForFolder(ref bi);
                        if (pidl != IntPtr.Zero)
                        {
                            StringBuilder path = new StringBuilder(MAX_PATH);
                            if (SHGetPathFromIDList(pidl, path))
                            {
                                SelectedPath = path.ToString();
                            }
                            Marshal.FreeCoTaskMem(pidl);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(bi.pszDisplayName);
                    }
                }

                private IntPtr BrowseCallback(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData)
                {
                    if (msg == BFFM_INITIALIZED && !string.IsNullOrEmpty(InitialDirectory))
                    {
                        SendMessage(hwnd, BFFM_SETSELECTIONW, IntPtr.Zero, Marshal.StringToHGlobalUni(InitialDirectory));
                    }
                    return IntPtr.Zero;
                }

                public bool Open()
                {
                    OpenDialog();
                    return !string.IsNullOrEmpty(SelectedPath);
                }
            }
        }
    }
}
