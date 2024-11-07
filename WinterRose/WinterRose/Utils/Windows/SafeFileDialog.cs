using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinterRose;

public static partial class Windows
{
    public class SaveFileDialog
    {
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetSaveFileName([In, Out] SaveFileName ofn);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class SaveFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public string filter;
            public string customFilter;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public IntPtr file;
            public int maxFile = 0;
            public string fileTitle;
            public int maxFileTitle = 0;
            public string initialDir;
            public string title;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public string defExt;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public string templateName;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        private enum SaveFileNameFlags
        {
            OFN_HIDEREADONLY = 0x4,
            OFN_OVERWRITEPROMPT = 0x2,
            OFN_FORCESHOWHIDDEN = 0x10000000,
            OFN_FILEMUSTEXIST = 0x1000,
            OFN_PATHMUSTEXIST = 0x800
        }

        public string Title { get; set; } = "Save a file...";
        public string InitialDirectory { get; set; } = null;
        public string Filter { get; set; } = "All files(*.*)\0\0";
        public string DefaultExtension { get; set; } = "txt";
        public bool ShowHidden { get; set; } = false;
        public bool Success { get; private set; }
        public string File { get; private set; }

        public static bool SaveFile(out string file, string title = "Safe your file", string filter = null, string initialDirectory = "C:\\", string defaultExtension = null, bool showHidden = false)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = title;
            dialog.InitialDirectory = initialDirectory;
            dialog.Filter = filter;
            dialog.DefaultExtension = defaultExtension;
            dialog.ShowHidden = showHidden;

            dialog.OpenDialog();
            if (dialog.Success)
            {
                file = dialog.File;
                return true;
            }

            file = null;
            return false;
        }

        private void OpenDialog()
        {
            Thread thread = new Thread(ShowSaveFileDialog);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void ShowSaveFileDialog()
        {
            const int MAX_FILE_LENGTH = 2048;

            Success = false;
            File = null;

            SaveFileName ofn = new SaveFileName();

            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.filter = Filter?.Replace("|", "\0") + "\0";
            ofn.fileTitle = new string(new char[MAX_FILE_LENGTH]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = InitialDirectory;
            ofn.title = Title;
            ofn.defExt = DefaultExtension;
            ofn.flags = (int)SaveFileNameFlags.OFN_HIDEREADONLY | (int)SaveFileNameFlags.OFN_OVERWRITEPROMPT | (int)SaveFileNameFlags.OFN_PATHMUSTEXIST;

            // Create buffer for file name
            ofn.file = Marshal.AllocHGlobal(MAX_FILE_LENGTH * Marshal.SystemDefaultCharSize);
            ofn.maxFile = MAX_FILE_LENGTH;

            // Initialize buffer with NULL bytes
            for (int i = 0; i < MAX_FILE_LENGTH * Marshal.SystemDefaultCharSize; i++)
            {
                Marshal.WriteByte(ofn.file, i, 0);
            }

            if (ShowHidden)
            {
                ofn.flags |= (int)SaveFileNameFlags.OFN_FORCESHOWHIDDEN;
            }

            Success = GetSaveFileName(ofn);

            if (Success)
            {
                File = Marshal.PtrToStringAuto(ofn.file);
            }

            Marshal.FreeHGlobal(ofn.file);
        }
    }
}
