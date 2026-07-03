using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WinterRose.Shortcuts;

public static class AppIconExtractor
{
    public static string GetIconPath(string executablePath, string? fallbackIconPath = null)
    {
        if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
            return fallbackIconPath;

        string? iconPath = null;

        if (OperatingSystem.IsWindows())
            iconPath = ExtractWindowsIcon(executablePath);

        if (OperatingSystem.IsLinux() || iconPath is null)
            iconPath = ExtractLinuxIcon(executablePath);

        if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
            return iconPath;

        return fallbackIconPath;
    }

    // ---------------- WINDOWS ----------------

    public static string? ExtractWindowsIcon(string exePath, string? fallbackIconPath = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
                return fallbackIconPath;
 
            string outputPath = Path.Combine(
                Path.GetTempPath(),
                $"{Path.GetFileNameWithoutExtension(exePath)}_icon.ico"
            );
 
            if (TryExtractWithExtractIconEx(exePath, outputPath))
                return outputPath;
 
            return fallbackIconPath;
        }
        catch
        {
            return fallbackIconPath;
        }
    }
 
    private static bool TryExtractWithExtractIconEx(string exePath, string outputPath)
    {
        IntPtr[] largeIcons = new IntPtr[1];
        IntPtr[] smallIcons = new IntPtr[1];
 
        try
        {
            uint extracted = ExtractIconEx(exePath, 0, largeIcons, smallIcons, 1);
 
            if (extracted == 0 || largeIcons[0] == IntPtr.Zero)
                return false;
 
            return SaveHIconAsIco(largeIcons[0], outputPath);
        }
        catch
        {
            return false;
        }
        finally
        {
            if (largeIcons[0] != IntPtr.Zero) DestroyIcon(largeIcons[0]);
            if (smallIcons[0] != IntPtr.Zero) DestroyIcon(smallIcons[0]);
        }
    }
 
    // ---- ICO writer: raw 32bpp BMP-format frames (no PNG, no System.Drawing) ----
 
    private static bool SaveHIconAsIco(IntPtr hIcon, string outputPath)
    {
        int[] SIZES = { 16, 32, 48, 256 };
 
        try
        {
            // Each entry: (size, colorPixelsBGRA (top-down as captured), andMask)
            var frames = new List<(int Size, byte[] Bgra)>();
 
            foreach (int size in SIZES)
            {
                if (!TryRenderIconToBgra(hIcon, size, out byte[] bgra))
                    return false;
 
                frames.Add((size, bgra));
            }
 
            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);
 
            // ICONDIR
            bw.Write((ushort)0);          // reserved
            bw.Write((ushort)1);          // type = icon
            bw.Write((ushort)frames.Count);
 
            // Precompute per-frame image data (BITMAPINFOHEADER + XOR + AND mask)
            var imageBlobs = new List<byte[]>();
            foreach (var frame in frames)
                imageBlobs.Add(BuildBmpIconImage(frame.Size, frame.Bgra));
 
            int dirEntrySize = 16;
            int offset = 6 + dirEntrySize * frames.Count;
 
            for (int i = 0; i < frames.Count; i++)
            {
                int size = frames[i].Size;
                byte wh = (byte)(size >= 256 ? 0 : size); // 0 means 256
 
                bw.Write(wh);              // bWidth
                bw.Write(wh);              // bHeight
                bw.Write((byte)0);         // color count (0 = >=256 colors / not palette)
                bw.Write((byte)0);         // reserved
                bw.Write((ushort)1);       // planes
                bw.Write((ushort)32);      // bit count
                bw.Write((uint)imageBlobs[i].Length); // bytes in image data
                bw.Write((uint)offset);    // offset to image data
 
                offset += imageBlobs[i].Length;
            }
 
            foreach (var blob in imageBlobs)
                bw.Write(blob);
 
            return true;
        }
        catch
        {
            return false;
        }
    }
 
    // Builds BITMAPINFOHEADER + 32bpp BGRA XOR mask (top-down source, written bottom-up
    // as ICO/BMP requires) + 1bpp AND mask (all zero since alpha already encodes transparency).
    private static byte[] BuildBmpIconImage(int size, byte[] topDownBgra)
    {
        int headerSize = 40;
        int xorRowBytes = size * 4; // 32bpp, always 4-byte aligned already
        int xorSize = xorRowBytes * size;
 
        // AND mask: 1 bpp, rows padded to 4-byte boundary
        int andRowBytes = ((size + 31) / 32) * 4;
        int andSize = andRowBytes * size;
 
        using var ms = new MemoryStream(headerSize + xorSize + andSize);
        using var bw = new BinaryWriter(ms);
 
        // BITMAPINFOHEADER: height = 2x actual (XOR + AND), per ICO spec
        bw.Write(headerSize);          // biSize
        bw.Write(size);                // biWidth
        bw.Write(size * 2);            // biHeight (XOR+AND combined)
        bw.Write((ushort)1);           // biPlanes
        bw.Write((ushort)32);          // biBitCount
        bw.Write(0);                   // biCompression = BI_RGB
        bw.Write(xorSize);             // biSizeImage (XOR only is conventional)
        bw.Write(0);                   // biXPelsPerMeter
        bw.Write(0);                   // biYPelsPerMeter
        bw.Write(0);                   // biClrUsed
        bw.Write(0);                   // biClrImportant
 
        // XOR mask: must be written BOTTOM-UP.
        // topDownBgra is row 0 = top row. Write rows in reverse order.
        for (int y = size - 1; y >= 0; y--)
        {
            int rowStart = y * xorRowBytes;
            bw.Write(topDownBgra, rowStart, xorRowBytes);
        }
 
        // AND mask: all zero bits = fully opaque everywhere (alpha channel handles real transparency).
        // Zero-filled MemoryStream default is fine; just advance the writer explicitly.
        byte[] zeroRow = new byte[andRowBytes];
        for (int y = 0; y < size; y++)
            bw.Write(zeroRow);
 
        bw.Flush();
        return ms.ToArray();
    }
 
    // ---- Rendering: DrawIconEx into a CreateDIBSection-backed 32bpp top-down bitmap ----
    // Using CreateDIBSection avoids GetDIBits stride/format ambiguity entirely: we get
    // a pointer straight to a buffer we fully control, with guaranteed 32bpp top-down layout.
 
    private static bool TryRenderIconToBgra(IntPtr hIcon, int size, out byte[] bgra)
    {
        bgra = Array.Empty<byte>();
 
        IntPtr hdcScreen = IntPtr.Zero;
        IntPtr hdcMem = IntPtr.Zero;
        IntPtr hBitmap = IntPtr.Zero;
        IntPtr oldObject = IntPtr.Zero;
        IntPtr pBits = IntPtr.Zero;
 
        try
        {
            hdcScreen = GetDC(IntPtr.Zero);
            hdcMem = CreateCompatibleDC(hdcScreen);
 
            var bmi = new BITMAPINFO();
            bmi.bmiHeader.biSize = Marshal.SizeOf<BITMAPINFOHEADER>();
            bmi.bmiHeader.biWidth = size;
            bmi.bmiHeader.biHeight = -size; // negative = top-down
            bmi.bmiHeader.biPlanes = 1;
            bmi.bmiHeader.biBitCount = 32;
            bmi.bmiHeader.biCompression = 0; // BI_RGB
 
            hBitmap = CreateDIBSection(hdcScreen, ref bmi, 0 /* DIB_RGB_COLORS */, out pBits, IntPtr.Zero, 0);
            if (hBitmap == IntPtr.Zero || pBits == IntPtr.Zero)
                return false;
 
            oldObject = SelectObject(hdcMem, hBitmap);
 
            // Zero the buffer first so untouched pixels are fully transparent, not garbage.
            int stride = size * 4;
            int total = stride * size;
            byte[] zero = new byte[total];
            Marshal.Copy(zero, 0, pBits, total);
 
            bool drawn = DrawIconEx(hdcMem, 0, 0, hIcon, size, size, 0, IntPtr.Zero, 0x0003 /* DI_NORMAL */);
            if (!drawn)
                return false;
 
            // GDI has now written premultiplied-or-not BGRA into pBits, top-down, 4-byte-aligned rows.
            byte[] buffer = new byte[total];
            Marshal.Copy(pBits, buffer, 0, total);
 
            bgra = buffer;
            return true;
        }
        finally
        {
            if (oldObject != IntPtr.Zero) SelectObject(hdcMem, oldObject);
            if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
            if (hdcMem != IntPtr.Zero) DeleteDC(hdcMem);
            if (hdcScreen != IntPtr.Zero) ReleaseDC(IntPtr.Zero, hdcScreen);
        }
    }
 
    // ---- Win32 interop ----
 
    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }
 
    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        // bmiColors omitted: unused for BI_RGB 32bpp
    }
 
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateDIBSection(
        IntPtr hdc,
        ref BITMAPINFO pbmi,
        uint usage,
        out IntPtr ppvBits,
        IntPtr hSection,
        uint offset);
 
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);
 
    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
 
    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);
 
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern uint ExtractIconEx(
        string lpszFile,
        int nIconIndex,
        IntPtr[] phiconLarge,
        IntPtr[] phiconSmall,
        uint nIcons);
 
    [DllImport("user32.dll")]
    private static extern bool DrawIconEx(
        IntPtr hdc,
        int xLeft,
        int yTop,
        IntPtr hIcon,
        int cxWidth,
        int cyWidth,
        uint istepIfAniCur,
        IntPtr hbrFlickerFreeDraw,
        uint diFlags);
 
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
 
    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
 
    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);
 
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);


    // ---------------- LINUX ----------------

    private static string? ExtractLinuxIcon(string exePath)
    {
        try
        {
            string? desktopFilePath = FindDesktopFile(exePath);

            if (string.IsNullOrEmpty(desktopFilePath) || !File.Exists(desktopFilePath))
                return null;

            string? iconValue = TryReadDesktopIconValue(desktopFilePath);

            if (string.IsNullOrEmpty(iconValue))
                return null;

            return ResolveXdgIcon(iconValue, desktopFilePath);
        }
        catch
        {
            return null;
        }
    }

    private static string? FindDesktopFile(string exePath)
    {
        string exeName = Path.GetFileNameWithoutExtension(exePath);

        string? dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME")
                           ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                               ".local/share");

        string dataDirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS")
                          ?? "/usr/local/share:/usr/share";

        List<string> searchPaths = [];

        searchPaths.Add(Path.Combine(dataHome, "applications", $"{exeName}.desktop"));

        foreach (string dir in dataDirs.Split(':'))
            searchPaths.Add(Path.Combine(dir, "applications", $"{exeName}.desktop"));

        foreach (string path in searchPaths)
            if (File.Exists(path))
                return path;

        return null;
    }

    private static string? TryReadDesktopIconValue(string desktopFilePath)
    {
        foreach (string line in File.ReadLines(desktopFilePath))
        {
            if (!line.StartsWith("Icon=", StringComparison.OrdinalIgnoreCase))
                continue;

            return line.Substring("Icon=".Length).Trim();
        }

        return null;
    }

    private static string? ResolveXdgIcon(string iconValue, string desktopFilePath)
    {
        // 1. Absolute path
        if (Path.IsPathFullyQualified(iconValue) && File.Exists(iconValue))
            return iconValue;

        // 2. Relative path (relative to .desktop file)
        if (iconValue.StartsWith("./") || iconValue.StartsWith("../") || iconValue.Contains("/"))
        {
            string baseDir = Path.GetDirectoryName(desktopFilePath) ?? "";
            string combined = Path.GetFullPath(Path.Combine(baseDir, iconValue));

            if (File.Exists(combined))
                return combined;

            return null;
        }

        // 3. Icon theme lookup (XDG spec style)
        return FindIconInThemes(iconValue);
    }

    private static string? FindIconInThemes(string iconName)
    {
        string[] baseDirs =
        {
            Environment.GetEnvironmentVariable("XDG_DATA_HOME")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share"),

            "/usr/local/share",
            "/usr/share"
        };

        string[] themes =
        {
            "hicolor",
            "Adwaita",
            "Papirus",
            "gnome"
        };

        string[] sizes =
        {
            "256x256",
            "128x128",
            "64x64",
            "48x48",
            "32x32",
            "scalable"
        };

        string[] extensions =
        {
            ".png",
            ".svg",
            ".xpm"
        };

        foreach (string baseDir in baseDirs)
        {
            foreach (string theme in themes)
            {
                foreach (string size in sizes)
                {
                    foreach (string ext in extensions)
                    {
                        string path = Path.Combine(
                            baseDir,
                            "icons",
                            theme,
                            size,
                            "apps",
                            iconName + ext
                        );

                        if (File.Exists(path))
                            return path;
                    }
                }
            }

            // fallback pixmaps
            foreach (string ext in extensions)
            {
                string pixmapPath = Path.Combine(baseDir, "pixmaps", iconName + ext);

                if (File.Exists(pixmapPath))
                    return pixmapPath;
            }
        }

        return null;
    }
}