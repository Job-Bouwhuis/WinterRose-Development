using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    public static partial class Windows
    {
        public static class Clipboard
        {
            private const uint CF_HDROP = 15; // Predefined clipboard format for file drop
            private const uint GHND = 0x42; // Allocates fixed or movable memory
            private const uint GMEM_DDESHARE = 0x2000; // Memory will be shared

            public static string WriteString(string text, bool globalClipboard = true)
            {
                try
                {
                    if (OpenClipboard(globalClipboard ? IntPtr.Zero : MyHandle.Handle))
                    {
                        EmptyClipboard();
                        IntPtr intPtr = StringToIntPtrUni(text);
                        if (intPtr != IntPtr.Zero)
                        {
                            SetClipboardData(CF_UNICODETEXT, intPtr);
                            CloseClipboard();
                            return string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    CloseClipboard();
                    return $"ERROR: {ex.GetType().Name} - {ex.Message}";
                }
                return "Failed to copy to clipboard.";
            }
            public static string WriteFile(string filePath, bool globalClipboard = true)
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        return "ERROR: File not found.";
                    }

                    if (OpenClipboard(globalClipboard ? IntPtr.Zero : MyHandle.Handle))
                    {
                        EmptyClipboard();

                        // Allocate memory to store the file path
                        IntPtr hDrop = CreateHDrop(filePath);
                        if (hDrop != IntPtr.Zero)
                        {
                            // Set the data in the clipboard
                            SetClipboardData(CF_HDROP, hDrop);
                            CloseClipboard();
                            return string.Empty;
                        }
                        CloseClipboard();
                    }
                }
                catch (Exception ex)
                {
                    CloseClipboard();
                    return $"ERROR: {ex.GetType().Name} - {ex.Message}";
                }
                return "Failed to copy file to clipboard.";
            }

            public static string ReadString(bool globalClipboard = true)
            {
                try
                {
                    if (OpenClipboard(globalClipboard ? IntPtr.Zero : MyHandle.Handle))
                    {
                        IntPtr hData = GetClipboardData(CF_UNICODETEXT);
                        if (hData != IntPtr.Zero)
                        {
                            IntPtr pText = GlobalLock(hData);
                            string text = Marshal.PtrToStringUni(pText);
                            GlobalUnlock(hData);
                            CloseClipboard();
                            return text;
                        }
                    }
                }
                catch (Exception ex)
                {
                    CloseClipboard();
                    return $"ERROR: {ex.GetType().Name} - {ex.Message}";
                }
                return null;
            }
            public static string[] ReadFiles(bool globalClipboard = true)
            {
                try
                {
                    if (OpenClipboard(globalClipboard ? IntPtr.Zero : MyHandle.Handle))
                    {
                        if (IsClipboardFormatAvailable(CF_HDROP))
                        {
                            IntPtr hDrop = GetClipboardData(CF_HDROP);
                            if (hDrop != IntPtr.Zero)
                            {
                                uint fileCount = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
                                string[] filePaths = new string[fileCount];

                                for (uint i = 0; i < fileCount; i++)
                                {
                                    StringBuilder filePath = new StringBuilder(260); // MAX_PATH length
                                    DragQueryFile(hDrop, i, filePath, filePath.Capacity);
                                    filePaths[i] = filePath.ToString();
                                }

                                CloseClipboard();
                                return filePaths;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CloseClipboard();
                    return new[] { $"ERROR: {ex.GetType().Name} - {ex.Message}" };
                }
                return null;
            }

            public static void Clear()
            {
                OpenClipboard(IntPtr.Zero);
                EmptyClipboard();
                CloseClipboard();
            }
            private static IntPtr CreateHDrop(string filePath)
            {
                // Get the size of the buffer needed
                int filePathSize = Encoding.Unicode.GetByteCount(filePath) + 2; // Add space for null terminator
                IntPtr hGlobal = GlobalAlloc(GHND | GMEM_DDESHARE, (ulong)(Marshal.SizeOf(typeof(DROPFILES)) + filePathSize));

                if (hGlobal == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                // Lock the memory and create the DROPFILES structure
                IntPtr pDropFiles = GlobalLock(hGlobal);
                DROPFILES dropFiles = new DROPFILES
                {
                    pFiles = Marshal.SizeOf(typeof(DROPFILES)),
                    pt = new POINT { x = 0, y = 0 },
                    fNC = false,
                    fWide = true // Indicates Unicode text
                };

                // Copy the DROPFILES structure to the allocated memory
                Marshal.StructureToPtr(dropFiles, pDropFiles, false);

                // Copy the file path right after the DROPFILES structure
                IntPtr pFilePath = (IntPtr)((long)pDropFiles + dropFiles.pFiles);
                byte[] filePathBytes = Encoding.Unicode.GetBytes(filePath + "\0");
                Marshal.Copy(filePathBytes, 0, pFilePath, filePathBytes.Length);

                // Unlock the memory
                GlobalUnlock(hGlobal);

                return hGlobal;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private struct DROPFILES
            {
                public int pFiles;
                public POINT pt;
                public bool fNC;
                public bool fWide;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int x;
                public int y;
            }

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool OpenClipboard(IntPtr hWndNewOwner);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool CloseClipboard();

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool EmptyClipboard();

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr GetClipboardData(uint uFormat);

            [DllImport("user32.dll")]
            private static extern bool IsClipboardFormatAvailable(uint format);

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            private static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder lpszFile, int cch);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GlobalAlloc(uint uFlags, ulong dwBytes);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GlobalLock(IntPtr hMem);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool GlobalUnlock(IntPtr hMem);
        }
    }
    
}
